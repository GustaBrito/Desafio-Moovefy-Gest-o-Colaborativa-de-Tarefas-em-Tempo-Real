import type {
  AreaResposta,
  AtualizarAreaRequisicao,
  CriarAreaRequisicao,
} from "../tipos/areas";
import { requisitarApi } from "./clienteApi";

export async function listarAreas(
  somenteAtivas: boolean = false
): Promise<AreaResposta[]> {
  const sufixo = somenteAtivas ? "?somenteAtivas=true" : "";
  return requisitarApi<AreaResposta[]>(`/api/areas${sufixo}`);
}

export async function criarArea(
  requisicao: CriarAreaRequisicao
): Promise<AreaResposta> {
  return requisitarApi<AreaResposta>("/api/areas", {
    method: "POST",
    corpo: requisicao,
  });
}

export async function atualizarArea(
  id: string,
  requisicao: AtualizarAreaRequisicao
): Promise<AreaResposta> {
  return requisitarApi<AreaResposta>(`/api/areas/${id}`, {
    method: "PUT",
    corpo: requisicao,
  });
}
