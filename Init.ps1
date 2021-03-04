[CmdletBinding(DefaultParameterSetName = "no-arguments")]
Param (
    [Parameter(Mandatory = $true, HelpMessage = "The path to a valid Sitecore license.xml file.")]
    [string]$LicenseXmlPath,

    # We do not need to use [SecureString] here since the value will be stored unencrypted in .env,
    # and used only for transient local development environments.
    [Parameter(HelpMessage = "Sets the sitecore\\admin password for this environment via environment variable. ""b"" is used by default.")]
    [string]$AdminPwd = "b",

    [Parameter(HelpMessage = "Name of the CM instance, used as prefix in the hostname. ""cm"" is used by default.")]
    [string]$CMName = "cm",

    [Parameter(HelpMessage = "Name of the ID instance, used as prefix in the hostname. ""id"" is used by default.")]
    [string]$IDName = "id",

    [Parameter(HelpMessage = "Domain name used for the hostnames and certificates. ""examples.local"" is used by default.")]
    [string]$EnvironmentDomain = "examples.local"
)

$ErrorActionPreference = "Stop";

if (-not $LicenseXmlPath.EndsWith("license.xml")) {
    Write-Error "Sitecore license file must be named 'license.xml'."
}
if (-not (Test-Path $LicenseXmlPath)) {
    Write-Error "Could not find Sitecore license file at path '$LicenseXmlPath'."
}
# We actually want the folder that it's in for mounting
$LicenseXmlPath = (Get-Item $LicenseXmlPath).Directory.FullName


Write-Host "Preparing your Sitecore Containers environment!" -ForegroundColor Green

################################################
# Retrieve and import SitecoreDockerTools module
################################################

# Check for Sitecore Gallery
Import-Module PowerShellGet
$SitecoreGallery = Get-PSRepository | Where-Object { $_.SourceLocation -eq "https://sitecore.myget.org/F/sc-powershell/api/v2" }
if (-not $SitecoreGallery) {
    Write-Host "Adding Sitecore PowerShell Gallery..." -ForegroundColor Green 
    Register-PSRepository -Name SitecoreGallery -SourceLocation https://sitecore.myget.org/F/sc-powershell/api/v2 -InstallationPolicy Trusted
    $SitecoreGallery = Get-PSRepository -Name SitecoreGallery
}

#Install and Import SitecoreDockerTools 
$dockerToolsVersion = "10.1.4"
Remove-Module SitecoreDockerTools -ErrorAction SilentlyContinue
if (-not (Get-InstalledModule -Name SitecoreDockerTools -RequiredVersion $dockerToolsVersion -ErrorAction SilentlyContinue)) {
    Write-Host "Installing SitecoreDockerTools..." -ForegroundColor Green
    Install-Module SitecoreDockerTools -RequiredVersion $dockerToolsVersion -Scope CurrentUser -Repository $SitecoreGallery.Name
}
Write-Host "Importing SitecoreDockerTools..." -ForegroundColor Green
Import-Module SitecoreDockerTools -RequiredVersion $dockerToolsVersion


##################################
# Configure TLS/HTTPS certificates
##################################

Push-Location docker\data\traefik\certs
try {
    $mkcert = ".\mkcert.exe"
    if ($null -ne (Get-Command mkcert.exe -ErrorAction SilentlyContinue)) {
        # mkcert installed in PATH
        $mkcert = "mkcert"
    } elseif (-not (Test-Path $mkcert)) {
        Write-Host "Downloading and installing mkcert certificate tool..." -ForegroundColor Green 
        Invoke-WebRequest "https://github.com/FiloSottile/mkcert/releases/download/v1.4.1/mkcert-v1.4.1-windows-amd64.exe" -UseBasicParsing -OutFile mkcert.exe
        if ((Get-FileHash mkcert.exe).Hash -ne "1BE92F598145F61CA67DD9F5C687DFEC17953548D013715FF54067B34D7C3246") {
            Remove-Item mkcert.exe -Force
            throw "Invalid mkcert.exe file"
        }
    }
    Write-Host "Generating Traefik TLS certificate..." -ForegroundColor Green
    & $mkcert -install
    & $mkcert "*.$EnvironmentDomain"
}
catch {
    Write-Error "An error occurred while attempting to generate TLS certificate: $_" -ForegroundColor Red
}
finally {
    Pop-Location
}


################################
# Add Windows hosts file entries
################################

Write-Host "Adding Windows hosts file entries..." -ForegroundColor Green

$CMHost = "{0}.{1}" -f $CMName, $EnvironmentDomain
$IDHost = "{0}.{1}" -f $IDName, $EnvironmentDomain
Add-HostsEntry $CMHost
Add-HostsEntry $IDHost


###############################
# Populate the environment file
###############################
$UserEnvPath = ".\.env.user"
if(-not (Test-Path $UserEnvPath -PathType Leaf)) {
    Write-Host "No User Env file found, making copy based off example." -ForegroundColor Yellow
    Copy-Item ".\.env" -Destination $UserEnvPath
}

Write-Host "Populating required .env.user file values..." -ForegroundColor Green

# SITECORE_LICENSE_FOLDER
Set-DockerComposeEnvFileVariable "SITECORE_LICENSE_FOLDER" -Value $LicenseXmlPath -Path $UserEnvPath

# CM_HOST
Set-DockerComposeEnvFileVariable "CM_HOST" -Value $CMHost -Path $UserEnvPath

# ID_HOST
Set-DockerComposeEnvFileVariable "ID_HOST" -Value $IDHost -Path $UserEnvPath

# REPORTING_API_KEY = random 64-128 chars
Set-DockerComposeEnvFileVariable "REPORTING_API_KEY" -Value (Get-SitecoreRandomString 128 -DisallowSpecial) -Path $UserEnvPath

# TELERIK_ENCRYPTION_KEY = random 64-128 chars
Set-DockerComposeEnvFileVariable "TELERIK_ENCRYPTION_KEY" -Value (Get-SitecoreRandomString 128) -Path $UserEnvPath

# MEDIA_REQUEST_PROTECTION_SHARED_SECRET = random chars
Set-DockerComposeEnvFileVariable "MEDIA_REQUEST_PROTECTION_SHARED_SECRET" -Value (Get-SitecoreRandomString 64) -Path $UserEnvPath

# SITECORE_IDSECRET = random 64 chars
Set-DockerComposeEnvFileVariable "SITECORE_IDSECRET" -Value (Get-SitecoreRandomString 64 -DisallowSpecial) -Path $UserEnvPath

# SITECORE_ID_CERTIFICATE
$idCertPassword = Get-SitecoreRandomString 8 -DisallowSpecial
Set-DockerComposeEnvFileVariable "SITECORE_ID_CERTIFICATE" -Value (Get-SitecoreCertificateAsBase64String -DnsName "localhost" -Password (ConvertTo-SecureString -String $idCertPassword -Force -AsPlainText)) -Path $UserEnvPath

# SITECORE_ID_CERTIFICATE_PASSWORD
Set-DockerComposeEnvFileVariable "SITECORE_ID_CERTIFICATE_PASSWORD" -Value $idCertPassword -Path $UserEnvPath

# SQL_SA_PASSWORD
# Need to ensure it meets SQL complexity requirements
Set-DockerComposeEnvFileVariable "SQL_SA_PASSWORD" -Value (Get-SitecoreRandomString 19 -DisallowSpecial -EnforceComplexity) -Path $UserEnvPath

# SITECORE_ADMIN_PASSWORD
Set-DockerComposeEnvFileVariable "SITECORE_ADMIN_PASSWORD" -Value $AdminPwd


######################
# Restore dotnet tools
######################
# Added interactive mode to ensure login capability for protected NuGet feeds present in your configuration.
dotnet tool restore --interactive


Write-Host "Done!" -ForegroundColor Green