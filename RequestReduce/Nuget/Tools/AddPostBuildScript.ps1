$solutionDir = [System.IO.Path]::GetDirectoryName($dte.Solution.FullName) + "\"
$path = $installPath.Replace($solutionDir, "`$(SolutionDir)")

$OptiPngDir = Join-Path $path "OptiPng"

$PostBuildScript = "
xcopy /s /y `"$OptiPngDir`" `"`$(TargetDir)`""