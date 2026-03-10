# Desafio Moovefy: Gestão Colaborativa de Tarefas em Tempo Real

## Descrição do Projeto
Sistema web full stack para gestão de projetos e tarefas em equipes, com autenticação JWT, dashboard de métricas e notificações em tempo real via SignalR.

O foco da implementação foi manter arquitetura profissional, regras de negócio explícitas e código legível, usando nomenclatura em português no domínio da solução.

## Tecnologias Utilizadas
### Backend
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- FluentValidation
- Serilog
- SignalR
- JWT Bearer Authentication
- IMemoryCache
- Rate Limiting nativo do ASP.NET Core
- Swagger/OpenAPI

### Frontend
- React
- TypeScript
- Vite
- React Router
- TanStack Query
- React Hook Form
- Zod
- SignalR Client (`@microsoft/signalr`)
- CSS global customizado
- Recharts

### Testes
- xUnit
- FluentAssertions
- testes unitários
- testes de integração com `WebApplicationFactory`

### DevOps
- Docker Compose
- GitHub Actions
- Kubernetes (manifests básicos)

## Funcionalidades Entregues
- CRUD completo de projetos
- CRUD completo de tarefas (incluindo edicao e atualizacao de status)
- filtros, ordenação e paginação de tarefas
- dashboard com métricas
- autenticação e autorização com JWT
- notificações em tempo real ao atribuir ou reatribuir tarefa (SignalR)
- histórico de notificações integrado no frontend
- cache em endpoints de consulta
- rate limiting global e específico no login
- padronização de respostas e erros da API

## Como Executar com Docker
### Pré-requisitos
- Docker Desktop em execução

### Subir a stack completa
```bash
docker compose --env-file .env.compose.example up -d --build
```

### URLs principais
- Frontend: `http://localhost:5173`
- API: `http://localhost:5258`
- Swagger: `http://localhost:5258/documentacao`

### Encerrar ambiente
```bash
docker compose down
```

## Como Executar sem Docker
### Pré-requisitos
- .NET SDK 8.0
- Node.js 20+
- npm 10+
- PostgreSQL

### Opção híbrida (recomendada para desenvolvimento local)
1. Suba só o banco com Docker:
```bash
docker compose up -d banco_dados
```

2. Suba a API (PowerShell):
```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ConnectionStrings__BancoDados="Host=localhost;Port=55433;Database=gerenciador_tarefas_dev;Username=postgres;Password=postgres"
$env:BancoDados__AplicarMigracoesAutomaticamente="true"
$env:BancoDados__AplicarSeedDadosDemonstracao="true"
dotnet run --project backend/src/GerenciadorTarefas.Api
```

3. Suba o frontend:
```bash
cd frontend
npm install
npm run dev
```

No PowerShell, copie o arquivo de ambiente com:
```powershell
Copy-Item .env.example .env
```

## Portas Usadas
- Frontend (Vite/Nginx): `5173`
- API (HTTP): `5258`
- API (HTTPS local profile): `7188`
- PostgreSQL no Docker Compose (host): `55433`
- PostgreSQL no container/cluster: `5432`

## Credenciais Padrão
Credenciais de demonstracao para ambiente `Development`/testes.
Em execucao de producao, configure usuarios JWT por variavel de ambiente.

- Administrador
  - Email: `admin@gerenciadortarefas.local`
  - Senha: `Admin@123`
- Colaborador
  - Email: `colaborador@gerenciadortarefas.local`
  - Senha: `Colaborador@123`

## Endpoints Principais
- `POST /api/autenticacao/login`
- `GET/POST/PUT/DELETE /api/projetos`
- `GET/POST/PUT/DELETE /api/tarefas`
- `PATCH /api/tarefas/{id}/status`
- `GET /api/dashboard/metricas`
- `GET /api/notificacoes`
- Hub SignalR: `/hubs/notificacoes`

## Como Rodar Testes
### Todos os testes
```bash
dotnet test backend/GerenciadorTarefas.sln -m:1 -v minimal
```

### Somente testes unitários
```bash
dotnet test backend/tests/GerenciadorTarefas.TestesUnitarios/GerenciadorTarefas.TestesUnitarios.csproj -m:1 -v minimal
```

### Somente testes de integração
```bash
dotnet test backend/tests/GerenciadorTarefas.TestesIntegracao/GerenciadorTarefas.TestesIntegracao.csproj -m:1 -v minimal
```

### Cobertura de testes (linha de base atual)
```bash
dotnet test backend/GerenciadorTarefas.sln -m:1 -v minimal --collect:"XPlat Code Coverage" --results-directory backend/TestResults
```

Resultado atual (execucao local em 10/03/2026):
- cobertura agregada de linhas: `67,78%`
- linhas cobertas: `2255`
- linhas validas: `3327`

## CI/CD
- CI em `.github/workflows/ci.yml`
  - executa em `push` e `pull_request` para `main`
  - backend: restore, build, testes unitarios/integracao e coleta de cobertura
  - frontend: install e build
  - upload de artefatos de teste/cobertura
- CD em `.github/workflows/cd.yml`
  - executa em `push` para `main` e em `workflow_dispatch`
  - build e push das imagens Docker da API e do frontend no GHCR
  - tags publicadas: `latest` e `${GITHUB_SHA}`

## Kubernetes
Manifests básicos em `kubernetes/`:
- namespace
- configmap
- secret de exemplo
- pvc
- deployment/service de banco
- deployment/service de API
- deployment/service de frontend
- ingress HTTP com rotas para frontend (`/`) e API (`/api`, `/hubs`)

Observação: as imagens nos manifests são placeholders e devem ser substituídas pelas imagens publicadas do projeto.

## Decisões Técnicas
Detalhamento em:
- `docs/arquitetura.md`
- `docs/decisoes-tecnicas.md`

Resumo:
- Clean Architecture leve
- Repository Pattern
- casos de uso por contexto
- controllers finos
- regras de negócio no domínio/aplicação

## Patterns Aplicados
- Repository
- Casos de Uso (Application Services)
- DTOs separados de entidades
- Middleware para tratamento global de exceções
- Envelope padronizado de sucesso e erro

## Trade-offs
- Arquitetura limpa, porém sem overengineering (sem Unit of Work dedicado)
- Autenticação com usuários de demonstração em configuração para simplificar ambiente local
- Kubernetes em nível essencial (sem HPA e sem observabilidade de cluster nesta fase)

## Uso de IA no Desenvolvimento
A IA foi usada como apoio técnico para:
- acelerar criação de estrutura e boilerplate
- revisar coerência arquitetural
- sugerir melhorias de organização e documentação

Todo código gerado foi revisado, testado e ajustado manualmente antes de aceitação.

## Melhorias Futuras
- pipeline de deploy automatizado por ambiente (staging/prod)
- observabilidade completa com tracing distribuído
- rotação de segredos com secret manager
- testes E2E no frontend
- políticas RBAC mais granulares
- autoscaling e políticas de disponibilidade no Kubernetes

## Estrutura do Projeto
```text
/
  backend/
    src/
      GerenciadorTarefas.Api/
      GerenciadorTarefas.Aplicacao/
      GerenciadorTarefas.Dominio/
      GerenciadorTarefas.Infraestrutura/
    tests/
      GerenciadorTarefas.TestesUnitarios/
      GerenciadorTarefas.TestesIntegracao/
  frontend/
    src/
      aplicacao/
      paginas/
      componentes/
      funcionalidades/
      servicos/
      ganchos/
      contextos/
      rotas/
      tipos/
      utilitarios/
      estilos/
  docs/
    arquitetura.md
    decisoes-tecnicas.md
  kubernetes/
  .github/workflows/
  docker-compose.yml
```

## Observações Finais
- O arquivo `frontend/.env` não deve ser versionado.
- O projeto segue a convenção de nomenclatura em português para domínio e aplicação.
- O fluxo de entrega foi incremental com commits pequenos e rastreáveis.
