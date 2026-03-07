# Desafio Moovefy: Gestão Colaborativa de Tarefas em Tempo Real

## 1. Visão Geral
Este repositório contém a construção incremental de um sistema web corporativo para gestão de tarefas em equipe, com foco em organização profissional, regras de negócio explícitas e notificações em tempo real.

A implementação seguirá um fluxo controlado de commits, preservando arquitetura, legibilidade e consistência técnica em todas as camadas.

## 2. Objetivo do Sistema
Entregar uma solução completa com:
- backend em ASP.NET Core Web API (.NET 8)
- frontend em React com TypeScript
- banco relacional PostgreSQL
- testes unitários e de integração
- documentação técnica
- Docker, CI/CD e manifests Kubernetes básicos

## 3. Escopo Funcional
O sistema deverá oferecer:
- cadastro e gerenciamento de projetos
- cadastro e gerenciamento de tarefas
- filtros e ordenação
- dashboard com métricas
- autenticação e autorização
- notificações em tempo real ao atribuir ou reatribuir tarefas

## 4. Stack Tecnológica Definida
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
- Swagger / OpenAPI

### Arquitetura
- Clean Architecture leve
- Camadas: Domínio, Aplicação, Infraestrutura e API
- Patterns: Repository e Services/Casos de Uso

### Testes
- xUnit
- FluentAssertions
- testes unitários
- testes de integração

### Frontend
- React
- TypeScript
- Vite
- React Router
- TanStack Query
- React Hook Form
- Zod
- Tailwind CSS
- Recharts

### DevOps
- Docker Compose
- GitHub Actions
- Kubernetes (manifests básicos)

## 5. Regra de Nomenclatura
Todo o código do projeto será escrito em português, incluindo:
- classes
- métodos
- funções
- componentes
- arquivos
- variáveis
- DTOs
- enums
- serviços
- pastas

Exceções: nomes técnicos obrigatórios de frameworks, bibliotecas e configurações do ecossistema.

## 6. Estrutura Inicial do Repositório
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
        projetos/
        tarefas/
        dashboard/
        autenticacao/
        notificacoes/
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
  README.md
```

## 7. Plano Macro de Execução (42 commits)
### Fase 1 - Fundação
1. inicializar estrutura do repositório
2. documentar escopo inicial e plano macro
3. criar solução .NET em camadas
4. iniciar frontend React com TypeScript
5. configurar padronizações do repositório

### Fase 2 - Domínio e Persistência
6. entidades principais
7. enums e regras de domínio
8. contratos de repositório
9. contexto e mapeamentos do banco
10. migration inicial
11. repositórios com EF Core

### Fase 3 - API Base
12. injeção de dependência
13. Swagger
14. tratamento global de exceções
15. validações com FluentValidation
16. logging com Serilog
17. padronização de respostas e erros

### Fase 4 - Projetos
18. listar e obter projeto por id
19. criar projeto
20. atualizar projeto
21. excluir projeto com validação
22. endpoints de projetos

### Fase 5 - Tarefas
23. criar tarefa
24. listar e obter tarefa por id
25. filtros de tarefa
26. ordenação e paginação
27. atualização completa
28. alteração de status
29. regras de transição e data de conclusão
30. bloqueio de exclusão em andamento
31. sinalização de atraso
32. endpoints completos de tarefas
33. documentação da regra de cancelamento

### Fase 6 - Dashboard e Notificações
34. caso de uso de métricas
35. endpoint de métricas
36. SignalR
37. notificação em atribuição/reatribuição
38. histórico simples de notificações

### Fase 7 - Segurança e Frontend
39. autenticação/autorização com JWT
40. páginas principais e formulários
41. filtros, validações, loading, toasts e responsividade

### Fase 8 - Fechamento
42. testes, Docker, CI, cache, rate limiting, Kubernetes, seed e ajustes finais

## 8. Status Atual
- Commit 1 concluído: estrutura inicial do repositório
- Commit 2 em desenvolvimento: documentação inicial de escopo e plano

## 9. Forma de Trabalho
- Um commit por vez
- Sem avanço para funcionalidades futuras sem autorização
- Mudanças pequenas, objetivas e rastreáveis
