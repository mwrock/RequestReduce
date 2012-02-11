param($installPath, $toolsPath, $package, $project)

. (Join-Path $toolsPath "AddPostBuildScript.ps1")

if($project.Type -eq "Web Site") {
	$target = Join-Path $project.Properties.Item("FullPath").Value "bin\\optipng.exe"
	Remove-Item $target
}
else {
# Get the current Post Build Event cmd
$currentPostBuildCmd = $project.Properties.Item("PostBuildEvent").Value

# Remove our post build command from it (if it's there)
$project.Properties.Item("PostBuildEvent").Value = $currentPostBuildCmd.Replace($PostBuildScript, "")
}