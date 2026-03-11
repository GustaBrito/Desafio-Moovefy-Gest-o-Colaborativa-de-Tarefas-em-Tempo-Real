export interface NotificacaoHistoricoResposta {
  id: string;
  responsavelUsuarioId: string;
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
  responsavelUsuarioId: string;
  tituloTarefa: string;
  reatribuicao: boolean;
  mensagem: string;
  dataOcorrencia: string;
}
