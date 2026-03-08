# Decisões Técnicas Iniciais

## 1. Diretriz Geral
A solução será construída com foco em clareza, simplicidade e organização profissional, priorizando legibilidade e manutenção de longo prazo.

## 2. Decisões de Arquitetura
1. Adotar Clean Architecture leve.
Motivo: separar responsabilidades sem criar complexidade desnecessária.

2. Organizar backend em quatro camadas (`Domínio`, `Aplicação`, `Infraestrutura`, `API`).
Motivo: facilitar testes, evolução e isolamento de regras de negócio.

3. Organizar frontend por funcionalidades.
Motivo: melhorar coesão e escalabilidade do código da interface.

## 3. Decisões de Modelagem de Domínio
1. Entidades centrais: `Projeto` e `Tarefa`.
2. Enums explícitos: `StatusTarefa` e `PrioridadeTarefa`.
3. Regras críticas de negócio não ficarão em controllers.

## 4. Decisões de Persistência
1. Banco relacional PostgreSQL.
Motivo: consistência transacional e aderência ao escopo do desafio.

2. Acesso a dados com EF Core e repositórios.
Motivo: manter abstração adequada sem overengineering.

3. Uso de migration para versionamento de esquema.
Motivo: rastreabilidade e repetibilidade de ambiente.

## 5. Decisões de API e Qualidade
1. Swagger/OpenAPI habilitado desde a base da API.
2. Tratamento global de exceções.
3. Validações com FluentValidation.
4. Padronização de respostas e erros.
5. Logging estruturado com Serilog.

## 6. Decisões de Frontend
1. React + TypeScript + Vite.
2. TanStack Query para consumo/cache de API.
3. React Hook Form + Zod para formulários e validação.
4. Tailwind CSS para produtividade com consistência visual.

## 7. Decisões de Segurança e Tempo Real
1. Autenticação/autorização com JWT.
2. Notificações em tempo real com SignalR para atribuição e reatribuição de tarefas.

## 8. Decisões de Operação
1. Docker Compose para execução local integrada.
2. GitHub Actions para CI/CD.
3. Manifests Kubernetes básicos para demonstração de prontidão de deploy.

## 9. Padrões e Princípios
- Repository Pattern
- Services/Casos de Uso
- DTOs separados de entidades
- SOLID e Clean Code
- controllers finos

## 10. Trade-offs Assumidos Neste Momento
1. Clean Architecture leve em vez de arquitetura extremamente detalhada.
Trade-off: menor formalismo em troca de entrega mais objetiva.

2. Uso de componentes simples e reutilizáveis no frontend, sem biblioteca visual complexa.
Trade-off: menos abstrações iniciais para manter curva de manutenção baixa.

3. Unit of Work apenas se necessidade real surgir.
Trade-off: evitar complexidade antecipada.

## 11. Registro de Evolução
Este documento será atualizado ao longo dos commits para registrar decisões complementares, incluindo:
- estratégia de cache
- política de rate limiting
- decisões finais de autenticação
- decisões finais de notificações

## 12. Decisão Técnica: Status `Cancelada` como Estado Terminal
### 12.1 Regra adotada
O status `Cancelada` é tratado como estado terminal da tarefa.

Isso significa:
- transições permitidas para cancelamento: `Pendente -> Cancelada` e `EmAndamento -> Cancelada`
- não é permitido sair de `Cancelada` para qualquer outro status
- não é permitido sair de `Concluida` para qualquer outro status

### 12.2 Fluxo oficial de status
- fluxo principal: `Pendente -> EmAndamento -> Concluida`
- fluxo alternativo de encerramento: `Pendente/EmAndamento -> Cancelada`

### 12.3 Justificativa técnica
1. Preserva histórico e rastreabilidade: tarefa cancelada não deve ser reaberta por transição de status.
2. Evita ambiguidade operacional: reduz cenários de ida e volta entre estados finais.
3. Simplifica manutenção da regra no domínio: máquina de estados explícita e previsível.

### 12.4 Impacto na API
- tentativas de transição inválida resultam em erro de operação inválida (HTTP 400 pelo tratamento global)
- a validação de transição permanece centralizada no domínio, não em controllers

### 12.5 Relação com outras regras
- `DataConclusao` é preenchida automaticamente apenas em `Concluida`
- tarefas `Cancelada` não são consideradas atrasadas
