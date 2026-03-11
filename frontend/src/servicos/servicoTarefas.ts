import type {
  AtualizarTarefaRequisicao,
  AtualizarStatusTarefaRequisicao,
  CriarTarefaRequisicao,
  FiltroConsultaTarefas,
  ResultadoPaginado,
  TarefaResposta,
} from "../tipos/tarefas";
import { requisitarApi } from "./clienteApi";

export async function listarTarefas(
  filtro: FiltroConsultaTarefas = {}
): Promise<ResultadoPaginado<TarefaResposta>> {
  const sufixoQuery = construirSufixoQuery(filtro);
  return requisitarApi<ResultadoPaginado<TarefaResposta>>(`/api/tarefas${sufixoQuery}`);
}

export async function criarTarefa(
  requisicao: CriarTarefaRequisicao
): Promise<TarefaResposta> {
  return requisitarApi<TarefaResposta>("/api/tarefas", {
    method: "POST",
    corpo: requisicao,
  });
}

export async function atualizarTarefa(
  id: string,
  requisicao: AtualizarTarefaRequisicao
): Promise<TarefaResposta> {
  return requisitarApi<TarefaResposta>(`/api/tarefas/${id}`, {
    method: "PUT",
    corpo: requisicao,
  });
}

export async function atualizarStatusTarefa(
  id: string,
  requisicao: AtualizarStatusTarefaRequisicao
): Promise<TarefaResposta> {
  return requisitarApi<TarefaResposta>(`/api/tarefas/${id}/status`, {
    method: "PATCH",
    corpo: requisicao,
  });
}

export async function excluirTarefa(id: string): Promise<void> {
  return requisitarApi<void>(`/api/tarefas/${id}`, {
    method: "DELETE",
  });
}

export async function listarTodasTarefas(
  filtroBase: Omit<FiltroConsultaTarefas, "numeroPagina" | "tamanhoPagina"> = {}
): Promise<TarefaResposta[]> {
  const tamanhoPagina = 100;
  const limitePaginasSeguranca = 200;

  let numeroPagina = 1;
  let totalPaginas = 1;
  const tarefas: TarefaResposta[] = [];

  while (numeroPagina <= totalPaginas && numeroPagina <= limitePaginasSeguranca) {
    const resultado = await listarTarefas({
      ...filtroBase,
      numeroPagina,
      tamanhoPagina,
    });

    tarefas.push(...resultado.itens);
    totalPaginas = resultado.totalPaginas;
    numeroPagina += 1;
  }

  return tarefas;
}

function construirSufixoQuery(filtro: FiltroConsultaTarefas): string {
  const parametros = new URLSearchParams();

  adicionarParametro(parametros, "projetoId", filtro.projetoId);
  adicionarParametro(parametros, "status", filtro.status);
  adicionarParametro(parametros, "responsavelUsuarioId", filtro.responsavelUsuarioId);
  adicionarParametro(parametros, "dataPrazoInicial", filtro.dataPrazoInicial);
  adicionarParametro(parametros, "dataPrazoFinal", filtro.dataPrazoFinal);
  adicionarParametro(parametros, "campoOrdenacao", filtro.campoOrdenacao);
  adicionarParametro(parametros, "direcaoOrdenacao", filtro.direcaoOrdenacao);
  adicionarParametro(parametros, "numeroPagina", filtro.numeroPagina);
  adicionarParametro(parametros, "tamanhoPagina", filtro.tamanhoPagina);

  if ([...parametros.keys()].length === 0) {
    return "";
  }

  return `?${parametros.toString()}`;
}

function adicionarParametro(
  parametros: URLSearchParams,
  chave: string,
  valor: string | number | undefined
): void {
  if (valor === undefined || valor === "") {
    return;
  }

  parametros.set(chave, String(valor));
}
