$solutionDir = [System.IO.Path]::GetDirectoryName($dte.Solution.FullName) + "\"
$path = $installPath.Replace($solutionDir, "`$(SolutionDir)")

$OptiPngDir = Join-Path $path "pngoptimization"

$PostBuildScript = "
start /MIN xcopy /s /y `"$OptiPngDir\*.exe`" `"`$(TargetDir)`""