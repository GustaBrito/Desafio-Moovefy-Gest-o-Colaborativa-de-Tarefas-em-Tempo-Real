import { useEffect, useMemo, useRef, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { FormularioTarefa } from "../funcionalidades/tarefas/FormularioTarefa";
import { QuadroTarefasOperacional } from "../funcionalidades/tarefas/QuadroTarefasOperacional";
import { TabelaTarefasOperacional } from "../funcionalidades/tarefas/TabelaTarefasOperacional";
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
  type FiltroConsultaTarefas,
  type TarefaResposta,
} from "../tipos/tarefas";

type VisualizacaoTarefas = "lista" | "quadro";
type ChipRapidoTarefas = "atrasadas" | "vence_hoje" | "urgentes";

interface FiltrosTarefasPersistidos {
  projetoIdFiltro: string;
  statusFiltro: string;
  responsavelUsuarioIdFiltro: string;
  dataPrazoInicialFiltro: string;
  dataPrazoFinalFiltro: string;
  campoOrdenacao: CampoOrdenacaoTarefa;
  direcaoOrdenacao: DirecaoOrdenacaoTarefa;
  tamanhoPagina: number;
  textoBusca: string;
  visualizacao: VisualizacaoTarefas;
  chipsAtivos: ChipRapidoTarefas[];
}

interface CartaoResumoTarefas {
  titulo: string;
  valor: number;
  subtitulo: string;
}

const CHAVE_FILTROS_TAREFAS = "tarefas_filtros_persistidos_v1";
const CHAVE_FILTRO_FAVORITO_TAREFAS = "tarefas_filtro_favorito_v1";

const nomesStatus: Record<StatusTarefa, string> = {
  [StatusTarefa.Pendente]: "Pendente",
  [StatusTarefa.EmAndamento]: "Em andamento",
  [StatusTarefa.Concluida]: "Concluida",
  [StatusTarefa.Cancelada]: "Cancelada",
};

const nomesCamposOrdenacao: Record<CampoOrdenacaoTarefa, string> = {
  [CampoOrdenacaoTarefa.DataCriacao]: "Data de criacao",
  [CampoOrdenacaoTarefa.DataPrazo]: "Data de prazo",
  [CampoOrdenacaoTarefa.Prioridade]: "Prioridade",
  [CampoOrdenacaoTarefa.Status]: "Status",
  [CampoOrdenacaoTarefa.Titulo]: "Titulo",
};

const nomesDirecaoOrdenacao: Record<DirecaoOrdenacaoTarefa, string> = {
  [DirecaoOrdenacaoTarefa.Ascendente]: "Ascendente",
  [DirecaoOrdenacaoTarefa.Descendente]: "Descendente",
};

const opcoesChipsRapidos: Array<{ id: ChipRapidoTarefas; rotulo: string }> = [
  { id: "atrasadas", rotulo: "Atrasadas" },
  { id: "vence_hoje", rotulo: "Vence hoje" },
  { id: "urgentes", rotulo: "Urgentes" },
];

export function PaginaTarefas(): JSX.Element {
  const clienteConsulta = useQueryClient();
  const { sessao, ehColaborador } = usarAutenticacao();
  const { historicoNotificacoes, mostrarErro, mostrarInformacao, mostrarSucesso } =
    usarNotificacao();

  const filtrosIniciais = useRef<FiltrosTarefasPersistidos | null>(
    lerFiltrosPersistidosTarefas()
  );
  const assinaturaAnteriorTarefas = useRef("");

  const [projetoIdFiltro, setProjetoIdFiltro] = useState(
    () => filtrosIniciais.current?.projetoIdFiltro ?? ""
  );
  const [statusFiltro, setStatusFiltro] = useState(
    () => filtrosIniciais.current?.statusFiltro ?? ""
  );
  const [responsavelUsuarioIdFiltro, setResponsavelUsuarioIdFiltro] = useState(
    () => filtrosIniciais.current?.responsavelUsuarioIdFiltro ?? ""
  );
  const [dataPrazoInicialFiltro, setDataPrazoInicialFiltro] = useState(
    () => filtrosIniciais.current?.dataPrazoInicialFiltro ?? ""
  );
  const [dataPrazoFinalFiltro, setDataPrazoFinalFiltro] = useState(
    () => filtrosIniciais.current?.dataPrazoFinalFiltro ?? ""
  );
  const [campoOrdenacao, setCampoOrdenacao] = useState<CampoOrdenacaoTarefa>(
    () => filtrosIniciais.current?.campoOrdenacao ?? CampoOrdenacaoTarefa.DataPrazo
  );
  const [direcaoOrdenacao, setDirecaoOrdenacao] =
    useState<DirecaoOrdenacaoTarefa>(
      () =>
        filtrosIniciais.current?.direcaoOrdenacao ??
        DirecaoOrdenacaoTarefa.Ascendente
    );
  const [numeroPagina, setNumeroPagina] = useState(1);
  const [tamanhoPagina, setTamanhoPagina] = useState(
    () => filtrosIniciais.current?.tamanhoPagina ?? 10
  );
  const [textoBusca, setTextoBusca] = useState(
    () => filtrosIniciais.current?.textoBusca ?? ""
  );
  const [visualizacao, setVisualizacao] = useState<VisualizacaoTarefas>(
    () => filtrosIniciais.current?.visualizacao ?? "lista"
  );
  const [chipsAtivos, setChipsAtivos] = useState<ChipRapidoTarefas[]>(
    () => filtrosIniciais.current?.chipsAtivos ?? []
  );
  const [idsSelecionados, setIdsSelecionados] = useState<string[]>([]);
  const [statusLote, setStatusLote] = useState("");
  const [tarefaEmEdicao, setTarefaEmEdicao] = useState<TarefaResposta | null>(null);
  const [executandoAcaoLote, setExecutandoAcaoLote] = useState(false);
  const [momentoUltimaSincronizacao, setMomentoUltimaSincronizacao] =
    useState<Date | null>(null);

  const campoBuscaRef = useRef<HTMLInputElement | null>(null);

  const filtroConsulta = useMemo<FiltroConsultaTarefas>(
    () => ({
      projetoId: projetoIdFiltro || undefined,
      status: statusFiltro ? (Number(statusFiltro) as StatusTarefa) : undefined,
      responsavelUsuarioId: responsavelUsuarioIdFiltro || undefined,
      dataPrazoInicial: dataPrazoInicialFiltro || undefined,
      dataPrazoFinal: dataPrazoFinalFiltro || undefined,
      campoOrdenacao,
      direcaoOrdenacao,
      numeroPagina,
      tamanhoPagina,
    }),
    [
      projetoIdFiltro,
      statusFiltro,
      responsavelUsuarioIdFiltro,
      dataPrazoInicialFiltro,
      dataPrazoFinalFiltro,
      campoOrdenacao,
      direcaoOrdenacao,
      numeroPagina,
      tamanhoPagina,
    ]
  );

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

  const tarefasPorStatus = useMemo(() => {
    const mapa = new Map<StatusTarefa, TarefaResposta[]>();

    Object.values(StatusTarefa)
      .filter((valor): valor is StatusTarefa => typeof valor === "number")
      .forEach((status) => {
        mapa.set(status, []);
      });

    tarefasFiltradas.forEach((tarefa) => {
      const listaStatus = mapa.get(tarefa.status);
      if (listaStatus) {
        listaStatus.push(tarefa);
      }
    });

    return mapa;
  }, [tarefasFiltradas]);

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
    const filtrosPersistidos: FiltrosTarefasPersistidos = {
      projetoIdFiltro,
      statusFiltro,
      responsavelUsuarioIdFiltro,
      dataPrazoInicialFiltro,
      dataPrazoFinalFiltro,
      campoOrdenacao,
      direcaoOrdenacao,
      tamanhoPagina,
      textoBusca,
      visualizacao,
      chipsAtivos,
    };
    salvarFiltrosPersistidosTarefas(filtrosPersistidos);
  }, [
    projetoIdFiltro,
    statusFiltro,
    responsavelUsuarioIdFiltro,
    dataPrazoInicialFiltro,
    dataPrazoFinalFiltro,
    campoOrdenacao,
    direcaoOrdenacao,
    tamanhoPagina,
    textoBusca,
    visualizacao,
    chipsAtivos,
  ]);

  useEffect(() => {
    const idsVisiveis = new Set(tarefasFiltradas.map((tarefa) => tarefa.id));
    setIdsSelecionados((idsAtuais) => idsAtuais.filter((id) => idsVisiveis.has(id)));
  }, [tarefasFiltradas]);

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
        const campoTitulo = document.getElementById("tituloTarefa") as HTMLInputElement | null;
        campoTitulo?.focus();
      } else if (evento.key.toLowerCase() === "r") {
        evento.preventDefault();
        void atualizarDadosTarefas();
      }
    }

    window.addEventListener("keydown", tratarAtalhos);
    return () => window.removeEventListener("keydown", tratarAtalhos);
  });

  async function atualizarDadosTarefas(): Promise<void> {
    await Promise.all([
      consultaTarefas.refetch(),
      consultaProjetos.refetch(),
      clienteConsulta.invalidateQueries({ queryKey: ["dashboard"] }),
    ]);
    setMomentoUltimaSincronizacao(new Date());
    mostrarInformacao("Dados de tarefas atualizados.");
  }

  function salvarFiltroFavorito(): void {
    const filtroFavorito: FiltrosTarefasPersistidos = {
      projetoIdFiltro,
      statusFiltro,
      responsavelUsuarioIdFiltro,
      dataPrazoInicialFiltro,
      dataPrazoFinalFiltro,
      campoOrdenacao,
      direcaoOrdenacao,
      tamanhoPagina,
      textoBusca,
      visualizacao,
      chipsAtivos,
    };

    window.localStorage.setItem(
      CHAVE_FILTRO_FAVORITO_TAREFAS,
      JSON.stringify(filtroFavorito)
    );
    mostrarSucesso("Filtro favorito salvo.");
  }

  function carregarFiltroFavorito(): void {
    try {
      const valor = window.localStorage.getItem(CHAVE_FILTRO_FAVORITO_TAREFAS);
      if (!valor) {
        mostrarErro("Nenhum filtro favorito salvo.");
        return;
      }

      const filtroFavorito = JSON.parse(valor) as FiltrosTarefasPersistidos;
      setProjetoIdFiltro(filtroFavorito.projetoIdFiltro);
      setStatusFiltro(filtroFavorito.statusFiltro);
      setResponsavelUsuarioIdFiltro(filtroFavorito.responsavelUsuarioIdFiltro);
      setDataPrazoInicialFiltro(filtroFavorito.dataPrazoInicialFiltro);
      setDataPrazoFinalFiltro(filtroFavorito.dataPrazoFinalFiltro);
      setCampoOrdenacao(filtroFavorito.campoOrdenacao);
      setDirecaoOrdenacao(filtroFavorito.direcaoOrdenacao);
      setTamanhoPagina(filtroFavorito.tamanhoPagina);
      setTextoBusca(filtroFavorito.textoBusca);
      setVisualizacao(filtroFavorito.visualizacao);
      setChipsAtivos(filtroFavorito.chipsAtivos);
      setNumeroPagina(1);
      mostrarSucesso("Filtro favorito carregado.");
    } catch {
      mostrarErro("Falha ao carregar filtro favorito.");
    }
  }

  function alternarChipRapido(chip: ChipRapidoTarefas): void {
    setChipsAtivos((chipsAtuais) =>
      chipsAtuais.includes(chip)
        ? chipsAtuais.filter((item) => item !== chip)
        : [...chipsAtuais, chip]
    );
    setNumeroPagina(1);
  }

  function limparFiltros(): void {
    setProjetoIdFiltro("");
    setStatusFiltro("");
    setResponsavelUsuarioIdFiltro("");
    setDataPrazoInicialFiltro("");
    setDataPrazoFinalFiltro("");
    setCampoOrdenacao(CampoOrdenacaoTarefa.DataPrazo);
    setDirecaoOrdenacao(DirecaoOrdenacaoTarefa.Ascendente);
    setNumeroPagina(1);
    setTamanhoPagina(10);
    setTextoBusca("");
    setChipsAtivos([]);
    setIdsSelecionados([]);
    setStatusLote("");
    setTarefaEmEdicao(null);
  }

  function aplicarMinhaVisao(): void {
    setProjetoIdFiltro("");
    setStatusFiltro("");
    setResponsavelUsuarioIdFiltro(sessao?.usuarioId ?? "");
    setDataPrazoInicialFiltro("");
    setDataPrazoFinalFiltro("");
    setTextoBusca("");
    setChipsAtivos(["atrasadas"]);
    setNumeroPagina(1);
  }

  function aplicarOperacaoDiaria(): void {
    const hoje = converterParaDataInput(new Date());
    setProjetoIdFiltro("");
    setStatusFiltro(String(StatusTarefa.EmAndamento));
    setResponsavelUsuarioIdFiltro("");
    setDataPrazoInicialFiltro(hoje);
    setDataPrazoFinalFiltro(hoje);
    setTextoBusca("");
    setChipsAtivos(["vence_hoje"]);
    setNumeroPagina(1);
  }

  function alternarOrdenacaoPorCabecalho(campo: CampoOrdenacaoTarefa): void {
    if (campoOrdenacao === campo) {
      setDirecaoOrdenacao((direcaoAtual) =>
        direcaoAtual === DirecaoOrdenacaoTarefa.Ascendente
          ? DirecaoOrdenacaoTarefa.Descendente
          : DirecaoOrdenacaoTarefa.Ascendente
      );
    } else {
      setCampoOrdenacao(campo);
      setDirecaoOrdenacao(DirecaoOrdenacaoTarefa.Ascendente);
    }
    setNumeroPagina(1);
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

  async function excluirTarefaComConfirmacao(tarefa: TarefaResposta): Promise<void> {
    if (ehColaborador) {
      mostrarErro("Colaborador nao pode excluir tarefas.");
      return;
    }

    const confirmouExclusao = window.confirm(
      `Deseja realmente excluir a tarefa "${tarefa.titulo}"?`
    );

    if (!confirmouExclusao) {
      return;
    }

    await mutacaoExcluirTarefa.mutateAsync(tarefa.id);

    if (tarefaEmEdicao?.id === tarefa.id) {
      setTarefaEmEdicao(null);
    }
  }

  function iniciarEdicaoTarefa(tarefa: TarefaResposta): void {
    if (ehColaborador) {
      mostrarErro("Colaborador nao pode editar tarefas.");
      return;
    }

    setTarefaEmEdicao(tarefa);
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
        <article className="cartao-listagem">
          <header className="cabecalho-listagem-projetos">
            <h3>Notificacoes recentes</h3>
            <span>{Math.min(historicoNotificacoes.length, 5)} exibidas</span>
          </header>
          <ul className="lista-com-acoes">
            {historicoNotificacoes.slice(0, 5).map((notificacao) => (
              <li className="item-listagem" key={notificacao.id}>
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
          <div className="acoes-salvar-filtros-tarefas">
            <button type="button" className="botao-secundario" onClick={salvarFiltroFavorito}>
              Salvar favorito
            </button>
            <button type="button" className="botao-secundario" onClick={carregarFiltroFavorito}>
              Carregar favorito
            </button>
          </div>
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
                .filter((valor) => typeof valor === "number")
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
          <button type="button" className="botao-secundario" onClick={aplicarMinhaVisao}>
            Minha visao
          </button>
          <button type="button" className="botao-secundario" onClick={aplicarOperacaoDiaria}>
            Operacao diaria
          </button>
          <button type="button" className="botao-secundario" onClick={limparFiltros}>
            Limpar filtros
          </button>
        </div>
        <p className="atalhos-tarefas">
          Atalhos: <strong>/</strong> buscar, <strong>n</strong> nova tarefa, <strong>r</strong>{" "}
          atualizar.
        </p>
      </article>

      <div className="grade-duas-colunas grade-tarefas-principal">
        {!ehColaborador ? (
          <FormularioTarefa
            projetos={consultaProjetos.data ?? []}
            usuariosResponsaveis={usuariosResponsaveis}
            responsavelUsuarioIdPadrao={sessao?.usuarioId ?? ""}
            emEnvio={mutacaoCriarTarefa.isPending || mutacaoAtualizarTarefa.isPending}
            titulo={tarefaEmEdicao ? "Editar tarefa" : "Nova tarefa"}
            rotuloBotao={tarefaEmEdicao ? "Atualizar tarefa" : "Salvar tarefa"}
            rotuloBotaoEmEnvio={tarefaEmEdicao ? "Atualizando..." : "Salvando..."}
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
            aoCancelarEdicao={
              tarefaEmEdicao ? () => setTarefaEmEdicao(null) : undefined
            }
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
            }}
          />
        ) : (
          <article className="cartao-listagem">
            <h3>Atualizacao de status</h3>
            <p>
              Seu perfil possui permissao para atualizar apenas o status das tarefas
              atribuidas a voce.
            </p>
          </article>
        )}

        <article className="cartao-listagem cartao-listagem-tarefas-modernizado">
          <header className="cabecalho-listagem-tarefas">
            <h3>Tarefas cadastradas</h3>
            <div className="acoes-cabecalho-listagem-tarefas">
              <span>
                Exibindo {tarefasFiltradas.length} de {totalRegistros} registro(s)
              </span>
              <div className="alternador-visualizacao-tarefas">
                <button type="button" className={visualizacao === "lista" ? "ativo" : ""} onClick={() => setVisualizacao("lista")}>
                  Lista
                </button>
                <button type="button" className={visualizacao === "quadro" ? "ativo" : ""} onClick={() => setVisualizacao("quadro")}>
                  Quadro
                </button>
              </div>
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
              {visualizacao === "lista" ? (
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
                  aoExcluir={(tarefa) => void excluirTarefaComConfirmacao(tarefa)}
                  aoOrdenar={alternarOrdenacaoPorCabecalho}
                />
              ) : (
                <QuadroTarefasOperacional
                  tarefasPorStatus={tarefasPorStatus}
                  idsSelecionados={idsSelecionados}
                  mapaProjetos={mapaProjetos}
                  aoAlternarSelecao={alternarSelecaoTarefa}
                />
              )}

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
      </div>
    </section>
  );
}

function obterStatusPermitidos(statusAtual: StatusTarefa): StatusTarefa[] {
  if (statusAtual === StatusTarefa.Pendente) {
    return [StatusTarefa.Pendente, StatusTarefa.EmAndamento, StatusTarefa.Cancelada];
  }
  if (statusAtual === StatusTarefa.EmAndamento) {
    return [StatusTarefa.EmAndamento, StatusTarefa.Concluida, StatusTarefa.Cancelada];
  }
  if (statusAtual === StatusTarefa.Concluida) {
    return [StatusTarefa.Concluida];
  }
  return [StatusTarefa.Cancelada];
}

function statusPermitido(statusAtual: StatusTarefa, novoStatus: StatusTarefa): boolean {
  if (statusAtual === novoStatus) {
    return true;
  }
  return obterStatusPermitidos(statusAtual).includes(novoStatus);
}

function tarefaVenceHoje(tarefa: TarefaResposta): boolean {
  const hoje = new Date();
  const prazo = new Date(tarefa.dataPrazo);

  return (
    prazo.getUTCFullYear() === hoje.getUTCFullYear() &&
    prazo.getUTCMonth() === hoje.getUTCMonth() &&
    prazo.getUTCDate() === hoje.getUTCDate()
  );
}

function normalizarTexto(texto: string): string {
  return texto
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .trim()
    .toLowerCase();
}

function converterParaDataInput(data: Date): string {
  return `${data.getFullYear()}-${String(data.getMonth() + 1).padStart(2, "0")}-${String(
    data.getDate()
  ).padStart(2, "0")}`;
}

function converterIsoParaDataInput(dataIso: string): string {
  const data = new Date(dataIso);
  return converterParaDataInput(data);
}

function escaparValorCsv(valor: string): string {
  return `"${valor.replace(/"/g, '""')}"`;
}

function lerFiltrosPersistidosTarefas(): FiltrosTarefasPersistidos | null {
  try {
    const valor = window.localStorage.getItem(CHAVE_FILTROS_TAREFAS);
    return valor ? (JSON.parse(valor) as FiltrosTarefasPersistidos) : null;
  } catch {
    return null;
  }
}

function salvarFiltrosPersistidosTarefas(filtros: FiltrosTarefasPersistidos): void {
  window.localStorage.setItem(CHAVE_FILTROS_TAREFAS, JSON.stringify(filtros));
}

function obterMensagemErro(excecao: unknown, mensagemPadrao: string): string {
  if (excecao instanceof Error && excecao.message.trim().length > 0) {
    return excecao.message;
  }

  return mensagemPadrao;
}


