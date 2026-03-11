import { useEffect } from "react";
import { HubConnectionState } from "@microsoft/signalr";
import { usarAutenticacao } from "../../ganchos/usarAutenticacao";
import { usarNotificacao } from "../../ganchos/usarNotificacao";
import { listarHistoricoNotificacoes } from "../../servicos/servicoNotificacoes";
import {
  EVENTO_TAREFA_ATRIBUIDA,
  METODO_ENTRAR_CANAL_RESPONSAVEL,
  METODO_SAIR_CANAL_RESPONSAVEL,
  criarConexaoNotificacoesTempoReal,
} from "../../servicos/servicoNotificacoesTempoReal";
import type {
  EventoTarefaAtribuidaTempoReal,
  NotificacaoHistoricoResposta,
} from "../../tipos/notificacoes";

export function InicializadorNotificacoesTempoReal(): null {
  const { estaAutenticado, sessao } = usarAutenticacao();
  const {
    mostrarErro,
    mostrarInformacao,
    definirHistoricoNotificacoes,
    adicionarNotificacaoHistorico,
  } = usarNotificacao();

  useEffect(() => {
    if (!estaAutenticado || !sessao?.tokenAcesso || !sessao.usuarioId) {
      definirHistoricoNotificacoes([]);
      return;
    }

    const tokenAcesso = sessao.tokenAcesso;
    let ativo = true;
    const conexaoNotificacoes = criarConexaoNotificacoesTempoReal(
      tokenAcesso
    );

    function tratarEventoTarefaAtribuida(
      evento: EventoTarefaAtribuidaTempoReal
    ): void {
      if (!ativo) {
        return;
      }

      const notificacaoRecebida: NotificacaoHistoricoResposta = {
        id: `${evento.tarefaId}:${evento.dataOcorrencia}`,
        responsavelUsuarioId: evento.responsavelUsuarioId,
        tarefaId: evento.tarefaId,
        projetoId: evento.projetoId,
        tituloTarefa: evento.tituloTarefa,
        mensagem: evento.mensagem,
        reatribuicao: evento.reatribuicao,
        dataCriacao: evento.dataOcorrencia,
      };

      adicionarNotificacaoHistorico(notificacaoRecebida);
      mostrarInformacao(evento.mensagem);
    }

    async function iniciarNotificacoes(): Promise<void> {
      try {
        const historico = await listarHistoricoNotificacoes(30);
        if (ativo) {
          definirHistoricoNotificacoes(historico);
        }
      } catch {
        if (ativo) {
          mostrarErro("Nao foi possivel carregar o historico de notificacoes.");
        }
      }

      conexaoNotificacoes.on(EVENTO_TAREFA_ATRIBUIDA, tratarEventoTarefaAtribuida);

      try {
        await conexaoNotificacoes.start();
        await conexaoNotificacoes.invoke(METODO_ENTRAR_CANAL_RESPONSAVEL);
      } catch {
        if (ativo) {
          mostrarErro("Nao foi possivel conectar no canal de notificacoes em tempo real.");
        }
      }
    }

    void iniciarNotificacoes();

    return () => {
      ativo = false;
      conexaoNotificacoes.off(EVENTO_TAREFA_ATRIBUIDA, tratarEventoTarefaAtribuida);

      if (conexaoNotificacoes.state === HubConnectionState.Connected) {
        void conexaoNotificacoes
          .invoke(METODO_SAIR_CANAL_RESPONSAVEL)
          .catch(() => undefined);
      }

      void conexaoNotificacoes.stop();
    };
  }, [
    adicionarNotificacaoHistorico,
    definirHistoricoNotificacoes,
    estaAutenticado,
    mostrarErro,
    mostrarInformacao,
    sessao?.tokenAcesso,
    sessao?.usuarioId
  ]);

  return null;
}
