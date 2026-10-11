param(
    [string]$Game = 'C:\Program Files (x86)\Steam\steamapps\common\CastleMiner Z',
    [Parameter(Mandatory=$true)][string]$PrivateOutput,
    [Parameter(Mandatory=$true)][string]$Decoder
)
$ErrorActionPreference = 'Stop'
$Game = (Resolve-Path -LiteralPath $Game).Path
$Decoder = (Resolve-Path -LiteralPath $Decoder).Path
$PrivateOutput = [IO.Path]::GetFullPath($PrivateOutput)
foreach ($protected in @($Game,$PSScriptRoot)) {
    $protected = [IO.Path]::GetFullPath($protected).TrimEnd('\')
    if ($PrivateOutput.StartsWith($protected + '\',[StringComparison]::OrdinalIgnoreCase) -or $PrivateOutput.Equals($protected,[StringComparison]::OrdinalIgnoreCase)) {
        throw 'Keep audio conversion outside the Steam installation and source project'
    }
}
$audio = Join-Path $PrivateOutput 'audio'
$stage = Join-Path (Split-Path $PrivateOutput -Parent) 'import-tools\audio-source'
New-Item -ItemType Directory -Path $audio,$stage -Force | Out-Null
foreach ($bank in @('Sounds','SoundsStreaming')) {
    Copy-Item -LiteralPath (Join-Path $Game "Content\$bank.xwb") -Destination (Join-Path $stage "$bank.xwb") -Force
    # vgmstream resolves cue names via a matching sidecar name for each bank.
    Copy-Item -LiteralPath (Join-Path $Game 'Content\Sounds.xsb') -Destination (Join-Path $stage "$bank.xsb") -Force
}
function Get-ClipMetadata([string]$Bank,[int]$Index) {
    $metadata = (& $Decoder -m -s $Index $Bank) -join "`n"
    if ($LASTEXITCODE -ne 0) { throw "Audio metadata failed: $Bank stream $Index" }
    return $metadata
}
$entries = [Collections.Generic.List[object]]::new()
foreach ($bank in @('Sounds','SoundsStreaming')) {
    $path = Join-Path $stage "$bank.xwb"
    $first = Get-ClipMetadata $path 1
    if ($first -notmatch 'stream count:\s+(\d+)') { throw 'Missing original audio bank count' }
    $count = [int]$Matches[1]
    $stream = $bank -eq 'SoundsStreaming'
    $expected = if ($stream) {12} else {148}
    if ($count -ne $expected) { throw "Unrecognized game audio version: $bank has $count clips" }
    for ($index=1; $index -le $count; $index++) {
        $metadata = if ($index -eq 1) {$first} else {Get-ClipMetadata $path $index}
        if ($metadata -notmatch '(?m)^stream name:\s*(.+)$') { throw "Cue name missing: $bank stream $index" }
        $name = $Matches[1].Trim()
        $prefix = if ($stream) {'stream'} else {'sfx'}
        $safeName = $name -replace '[\\/:*?"<>|]','_'
        $filename = '{0}-{1:D4}-{2}.wav' -f $prefix,$index,$safeName
        $destination = Join-Path $audio $filename
        if (-not (Test-Path -LiteralPath $destination)) {
            & $Decoder -i -s $index -o $destination $path | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "Audio decode failed: $name" }
        }
        $aliases = @($name -split ';\s*' | ForEach-Object {$_.Trim()} | Where-Object {$_})
        $entries.Add(@{File=$filename;Stream=$stream;Aliases=$aliases})
    }
}
$manifest = Join-Path $audio 'cues.bin'
$writer = [IO.BinaryWriter]::new([IO.File]::Create($manifest + '.tmp'))
try {
    $writer.Write([uint32]0x31414d43)
    $writer.Write([int]$entries.Count)
    foreach ($entry in $entries) {
        $writer.Write([string]$entry.File)
        $writer.Write([byte][int]$entry.Stream)
        $writer.Write([int]$entry.Aliases.Count)
        foreach ($alias in $entry.Aliases) {$writer.Write([string]$alias)}
    }
    $writer.Flush()
} finally {$writer.Dispose()}
Move-Item -LiteralPath ($manifest + '.tmp') -Destination $manifest -Force
Write-Output "Private audio import complete: $($entries.Count) clips"
