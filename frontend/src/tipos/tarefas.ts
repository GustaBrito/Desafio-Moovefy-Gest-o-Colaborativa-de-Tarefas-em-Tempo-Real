export enum StatusTarefa {
  Pendente = 1,
  EmAndamento = 2,
  Concluida = 3,
  Cancelada = 4,
}

export enum PrioridadeTarefa {
  Baixa = 1,
  Media = 2,
  Alta = 3,
  Urgente = 4,
}

export enum CampoOrdenacaoTarefa {
  DataCriacao = 1,
  DataPrazo = 2,
  Prioridade = 3,
  Status = 4,
  Titulo = 5,
}

export enum DirecaoOrdenacaoTarefa {
  Ascendente = 1,
  Descendente = 2,
}

export interface TarefaResposta {
  id: string;
  titulo: string;
  descricao?: string | null;
  status: StatusTarefa;
  prioridade: PrioridadeTarefa;
  projetoId: string;
  responsavelId: string;
  dataCriacao: string;
  dataPrazo: string;
  dataConclusao?: string | null;
  estaAtrasada: boolean;
}

export interface ResultadoPaginado<TItem> {
  itens: TItem[];
  totalRegistros: number;
  numeroPagina: number;
  tamanhoPagina: number;
  totalPaginas: number;
}

export interface CriarTarefaRequisicao {
  titulo: string;
  descricao?: string | null;
  prioridade: PrioridadeTarefa;
  projetoId: string;
  responsavelId: string;
  dataPrazo: string;
}

export interface AtualizarTarefaRequisicao {
  titulo: string;
  descricao?: string | null;
  status: StatusTarefa;
  prioridade: PrioridadeTarefa;
  responsavelId: string;
  dataPrazo: string;
}

export interface AtualizarStatusTarefaRequisicao {
  status: StatusTarefa;
}

export interface FiltroConsultaTarefas {
  projetoId?: string;
  status?: StatusTarefa;
  responsavelId?: string;
  dataPrazoInicial?: string;
  dataPrazoFinal?: string;
  campoOrdenacao?: CampoOrdenacaoTarefa;
  direcaoOrdenacao?: DirecaoOrdenacaoTarefa;
  numeroPagina?: number;
  tamanhoPagina?: number;
}
