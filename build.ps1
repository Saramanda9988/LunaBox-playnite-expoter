param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [string]$ToolboxPath = "",
    [string]$PackageDirectory = ""
)

$ErrorActionPreference = "Stop"
$pluginRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectFile = Join-Path $pluginRoot "LunaBoxExporter.csproj"
$buildOutput = Join-Path $pluginRoot "bin\$Configuration"

dotnet build $projectFile --configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "LunaBox Exporter build failed."
}

if ([string]::IsNullOrWhiteSpace($ToolboxPath)) {
    $toolboxCandidates = @(
        (Join-Path $env:LOCALAPPDATA "Playnite\Toolbox.exe"),
        (Join-Path $env:LOCALAPPDATA "Programs\Playnite\Toolbox.exe"),
        (Join-Path $env:ProgramFiles "Playnite\Toolbox.exe"),
        (Join-Path ${env:ProgramFiles(x86)} "Playnite\Toolbox.exe")
    )
    $ToolboxPath = $toolboxCandidates |
        Where-Object { Test-Path -LiteralPath $_ } |
        Select-Object -First 1
}

if ([string]::IsNullOrWhiteSpace($ToolboxPath)) {
    Write-Host "Build completed: $buildOutput"
    Write-Host "Provide -ToolboxPath to create a .pext package."
    exit 0
}

if ([string]::IsNullOrWhiteSpace($PackageDirectory)) {
    $PackageDirectory = Join-Path $pluginRoot "dist"
}

New-Item -ItemType Directory -Path $PackageDirectory -Force | Out-Null
& $ToolboxPath pack $buildOutput $PackageDirectory
if ($LASTEXITCODE -ne 0) {
    throw "Playnite Toolbox package command failed."
}

Write-Host "Package completed: $PackageDirectory"
