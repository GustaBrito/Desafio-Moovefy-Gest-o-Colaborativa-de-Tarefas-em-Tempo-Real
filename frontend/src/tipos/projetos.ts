export interface ProjetoResposta {
  id: string;
  nome: string;
  descricao?: string | null;
  areaId: string;
  areaNome: string;
  gestorUsuarioId?: string | null;
  gestorNome?: string | null;
  dataCriacao: string;
}

export interface CriarProjetoRequisicao {
  nome: string;
  descricao?: string | null;
  areaId: string;
  gestorUsuarioId?: string | null;
}

export interface AtualizarProjetoRequisicao {
  nome: string;
  descricao?: string | null;
  areaId: string;
  gestorUsuarioId?: string | null;
}
