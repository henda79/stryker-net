# Packs this fork's dotnet-stryker for Assay to bundle. See assay\README.md.
param(
    [Parameter(Mandatory)] [string] $Version,   # e.g. 5.1.1-assay.1
    [string] $Output = (Join-Path $PSScriptRoot '..\..\stryker-net-packages')
)
$ErrorActionPreference = 'Stop'
if ($Version -notmatch '^(\d+\.\d+\.\d+)-assay\.\d+$') { throw "Version '$Version' must look like 5.1.1-assay.1." }
$prefix, $suffix = $Version -split '-', 2
$project = Join-Path $PSScriptRoot '..\src\Stryker.CLI\Stryker.CLI\Stryker.CLI.csproj'
New-Item -ItemType Directory -Force $Output | Out-Null
dotnet pack $project -c Release -p:VersionPrefix=$prefix -p:VersionSuffix=$suffix -o $Output -nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed ($LASTEXITCODE)." }
$package = Join-Path (Resolve-Path $Output) "dotnet-stryker.$Version.nupkg"
if (-not (Test-Path $package)) { throw "Expected $package." }
Write-Host "Packed $package"
