param(
    [string]$Host = "localhost",
    [int]$PortaApi = 5258,
    [string]$ConnectionString = "Host=localhost;Port=55433;Database=gerenciador_tarefas_dev;Username=postgres;Password=postgres",
    [bool]$AplicarMigracoesAutomaticamente = $true,
    [bool]$AplicarSeedDadosDemonstracao = $true
)

$ErrorActionPreference = "Stop"

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://$Host`:$PortaApi"
$env:ConnectionStrings__BancoDados = $ConnectionString
$env:BancoDados__AplicarMigracoesAutomaticamente = $AplicarMigracoesAutomaticamente.ToString().ToLowerInvariant()
$env:BancoDados__AplicarSeedDadosDemonstracao = $AplicarSeedDadosDemonstracao.ToString().ToLowerInvariant()

Write-Host "Iniciando API em $($env:ASPNETCORE_URLS)"
Write-Host "String de conexao: $ConnectionString"

dotnet run --project backend/src/GerenciadorTarefas.Api/GerenciadorTarefas.Api.csproj
