export interface RespostaSucessoApi<TDados> {
  sucesso: boolean;
  mensagem: string;
  dados: TDados;
  codigoRastreio?: string;
}

export interface RespostaErroApi {
  status: number;
  codigo: string;
  mensagem: string;
  detalhe?: string;
  codigoRastreio?: string;
}
