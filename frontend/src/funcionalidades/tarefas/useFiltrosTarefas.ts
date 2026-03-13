import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  CampoOrdenacaoTarefa,
  DirecaoOrdenacaoTarefa,
  StatusTarefa,
  type FiltroConsultaTarefas,
} from "../../tipos/tarefas";
import {
  type ChipRapidoTarefas,
  type FiltrosTarefasPersistidos,
  type VisualizacaoTarefas,
  lerFiltrosPersistidosTarefas,
  salvarFiltrosPersistidosTarefas,
} from "./configuracoesFiltrosTarefas";

interface ResultadoUseFiltrosTarefas {
  projetoIdFiltro: string;
  statusFiltro: string;
  responsavelUsuarioIdFiltro: string;
  dataPrazoInicialFiltro: string;
  dataPrazoFinalFiltro: string;
  campoOrdenacao: CampoOrdenacaoTarefa;
  direcaoOrdenacao: DirecaoOrdenacaoTarefa;
  numeroPagina: number;
  tamanhoPagina: number;
  textoBusca: string;
  visualizacao: VisualizacaoTarefas;
  chipsAtivos: ChipRapidoTarefas[];
  filtroConsulta: FiltroConsultaTarefas;
  setProjetoIdFiltro: (valor: string) => void;
  setStatusFiltro: (valor: string) => void;
  setResponsavelUsuarioIdFiltro: (valor: string) => void;
  setDataPrazoInicialFiltro: (valor: string) => void;
  setDataPrazoFinalFiltro: (valor: string) => void;
  setCampoOrdenacao: (valor: CampoOrdenacaoTarefa) => void;
  setDirecaoOrdenacao: (valor: DirecaoOrdenacaoTarefa) => void;
  setNumeroPagina: (valor: number | ((paginaAtual: number) => number)) => void;
  setTamanhoPagina: (valor: number) => void;
  setTextoBusca: (valor: string) => void;
  alternarChipRapido: (chip: ChipRapidoTarefas) => void;
  alternarOrdenacaoPorCabecalho: (campo: CampoOrdenacaoTarefa) => void;
  limparFiltros: () => void;
}

export function useFiltrosTarefas(): ResultadoUseFiltrosTarefas {
  const filtrosIniciais = useRef<FiltrosTarefasPersistidos | null>(
    lerFiltrosPersistidosTarefas()
  );

  const [projetoIdFiltro, setProjetoIdFiltroEstado] = useState(
    () => filtrosIniciais.current?.projetoIdFiltro ?? ""
  );
  const [statusFiltro, setStatusFiltroEstado] = useState(
    () => filtrosIniciais.current?.statusFiltro ?? ""
  );
  const [responsavelUsuarioIdFiltro, setResponsavelUsuarioIdFiltroEstado] = useState(
    () => filtrosIniciais.current?.responsavelUsuarioIdFiltro ?? ""
  );
  const [dataPrazoInicialFiltro, setDataPrazoInicialFiltroEstado] = useState(
    () => filtrosIniciais.current?.dataPrazoInicialFiltro ?? ""
  );
  const [dataPrazoFinalFiltro, setDataPrazoFinalFiltroEstado] = useState(
    () => filtrosIniciais.current?.dataPrazoFinalFiltro ?? ""
  );
  const [campoOrdenacao, setCampoOrdenacaoEstado] = useState<CampoOrdenacaoTarefa>(
    () => filtrosIniciais.current?.campoOrdenacao ?? CampoOrdenacaoTarefa.DataCriacao
  );
  const [direcaoOrdenacao, setDirecaoOrdenacaoEstado] =
    useState<DirecaoOrdenacaoTarefa>(
      () =>
        filtrosIniciais.current?.direcaoOrdenacao ??
        DirecaoOrdenacaoTarefa.Ascendente
    );
  const [numeroPagina, setNumeroPaginaEstado] = useState(1);
  const [tamanhoPagina, setTamanhoPaginaEstado] = useState(
    () => filtrosIniciais.current?.tamanhoPagina ?? 10
  );
  const [textoBusca, setTextoBuscaEstado] = useState(
    () => filtrosIniciais.current?.textoBusca ?? ""
  );
  const [visualizacao] = useState<VisualizacaoTarefas>(
    () => filtrosIniciais.current?.visualizacao ?? "lista"
  );
  const [chipsAtivos, setChipsAtivos] = useState<ChipRapidoTarefas[]>(
    () => filtrosIniciais.current?.chipsAtivos ?? []
  );

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

  const setProjetoIdFiltro = useCallback((valor: string) => {
    setProjetoIdFiltroEstado(valor);
    setNumeroPaginaEstado(1);
  }, []);

  const setStatusFiltro = useCallback((valor: string) => {
    setStatusFiltroEstado(valor);
    setNumeroPaginaEstado(1);
  }, []);

  const setResponsavelUsuarioIdFiltro = useCallback((valor: string) => {
    setResponsavelUsuarioIdFiltroEstado(valor);
    setNumeroPaginaEstado(1);
  }, []);

  const setDataPrazoInicialFiltro = useCallback((valor: string) => {
    setDataPrazoInicialFiltroEstado(valor);
    setNumeroPaginaEstado(1);
  }, []);

  const setDataPrazoFinalFiltro = useCallback((valor: string) => {
    setDataPrazoFinalFiltroEstado(valor);
    setNumeroPaginaEstado(1);
  }, []);

  const setCampoOrdenacao = useCallback((valor: CampoOrdenacaoTarefa) => {
    setCampoOrdenacaoEstado(valor);
    setNumeroPaginaEstado(1);
  }, []);

  const setDirecaoOrdenacao = useCallback((valor: DirecaoOrdenacaoTarefa) => {
    setDirecaoOrdenacaoEstado(valor);
    setNumeroPaginaEstado(1);
  }, []);

  const setNumeroPagina = useCallback(
    (valor: number | ((paginaAtual: number) => number)) => {
      if (typeof valor === "function") {
        setNumeroPaginaEstado((paginaAtual) => valor(paginaAtual));
        return;
      }

      setNumeroPaginaEstado(valor);
    },
    []
  );

  const setTamanhoPagina = useCallback((valor: number) => {
    setTamanhoPaginaEstado(valor);
    setNumeroPaginaEstado(1);
  }, []);

  const setTextoBusca = useCallback((valor: string) => {
    setTextoBuscaEstado(valor);
    setNumeroPaginaEstado(1);
  }, []);

  const alternarChipRapido = useCallback((chip: ChipRapidoTarefas) => {
    setChipsAtivos((chipsAtuais) =>
      chipsAtuais.includes(chip)
        ? chipsAtuais.filter((item) => item !== chip)
        : [...chipsAtuais, chip]
    );
    setNumeroPaginaEstado(1);
  }, []);

  const alternarOrdenacaoPorCabecalho = useCallback(
    (campo: CampoOrdenacaoTarefa) => {
      if (campo === CampoOrdenacaoTarefa.DataPrazo) {
        return;
      }

      if (campoOrdenacao === campo) {
        setDirecaoOrdenacaoEstado((direcaoAtual) =>
          direcaoAtual === DirecaoOrdenacaoTarefa.Ascendente
            ? DirecaoOrdenacaoTarefa.Descendente
            : DirecaoOrdenacaoTarefa.Ascendente
        );
      } else {
        setCampoOrdenacaoEstado(campo);
        setDirecaoOrdenacaoEstado(DirecaoOrdenacaoTarefa.Ascendente);
      }

      setNumeroPaginaEstado(1);
    },
    [campoOrdenacao]
  );

  const limparFiltros = useCallback(() => {
    setProjetoIdFiltroEstado("");
    setStatusFiltroEstado("");
    setResponsavelUsuarioIdFiltroEstado("");
    setDataPrazoInicialFiltroEstado("");
    setDataPrazoFinalFiltroEstado("");
    setCampoOrdenacaoEstado(CampoOrdenacaoTarefa.DataCriacao);
    setDirecaoOrdenacaoEstado(DirecaoOrdenacaoTarefa.Ascendente);
    setNumeroPaginaEstado(1);
    setTamanhoPaginaEstado(10);
    setTextoBuscaEstado("");
    setChipsAtivos([]);
  }, []);

  return {
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
    visualizacao,
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
    limparFiltros,
  };
}
