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
	$version = "1.3." + (git log v1.3.. --pretty=oneline | measure-object).Count
	$projectFiles = "$baseDir\RequestReduce\RequestReduce.csproj"
}

task Debug -depends Default
task Default -depends Setup-40-Projects, Clean-Solution, Setup-IIS, Build-Solution, Reset, Test-Solution
task BuildNet35 -depends Setup-35-Projects, Clean-35-Solution, Setup-IIS, Build-35-Solution, Reset
task Download35 -depends Setup-35-Projects, Clean-35-Solution, Update-AssemblyInfoFiles, Build-35-Solution, Reset
task Download -depends Pull-Repo, Setup-40-Projects, Pull-Web, Clean-Solution, Update-AssemblyInfoFiles, Build-Solution, Build-Output, Push-Repo, Update-Website-Download-Links, Push-Web
task Push-Nuget-All -depends Push-Nuget-Core, Push-Nuget-SqlServer

task Reset {
  Change-Framework-Version $projectFiles '4.0' $true
}

task Setup-IIS {
    Setup-IIS "RequestReduce" $baseDir $port
}

task Setup-35-Projects {
  Change-Framework-Version $projectFiles '3.5' $false
}

task Setup-40-Projects {
  Change-Framework-Version $projectFiles '4.0' $false
}

task Clean-35-Solution -depends Clean-BuildFiles {
    exec { msbuild 'RequestReduce\RequestReduce.csproj' /t:Clean /v:quiet }
}

task Clean-Solution -depends Clean-BuildFiles {
    clean $baseDir\RequestReduce\Nuget\Lib
	create $baseDir\RequestReduce\Nuget\Lib
    exec { msbuild RequestReduce.sln /t:Clean /v:quiet }
}

task echo-path {
	write-host "dir:" $webDir
}
task Update-AssemblyInfoFiles {
	$commit = git log -1 v1.0.. --pretty=format:%H
	Update-AssemblyInfoFiles $version $commit
}

task create-WebProject-build-target {
  create $env:MSBuildExtensionsPath32\Microsoft\VisualStudio\v10.0
	Copy-Item $baseDir\Microsoft.WebApplication.targets $env:MSBuildExtensionsPath32\Microsoft\VisualStudio\v10.0\Microsoft.WebApplication.targets
}

task Build-35-Solution {
  exec { msbuild 'RequestReduce\RequestReduce.csproj' /maxcpucount /t:Build /v:Minimal /p:Configuration=$configuration }
}

task Build-Solution {
    Change-Framework-Version $projectFiles '4.0' $false
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

task Pull-Repo {
	exec {git pull }
}

task Push-Web {
    $message="deploying $version"
	cd $webDir
	exec { git add . }
	exec { git commit -a -m $message }
	exec { git push }
	cd $currentDir
}

task Push-Repo {
    $message="deploying $version"
	exec { git add . }
	exec { git commit -a -m $message }
	exec { git push }
}

task Push-Nuget-Core {
	$pkg = Get-Item -path $filesDir/RequestReduce.*.*.*.nupkg
	exec { .\Tools\nuget.exe push $filesDir\$($pkg.Name) }
}

task Push-Nuget-SqlServer {
	$pkg = Get-Item -path $filesDir/RequestReduce.SqlServer.*.*.*.nupkg
	exec { .\Tools\nuget.exe push $filesDir\$($pkg.Name) }
}

task Merge-35-Assembly {
  clean $baseDir\RequestReduce\Nuget\Lib\net20
  create $baseDir\RequestReduce\Nuget\Lib\net20
  if ($env:PROCESSOR_ARCHITECTURE -eq "x64") {$bitness = "64"}
  exec { .\Tools\ilmerge.exe /t:library /internalize /targetplatform:"v2,$env:windir\Microsoft.NET\Framework$bitness\v2.0.50727" /wildcards /out:$baseDir\RequestReduce\Nuget\Lib\net20\RequestReduce.dll "$baseDir\RequestReduce\bin\v3.5\$configuration\RequestReduce.dll" "$baseDir\RequestReduce\bin\v3.5\$configuration\AjaxMin.dll" "$baseDir\RequestReduce\bin\v3.5\$configuration\StructureMap.dll" "$baseDir\RequestReduce\bin\v3.5\$configuration\nquant.core.dll" }
}

task Merge-40-Assembly -depends Build-Solution {
	clean $baseDir\RequestReduce\Nuget\Lib\net40
	clean $baseDir\RequestReduce.SqlServer\Nuget\Lib\net40
	create $baseDir\RequestReduce\Nuget\Lib\net40
	create $baseDir\RequestReduce.SqlServer\Nuget\Lib\net40
	if ($env:PROCESSOR_ARCHITECTURE -eq "x64") {$bitness = "64"}
    exec { .\Tools\ilmerge.exe /t:library /internalize /targetplatform:"v4,$env:windir\Microsoft.NET\Framework$bitness\v4.0.30319" /wildcards /out:$baseDir\RequestReduce\Nuget\Lib\net40\RequestReduce.dll "$baseDir\RequestReduce\bin\v4.0\$configuration\RequestReduce.dll" "$baseDir\RequestReduce\bin\v4.0\$configuration\AjaxMin.dll" "$baseDir\RequestReduce\bin\v4.0\$configuration\StructureMap.dll" "$baseDir\RequestReduce\bin\v4.0\$configuration\nquant.core.dll" }
}

task Build-Output -depends Merge-35-Assembly, Merge-40-Assembly {
	clean $filesDir
	clean $baseDir\RequestReduce\Nuget\pngoptimization
	create $baseDir\RequestReduce\Nuget\pngoptimization
  $Spec = [xml](get-content "RequestReduce\Nuget\RequestReduce.nuspec")
  $Spec.package.metadata.version = $version
  $Spec.Save("RequestReduce\Nuget\RequestReduce.nuspec")
  $Spec = [xml](get-content "RequestReduce.SqlServer\Nuget\RequestReduce.SqlServer.nuspec")
  $Spec.package.metadata.version = $version
  $Spec.package.metadata.dependencies.dependency[0].SetAttribute("version", $version)
  $Spec.Save("RequestReduce.SqlServer\Nuget\RequestReduce.SqlServer.nuspec")
  clean $baseDir\RequestReduce\Nuget\Content\App_Readme
  create $baseDir\RequestReduce\Nuget\Content\App_Readme
  Copy-Item $baseDir\RequestReduce.SqlServer\bin\$configuration\RequestReduce.SqlServer.* $baseDir\RequestReduce.SqlServer\Nuget\lib\net40\
  Copy-Item $baseDir\Readme.md $baseDir\RequestReduce\Nuget\Content\App_Readme\RequestReduce.readme.txt
  Copy-Item $baseDir\packages\pngoptimization\*.* $baseDir\RequestReduce\Nuget\pngoptimization\
  create $filesDir\net35
  create $filesDir\net40
  Copy-Item $baseDir\requestreduce\nuget\lib\net20\*.* $filesDir\net35
  Copy-Item $baseDir\requestreduce\nuget\lib\net40\*.* $filesDir\net40
  Copy-Item $baseDir\License.txt $filesDir
  Copy-Item $baseDir\RequestReduce\Nuget\pngoptimization\*.txt $filesDir
  Copy-Item $baseDir\Readme.md $filesDir\RequestReduce.readme.txt
  Copy-Item $baseDir\RequestReduce\Nuget\pngoptimization\*.dll $filesDir\net35
  Copy-Item $baseDir\RequestReduce\Nuget\pngoptimization\*.dll $filesDir\net40
  create $filesDir\RequestReduce.SqlServer
  Copy-Item $baseDir\requestreduce.SqlServer\bin\$configuration\Entityframework.* $filesDir\RequestReduce.SqlServer
  Copy-Item $baseDir\requestreduce.SqlServer\nuget\lib\net40\*.* $filesDir\RequestReduce.SqlServer
  Copy-Item $baseDir\requestreduce.SqlServer\nuget\tools\*.* $filesDir\RequestReduce.SqlServer
  cd $filesDir
  exec { & $baseDir\Tools\zip.exe -9 -r RequestReduce-$version.zip . }
  cd $currentDir
  exec { .\Tools\nuget.exe pack "RequestReduce\Nuget\RequestReduce.nuspec" -o $filesDir }
  exec { .\Tools\nuget.exe pack "RequestReduce.SqlServer\Nuget\RequestReduce.SqlServer.nuspec" -o $filesDir }
}

task Update-Website-Download-Links {
	 $downloadUrl="BuildFiles/RequestReduce-" + $version + ".zip"
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

    Get-ChildItem -path $baseDir -r -filter GlobalAssemblyInfo.cs | ForEach-Object {
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

function Change-Framework-Version ([string[]] $projFiles, [string] $frameworkVersion, [boolean] $setDefaultPath) {
  $nQuantRegex = [regex] '<HintPath>\.\.\\packages\\nQuant.+\\Lib\\(net\d{2})\\nQuant\.Core\.dll</HintPath>'
  if ($frameworkVersion -eq '4.0') { $nugetVer = 'net40' } else { $nugetVer = 'net20' }

	foreach ($projFile in $projFiles) {	
		$content = [xml] (get-content $projFile)
		$content.Project.SetAttribute("ToolsVersion", $frameworkVersion)
		$content.Project.PropertyGroup[0].TargetFrameworkVersion = "v$frameworkVersion"
		if ($setDefaultPath) {
      $paths = 'bin\Debug\', 'bin\Release\'
		} else {
      $paths = "bin\v$frameworkVersion\Debug\", "bin\v$frameworkVersion\Release\"
    }
    
    $content.Project.PropertyGroup[1].OutputPath = $paths[0]
    $content.Project.PropertyGroup[2].OutputPath = $paths[1]
    
    if ($frameworkVersion -eq '4.0') {
      $ref = $content.Project.ItemGroup[0].Reference | where-object { $_.GetAttribute('Include') -eq 'System.Web.Abstractions' }
      if ($ref -ne $null) {
        $content.Project.ItemGroup[0].RemoveChild($ref) | out-null
      }
    } else {
      $ref = $content.CreateElement('Reference', 'http://schemas.microsoft.com/developer/msbuild/2003')
      $ref.SetAttribute('Include', 'System.Web.Abstractions')
      $content.Project.ItemGroup[0].AppendChild($ref) | out-null
    }
    
		$content.Save($projFile)
		
		$content = [System.IO.File]::ReadAllText($projFile)
		$match = $nQuantRegex.Match($content)
		$content = $content.Remove($match.Groups[1].Index, $match.Groups[1].Length)
		$content = $content.Insert($match.Groups[1].Index, $nugetVer)
		set-content $projFile $content
	}
}