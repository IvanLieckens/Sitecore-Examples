[CmdletBinding(DefaultParameterSetName = "no-arguments")]
Param (
    [Parameter(HelpMessage = "Whether you desire to build the solution or the containers as well.")]
    [Switch]$IncludeContainers
)

if ($IncludeContainers) {
    docker-compose --env-file .\.env.user build
}