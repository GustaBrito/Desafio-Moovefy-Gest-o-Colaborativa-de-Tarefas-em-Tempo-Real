# Arquitetura da Solução

## 1. Visão Geral
A solução adota uma Clean Architecture leve para separar responsabilidades, reduzir acoplamento e manter evolução segura.

Blocos principais:
- backend em camadas (`Domínio`, `Aplicação`, `Infraestrutura`, `API`)
- frontend em módulos por funcionalidade
- PostgreSQL como persistência relacional
- SignalR para comunicação em tempo real

## 1.1 Diagrama de Arquitetura (Texto)

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

## 2. Camadas do Backend

### 2.1 Domínio (`GerenciadorTarefas.Dominio`)
Responsável por:
- entidades centrais (`Usuario`, `Area`, `Projeto`, `Tarefa`, `Notificacao`)
- entidades de relacionamento (`UsuarioArea`, `ProjetoArea`, `ProjetoUsuarioVinculado`)
- enums (`PerfilGlobalUsuario`, `StatusTarefa`, `PrioridadeTarefa`)
- regras essenciais de negócio
- contratos de repositório

Regras importantes mantidas no domínio:
- fluxo de status principal: `Pendente -> EmAndamento -> Concluida`
- cancelamento explícito como estado terminal
- preenchimento automático de `DataConclusao` ao concluir tarefa
- cálculo de atraso de tarefa com base em prazo e status

### 2.2 Aplicação (`GerenciadorTarefas.Aplicacao`)
Responsável por:
- casos de uso por contexto (autenticacao, usuarios, areas, projetos, tarefas, dashboard, notificacoes)
- DTOs/modelos de entrada e saída
- orquestração entre domínio, repositórios e serviços externos

Diretriz:
- não conter detalhes de infraestrutura
- centralizar regras de aplicação e validações de fluxo

### 2.3 Infraestrutura (`GerenciadorTarefas.Infraestrutura`)
Responsável por:
- `DbContext` e mapeamentos EF Core
- migrations
- repositórios concretos
- seed de dados de demonstração

Implementações:
- persistência em PostgreSQL
- autenticação por usuário persistido com senha em hash PBKDF2
- consultas paginadas e filtradas para tarefas
- persistência de histórico de notificações

### 2.4 API (`GerenciadorTarefas.Api`)
Responsável por:
- exposição dos endpoints HTTP
- autenticação/autorização
- documentação Swagger
- middleware de tratamento global de exceções
- observabilidade e políticas transversais

Diretriz:
- controllers finos, com validações de acesso/escopo e delegação da regra principal para casos de uso

## 3. Dependências Entre Camadas
Fluxo de dependência:
- API -> Aplicação + Infraestrutura
- Aplicação -> Domínio
- Infraestrutura -> Aplicação + Domínio
- Domínio -> sem dependências das demais camadas

Essa direção garante isolamento das regras centrais e facilita testes.

## 4. Arquitetura do Frontend
Estrutura em `frontend/src`:
- `paginas`: composição de telas
- `funcionalidades`: módulos por domínio (`autenticacao`, `usuarios`, `areas`, `projetos`, `tarefas`, `dashboard`, `notificacoes`)
- `componentes`: elementos reutilizáveis
- `servicos`: cliente HTTP e chamadas de API
- `rotas`: roteamento e proteção por autenticação
- `ganchos` e `contextos`: estado e comportamento compartilhado

Padrões de UI:
- formulários com React Hook Form + Zod
- cache de dados e sincronização com TanStack Query
- destaque visual para tarefas atrasadas

## 5. Fluxos Principais

### 5.1 Login
1. frontend envia credenciais para `POST /api/autenticacao/login`
2. API valida credenciais do usuario persistido no banco (hash seguro)
3. JWT e claims de perfil/area retornados e usados nas chamadas autenticadas

### 5.2 CRUD de tarefas
1. requisição entra no controller
2. caso de uso valida entrada e aplica regra de negócio
3. repositório persiste alterações no banco
4. API retorna envelope padronizado

### 5.3 Notificação em tempo real
1. criação/atualização de responsável dispara caso de uso
2. serviço de notificações registra histórico
3. SignalR envia evento para grupo do responsável

## 6. Regras de Negócio Relevantes
- manutenção administrativa de usuários e áreas é restrita por perfil
- escopo de acesso para `Admin` e `Colaborador` respeita vínculo por área
- tarefa em `EmAndamento` não pode ser excluída
- projeto com tarefas vinculadas não pode ser excluído
- `Cancelada` é estado terminal
- responsável de tarefa deve ser usuário ativo e pertencente à área do projeto
- tarefas vencidas são sinalizadas automaticamente
- conclusão preenche `DataConclusao`

## 7. Capacidades Transversais
- autenticação/autorização com JWT
- tratamento global de exceções com envelope padronizado
- logging estruturado com Serilog
- cache de consultas com IMemoryCache e invalidação por prefixo
- rate limiting global e específico para login
- CORS configurável com fallback para ambiente local
- health checks de liveness/readiness
- correlação de requisição e métricas operacionais básicas

## 8. Observabilidade e Qualidade
- documentação OpenAPI/Swagger
- testes unitários para regras de negócio prioritárias
- testes de integração para endpoints críticos
- workflow de CI para build/testes de backend e frontend
- workflow de CD para build/publish de imagens Docker no GHCR

## 9. Deploy e Operação
- Docker Compose para ambiente local integrado
- manifests Kubernetes para namespace, banco, API, frontend e ingress HTTP
- `ConfigMap` e `Secret` para configuração de runtime no cluster

## 10. Evolução Recomendada
- adicionar deploy automatizado por ambiente usando as imagens publicadas
- adicionar tracing distribuído
- adotar secret manager
- incluir testes E2E no frontend
- incluir HPA, probes refinadas e políticas de disponibilidade no Kubernetes

## 11. Estrutura de Pastas do Projeto

```text
/
|-- backend/
|   |-- src/
|   |   |-- GerenciadorTarefas.Api/
|   |   |   |-- Configuracoes/
|   |   |   |-- Contratos/
|   |   |   |-- Controladores/
|   |   |   |-- Hubs/
|   |   |   |-- Intermediarios/
|   |   |   |-- Modelos/
|   |   |   |-- Seguranca/
|   |   |   |-- Servicos/
|   |   |   |-- Validacoes/
|   |   |   |-- Program.cs
|   |   |   `-- appsettings*.json
|   |   |-- GerenciadorTarefas.Aplicacao/
|   |   |   |-- CasosDeUso/
|   |   |   |-- Contratos/
|   |   |   `-- Modelos/
|   |   |-- GerenciadorTarefas.Dominio/
|   |   |   |-- Contratos/
|   |   |   |-- Entidades/
|   |   |   |-- Enumeracoes/
|   |   |   `-- Modelos/
|   |   `-- GerenciadorTarefas.Infraestrutura/
|   |       |-- Persistencia/
|   |       |   |-- Mapeamentos/
|   |       |   |-- Migracoes/
|   |       |   `-- Sementes/
|   |       |-- Repositorios/
|   |       `-- Seguranca/
|   |-- tests/
|   |   |-- GerenciadorTarefas.TestesUnitarios/
|   |   `-- GerenciadorTarefas.TestesIntegracao/
|   `-- GerenciadorTarefas.sln
|-- frontend/
|   |-- src/
|   |   |-- aplicacao/
|   |   |-- componentes/
|   |   |-- contextos/
|   |   |-- estilos/
|   |   |-- funcionalidades/
|   |   |-- ganchos/
|   |   |-- paginas/
|   |   |-- rotas/
|   |   |-- servicos/
|   |   |-- testes/
|   |   |-- tipos/
|   |   `-- utilitarios/
|   |-- package.json
|   |-- vite.config.ts
|   `-- vercel.json
|-- docs/
|-- kubernetes/
|-- scripts/
|-- docker-compose.yml
`-- README.md
```
