param(
    [string]$Host = "0.0.0.0",
    [int]$PortaApi = 8080,
    [Parameter(Mandatory = $true)][string]$ConnectionString,
    [Parameter(Mandatory = $true)][string]$ChaveJwt,
    [bool]$AplicarMigracoesAutomaticamente = $false
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    throw "ConnectionString obrigatoria para inicializar em modo de producao."
}

if ([string]::IsNullOrWhiteSpace($ChaveJwt) -or $ChaveJwt.Length -lt 32) {
    throw "Chave JWT obrigatoria com no minimo 32 caracteres para modo de producao."
}

$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS = "http://$Host`:$PortaApi"
$env:ConnectionStrings__BancoDados = $ConnectionString
$env:AutenticacaoJwt__ChaveSecreta = $ChaveJwt
$env:BancoDados__AplicarMigracoesAutomaticamente = $AplicarMigracoesAutomaticamente.ToString().ToLowerInvariant()
$env:BancoDados__AplicarSeedDadosDemonstracao = "false"

Write-Host "Iniciando API em modo Production em $($env:ASPNETCORE_URLS)"

dotnet run --project backend/src/GerenciadorTarefas.Api/GerenciadorTarefas.Api.csproj --no-launch-profile
