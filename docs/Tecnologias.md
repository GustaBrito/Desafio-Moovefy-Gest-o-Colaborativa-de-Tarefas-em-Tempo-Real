# 2. Tecnologias Utilizadas

## 2.1 Visao geral da stack

O projeto foi construido como aplicacao full stack com:
- backend em .NET 8 (ASP.NET Core Web API) com arquitetura em camadas;
- frontend em React + TypeScript com Vite;
- persistencia em PostgreSQL;
- infraestrutura conteinerizada com Docker/Docker Compose;
- pipeline de CI no GitHub Actions;
- testes automatizados para backend e frontend;
- documentacao de API via Swagger/OpenAPI.

Esta secao detalha tecnologia, versao, finalidade e evidencia no codigo.

## 2.2 Backend

### Plataforma e framework

| Tecnologia | Versao | Finalidade | Evidencia |
|---|---|---|---|
| .NET | 8 (`net8.0`) | Runtime/plataforma do backend | `backend/src/GerenciadorTarefas.Api/GerenciadorTarefas.Api.csproj` |
| ASP.NET Core Web API | 8 | Exposicao de endpoints REST, middleware e SignalR | `backend/src/GerenciadorTarefas.Api/GerenciadorTarefas.Api.csproj` |

### Principais bibliotecas

| Biblioteca | Versao | Finalidade | Evidencia |
|---|---|---|---|
| `Microsoft.EntityFrameworkCore` | 8.0.0 | ORM e acesso a dados | `backend/src/GerenciadorTarefas.Infraestrutura/GerenciadorTarefas.Infraestrutura.csproj` |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 8.0.0 | Provider EF Core para PostgreSQL | `backend/src/GerenciadorTarefas.Infraestrutura/GerenciadorTarefas.Infraestrutura.csproj` |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.0 | Suporte a migrations e tooling EF | `backend/src/GerenciadorTarefas.Api/GerenciadorTarefas.Api.csproj` e `backend/src/GerenciadorTarefas.Infraestrutura/GerenciadorTarefas.Infraestrutura.csproj` |
| `FluentValidation.AspNetCore` | 11.3.0 | Validacao de contratos/modelos | `backend/src/GerenciadorTarefas.Api/GerenciadorTarefas.Api.csproj` |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.0 | Autenticacao e autorizacao com JWT | `backend/src/GerenciadorTarefas.Api/GerenciadorTarefas.Api.csproj` |
| SignalR (ASP.NET Core) | nativo do framework | Notificacoes em tempo real | uso no projeto API + cliente `@microsoft/signalr` no frontend |
| `Serilog.AspNetCore` | 8.0.1 | Logging estruturado no backend | `backend/src/GerenciadorTarefas.Api/GerenciadorTarefas.Api.csproj` |
| `Serilog.Settings.Configuration` | 8.0.0 | Configuracao de logging via `appsettings` | `backend/src/GerenciadorTarefas.Api/GerenciadorTarefas.Api.csproj` |
| `Serilog.Sinks.Console` | 5.0.1 | Saida de logs para console/containers | `backend/src/GerenciadorTarefas.Api/GerenciadorTarefas.Api.csproj` |
| `Swashbuckle.AspNetCore` | 6.5.0 | Swagger/OpenAPI para documentacao da API | `backend/src/GerenciadorTarefas.Api/GerenciadorTarefas.Api.csproj` |

### Recursos de plataforma usados

| Recurso | Finalidade |
|---|---|
| Rate Limiting nativo ASP.NET Core | Protecao basica contra abuso de requisicoes |
| `IMemoryCache` | Cache de consultas e melhoria de latencia em endpoints de leitura |
| Middleware global | Tratamento centralizado de erros e padronizacao de resposta |

## 2.3 Frontend

### Plataforma e framework

| Tecnologia | Versao | Finalidade | Evidencia |
|---|---|---|---|
| React | `^18.3.1` | Biblioteca de UI | `frontend/package.json` |
| TypeScript | `^5.6.2` | Tipagem estatica e robustez do frontend | `frontend/package.json` |
| Vite | `^7.3.1` | Bundler e servidor de desenvolvimento | `frontend/package.json` |

### Principais bibliotecas

| Biblioteca | Versao | Finalidade | Evidencia |
|---|---|---|---|
| `react-router-dom` | `^6.28.0` | Roteamento SPA e protecao de rotas | `frontend/package.json` |
| `@tanstack/react-query` | `^5.59.15` | Estado assincrono, cache, invalidacao e sincronizacao de dados | `frontend/package.json` |
| `react-hook-form` | `^7.53.1` | Gestao de formularios | `frontend/package.json` |
| `zod` | `^3.23.8` | Schema/validacao de dados no cliente | `frontend/package.json` |
| `@hookform/resolvers` | `^3.9.1` | Integracao Zod + React Hook Form | `frontend/package.json` |
| `@microsoft/signalr` | `^10.0.0` | Cliente de notificacoes em tempo real | `frontend/package.json` |
| `recharts` | `^3.8.0` | Graficos do dashboard | `frontend/package.json` |

## 2.4 Banco de dados

| Tecnologia | Versao | Finalidade | Evidencia |
|---|---|---|---|
| PostgreSQL | 16 (imagem `16-alpine`) | Persistencia relacional principal | `docker-compose.yml` |
| Npgsql | 8.x | Conexao e provider .NET para PostgreSQL | `backend/src/GerenciadorTarefas.Infraestrutura/GerenciadorTarefas.Infraestrutura.csproj` e `backend/tests/GerenciadorTarefas.TestesIntegracao/GerenciadorTarefas.TestesIntegracao.csproj` |

## 2.5 Infra e DevOps

| Tecnologia | Finalidade | Evidencia |
|---|---|---|
| Docker | Empacotamento da API e frontend | `backend/src/GerenciadorTarefas.Api/Dockerfile`, `frontend/Dockerfile` |
| Docker Compose | Orquestracao local de `banco_dados`, `api` e `frontend` | `docker-compose.yml` |
| GitHub Actions | CI com build/teste/cobertura de backend e frontend | `.github/workflows/ci.yml` |
| Kubernetes manifests | Base para deploy em cluster (deployments/services/ingress/config) | pasta `kubernetes/` |

## 2.6 Testes

| Camada | Tecnologia | Versao | Finalidade | Evidencia |
|---|---|---|---|---|
| Backend unitario | xUnit | 2.9.0 | Testes de unidade de dominio/aplicacao | `backend/tests/GerenciadorTarefas.TestesUnitarios/GerenciadorTarefas.TestesUnitarios.csproj` |
| Backend unitario/integracao | FluentAssertions | 6.12.0 | Assercoes expressivas | projetos de teste backend |
| Backend | Microsoft.NET.Test.Sdk | 17.10.0 | Execucao de testes .NET | projetos de teste backend |
| Backend integracao | Microsoft.AspNetCore.Mvc.Testing | 8.0.0 | Hospedagem da API em testes de integracao | `backend/tests/GerenciadorTarefas.TestesIntegracao/GerenciadorTarefas.TestesIntegracao.csproj` |
| Cobertura backend | coverlet.collector | 6.0.2 | Coleta de cobertura em testes .NET | projetos de teste backend |
| Frontend | Vitest | `^4.0.18` | Runner de testes frontend | `frontend/package.json` |
| Frontend | Testing Library (`react`, `user-event`, `jest-dom`) | `^16.3.2`, `^14.6.1`, `^6.9.1` | Testes de comportamento/componentes | `frontend/package.json` |
| Frontend cobertura | `@vitest/coverage-v8` | `^4.0.18` | Coleta de cobertura frontend | `frontend/package.json` |
| Frontend ambiente teste | `jsdom` | `^28.1.0` | Simulacao de DOM para testes | `frontend/package.json` |

## 2.7 Documentacao da API

| Tecnologia | Finalidade | Evidencia |
|---|---|---|
| Swagger/OpenAPI (Swashbuckle) | Geracao e exposicao da documentacao de endpoints da API | dependencia em `GerenciadorTarefas.Api.csproj` e rota de documentacao da API |

## 2.8 Resumo de aderencia ao desafio

A stack utilizada cobre os pilares pedidos no desafio:
- backend .NET com API REST, autenticacao JWT, validacoes, logging e tempo real;
- frontend React com tipagem, formularios, roteamento e consumo de API;
- banco relacional PostgreSQL;
- execucao local com Docker Compose;
- CI automatizada com build, testes e cobertura;
- documentacao de API com Swagger/OpenAPI.
