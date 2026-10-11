param(
    [string]$Game = 'C:\Program Files (x86)\Steam\steamapps\common\CastleMiner Z',
    [Parameter(Mandatory=$true)][string]$PrivateOutput,
    [ValidateSet('base','models','catalog','terrain-pack','presentation','generator','dragons','dragon-effects','mode-oracle','effects','enemy-oracle','item-dynamics','crosshair','avatar','barrels','reloads','selector','seed-oracle','weapon-effects','scopes','storage-ui')][string]$Mode = 'base',
    [int]$Shard = 0,
    [int]$Shards = 8
)
$ErrorActionPreference = 'Stop'
$Game = (Resolve-Path -LiteralPath $Game).Path
$PrivateOutput = [IO.Path]::GetFullPath($PrivateOutput)
$project = [IO.Path]::GetFullPath($PSScriptRoot).TrimEnd('\')
if ($PrivateOutput.StartsWith($project + '\',[StringComparison]::OrdinalIgnoreCase) -or
    $PrivateOutput.Equals($project,[StringComparison]::OrdinalIgnoreCase) -or
    $PrivateOutput.StartsWith($Game + '\',[StringComparison]::OrdinalIgnoreCase) -or
    $PrivateOutput.Equals($Game,[StringComparison]::OrdinalIgnoreCase)) {
    throw 'Keep private imported files outside both the source project and the Steam installation'
}
if (-not (Test-Path -LiteralPath (Join-Path $Game 'CastleMinerZ.exe'))) { throw 'Owned CastleMiner Z installation missing' }
New-Item -ItemType Directory -Path $PrivateOutput -Force | Out-Null
$toolsDir = Join-Path (Split-Path $PrivateOutput -Parent) 'import-tools'
New-Item -ItemType Directory -Path $toolsDir -Force | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
$references = @(Get-ChildItem -LiteralPath (Join-Path $env:WINDIR 'Microsoft.NET\assembly\GAC_32') -Recurse -Filter 'Microsoft.Xna.Framework*.dll' | ForEach-Object { '/r:' + $_.FullName })
if ($references.Count -eq 0) { throw 'XNA 4 runtime is required for this private Windows conversion' }
$references += '/r:' + (Join-Path $Game 'DNA.Common.dll')
$exe = Join-Path $toolsDir 'ImportGame.exe'
$sources = @('ImportGame.cs','CatalogImporter.cs','AnimationImporter.cs','TerrainPackImporter.cs','GameplayImporter.cs','PresentationImporter.cs','GeneratorImporter.cs','HudImporter.cs','DragonImporter.cs','EffectsImporter.cs','EnemyOracleImporter.cs','ItemDynamicsImporter.cs','MagazineGeometryImporter.cs','AvatarImporter.cs','OfflineModelImporter.cs','BarrelImporter.cs','ReloadImporter.cs','ScopeImporter.cs') | ForEach-Object { Join-Path $PSScriptRoot $_ }
& $compiler /nologo /target:exe /platform:x86 "/out:$exe" @references @sources
if ($LASTEXITCODE -ne 0) { throw 'Importer compilation failed' }
if ($Mode -eq 'terrain-pack') { & $exe $Game $PrivateOutput terrain-pack 8192 $Shard $Shards }
elseif ($Mode -eq 'base') { & $exe $Game $PrivateOutput }
else { & $exe $Game $PrivateOutput $Mode }
if ($LASTEXITCODE -ne 0) { throw 'Private game import failed' }
# Imported files are private game content; do not add them to a shared source archive.
