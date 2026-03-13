import { useEffect, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { FormularioProjeto } from "../funcionalidades/projetos/FormularioProjeto";
import { useFiltrosProjetos } from "../funcionalidades/projetos/useFiltrosProjetos";
import {
  type CriterioOrdenacaoProjeto,
  type ModoVisualizacaoProjeto,
  escaparValorCsv,
  formatarDataHora,
  obterMensagemErro,
  obterNomesAreasProjeto,
  obterNomesPessoasProjeto,
} from "../funcionalidades/projetos/utilitariosProjetos";
import { usarAutenticacao } from "../ganchos/usarAutenticacao";
import { usarNotificacao } from "../ganchos/usarNotificacao";
import { listarAreas } from "../servicos/servicoAreas";
import {
  atualizarProjeto,
  criarProjeto,
  excluirProjeto,
  listarProjetos,
} from "../servicos/servicoProjetos";
import { listarUsuarios } from "../servicos/servicoUsuarios";
import type {
  AtualizarProjetoRequisicao,
  CriarProjetoRequisicao,
  ProjetoResposta,
} from "../tipos/projetos";

export function PaginaProjetos(): JSX.Element {
  const clienteConsulta = useQueryClient();
  const { ehColaborador } = usarAutenticacao();
  const { mostrarErro, mostrarSucesso } = usarNotificacao();
  const [projetoEmEdicao, setProjetoEmEdicao] = useState<ProjetoResposta | null>(null);
  const [projetoParaExcluir, setProjetoParaExcluir] = useState<ProjetoResposta | null>(null);
  const [modalProjetoAberto, setModalProjetoAberto] = useState(false);
  const [modoVisualizacao, setModoVisualizacao] =
    useState<ModoVisualizacaoProjeto>("tabela");

  useEffect(() => {
    if (!modalProjetoAberto && !projetoParaExcluir) {
      return undefined;
    }

    function tratarTeclaEscape(evento: KeyboardEvent): void {
      if (evento.key !== "Escape") {
        return;
      }

      if (projetoParaExcluir) {
        setProjetoParaExcluir(null);
        return;
      }

      setModalProjetoAberto(false);
      setProjetoEmEdicao(null);
    }

    window.addEventListener("keydown", tratarTeclaEscape);

    return () => {
      window.removeEventListener("keydown", tratarTeclaEscape);
    };
  }, [modalProjetoAberto, projetoParaExcluir]);

  const consultaProjetos = useQuery({
    queryKey: ["projetos"],
    queryFn: listarProjetos,
  });

  const consultaAreas = useQuery({
    queryKey: ["areas", "ativas"],
    queryFn: () => listarAreas(true),
    staleTime: 120000,
  });

  const consultaUsuarios = useQuery({
    queryKey: ["usuarios", "ativos"],
    queryFn: () => listarUsuarios(true),
    enabled: !ehColaborador,
    staleTime: 120000,
  });

  const mutacaoCriarProjeto = useMutation({
    mutationFn: criarProjeto,
    onSuccess: async () => {
      await clienteConsulta.invalidateQueries({ queryKey: ["projetos"] });
      mostrarSucesso("Projeto criado com sucesso.");
    },
    onError: (excecao) => {
      mostrarErro(obterMensagemErro(excecao, "Falha ao criar projeto."));
    },
  });

  const mutacaoAtualizarProjeto = useMutation({
    mutationFn: ({ id, dados }: { id: string; dados: AtualizarProjetoRequisicao }) =>
      atualizarProjeto(id, dados),
    onSuccess: async () => {
      await clienteConsulta.invalidateQueries({ queryKey: ["projetos"] });
      mostrarSucesso("Projeto atualizado com sucesso.");
    },
    onError: (excecao) => {
      mostrarErro(obterMensagemErro(excecao, "Falha ao atualizar projeto."));
    },
  });

  const mutacaoExcluirProjeto = useMutation({
    mutationFn: excluirProjeto,
    onSuccess: async () => {
      await clienteConsulta.invalidateQueries({ queryKey: ["projetos"] });
      mostrarSucesso("Projeto excluido com sucesso.");
    },
    onError: (excecao) => {
      mostrarErro(obterMensagemErro(excecao, "Falha ao excluir projeto."));
    },
  });

  async function enviarProjeto(dados: CriarProjetoRequisicao): Promise<void> {
    if (ehColaborador) {
      mostrarErro("Colaborador nao pode criar ou editar projetos.");
      return;
    }

    if (projetoEmEdicao) {
      await mutacaoAtualizarProjeto.mutateAsync({ id: projetoEmEdicao.id, dados });
      setProjetoEmEdicao(null);
      setModalProjetoAberto(false);
      return;
    }

    await mutacaoCriarProjeto.mutateAsync(dados);
    setModalProjetoAberto(false);
  }

  function abrirModalNovoProjeto(): void {
    if (ehColaborador) {
      mostrarErro("Colaborador nao pode criar projetos.");
      return;
    }

    setProjetoEmEdicao(null);
    setModalProjetoAberto(true);
  }

  function abrirModalEdicaoProjeto(projeto: ProjetoResposta): void {
    if (ehColaborador) {
      mostrarErro("Colaborador nao pode editar projetos.");
      return;
    }

    setProjetoEmEdicao(projeto);
    setModalProjetoAberto(true);
  }

  function abrirModalConfirmacaoExclusao(projeto: ProjetoResposta): void {
    if (ehColaborador) {
      mostrarErro("Colaborador nao pode excluir projetos.");
      return;
    }

    setProjetoParaExcluir(projeto);
  }

  async function confirmarExclusaoProjeto(): Promise<void> {
    if (!projetoParaExcluir) {
      return;
    }

    await mutacaoExcluirProjeto.mutateAsync(projetoParaExcluir.id);

    if (projetoEmEdicao?.id === projetoParaExcluir.id) {
      setProjetoEmEdicao(null);
      setModalProjetoAberto(false);
    }

    setProjetoParaExcluir(null);
  }

  const {
    textoBusca,
    criterioOrdenacao,
    somenteComDescricao,
    projetosFiltrados,
    setTextoBusca,
    setCriterioOrdenacao,
    setSomenteComDescricao,
    limparFiltros,
  } = useFiltrosProjetos(consultaProjetos.data ?? []);

  const colunasQuadroProjetos = useMemo(() => {
    const gruposPorArea = new Map<string, ProjetoResposta[]>();

    projetosFiltrados.forEach((projeto) => {
      const nomeArea = obterNomesAreasProjeto(projeto)[0] ?? "Sem area";
      const listaAtual = gruposPorArea.get(nomeArea) ?? [];
      listaAtual.push(projeto);
      gruposPorArea.set(nomeArea, listaAtual);
    });

    return Array.from(gruposPorArea.entries())
      .map(([areaNome, projetos]) => ({
        areaNome,
        projetos: [...projetos].sort((projetoAtual, proximoProjeto) =>
          projetoAtual.nome.localeCompare(proximoProjeto.nome, "pt-BR")
        ),
      }))
      .sort((grupoAtual, proximoGrupo) =>
        grupoAtual.areaNome.localeCompare(proximoGrupo.areaNome, "pt-BR")
      );
  }, [projetosFiltrados]);

  const totalProjetos = consultaProjetos.data?.length ?? 0;
  const totalProjetosComDescricao =
    consultaProjetos.data?.filter(
      (projeto) => Boolean(projeto.descricao && projeto.descricao.trim().length > 0)
    ).length ?? 0;
  const totalProjetosSemDescricao = totalProjetos - totalProjetosComDescricao;

  const projetoMaisRecente = useMemo(() => {
    const listaProjetos = consultaProjetos.data ?? [];

    if (listaProjetos.length === 0) {
      return null;
    }

    return [...listaProjetos].sort(
      (projetoAtual, proximoProjeto) =>
        new Date(proximoProjeto.dataCriacao).getTime() -
        new Date(projetoAtual.dataCriacao).getTime()
    )[0];
  }, [consultaProjetos.data]);

  const existemProjetosNoSistema = totalProjetos > 0;
  const existemProjetosFiltrados = projetosFiltrados.length > 0;
  const estaAtualizandoLista = consultaProjetos.isFetching && !consultaProjetos.isLoading;

  function exportarProjetosCsv(): void {
    if (!consultaProjetos.data || consultaProjetos.data.length === 0) {
      mostrarErro("Nao ha projetos para exportar.");
      return;
    }

    const cabecalho = ["id", "nome", "descricao", "data_criacao"];
    const linhas = consultaProjetos.data.map((projeto) => [
      projeto.id,
      projeto.nome,
      projeto.descricao ?? "",
      projeto.dataCriacao,
    ]);
    const conteudoCsv = [cabecalho, ...linhas]
      .map((linha) => linha.map((valor) => escaparValorCsv(valor)).join(";"))
      .join("\n");

    const arquivo = new Blob([`\uFEFF${conteudoCsv}`], {
      type: "text/csv;charset=utf-8;",
    });
    const urlArquivo = URL.createObjectURL(arquivo);
    const linkDownload = document.createElement("a");
    linkDownload.href = urlArquivo;
    linkDownload.download = `projetos-${new Date().toISOString().slice(0, 10)}.csv`;
    document.body.appendChild(linkDownload);
    linkDownload.click();
    document.body.removeChild(linkDownload);
    URL.revokeObjectURL(urlArquivo);
    mostrarSucesso("Exportacao concluida.");
  }

  return (
    <section className="pagina-conteudo pagina-projetos pagina-projetos-modernizada">
      <header className="cabecalho-pagina cabecalho-projetos">
        <div>
          <h1>Projetos</h1>
          <p>Gerencie o portfolio da equipe com mais controle e visibilidade.</p>
        </div>

        <div className="acoes-cabecalho-projetos">
          {!ehColaborador && (
            <button
              type="button"
              className="botao-secundario botao-acao-principal-projeto"
              onClick={abrirModalNovoProjeto}
            >
              + Novo projeto
            </button>
          )}

          <button
            type="button"
            className="botao-secundario"
            onClick={() => consultaProjetos.refetch()}
            disabled={consultaProjetos.isFetching}
          >
            Atualizar lista
          </button>

          <button
            type="button"
            className="botao-secundario"
            onClick={exportarProjetosCsv}
            disabled={!existemProjetosNoSistema}
          >
            Exportar CSV
          </button>
        </div>
      </header>

      {estaAtualizandoLista && (
        <p className="rotulo-atualizacao-projetos">Atualizando dados de projetos...</p>
      )}

      <section className="grade-resumo-projetos">
        <article className="cartao-resumo-projeto">
          <h3>Total de projetos</h3>
          <strong>{totalProjetos}</strong>
          <span>Base ativa no sistema</span>
        </article>

        <article className="cartao-resumo-projeto">
          <h3>Com descricao</h3>
          <strong>{totalProjetosComDescricao}</strong>
          <span>Documentacao registrada</span>
        </article>

        <article className="cartao-resumo-projeto">
          <h3>Sem descricao</h3>
          <strong>{totalProjetosSemDescricao}</strong>
          <span>Itens para complementar</span>
        </article>

        <article className="cartao-resumo-projeto">
          <h3>Ultimo cadastro</h3>
          <strong>{projetoMaisRecente?.nome ?? "Sem dados"}</strong>
          <span>
            {projetoMaisRecente
              ? formatarDataHora(projetoMaisRecente.dataCriacao)
              : "Aguardando primeiro projeto"}
          </span>
        </article>
      </section>

      <article className="cartao-filtros painel-filtros-projetos">
        <h3>Busca e organizacao</h3>

        <div className="grade-filtros grade-filtros-projetos">
          <label htmlFor="buscaProjeto">
            Buscar por nome ou descricao
            <input
              id="buscaProjeto"
              type="text"
              placeholder="Ex.: Integracao ERP"
              value={textoBusca}
              onChange={(evento) => setTextoBusca(evento.target.value)}
            />
          </label>

          <label htmlFor="ordenacaoProjeto">
            Ordenar por
            <select
              id="ordenacaoProjeto"
              value={criterioOrdenacao}
              onChange={(evento) =>
                setCriterioOrdenacao(evento.target.value as CriterioOrdenacaoProjeto)
              }
            >
              <option value="data_mais_recente">Data mais recente</option>
              <option value="data_mais_antiga">Data mais antiga</option>
              <option value="nome_ascendente">Nome (A-Z)</option>
              <option value="nome_descendente">Nome (Z-A)</option>
            </select>
          </label>
        </div>

        <label className="opcao-filtro-projeto">
          <input
            type="checkbox"
            checked={somenteComDescricao}
            onChange={(evento) => setSomenteComDescricao(evento.target.checked)}
          />
          Mostrar somente projetos com descricao
        </label>

        <button type="button" className="botao-secundario" onClick={limparFiltros}>
          Limpar filtros
        </button>
      </article>

      <article className="cartao-listagem cartao-listagem-projetos cartao-listagem-projetos-modernizado">
          <header className="cabecalho-listagem-projetos">
            <h3>Projetos cadastrados</h3>
            <div className="acoes-listagem-projetos">
              <span>{projetosFiltrados.length} encontrados</span>
              <div className="alternador-visualizacao-projetos">
                <button
                  type="button"
                  className={modoVisualizacao === "tabela" ? "ativo" : ""}
                  onClick={() => setModoVisualizacao("tabela")}
                >
                  Tabela
                </button>
                <button
                  type="button"
                  className={modoVisualizacao === "quadro" ? "ativo" : ""}
                  onClick={() => setModoVisualizacao("quadro")}
                >
                  Quadros
                </button>
              </div>
            </div>
          </header>

          {consultaProjetos.isLoading && (
            <div className="lista-esqueleto-projetos">
              <div className="bloco-esqueleto bloco-esqueleto-linha-projeto" />
              <div className="bloco-esqueleto bloco-esqueleto-linha-projeto" />
              <div className="bloco-esqueleto bloco-esqueleto-linha-projeto" />
            </div>
          )}

          {consultaProjetos.isError && (
            <div className="estado-vazio-projetos">
              <p className="mensagem-erro">
                {obterMensagemErro(consultaProjetos.error, "Falha ao carregar projetos.")}
              </p>
              <button
                type="button"
                className="botao-secundario"
                onClick={() => consultaProjetos.refetch()}
              >
                Tentar novamente
              </button>
            </div>
          )}

          {!consultaProjetos.isLoading &&
            !consultaProjetos.isError &&
            !existemProjetosNoSistema && (
              <div className="estado-vazio-projetos">
                <p>Nenhum projeto cadastrado ate o momento.</p>
                <span>Inicie cadastrando o primeiro projeto da sua carteira.</span>
              </div>
            )}

          {!consultaProjetos.isLoading &&
            !consultaProjetos.isError &&
            existemProjetosNoSistema &&
            !existemProjetosFiltrados && (
              <div className="estado-vazio-projetos">
                <p>Nenhum projeto encontrado com os filtros atuais.</p>
                <button
                  type="button"
                  className="botao-secundario"
                  onClick={limparFiltros}
                >
                  Limpar filtros
                </button>
              </div>
            )}

          {existemProjetosFiltrados && modoVisualizacao === "tabela" && (
            <div className="container-tabela-projetos">
              <table className="tabela-projetos">
                <thead>
                  <tr>
                    <th>Projeto</th>
                    <th>Areas</th>
                    <th>Pessoas</th>
                    <th>Criacao</th>
                    <th>Descricao</th>
                    <th>Acoes</th>
                  </tr>
                </thead>
                <tbody>
                  {projetosFiltrados.map((projeto) => {
                    const nomesAreas = obterNomesAreasProjeto(projeto);
                    const nomesPessoas = obterNomesPessoasProjeto(projeto);

                    return (
                      <tr key={projeto.id}>
                        <td>
                          <div className="coluna-projeto-principal">
                            <strong>{projeto.nome}</strong>
                            <span>ID: {projeto.id.slice(0, 8)}</span>
                          </div>
                        </td>
                        <td>
                          <div className="lista-selos-listagem">
                            {nomesAreas.map((nomeArea) => (
                              <span key={`${projeto.id}-${nomeArea}`} className="selo-sucesso-projeto">
                                {nomeArea}
                              </span>
                            ))}
                          </div>
                        </td>
                        <td>
                          {nomesPessoas.length > 0 ? (
                            <div className="lista-selos-listagem">
                              {nomesPessoas.map((nomePessoa) => (
                                <span
                                  key={`${projeto.id}-${nomePessoa}`}
                                  className="selo-secundario-projeto"
                                >
                                  {nomePessoa}
                                </span>
                              ))}
                            </div>
                          ) : (
                            "Sem vinculacoes"
                          )}
                        </td>
                        <td>{formatarDataHora(projeto.dataCriacao)}</td>
                        <td>{projeto.descricao || "Sem descricao."}</td>
                        <td>
                          <div className="acoes-item-listagem acoes-icones-projeto">
                            <button
                              type="button"
                              className="botao-icone-acao botao-icone-editar"
                              onClick={() => abrirModalEdicaoProjeto(projeto)}
                              disabled={mutacaoExcluirProjeto.isPending || ehColaborador}
                              aria-label={`Editar projeto ${projeto.nome}`}
                              title="Editar projeto"
                            >
                              <IconeEditar />
                            </button>
                            <button
                              type="button"
                              className="botao-icone-acao botao-icone-excluir"
                              onClick={() => abrirModalConfirmacaoExclusao(projeto)}
                              disabled={mutacaoExcluirProjeto.isPending || ehColaborador}
                              aria-label={`Excluir projeto ${projeto.nome}`}
                              title="Excluir projeto"
                            >
                              <IconeExcluir />
                            </button>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}

          {existemProjetosFiltrados && modoVisualizacao === "quadro" && (
            <div className="quadro-projetos">
              {colunasQuadroProjetos.map((grupo) => (
                <article className="coluna-quadro-projetos" key={grupo.areaNome}>
                  <header>
                    <h4>{grupo.areaNome}</h4>
                    <span>{grupo.projetos.length} projeto(s)</span>
                  </header>
                  <ul>
                    {grupo.projetos.map((projeto) => (
                      <li className="cartao-quadro-projeto" key={projeto.id}>
                        {(() => {
                          const nomesAreas = obterNomesAreasProjeto(projeto);
                          const nomesPessoas = obterNomesPessoasProjeto(projeto);

                          return (
                            <>
                              <div className="topo-item-projeto">
                                <strong>{projeto.nome}</strong>
                                {projeto.descricao ? (
                                  <span className="selo-sucesso-projeto">Descricao registrada</span>
                                ) : (
                                  <span className="selo-alerta-projeto">Sem descricao</span>
                                )}
                              </div>
                              <div className="lista-selos-listagem">
                                {nomesAreas.map((nomeArea) => (
                                  <span
                                    key={`${projeto.id}-area-${nomeArea}`}
                                    className="selo-sucesso-projeto"
                                  >
                                    {nomeArea}
                                  </span>
                                ))}
                              </div>
                              <span>{projeto.descricao || "Sem descricao."}</span>
                              {nomesPessoas.length > 0 && (
                                <small>Pessoas: {nomesPessoas.join(", ")}</small>
                              )}
                              <small>Criado em {formatarDataHora(projeto.dataCriacao)}</small>

                              <div className="acoes-item-listagem acoes-icones-projeto">
                                <button
                                  type="button"
                                  className="botao-icone-acao botao-icone-editar"
                                  onClick={() => abrirModalEdicaoProjeto(projeto)}
                                  disabled={mutacaoExcluirProjeto.isPending || ehColaborador}
                                  aria-label={`Editar projeto ${projeto.nome}`}
                                  title="Editar projeto"
                                >
                                  <IconeEditar />
                                </button>
                                <button
                                  type="button"
                                  className="botao-icone-acao botao-icone-excluir"
                                  onClick={() => abrirModalConfirmacaoExclusao(projeto)}
                                  disabled={mutacaoExcluirProjeto.isPending || ehColaborador}
                                  aria-label={`Excluir projeto ${projeto.nome}`}
                                  title="Excluir projeto"
                                >
                                  <IconeExcluir />
                                </button>
                              </div>
                            </>
                          );
                        })()}
                      </li>
                    ))}
                  </ul>
                </article>
              ))}
            </div>
          )}
      </article>

      {modalProjetoAberto && !ehColaborador && (
        <div className="sobreposicao-modal" role="presentation">
          <section
            className="cartao-modal cartao-modal-projeto"
            role="dialog"
            aria-modal="true"
            aria-labelledby="tituloModalProjeto"
          >
            <header className="cabecalho-modal">
              <h3 id="tituloModalProjeto">
                {projetoEmEdicao ? "Editar projeto" : "Novo projeto"}
              </h3>
              <button
                type="button"
                className="botao-fechar-modal"
                onClick={() => {
                  setModalProjetoAberto(false);
                  setProjetoEmEdicao(null);
                }}
                aria-label="Fechar modal de projeto"
              >
                x
              </button>
            </header>

            <FormularioProjeto
              areas={consultaAreas.data ?? []}
              pessoas={(consultaUsuarios.data ?? []).map((usuario) => ({
                id: usuario.id,
                nome: usuario.nome,
                email: usuario.email,
              }))}
              emEnvio={mutacaoCriarProjeto.isPending || mutacaoAtualizarProjeto.isPending}
              aoEnviar={async (dados) =>
                enviarProjeto({
                  nome: dados.nome,
                  descricao: dados.descricao || null,
                  areaId: dados.areaIds[0],
                  areaIds: dados.areaIds,
                  gestorUsuarioId: dados.usuarioIdsVinculados[0] ?? null,
                  usuarioIdsVinculados: dados.usuarioIdsVinculados,
                })
              }
              valoresIniciais={
                projetoEmEdicao
                  ? {
                      nome: projetoEmEdicao.nome,
                      descricao: projetoEmEdicao.descricao ?? "",
                      areaIds:
                        projetoEmEdicao.areaIds && projetoEmEdicao.areaIds.length > 0
                          ? projetoEmEdicao.areaIds
                          : [projetoEmEdicao.areaId],
                      usuarioIdsVinculados:
                        projetoEmEdicao.usuarioIdsVinculados &&
                        projetoEmEdicao.usuarioIdsVinculados.length > 0
                          ? projetoEmEdicao.usuarioIdsVinculados
                          : projetoEmEdicao.gestorUsuarioId
                            ? [projetoEmEdicao.gestorUsuarioId]
                            : [],
                    }
                  : undefined
              }
              titulo={projetoEmEdicao ? "Atualizar dados do projeto" : "Cadastrar novo projeto"}
              rotuloBotao={projetoEmEdicao ? "Salvar alteracoes" : "Criar projeto"}
              rotuloBotaoEmEnvio={
                projetoEmEdicao ? "Salvando..." : "Criando..."
              }
              aoCancelarEdicao={() => {
                setModalProjetoAberto(false);
                setProjetoEmEdicao(null);
              }}
            />
          </section>
        </div>
      )}

      {projetoParaExcluir && (
        <div className="sobreposicao-modal" role="presentation">
          <section
            className="cartao-modal cartao-modal-confirmacao"
            role="dialog"
            aria-modal="true"
            aria-labelledby="tituloModalExclusaoProjeto"
          >
            <header className="cabecalho-modal">
              <h3 id="tituloModalExclusaoProjeto">Confirmar exclusao de projeto</h3>
              <button
                type="button"
                className="botao-fechar-modal"
                onClick={() => setProjetoParaExcluir(null)}
                aria-label="Fechar confirmacao de exclusao"
              >
                x
              </button>
            </header>

            <p>
              Deseja realmente excluir o projeto <strong>{projetoParaExcluir.nome}</strong>?
            </p>
            <span className="mensagem-erro">
              Esta acao pode impactar tarefas vinculadas ao projeto.
            </span>

            <div className="linha-botoes-formulario">
              <button
                type="button"
                className="botao-perigo"
                onClick={() => {
                  void confirmarExclusaoProjeto();
                }}
                disabled={mutacaoExcluirProjeto.isPending}
              >
                {mutacaoExcluirProjeto.isPending ? "Excluindo..." : "Excluir projeto"}
              </button>
              <button
                type="button"
                className="botao-secundario"
                onClick={() => setProjetoParaExcluir(null)}
                disabled={mutacaoExcluirProjeto.isPending}
              >
                Cancelar
              </button>
            </div>
          </section>
        </div>
      )}
    </section>
  );
}

function IconeEditar(): JSX.Element {
  return (
    <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true" focusable="false">
      <path
        d="M4 20h4l10-10-4-4L4 16v4zm14.7-11.3 1.6-1.6a1 1 0 0 0 0-1.4l-2-2a1 1 0 0 0-1.4 0l-1.6 1.6 3.4 3.4z"
        fill="currentColor"
      />
    </svg>
  );
}

function IconeExcluir(): JSX.Element {
  return (
    <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true" focusable="false">
      <path
        d="M9 3h6l1 2h4v2H4V5h4l1-2zm-2 6h10l-1 11a2 2 0 0 1-2 2H10a2 2 0 0 1-2-2L7 9zm3 2v8h2v-8h-2zm4 0v8h2v-8h-2z"
        fill="currentColor"
      />
    </svg>
  );
}

