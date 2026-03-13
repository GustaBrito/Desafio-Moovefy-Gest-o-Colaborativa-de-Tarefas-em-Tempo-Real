import type { ProjetoResposta } from "../../tipos/projetos";

export type CriterioOrdenacaoProjeto =
  | "nome_ascendente"
  | "nome_descendente"
  | "data_mais_recente"
  | "data_mais_antiga";

export type ModoVisualizacaoProjeto = "tabela" | "quadro";

export function normalizarTexto(texto: string): string {
  return texto
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .trim()
    .toLowerCase();
}

export function obterNomesAreasProjeto(projeto: ProjetoResposta): string[] {
  if (projeto.areasNomes && projeto.areasNomes.length > 0) {
    return projeto.areasNomes;
  }

  if (projeto.areaNome && projeto.areaNome.trim().length > 0) {
    return [projeto.areaNome];
  }

  return ["Sem area"];
}

export function obterNomesPessoasProjeto(projeto: ProjetoResposta): string[] {
  if (
    projeto.usuariosNomesVinculados &&
    projeto.usuariosNomesVinculados.length > 0
  ) {
    return projeto.usuariosNomesVinculados;
  }

  if (projeto.gestorNome && projeto.gestorNome.trim().length > 0) {
    return [projeto.gestorNome];
  }

  return [];
}

export function formatarDataHora(data: string): string {
  return new Date(data).toLocaleString("pt-BR");
}

export function escaparValorCsv(valor: string): string {
  return `"${valor.replace(/"/g, '""')}"`;
}

export function obterMensagemErro(
  excecao: unknown,
  mensagemPadrao: string
): string {
  if (excecao instanceof Error && excecao.message.trim().length > 0) {
    return excecao.message;
  }

  return mensagemPadrao;
}
