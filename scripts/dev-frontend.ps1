param(
    [string]$ApiUrl = "http://localhost:5258",
    [string]$Host = "localhost",
    [int]$PortaFrontend = 5173
)

$ErrorActionPreference = "Stop"

$env:VITE_URL_API = $ApiUrl

Write-Host "Iniciando frontend em http://$Host`:$PortaFrontend com API em $ApiUrl"

Push-Location frontend
try {
    npm run dev -- --host $Host --port $PortaFrontend
}
finally {
    Pop-Location
}
