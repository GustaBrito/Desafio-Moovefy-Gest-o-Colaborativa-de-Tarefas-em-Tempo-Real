export interface ProjetoResposta {
  id: string;
  nome: string;
  descricao?: string | null;
  dataCriacao: string;
}

export interface CriarProjetoRequisicao {
  nome: string;
  descricao?: string | null;
}

export interface AtualizarProjetoRequisicao {
  nome: string;
  descricao?: string | null;
}
