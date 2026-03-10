# Arquitetura da Solução

## 1. Visão Geral
A solução adota uma Clean Architecture leve para separar responsabilidades, reduzir acoplamento e manter evolução segura.

Blocos principais:
- backend em camadas (`Domínio`, `Aplicação`, `Infraestrutura`, `API`)
- frontend em módulos por funcionalidade
- PostgreSQL como persistência relacional
- SignalR para comunicação em tempo real

## 2. Camadas do Backend

### 2.1 Domínio (`GerenciadorTarefas.Dominio`)
Responsável por:
- entidades centrais (`Projeto`, `Tarefa`, `Notificacao`)
- enums (`StatusTarefa`, `PrioridadeTarefa`)
- regras essenciais de negócio
- contratos de repositório

Regras importantes mantidas no domínio:
- fluxo de status principal: `Pendente -> EmAndamento -> Concluida`
- cancelamento explícito como estado terminal
- preenchimento automático de `DataConclusao` ao concluir tarefa
- cálculo de atraso de tarefa com base em prazo e status

### 2.2 Aplicação (`GerenciadorTarefas.Aplicacao`)
Responsável por:
- casos de uso por contexto (projetos, tarefas, dashboard, notificações)
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
- controllers finos, sem regra de negócio

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
- `funcionalidades`: módulos por domínio (`projetos`, `tarefas`, `dashboard`, `autenticacao`, `notificacoes`)
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
2. API valida credenciais configuradas
3. JWT retornado e usado nas chamadas autenticadas

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
- tarefa em `EmAndamento` não pode ser excluída
- projeto com tarefas vinculadas não pode ser excluído
- `Cancelada` é estado terminal
- tarefas vencidas são sinalizadas automaticamente
- conclusão preenche `DataConclusao`

## 7. Capacidades Transversais
- autenticação/autorização com JWT
- tratamento global de exceções com envelope padronizado
- logging estruturado com Serilog
- cache de consultas com IMemoryCache e invalidação por prefixo
- rate limiting global e específico para login
- CORS configurável com fallback para ambiente local

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
