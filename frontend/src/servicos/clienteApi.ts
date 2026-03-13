import type { RespostaErroApi, RespostaSucessoApi } from "../tipos/comum";
import {
  obterSessaoArmazenada,
  removerSessao,
  sessaoExpirou,
} from "./servicoSessao";

const urlBaseApiConfigurada = (import.meta.env.VITE_URL_API ?? "").trim();
const urlBaseApi =
  urlBaseApiConfigurada.length > 0
    ? urlBaseApiConfigurada
    : import.meta.env.DEV
      ? "http://localhost:5258"
      : "";

interface OpcoesRequisicaoApi extends Omit<RequestInit, "body"> {
  corpo?: unknown;
}

export class ErroRequisicaoApi extends Error {
  public readonly status: number;
  public readonly codigo: string;
  public readonly detalhe?: string;
  public readonly codigoRastreio?: string;
  public readonly retryAfterSegundos?: number;

  constructor(parametros: {
    mensagem: string;
    status: number;
    codigo: string;
    detalhe?: string;
    codigoRastreio?: string;
    retryAfterSegundos?: number;
  }) {
    super(parametros.mensagem);
    this.name = "ErroRequisicaoApi";
    this.status = parametros.status;
    this.codigo = parametros.codigo;
    this.detalhe = parametros.detalhe;
    this.codigoRastreio = parametros.codigoRastreio;
    this.retryAfterSegundos = parametros.retryAfterSegundos;
  }
}

export function obterUrlBaseApi(): string {
  if (!urlBaseApi) {
    throw new Error(
      "A variavel VITE_URL_API nao foi configurada para este ambiente."
    );
  }

  return urlBaseApi.endsWith("/") ? urlBaseApi.slice(0, -1) : urlBaseApi;
}

export async function requisitarApi<TResposta>(
  rota: string,
  opcoes: OpcoesRequisicaoApi = {}
): Promise<TResposta> {
  let urlBaseResolvida: string;
  try {
    urlBaseResolvida = obterUrlBaseApi();
  } catch (erro) {
    throw new ErroRequisicaoApi({
      mensagem:
        erro instanceof Error
          ? erro.message
          : "A URL da API nao foi configurada.",
      status: 0,
      codigo: "configuracao_invalida",
    });
  }

  const url = `${urlBaseResolvida}${rota}`;
  const sessao = obterSessaoArmazenada();

  const headers = new Headers(opcoes.headers);
  headers.set("Accept", "application/json");

  if (opcoes.corpo !== undefined) {
    headers.set("Content-Type", "application/json");
  }

  if (sessao && sessaoExpirou(sessao)) {
    removerSessao();
    throw new ErroRequisicaoApi({
      mensagem: "Sua sessao expirou. Faca login novamente.",
      status: 401,
      codigo: "sessao_expirada",
    });
  }

  if (sessao?.tokenAcesso) {
    headers.set("Authorization", `Bearer ${sessao.tokenAcesso}`);
  }

  let resposta: Response;

  try {
    resposta = await fetch(url, {
      method: opcoes.method ?? "GET",
      headers,
      body: opcoes.corpo !== undefined ? JSON.stringify(opcoes.corpo) : undefined,
    });
  } catch {
    throw new ErroRequisicaoApi({
      mensagem:
        "Nao foi possivel conectar com a API. Verifique se o backend esta online.",
      status: 0,
      codigo: "falha_rede",
    });
  }

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
  const retryAfterCabecalho = resposta.headers.get("Retry-After");
  const retryAfterSegundos = retryAfterCabecalho
    ? Number.parseInt(retryAfterCabecalho, 10)
    : undefined;

  let erroApi: RespostaErroApi | null = null;

  try {
    erroApi = (await resposta.json()) as RespostaErroApi;
  } catch {
    // Mantem erro padrao quando o corpo nao for JSON.
  }

  if (resposta.status === 401 && obterSessaoArmazenada()) {
    removerSessao();
  }

  throw new ErroRequisicaoApi({
    mensagem: erroApi?.mensagem || "Falha na requisicao.",
    status: erroApi?.status || resposta.status,
    codigo: erroApi?.codigo || "erro_requisicao",
    detalhe: erroApi?.detalhe,
    codigoRastreio: erroApi?.codigoRastreio,
    retryAfterSegundos:
      typeof retryAfterSegundos === "number" && !Number.isNaN(retryAfterSegundos)
        ? retryAfterSegundos
        : undefined,
  });
}
