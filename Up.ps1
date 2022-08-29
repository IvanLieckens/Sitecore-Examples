[CmdletBinding(DefaultParameterSetName = "no-arguments")]
Param (
    [Parameter(HelpMessage = "Name of the CM instance, used as prefix in the hostname. 'cm' is used by default.")]
    [string]$CMName = "cm",

    [Parameter(HelpMessage = "Name of the ID instance, used as prefix in the hostname. 'id' is used by default.")]
    [string]$IDName = "id",

    [Parameter(HelpMessage = "Domain name used for the hostnames and certificates. 'examples.local' is used by default.")]
    [string]$EnvironmentDomain = "examples.local",

    [Parameter(HelpMessage = "Path to the .env file to use. '.\.env.user' is used by default.")]
    [string]$EnvFilePath = ".\.env.user",

    [Parameter(HelpMessage = "Determines whether there will be a dotnet sitecore ser push + publish or not.")]
    [Switch]$SkipPush,

    [Parameter(HelpMessage = "Determines whether there will be docker-compose build or not.")]
    [Switch]$SkipBuild
)

$ErrorActionPreference = "Stop";
$CMHost = "{0}.{1}" -f $CMName, $EnvironmentDomain
$IDHost = "{0}.{1}" -f $IDName, $EnvironmentDomain

# Double check whether init has been run
if (-not (Test-Path $EnvFilePath -PathType Leaf)) {
    throw "There is no '$EnvFilePath' file. Did you run 'Init.ps1'?"
}

# Build the containers
if (-not $SkipBuild) {
    docker-compose --env-file $EnvFilePath build
}

# Start the Sitecore instance
Write-Host "Starting Sitecore environment..." -ForegroundColor Green
docker-compose --env-file $EnvFilePath up -d

# Wait for Traefik to expose CM route
Write-Host "Waiting for CM to become available..." -ForegroundColor Green
$startTime = Get-Date
do {
    Start-Sleep -Milliseconds 100
    try {
        $status = Invoke-RestMethod "http://localhost:8079/api/http/routers/cm-secure@docker"
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -ne "404") {
            throw
        }
    }
} while ($status.status -ne "enabled" -and $startTime.AddSeconds(15) -gt (Get-Date))
if (-not $status.status -eq "enabled") {
    $status
    Write-Error "Timeout waiting for Sitecore CM to become available via Traefik proxy. Check CM container logs."
}

if (-not $SkipPush) {
    dotnet sitecore login --cm "https://$CMHost/" --auth "https://$IDHost/" --allow-write true
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Unable to log into Sitecore, did the Sitecore environment start correctly? See logs above."
    }

    Write-Host "Pushing latest items to Sitecore..." -ForegroundColor Green

    dotnet sitecore ser push
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Serialization push failed, see errors above."
    }

    dotnet sitecore publish
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Item publish failed, see errors above."
    }
}

Write-Host "Sitecore is online and ready." -ForegroundColor Green