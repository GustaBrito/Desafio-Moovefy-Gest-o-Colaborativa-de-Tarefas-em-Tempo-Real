# 3. Como Executar (Aprofundado)

## 3.1 Objetivo desta documentacao

Este guia padroniza a execucao do projeto em diferentes cenarios:
- com Docker;
- sem Docker;
- modo desenvolvimento;
- modo producao.

Premissa importante:
- em todos os cenarios, a aplicacao depende de banco PostgreSQL ativo e acessivel.

## 3.2 Pre-requisitos

Obrigatorios:
- .NET SDK `8.0.418` (arquivo `global.json`);
- Node.js `20.x` + npm;
- Docker Desktop + Docker Compose (para fluxo containerizado);
- acesso a PostgreSQL (container ou instancia externa).

Opcionais:
- `dotnet-ef` para operacoes manuais de migration;
- cliente SQL (DBeaver, pgAdmin Desktop, Azure Data Studio).

Instalacao opcional do `dotnet-ef`:

```powershell
dotnet tool install --global dotnet-ef
```

## 3.3 Padrao de host e portas

Padrao adotado para desenvolvimento local:
- API: `http://localhost:5258`
- Frontend: `http://localhost:5173`
- Banco PostgreSQL (host): `localhost:55433`
- Swagger: `http://localhost:5258/documentacao`
- SignalR Hub: `http://localhost:5258/hubs/notificacoes`
- Health checks:
  - `http://localhost:5258/health/live`
  - `http://localhost:5258/health/ready`

Esse padrao esta refletido em:
- `scripts/dev-sem-docker-api.ps1`
- `scripts/dev-frontend.ps1`
- `.env.example`
- `frontend/.env.example`
- `backend/src/GerenciadorTarefas.Api/Properties/launchSettings.json`

## 3.4 Variaveis de ambiente e configuracoes

Arquivos base:
- `.env.example`
- `.env.compose.example`
- `frontend/.env.example`
- `backend/src/GerenciadorTarefas.Api/appsettings.json`
- `backend/src/GerenciadorTarefas.Api/appsettings.Development.json`

Variaveis criticas:
- `ConnectionStrings__BancoDados`: conexao com PostgreSQL.
- `AutenticacaoJwt__ChaveSecreta`: chave JWT (minimo recomendado: 32+ caracteres).
- `ASPNETCORE_ENVIRONMENT`: `Development` ou `Production`.
- `ASPNETCORE_URLS`: host/porta de bind da API.
- `BancoDados__AplicarMigracoesAutomaticamente`: aplica migrations no startup.
- `BancoDados__AplicarSeedDadosDemonstracao`: aplica seed no startup.
- `VITE_URL_API`: URL base da API consumida pelo frontend.
- `Cors__OrigensPermitidas__0..N`: origens permitidas para frontend.

## 3.5 Execucao com Docker (stack completa)

### Passo a passo

1. Criar arquivo de ambiente:

```powershell
Copy-Item .env.example .env
```

2. Subir toda a stack:

```powershell
docker compose --env-file .env up -d --build
```

Alternativa via script:

```powershell
.\scripts\dev-com-docker.ps1
```

3. Verificar status dos containers:

```powershell
docker compose ps
```

Servicos iniciados:
- `banco_dados` (postgres:16-alpine);
- `api` (.NET 8);
- `frontend` (Nginx servindo build Vite).

### Validacao rapida

- Abrir frontend: `http://localhost:5173`
- Abrir Swagger: `http://localhost:5258/documentacao`
- Testar health readiness: `http://localhost:5258/health/ready`

## 3.6 Execucao sem Docker (apps locais)

### Cenario A: banco local instalado externamente

1. Garantir PostgreSQL ativo.
2. Subir API:

```powershell
.\scripts\dev-sem-docker-api.ps1
```

3. Em outro terminal, subir frontend:

```powershell
.\scripts\dev-frontend.ps1
```

### Cenario B: somente banco em container, apps locais

1. Subir apenas banco:

```powershell
docker compose up -d banco_dados
```

2. Subir API local:

```powershell
.\scripts\dev-sem-docker-api.ps1
```

3. Subir frontend local:

```powershell
.\scripts\dev-frontend.ps1
```

### Parametros uteis dos scripts

API local (`dev-sem-docker-api.ps1`):
- `-Host` (padrao `localhost`)
- `-PortaApi` (padrao `5258`)
- `-ConnectionString`
- `-AplicarMigracoesAutomaticamente` (padrao `true`)
- `-AplicarSeedDadosDemonstracao` (padrao `true`)

Frontend local (`dev-frontend.ps1`):
- `-ApiUrl` (padrao `http://localhost:5258`)
- `-Host` (padrao `localhost`)
- `-PortaFrontend` (padrao `5173`)

## 3.7 Modo desenvolvimento vs producao

### Desenvolvimento

No modo `Development`:
- `UseHttpsRedirection` nao e forcado;
- CORS de dev permite origens locais configuradas;
- seed de demonstracao pode ser habilitado;
- logs tendem a ser mais verbosos.

Execucao recomendada:
- via scripts `dev-sem-docker-api.ps1` + `dev-frontend.ps1`;
- ou stack completa com `docker compose`.

### Producao

No modo `Production`:
- `UseHttpsRedirection` e aplicado;
- seed de demonstracao deve ficar desabilitado;
- chave JWT obrigatoriamente forte;
- conexao de banco deve apontar para ambiente persistente.

Script dedicado para API:

```powershell
.\scripts\producao-api.ps1 `
  -ConnectionString "Host=SEU_HOST;Port=5432;Database=SEU_DB;Username=SEU_USER;Password=SEU_PASS" `
  -ChaveJwt "SUA_CHAVE_JWT_SUPER_FORTE_COM_32_OU_MAIS_CARACTERES" `
  -Host "0.0.0.0" `
  -PortaApi 8080 `
  -AplicarMigracoesAutomaticamente $false
```

## 3.8 Migrations e seed

Em desenvolvimento, os scripts podem aplicar migrations/seed automaticamente.

Execucao manual de migrations:

```powershell
dotnet ef database update `
  --project backend/src/GerenciadorTarefas.Infraestrutura/GerenciadorTarefas.Infraestrutura.csproj `
  --startup-project backend/src/GerenciadorTarefas.Api/GerenciadorTarefas.Api.csproj
```

Ordem recomendada de subida:
1. Banco;
2. API;
3. Frontend.

## 3.9 Credenciais seed (quando habilitado)

Com `BancoDados__AplicarSeedDadosDemonstracao=true`, o ambiente sobe com usuarios de exemplo:
- `superadmin@gerenciadortarefas.local` / `SuperAdmin@123`
- `admin.marketing@gerenciadortarefas.local` / `AdminMarketing@123`
- `admin.dev@gerenciadortarefas.local` / `AdminDev@123`
- `admin.gestao@gerenciadortarefas.local` / `AdminGestao@123`
- `colaborador.dev@gerenciadortarefas.local` / `ColaboradorDev@123`
- `colaborador.marketing@gerenciadortarefas.local` / `ColaboradorMkt@123`

## 3.10 Preparacao para deploy do frontend na Vercel

O frontend ja possui configuracao base para Vercel em:
- `frontend/vercel.json`

Fluxo recomendado:
1. Publicar API em endpoint HTTPS acessivel externamente.
2. Configurar CORS da API para a URL publica da Vercel.
3. Definir no projeto Vercel a variavel:
   - `VITE_URL_API=https://sua-api-publica`
4. Executar deploy do frontend apontando para pasta `frontend`.

Observacao importante:
- Vercel hospedara apenas o frontend React;
- o backend .NET e o PostgreSQL devem estar em plataforma compativel (container host, VM, PaaS etc.).

## 3.11 Checklist rapido de validacao

Antes de testar funcionalidade:
1. Banco esta ativo e acessivel pela `ConnectionString`.
2. API responde em `/documentacao` e `/health/ready`.
3. Frontend abre sem erro e aponta para `VITE_URL_API` correta.
4. Login funciona com credenciais seed (se seed habilitado).
5. Notificacoes em tempo real conectam apos login.

## 3.12 Troubleshooting rapido

Erro de Docker engine (pipe `dockerDesktopLinuxEngine` inexistente):
- iniciar/reiniciar Docker Desktop;
- validar com `docker version`;
- subir novamente com `docker compose up -d --build`.

Frontend sem conexao com API:
- conferir `VITE_URL_API`;
- conferir CORS (`Cors:OrigensPermitidas`);
- conferir se API esta realmente em execucao na porta esperada.

Toast de falha na mensageria em tempo real:
- confirmar endpoint `/hubs/notificacoes`;
- confirmar token JWT valido na sessao;
- validar se URL base da API no frontend esta correta.
