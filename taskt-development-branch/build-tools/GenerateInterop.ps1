$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Design
Add-Type @'
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
public class PetaiTypeLibImporter : ITypeLibImporterNotifySink {
    [DllImport("oleaut32.dll", CharSet=CharSet.Unicode, PreserveSig=false)]
    static extern void LoadTypeLibEx(string path, int kind, out System.Runtime.InteropServices.ComTypes.ITypeLib lib);
    public string DirectoryPath;
    public void ReportEvent(ImporterEventKind kind, int code, string message) { }
    public Assembly ResolveRef(object lib) {
        string name = Marshal.GetTypeLibName((System.Runtime.InteropServices.ComTypes.ITypeLib)lib);
        string output = Path.Combine(DirectoryPath, "Interop." + name + ".dll");
        var builder = new TypeLibConverter().ConvertTypeLibToAssembly(lib, output, TypeLibImporterFlags.None, this, null, null, name, null);
        builder.Save(Path.GetFileName(output));
        return builder;
    }
    public void Import(string path, string name) {
        System.Runtime.InteropServices.ComTypes.ITypeLib lib;
        LoadTypeLibEx(path, 2, out lib);
        var output = Path.Combine(DirectoryPath, "Interop." + name + ".dll");
        var builder = new TypeLibConverter().ConvertTypeLibToAssembly(lib, output, TypeLibImporterFlags.None, this, null, null, name, null);
        builder.Save(Path.GetFileName(output));
    }
}
'@
$options = New-Object System.Windows.Forms.Design.AxImporter+Options
$options.outputDirectory = Join-Path $PSScriptRoot 'interop'
New-Item $options.outputDirectory -ItemType Directory -Force | Out-Null
$options.outputName = 'AxInterop.MSTSCLib.dll'
$options.overwriteRCW = $true
$options.silentMode = $true
$importer = New-Object System.Windows.Forms.Design.AxImporter($options)
$importer.GenerateFromFile((New-Object IO.FileInfo('C:\Windows\System32\mstscax.dll')))
$importer.GeneratedAssemblies
$converter = New-Object PetaiTypeLibImporter
$converter.DirectoryPath = $options.outputDirectory
$registry = [Microsoft.Win32.RegistryKey]::OpenBaseKey('ClassesRoot', 'Registry64')
@(
    @('ADODB','B691E011-1797-432E-907A-4D8C69339129','6.1'),
    @('Microsoft.Office.Core','2DF8D04C-5BFA-101B-BDE5-00AA0044DE52','2.8'),
    @('MSDASC','2206CEB0-19C1-11D1-89E0-00C04FD7A829','1.0'),
    @('MSHTML','3050F1C5-98B5-11CF-BB82-00AA00BDCE0B','4.0'),
    @('SHDocVw','EAB22AC0-30C1-11CF-A7EB-0000C05BAE0B','1.1'),
    @('stdole','00020430-0000-0000-C000-000000000046','2.0')
) | ForEach-Object {
    $key = $registry.OpenSubKey("TypeLib\{$($_[1])}\$($_[2])\0\win64")
    if (!$key) { $key = $registry.OpenSubKey("TypeLib\{$($_[1])}\$($_[2])\0\win32") }
    $path = [Environment]::ExpandEnvironmentVariables($key.GetValue(''))
    Write-Output "$($_[0]): $path"
    $converter.Import($path, $_[0])
}
