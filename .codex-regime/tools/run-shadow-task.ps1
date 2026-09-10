param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('monitor', 'scan', 'health', 'calendar')]
    [string]$Mode
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$preferredPython = Join-Path $env:LOCALAPPDATA 'Python\pythoncore-3.14-64\python.exe'
if (Test-Path -LiteralPath $preferredPython) {
    $python = $preferredPython
} else {
    $pythonCommand = Get-Command python -ErrorAction Stop
    $python = $pythonCommand.Source
}

Set-Location -LiteralPath $projectRoot
$env:PYTHONPATH = Join-Path $projectRoot 'src'
& $python -m rh_crypto_bot.scheduled $Mode
exit $LASTEXITCODE
