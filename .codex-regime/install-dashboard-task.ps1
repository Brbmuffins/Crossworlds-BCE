$ErrorActionPreference = 'Stop'

$taskName = 'Robinhood Crypto Dashboard'
$projectRoot = Split-Path -Parent $PSScriptRoot
$python = Join-Path $projectRoot '.venv\Scripts\pythonw.exe'
if (-not (Test-Path -LiteralPath $python)) {
    throw 'The project Python environment is missing. Recreate .venv before installing the dashboard task.'
}
$action = New-ScheduledTaskAction `
    -Execute $python `
    -Argument '-m rh_crypto_bot.cli dashboard --host 127.0.0.1 --port 8765' `
    -WorkingDirectory $projectRoot
$logonTrigger = New-ScheduledTaskTrigger -AtLogOn
$recoveryTrigger = New-ScheduledTaskTrigger `
    -Once `
    -At ((Get-Date).AddMinutes(1)) `
    -RepetitionInterval (New-TimeSpan -Minutes 5)
$settings = New-ScheduledTaskSettingsSet `
    -MultipleInstances IgnoreNew `
    -RestartCount 3 `
    -RestartInterval (New-TimeSpan -Minutes 1) `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -StartWhenAvailable `
    -ExecutionTimeLimit ([TimeSpan]::Zero)
$principal = New-ScheduledTaskPrincipal `
    -UserId "$env:USERDOMAIN\$env:USERNAME" `
    -LogonType Interactive `
    -RunLevel Limited

Register-ScheduledTask `
    -TaskName $taskName `
    -Action $action `
    -Trigger @($logonTrigger, $recoveryTrigger) `
    -Settings $settings `
    -Principal $principal `
    -Description 'Local read-only crypto bot dashboard on 127.0.0.1:8765.' `
    -Force | Out-Null

Start-ScheduledTask -TaskName $taskName
Write-Output "Installed and started: $taskName"
