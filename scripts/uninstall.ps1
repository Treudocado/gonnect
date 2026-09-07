[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$progId = 'GOnnect.OutlookAddIn'
$classId = '{A3D2629C-32F1-48E7-BD24-AC02E70427E8}'
$installDirectory = Join-Path $env:LOCALAPPDATA 'GOnnect\OutlookAddIn'

if (Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue) {
    throw 'Bitte Outlook vollständig schließen und die Deinstallation erneut starten.'
}

$registryKeys = @(
    "HKCU:\Software\Microsoft\Office\Outlook\Addins\$progId",
    "HKCU:\Software\Classes\$progId",
    "HKCU:\Software\Classes\CLSID\$classId"
)

foreach ($registryKey in $registryKeys) {
    if (Test-Path -Path $registryKey) {
        Remove-Item -Path $registryKey -Recurse -Force
    }
}

if (Test-Path -LiteralPath $installDirectory) {
    $escapedDirectory = $installDirectory.Replace("'", "''")
    $cleanup = "Start-Sleep -Seconds 2; Remove-Item -LiteralPath '$escapedDirectory' -Recurse -Force"
    Start-Process -FilePath 'powershell.exe' -WindowStyle Hidden -ArgumentList @(
        '-NoProfile',
        '-ExecutionPolicy', 'Bypass',
        '-Command', $cleanup
    ) | Out-Null
}

Write-Host ''
Write-Host 'Das GOnnect Outlook Add-in wurde deinstalliert.' -ForegroundColor Green
