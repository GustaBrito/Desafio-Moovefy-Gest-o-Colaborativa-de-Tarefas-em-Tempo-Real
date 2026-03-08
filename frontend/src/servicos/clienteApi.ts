import type { RespostaErroApi, RespostaSucessoApi } from "../tipos/comum";
import { obterSessaoArmazenada } from "./servicoSessao";

const urlBaseApi = import.meta.env.VITE_URL_API ?? "http://localhost:5258";

interface OpcoesRequisicaoApi extends Omit<RequestInit, "body"> {
  corpo?: unknown;
}

export async function requisitarApi<TResposta>(
  rota: string,
  opcoes: OpcoesRequisicaoApi = {}
): Promise<TResposta> {
  const url = `${urlBaseApi}${rota}`;
  const sessao = obterSessaoArmazenada();

  const headers = new Headers(opcoes.headers);
  headers.set("Accept", "application/json");

  if (opcoes.corpo !== undefined) {
    headers.set("Content-Type", "application/json");
  }

  if (sessao?.tokenAcesso) {
    headers.set("Authorization", `Bearer ${sessao.tokenAcesso}`);
  }

  const resposta = await fetch(url, {
    method: opcoes.method ?? "GET",
    headers,
    body: opcoes.corpo !== undefined ? JSON.stringify(opcoes.corpo) : undefined,
  });

  if (!resposta.ok) {
    await lancarErroApi(resposta);
  }

  if (resposta.status === 204) {
    return undefined as TResposta;
  }

  const respostaSucesso = (await resposta.json()) as RespostaSucessoApi<TResposta>;

  return respostaSucesso.dados;
}

async function lancarErroApi(resposta: Response): Promise<never> {
  let mensagemErro = "Falha na requisicao.";

  try {
    const erro = (await resposta.json()) as RespostaErroApi;
    mensagemErro = erro.detalhe || erro.mensagem || mensagemErro;
  } catch {
    // Mantem mensagem padrao quando o corpo nao e JSON.
  }

  throw new Error(mensagemErro);
}
