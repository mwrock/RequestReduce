$psake.use_exit_on_error = $true
properties {
    $baseDir = resolve-path .
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

task Default -depends Clean-Solution, Build-Solution, Test-Solution, Build-Nuget

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
    exec { .\Tools\ilmerge.exe /t:library /internalize /targetplatform:"v4,$env:windir\Microsoft.NET\Framework64\v4.0.30319" /wildcards /out:$baseDir\RequestReduce\Nuget\Lib\RequestReduce.dll $baseDir\RequestReduce\bin\$configuration\*.dll }
	create $nugetDir
    exec { .\Tools\nuget.exe p "RequestReduce\Nuget\RequestReduce.nuspec" -o $nugetDir }
}


task Test-Solution {
    exec { .\packages\xunit.1.7.0.1540\Tools\xunit.console.clr4.exe "RequestReduce.Facts\bin\$configuration\RequestReduce.Facts.dll" }
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