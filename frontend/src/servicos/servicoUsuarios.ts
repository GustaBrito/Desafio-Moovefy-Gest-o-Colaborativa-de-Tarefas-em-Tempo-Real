import type {
  AlterarStatusUsuarioRequisicao,
  AtualizarUsuarioRequisicao,
  CriarUsuarioRequisicao,
  UsuarioResposta,
} from "../tipos/usuarios";
import { requisitarApi } from "./clienteApi";

export async function listarUsuarios(
  somenteAtivos: boolean = false
): Promise<UsuarioResposta[]> {
  const sufixo = somenteAtivos ? "?somenteAtivos=true" : "";
  return requisitarApi<UsuarioResposta[]>(`/api/usuarios${sufixo}`);
}

export async function obterUsuarioPorId(id: string): Promise<UsuarioResposta> {
  return requisitarApi<UsuarioResposta>(`/api/usuarios/${id}`);
}

export async function criarUsuario(
  requisicao: CriarUsuarioRequisicao
): Promise<UsuarioResposta> {
  return requisitarApi<UsuarioResposta>("/api/usuarios", {
    method: "POST",
    corpo: requisicao,
  });
}

export async function atualizarUsuario(
  id: string,
  requisicao: AtualizarUsuarioRequisicao
): Promise<UsuarioResposta> {
  return requisitarApi<UsuarioResposta>(`/api/usuarios/${id}`, {
    method: "PUT",
    corpo: requisicao,
  });
}

export async function alterarStatusUsuario(
  id: string,
  requisicao: AlterarStatusUsuarioRequisicao
): Promise<UsuarioResposta> {
  return requisitarApi<UsuarioResposta>(`/api/usuarios/${id}/status`, {
    method: "PATCH",
    corpo: requisicao,
  });
}
