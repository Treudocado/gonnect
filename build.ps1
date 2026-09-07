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

