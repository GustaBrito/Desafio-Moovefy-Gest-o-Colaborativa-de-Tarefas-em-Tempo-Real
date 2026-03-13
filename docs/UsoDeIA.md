# 5. Uso de IA no Desenvolvimento (Aprofundado)

## 5.1 Objetivo e transparencia

Este documento descreve de forma objetiva como IA foi usada no projeto.

Premissas adotadas:
- IA foi suporte de produtividade.
- Decisao tecnica final foi sempre humana.
- Todo codigo proposto por IA passou por revisao e validacao.

## 5.2 Ferramentas de IA utilizadas

Ferramentas usadas durante o desenvolvimento:
- assistente de engenharia de software com foco em codigo e automacao (Codex/ChatGPT).

Uso principal:
- aceleracao de iteracoes tecnicas;
- apoio em diagnostico e organizacao de solucao;
- apoio em documentacao tecnica.

## 5.3 Onde a IA foi aplicada

### Backend

- apoio na organizacao de contratos/casos de uso e mapeamento de camadas;
- sugestoes para validacoes de entrada e resposta padronizada de API;
- apoio em investigacao de erros (autorizacao, datas, compatibilidade com PostgreSQL, fluxo de notificacoes).

### Frontend

- apoio em refino de telas e fluxos de UX (login, dashboard, projetos, tarefas, usuarios, areas);
- apoio em tipagem TypeScript e alinhamento de contratos com a API;
- apoio em tratamento de estados de erro/loading e consistencia de consumo da API.

### Testes

- apoio para estruturar e expandir testes unitarios, integracao e frontend;
- apoio em cenarios negativos e de seguranca;
- apoio para estabilizacao de testes e cobertura.

### Documentacao

- apoio na escrita e refinamento do README;
- apoio na organizacao da pasta `docs` por item exigido no desafio;
- apoio para explicitar trade-offs, execucao e limites conhecidos.

## 5.4 O que foi ajustado manualmente apos sugestoes de IA

- regras de autorizacao por perfil e escopo de area;
- contratos de request/response para compatibilidade end-to-end;
- validacoes de negocio de tarefas/projetos/usuarios;
- endurecimento de seguranca (login, claims, regras de notificacao);
- detalhes de layout e interacao das telas;
- scripts de execucao e pipelines de CI/CD.

## 5.5 Processo de governanca aplicado

Para cada sugestao de IA, foi seguido um fluxo padrao:
1. analisar a proposta e verificar aderencia ao escopo.
2. adaptar ao padrao arquitetural do repositorio.
3. validar efeitos colaterais em build, testes e execucao local.
4. revisar seguranca, autorizacao e consistencia de dados.
5. documentar impacto quando houve alteracao de contrato/comportamento.

## 5.6 Como o codigo foi validado

Validacoes tecnicas utilizadas:
- `dotnet build` no backend;
- `dotnet test` (unitarios e integracao);
- `npm run build` no frontend;
- `npm run test` e `npm run test:coverage` no frontend;
- execucao local com e sem Docker;
- validacao manual de fluxos criticos (login, projetos, tarefas, dashboard, notificacoes).

Critico:
- nenhuma sugestao de IA foi considerada pronta sem passar por validacao tecnica.

## 5.7 Limites de uso da IA

Para reduzir risco tecnico, a IA nao foi usada como fonte unica de verdade em:
- regras de negocio especificas do desafio;
- autorizacao por perfil/escopo;
- seguranca de autenticacao e notificacoes;
- decisoes arquiteturais de longo prazo.

Nesses pontos, prevaleceu revisao humana orientada por testes e execucao real.

## 5.8 Riscos conhecidos e mitigacoes

Risco: sugestao gerar codigo plausivel, mas inconsistente com o repositorio.
- mitigacao: revisao de aderencia arquitetural e contratos existentes.

Risco: falso positivo de seguranca por implementacao parcial.
- mitigacao: validacao em endpoint real + testes de permissao/escopo.

Risco: acoplamento indevido entre frontend e backend por alteracao rapida.
- mitigacao: tipagem, testes e validacao end-to-end.

## 5.9 Beneficio real obtido

Ganhos percebidos com uso responsavel de IA:
- maior velocidade de iteracao;
- melhor cobertura de cenarios de erro e regressao;
- melhor qualidade de documentacao;
- suporte para refatoracoes com menor tempo de resposta.

## 5.10 Declaracao final

O uso de IA neste projeto foi intencional, supervisionado e tecnicamente auditado.

A entrega final reflete:
- responsabilidade tecnica humana;
- validacao por build/testes/execucao;
- transparencia sobre onde IA ajudou e onde nao substituiu engenharia.
