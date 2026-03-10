export interface RespostaSucessoApi<TDados> {
  sucesso: boolean;
  mensagem: string;
  dados: TDados;
  codigoRastreio?: string;
}

export interface RespostaErroApi {
  sucesso?: boolean;
  status: number;
  codigo: string;
  mensagem: string;
  detalhe?: string;
  codigoRastreio?: string;
  errosValidacao?: ErroValidacaoResposta[];
}

export interface ErroValidacaoResposta {
  campo: string;
  mensagem: string;
}
