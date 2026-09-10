$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$python = Join-Path $env:LOCALAPPDATA 'Python\pythoncore-3.14-64\python.exe'
Set-Location -LiteralPath $projectRoot
$env:PYTHONPATH = Join-Path $projectRoot 'src'
& $python -m rh_crypto_bot.order_flow
exit $LASTEXITCODE
