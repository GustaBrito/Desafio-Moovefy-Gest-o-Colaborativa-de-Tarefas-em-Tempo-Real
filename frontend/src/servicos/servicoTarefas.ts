import type {
  CriarTarefaRequisicao,
  ResultadoPaginado,
  TarefaResposta,
} from "../tipos/tarefas";
import { requisitarApi } from "./clienteApi";

export async function listarTarefas(): Promise<ResultadoPaginado<TarefaResposta>> {
  return requisitarApi<ResultadoPaginado<TarefaResposta>>("/api/tarefas");
}

export async function criarTarefa(
  requisicao: CriarTarefaRequisicao
): Promise<TarefaResposta> {
  return requisitarApi<TarefaResposta>("/api/tarefas", {
    method: "POST",
    corpo: requisicao,
  });
}
