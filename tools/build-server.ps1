# build-server.ps1 - headless Linux dedicated-server build + VPS package.
# Run from the repo root:  powershell -ExecutionPolicy Bypass -File tools\build-server.ps1
#
# Prereqs (one-time, interactive):
#   1. git lfs pull            - all LFS objects present (no pointer files)
#   2. Unity editor version in ProjectSettings/ProjectVersion.txt installed
#      with the "Linux Dedicated Server Build Support" module.
#
# Output: build\DedicatedServer\CrossWords.x86_64 (+ CrossWords_Data) - the exact
# names the live crossworlds-server systemd unit's ExecStart expects - then packed
# into build\crossworlds-server.tar.gz for upload with tools\deploy-server.sh.

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot

# Resolve editor from ProjectVersion.txt so upgrades don't break this script
$version = (Get-Content "$repo\ProjectSettings\ProjectVersion.txt" | Select-String 'm_EditorVersion: (.+)').Matches[0].Groups[1].Value.Trim()
$unity = "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"
if (-not (Test-Path $unity)) { throw "Unity $version not installed at $unity" }

# Refuse to build with un-smudged LFS pointers (broken meshes/textures/prefabs)
$pointers = git lfs ls-files -n | Where-Object {
  (Test-Path $_) -and ((Get-Content $_ -TotalCount 1 -ErrorAction SilentlyContinue) -like 'version https://git-lfs*')
}
if ($pointers) { throw "LFS pointer files present - run 'git lfs pull' first:`n$($pointers -join "`n")" }

Write-Host "Building Linux dedicated server with Unity $version (batchmode)..."
$log = "$repo\build\server-build.log"
New-Item -ItemType Directory -Force "$repo\build" | Out-Null
$unityArgs = @(
  '-batchmode',
  '-quit',
  '-projectPath', $repo,
  '-executeMethod', 'BuildScript.BuildDedicatedServer',
  '-logFile', $log
)
$unityProcess = Start-Process -FilePath $unity -ArgumentList $unityArgs `
  -Wait -PassThru -WindowStyle Hidden
if ($unityProcess.ExitCode -ne 0) {
  throw "Build FAILED (exit $($unityProcess.ExitCode)) - see $log"
}

# BuildScript already emits CrossWords.x86_64 / CrossWords_Data (the names the
# live crossworlds-server unit expects) - no rename needed.
$out = "$repo\build\DedicatedServer"
if (-not (Test-Path "$out\CrossWords.x86_64")) {
  throw "Expected $out\CrossWords.x86_64 not found - did BuildScript.locationPathName change?"
}

$tar = "$repo\build\crossworlds-server.tar.gz"
if (Test-Path $tar) { Remove-Item $tar }
# Exclude Unity's IL2CPP symbol/backup folders - huge and must not ship
tar -czf $tar -C $out --exclude "*_BackUpThisFolder_ButDontShipItWithYourGame" --exclude "*_BurstDebugInformation_DoNotShip" .
Write-Host "OK -> $tar"
Write-Host "Next: scp $tar and tools/deploy-server.sh to the VPS, then run: sudo bash deploy-server.sh"
