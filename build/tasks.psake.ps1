# SYNOPSIS: This is a psake task file.

FormatTaskName "$([string]::Concat([System.Linq.Enumerable]::Repeat('-', 70)))`r`n  {0}`r`n$([string]::Concat([System.Linq.Enumerable]::Repeat('-', 70)))";
Properties {
	$Dependencies = @("Ncrement");

    # Arguments
	$Major = $false;
	$Minor = $false;
	$Filter = $null;
	$InPreview = $false;
	$Interactive = $true;
	$InProduction = $false;
	$Configuration = "Debug";
	$EnvironmentName = $null;

	# Files & Folders
	$MSBuildExe = "";
	$ToolsFolder = "";
	$SecretsFilePath = "";
	$SolutionFolder = (Split-Path $PSScriptRoot -Parent);
	$SolutionName =   (Split-Path $SolutionFolder -Leaf);
	$ArtifactsFolder = (Join-Path $SolutionFolder "artifacts");
	$ManifestFilePath = (Join-Path $PSScriptRoot  "manifest.json");
}

Task "Default" -depends @("test", "pack");

Task "Publish" -depends @("clean", "test", "pack", "push-nuget", "tag") `
-description "This task compiles, test then publish all packages to their respective destination.";

# ======================================================================

Task "Restore-Dependencies" -alias "restore" -description "This task generate and/or import all file and module dependencies." `
-action {
	# Import powershell module dependencies
	# ==================================================
	foreach ($moduleId in $Dependencies)
	{
		$modulePath = Join-Path $ToolsFolder "$moduleId/*/*.psd1";
		if (-not (Test-Path $modulePath)) { Save-Module $moduleId -Path $ToolsFolder; }
		Import-Module $modulePath -Force;
		Write-Host "  * imported the '$moduleId-$(Split-Path (Get-Item $modulePath).DirectoryName -Leaf)' powershell module.";
	}

	# Generating the build manifest file
	# ==================================================
	if (-not (Test-Path $ManifestFilePath))
	{
		New-NcrementManifest | ConvertTo-Json | Out-File $ManifestFilePath -Encoding utf8;
		Write-Host "  * added 'build/$(Split-Path $ManifestFilePath -Leaf)' to the solution.";
	}

	# Generating a secrets file template
	# ==================================================
	if (-not (Test-Path $SecretsFilePath))
	{
		"{}" | Out-File $SecretsFilePath -Encoding utf8;
		Write-Host "  * added '$(Split-Path $SecretsFilePath -Leaf)' to the solution.";
	}
}

#region ----- PUBLISHING -----------------------------------------------

Task "Package-Solution" -alias "pack" -description "This task generates all deployment packages." `
-depends @("restore") -action {
	if (Test-Path $ArtifactsFolder) { Remove-Item $ArtifactsFolder -Recurse -Force; }
	New-Item $ArtifactsFolder -ItemType Directory | Out-Null;
	$version = $ManifestFilePath | Select-NcrementVersionNumber $EnvironmentName;

	# Create nuget package
	# ==================================================
	$project = Join-Path $SolutionFolder "src/*MSBuild/*.*proj" | Get-Item;
	$outputFolder = Join-Path $SolutionFolder "src/$SolutionName/bin/msbuild-targets";
	Write-Separator "dotnet publish '$($project.Name)'";
	Exec { &dotnet publish $project.FullName --configuration $Configuration --output $outputFolder; }

	$project = Join-Path $SolutionFolder "src/$SolutionName/*.*proj" | Get-Item;
	Write-Separator "dotnet pack '$($project.Name)'";
	Exec { &dotnet pack $project.FullName --output $ArtifactsFolder --configuration $Configuration -p:"Version=$version"; }

	Remove-Item $outputFolder -Recurse -Force;

	# Expand nuget package for debugging
	# ==================================================
	[string]$nupkg = Join-Path $ArtifactsFolder "*.nupkg" | Resolve-Path;
	$zip = [IO.Path]::ChangeExtension($nupkg, ".zip");
	Copy-Item $nupkg -Destination $zip;
	Expand-Archive $zip -DestinationPath "$ArtifactsFolder/msbuild";
	Remove-Item $zip -Force;

	# Create vsix package
	# ==================================================
	$project = Join-Path $SolutionFolder "src/*VSIX/*.*proj" | Get-Item;
	Write-Separator "msbuild '$($project.Name)'";
	Exec { &$MSBuildExe $project.FullName -property:"Configuration=$Configuration" -restore; }
	Join-Path $SolutionFolder "src/*VSIX/bin/$Configuration/*.vsix" | Get-Item | Copy-Item -Destination $ArtifactsFolder;
}

Task "Publish-NuGet-Packages" -alias "push-nuget" -description "This task publish all nuget packages to a nuget repository." `
-precondition { return ($InProduction -or $InPreview ) -and (Test-Path $ArtifactsFolder -PathType Container) } `
-action {
	Get-ChildItem $ArtifactsFolder -Filter "*.nupkg";
}

Task "Publish-VSIX-Package" -alias "push-vsix" -description "This task publish all VSIX packages to https://marketplace.visualstudio.com/" `
-precondition { return (Test-Path $ArtifactsFolder -PathType Container) } `
-action {
	[string]$vsixPublisher = Join-Path "$($env:ProgramFiles)*" "Microsoft Visual Studio\*\*\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe" | Resolve-Path -ErrorAction Stop; 
	$package = Join-Path $ArtifactsFolder "*.vsix" | Get-Item;
	$manifest = Join-Path $PSScriptRoot "publishing/visual-studio-marketplace.json" | Get-Item;
	$pat = Get-Secret "VISUAL_STUDIO_MARKETPLACE_PAT" "vsixMarketplace";

	Write-Separator "VsixPublish publish -payload '$($package.Name)'";
	Exec { &$vsixPublisher publish -payload $package.FullName -publishManifest $manifest.FullName -personalAccessToken $pat -ignoreWarnings "VSIXValidatorWarning01,VSIXValidatorWarning02"; }
}

Task "Add-GitReleaseTag" -alias "tag" -description "This task tags the lastest commit with the version number." `
-precondition { return ($InProduction -or $InPreview ) } `
-depends @("restore") -action { 
	$version = $ManifestFilePath | Select-NcrementVersionNumber $EnvironmentName -Format "C";
	
	if (-not ((&git status | Out-String) -match 'nothing to commit'))
	{
		Write-Separator "git commit";
		Exec { &git add .; }
		Exec { &git commit -m "Increment version number to '$version'."; }
	}

	Write-Separator "git tag '$version'";
	Exec { &git -annotate "v$version" --message "Version $version"; }
}

#endregion

#region ----- COMPILATION ----------------------------------------------

Task "Clean" -description "This task removes all generated files and folders from the solution." `
-action {
	foreach ($itemsToRemove in @("artifacts", "TestResults", "*/*/bin/", "*/*/obj/", "*/*/node_modules/", "*/*/package-lock.json"))
	{
		$itemPath = Join-Path $SolutionFolder $itemsToRemove;
		if (Test-Path $itemPath)
		{
			Resolve-Path $itemPath `
				| Write-Value "  * removed '{0}'." -PassThru `
					| Remove-Item -Recurse -Force;
		}
	}
}

Task "Increment-Version-Number" -alias "version" -description "This task increments all of the projects version number." `
-depends @("restore") -action {
	$manifest = $ManifestFilePath | Step-NcrementVersionNumber -Major:$Major -Minor:$Minor -Patch | Edit-NcrementManifest $ManifestFilePath;
	$newVersion = $ManifestFilePath | Select-NcrementVersionNumber $EnvironmentName;

	foreach ($item in @("*/*/*.*proj", "src/*/*.vsixmanifest", "src/*/*.psd1"))
	{
		$itemPath = Join-Path $SolutionFolder $item;
		if (Test-Path $itemPath)
		{
			Get-ChildItem $itemPath | Update-NcrementProjectFile $ManifestFilePath `
				| Write-Value "  * incremented '{0}' version number to '$newVersion'.";
		}
	}
}

Task "Build-Solution" -alias "compile" -description "This task compiles projects in the solution." `
-action {
	$solutionFile = Join-Path $SolutionFolder "*.sln" | Get-Item;
	Write-Separator "msbuild '$($solutionFile.Name)'";
	Exec { &$MSBuildExe $solutionFile.FullName -property:Configuration=$Configuration -restore ; }
}

Task "Run-Tests" -alias "test" -description "This task invoke all tests within the 'tests' folder." `
-action {
	foreach ($item in @("tests/*MSTest/*.*proj"))
	{
		[string]$projectPath = Join-Path $SolutionFolder $item;
		if (Test-Path $projectPath -PathType Leaf)
		{
			$projectPath = Resolve-Path $projectPath;
			Write-Separator "dotnet test '$(Split-Path $projectPath -Leaf)'";
			Exec { &dotnet test $projectPath --configuration $Configuration; }
		}
	}
}

#endregion

#region ----- FUNCTIONS ----------------------------------------------

function Write-Value
{
	Param(
		[Parameter(Mandatory)]
		[string]$FormatString,

		$Arg1, $Arg2,

		[Alias('c', "fg")]
		[System.ConsoleColor]$ForegroundColor = [System.ConsoleColor]::Gray,

		[Parameter(ValueFromPipeline)]
		$InputObject,

		[switch]$PassThru
	)

	PROCESS
	{
		Write-Host ([string]::Format($FormatString, $InputObject, $Arg1, $Arg2)) -ForegroundColor $ForegroundColor;
		if ($PassThru -and $InputObject) { return $InputObject }
	}
}

function Write-Separator ([string]$Title = "", [int]$length = 70)
{
	$header = [string]::Concat([System.Linq.Enumerable]::Repeat('-', $length));
	if (-not [String]::IsNullOrEmpty($Title))
	{
		$header = $header.Insert(4, " $Title ");
		if ($header.Length -gt $length) { $header = $header.Substring(0, $length); }
	}
	Write-Host "`r`n$header`r`n" -ForegroundColor DarkGray;
}

function Get-Secret
{
	Param(
		[Parameter(Mandatory)]
		[string]$EnvironmentVariable,
		[Parameter(Mandatory)]
		[string]$JPath
	)

	$result = [Environment]::ExpandEnvironmentVariables("%$EnvironmentVariable%");
	if ([string]::IsNullOrEmpty($result) -or ($result -eq "%$EnvironmentVariable%"))
	{
		$secrets = Get-Content $SecretsFilePath | ConvertFrom-Json;
		$properties = $JPath.Split(@('.', '/'));
		foreach($prop in $properties)
		{
			$result = $secrets.$prop;
		}
	}
	return $result;
}


#endregion
