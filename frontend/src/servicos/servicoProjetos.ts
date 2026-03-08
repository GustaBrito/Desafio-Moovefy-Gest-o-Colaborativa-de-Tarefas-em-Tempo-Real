import type {
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
