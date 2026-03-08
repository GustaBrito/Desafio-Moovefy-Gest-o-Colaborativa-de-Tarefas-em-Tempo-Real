export interface LoginRequisicao {
  email: string;
  senha: string;
}

export interface LoginResposta {
  usuarioId: string;
  nome: string;
  email: string;
  perfil: string;
  tokenAcesso: string;
  tipoToken: string;
  expiraEmUtc: string;
}

export interface SessaoAutenticacao {
  usuarioId: string;
  nome: string;
  email: string;
  perfil: string;
  tokenAcesso: string;
  tipoToken: string;
  expiraEmUtc: string;
}
