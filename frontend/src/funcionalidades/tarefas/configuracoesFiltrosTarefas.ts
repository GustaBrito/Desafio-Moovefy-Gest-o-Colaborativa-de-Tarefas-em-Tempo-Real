import {
  CampoOrdenacaoTarefa,
  DirecaoOrdenacaoTarefa,
  StatusTarefa,
} from "../../tipos/tarefas";

export type VisualizacaoTarefas = "lista" | "quadro";
export type ChipRapidoTarefas = "atrasadas" | "vence_hoje" | "urgentes";

export interface FiltrosTarefasPersistidos {
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

export const nomesStatus: Record<StatusTarefa, string> = {
  [StatusTarefa.Pendente]: "Pendente",
  [StatusTarefa.EmAndamento]: "Em andamento",
  [StatusTarefa.Concluida]: "Concluida",
  [StatusTarefa.Cancelada]: "Cancelada",
};

export const nomesCamposOrdenacao: Record<CampoOrdenacaoTarefa, string> = {
  [CampoOrdenacaoTarefa.DataCriacao]: "Data de criacao",
  [CampoOrdenacaoTarefa.DataPrazo]: "Data de prazo",
  [CampoOrdenacaoTarefa.Prioridade]: "Prioridade",
  [CampoOrdenacaoTarefa.Status]: "Status",
  [CampoOrdenacaoTarefa.Titulo]: "Titulo",
};

export const nomesDirecaoOrdenacao: Record<DirecaoOrdenacaoTarefa, string> = {
  [DirecaoOrdenacaoTarefa.Ascendente]: "Ascendente",
  [DirecaoOrdenacaoTarefa.Descendente]: "Descendente",
};

export const opcoesChipsRapidos: Array<{
  id: ChipRapidoTarefas;
  rotulo: string;
}> = [
  { id: "atrasadas", rotulo: "Atrasadas" },
  { id: "vence_hoje", rotulo: "Vence hoje" },
  { id: "urgentes", rotulo: "Urgentes" },
];

const CHAVE_FILTROS_TAREFAS = "tarefas_filtros_persistidos_v1";

export function lerFiltrosPersistidosTarefas(): FiltrosTarefasPersistidos | null {
  try {
    const valor = window.localStorage.getItem(CHAVE_FILTROS_TAREFAS);
    if (!valor) {
      return null;
    }

    const filtros = JSON.parse(valor) as FiltrosTarefasPersistidos;
    if (filtros.campoOrdenacao === CampoOrdenacaoTarefa.DataPrazo) {
      filtros.campoOrdenacao = CampoOrdenacaoTarefa.DataCriacao;
    }

    return filtros;
  } catch {
    return null;
  }
}

export function salvarFiltrosPersistidosTarefas(
  filtros: FiltrosTarefasPersistidos
): void {
  window.localStorage.setItem(CHAVE_FILTROS_TAREFAS, JSON.stringify(filtros));
}
