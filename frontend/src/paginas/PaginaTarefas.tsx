import { useEffect, useMemo, useRef, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { FormularioTarefa } from "../funcionalidades/tarefas/FormularioTarefa";
import { TabelaTarefasOperacional } from "../funcionalidades/tarefas/TabelaTarefasOperacional";
import {
  nomesCamposOrdenacao,
  nomesDirecaoOrdenacao,
  nomesStatus,
  opcoesChipsRapidos,
} from "../funcionalidades/tarefas/configuracoesFiltrosTarefas";
import { useFiltrosTarefas } from "../funcionalidades/tarefas/useFiltrosTarefas";
import {
  converterIsoParaDataInput,
  escaparValorCsv,
  normalizarTexto,
  obterMensagemErro,
  obterStatusPermitidos,
  statusPermitido,
  tarefaVenceHoje,
} from "../funcionalidades/tarefas/utilitariosTarefas";
import { usarAutenticacao } from "../ganchos/usarAutenticacao";
import { usarNotificacao } from "../ganchos/usarNotificacao";
import { listarProjetos } from "../servicos/servicoProjetos";
import { listarUsuarios } from "../servicos/servicoUsuarios";
import {
  atualizarTarefa,
  atualizarStatusTarefa,
  criarTarefa,
  excluirTarefa,
  listarTarefas,
} from "../servicos/servicoTarefas";
import {
  CampoOrdenacaoTarefa,
  DirecaoOrdenacaoTarefa,
  StatusTarefa,
  type TarefaResposta,
} from "../tipos/tarefas";

interface CartaoResumoTarefas {
  titulo: string;
  valor: number;
  subtitulo: string;
}

export function PaginaTarefas(): JSX.Element {
  const clienteConsulta = useQueryClient();
  const { sessao, ehColaborador } = usarAutenticacao();
  const { historicoNotificacoes, mostrarErro, mostrarInformacao, mostrarSucesso } =
    usarNotificacao();
  const assinaturaAnteriorTarefas = useRef("");
  const {
    projetoIdFiltro,
    statusFiltro,
    responsavelUsuarioIdFiltro,
    dataPrazoInicialFiltro,
    dataPrazoFinalFiltro,
    campoOrdenacao,
    direcaoOrdenacao,
    numeroPagina,
    tamanhoPagina,
    textoBusca,
    chipsAtivos,
    filtroConsulta,
    setProjetoIdFiltro,
    setStatusFiltro,
    setResponsavelUsuarioIdFiltro,
    setDataPrazoInicialFiltro,
    setDataPrazoFinalFiltro,
    setCampoOrdenacao,
    setDirecaoOrdenacao,
    setNumeroPagina,
    setTamanhoPagina,
    setTextoBusca,
    alternarChipRapido,
    alternarOrdenacaoPorCabecalho,
    limparFiltros: limparFiltrosBase,
  } = useFiltrosTarefas();
  const [idsSelecionados, setIdsSelecionados] = useState<string[]>([]);
  const [statusLote, setStatusLote] = useState("");
  const [tarefaEmEdicao, setTarefaEmEdicao] = useState<TarefaResposta | null>(null);
  const [modalTarefaAberto, setModalTarefaAberto] = useState(false);
  const [tarefaParaExcluir, setTarefaParaExcluir] = useState<TarefaResposta | null>(null);
  const [executandoAcaoLote, setExecutandoAcaoLote] = useState(false);
  const [momentoUltimaSincronizacao, setMomentoUltimaSincronizacao] =
    useState<Date | null>(null);

  const campoBuscaRef = useRef<HTMLInputElement | null>(null);

  const consultaProjetos = useQuery({
    queryKey: ["projetos"],
    queryFn: listarProjetos,
    staleTime: 120000,
  });

  const consultaTarefas = useQuery({
    queryKey: ["tarefas", filtroConsulta],
    queryFn: () => listarTarefas(filtroConsulta),
    placeholderData: (dadosAnteriores) => dadosAnteriores,
  });

  const consultaUsuarios = useQuery({
    queryKey: ["usuarios", "opcoes", "ativos"],
    queryFn: () => listarUsuarios(true),
    enabled: !ehColaborador,
    staleTime: 120000,
  });

  const mutacaoCriarTarefa = useMutation({
    mutationFn: criarTarefa,
    onSuccess: async () => {
      await Promise.all([
        clienteConsulta.invalidateQueries({ queryKey: ["tarefas"] }),
        clienteConsulta.invalidateQueries({ queryKey: ["dashboard"] }),
      ]);
      mostrarSucesso("Tarefa criada com sucesso.");
    },
    onError: (excecao) => {
      mostrarErro(obterMensagemErro(excecao, "Falha ao criar tarefa."));
    },
  });

  const mutacaoAtualizarStatus = useMutation({
    mutationFn: ({ id, status }: { id: string; status: StatusTarefa }) =>
      atualizarStatusTarefa(id, { status }),
    onSuccess: async () => {
      await Promise.all([
        clienteConsulta.invalidateQueries({ queryKey: ["tarefas"] }),
        clienteConsulta.invalidateQueries({ queryKey: ["dashboard"] }),
      ]);
      mostrarSucesso("Status da tarefa atualizado.");
    },
    onError: (excecao) => {
      mostrarErro(
        obterMensagemErro(excecao, "Falha ao atualizar status da tarefa.")
      );
    },
  });

  const mutacaoAtualizarTarefa = useMutation({
    mutationFn: ({ id, dados }: { id: string; dados: Parameters<typeof atualizarTarefa>[1] }) =>
      atualizarTarefa(id, dados),
    onSuccess: async () => {
      await Promise.all([
        clienteConsulta.invalidateQueries({ queryKey: ["tarefas"] }),
        clienteConsulta.invalidateQueries({ queryKey: ["dashboard"] }),
      ]);
      mostrarSucesso("Tarefa atualizada com sucesso.");
    },
    onError: (excecao) => {
      mostrarErro(obterMensagemErro(excecao, "Falha ao atualizar tarefa."));
    },
  });

  const mutacaoExcluirTarefa = useMutation({
    mutationFn: excluirTarefa,
    onSuccess: async () => {
      await Promise.all([
        clienteConsulta.invalidateQueries({ queryKey: ["tarefas"] }),
        clienteConsulta.invalidateQueries({ queryKey: ["dashboard"] }),
      ]);
      mostrarSucesso("Tarefa excluida com sucesso.");
    },
    onError: (excecao) => {
      mostrarErro(obterMensagemErro(excecao, "Falha ao excluir tarefa."));
    },
  });

  const mapaProjetos = useMemo(
    () => new Map((consultaProjetos.data ?? []).map((projeto) => [projeto.id, projeto.nome])),
    [consultaProjetos.data]
  );

  const usuariosResponsaveis = useMemo(() => {
    if (ehColaborador) {
      if (!sessao) {
        return [];
      }

      return [
        {
          id: sessao.usuarioId,
          nome: sessao.nome,
          email: sessao.email,
        },
      ];
    }

    return (consultaUsuarios.data ?? []).map((usuario) => ({
      id: usuario.id,
      nome: usuario.nome,
      email: usuario.email,
    }));
  }, [consultaUsuarios.data, ehColaborador, sessao]);

  const tarefasServidor = consultaTarefas.data?.itens ?? [];

  const tarefasFiltradas = useMemo(() => {
    const textoBuscaNormalizado = normalizarTexto(textoBusca);

    return tarefasServidor.filter((tarefa) => {
      if (textoBuscaNormalizado) {
        const tituloNormalizado = normalizarTexto(tarefa.titulo);
        const descricaoNormalizada = normalizarTexto(tarefa.descricao ?? "");

        if (
          !tituloNormalizado.includes(textoBuscaNormalizado) &&
          !descricaoNormalizada.includes(textoBuscaNormalizado)
        ) {
          return false;
        }
      }

      if (chipsAtivos.includes("atrasadas") && !tarefa.estaAtrasada) {
        return false;
      }

      if (chipsAtivos.includes("vence_hoje") && !tarefaVenceHoje(tarefa)) {
        return false;
      }

      if (chipsAtivos.includes("urgentes") && tarefa.prioridade !== 4) {
        return false;
      }

      return true;
    });
  }, [chipsAtivos, tarefasServidor, textoBusca]);

  const cartoesResumo = useMemo<CartaoResumoTarefas[]>(() => {
    const total = tarefasFiltradas.length;
    const atrasadas = tarefasFiltradas.filter((tarefa) => tarefa.estaAtrasada).length;
    const emAndamento = tarefasFiltradas.filter(
      (tarefa) => tarefa.status === StatusTarefa.EmAndamento
    ).length;
    const concluidasNoPrazo = tarefasFiltradas.filter(
      (tarefa) =>
        tarefa.status === StatusTarefa.Concluida &&
        Boolean(tarefa.dataConclusao) &&
        new Date(tarefa.dataConclusao ?? 0).getTime() <=
          new Date(tarefa.dataPrazo).getTime()
    ).length;

    return [
      { titulo: "Total no recorte", valor: total, subtitulo: "Tarefas visiveis" },
      { titulo: "Atrasadas", valor: atrasadas, subtitulo: "Demandas vencidas" },
      { titulo: "Em andamento", valor: emAndamento, subtitulo: "Execucao ativa" },
      { titulo: "Concluidas no prazo", valor: concluidasNoPrazo, subtitulo: "Qualidade" },
    ];
  }, [tarefasFiltradas]);

  const totalPaginas = consultaTarefas.data?.totalPaginas ?? 1;
  const totalRegistros = consultaTarefas.data?.totalRegistros ?? 0;
  const semResultados =
    !consultaTarefas.isLoading && !consultaTarefas.isError && tarefasFiltradas.length === 0;

  useEffect(() => {
    const idsVisiveis = new Set(tarefasFiltradas.map((tarefa) => tarefa.id));
    setIdsSelecionados((idsAtuais) => idsAtuais.filter((id) => idsVisiveis.has(id)));
  }, [tarefasFiltradas]);

  useEffect(() => {
    if (!modalTarefaAberto && !tarefaParaExcluir) {
      return undefined;
    }

    function tratarTeclaEscape(evento: KeyboardEvent): void {
      if (evento.key !== "Escape") {
        return;
      }

      if (tarefaParaExcluir) {
        setTarefaParaExcluir(null);
        return;
      }

      setModalTarefaAberto(false);
      setTarefaEmEdicao(null);
    }

    window.addEventListener("keydown", tratarTeclaEscape);
    return () => window.removeEventListener("keydown", tratarTeclaEscape);
  }, [modalTarefaAberto, tarefaParaExcluir]);

  useEffect(() => {
    const assinaturaAtual = tarefasServidor
      .map(
        (tarefa) =>
          `${tarefa.id}:${tarefa.status}:${tarefa.responsavelUsuarioId}:${tarefa.dataConclusao ?? ""}`
      )
      .join("|");

    if (
      assinaturaAnteriorTarefas.current.length > 0 &&
      assinaturaAnteriorTarefas.current !== assinaturaAtual
    ) {
      setMomentoUltimaSincronizacao(new Date());
    }

    assinaturaAnteriorTarefas.current = assinaturaAtual;
  }, [consultaTarefas.dataUpdatedAt, tarefasServidor]);

  useEffect(() => {
    function tratarAtalhos(evento: KeyboardEvent): void {
      const alvo = evento.target as HTMLElement | null;
      const digitandoEmCampo =
        alvo instanceof HTMLInputElement ||
        alvo instanceof HTMLTextAreaElement ||
        alvo instanceof HTMLSelectElement;

      if (digitandoEmCampo) {
        return;
      }

      if (evento.key === "/") {
        evento.preventDefault();
        campoBuscaRef.current?.focus();
      } else if (evento.key.toLowerCase() === "n") {
        evento.preventDefault();
        if (!ehColaborador) {
          abrirModalNovaTarefa();
        }
      } else if (evento.key.toLowerCase() === "r") {
        evento.preventDefault();
        void atualizarDadosTarefas();
      }
    }

    window.addEventListener("keydown", tratarAtalhos);
    return () => window.removeEventListener("keydown", tratarAtalhos);
  }, [ehColaborador]);

  async function atualizarDadosTarefas(): Promise<void> {
    await Promise.all([
      consultaTarefas.refetch(),
      consultaProjetos.refetch(),
      clienteConsulta.invalidateQueries({ queryKey: ["dashboard"] }),
    ]);
    setMomentoUltimaSincronizacao(new Date());
    mostrarInformacao("Dados de tarefas atualizados.");
  }

  function abrirModalNovaTarefa(): void {
    if (ehColaborador) {
      mostrarErro("Colaborador nao pode criar tarefas.");
      return;
    }

    setTarefaEmEdicao(null);
    setModalTarefaAberto(true);
  }

  function limparFiltros(): void {
    limparFiltrosBase();
    setIdsSelecionados([]);
    setStatusLote("");
    setTarefaEmEdicao(null);
  }

  async function alterarStatusDaTarefa(
    tarefa: TarefaResposta,
    novoStatus: StatusTarefa
  ): Promise<void> {
    if (ehColaborador && tarefa.responsavelUsuarioId !== sessao?.usuarioId) {
      mostrarErro("Colaborador pode alterar status apenas das proprias tarefas.");
      return;
    }

    if (novoStatus === tarefa.status) {
      return;
    }

    if (!statusPermitido(tarefa.status, novoStatus)) {
      mostrarErro("Transicao de status invalida para esta tarefa.");
      return;
    }

    await mutacaoAtualizarStatus.mutateAsync({ id: tarefa.id, status: novoStatus });
  }

  function solicitarExclusaoTarefa(tarefa: TarefaResposta): void {
    if (ehColaborador) {
      mostrarErro("Colaborador nao pode excluir tarefas.");
      return;
    }

    setTarefaParaExcluir(tarefa);
  }

  async function confirmarExclusaoTarefa(): Promise<void> {
    if (!tarefaParaExcluir) {
      return;
    }

    await mutacaoExcluirTarefa.mutateAsync(tarefaParaExcluir.id);

    if (tarefaEmEdicao?.id === tarefaParaExcluir.id) {
      setTarefaEmEdicao(null);
      setModalTarefaAberto(false);
    }

    setTarefaParaExcluir(null);
  }

  function iniciarEdicaoTarefa(tarefa: TarefaResposta): void {
    if (ehColaborador) {
      mostrarErro("Colaborador nao pode editar tarefas.");
      return;
    }

    setTarefaEmEdicao(tarefa);
    setModalTarefaAberto(true);
  }

  function alternarSelecaoTarefa(id: string): void {
    setIdsSelecionados((idsAtuais) =>
      idsAtuais.includes(id)
        ? idsAtuais.filter((item) => item !== id)
        : [...idsAtuais, id]
    );
  }

  function alternarSelecaoTodasVisiveis(): void {
    const idsVisiveis = tarefasFiltradas.map((tarefa) => tarefa.id);
    const todasSelecionadas =
      idsVisiveis.length > 0 && idsVisiveis.every((id) => idsSelecionados.includes(id));

    if (todasSelecionadas) {
      setIdsSelecionados((idsAtuais) => idsAtuais.filter((id) => !idsVisiveis.includes(id)));
    } else {
      setIdsSelecionados((idsAtuais) => [...new Set([...idsAtuais, ...idsVisiveis])]);
    }
  }

  async function aplicarStatusEmLote(): Promise<void> {
    if (ehColaborador) {
      mostrarErro("Colaborador nao pode executar atualizacao em lote.");
      return;
    }

    if (!statusLote) {
      mostrarErro("Selecione um status para aplicar em lote.");
      return;
    }

    if (idsSelecionados.length === 0) {
      mostrarErro("Selecione ao menos uma tarefa.");
      return;
    }

    const novoStatus = Number(statusLote) as StatusTarefa;
    const tarefasSelecionadas = tarefasFiltradas.filter((tarefa) =>
      idsSelecionados.includes(tarefa.id)
    );
    const tarefasValidas = tarefasSelecionadas.filter((tarefa) =>
      statusPermitido(tarefa.status, novoStatus)
    );

    if (tarefasValidas.length === 0) {
      mostrarErro("Nenhuma tarefa selecionada permite esse status.");
      return;
    }

    setExecutandoAcaoLote(true);
    const resultados = await Promise.allSettled(
      tarefasValidas.map((tarefa) =>
        atualizarStatusTarefa(tarefa.id, {
          status: novoStatus,
        })
      )
    );
    setExecutandoAcaoLote(false);

    const totalSucesso = resultados.filter(
      (resultado) => resultado.status === "fulfilled"
    ).length;
    const totalFalha = resultados.length - totalSucesso;

    await Promise.all([
      clienteConsulta.invalidateQueries({ queryKey: ["tarefas"] }),
      clienteConsulta.invalidateQueries({ queryKey: ["dashboard"] }),
    ]);

    setIdsSelecionados([]);
    setStatusLote("");

    if (totalSucesso > 0) {
      mostrarSucesso(`${totalSucesso} tarefa(s) atualizada(s) em lote.`);
    }
    if (totalFalha > 0) {
      mostrarErro(`${totalFalha} tarefa(s) falharam na atualizacao em lote.`);
    }
  }

  async function excluirSelecionadasEmLote(): Promise<void> {
    if (ehColaborador) {
      mostrarErro("Colaborador nao pode executar exclusao em lote.");
      return;
    }

    if (idsSelecionados.length === 0) {
      mostrarErro("Selecione ao menos uma tarefa para exclusao em lote.");
      return;
    }

    const confirmouExclusao = window.confirm(
      `Deseja realmente excluir ${idsSelecionados.length} tarefa(s) selecionada(s)?`
    );
    if (!confirmouExclusao) {
      return;
    }

    const tarefasSelecionadas = tarefasFiltradas.filter((tarefa) =>
      idsSelecionados.includes(tarefa.id)
    );

    setExecutandoAcaoLote(true);
    const resultados = await Promise.allSettled(
      tarefasSelecionadas.map((tarefa) => excluirTarefa(tarefa.id))
    );
    setExecutandoAcaoLote(false);

    const totalSucesso = resultados.filter(
      (resultado) => resultado.status === "fulfilled"
    ).length;
    const totalFalha = resultados.length - totalSucesso;

    await Promise.all([
      clienteConsulta.invalidateQueries({ queryKey: ["tarefas"] }),
      clienteConsulta.invalidateQueries({ queryKey: ["dashboard"] }),
    ]);

    setIdsSelecionados([]);

    if (totalSucesso > 0) {
      mostrarSucesso(`${totalSucesso} tarefa(s) excluida(s) em lote.`);
    }
    if (totalFalha > 0) {
      mostrarErro(`${totalFalha} tarefa(s) nao puderam ser excluidas.`);
    }
  }

  function exportarTarefasCsv(): void {
    if (tarefasFiltradas.length === 0) {
      mostrarErro("Nao ha tarefas para exportar no recorte atual.");
      return;
    }

    const cabecalho = [
      "id",
      "titulo",
      "descricao",
      "status",
      "projeto",
      "responsavel_usuario_id",
      "responsavel_nome",
      "prazo",
      "atrasada",
    ];

    const linhas = tarefasFiltradas.map((tarefa) => [
      tarefa.id,
      tarefa.titulo,
      tarefa.descricao ?? "",
      nomesStatus[tarefa.status],
      mapaProjetos.get(tarefa.projetoId) ?? tarefa.projetoId,
      tarefa.responsavelUsuarioId,
      tarefa.responsavelNome ?? "",
      tarefa.dataPrazo,
      tarefa.estaAtrasada ? "sim" : "nao",
    ]);

    const conteudo = [cabecalho, ...linhas]
      .map((linha) => linha.map((valor) => escaparValorCsv(valor)).join(";"))
      .join("\n");

    const arquivo = new Blob([`\uFEFF${conteudo}`], {
      type: "text/csv;charset=utf-8;",
    });

    const urlArquivo = URL.createObjectURL(arquivo);
    const linkDownload = document.createElement("a");
    linkDownload.href = urlArquivo;
    linkDownload.download = `tarefas-${new Date().toISOString().slice(0, 10)}.csv`;
    document.body.appendChild(linkDownload);
    linkDownload.click();
    document.body.removeChild(linkDownload);
    URL.revokeObjectURL(urlArquivo);

    mostrarSucesso("Exportacao de tarefas concluida.");
  }

  const totalSelecionadas = idsSelecionados.length;
  const totalVisivel = tarefasFiltradas.length;
  const todasVisiveisSelecionadas =
    totalVisivel > 0 && tarefasFiltradas.every((tarefa) => idsSelecionados.includes(tarefa.id));

  return (
    <section className="pagina-conteudo pagina-tarefas">
      <header className="cabecalho-pagina cabecalho-tarefas">
        <div>
          <h1>Tarefas</h1>
          <p>Visao operacional com filtros avancados e duas visualizacoes.</p>
        </div>
        <div className="acoes-cabecalho-tarefas">
          {!ehColaborador && (
            <button
              type="button"
              className="botao-secundario botao-acao-principal-projeto"
              onClick={abrirModalNovaTarefa}
            >
              + Nova tarefa
            </button>
          )}
          <button type="button" className="botao-secundario" onClick={() => void atualizarDadosTarefas()}>
            Atualizar
          </button>
          <button type="button" className="botao-secundario" onClick={exportarTarefasCsv}>
            Exportar CSV
          </button>
        </div>
      </header>

      <section className="grade-resumo-tarefas">
        {cartoesResumo.map((cartao) => (
          <article className="cartao-resumo-tarefa" key={cartao.titulo}>
            <h3>{cartao.titulo}</h3>
            <strong>{cartao.valor}</strong>
            <span>{cartao.subtitulo}</span>
          </article>
        ))}
      </section>

      {historicoNotificacoes.length > 0 && (
        <article className="cartao-listagem painel-notificacoes-tarefas-resumido">
          <header className="cabecalho-listagem-projetos">
            <h3>Notificacoes recentes</h3>
            <span>{Math.min(historicoNotificacoes.length, 3)} exibidas</span>
          </header>
          <ul className="lista-com-acoes lista-notificacoes-resumida">
            {historicoNotificacoes.slice(0, 3).map((notificacao) => (
              <li className="item-listagem item-notificacao-resumida" key={notificacao.id}>
                <div className="conteudo-item-listagem">
                  <strong>{notificacao.tituloTarefa}</strong>
                  <span>{notificacao.mensagem}</span>
                  <small>{new Date(notificacao.dataCriacao).toLocaleString("pt-BR")}</small>
                </div>
              </li>
            ))}
          </ul>
        </article>
      )}

      <article className="cartao-filtros painel-filtros-tarefas">
        <header className="cabecalho-painel-filtros-tarefas">
          <h3>Filtros e ordenacao</h3>
        </header>
        <div className="grade-filtros grade-filtros-tarefas">
          <label htmlFor="filtroBuscaTarefa">
            Buscar por titulo ou descricao
            <input
              id="filtroBuscaTarefa"
              ref={campoBuscaRef}
              type="text"
              value={textoBusca}
              onChange={(evento) => setTextoBusca(evento.target.value)}
            />
          </label>

          <label htmlFor="filtroProjetoId">
            Projeto
            <select id="filtroProjetoId" value={projetoIdFiltro} onChange={(evento) => setProjetoIdFiltro(evento.target.value)}>
              <option value="">Todos</option>
              {consultaProjetos.data?.map((projeto) => (
                <option key={projeto.id} value={projeto.id}>
                  {projeto.nome}
                </option>
              ))}
            </select>
          </label>

          <label htmlFor="filtroStatus">
            Status
            <select id="filtroStatus" value={statusFiltro} onChange={(evento) => setStatusFiltro(evento.target.value)}>
              <option value="">Todos</option>
              {Object.values(StatusTarefa)
                .filter((valor) => typeof valor === "number")
                .map((status) => (
                  <option key={status} value={status}>
                    {nomesStatus[status as StatusTarefa]}
                  </option>
                ))}
            </select>
          </label>

          <label htmlFor="filtroresponsavelUsuarioId">
            Responsavel
            <select
              id="filtroresponsavelUsuarioId"
              value={responsavelUsuarioIdFiltro}
              onChange={(evento) => setResponsavelUsuarioIdFiltro(evento.target.value)}
            >
              <option value="">Todos</option>
              {usuariosResponsaveis.map((usuario) => (
                <option key={usuario.id} value={usuario.id}>
                  {usuario.nome}
                </option>
              ))}
            </select>
          </label>

          <label htmlFor="filtroDataPrazoInicial">
            Prazo inicial
            <input id="filtroDataPrazoInicial" type="date" value={dataPrazoInicialFiltro} onChange={(evento) => setDataPrazoInicialFiltro(evento.target.value)} />
          </label>

          <label htmlFor="filtroDataPrazoFinal">
            Prazo final
            <input id="filtroDataPrazoFinal" type="date" value={dataPrazoFinalFiltro} onChange={(evento) => setDataPrazoFinalFiltro(evento.target.value)} />
          </label>

          <label htmlFor="filtroCampoOrdenacao">
            Ordenar por
            <select id="filtroCampoOrdenacao" value={campoOrdenacao} onChange={(evento) => setCampoOrdenacao(Number(evento.target.value) as CampoOrdenacaoTarefa)}>
              {Object.values(CampoOrdenacaoTarefa)
                .filter((valor) => typeof valor === "number" && valor !== CampoOrdenacaoTarefa.DataPrazo)
                .map((campo) => (
                  <option key={campo} value={campo}>
                    {nomesCamposOrdenacao[campo as CampoOrdenacaoTarefa]}
                  </option>
                ))}
            </select>
          </label>

          <label htmlFor="filtroDirecaoOrdenacao">
            Direcao
            <select id="filtroDirecaoOrdenacao" value={direcaoOrdenacao} onChange={(evento) => setDirecaoOrdenacao(Number(evento.target.value) as DirecaoOrdenacaoTarefa)}>
              {Object.values(DirecaoOrdenacaoTarefa)
                .filter((valor) => typeof valor === "number")
                .map((direcao) => (
                  <option key={direcao} value={direcao}>
                    {nomesDirecaoOrdenacao[direcao as DirecaoOrdenacaoTarefa]}
                  </option>
                ))}
            </select>
          </label>

          <label htmlFor="filtroTamanhoPagina">
            Itens por pagina
            <select id="filtroTamanhoPagina" value={tamanhoPagina} onChange={(evento) => setTamanhoPagina(Number(evento.target.value))}>
              <option value={5}>5</option>
              <option value={10}>10</option>
              <option value={20}>20</option>
            </select>
          </label>
        </div>

        <div className="linha-chips-tarefas">
          {opcoesChipsRapidos.map((chip) => (
            <button
              key={chip.id}
              type="button"
              className={`chip-filtro-tarefa${chipsAtivos.includes(chip.id) ? " chip-filtro-tarefa-ativo" : ""}`}
              onClick={() => alternarChipRapido(chip.id)}
            >
              {chip.rotulo}
            </button>
          ))}
        </div>

        <div className="acoes-rapidas-filtros-tarefas">
          <button type="button" className="botao-secundario" onClick={limparFiltros}>
            Limpar filtros
          </button>
        </div>
        <p className="atalhos-tarefas">
          Atalhos: <strong>/</strong> buscar, <strong>n</strong> nova tarefa, <strong>r</strong>{" "}
          atualizar.
        </p>
      </article>

      {ehColaborador && (
        <article className="cartao-listagem aviso-permissao-colaborador">
          <h3>Atualizacao de status</h3>
          <p>
            Seu perfil possui permissao para atualizar apenas o status das tarefas
            atribuidas a voce.
          </p>
        </article>
      )}

      <article className="cartao-listagem cartao-listagem-tarefas-modernizado cartao-listagem-tarefas-full">
        <header className="cabecalho-listagem-tarefas">
          <h3>Tarefas cadastradas</h3>
          <div className="acoes-cabecalho-listagem-tarefas">
            <span>
              Exibindo {tarefasFiltradas.length} de {totalRegistros} registro(s)
            </span>
          </div>
        </header>

        <section className="painel-acoes-lote-tarefas">
          <label htmlFor="statusLoteTarefas">
            Alterar status em lote
            <select
              id="statusLoteTarefas"
              value={statusLote}
              onChange={(evento) => setStatusLote(evento.target.value)}
              disabled={executandoAcaoLote || ehColaborador}
            >
              <option value="">Selecione</option>
              {Object.values(StatusTarefa)
                .filter((valor) => typeof valor === "number")
                .map((status) => (
                  <option key={status} value={status}>
                    {nomesStatus[status as StatusTarefa]}
                  </option>
                ))}
            </select>
          </label>
          <button
            type="button"
            className="botao-secundario"
            onClick={() => void aplicarStatusEmLote()}
            disabled={executandoAcaoLote || totalSelecionadas === 0 || ehColaborador}
          >
            Aplicar em selecionadas
          </button>
          <button
            type="button"
            className="botao-perigo"
            onClick={() => void excluirSelecionadasEmLote()}
            disabled={executandoAcaoLote || totalSelecionadas === 0 || ehColaborador}
          >
            Excluir selecionadas
          </button>
          <button
            type="button"
            className="botao-secundario"
            onClick={alternarSelecaoTodasVisiveis}
            disabled={tarefasFiltradas.length === 0}
          >
            {todasVisiveisSelecionadas ? "Desmarcar visiveis" : "Marcar visiveis"}
          </button>
          <span>{totalSelecionadas} selecionada(s)</span>
        </section>

        {momentoUltimaSincronizacao && (
          <p className="rotulo-atualizacao-tarefas">
            Sincronizado em {momentoUltimaSincronizacao.toLocaleTimeString("pt-BR")}
          </p>
        )}

        {consultaTarefas.isLoading && (
          <div className="lista-esqueleto-tarefas">
            <div className="bloco-esqueleto bloco-esqueleto-linha-tarefa" />
            <div className="bloco-esqueleto bloco-esqueleto-linha-tarefa" />
          </div>
        )}

        {consultaTarefas.isError && (
          <p className="mensagem-erro">
            {obterMensagemErro(consultaTarefas.error, "Falha ao carregar tarefas.")}
          </p>
        )}

        {semResultados && <p>Nenhuma tarefa encontrada com os filtros atuais.</p>}

        {!consultaTarefas.isLoading && !consultaTarefas.isError && !semResultados && (
          <>
            <TabelaTarefasOperacional
              tarefas={tarefasFiltradas}
              idsSelecionados={idsSelecionados}
              todasVisiveisSelecionadas={todasVisiveisSelecionadas}
              mapaProjetos={mapaProjetos}
              carregandoAtualizacaoStatus={mutacaoAtualizarStatus.isPending}
              carregandoEdicao={mutacaoAtualizarTarefa.isPending}
              carregandoExclusao={mutacaoExcluirTarefa.isPending}
              permitirEditar={!ehColaborador}
              permitirExcluir={!ehColaborador}
              campoOrdenacao={campoOrdenacao}
              direcaoOrdenacao={direcaoOrdenacao}
              obterStatusPermitidos={obterStatusPermitidos}
              aoAlternarSelecaoTodas={alternarSelecaoTodasVisiveis}
              aoAlternarSelecao={alternarSelecaoTarefa}
              aoAlterarStatus={(tarefa, novoStatus) =>
                void alterarStatusDaTarefa(tarefa, novoStatus)
              }
              aoEditar={iniciarEdicaoTarefa}
              aoExcluir={solicitarExclusaoTarefa}
              aoOrdenar={alternarOrdenacaoPorCabecalho}
            />

            <footer className="rodape-listagem">
              <span>
                Pagina {numeroPagina} de {totalPaginas} | {totalRegistros} tarefa(s) no servidor
              </span>
              <div className="acoes-paginacao">
                <button
                  type="button"
                  className="botao-secundario"
                  onClick={() => setNumeroPagina((paginaAtual) => paginaAtual - 1)}
                  disabled={numeroPagina <= 1}
                >
                  Anterior
                </button>
                <button
                  type="button"
                  className="botao-secundario"
                  onClick={() => setNumeroPagina((paginaAtual) => paginaAtual + 1)}
                  disabled={numeroPagina >= totalPaginas}
                >
                  Proxima
                </button>
              </div>
            </footer>
          </>
        )}
      </article>

      {modalTarefaAberto && !ehColaborador && (
        <div className="sobreposicao-modal" role="presentation">
          <section
            className="cartao-modal cartao-modal-tarefa"
            role="dialog"
            aria-modal="true"
            aria-labelledby="tituloModalTarefa"
          >
            <header className="cabecalho-modal">
              <h3 id="tituloModalTarefa">
                {tarefaEmEdicao ? "Editar tarefa" : "Nova tarefa"}
              </h3>
              <button
                type="button"
                className="botao-fechar-modal"
                onClick={() => {
                  setModalTarefaAberto(false);
                  setTarefaEmEdicao(null);
                }}
                aria-label="Fechar modal de tarefa"
              >
                x
              </button>
            </header>

            <FormularioTarefa
              projetos={consultaProjetos.data ?? []}
              usuariosResponsaveis={usuariosResponsaveis}
              responsavelUsuarioIdPadrao={sessao?.usuarioId ?? ""}
              emEnvio={mutacaoCriarTarefa.isPending || mutacaoAtualizarTarefa.isPending}
              titulo={tarefaEmEdicao ? "Atualizar tarefa existente" : "Cadastrar nova tarefa"}
              rotuloBotao={tarefaEmEdicao ? "Salvar alteracoes" : "Criar tarefa"}
              rotuloBotaoEmEnvio={tarefaEmEdicao ? "Salvando..." : "Criando..."}
              permitirPrazoPassado={Boolean(tarefaEmEdicao)}
              valoresIniciais={
                tarefaEmEdicao
                  ? {
                      titulo: tarefaEmEdicao.titulo,
                      descricao: tarefaEmEdicao.descricao ?? "",
                      prioridade: tarefaEmEdicao.prioridade,
                      projetoId: tarefaEmEdicao.projetoId,
                      responsavelUsuarioId: tarefaEmEdicao.responsavelUsuarioId,
                      dataPrazo: converterIsoParaDataInput(tarefaEmEdicao.dataPrazo),
                    }
                  : undefined
              }
              aoCancelarEdicao={() => {
                setModalTarefaAberto(false);
                setTarefaEmEdicao(null);
              }}
              aoEnviar={async (dados) => {
                if (tarefaEmEdicao) {
                  await mutacaoAtualizarTarefa.mutateAsync({
                    id: tarefaEmEdicao.id,
                    dados: {
                      titulo: dados.titulo,
                      descricao: dados.descricao || null,
                      status: tarefaEmEdicao.status,
                      prioridade: dados.prioridade,
                      responsavelUsuarioId: dados.responsavelUsuarioId,
                      dataPrazo: new Date(`${dados.dataPrazo}T23:59:59Z`).toISOString(),
                    },
                  });
                  setTarefaEmEdicao(null);
                  setModalTarefaAberto(false);
                  return;
                }

                await mutacaoCriarTarefa.mutateAsync({
                  titulo: dados.titulo,
                  descricao: dados.descricao || null,
                  prioridade: dados.prioridade,
                  projetoId: dados.projetoId,
                  responsavelUsuarioId: dados.responsavelUsuarioId,
                  dataPrazo: new Date(`${dados.dataPrazo}T23:59:59Z`).toISOString(),
                });
                setModalTarefaAberto(false);
              }}
            />
          </section>
        </div>
      )}

      {tarefaParaExcluir && (
        <div className="sobreposicao-modal" role="presentation">
          <section
            className="cartao-modal cartao-modal-confirmacao"
            role="dialog"
            aria-modal="true"
            aria-labelledby="tituloModalExclusaoTarefa"
          >
            <header className="cabecalho-modal">
              <h3 id="tituloModalExclusaoTarefa">Confirmar exclusao de tarefa</h3>
              <button
                type="button"
                className="botao-fechar-modal"
                onClick={() => setTarefaParaExcluir(null)}
                aria-label="Fechar confirmacao de exclusao"
              >
                x
              </button>
            </header>

            <p>
              Deseja realmente excluir a tarefa <strong>{tarefaParaExcluir.titulo}</strong>?
            </p>
            <span className="mensagem-erro">
              Esta acao nao pode ser desfeita.
            </span>

            <div className="linha-botoes-formulario">
              <button
                type="button"
                className="botao-perigo"
                onClick={() => {
                  void confirmarExclusaoTarefa();
                }}
                disabled={mutacaoExcluirTarefa.isPending}
              >
                {mutacaoExcluirTarefa.isPending ? "Excluindo..." : "Excluir tarefa"}
              </button>
              <button
                type="button"
                className="botao-secundario"
                onClick={() => setTarefaParaExcluir(null)}
                disabled={mutacaoExcluirTarefa.isPending}
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


