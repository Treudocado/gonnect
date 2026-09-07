[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$progId = 'GOnnect.OutlookAddIn'
$classId = '{A3D2629C-32F1-48E7-BD24-AC02E70427E8}'
$className = 'GOnnect.OutlookAddIn.Connect'
$installDirectory = Join-Path $env:LOCALAPPDATA 'GOnnect\OutlookAddIn'
$sourceAssembly = Join-Path $PSScriptRoot 'GOnnect.OutlookAddIn.dll'
$installedAssembly = Join-Path $installDirectory 'GOnnect.OutlookAddIn.dll'

if (-not [Environment]::Is64BitProcess) {
    throw 'Die Installation muss mit der 64-Bit-Version von PowerShell ausgeführt werden.'
}

if (Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue) {
    throw 'Bitte Outlook vollständig schließen und die Installation erneut starten.'
}

if (-not (Test-Path -LiteralPath $sourceAssembly -PathType Leaf)) {
    throw "Die Add-in-Datei wurde nicht gefunden: $sourceAssembly"
}

New-Item -ItemType Directory -Path $installDirectory -Force | Out-Null
Copy-Item -LiteralPath $sourceAssembly -Destination $installedAssembly -Force
Unblock-File -LiteralPath $installedAssembly

foreach ($fileName in @('uninstall.ps1', 'uninstall.cmd')) {
    $sourceFile = Join-Path $PSScriptRoot $fileName
    if (Test-Path -LiteralPath $sourceFile -PathType Leaf) {
        Copy-Item -LiteralPath $sourceFile -Destination $installDirectory -Force
    }
}

$assemblyName = [Reflection.AssemblyName]::GetAssemblyName($installedAssembly)
$assemblyFullName = $assemblyName.FullName
$assemblyVersion = $assemblyName.Version.ToString()
$codeBase = ([Uri]$installedAssembly).AbsoluteUri
$runtimeVersion = 'v4.0.30319'

function New-RegistryKey {
    param([Parameter(Mandatory = $true)][string]$Path)

    New-Item -Path $Path -Force | Out-Null
}

function Set-DefaultRegistryValue {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Value
    )

    New-RegistryKey -Path $Path
    Set-Item -Path $Path -Value $Value
}

function Set-StringRegistryValue {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Value
    )

    New-RegistryKey -Path $Path
    New-ItemProperty -Path $Path -Name $Name -Value $Value -PropertyType String -Force |
        Out-Null
}

$classesRoot = 'HKCU:\Software\Classes'
$classKey = Join-Path $classesRoot "CLSID\$classId"
$inprocKey = Join-Path $classKey 'InprocServer32'
$versionKey = Join-Path $inprocKey $assemblyVersion
$progIdKey = Join-Path $classesRoot $progId
$outlookAddInKey = "HKCU:\Software\Microsoft\Office\Outlook\Addins\$progId"

Set-DefaultRegistryValue -Path $classKey -Value 'GOnnect Outlook Add-in'
Set-DefaultRegistryValue -Path (Join-Path $classKey 'ProgId') -Value $progId
Set-DefaultRegistryValue -Path $inprocKey -Value 'mscoree.dll'
Set-StringRegistryValue -Path $inprocKey -Name 'ThreadingModel' -Value 'Both'
Set-StringRegistryValue -Path $inprocKey -Name 'Class' -Value $className
Set-StringRegistryValue -Path $inprocKey -Name 'Assembly' -Value $assemblyFullName
Set-StringRegistryValue -Path $inprocKey -Name 'RuntimeVersion' -Value $runtimeVersion
Set-StringRegistryValue -Path $inprocKey -Name 'CodeBase' -Value $codeBase

New-RegistryKey -Path $versionKey
Set-StringRegistryValue -Path $versionKey -Name 'Class' -Value $className
Set-StringRegistryValue -Path $versionKey -Name 'Assembly' -Value $assemblyFullName
Set-StringRegistryValue -Path $versionKey -Name 'RuntimeVersion' -Value $runtimeVersion
Set-StringRegistryValue -Path $versionKey -Name 'CodeBase' -Value $codeBase

Set-DefaultRegistryValue -Path (Join-Path $classKey 'Implemented Categories\{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}') -Value ''
Set-DefaultRegistryValue -Path $progIdKey -Value 'GOnnect Outlook Add-in'
Set-DefaultRegistryValue -Path (Join-Path $progIdKey 'CLSID') -Value $classId

New-RegistryKey -Path $outlookAddInKey
New-ItemProperty -Path $outlookAddInKey -Name 'FriendlyName' -Value 'GOnnect-Anruffunktion' -PropertyType String -Force | Out-Null
New-ItemProperty -Path $outlookAddInKey -Name 'Description' -Value 'Wählt Outlook-Kontakte über GOnnect.' -PropertyType String -Force | Out-Null
New-ItemProperty -Path $outlookAddInKey -Name 'LoadBehavior' -Value 3 -PropertyType DWord -Force | Out-Null
New-ItemProperty -Path $outlookAddInKey -Name 'CommandLineSafe' -Value 0 -PropertyType DWord -Force | Out-Null

Write-Host ''
Write-Host 'Das GOnnect Outlook Add-in wurde für den aktuellen Benutzer installiert.' -ForegroundColor Green
Write-Host 'Outlook kann jetzt wieder gestartet werden.'
