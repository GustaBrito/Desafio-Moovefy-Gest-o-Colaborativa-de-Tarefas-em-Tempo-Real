# 1. Descricao do Projeto

Este projeto implementa um **Sistema de Gestao de Tarefas com Notificacoes** para organizacao do trabalho em equipes.

A aplicacao resolve o problema de controle operacional de atividades, centralizando o acompanhamento de projetos, tarefas e responsaveis em um fluxo unico, com visibilidade de status e prioridades.

Os principais modulos do sistema sao:
- autenticacao e controle de acesso
- gestao de projetos
- gestao de tarefas
- dashboard com metricas operacionais
- notificacoes e atualizacoes em tempo real

De ponta a ponta, a solucao permite autenticar usuarios, gerenciar projetos e tarefas, acompanhar indicadores no dashboard e receber notificacoes em tempo real sobre eventos relevantes de atribuicao e acompanhamento de tarefas.

Documentacao aprofundada por item:
- `docs/01-descricao-do-projeto.md`
- `docs/02-tecnologias-utilizadas.md`
- `docs/03-como-executar.md`
- `docs/04-decisoes-tecnicas.md`
- `docs/05-uso-de-ia-no-desenvolvimento.md`
- `docs/06-melhorias-futuras.md`
- `docs/07-testes.md`
- `docs/arquitetura.md`
- `docs/decisoes-tecnicas.md`

# 2. Tecnologias Utilizadas

## Backend
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8 (ORM)
- Npgsql Entity Framework Core Provider (PostgreSQL)
- FluentValidation (validacoes)
- JWT Bearer Authentication (autenticacao e autorizacao)
- SignalR (mensageria em tempo real)
- Serilog + Console Sink (logging estruturado)
- Cache em memoria (IMemoryCache)
- Rate Limiting nativo do ASP.NET Core

## Frontend
- React 18
- TypeScript
- Vite
- React Router DOM (roteamento)
- TanStack React Query (estado assicrono e cache de dados)
- React Hook Form + Zod + @hookform/resolvers (formularios e validacoes)
- Recharts (graficos no dashboard)
- @microsoft/signalr (cliente de tempo real)

## Banco de Dados
- PostgreSQL

## Infra e DevOps
- Docker
- Docker Compose
- GitHub Actions (pipeline de CI para build e testes de backend/frontend)

## Testes
- xUnit (testes unitarios e de integracao no backend)
- FluentAssertions
- Microsoft.AspNetCore.Mvc.Testing (WebApplicationFactory para integracao)
- Vitest + Testing Library + jsdom (testes no frontend)
- Coverlet e V8 coverage (coleta de cobertura)

## Documentacao da API
- Swagger / OpenAPI (Swashbuckle)

# 3. Como Executar

## 3.1 Pre-requisitos
- .NET SDK `8.0.418` (conforme `global.json`)
- Node.js `20.x` e npm
- Docker e Docker Compose (para execucao conteinerizada)
- PostgreSQL local (se optar por rodar sem Docker completo)
- Opcional: `dotnet-ef` para migracoes manuais

Comando opcional para instalar `dotnet-ef`:
```powershell
dotnet tool install --global dotnet-ef
```

## 3.2 Como rodar com Docker

Arquivos utilizados:
- `docker-compose.yml`
- `.env` (recomendado copiar de `.env.example` ou `.env.compose.example`)
- `backend/src/GerenciadorTarefas.Api/Dockerfile`
- `frontend/Dockerfile`

1. Criar arquivo de ambiente:
```powershell
Copy-Item .env.example .env
```

2. Subir todos os servicos:
```powershell
docker compose --env-file .env up -d --build
```

Alternativa usando script:
```powershell
.\scripts\dev-com-docker.ps1
```

3. Verificar se subiu corretamente:
```powershell
docker compose ps
```

Servicos iniciados:
- `banco_dados` (PostgreSQL)
- `api` (.NET Web API)
- `frontend` (Nginx servindo build React)

Validacoes rapidas:
- Frontend: `http://localhost:5173`
- API Swagger: `http://localhost:5258/documentacao`

## 3.3 Como rodar sem Docker

### Opcao A: banco local ja instalado
1. Iniciar PostgreSQL local (ou garantir instancia externa ativa).
2. Subir backend:
```powershell
.\scripts\dev-sem-docker-api.ps1
```
3. Subir frontend em outro terminal:
```powershell
.\scripts\dev-frontend.ps1
```

### Opcao B: somente banco via Docker + app local
1. Subir apenas o banco:
```powershell
docker compose up -d banco_dados
```
2. Subir backend:
```powershell
.\scripts\dev-sem-docker-api.ps1
```
3. Subir frontend:
```powershell
.\scripts\dev-frontend.ps1
```

### Migracoes e seed
No script `dev-sem-docker-api.ps1`, por padrao:
- `BancoDados__AplicarMigracoesAutomaticamente=true`
- `BancoDados__AplicarSeedDadosDemonstracao=true`

Migracao manual (opcional):
```powershell
dotnet ef database update --project backend/src/GerenciadorTarefas.Infraestrutura/GerenciadorTarefas.Infraestrutura.csproj --startup-project backend/src/GerenciadorTarefas.Api/GerenciadorTarefas.Api.csproj
```

Ordem recomendada de inicializacao:
1. Banco
2. Backend
3. Frontend

## 3.4 Variaveis de ambiente / configuracao

Arquivos principais:
- `.env.example` / `.env.compose.example`
- `backend/src/GerenciadorTarefas.Api/appsettings.json`
- `backend/src/GerenciadorTarefas.Api/appsettings.Development.json`
- `frontend/.env.example`

Variaveis mais importantes:
- Banco: `ConnectionStrings__BancoDados`
- Banco (docker): `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_PORTA_HOST`
- JWT: `AutenticacaoJwt__ChaveSecreta` (minimo 32 caracteres)
- JWT: `AutenticacaoJwt__Emissor`
- JWT: `AutenticacaoJwt__Publico`
- Frontend: `VITE_URL_API`
- API: `ASPNETCORE_URLS`
- API (docker): `API_PORTA_HOST`, `API_PORTA_CONTAINER`
- CORS: `Cors__OrigensPermitidas__0`, `Cors__OrigensPermitidas__1`, etc.

## 3.5 Credenciais padrao

Credenciais seed (quando `AplicarSeedDadosDemonstracao=true`):

- SuperAdmin | Email: `superadmin@gerenciadortarefas.local` | Senha: `SuperAdmin@123` | Perfil: `SuperAdmin`
- Admin Marketing | Email: `admin.marketing@gerenciadortarefas.local` | Senha: `AdminMarketing@123` | Perfil: `Admin`
- Admin Desenvolvimento | Email: `admin.dev@gerenciadortarefas.local` | Senha: `AdminDev@123` | Perfil: `Admin`
- Admin Gestao | Email: `admin.gestao@gerenciadortarefas.local` | Senha: `AdminGestao@123` | Perfil: `Admin`
- Colaborador Desenvolvimento | Email: `colaborador.dev@gerenciadortarefas.local` | Senha: `ColaboradorDev@123` | Perfil: `Colaborador`
- Colaborador Marketing | Email: `colaborador.marketing@gerenciadortarefas.local` | Senha: `ColaboradorMkt@123` | Perfil: `Colaborador`

Perfis disponiveis no sistema:
- `SuperAdmin`
- `Admin`
- `Colaborador`

## 3.6 Portas

Portas padrao da aplicacao:
- Frontend: `5173`
- API: `5258`
- Banco PostgreSQL (host): `55433`
- Swagger: `http://localhost:5258/documentacao`
- SignalR Hub: `http://localhost:5258/hubs/notificacoes`

Ferramenta de administracao do banco:
- Nao existe servico de administracao (ex.: pgAdmin) no `docker-compose.yml` atual.
- Se necessario, utilize cliente externo (DBeaver, pgAdmin desktop, Azure Data Studio etc.).

# 4. Decisoes Tecnicas

## 4.1 Arquitetura escolhida

A solucao foi organizada com separacao clara entre frontend e backend, com backend em camadas seguindo uma Clean Architecture leve:
- `GerenciadorTarefas.Dominio`: entidades e contratos centrais.
- `GerenciadorTarefas.Aplicacao`: casos de uso, modelos e orquestracao de regras.
- `GerenciadorTarefas.Infraestrutura`: EF Core, repositorios, mapeamentos, migrations e seed.
- `GerenciadorTarefas.Api`: endpoints HTTP, autenticacao/autorizacao, middlewares e SignalR.

Motivos principais da escolha:
- reduzir acoplamento entre regra de negocio e tecnologia de persistencia.
- facilitar testes por camada (unitario na aplicacao/dominio e integracao na API).
- permitir evolucao incremental sem reescrever toda a base.
- manter frontend e backend independentes para deploy e manutencao.

No frontend, a modularizacao foi feita por contexto de funcionalidade (`autenticacao`, `projetos`, `tarefas`, `usuarios`, `areas`, `dashboard`, `notificacoes`), com separacao entre:
- pagina (orquestracao de tela),
- componente reutilizavel,
- servico de API,
- contexto/gancho de estado.

## 4.2 Patterns aplicados

Pattern principal exigido no desafio:
- Repository Pattern
  - Onde foi aplicado: contratos no dominio e implementacoes em `Infraestrutura/Repositorios` (ex.: `RepositorioTarefa`, `RepositorioProjeto`, `RepositorioUsuario`).
  - Por que foi escolhido: isolar acesso a dados e evitar dependencias diretas de EF Core nos casos de uso.
  - Problema que resolve: diminui acoplamento entre regra de negocio e banco, facilitando manutencao e testes.

Outros patterns aplicados na implementacao:
- Cache-Aside
  - Onde: `IServicoCacheConsulta` / `ServicoCacheConsultaMemoria` e uso nos controladores com invalidacao apos mutacoes.
  - Beneficio: melhora tempo de resposta em consultas repetidas sem comprometer consistencia basica.

- Dependency Injection
  - Onde: configuracao central em `ConfiguracaoInjecaoDependencia`.
  - Beneficio: composicao de servicos desacoplada e testavel.

## 4.3 Trade-offs

- EF Core em vez de Dapper
  - Ganho: produtividade com mapeamentos, migrations e relacoes complexas do dominio.
  - Trade-off: menos controle de SQL fino em cenarios extremos de performance.

- Context API + React Query em vez de Redux
  - Ganho: menor boilerplate para o escopo do desafio, mantendo estado global essencial (sessao/notificacao) e cache assicrono robusto.
  - Trade-off: em cenarios muito grandes, pode exigir evolucao futura para arquitetura de estado mais formal.

- SignalR para notificacoes e HTTP para consultas principais
  - Ganho: tempo real apenas no que precisa (evento de notificacao), sem complexidade de tornar toda a aplicacao stateful em websocket.
  - Trade-off: listas e metricas continuam dependentes de refresh/query invalidation.

- JWT simples sem ASP.NET Identity completo/OAuth externo
  - Ganho: implementacao pragmatica e aderente ao escopo do desafio.
  - Trade-off: nao cobre recursos avancados de identidade (federacao, refresh token robusto, gestao completa de contas).

- Observabilidade e seguranca elevadas, mas com escopo controlado
  - Ganho: logging estruturado, rate limiting, health checks e regras de autorizacao por perfil/area.
  - Trade-off: tracing distribuido e politicas mais avancadas de operacao podem ser evoluidos em proxima etapa.

# 5. Uso de IA no Desenvolvimento

## 5.1 Ferramentas utilizadas
- Assistente de IA generativa para engenharia de software (Codex/ChatGPT).

## 5.2 Onde a IA foi utilizada
- Apoio na criacao de boilerplate de backend e frontend.
- Apoio na estruturacao inicial de testes (unitarios, integracao e frontend).
- Apoio na revisao de consistencia entre regras de negocio, contratos de API e telas.
- Apoio na redacao e organizacao da documentacao tecnica.
- Apoio na investigacao de erros e propostas de correcao.

## 5.3 O que foi ajustado manualmente
- Regras de autorizacao por perfil e por area.
- Validacoes de negocio e de seguranca em endpoints sensiveis.
- Contratos de entrada/saida para manter compatibilidade entre backend e frontend.
- Refinamento de layout e fluxo das telas para aderencia ao escopo.
- Ajustes de pipeline e scripts de execucao para ambiente local/CI.

## 5.4 O que foi refatorado ou corrigido apos geracao
- Correcao de inconsistencias de tipagem e contratos no frontend.
- Correcao de falhas de autorizacao e endurecimento de acesso em notificacoes/SignalR.
- Correcao de problemas de datas e timezone no fluxo com PostgreSQL.
- Refatoracao de partes da UI para reduzir acoplamento e melhorar manutencao.
- Ajustes em testes para estabilidade e cobertura representativa.

## 5.5 Como o codigo foi validado
- Build backend: `dotnet build`.
- Testes unitarios backend: `dotnet test` (projeto de testes unitarios).
- Testes de integracao backend com banco real quando ambiente disponivel.
- Build frontend: `npm run build`.
- Testes frontend: `npm run test`.
- Cobertura frontend: `npm run test:coverage`.
- Validacao manual de fluxos criticos (autenticacao, projetos, tarefas, dashboard, notificacoes).
- Execucao em ambiente conteinerizado com `docker compose` para verificacao ponta a ponta.

## 5.6 Criterio de uso responsavel
- A IA foi usada como apoio de produtividade, nao como substituto de decisao tecnica.
- Toda sugestao aplicada passou por revisao manual.
- Codigo e documentacao foram ajustados com base em testes, execucao real e analise critica.

# 6. Melhorias Futuras

## 6.1 O que seria feito com mais tempo
- Ampliar cobertura de testes de integracao backend com cenarios mais extensos de autorizacao por area/perfil.
- Adicionar testes E2E no frontend para fluxos criticos (login, projetos, tarefas, dashboard, notificacoes).
- Evoluir UX de filtros e tabelas com busca avancada, paginacao mais flexivel e melhor tratamento de estados vazios.
- Implementar telemetria mais completa com trilhas de erro e indicadores operacionais por endpoint.

## 6.2 O que foi simplificado para entregar no prazo
- Fluxo de autenticacao mantido em JWT simples sem refresh token robusto.
- Escopo de observabilidade focado em logging e health checks basicos.
- Testes frontend priorizados para fluxos principais, sem suite E2E completa.
- Infra de deploy mantida em nivel pratico (CI e compose), sem estrategia multiambiente totalmente automatizada.

## 6.3 Funcionalidades que podem ser adicionadas
- Busca avancada com filtros compostos e persistencia de visoes salvas por usuario.
- Paginacao server-side com ordenacao multi-coluna em todas as listagens administrativas.
- Historico de auditoria por entidade (quem alterou, quando e o que mudou).
- Acoes em lote com maior rastreabilidade e feedback operacional.

## 6.4 Melhorias arquiteturais
- RBAC mais completo com politicas por permissao fina alem do perfil global.
- Introduzir camada mais explicita para autorizacao por recurso em todos os casos de uso.
- Evoluir modelo de cache com estrategia distribuida para cenarios de escala horizontal.

## 6.5 Melhorias de UX
- Filtros unificados por contexto com experiencia consistente entre paginas.
- Melhor feedback de carregamento, erro e sucesso em operacoes longas.
- Acessibilidade ampliada (teclado, contraste, leitores de tela e mensagens semanticas).

## 6.6 Melhorias de seguranca
- Refresh token com rotacao e revogacao.
- Politicas mais fortes de sessao e endurecimento adicional de CORS por ambiente.
- Bloqueio progressivo por tentativa de login e trilha de auditoria de autenticacao.

## 6.7 Melhorias de performance
- Otimizacao de consultas criticas com indices e revisao de planos no PostgreSQL.
- Cache com invalidacao mais granular para dashboard e listagens pesadas.
- Reducao de payloads e melhoria de tempo de carregamento no frontend.

## 6.8 Melhorias de observabilidade e operacao
- Logs estruturados com correlacao fim a fim entre frontend, API e banco.
- Metricas de aplicacao e alertas operacionais (latencia, erro, saturacao).
- Tracing distribuido para diagnostico de gargalos.
- Deploy automatizado mais robusto com promocoes por ambiente, validacoes e rollback.

# 7. Testes

## 7.1 Tipos de testes
- Testes unitarios (backend): foco em regras de dominio e casos de uso da camada de aplicacao.
- Testes de integracao (backend): validacao de endpoints, autenticacao/autorizacao e fluxo real com banco.
- Testes frontend: componentes, paginas, servicos e rotas com Vitest + Testing Library.

## 7.2 Como executar

Backend unitario:
```powershell
dotnet test backend/tests/GerenciadorTarefas.TestesUnitarios/GerenciadorTarefas.TestesUnitarios.csproj
```

Backend integracao (requer PostgreSQL ativo e `TESTES_INTEGRACAO_CONNECTION_STRING_ADMIN`):
```powershell
dotnet test backend/tests/GerenciadorTarefas.TestesIntegracao/GerenciadorTarefas.TestesIntegracao.csproj
```

Frontend testes:
```powershell
cd frontend
npm run test
```

Frontend cobertura:
```powershell
cd frontend
npm run test:coverage
```

Execucao consolidada (frontend + backend):
```powershell
.\scripts\testes-completo.ps1
```

Execucao consolidada sem integracao backend:
```powershell
.\scripts\testes-completo.ps1 -PularIntegracaoBackend
```

## 7.3 Cobertura

Snapshot de cobertura local (12/03/2026):
- Backend (unitarios, cobertura Cobertura via `dotnet test --collect:"XPlat Code Coverage"`):
- Geral: 65.46% linhas, 53.61% branches.
- Camada `GerenciadorTarefas.Aplicacao`: 63.84% linhas, 51.15% branches.
- Camada `GerenciadorTarefas.Dominio`: 85.45% linhas, 100% branches.
- Frontend (Vitest + V8, `npm run test:coverage`):
- Geral: 57.77% statements, 50.43% branches, 56.98% funcoes, 58.24% linhas.

Foco atual da cobertura:
- regras de negocio de tarefas (transicoes de status, validacoes e restricoes de exclusao),
- regras de autorizacao por perfil/area,
- fluxo de autenticacao JWT e validacoes principais,
- notificacoes e historico em fluxos criticos,
- servicos frontend e partes relevantes das paginas administrativas.

## 7.4 Estrategia de testes

Regras criticas priorizadas:
- autenticacao com usuario persistido e validacao de credenciais.
- limites de acesso por perfil (`SuperAdmin`, `Admin`, `Colaborador`).
- restricao de escopo por area para operacoes administrativas.
- consistencia de fluxo de tarefas (criacao, atualizacao, status e bloqueios de operacao invalida).
- comportamento de notificacoes com autorizacao baseada no usuario autenticado.

Cenarios de negocio cobertos:
- CRUD principal de projetos e tarefas.
- bloqueios de operacoes indevidas por perfil.
- validacoes de entrada em fluxos sensiveis.
- comportamentos de UI para login, dashboard e manutencao administrativa.

Limitacoes atuais:
- parte dos testes de integracao depende de PostgreSQL ativo no ambiente.
- execucao local de cobertura frontend pode variar conforme recursos do ambiente.
- cobertura de frontend ainda pode crescer em telas mais extensas.
- nao ha suite E2E completa no frontend nesta versao.

## 8 Diagrama de Arquitetura

```text
+----------------------------------------------------------------------------------------------------+
|                                         DIAGRAMA DE ARQUITETURA                                   |
|                     Gestao Colaborativa de Tarefas em Tempo Real - Projeto Completo               |
+----------------------------------------------------------------------------------------------------+

      +---------------------------+
      |         USUARIOS          |
      |---------------------------|
      | - SuperAdmin              |
      | - Admin                   |
      | - Colaborador             |
      +---------------------------+
                  |
                  | Acesso via navegador
                  v

+-------------------------------------------------------+     REST API (JSON / HTTPS)    +-------------------------------------------------------+
| FRONTEND (React + TS + Vite)                         |------------------------------->| BACKEND API (ASP.NET Core / .NET 8)                  |
|-------------------------------------------------------|                                |-------------------------------------------------------|
| React SPA                                             |                                | Program.cs / Bootstrap                               |
| App / Rotas / Layout                                  |                                | - DI                                                 |
| RotaProtegida                                         |                                | - Auth JWT                                           |
|                                                       |                                | - CORS                                               |
| +---------------------------------------------------+ |                                | - Rate Limiting                                      |
| | PAGINAS                                           | |                                | - Swagger                                            |
| |---------------------------------------------------| |                                | - SignalR                                            |
| | - PaginaLogin                                     | |                                | - Middlewares                                        |
| | - PaginaDashboard                                 | |                                +-------------------------------------------------------+
| | - PaginaProjetos                                  | |                                                     |
| | - PaginaTarefas                                   | |                                                     v
| | - PaginaUsuarios                                  | |                                +-------------------------------------------------------+
| | - PaginaAreas                                     | |                                | API / CONTROLLERS                                     |
| +---------------------------------------------------+ |                                |-------------------------------------------------------|
|                                                       |                                | - ControladorAutenticacao                            |
| +---------------------------------------------------+ | WebSocket / SignalR (Tempo Real)| - ControladorProjetos                                |
| | ESTADO E COMUNICACAO                              |---------------------------------->| - ControladorTarefas                                 |
| |---------------------------------------------------| |                                | - ControladorDashboard                               |
| | - ContextoAutenticacao                            | |                                | - ControladorNotificacoes                            |
| | - ContextoNotificacao                             | |                                | - ControladorUsuarios                                |
| | - TanStack Query                                  | |                                | - ControladorAreas                                   |
| | - clienteApi / Servicos REST                      | |                                +-------------------------------------------------------+
| | - Cliente SignalR                                 | |                                                     |
| +---------------------------------------------------+ |                                                     v
|                                                       |                                +-------------------------------------------------------+
| +---------------------------------------------------+ |                                | MIDDLEWARE / SEGURANCA / OBSERVABILIDADE            |
| | FORMULARIOS E UX                                  | |                                |-------------------------------------------------------|
| |---------------------------------------------------| |                                | - JWT + Claims + Perfis                              |
| | - React Hook Form                                 | |                                | - Policies + Escopo por Area                         |
| | - Zod                                             | |                                | - CORS                                               |
| | - Componentes / Tabelas / Cards / Graficos       | |                                | - Rate Limiting                                      |
| +---------------------------------------------------+ |                                | - Serilog + CorrelationId                            |
+-------------------------------------------------------+                                | - Health Checks (/health/live, /health/ready)       |
                                                                                         | - Cache de Consulta (IMemoryCache)                  |
                                                                                         +-------------------------------------------------------+
                                                                                                             |
                                                                                                             v
                                                                                         +-------------------------------------------------------+
                                                                                         | APLICACAO / DOMINIO / INFRAESTRUTURA                |
                                                                                         |-------------------------------------------------------|
                                                                                         | Aplicacao: CasosDeUso e Contratos                   |
                                                                                         | Dominio: Entidades, Enumeracoes, Regras             |
                                                                                         | Infraestrutura: Repositorios, EF Core, Migrations   |
                                                                                         +-------------------------------------------------------+
                                                                                                             |
                                                                                                             v
                                                                                         +-------------------------------+
                                                                                         | POSTGRESQL                    |
                                                                                         |-------------------------------|
                                                                                         | - Persistencia relacional     |
                                                                                         +-------------------------------+
```