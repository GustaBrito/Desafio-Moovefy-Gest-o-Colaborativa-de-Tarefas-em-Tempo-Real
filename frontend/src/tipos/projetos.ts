export interface ProjetoResposta {
  id: string;
  nome: string;
  descricao?: string | null;
  areaId: string;
  areaNome: string;
  areaIds?: string[];
  areasNomes?: string[];
  gestorUsuarioId?: string | null;
  gestorNome?: string | null;
  usuarioIdsVinculados?: string[];
  usuariosNomesVinculados?: string[];
  dataCriacao: string;
}

export interface CriarProjetoRequisicao {
  nome: string;
  descricao?: string | null;
  areaId: string;
  areaIds?: string[];
  gestorUsuarioId?: string | null;
  usuarioIdsVinculados?: string[];
}

export interface AtualizarProjetoRequisicao {
  nome: string;
  descricao?: string | null;
  areaId: string;
  areaIds?: string[];
  gestorUsuarioId?: string | null;
  usuarioIdsVinculados?: string[];
}
