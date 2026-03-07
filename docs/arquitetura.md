# Arquitetura da Solução

## 1. Visão Arquitetural
A solução seguirá uma Clean Architecture leve, com separação clara de responsabilidades e baixo acoplamento entre camadas.

Camadas principais:
- Domínio
- Aplicação
- Infraestrutura
- API

No frontend, a organização será por funcionalidades para manter alta coesão e evolução previsível.

## 2. Backend
### 2.1 Domínio (`GerenciadorTarefas.Dominio`)
Responsável por:
- entidades centrais (`Projeto`, `Tarefa`)
- enums de domínio (`StatusTarefa`, `PrioridadeTarefa`)
- regras essenciais do negócio
- contratos que representem necessidades do domínio

Diretrizes:
- não depender de infraestrutura
- concentrar regras de negócio críticas

### 2.2 Aplicação (`GerenciadorTarefas.Aplicacao`)
Responsável por:
- casos de uso
- DTOs de entrada e saída
- orquestração de regras entre domínio e persistência
- validações de aplicação

Diretrizes:
- não conter detalhes de acesso a dados concretos
- manter serviços/casos de uso pequenos e objetivos

### 2.3 Infraestrutura (`GerenciadorTarefas.Infraestrutura`)
Responsável por:
- acesso ao banco PostgreSQL
- implementação dos repositórios
- configurações do EF Core e mapeamentos
- integrações técnicas (cache, autenticação, notificações e afins)

Diretrizes:
- encapsular detalhes de framework
- expor implementações por contratos

### 2.4 API (`GerenciadorTarefas.Api`)
Responsável por:
- exposição dos endpoints HTTP
- composição da aplicação (injeção de dependência)
- autenticação/autorização
- middlewares transversais (erros, logging, rate limiting)
- documentação OpenAPI/Swagger

Diretrizes:
- controllers finos
- sem regra de negócio na camada de API

## 3. Frontend
Estrutura principal em `frontend/src`:
- `paginas`: pontos de entrada de tela
- `componentes`: componentes reutilizáveis
- `funcionalidades`: módulos por contexto (`projetos`, `tarefas`, `dashboard`, `autenticacao`, `notificacoes`)
- `servicos`: acesso à API
- `ganchos`: hooks customizados
- `contextos`: contexto global quando necessário
- `rotas`: roteamento da aplicação
- `tipos`: contratos de tipos
- `utilitarios`: funções puras de apoio
- `estilos`: estilos globais e tokens

Diretrizes:
- componentização por feature
- validação de formulários com Zod
- cache e sincronização com TanStack Query

## 4. Fluxo de Dependência
Regras de dependência no backend:
- API depende de Aplicação e Infraestrutura
- Infraestrutura depende de Aplicação e Domínio
- Aplicação depende de Domínio
- Domínio não depende de outras camadas

## 5. Regras de Negócio Prioritárias
As seguintes regras serão tratadas como centrais no domínio/aplicação:
- fluxo de status da tarefa (`Pendente -> EmAndamento -> Concluida`)
- preenchimento automático de data de conclusão
- bloqueio de exclusão de tarefa em andamento
- bloqueio de exclusão de projeto com tarefas vinculadas
- sinalização de tarefas vencidas
- tratamento explícito de `Cancelada` como decisão técnica documentada

## 6. Observabilidade, Segurança e Escalabilidade
Capacidades previstas:
- logging estruturado com Serilog
- autenticação/autorização com JWT
- notificações em tempo real com SignalR
- cache com IMemoryCache
- rate limiting nativo do ASP.NET Core

## 7. Estratégia de Evolução
A implementação seguirá fluxo incremental em commits pequenos e rastreáveis, reduzindo risco de regressão e facilitando revisão técnica contínua.
