param(
    [string]$ArquivoAmbiente = ".env"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $ArquivoAmbiente)) {
    throw "Arquivo de ambiente '$ArquivoAmbiente' nao encontrado. Crie-o a partir de .env.example ou .env.compose.example."
}

Write-Host "Subindo stack Docker com arquivo de ambiente: $ArquivoAmbiente"
docker compose --env-file $ArquivoAmbiente up -d --build
