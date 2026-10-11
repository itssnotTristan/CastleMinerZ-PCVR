param([string]$RuntimeDirectory=$PSScriptRoot)
$ErrorActionPreference='Stop'
$RuntimeDirectory=(Resolve-Path -LiteralPath $RuntimeDirectory).Path
$source=Join-Path $RuntimeDirectory 'game'
$destination=Join-Path $RuntimeDirectory 'graphics-hd'
if(Test-Path -LiteralPath $destination){Write-Host 'Keeping the existing HD texture pack.';exit 0}
$tool=Join-Path $RuntimeDirectory 'tools/TexturePackTool.exe'
if(-not (Test-Path -LiteralPath $tool -PathType Leaf)){throw 'Texture conversion tool is missing. Extract the whole mod ZIP.'}
$manifest=Get-Content -LiteralPath (Join-Path $RuntimeDirectory 'import-manifest.json') -Raw | ConvertFrom-Json
$stage=Join-Path $RuntimeDirectory ('graphics-hd-import-'+[Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($stage) | Out-Null
$entries=@()
foreach($asset in $manifest.assets){
    $name=[IO.Path]::GetFileName([string]$asset.path)
    $mode=if($name.EndsWith('.rgba')){'resize'}elseif($name.EndsWith('.cube')){'cube'}else{$null}
    if(-not $mode){continue}
    $inputPath=Join-Path $source $name
    $outputPath=Join-Path $stage $name
    & $tool $mode $inputPath $outputPath | Out-Null
    if($LASTEXITCODE -ne 0){throw "HD conversion failed: $name. Existing original textures are preserved."}
    $entries+=@{file=$name;outputSha256=(Get-FileHash -LiteralPath $outputPath -Algorithm SHA256).Hash.ToLowerInvariant()}
}
[IO.File]::WriteAllText((Join-Path $stage 'manifest.json'),(@{format=1;method='local filtered 2x upscale';textures=$entries} | ConvertTo-Json -Depth 5))
Move-Item -LiteralPath $stage -Destination $destination
Write-Host 'Local HD textures are ready. Use Options > HD textures to switch them on or off.'
