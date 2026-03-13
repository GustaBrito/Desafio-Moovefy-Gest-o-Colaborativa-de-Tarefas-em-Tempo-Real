# 1. Descricao do Projeto (Aprofundada)

## 1.1 Visao Executiva
O projeto implementa um sistema full stack para gestao de tarefas com notificacoes, com foco em organizacao operacional de equipes, rastreabilidade e controle por perfil de acesso.

A solucao entrega um fluxo completo de trabalho: autenticacao de usuario, gestao de projetos, gestao de tarefas, visao gerencial por dashboard e notificacoes em tempo real para eventos relevantes.

## 1.2 Contexto do Problema
Em operacoes sem uma plataforma centralizada, problemas recorrentes tendem a ocorrer:
- baixa visibilidade do andamento das tarefas;
- dificuldade para saber responsavel, prazo e prioridade de cada entrega;
- comunicacao fragmentada entre time tecnico e gestao;
- pouco historico para auditoria e acompanhamento;
- dificuldade para separar o que cada perfil pode ou nao administrar.

O sistema foi desenhado para reduzir esses pontos com fluxo unificado, dados consistentes e regras de autorizacao no backend.

## 1.3 Objetivos do Projeto
Objetivo geral:
- disponibilizar uma plataforma de gestao de tarefas e projetos com notificacoes em tempo real e controle de acesso por perfil/escopo.

Objetivos especificos:
- manter cadastro e manutencao de projetos e tarefas em um unico ambiente;
- garantir atribuicao de tarefas para usuarios validos;
- permitir acompanhamento por metricas operacionais no dashboard;
- aplicar autorizacao por perfil e area de forma consistente;
- registrar historico de notificacoes para rastreabilidade;
- fornecer base tecnica executavel com testes e documentacao.

## 1.4 Publico e Perfis de Usuario
Perfis do sistema:
- `SuperAdmin`: acesso global de administracao.
- `Admin`: administracao limitada ao proprio escopo.
- `Colaborador`: atuacao operacional em tarefas permitidas, sem manutencao administrativa de usuarios/areas.

Publico alvo:
- equipes tecnicas e de negocio que precisam coordenar entregas;
- liderancas que acompanham prazo, status e capacidade de execucao;
- responsaveis por operacao e governanca de acesso.

## 1.5 Escopo Funcional de Ponta a Ponta
Modulos principais:
- autenticacao e sessao
- projetos
- tarefas
- dashboard
- notificacoes
- administracao (usuarios e areas)

Fluxo principal:
1. usuario autentica e recebe permissao conforme perfil;
2. usuario consulta ou administra projetos dentro do escopo permitido;
3. tarefas sao criadas/atualizadas e atribuida a responsaveis validos;
4. dashboard consolida indicadores para acompanhamento;
5. notificacoes em tempo real e historico registram eventos relevantes.

## 1.6 Regras de Negocio Relevantes
- tarefas devem manter responsavel por identificador estavel de usuario;
- atribuicao de tarefa exige validacao de usuario ativo e escopo permitido;
- autorizacao de manutencao administrativa deve ser aplicada no backend;
- consulta de historico de notificacoes respeita usuario autenticado e politica de acesso;
- comunicacao em tempo real usa contexto autenticado para grupos de notificacao.

## 1.7 Escopo Fora da Entrega (Nao Objetivos desta Fase)
- adocao de ASP.NET Identity completo com federacao externa;
- implementacao de OAuth/OpenID Connect com provedor terceiro;
- suite E2E completa de frontend;
- plataforma completa de observabilidade distribuida com tracing de producao.

Esses pontos ficam mapeados como evolucao futura para nao comprometer o foco da entrega principal.

## 1.8 Valor Entregue
Valor funcional:
- centralizacao da operacao de projetos e tarefas;
- melhor comunicacao de mudancas por notificacoes em tempo real;
- mais previsibilidade de entrega via acompanhamento de status e prazo.

Valor tecnico:
- arquitetura separada por camadas no backend;
- frontend modular por funcionalidade;
- base com testes, documentacao e pipeline de CI.

## 1.9 Criterios de Sucesso da Entrega
- sistema executa localmente com configuracao documentada;
- usuarios conseguem autenticar e operar conforme perfil;
- projetos e tarefas podem ser mantidos com regras de autorizacao ativas;
- dashboard exibe indicadores coerentes com os dados;
- notificacoes funcionam em tempo real e com historico consultavel.

## 1.10 Resumo Final
Esta entrega prioriza um nucleo funcional robusto e coerente com o desafio tecnico: gestao de tarefas com controle de acesso, visao operacional e notificacoes, sustentada por arquitetura organizada e pronta para evolucao incremental.
