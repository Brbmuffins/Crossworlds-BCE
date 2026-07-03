# build-server.ps1 — headless Linux dedicated-server build + VPS package.
# Run from the repo root:  powershell -ExecutionPolicy Bypass -File tools\build-server.ps1
#
# Prereqs (one-time, interactive):
#   1. git lfs pull            — all LFS objects present (no pointer files)
#   2. Unity editor version in ProjectSettings/ProjectVersion.txt installed
#      with the "Linux Dedicated Server Build Support" module.
#
# Output: build\DedicatedServer\  renamed to the names the VPS systemd unit
# expects (CrossworldsBCE.x86_64 + CrossworldsBCE_Data), then packed into
# build\crossworlds-server.tar.gz for upload with tools\deploy-server.sh.

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
& $unity -batchmode -quit -projectPath $repo `
  -executeMethod BuildScript.BuildDedicatedServer `
  -logFile $log
if ($LASTEXITCODE -ne 0) { throw "Build FAILED (exit $LASTEXITCODE) - see $log" }

# Rename to the exact names /etc/systemd/system/crossworlds.service expects.
# Binary and _Data folder names MUST match each other.
$out = "$repo\build\DedicatedServer"
if (Test-Path "$out\Crossworlds.x86_64") {
  Move-Item -Force "$out\Crossworlds.x86_64" "$out\CrossworldsBCE.x86_64"
  Move-Item -Force "$out\Crossworlds_Data"  "$out\CrossworldsBCE_Data"
}

$tar = "$repo\build\crossworlds-server.tar.gz"
if (Test-Path $tar) { Remove-Item $tar }
tar -czf $tar -C $out .
Write-Host "OK -> $tar"
Write-Host "Next: scp $tar and tools/deploy-server.sh to the VPS, then run: sudo bash deploy-server.sh"
