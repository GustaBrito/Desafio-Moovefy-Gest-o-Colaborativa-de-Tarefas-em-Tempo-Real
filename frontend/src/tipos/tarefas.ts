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
