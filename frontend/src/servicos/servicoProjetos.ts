import type {
  AtualizarProjetoRequisicao,
  CriarProjetoRequisicao,
  ProjetoResposta,
} from "../tipos/projetos";
import { requisitarApi } from "./clienteApi";

export async function listarProjetos(): Promise<ProjetoResposta[]> {
  return requisitarApi<ProjetoResposta[]>("/api/projetos");
}

export async function criarProjeto(
  requisicao: CriarProjetoRequisicao
): Promise<ProjetoResposta> {
  return requisitarApi<ProjetoResposta>("/api/projetos", {
    method: "POST",
    corpo: requisicao,
  });
}

export async function atualizarProjeto(
  id: string,
  requisicao: AtualizarProjetoRequisicao
): Promise<ProjetoResposta> {
  return requisitarApi<ProjetoResposta>(`/api/projetos/${id}`, {
    method: "PUT",
    corpo: requisicao,
  });
}

export async function excluirProjeto(id: string): Promise<void> {
  return requisitarApi<void>(`/api/projetos/${id}`, {
    method: "DELETE",
  });
}
