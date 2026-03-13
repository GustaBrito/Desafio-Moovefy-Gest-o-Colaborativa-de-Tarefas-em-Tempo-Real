# 6. Melhorias Futuras (Aprofundado)

## 6.1 Objetivo deste documento

Registrar a evolucao planejada do projeto apos a entrega atual, com foco em:
- consolidacao tecnica;
- aumento de confiabilidade operacional;
- melhoria de experiencia do usuario;
- preparo para escala e manutencao de longo prazo.

Este roadmap complementa a entrega atual sem descaracterizar as decisoes ja tomadas.

## 6.2 Estado atual (base ja entregue)

O projeto ja possui:
- autenticacao JWT com usuario persistido e hash de senha;
- autorizacao por perfil e escopo de area no backend;
- SignalR para notificacoes em tempo real com historico persistido;
- cache em memoria com invalidacao por prefixo;
- rate limiting global e especifico para login;
- health checks de liveness/readiness e logging estruturado;
- CI/CD basico com build/testes/cobertura/publicacao de imagens;
- testes automatizados em backend e frontend.

As melhorias abaixo visam elevar a maturidade para um patamar de producao mais robusto.

## 6.3 O que foi simplificado para cumprir prazo

Itens deliberadamente simplificados nesta fase:
- sessao com JWT simples sem refresh token com rotacao;
- lockout/rate limit e cache em memoria local (nao distribuido);
- observabilidade sem tracing distribuido completo;
- cobertura de testes frontend sem suite E2E de ponta a ponta;
- deploy automatizado focado em CI/CD essencial, sem esteira multiambiente completa.

Racional:
- priorizar aderencia ao escopo funcional central do desafio;
- entregar base coerente e extensivel, sem refatoracao excessiva.

## 6.4 Roadmap priorizado

### Fase 1 (curto prazo): robustez imediata

| Melhoria | Problema atual | Ganho esperado | Esforco estimado |
|---|---|---|---|
| Refresh token com rotacao/revogacao | Sessao depende apenas de access token | Mais seguranca e controle de sessao | Medio |
| Hardening de CORS por ambiente | Configuracao atual ainda e relativamente ampla em dev | Menor superficie de ataque em deploy publico | Baixo |
| Politica de senha e lockout persistente/distribuido | Controle de tentativas em memoria local | Comportamento consistente em multiplas replicas | Medio |
| Padronizar auditoria de acoes administrativas | Nem toda operacao critica gera trilha de auditoria explicita | Melhor rastreabilidade e investigacao | Medio |

### Fase 2 (curto/medio prazo): qualidade de produto

| Melhoria | Problema atual | Ganho esperado | Esforco estimado |
|---|---|---|---|
| Busca avancada e paginacao server-side em todas as listagens | Escalabilidade de consultas pode degradar com volume | Melhor UX e desempenho com grandes volumes | Medio |
| Filtros salvos por usuario (visoes) | Experiencia ainda depende de filtros manuais repetitivos | Produtividade operacional no dia a dia | Medio |
| Acoes em lote com feedback transacional | Operacoes repetitivas ainda sao majoritariamente unitarias | Reducao de tempo operacional | Medio |
| Melhorias de acessibilidade (teclado, contraste, semantica) | Acessibilidade pode evoluir em telas grandes | Melhor qualidade de UX e inclusao | Baixo/Medio |

### Fase 3 (medio prazo): arquitetura e operacao

| Melhoria | Problema atual | Ganho esperado | Esforco estimado |
|---|---|---|---|
| Cache distribuido (ex.: Redis) | Cache atual e por instancia | Escala horizontal com comportamento previsivel | Medio |
| RBAC mais granular por permissao | Regras focadas em perfil + area | Controle fino para cenarios corporativos | Medio/Alto |
| Camada explicita de autorizacao por recurso | Parte das verificacoes esta concentrada em controllers/casos de uso | Maior consistencia e manutenibilidade de seguranca | Medio |
| Otimizacao de consultas criticas com indices e tuning | Crescimento de dados pode aumentar latencia | Melhor throughput e menor custo operacional | Medio |

### Fase 4 (medio/longo prazo): observabilidade e plataforma

| Melhoria | Problema atual | Ganho esperado | Esforco estimado |
|---|---|---|---|
| Metricas operacionais padronizadas (latencia, erro, saturacao) | Observabilidade ainda centrada em logs/health checks | Visao quantitativa da saude do sistema | Medio |
| Tracing distribuido ponta a ponta | Diagnostico de gargalo ainda manual em varios fluxos | Investigacao rapida de incidentes | Medio/Alto |
| Alertas e SLOs por ambiente | Monitoracao ainda nao orientada a objetivos de servico | Maior confiabilidade operacional | Medio |
| Pipeline de deploy multiambiente com validacoes e rollback | CD atual publica imagem, mas sem fluxo completo de promocao | Entrega mais segura e previsivel | Alto |

## 6.5 Melhorias por categoria

### Funcionalidades
- paginacao e busca avancada por modulo;
- visoes salvas e filtros compostos por usuario;
- historico de auditoria por entidade com diff de alteracoes.

### Arquitetura
- separar ainda mais orquestracao de autorizacao por recurso;
- evoluir padrao de cache para infraestrutura distribuida;
- maior isolamento de regras cross-cutting reutilizaveis.

### Seguranca
- refresh token com revogacao;
- politica de senha evolutiva (complexidade, expiracao opcional, historico);
- lockout persistente e rastreavel.

### Frontend/UX
- reduzir acoplamento em telas extensas com mais hooks/componentes de orquestracao;
- evoluir estados de loading/erro para experiencias mais consistentes;
- avancar em acessibilidade e navegacao por teclado.

### Testes e qualidade
- ampliar cobertura de integracao em cenarios de permissao complexa;
- consolidar testes de frontend para fluxos criticos com maior profundidade;
- adicionar E2E para caminho feliz + cenarios de falha relevantes.

### Observabilidade e operacao
- metricas, tracing e correlacao de eventos entre frontend/API/banco;
- alarmes e dashboards operacionais por ambiente;
- runbooks para incidentes comuns e degradacao parcial.

## 6.6 Indicadores para medir evolucao

Indicadores recomendados para acompanhar maturidade:
- tempo medio de resposta por endpoint critico;
- taxa de erro 4xx/5xx por modulo;
- taxa de falha de login por bloqueio e por credencial invalida;
- cobertura de testes por camada;
- lead time de deploy e taxa de rollback;
- tempo medio para identificar e corrigir incidente.

## 6.7 Sequencia recomendada de execucao

1. seguranca de sessao e hardening (refresh token, lockout distribuido, CORS por ambiente).
2. escalabilidade funcional (paginacao, busca, filtros salvos, acoes em lote).
3. arquitetura/escala (RBAC granular, cache distribuido, tuning de dados).
4. observabilidade avancada e pipeline multiambiente com rollback.

## 6.8 Resultado esperado com a evolucao

Com esse plano, a solucao evolui de uma entrega forte de desafio tecnico para um baseline mais proximo de producao corporativa, com:
- maior seguranca e governanca;
- melhor experiencia operacional para usuarios;
- melhor previsibilidade de manutencao e escala;
- melhor postura de engenharia para crescimento continuo.
