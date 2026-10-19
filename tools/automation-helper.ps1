param([switch]$ValidateOnly, [switch]$SelfTest, [int]$BrowserTestProcessId)
$ErrorActionPreference = 'Stop'
$source = Join-Path $PSScriptRoot 'automation-helper.cs'
$references = @(
  'System.dll',
  'System.Core.dll',
  'System.Windows.Forms.dll',
  'System.Drawing.dll',
  'System.Web.Extensions.dll',
  'UIAutomationClient.dll',
  'UIAutomationTypes.dll',
  'WindowsBase.dll'
)
$resolved = foreach ($reference in $references) {
  if ($reference.EndsWith('.dll', [StringComparison]::OrdinalIgnoreCase)) {
    $assembly = [System.Reflection.Assembly]::LoadWithPartialName([IO.Path]::GetFileNameWithoutExtension($reference))
    if (-not $assembly) { throw "Required framework assembly unavailable: $reference" }
    $assembly.Location
  } else { $reference }
}
Add-Type -Path $source -ReferencedAssemblies $resolved
if ($SelfTest) { [PetAutomation]::SelfTest() }
elseif ($BrowserTestProcessId -gt 0) { [PetAutomation]::BrowserSelfTest($BrowserTestProcessId) }
else { [PetAutomation]::MainLoop([bool]$ValidateOnly) }
