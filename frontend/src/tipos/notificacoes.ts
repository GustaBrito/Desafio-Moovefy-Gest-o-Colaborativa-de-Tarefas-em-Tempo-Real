export interface NotificacaoHistoricoResposta {
  id: string;
  responsavelId: string;
  tarefaId: string;
  projetoId: string;
  tituloTarefa: string;
  mensagem: string;
  reatribuicao: boolean;
  dataCriacao: string;
}

export interface EventoTarefaAtribuidaTempoReal {
  tarefaId: string;
  projetoId: string;
  responsavelId: string;
  tituloTarefa: string;
  reatribuicao: boolean;
  mensagem: string;
  dataOcorrencia: string;
}
