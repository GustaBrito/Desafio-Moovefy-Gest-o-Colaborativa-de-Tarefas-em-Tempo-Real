export enum PerfilGlobalUsuario {
  SuperAdmin = 1,
  Admin = 2,
  Colaborador = 3,
}

export interface LoginRequisicao {
  email: string;
  senha: string;
}

export interface LoginResposta {
  usuarioId: string;
  nome: string;
  email: string;
  perfilGlobal: PerfilGlobalUsuario;
  areaIds: string[];
  tokenAcesso: string;
  tipoToken: string;
  expiraEmUtc: string;
}

export interface SessaoAutenticacao {
  usuarioId: string;
  nome: string;
  email: string;
  perfilGlobal: PerfilGlobalUsuario;
  areaIds: string[];
  tokenAcesso: string;
  tipoToken: string;
  expiraEmUtc: string;
}
