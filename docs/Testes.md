# 7. Testes (Aprofundado)

## 7.1 Objetivo da estrategia de testes

A estrategia de testes foi desenhada para validar:
- regras de negocio centrais;
- autorizacao por perfil e area;
- contratos HTTP da API;
- fluxos criticos do frontend;
- estabilidade da entrega em CI.

Foco principal:
- priorizar testes representativos do comportamento real do sistema, nao apenas cobertura superficial.

## 7.2 Tipos de testes adotados

## Backend

### Testes unitarios
- Framework: `xUnit` + `FluentAssertions`.
- Projeto: `backend/tests/GerenciadorTarefas.TestesUnitarios`.
- Escopo: dominio e casos de uso da camada aplicacao.

### Testes de integracao
- Framework: `xUnit` + `Microsoft.AspNetCore.Mvc.Testing`.
- Projeto: `backend/tests/GerenciadorTarefas.TestesIntegracao`.
- Escopo: endpoints reais da API com autenticacao/autorizacao, migrations e seed.
- Banco: PostgreSQL real por banco efemero criado em runtime de teste.

## Frontend

### Testes de componente, pagina, contexto e servico
- Framework: `Vitest` + `Testing Library` + `jsdom`.
- Escopo: login, rotas protegidas, telas principais, formularios, servicos e notificacoes em tempo real.

### Cobertura frontend
- Provider: `v8`.
- Reporters: `text`, `html`, `lcov`.
- Configuracao: `frontend/vite.config.ts`.

## 7.3 Estrutura de testes no repositorio

Backend unitario:
- `backend/tests/GerenciadorTarefas.TestesUnitarios/Aplicacao`
- `backend/tests/GerenciadorTarefas.TestesUnitarios/Dominio`

Backend integracao:
- `backend/tests/GerenciadorTarefas.TestesIntegracao/AutenticacaoIntegracaoTestes.cs`
- `backend/tests/GerenciadorTarefas.TestesIntegracao/AdministracaoIntegracaoTestes.cs`
- `backend/tests/GerenciadorTarefas.TestesIntegracao/ProjetosIntegracaoTestes.cs`
- `backend/tests/GerenciadorTarefas.TestesIntegracao/TarefasIntegracaoTestes.cs`
- `backend/tests/GerenciadorTarefas.TestesIntegracao/NotificacoesIntegracaoTestes.cs`
- `backend/tests/GerenciadorTarefas.TestesIntegracao/DashboardIntegracaoTestes.cs`
- `backend/tests/GerenciadorTarefas.TestesIntegracao/ObservabilidadeIntegracaoTestes.cs`

Frontend:
- `frontend/src/**/*.test.ts`
- `frontend/src/**/*.test.tsx`

## 7.4 Como executar os testes

### Backend unitario

```powershell
dotnet test backend/tests/GerenciadorTarefas.TestesUnitarios/GerenciadorTarefas.TestesUnitarios.csproj
```

### Backend integracao

Pre-requisito:
- PostgreSQL ativo e acessivel.

Opcionalmente definir conexao administrativa:

```powershell
$env:TESTES_INTEGRACAO_CONNECTION_STRING_ADMIN="Host=localhost;Port=55433;Database=postgres;Username=postgres;Password=postgres;Pooling=false"
```

Execucao:

```powershell
dotnet test backend/tests/GerenciadorTarefas.TestesIntegracao/GerenciadorTarefas.TestesIntegracao.csproj
```

### Frontend

```powershell
cd frontend
npm run test
```

Cobertura:

```powershell
cd frontend
npm run test:coverage
```

### Execucao completa (script unico)

```powershell
.\scripts\testes-completo.ps1
```

Sem integracao backend:

```powershell
.\scripts\testes-completo.ps1 -PularIntegracaoBackend
```

## 7.5 Execucao em CI

Pipeline:
- `.github/workflows/ci.yml`

Fluxo de testes em CI:
- backend:
  - sobe PostgreSQL 16 em service container;
  - executa testes unitarios e integracao;
  - coleta cobertura com `coverlet.collector`.
- frontend:
  - executa build;
  - executa testes com cobertura via `vitest --coverage`;
  - publica artefatos de cobertura.

## 7.6 Cobertura atual (snapshot)

Data de referencia do snapshot local: `12/03/2026`.

## Backend (Cobertura/Coverlet)

Origem:
- `backend/TestResults/readme-cobertura/a0563b45-7099-48b1-9212-b359439ebe6d/coverage.cobertura.xml`

Metricas gerais:
- linhas: `65.46%`
- branches: `53.61%`

Por pacote:
- `GerenciadorTarefas.Aplicacao`: linhas `63.84%`, branches `51.15%`
- `GerenciadorTarefas.Dominio`: linhas `85.45%`, branches `100%`

## Frontend (Vitest + V8)

Origem:
- execucao `npm run test:coverage` (saida de console e `frontend/coverage/lcov.info`)

Metricas gerais:
- statements: `57.77%`
- branches: `50.43%`
- funcoes: `56.98%`
- linhas: `58.24%`

## 7.7 Status de execucao local mais recente

Executado localmente em `12/03/2026`:

- backend unitario:
  - resultado: `89/89` aprovados
- backend integracao:
  - resultado: `22/22` aprovados
- frontend:
  - execucao da suite acontece via `npm run test` e `npm run test:coverage` (local/CI)
  - nao ha testes marcados como `skip` no repositorio

## 7.8 Regras de negocio e fluxos cobertos

Exemplos de cenarios cobertos:
- login valido/invalido e bloqueio temporario por tentativas;
- restricoes de acesso por perfil (`SuperAdmin`, `Admin`, `Colaborador`);
- administracao de usuarios/areas dentro e fora de escopo;
- criacao/edicao de projetos e tarefas com validacoes;
- historico de notificacoes respeitando usuario autenticado;
- health checks e endpoints de observabilidade;
- fluxos de UI principais (login, dashboard, usuarios, projetos, areas).

## 7.9 Limitacoes atuais da suite

- parte dos testes de integracao depende de infraestrutura PostgreSQL disponivel;
- a execucao local completa de cobertura frontend pode variar por ambiente (maquina/recursos/timeout);
- cobertura frontend ainda pode crescer em cenarios mais densos de tabela/filtros.

## 7.10 Proximas evolucoes recomendadas para testes

1. ampliar testes de regressao para autorizacao por recurso (perfil + area + ownership).
2. adicionar testes E2E para fluxos ponta a ponta mais criticos.
3. definir metas de cobertura por camada com gate progressivo na CI.
4. aprofundar cenarios frontend de filtros, paginacao e operacoes em lote.
5. evoluir a coleta de cobertura para relatorios consolidados por modulo.
