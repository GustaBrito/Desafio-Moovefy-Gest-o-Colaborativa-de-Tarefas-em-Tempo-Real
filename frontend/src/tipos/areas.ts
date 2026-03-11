export interface AreaResposta {
  id: string;
  nome: string;
  codigo?: string | null;
  ativa: boolean;
}

export interface CriarAreaRequisicao {
  nome: string;
  codigo?: string | null;
  ativa: boolean;
}

export interface AtualizarAreaRequisicao {
  nome: string;
  codigo?: string | null;
  ativa: boolean;
}
