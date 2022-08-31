[CmdletBinding(DefaultParameterSetName = "no-arguments")]
Param (
    [Parameter(HelpMessage = "Path to the .env file to use. '.\.env.user' is used by default.")]
    [string]$EnvFilePath = ".\.env.user"
)

Write-Host "Down containers..." -ForegroundColor Green
try {
  docker-compose --env-file $EnvFilePath down
  if ($LASTEXITCODE -ne 0) {
    Write-Error "Container down failed, see errors above."
  }
}
finally {
}
