$psake.use_exit_on_error = $true
properties {
    $baseDir = resolve-path .
	$port = "8877"
    $configuration = "debug"
	# Package Directories
	$filesDir = "$baseDir\BuildFiles"
	if ( -not ( Test-Path ENV:\NugetOutput )) {
		$nugetDir = "$baseDir\NugetPkg"
	}
	else {
		$nugetDir = $env:NugetOutput
	}
}

task Default -depends Clean-Solution, Setup-IIS, Build-Solution, Test-Solution, Build-Nuget

task Setup-IIS {
    Setup-IIS "RequestReduce" $baseDir $port
}

task Clean-Solution -depends Clean-BuildFiles {
    exec { msbuild RequestReduce.sln /t:Clean /v:quiet }
}

task Build-Solution {
    exec { msbuild RequestReduce.sln /maxcpucount /t:Build /v:Minimal /p:Configuration=$configuration }
}

task Clean-BuildFiles {
    clean $filesDir
}

task Build-Nuget {
	clean $baseDir\RequestReduce\Nuget\Lib
	create $baseDir\RequestReduce\Nuget\Lib
	if ($env:PROCESSOR_ARCHITECTURE -eq "x64") {$bitness = "64"}
    exec { .\Tools\ilmerge.exe /t:library /internalize /targetplatform:"v4,$env:windir\Microsoft.NET\Framework$bitness\v4.0.30319" /wildcards /out:$baseDir\RequestReduce\Nuget\Lib\RequestReduce.dll $baseDir\RequestReduce\bin\$configuration\RequestReduce.dll $baseDir\RequestReduce\bin\$configuration\AjaxMin.dll $baseDir\RequestReduce\bin\$configuration\EntityFramework.dll $baseDir\RequestReduce\bin\$configuration\StructureMap.dll }
	create $nugetDir
    exec { .\Tools\nuget.exe pack "RequestReduce\Nuget\RequestReduce.nuspec" -o $nugetDir }
}


task Test-Solution {
    exec { .\packages\xunit.Runner\xunit.console.clr4.exe "RequestReduce.Facts\bin\$configuration\RequestReduce.Facts.dll" }
    exec { .\packages\xunit.Runner\xunit.console.clr4.exe "RequestReduce.Facts.Integration\bin\$configuration\RequestReduce.Facts.Integration.dll" }
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