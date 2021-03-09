# escape=`

ARG PARENT_IMAGE
ARG SOLUTION_IMAGE
ARG TOOLS_IMAGE
ARG MANAGEMENT_SERVICES_IMAGE

FROM ${SOLUTION_IMAGE} as solution
FROM ${TOOLS_IMAGE} as tools
FROM ${MANAGEMENT_SERVICES_IMAGE} AS management_services
FROM ${PARENT_IMAGE}

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

WORKDIR C:\inetpub\wwwroot

# Copy developer tools and entrypoint
COPY --from=tools C:\tools C:\tools

# Copy the Sitecore Management Services Module
COPY --from=management_services C:\module\cm\content C:\inetpub\wwwroot

# Copy solution files
COPY --from=solution /artifacts/sitecore/cm/ ./

# Transform Web.config
COPY transforms/Web.config /temp/
RUN & 'C:\tools\scripts\Invoke-XdtTransform.ps1' -Path 'Web.config' -XdtPath 'C:\temp\Web.config' -XdtDllPath 'C:\tools\bin\Microsoft.Web.XmlTransform.dll'; `
    Remove-Item -Path '\\?\C:\temp' -Recurse -Force;