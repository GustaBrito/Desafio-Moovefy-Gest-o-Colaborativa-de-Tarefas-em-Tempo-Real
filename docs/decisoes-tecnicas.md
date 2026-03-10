# Decisões Técnicas

## 1. Escopo das Decisões
Este documento registra as decisões técnicas principais adotadas na implementação, os trade-offs assumidos e os impactos na manutenção do sistema.

## 2. Decisão: Clean Architecture Leve
Decisão:
- organizar backend em `Domínio`, `Aplicação`, `Infraestrutura` e `API`

Motivo:
- separar responsabilidades e manter regras de negócio isoladas

Impacto:
- melhoria de testabilidade e clareza
- aumento moderado de arquivos e contratos

## 3. Decisão: Repository Pattern sem Unit of Work Dedicado
Decisão:
- usar repositórios por agregado e persistência via EF Core
- não criar camada adicional de Unit of Work dedicada nesta fase

Motivo:
- evitar complexidade prematura

Impacto:
- solução mais direta para o escopo atual
- pode ser revisado caso surjam transações distribuídas mais complexas

## 4. Decisão: Status `Cancelada` como Estado Terminal
Decisão:
- `Cancelada` é estado final, assim como `Concluida`

Regras:
- permitido: `Pendente -> Cancelada`
- permitido: `EmAndamento -> Cancelada`
- proibido sair de `Cancelada` ou `Concluida`

Motivo:
- preservar rastreabilidade e evitar reabertura ambígua por transição

Impacto:
- transição inválida retorna erro de operação inválida (HTTP 400)

## 5. Decisão: Autenticação JWT com Usuários de Demonstração
Decisão:
- autenticação JWT com usuários definidos em configuração da API

Motivo:
- simplificar setup local e testes do desafio

Impacto:
- acelera desenvolvimento e validação manual
- não substitui integração com identidade corporativa real

Evolução futura:
- migrar para provedor externo (Identity, OAuth2/OIDC, etc.)

## 6. Decisão: Notificações com SignalR + Histórico Persistido
Decisão:
- enviar notificações em tempo real ao responsável por grupo SignalR
- persistir histórico de notificações no banco

Motivo:
- atender requisito de tempo real sem perder trilha histórica

Impacto:
- cobertura funcional para uso online e consulta posterior

## 7. Decisão: Cache de Consulta com IMemoryCache
Decisão:
- aplicar cache em listagens e consultas de projetos, tarefas e dashboard
- invalidar cache em mutações relacionadas

Motivo:
- reduzir custo de leitura e latência em endpoints consultivos

Impacto:
- melhora de desempenho em cenários de leitura repetida
- exige disciplina de invalidação para evitar dado obsoleto

## 8. Decisão: Rate Limiting Nativo do ASP.NET Core
Decisão:
- política global por janela fixa
- política mais restritiva para login

Motivo:
- proteção básica contra abuso e brute force em autenticação

Impacto:
- respostas padronizadas com HTTP 429 e `Retry-After` quando aplicável

## 9. Decisão: Seed de Dados para Demonstração
Decisão:
- inserir dados iniciais de projetos, tarefas e notificações em desenvolvimento

Motivo:
- reduzir esforço manual para demonstração e testes exploratórios

Impacto:
- onboarding mais rápido do avaliador e do time

## 10. Decisão: Docker Compose como Ambiente Local Padrão
Decisão:
- orquestrar banco, API e frontend com `docker compose`

Motivo:
- padronizar ambiente local e reduzir divergências de setup

Impacto:
- menor custo de entrada para execução do projeto

## 11. Decisão: Pipeline de CI no GitHub Actions
Decisão:
- workflow com build e testes do backend + build do frontend

Motivo:
- validar saúde básica da entrega a cada push/PR

Impacto:
- redução de regressões antes de merge

## 12. Decisão: Kubernetes Básico para Prontidão de Deploy
Decisão:
- adicionar manifests de namespace, deployments, services, configmap, secret de exemplo e pvc

Motivo:
- demonstrar preparação para execução em cluster

Impacto:
- base de deploy criada
- ainda sem ingress, autoscaling e políticas avançadas

## 13. Trade-offs Assumidos
- autenticação simplificada por configuração em vez de identidade corporativa
- Kubernetes em nível básico para foco no core funcional
- ausência de testes E2E de frontend nesta etapa
- ausência de observabilidade distribuída completa

## 14. Uso de IA no Desenvolvimento
Uso aplicado:
- apoio na estrutura inicial e automações repetitivas
- revisão de consistência arquitetural
- aceleração na documentação

Governança adotada:
- revisão manual de todo código gerado
- validação com testes e execução local
- ajustes humanos para regras de negócio e clareza

## 15. Melhorias Futuras Priorizadas
1. autenticação integrada a provedor de identidade real
2. CD com deploy automatizado por ambiente
3. observabilidade com métricas, tracing e correlação fim a fim
4. testes E2E do frontend
5. Kubernetes com ingress, HPA, probes refinadas e gestão de segredos
