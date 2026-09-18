param(
  [Parameter(Mandatory = $true)]
  [ValidatePattern('^\d+\.\d+\.\d+([-.][0-9A-Za-z.-]+)?$')]
  [string]$Version
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$dotnet = Join-Path $repo '.tools\dotnet\dotnet.exe'
$launcherRoot = Join-Path $repo 'Launcher'
$project = Join-Path $launcherRoot 'Crossworlds.Launcher\Crossworlds.Launcher.csproj'
$installerIcon = Join-Path $launcherRoot 'Crossworlds.Launcher\Assets\CrossworldsDesktop-PortalCrest.ico'
$gameBuild = Join-Path $repo 'build\WindowsClient'
$releaseRoot = Join-Path $repo 'build\DirectRelease'
$launcherPublish = Join-Path $releaseRoot 'launcher'
$staging = Join-Path $releaseRoot 'staging'
$releases = Join-Path $releaseRoot 'releases'

if (-not (Test-Path $dotnet)) {
  throw 'Project-local .NET SDK missing. Install it into .tools\dotnet first.'
}
if (-not (Test-Path (Join-Path $gameBuild 'Crossworlds.exe'))) {
  throw 'Windows client missing. Build it with BCE/Build/Windows Client first.'
}
if (-not (Test-Path $installerIcon)) {
  throw "Installer icon missing: $installerIcon"
}

New-Item -ItemType Directory -Force $releaseRoot, $releases | Out-Null
if (Test-Path $launcherPublish) { Remove-Item -LiteralPath $launcherPublish -Recurse -Force }
if (Test-Path $staging) { Remove-Item -LiteralPath $staging -Recurse -Force }

& $dotnet restore $project
if ($LASTEXITCODE -ne 0) { throw 'Launcher restore failed.' }
& $dotnet publish $project -c Release -r win-x64 --self-contained true -o $launcherPublish
if ($LASTEXITCODE -ne 0) { throw 'Launcher build failed.' }

New-Item -ItemType Directory -Force $staging, (Join-Path $staging 'Game') | Out-Null
Copy-Item -Path (Join-Path $launcherPublish '*') -Destination $staging -Recurse -Force
Copy-Item -Path (Join-Path $gameBuild '*') -Destination (Join-Path $staging 'Game') -Recurse -Force

Push-Location $launcherRoot
try {
  & $dotnet tool restore
  if ($LASTEXITCODE -ne 0) { throw 'Velopack tool restore failed.' }
  & $dotnet tool run vpk -- pack `
    --packId CrossworldsBCE `
    --packVersion $Version `
    --packDir $staging `
    --mainExe CrossworldsLauncher.exe `
    --icon $installerIcon `
    --packTitle 'Crossworlds - Beyond the Celestial Edge' `
    --outputDir $releases
  if ($LASTEXITCODE -ne 0) { throw 'Velopack packaging failed.' }
}
finally {
  Pop-Location
}

Write-Host "Installer release $Version created in $releases"
