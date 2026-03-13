Param(
    [switch]$PularIntegracaoBackend = $false
)

$ErrorActionPreference = "Stop"

Write-Host "==> Frontend: instalando dependencias e executando build + testes com cobertura"
Push-Location frontend
try {
    cmd /c npm install
    cmd /c npm run build
    cmd /c npm run test:coverage
}
finally {
    Pop-Location
}

Write-Host "==> Backend: build + testes unitarios"
Push-Location backend
try {
    dotnet build
    dotnet test tests/GerenciadorTarefas.TestesUnitarios/GerenciadorTarefas.TestesUnitarios.csproj

    if (-not $PularIntegracaoBackend) {
        Write-Host "==> Backend: testes de integracao (requer PostgreSQL ativo)"
        dotnet test tests/GerenciadorTarefas.TestesIntegracao/GerenciadorTarefas.TestesIntegracao.csproj
    }
}
finally {
    Pop-Location
}

Write-Host "==> Execucao completa finalizada."
