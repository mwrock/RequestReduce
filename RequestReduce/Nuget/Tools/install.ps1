param($installPath, $toolsPath, $package, $project)

. (Join-Path $toolsPath "AddPostBuildScript.ps1")

if($project.Object -is [VsWebSite.VSWebSite]) {
	$target = Join-Path $project.FullName "bin"
	$dest = Join-Path $toolsPath  "..\\pngoptimization\\*.exe"
	Copy-Item $dest $target
}
else {
	# Get the current Post Build Event cmd
	$currentPostBuildCmd = $project.Properties.Item("PostBuildEvent").Value
	
	# Append our post build command if it's not already there

	if (!$currentPostBuildCmd.Contains($PostBuildScript)) {
	    $project.Properties.Item("PostBuildEvent").Value += $PostBuildScript
	}
}