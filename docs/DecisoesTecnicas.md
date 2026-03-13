# 4. Decisoes Tecnicas (Aprofundado)

## 4.1 Objetivo deste documento

Registrar as decisoes tecnicas centrais da solucao, com foco em:
- arquitetura escolhida;
- patterns aplicados;
- trade-offs assumidos.

O objetivo e deixar claro o "por que" de cada escolha e o impacto no codigo.

## 4.2 Arquitetura escolhida

### Backend em camadas (Clean Architecture leve)

Decisao:
- organizar backend em `Dominio`, `Aplicacao`, `Infraestrutura` e `Api`.

Motivo:
- isolar regra de negocio de detalhes de framework/persistencia;
- manter controllers finos e casos de uso explicitos;
- facilitar evolucao e teste por camada.

Evidencias no codigo:
- `backend/src/GerenciadorTarefas.Dominio`
- `backend/src/GerenciadorTarefas.Aplicacao`
- `backend/src/GerenciadorTarefas.Infraestrutura`
- `backend/src/GerenciadorTarefas.Api`

Impacto:
- melhor separacao de responsabilidades e testabilidade;
- custo maior de organizacao (mais contratos e arquivos).

### Frontend modular por contexto funcional

Decisao:
- separar frontend por dominio funcional (`autenticacao`, `projetos`, `tarefas`, `usuarios`, `areas`, `dashboard`, `notificacoes`), mantendo `paginas`, `componentes`, `servicos`, `contextos` e `rotas`.

Motivo:
- reduzir acoplamento entre UI, estado e integracao com API;
- permitir evolucao incremental sem reescrever telas inteiras.

Evidencias no codigo:
- `frontend/src/funcionalidades`
- `frontend/src/paginas`
- `frontend/src/contextos`
- `frontend/src/servicos`

Impacto:
- base mais facil de manter;
- algumas telas ainda grandes exigem refino continuo de componentizacao.

## 4.3 Patterns aplicados

### Repository Pattern (principal pattern do desafio)

Decisao:
- expor contratos de repositorio no dominio e implementacoes na infraestrutura.

Motivo:
- evitar acesso direto de casos de uso ao EF Core;
- permitir mudanca de tecnologia de dados com menor impacto na aplicacao.

Evidencias no codigo:
- contratos: `backend/src/GerenciadorTarefas.Dominio/Contratos`
- implementacoes: `backend/src/GerenciadorTarefas.Infraestrutura/Repositorios`

Impacto:
- desacoplamento entre regra e persistencia;
- necessidade de manutencao dos contratos de repositorio.

### Dependency Injection (composicao de aplicacao)

Decisao:
- registrar toda a composicao em um ponto central.

Motivo:
- controlar ciclo de vida dos servicos;
- simplificar troca de implementacoes.

Evidencia:
- `backend/src/GerenciadorTarefas.Api/Configuracoes/ConfiguracaoInjecaoDependencia.cs`

Impacto:
- bootstrapping previsivel e rastreavel;
- exige disciplina para manter registrations coerentes.

### Cache-Aside para consultas

Decisao:
- usar `IMemoryCache` com chaveamento por prefixo e invalidacao apos mutacoes.

Motivo:
- reduzir latencia/custo de consultas repetidas (dashboard, projetos, tarefas).

Evidencias:
- `backend/src/GerenciadorTarefas.Api/Servicos/Cache/ServicoCacheConsultaMemoria.cs`
- `backend/src/GerenciadorTarefas.Api/Servicos/Cache/PoliticasCacheConsulta.cs`

Impacto:
- melhora de desempenho em leitura;
- risco de stale data mitigado por invalidacao explicita.

### Policy-Based Authorization

Decisao:
- combinar `[Authorize]`, policies e verificacao de escopo por area no backend.

Motivo:
- garantir que seguranca nao dependa apenas da UI;
- suportar matriz de acesso por perfil (`SuperAdmin`, `Admin`, `Colaborador`).

Evidencias:
- policies: `backend/src/GerenciadorTarefas.Api/Configuracoes/ConfiguracaoAutenticacaoJwt.cs`
- constantes: `backend/src/GerenciadorTarefas.Api/Seguranca/PoliticasAutorizacao.cs`
- aplicacao em controladores: `ControladorUsuarios`, `ControladorAreas`, `ControladorProjetos`, `ControladorTarefas`

Impacto:
- enforcement centralizado e auditavel;
- aumenta complexidade de regra de escopo em endpoints administrativos.

### Middleware Pipeline para cross-cutting concerns

Decisao:
- centralizar tratamento global de excecao, correlacao, logging, CORS, rate limiting e autenticacao no pipeline HTTP.

Motivo:
- padronizar comportamento e reduzir duplicacao nos endpoints.

Evidencia:
- `backend/src/GerenciadorTarefas.Api/Program.cs`

Impacto:
- API mais consistente;
- exige ordem correta de middlewares para evitar regressao.

## 4.4 Decisoes de autenticacao e seguranca

### JWT simples com usuarios persistidos no banco

Decisao:
- manter JWT simples (sem Identity completo/OIDC), mas autenticar usuarios persistidos com hash seguro.

Motivo:
- aderencia ao escopo com implementacao pragmatica;
- eliminar dependencia de usuarios autenticaveis hardcoded.

Evidencias:
- login controller: `backend/src/GerenciadorTarefas.Api/Controladores/ControladorAutenticacao.cs`
- servico JWT: `backend/src/GerenciadorTarefas.Api/Servicos/Autenticacao/ServicoAutenticacaoJwt.cs`
- hash de senha PBKDF2: `backend/src/GerenciadorTarefas.Infraestrutura/Seguranca/ServicoHashSenhaPbkdf2.cs`

Impacto:
- autenticacao funcional, segura e alinhada ao escopo;
- ainda sem refresh token/rotacao para cenarios enterprise.

### Claims de perfil e area no token

Decisao:
- emitir claims de usuario, perfil e area no JWT.

Motivo:
- permitir autorizacao por papel e escopo organizacional sem consulta extra em toda requisicao.

Evidencias:
- emissao de claims: `ServicoAutenticacaoJwt.cs`
- leitura de claims/contexto: `backend/src/GerenciadorTarefas.Api/Seguranca/ExtensoesClaimsPrincipal.cs`

Impacto:
- autorizacao eficiente por request;
- requer invalidacao natural por expiracao quando vinculos mudam.

### Endurecimento de login

Decisao:
- aplicar rate limit especifico no login e bloqueio temporario por tentativas falhas.

Motivo:
- reduzir risco de brute force.

Evidencias:
- rate limit login: `backend/src/GerenciadorTarefas.Api/Configuracoes/ConfiguracaoRateLimiting.cs`
- controle de tentativas: `backend/src/GerenciadorTarefas.Api/Servicos/Autenticacao/ControleTentativasLoginMemoria.cs`
- configuracao: `backend/src/GerenciadorTarefas.Api/Modelos/Autenticacao/ConfiguracaoSegurancaLogin.cs`

Impacto:
- melhora postura de seguranca;
- armazenamento em memoria e local ao processo (nao distribuido).

## 4.5 Decisoes de modelagem e persistencia

Decisao:
- usar PostgreSQL + EF Core com migrations versionadas.

Motivo:
- produtividade com relacoes, validacao de modelo e evolucao de schema.

Evidencias:
- contexto: `backend/src/GerenciadorTarefas.Infraestrutura/Persistencia/ContextoBancoDados.cs`
- migrations: `backend/src/GerenciadorTarefas.Infraestrutura/Persistencia/Migracoes`

Impacto:
- schema rastreavel e reproduzivel;
- em cenarios de performance extrema, consultas especificas podem demandar tuning SQL.

Decisao adicional:
- manter relacionamentos por IDs estaveis (usuario/projeto/area/tarefa), com nomes para exibicao.

Motivo:
- evitar inconsistencias por alteracao de nome e manter integridade referencial.

Impacto:
- maior robustez de dados e API mais previsivel.

## 4.6 Decisoes de tempo real e notificacoes

Decisao:
- usar SignalR para evento em tempo real e persistir historico de notificacoes em banco.

Motivo:
- combinar experiencia reativa com rastreabilidade historica.

Evidencias:
- hub: `backend/src/GerenciadorTarefas.Api/Hubs/HubNotificacoes.cs`
- controlador historico: `backend/src/GerenciadorTarefas.Api/Controladores/ControladorNotificacoes.cs`

Impacto:
- notificacao online + consulta posterior;
- sistema ainda sem fila dedicada para reprocessamento.

Decisao de seguranca no SignalR:
- grupo derivado do usuario autenticado, sem aceitar GUID arbitrario do cliente.

Impacto:
- reduz risco de assinatura indevida em canais de outros usuarios.

## 4.7 Decisoes de observabilidade e operacao

Decisao:
- adotar logging estruturado (Serilog), correlacao por requisicao e health checks de liveness/readiness.

Motivo:
- diagnostico mais rapido e operacao mais confiavel.

Evidencias:
- observabilidade: `backend/src/GerenciadorTarefas.Api/Configuracoes/ConfiguracaoObservabilidade.cs`
- pipeline: `backend/src/GerenciadorTarefas.Api/Program.cs`
- configuracao de logs: `backend/src/GerenciadorTarefas.Api/appsettings*.json`

Impacto:
- melhora visibilidade operacional;
- tracing distribuido ainda nao implementado.

Decisao de entrega:
- manter CI para build/testes/cobertura e CD para imagens no GHCR.

Evidencias:
- `.github/workflows/ci.yml`
- `.github/workflows/cd.yml`

## 4.8 Trade-offs assumidos

1. JWT simples em vez de stack completa de identidade.
- ganho: menor complexidade e aderencia ao prazo.
- custo: sem refresh token robusto, revogacao e federacao.

2. EF Core em vez de micro-ORM.
- ganho: produtividade, migrations e relacoes complexas com menos codigo.
- custo: tuning fino de consulta pode exigir intervencao pontual.

3. Controle de lockout e cache em memoria local.
- ganho: implementacao direta e suficiente para desafio.
- custo: sem compartilhamento entre replicas (nao distribuido).

4. SignalR direto sem broker de mensageria.
- ganho: baixo overhead para tempo real.
- custo: menor resiliencia para cenarios de alto volume/reprocessamento.

5. Frontend com Context + React Query (sem Redux).
- ganho: menor boilerplate com boa separacao entre sessao global e estado assincrono.
- custo: pode exigir evolucao adicional em cenarios de maior complexidade global.

## 4.9 Alternativas nao adotadas nesta fase

- ASP.NET Identity completo com fluxo de conta/refresh token.
- OAuth2/OIDC com provedor externo.
- cache distribuido (Redis).
- fila dedicada para notificacoes (ex.: RabbitMQ/Kafka).
- tracing distribuido completo (OpenTelemetry + collector).

Motivo de nao adocao:
- manter foco no escopo funcional principal e previsibilidade de entrega.

## 4.10 Direcao de evolucao recomendada

1. Evoluir seguranca de sessao (refresh token com rotacao/revogacao).
2. Externalizar lockout/cache para mecanismo distribuido.
3. Ampliar observabilidade com metricas e tracing distribuido.
4. Expandir testes E2E frontend e cenarios de carga backend.
5. Adotar deploy por ambiente com validacoes e rollback automatizado.
