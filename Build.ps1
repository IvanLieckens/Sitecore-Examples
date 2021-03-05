[CmdletBinding(DefaultParameterSetName = "no-arguments")]
Param (
    [Parameter(HelpMessage = "Name of the solution.")]
    [string]$SolutionName = "Examples",

    [Parameter(HelpMessage = "Configuration to build in.")]
    [ValidateSet('Debug','Release')]
    [string]$Configuration = "Debug",

    [Parameter(HelpMessage = "Pubilsh Profile to build.")]
    [string]$PublishProfile = "docker",

    [Parameter(HelpMessage = "Whether you desire to build the solution or the containers as well.")]
    [Switch]$IncludeContainers
)

$output = New-Object System.Text.StringBuilder
$vsWhere = "${Env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (-Not (Test-Path $vsWhere -PathType Leaf))
{
	throw "No Visual Studio present"
}

& "$vsWhere" -latest -format json 2>&1 |
	ForEach-Object {
		if ($_ -is [System.Management.Automation.ErrorRecord]) {
			Write-Verbose "STDERR: $($_.Exception.Message)"
		}
		else {
			Write-Verbose $_
			$null = $output.AppendLine($_)
		}
	}
$vsInstance = (ConvertFrom-Json -InputObject $output.ToString()) | Select-Object -First 1
$msBuildExe = Join-Path $vsInstance.installationPath "\MSBuild\Current\Bin\msbuild.exe"
& "$msBuildExe" ".\$SolutionName.sln" /t:Clean
& "$msBuildExe" ".\$SolutionName.sln" /t:Build /p:platform=`"Any CPU`" /p:configuration=$Configuration /p:DeployOnBuild=true /p:PublishProfile=$PublishProfile /p:SkipInvalidConfigurations=true

if ($IncludeContainers) {
    docker-compose --env-file .\.env.user build
}