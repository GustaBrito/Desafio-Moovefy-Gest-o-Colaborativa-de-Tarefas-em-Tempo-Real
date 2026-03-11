import type { PerfilGlobalUsuario } from "./autenticacao";

export interface UsuarioResposta {
  id: string;
  nome: string;
  email: string;
  perfilGlobal: PerfilGlobalUsuario;
  ativo: boolean;
  dataCriacao: string;
  ultimoAcesso?: string | null;
  areaIds: string[];
  areaNomes: string[];
}

export interface CriarUsuarioRequisicao {
  nome: string;
  email: string;
  senha: string;
  perfilGlobal: PerfilGlobalUsuario;
  ativo: boolean;
  areaIds: string[];
}

export interface AtualizarUsuarioRequisicao {
  nome: string;
  email: string;
  perfilGlobal: PerfilGlobalUsuario;
  ativo: boolean;
  novaSenha?: string | null;
  areaIds: string[];
}

export interface AlterarStatusUsuarioRequisicao {
  ativo: boolean;
}
