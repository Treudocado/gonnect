[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$solution = Join-Path $projectRoot 'GOnnect.OutlookAddIn.sln'
$vsWhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'

if (-not (Test-Path -LiteralPath $vsWhere -PathType Leaf)) {
    throw 'vswhere.exe wurde nicht gefunden.'
}

$msBuild = & $vsWhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' |
    Select-Object -First 1
if (-not $msBuild) {
    throw 'MSBuild wurde nicht gefunden.'
}

& $msBuild $solution /m /restore /t:Rebuild /p:Configuration=Release /p:Platform=x64
if ($LASTEXITCODE -ne 0) {
    throw "MSBuild ist mit Exitcode $LASTEXITCODE fehlgeschlagen."
}

$testExecutable = Join-Path $projectRoot 'tests\GOnnect.OutlookAddIn.Tests\bin\Release\GOnnect.OutlookAddIn.Tests.exe'
& $testExecutable
if ($LASTEXITCODE -ne 0) {
    throw "Die Tests sind mit Exitcode $LASTEXITCODE fehlgeschlagen."
}

$smokeTestId = [Guid]::NewGuid().ToString('N')
$smokeTestRoot = "HKCU:\Software\GOnnectOutlookAddInTests\$smokeTestId"
$runComActivationTest = $env:GITHUB_ACTIONS -eq 'true'
$smokeTestClassesRoot = if ($runComActivationTest) {
    'HKCU:\Software\Classes'
} else {
    Join-Path $smokeTestRoot 'Classes'
}
$smokeTestOutlookRoot = Join-Path $smokeTestRoot 'OutlookAddins'
$smokeTestDirectory = Join-Path $env:TEMP "GOnnectOutlookAddInTests\$smokeTestId"
$smokeTestSource = Join-Path $smokeTestDirectory 'Source'
$smokeTestInstall = Join-Path $smokeTestDirectory 'Installed'
$smokeTestProgId = 'GOnnect.OutlookAddIn'
$smokeTestClassId = '{A3D2629C-32F1-48E7-BD24-AC02E70427E8}'

try {
    New-Item -ItemType Directory -Path $smokeTestSource -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $projectRoot 'src\GOnnect.OutlookAddIn\bin\Release\GOnnect.OutlookAddIn.dll') -Destination $smokeTestSource
    Copy-Item -LiteralPath (Join-Path $projectRoot 'scripts\install.ps1') -Destination $smokeTestSource
    Copy-Item -LiteralPath (Join-Path $projectRoot 'scripts\uninstall.ps1') -Destination $smokeTestSource
    Copy-Item -LiteralPath (Join-Path $projectRoot 'scripts\uninstall.cmd') -Destination $smokeTestSource

    & (Join-Path $smokeTestSource 'install.ps1') `
        -InstallDirectory $smokeTestInstall `
        -ClassesRoot $smokeTestClassesRoot `
        -OutlookAddInRoot $smokeTestOutlookRoot

    $classKey = Join-Path $smokeTestClassesRoot "CLSID\$smokeTestClassId"
    $inprocKey = Join-Path $classKey 'InprocServer32'
    $addInKey = Join-Path $smokeTestOutlookRoot $smokeTestProgId
    if ((Get-Item -Path $classKey).GetValue('') -ne 'GOnnect Outlook Add-in') {
        throw 'Der Installations-Smoketest konnte die COM-Registrierung nicht bestätigen.'
    }
    $inprocProperties = Get-ItemProperty -Path $inprocKey
    if ((Get-Item -Path $inprocKey).GetValue('') -ne 'mscoree.dll' -or
        $inprocProperties.Class -ne 'GOnnect.OutlookAddIn.Connect' -or
        $inprocProperties.Assembly -notlike 'GOnnect.OutlookAddIn,*' -or
        $inprocProperties.RuntimeVersion -ne 'v4.0.30319' -or
        $inprocProperties.CodeBase -notlike 'file:///*GOnnect.OutlookAddIn.dll' -or
        $inprocProperties.ThreadingModel -ne 'Both') {
        throw 'Der Installations-Smoketest hat unvollständige COM-Werte gefunden.'
    }
    if ((Get-ItemPropertyValue -Path $addInKey -Name 'LoadBehavior') -ne 3) {
        throw 'Der Installations-Smoketest konnte die Outlook-Registrierung nicht bestätigen.'
    }

    if ($runComActivationTest) {
        & $testExecutable --com-activation
        if ($LASTEXITCODE -ne 0) {
            throw "Der COM-Aktivierungstest ist mit Exitcode $LASTEXITCODE fehlgeschlagen."
        }
    }

    & (Join-Path $smokeTestSource 'uninstall.ps1') `
        -InstallDirectory $smokeTestInstall `
        -ClassesRoot $smokeTestClassesRoot `
        -OutlookAddInRoot $smokeTestOutlookRoot

    if ((Test-Path -Path $classKey) -or (Test-Path -Path $addInKey)) {
        throw 'Der Deinstallations-Smoketest hat Registrierungseinträge zurückgelassen.'
    }
}
finally {
    $smokeTestRegistryKeys = @(
        (Join-Path $smokeTestOutlookRoot $smokeTestProgId),
        (Join-Path $smokeTestClassesRoot $smokeTestProgId),
        (Join-Path $smokeTestClassesRoot "CLSID\$smokeTestClassId")
    )
    foreach ($registryKey in $smokeTestRegistryKeys) {
        if (Test-Path -Path $registryKey) {
            Remove-Item -Path $registryKey -Recurse -Force
        }
    }
    if (Test-Path -Path $smokeTestRoot) {
        Remove-Item -Path $smokeTestRoot -Recurse -Force
    }
    if (Test-Path -LiteralPath $smokeTestDirectory) {
        Remove-Item -LiteralPath $smokeTestDirectory -Recurse -Force
    }
}

$artifactRoot = Join-Path $projectRoot 'artifacts'
$packageDirectory = Join-Path $artifactRoot 'GOnnect-Outlook-AddIn'
$archive = Join-Path $artifactRoot 'GOnnect-Outlook-AddIn-x64.zip'

if (Test-Path -LiteralPath $artifactRoot) {
    Remove-Item -LiteralPath $artifactRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $packageDirectory -Force | Out-Null

Copy-Item -LiteralPath (Join-Path $projectRoot 'src\GOnnect.OutlookAddIn\bin\Release\GOnnect.OutlookAddIn.dll') -Destination $packageDirectory
Copy-Item -LiteralPath (Join-Path $projectRoot 'scripts\install.ps1') -Destination $packageDirectory
Copy-Item -LiteralPath (Join-Path $projectRoot 'scripts\install.cmd') -Destination $packageDirectory
Copy-Item -LiteralPath (Join-Path $projectRoot 'scripts\uninstall.ps1') -Destination $packageDirectory
Copy-Item -LiteralPath (Join-Path $projectRoot 'scripts\uninstall.cmd') -Destination $packageDirectory
Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md') -Destination $packageDirectory

Compress-Archive -Path $packageDirectory -DestinationPath $archive -CompressionLevel Optimal
Write-Host "Package created: $archive"
