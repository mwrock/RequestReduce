$psake.use_exit_on_error = $true
properties {
    $currentDir = resolve-path .
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value
    $baseDir = Split-Path -parent $Invocation.MyCommand.Definition | split-path -parent | split-path -parent | split-path -parent
	$port = "8877"
    $configuration = "debug"
	# Package Directories
	$webDir = (get-childitem (split-path c:\requestreduce) -filter mwrock.github.com).fullname
	$filesDir = "$webDir\BuildFiles"
	$version = "0.9." + (git log --pretty=oneline | measure-object).Count
}

task Debug -depends Default
task Default -depends Clean-Solution, Setup-IIS, Build-Solution, Test-Solution
task Download -depends Clean-Solution, Update-AssemblyInfoFiles, Build-Solution, Pull-Web, Build-Output, Update-Website-Download-Links, Push-Web

task Setup-IIS {
    Setup-IIS "RequestReduce" $baseDir $port
}

task Clean-Solution -depends Clean-BuildFiles {
    exec { msbuild RequestReduce.sln /t:Clean /v:quiet }
}

task echo-path {
	write-host "dir:" $webDir
}
task Update-AssemblyInfoFiles {
	$commit = git log -1 --pretty=format:%H
	Update-AssemblyInfoFiles $version $commit
}

task create-WebProject-build-target {
    create $env:MSBuildExtensionsPath32\Microsoft\VisualStudio\v10.0
	Copy-Item $baseDir\Microsoft.WebApplication.targets $env:MSBuildExtensionsPath32\Microsoft\VisualStudio\v10.0\Microsoft.WebApplication.targets
}
task Build-Solution {
    exec { msbuild RequestReduce.sln /maxcpucount /t:Build /v:Minimal /p:Configuration=$configuration }
}

task Clean-BuildFiles -depends create-WebProject-build-target {
    clean $filesDir
}

task Pull-Web {
	cd $webDir
	exec {git pull }
	cd $currentDir
}

task Push-Web {
    $message="deploying $version"
	cd $webDir
	exec { git add . }
	exec { git commit -a -m $message }
	exec { git push }
	cd $currentDir
}

task Push-Nuget {
	exec { .\Tools\nuget.exe push $filesDir\RequestReduce.$version.nupkg }
}

task Merge-Assembly -depends Build-Solution {
	clean $baseDir\RequestReduce\Nuget\Lib
	create $baseDir\RequestReduce\Nuget\Lib
	if ($env:PROCESSOR_ARCHITECTURE -eq "x64") {$bitness = "64"}
    exec { .\Tools\ilmerge.exe /t:library /internalize /targetplatform:"v4,$env:windir\Microsoft.NET\Framework$bitness\v4.0.30319" /wildcards /out:$baseDir\RequestReduce\Nuget\Lib\RequestReduce.dll $baseDir\RequestReduce\bin\$configuration\RequestReduce.dll $baseDir\RequestReduce\bin\$configuration\AjaxMin.dll $baseDir\RequestReduce\bin\$configuration\EntityFramework.dll $baseDir\RequestReduce\bin\$configuration\StructureMap.dll $baseDir\RequestReduce\bin\$configuration\nquant.core.dll }
}

task Build-Output -depends Merge-Assembly {
	clean $filesDir
	create $filesDir
	clean $baseDir\RequestReduce\Nuget\pngoptimization
	create $baseDir\RequestReduce\Nuget\pngoptimization
    $Spec = [xml](get-content "RequestReduce\Nuget\RequestReduce.nuspec")
    $Spec.package.metadata.version = $version
    $Spec.Save("RequestReduce\Nuget\RequestReduce.nuspec")
	clean $baseDir\RequestReduce\Nuget\Content\App_Readme
	create $baseDir\RequestReduce\Nuget\Content\App_Readme
	Copy-Item $baseDir\Readme.md $baseDir\RequestReduce\Nuget\Content\App_Readme\RequestReduce.readme.txt
	Copy-Item $baseDir\packages\pngoptimization\*.* $baseDir\RequestReduce\Nuget\pngoptimization\
	exec { .\Tools\zip.exe -j -9 $filesDir\RequestReduce$version.zip $baseDir\RequestReduce\Nuget\Lib\RequestReduce.dll $baseDir\RequestReduce\Nuget\Lib\RequestReduce.pdb $baseDir\RequestReduce\Nuget\pngoptimization\*.* $baseDir\License.txt $baseDir\RequestReduce\Nuget\Tools\RequestReduceFiles.sql }
    exec { .\Tools\nuget.exe pack "RequestReduce\Nuget\RequestReduce.nuspec" -o $filesDir }
}

task Update-Website-Download-Links {
	 $downloadUrl="BuildFiles/RequestReduce" + $version + ".zip"
	 $downloadButtonUrlPatern="BuildFiles/RequestReduce[0-9]+(\.([0-9]+|\*)){1,3}\.zip"
	 $downloadLinkTextPattern="V[0-9]+(\.([0-9]+|\*)){1,3}"
	 $filename = "$webDir\index.html"
     (Get-Content $filename) | % {$_ -replace $downloadButtonUrlPatern, $downloadUrl } | % {$_ -replace $downloadLinkTextPattern, ("v"+$version) } | Set-Content $filename
}

task Test-Solution {
    exec { .\packages\xunit.Runner\xunit.console.clr4.exe "RequestReduce.Facts\bin\$configuration\RequestReduce.Facts.dll" }
    exec { .\packages\xunit.Runner\xunit.console.clr4.exe "RequestReduce.Facts.Integration\bin\$configuration\RequestReduce.Facts.Integration.dll" /-trait "type=manual_adhoc" }
}

function roboexec([scriptblock]$cmd) {
    & $cmd | out-null
    if ($lastexitcode -eq 0) { throw "No files were copied for command: " + $cmd }
}

function clean($path) {
    remove-item -force -recurse $path -ErrorAction SilentlyContinue
}

function create([string[]]$paths) {
    foreach ($path in $paths) {
        if ((test-path $path) -eq $FALSE) {
            new-item -path $path -type directory | out-null
        }
    }
}

function Load-IISProvider {
    $module = Get-Module | Where-Object {$_.Name -eq "WebAdministration"}
    if($module -eq $null) {
        Import-Module WebAdministration
    }
}

function Setup-IIS([string] $siteName, [string] $solutionDir, [string] $port )
{
  try
  {
    Load-IISProvider

    # cleanup
	echo "looking for $siteName website"
    if(Test-Path IIS:\Sites\$siteName)
    {
		return
	}

    echo "Setting up $siteName website"
    $websitePhysicalPath = $solutionDir + "\RequestReduce.SampleWeb"
     
    # Create the site
    $id = (Get-ChildItem IIS:\Sites | foreach {$_.id} | sort -Descending | select -first 1) + 1
    New-Website -Name $siteName -Port $port -PhysicalPath $websitePhysicalPath -Id $id
       
    # Create app pool and have it run under network service
    $appPool = New-Item -Force IIS:\AppPools\$siteName
    $appPool.processModel.identityType = "NetworkService"
    $appPool.managedRuntimeVersion = "v4.0.30319"
    $appPool | Set-Item
        
    # Set app pool
    Set-ItemProperty IIS:\Sites\$siteName -name applicationPool -value $siteName

    #Start Site
    Start-WebItem IIS:\Sites\$siteName
 }
 catch {
   "Error in Setup-IIS: " + $_.Exception
 }
}

# Borrowed from Luis Rocha's Blog (http://www.luisrocha.net/2009/11/setting-assembly-version-with-windows.html)
function Update-AssemblyInfoFiles ([string] $version, [string] $commit) {
    $assemblyVersionPattern = 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $fileVersionPattern = 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $fileCommitPattern = 'AssemblyTrademarkAttribute\("[a-f0-9]{40}"\)'
    $assemblyVersion = 'AssemblyVersion("' + $version + '")';
    $fileVersion = 'AssemblyFileVersion("' + $version + '")';
    $commitVersion = 'AssemblyTrademarkAttribute("' + $commit + '")';

    Get-ChildItem -path $baseDir -r -filter AssemblyInfo.cs | ForEach-Object {
        $filename = $_.Directory.ToString() + '\' + $_.Name
        $filename + ' -> ' + $version
        
        # If you are using a source control that requires to check-out files before 
        # modifying them, make sure to check-out the file here.
        # For example, TFS will require the following command:
        # tf checkout $filename
    
        (Get-Content $filename) | ForEach-Object {
            % {$_ -replace $assemblyVersionPattern, $assemblyVersion } |
            % {$_ -replace $fileVersionPattern, $fileVersion } |
			% {$_ -replace $fileCommitPattern, $commitVersion }
        } | Set-Content $filename
    }
}