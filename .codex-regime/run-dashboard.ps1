$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$python = Join-Path $projectRoot '.venv\Scripts\python.exe'
if (-not (Test-Path -LiteralPath $python)) {
    $python = Join-Path $env:LOCALAPPDATA 'Python\pythoncore-3.14-64\python.exe'
}
Set-Location -LiteralPath $projectRoot
$env:PYTHONPATH = Join-Path $projectRoot 'src'
& $python -m rh_crypto_bot.cli dashboard --host 127.0.0.1 --port 8765
exit $LASTEXITCODE
