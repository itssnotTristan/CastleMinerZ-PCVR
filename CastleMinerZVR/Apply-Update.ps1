param([Parameter(Mandatory=$true)][string]$Request,[switch]$StageOnly,[switch]$ValidateOnly,[switch]$NoLaunch)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$config=Get-Content -LiteralPath $Request -Raw | ConvertFrom-Json
$runtimePath=[IO.Path]::GetFullPath([string]$config.runtime).TrimEnd('\','/')
if($runtimePath -ne [IO.Path]::GetFullPath($PSScriptRoot).TrimEnd('\','/')){throw 'Update destination must be this game directory.'}
$requestDirectory=[IO.Path]::GetDirectoryName([IO.Path]::GetFullPath($Request))
$archivePath=[IO.Path]::GetFullPath([string]$config.archive)
if($archivePath -ne [IO.Path]::Combine($requestDirectory,'package.zip')){throw 'Invalid staged archive location.'}
$hash=[string]$config.sha256
$hashStream=[IO.File]::OpenRead($archivePath);$algorithm=[Security.Cryptography.SHA256]::Create()
try{$actualHash=[BitConverter]::ToString($algorithm.ComputeHash($hashStream)).Replace('-','').ToLowerInvariant()}finally{$algorithm.Dispose();$hashStream.Dispose()}
if($hash -notmatch '^[0-9a-f]{64}$' -or $actualHash -ne $hash){throw 'Update checksum mismatch.'}
$stagePath=[IO.Path]::Combine($requestDirectory,'stage-'+[Guid]::NewGuid().ToString('N'))
function Assert-PlainDirectory([string]$path){
    $parent=[IO.DirectoryInfo]::new([IO.Path]::GetFullPath($path))
    while($null -ne $parent){
        if($parent.Exists -and ($parent.Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Update paths must not contain directory links.'}
        $parent=$parent.Parent
    }
}
Assert-PlainDirectory $runtimePath
Assert-PlainDirectory $requestDirectory
$allowed=@('CastleMinerZVR.exe','openxr_loader.dll','Apply-Update.ps1','build.json','vcruntime140.dll','vcruntime140_1.dll','msvcp140.dll','concrt140.dll','README.md','THIRD-PARTY-LICENSES.txt')
$zip=[IO.Compression.ZipFile]::OpenRead($archivePath)
try{
    if($zip.Entries.Count -gt 16){throw 'Too many update files.'}
    $names=@{};$expandedSize=0L
    foreach($entry in $zip.Entries){
        $name=$entry.FullName
        if($allowed -notcontains $name -or $names.ContainsKey($name.ToLowerInvariant()) -or $entry.Length -gt 67108864){throw 'Unexpected update file.'}
        if((($entry.ExternalAttributes -shr 16) -band 0xF000) -eq 0xA000){throw 'Symbolic links are not allowed.'}
        $names[$name.ToLowerInvariant()]=$true;$expandedSize+=$entry.Length
    }
    if(-not $names.ContainsKey('castleminerzvr.exe') -or -not $names.ContainsKey('openxr_loader.dll') -or $expandedSize -gt 134217728){throw 'Incomplete or oversized update.'}
    if($ValidateOnly){Write-Output 'PCVR_UPDATE_ZIP_VALID';exit 0}
    [IO.Directory]::CreateDirectory($stagePath) | Out-Null
    foreach($entry in $zip.Entries){
        $target=[IO.Path]::Combine($stagePath,$entry.FullName)
        if(Test-Path -LiteralPath $target){
            if((Get-Item -LiteralPath $target).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Unsafe stage path.'}
        }
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry,$target,$true)
    }
}finally{$zip.Dispose()}
$exePath=[IO.Path]::Combine($stagePath,'CastleMinerZVR.exe')
$header=[IO.File]::ReadAllBytes($exePath)
if($header.Length -lt 128 -or $header[0] -ne 77 -or $header[1] -ne 90){throw 'Invalid Windows executable.'}
if($StageOnly){Write-Output 'PCVR_UPDATE_STAGED';exit 0}
$parentProcess=Get-Process -Id ([int]$config.parentPid) -ErrorAction SilentlyContinue
if($parentProcess){
    if([IO.Path]::GetFullPath($parentProcess.Path) -ne [IO.Path]::Combine($runtimePath,'CastleMinerZVR.exe')){throw 'Unexpected parent game process.'}
    if(-not $parentProcess.WaitForExit(120000)){throw 'Game did not finish saving and close.'}
}
$backupPath=[IO.Path]::Combine($runtimePath,'rollback-'+[Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($backupPath) | Out-Null
$replaced=@()
try{
    foreach($file in Get-ChildItem -LiteralPath $stagePath -File){
        if($allowed -notcontains $file.Name){throw 'Unexpected staged file.'}
        $target=[IO.Path]::Combine($runtimePath,$file.Name)
        if((Test-Path -LiteralPath $target) -and ((Get-Item -LiteralPath $target).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Update target must not be a file link.'}
        if(Test-Path -LiteralPath $target){Copy-Item -LiteralPath $target -Destination ([IO.Path]::Combine($backupPath,$file.Name))}
        $replaced+=$file.Name
        Copy-Item -LiteralPath $file.FullName -Destination $target -Force
    }
    if(-not $NoLaunch){Start-Process -FilePath ([IO.Path]::Combine($runtimePath,'CastleMinerZVR.exe')) -WorkingDirectory $runtimePath -WindowStyle Hidden}
}catch{
    foreach($name in $replaced){$previous=[IO.Path]::Combine($backupPath,$name);$target=[IO.Path]::Combine($runtimePath,$name);if(Test-Path -LiteralPath $previous){Copy-Item -LiteralPath $previous -Destination $target -Force}elseif(Test-Path -LiteralPath $target){Remove-Item -LiteralPath $target -Force}}
    throw
}
