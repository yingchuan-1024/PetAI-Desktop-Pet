param([switch]$Offline)
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$nuget = Join-Path $PSScriptRoot '.nuget\nuget.exe'
if (!(Test-Path $nuget)) {
    if ($Offline) { throw 'Offline build requires cached NuGet executable' }
    Invoke-WebRequest 'https://dist.nuget.org/win-x86-commandline/v6.14.0/nuget.exe' -OutFile $nuget -UseBasicParsing
}
if (!$Offline) {
    & $nuget restore 'taskt\packages.config' -PackagesDirectory 'packages' -Source 'https://api.nuget.org/v3/index.json' -NonInteractive -Verbosity quiet
    if ($LASTEXITCODE) { throw 'NuGet restore failed' }
}
$toolRoot = Join-Path $PSScriptRoot 'build-tools'
@(@('Microsoft.Build.Runtime','17.14.28'), @('Microsoft.NETFramework.ReferenceAssemblies.net48','1.0.3'), @('Microsoft.Net.Compilers.Toolset','4.14.0')) | ForEach-Object {
    if (!(Test-Path (Join-Path $toolRoot "$($_[0]).$($_[1])"))) {
        if ($Offline) { throw "Missing cached build tool: $($_[0])" }
        & $nuget install $_[0] -Version $_[1] -OutputDirectory $toolRoot -Source 'https://api.nuget.org/v3/index.json' -NonInteractive -Verbosity quiet
        if ($LASTEXITCODE) { throw 'Build tool restore failed' }
    }
}
$msbuildDir = Join-Path $toolRoot 'Microsoft.Build.Runtime.17.14.28\contentFiles\any\net472'
Get-ChildItem $toolRoot -Directory | ForEach-Object {
    foreach ($framework in @('net472', 'net462', 'net461', 'net46', 'net45', 'netstandard2.0')) {
        $lib = Join-Path $_.FullName "lib\$framework"
        if (Test-Path $lib) {
            Copy-Item (Join-Path $lib '*.dll') $msbuildDir -Force
            break
        }
    }
}
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $toolRoot 'GenerateInterop.ps1')
if ($LASTEXITCODE) { throw 'COM wrapper generation failed' }
$frameworkRoot = Join-Path $toolRoot 'Microsoft.NETFramework.ReferenceAssemblies.net48.1.0.3\build\'
$compilerRoot = Join-Path $toolRoot 'Microsoft.Net.Compilers.Toolset.4.14.0\tasks\net472'
& (Join-Path $msbuildDir 'MSBuild.exe') 'taskt\taskt.csproj' /t:Build /p:Configuration=Release /p:Platform=AnyCPU /p:PreBuildEvent= /p:PostBuildEvent= /p:GenerateSatelliteAssembliesForCore=true "/p:TargetFrameworkRootPath=$frameworkRoot" "/p:RoslynTargetsPath=$compilerRoot" "/p:CustomBeforeMicrosoftCommonTargets=$toolRoot\LocalInterop.targets" /nologo /v:minimal
if ($LASTEXITCODE) { throw 'Taskt build failed' }
$runtime = Join-Path (Split-Path $PSScriptRoot -Parent) 'taskt-runtime'
New-Item $runtime -ItemType Directory -Force | Out-Null
Copy-Item 'taskt\bin\Release\*' $runtime -Recurse -Force
Copy-Item 'LICENSE' (Join-Path $runtime 'LICENSE') -Force
Copy-Item 'NOTICE.txt' (Join-Path $runtime 'NOTICE.txt') -Force
Copy-Item 'PETAI-CHANGES.txt' (Join-Path $runtime 'PETAI-CHANGES.txt') -Force
foreach ($folder in @('Samples', 'Manual')) {
    $source = Join-Path 'taskt' $folder
    if (Test-Path $source) { Copy-Item $source $runtime -Recurse -Force }
}
$licenses = Join-Path $runtime 'third-party-licenses'
New-Item $licenses -ItemType Directory -Force | Out-Null
Get-ChildItem 'packages' -Directory | ForEach-Object {
    $package = $_
    Get-ChildItem $package.FullName -File -Recurse | Where-Object { $_.Name -match '^(licen[sc]e|notice|copying)(\.|$)' } | ForEach-Object {
        $target = Join-Path $licenses $package.Name
        New-Item $target -ItemType Directory -Force | Out-Null
        Copy-Item $_.FullName $target -Force
    }
    Get-ChildItem $package.FullName -Filter '*.nupkg' -File | ForEach-Object {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $zip = [IO.Compression.ZipFile]::OpenRead($_.FullName)
        try {
            $entry = $zip.Entries | Where-Object FullName -Like '*.nuspec' | Select-Object -First 1
            if ($entry) {
                $target = Join-Path $licenses $package.Name
                New-Item $target -ItemType Directory -Force | Out-Null
                [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, (Join-Path $target 'package.nuspec'), $true)
            }
        } finally { $zip.Dispose() }
    }
}
$check = Start-Process (Join-Path $runtime 'taskt.exe') -ArgumentList '--petai-check' -Wait -PassThru -NoNewWindow
if ($check.ExitCode) { throw "Taskt self-check failed: $($check.ExitCode)" }
Write-Output "Runtime ready: $runtime\taskt.exe"
