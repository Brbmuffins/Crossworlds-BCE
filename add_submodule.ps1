Set-Location "D:\Crossworlds"
$log = "add_submodule_log.txt"
"Starting..." | Tee-Object $log

Remove-Item -Force ".git\index.lock" -ErrorAction SilentlyContinue
Remove-Item -Force ".git\modules\web\config.lock" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force ".git\modules\web" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force "web" -ErrorAction SilentlyContinue
"Lock files and old state cleared." | Tee-Object $log -Append

git submodule add -b CrossWorldsWEB https://github.com/Brbmuffins/Cross-Worlds-Web.git web 2>&1 | Tee-Object $log -Append

if ($LASTEXITCODE -eq 0) {
    git commit -m "Add CrossWorldsWEB as submodule at web/" 2>&1 | Tee-Object $log -Append
    "SUCCESS - push via GitHub Desktop." | Tee-Object $log -Append
} else {
    "FAILED - see above." | Tee-Object $log -Append
}

Write-Host "Press any key to close."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
