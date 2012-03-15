$psake.use_exit_on_error = $true
properties {
  $currentDir = resolve-path .
  $Invocation = (Get-Variable MyInvocation -Scope 1).Value
  $baseDir = Split-Path -parent $Invocation.MyCommand.Definition | split-path -parent | split-path -parent | split-path -parent
  echo $baseDir
  $port = "8877"
  $configuration = "debug"
	# Package Directories
	$webDir = (get-childitem (split-path c:\requestreduce) -filter mwrock.github.com).fullname
	$filesDir = "$baseDir\BuildFiles"
	$version = git describe --abbrev=0 --tags
	$version = $version.substring(1) + '.' + (git log $($version + '..') --pretty=oneline | measure-object).Count
	$projectFiles = "$baseDir\RequestReduce\RequestReduce.csproj"
	$nugetDir = "$baseDir\.NuGet"
}

task Test-Solution -depends Unit-Tests, Integration-Tests
task Debug -depends Default
task Default -depends Clean-Solution, Setup-IIS, Build-35-Solution, Build-Solution, Test-Solution
task Download -depends Pull-Repo, Pull-Web, Clean-Solution, Update-AssemblyInfoFiles, Build-Output, Push-Repo, Update-Website-Download-Links, Push-Web
task private-download -depends Pull-Repo, Clean-Solution, Update-AssemblyInfoFiles, Build-Output, Push-Repo
task Push-Nuget-All -depends Push-Nuget-Core, Push-Nuget-SqlServer, Push-Nuget-SassLessCoffee

task Setup-IIS {
    Setup-IIS "RequestReduce" $baseDir $port
}

task Clean-Solution -depends Clean-BuildFiles {
	$conf = $configuration+35
    clean $baseDir\RequestReduce\Nuget\Lib
	create $baseDir\RequestReduce\Nuget\Lib
    exec { msbuild RequestReduce.sln /t:Clean /v:quiet }
	exec { msbuild 'RequestReduce\RequestReduce.csproj' /t:Clean /v:quiet /p:Configuration=$conf }
	exec { msbuild 'RequestReduce.SqlServer\RequestReduce.SqlServer.csproj' /t:Clean /v:quiet /p:Configuration=$conf }
}	

task echo-path {
	write-host "dir:" $webDir
}

task Update-AssemblyInfoFiles {
	$v = git describe --abbrev=0 --tags
	$commit = git log -1 $($v + '..') --pretty=format:%H
	Update-AssemblyInfoFiles $version $commit
}

task create-WebProject-build-target {
  create $env:MSBuildExtensionsPath32\Microsoft\VisualStudio\v10.0
	Copy-Item $baseDir\Microsoft.WebApplication.targets $env:MSBuildExtensionsPath32\Microsoft\VisualStudio\v10.0\Microsoft.WebApplication.targets
}

task Build-35-Solution {
  $conf = $configuration+35
  exec { msbuild 'RequestReduce\RequestReduce.csproj' /maxcpucount /t:Build /v:Minimal /p:Configuration=$conf }
  exec { msbuild 'RequestReduce.SqlServer\RequestReduce.SqlServer.csproj' /maxcpucount /t:Build /v:Minimal /p:Configuration=$conf }
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
	$pkg = Get-Item -path $filesDir/RequestReduce.1.*.*.nupkg
	exec { .$nugetDir\nuget.exe push $filesDir\$($pkg.Name) }
}

task Push-Nuget-SqlServer {
	$pkg = Get-Item -path $filesDir/RequestReduce.SqlServer.*.*.*.nupkg
	exec { .$nugetDir\nuget.exe push $filesDir\$($pkg.Name) }
}

task Push-Nuget-SassLessCoffee {
	$pkg = Get-Item -path $filesDir/RequestReduce.SassLessCoffee.*.*.*.nupkg
	exec { .$nugetDir\nuget.exe push $filesDir\$($pkg.Name) }
}

task Merge-35-Assembly -depends Build-35-Solution {
  clean $baseDir\RequestReduce\Nuget\Lib\net20
  clean $baseDir\RequestReduce.SqlServer\Nuget\Lib\net20
  create $baseDir\RequestReduce\Nuget\Lib\net20
  create $baseDir\RequestReduce.SqlServer\Nuget\Lib\net20
  if ($env:PROCESSOR_ARCHITECTURE -eq "x64") {$bitness = "64"}
  exec { .\Tools\ilmerge.exe /t:library /internalize /targetplatform:"v2,$env:windir\Microsoft.NET\Framework$bitness\v2.0.50727" /wildcards /out:$baseDir\RequestReduce\Nuget\Lib\net20\RequestReduce.dll "$baseDir\RequestReduce\bin\v3.5\$configuration\RequestReduce.dll" "$baseDir\RequestReduce\bin\v3.5\$configuration\AjaxMin.dll" "$baseDir\RequestReduce\bin\v3.5\$configuration\StructureMap.dll" "$baseDir\RequestReduce\bin\v3.5\$configuration\nquant.core.dll" }
}

task Merge-40-Assembly -depends Build-Solution {
	clean $baseDir\RequestReduce\Nuget\Lib\net40
	clean $baseDir\RequestReduce.SqlServer\Nuget\Lib\net40
	clean $baseDir\RequestReduce.SassLessCoffee\Nuget\Lib\net40
	create $baseDir\RequestReduce\Nuget\Lib\net40
	create $baseDir\RequestReduce.SqlServer\Nuget\Lib\net40
	create $baseDir\RequestReduce.SassLessCoffee\Nuget\Lib\net40
	if ($env:PROCESSOR_ARCHITECTURE -eq "AMD64") {
		if( Test-Path "${env:ProgramFiles(x86)}\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0" ) {
			exec { .\Tools\ilmerge.exe /t:library /internalize /targetplatform:"v4,${env:ProgramFiles(x86)}\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0" /wildcards /out:$baseDir\RequestReduce\Nuget\Lib\net40\RequestReduce.dll "$baseDir\RequestReduce\bin\v4.0\$configuration\RequestReduce.dll" "$baseDir\RequestReduce\bin\v4.0\$configuration\AjaxMin.dll" "$baseDir\RequestReduce\bin\v4.0\$configuration\StructureMap.dll" "$baseDir\RequestReduce\bin\v4.0\$configuration\nquant.core.dll" }
		}
		else {
			exec { .\Tools\ilmerge.exe /t:library /internalize /targetplatform:"v4,$env:windir\Microsoft.NET\Framework64\v4.0.30319" /wildcards /out:$baseDir\RequestReduce\Nuget\Lib\net40\RequestReduce.dll "$baseDir\RequestReduce\bin\v4.0\$configuration\RequestReduce.dll" "$baseDir\RequestReduce\bin\v4.0\$configuration\AjaxMin.dll" "$baseDir\RequestReduce\bin\v4.0\$configuration\StructureMap.dll" "$baseDir\RequestReduce\bin\v4.0\$configuration\nquant.core.dll" }
		}
	}
	else {
		if( Test-Path "$env:ProgramFiles\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0" ) {
			exec { .\Tools\ilmerge.exe /t:library /internalize /targetplatform:"v4,$env:ProgramFiles\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0" /wildcards /out:$baseDir\RequestReduce\Nuget\Lib\net40\RequestReduce.dll "$baseDir\RequestReduce\bin\v4.0\$configuration\RequestReduce.dll" "$baseDir\RequestReduce\bin\v4.0\$configuration\AjaxMin.dll" "$baseDir\RequestReduce\bin\v4.0\$configuration\StructureMap.dll" "$baseDir\RequestReduce\bin\v4.0\$configuration\nquant.core.dll" }
		}
		else {
			exec { .\Tools\ilmerge.exe /t:library /internalize /targetplatform:"v4,$env:windir\Microsoft.NET\Framework\v4.0.30319" /wildcards /out:$baseDir\RequestReduce\Nuget\Lib\net40\RequestReduce.dll "$baseDir\RequestReduce\bin\v4.0\$configuration\RequestReduce.dll" "$baseDir\RequestReduce\bin\v4.0\$configuration\AjaxMin.dll" "$baseDir\RequestReduce\bin\v4.0\$configuration\StructureMap.dll" "$baseDir\RequestReduce\bin\v4.0\$configuration\nquant.core.dll" }
		}
	}
}

task Build-Output -depends Merge-35-Assembly, Merge-40-Assembly {
  clean $filesDir
  clean $baseDir\RequestReduce\Nuget\pngoptimization
  create $baseDir\RequestReduce\Nuget\pngoptimization
  clean $baseDir\RequestReduce\Nuget\Content\App_Readme
  create $baseDir\RequestReduce\Nuget\Content\App_Readme
  Copy-Item $baseDir\RequestReduce.SqlServer\bin\v4.0\$configuration\RequestReduce.SqlServer.* $baseDir\RequestReduce.SqlServer\Nuget\lib\net40\
  Copy-Item $baseDir\RequestReduce.SqlServer\bin\v3.5\$configuration\RequestReduce.SqlServer.* $baseDir\RequestReduce.SqlServer\Nuget\lib\net20\
  Copy-Item $baseDir\RequestReduce.SassLessCoffee\bin\$configuration\RequestReduce.SassLessCoffee.* $baseDir\RequestReduce.SassLessCoffee\Nuget\lib\net40\
  Copy-Item $baseDir\Readme.md $baseDir\RequestReduce\Nuget\Content\App_Readme\RequestReduce.readme.txt
  Copy-Item $baseDir\ExternalBinaries\pngoptimization\*.* $baseDir\RequestReduce\Nuget\pngoptimization\
  create $filesDir\net35
  create $filesDir\net40
  Copy-Item $baseDir\requestreduce\nuget\lib\net20\*.* $filesDir\net35
  Copy-Item $baseDir\requestreduce\nuget\lib\net40\*.* $filesDir\net40
  Copy-Item $baseDir\License.txt $filesDir
  Copy-Item $baseDir\RequestReduce\Nuget\pngoptimization\*.txt $filesDir
  Copy-Item $baseDir\Readme.md $filesDir\RequestReduce.readme.txt
  Copy-Item $baseDir\RequestReduce\Nuget\pngoptimization\*.exe $filesDir\net35
  Copy-Item $baseDir\RequestReduce\Nuget\pngoptimization\*.exe $filesDir\net40
  create $filesDir\RequestReduce.SqlServer
  create $filesDir\RequestReduce.SqlServer\net35
  create $filesDir\RequestReduce.SqlServer\net40
  Copy-Item $baseDir\requestreduce.SqlServer\nuget\lib\net20\*.* $filesDir\RequestReduce.SqlServer\net35
  Copy-Item $baseDir\requestreduce.SqlServer\nuget\lib\net40\*.* $filesDir\RequestReduce.SqlServer\net40
  Copy-Item $baseDir\requestreduce.SqlServer\nuget\tools\*.* $filesDir\RequestReduce.SqlServer
  create $filesDir\RequestReduce.SassLessCoffee
  Copy-Item $baseDir\requestreduce.SassLessCoffee\bin\$configuration\dotless.core.* $filesDir\RequestReduce.SassLessCoffee
  Copy-Item $baseDir\requestreduce.SassLessCoffee\bin\$configuration\IronRuby.* $filesDir\RequestReduce.SassLessCoffee
  Copy-Item $baseDir\requestreduce.SassLessCoffee\bin\$configuration\Jurassic.* $filesDir\RequestReduce.SassLessCoffee
  Copy-Item $baseDir\requestreduce.SassLessCoffee\bin\$configuration\Microsoft.* $filesDir\RequestReduce.SassLessCoffee
  Copy-Item $baseDir\requestreduce.SassLessCoffee\bin\$configuration\SassAndCoffee.Core.* $filesDir\RequestReduce.SassLessCoffee
  Copy-Item $baseDir\requestreduce.SassLessCoffee\bin\$configuration\V8Bridge.Interface.* $filesDir\RequestReduce.SassLessCoffee
  Copy-Item $baseDir\requestreduce.SassLessCoffee\nuget\lib\net40\*.* $filesDir\RequestReduce.SassLessCoffee
  cd $filesDir
  exec { & $baseDir\Tools\zip.exe -9 -r RequestReduce-$version.zip . }
  cd $currentDir
  exec { .$nugetDir\nuget.exe pack "RequestReduce\Nuget\RequestReduce.nuspec" -o $filesDir -version $version }
  exec { .$nugetDir\nuget.exe pack "RequestReduce.SqlServer\Nuget\RequestReduce.SqlServer.nuspec" -o $filesDir -version $version }
  exec { .$nugetDir\nuget.exe pack "RequestReduce.SassLessCoffee\Nuget\RequestReduce.SassLessCoffee.nuspec" -o $filesDir -version $version }
}

task Update-Website-Download-Links {
	 Copy-Item $filesDir\RequestReduce-$version.zip $webDir
	 $downloadUrl="RequestReduce-" + $version + ".zip"
	 $downloadButtonUrlPatern="RequestReduce-[0-9]+(\.([0-9]+|\*)){1,3}\.zip"
	 $downloadLinkTextPattern="V[0-9]+(\.([0-9]+|\*)){1,3}"
	 $filename = "$webDir\index.html"
     (Get-Content $filename) | % {$_ -replace $downloadButtonUrlPatern, $downloadUrl } | % {$_ -replace $downloadLinkTextPattern, ("v"+$version) } | Set-Content $filename
}

task Unit-Tests -depends Build-Solution {
    exec { .\ExternalBinaries\xunit.Runner\xunit.console.clr4.exe "RequestReduce.Facts\bin\$configuration\RequestReduce.Facts.dll" }
}

task Integration-Tests -depends Build-Solution {
    exec {.\ExternalBinaries\xunit.Runner\xunit.console.clr4.exe "RequestReduce.Facts.Integration\bin\$configuration\RequestReduce.Facts.Integration.dll" /-trait "type=manual_adhoc" }
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
	$siteName35 = $sitename+35
    $websitePhysicalPath = $solutionDir + "\RequestReduce.SampleWeb"
     
    # Create the site
    $id = (Get-ChildItem IIS:\Sites | foreach {$_.id} | sort -Descending | select -first 1) + 1
    New-Website -Name $siteName -Port $port -PhysicalPath $websitePhysicalPath -Id $id
    New-Website -Name $siteName35 -Port ([int]$port+1) -PhysicalPath ($websitePhysicalPath+35) -Id ($id+1)

    # Create app pool and have it run under network service
    $appPool = New-Item -Force IIS:\AppPools\$siteName
    $appPool.processModel.identityType = "NetworkService"
    $appPool.managedRuntimeVersion = "v4.0.30319"
    $appPool | Set-Item
      
    $appPool = New-Item -Force IIS:\AppPools\$siteName35
    $appPool.processModel.identityType = "NetworkService"
    $appPool.managedRuntimeVersion = "v2.0"
    $appPool | Set-Item
    # Set app pool
    Set-ItemProperty IIS:\Sites\$siteName -name applicationPool -value $siteName
    Set-ItemProperty IIS:\Sites\$siteName35 -name applicationPool -value $siteName35

	New-WebApplication -Name styles\secure -Site $siteName -PhysicalPath $websitePhysicalPath\Styles\secure -ApplicationPool $siteName

    #Start Site
    Start-WebItem IIS:\Sites\$siteName
    Start-WebItem IIS:\Sites\$siteName35
 }
 catch {
   "Error in Setup-IIS: " + $_.Exception
 }
}

# Borrowed from Luis Rocha's Blog (http://www.luisrocha.net/2009/11/setting-assembly-version-with-windows.html)
function Update-AssemblyInfoFiles ([string] $version, [string] $commit) {
    $assemblyVersionPattern = 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $fileVersionPattern = 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $fileCommitPattern = 'AssemblyTrademarkAttribute\("([a-f0-9]{40})?"\)'
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