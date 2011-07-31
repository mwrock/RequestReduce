$solutionDir = [System.IO.Path]::GetDirectoryName($dte.Solution.FullName) + "\"
$path = $installPath.Replace($solutionDir, "`$(SolutionDir)")

$OptiPngDir = Join-Path $path "pngoptimization"

$PostBuildScript = "
xcopy /s /y `"$OptiPngDir`" `"`$(TargetDir)`""