import {
  createContext,
  useCallback,
  useMemo,
  useRef,
  useState,
  type PropsWithChildren,
} from "react";
import type { NotificacaoHistoricoResposta } from "../tipos/notificacoes";

const DURACAO_PADRAO_NOTIFICACAO_MS = 4500;

export type TipoNotificacao = "sucesso" | "erro" | "informacao";

export interface NotificacaoToast {
  id: number;
  mensagem: string;
  tipo: TipoNotificacao;
}

export interface ValorContextoNotificacao {
  notificacoes: NotificacaoToast[];
  historicoNotificacoes: NotificacaoHistoricoResposta[];
  mostrarSucesso: (mensagem: string) => void;
  mostrarErro: (mensagem: string) => void;
  mostrarInformacao: (mensagem: string) => void;
  removerNotificacao: (id: number) => void;
  definirHistoricoNotificacoes: (
    notificacoes: NotificacaoHistoricoResposta[]
  ) => void;
  adicionarNotificacaoHistorico: (
    notificacao: NotificacaoHistoricoResposta
  ) => void;
}

export const ContextoNotificacao = createContext<ValorContextoNotificacao | null>(
  null
);

export function ProvedorNotificacao({
  children,
}: PropsWithChildren): JSX.Element {
  const [notificacoes, setNotificacoes] = useState<NotificacaoToast[]>([]);
  const [historicoNotificacoes, setHistoricoNotificacoes] = useState<
    NotificacaoHistoricoResposta[]
  >([]);
  const contadorId = useRef(1);

  const removerNotificacao = useCallback((id: number) => {
    setNotificacoes((notificacoesAtuais) =>
      notificacoesAtuais.filter((notificacao) => notificacao.id !== id)
    );
  }, []);

  const adicionarNotificacao = useCallback(
    (
      mensagem: string,
      tipo: TipoNotificacao,
      duracaoMs: number = DURACAO_PADRAO_NOTIFICACAO_MS
    ) => {
      const id = contadorId.current;
      contadorId.current += 1;

      setNotificacoes((notificacoesAtuais) => [
        ...notificacoesAtuais,
        {
          id,
          mensagem,
          tipo,
        },
      ]);

      window.setTimeout(() => {
        removerNotificacao(id);
      }, duracaoMs);
    },
    [removerNotificacao]
  );

  const mostrarSucesso = useCallback(
    (mensagem: string) => {
      adicionarNotificacao(mensagem, "sucesso");
    },
    [adicionarNotificacao]
  );

  const mostrarErro = useCallback(
    (mensagem: string) => {
      adicionarNotificacao(mensagem, "erro");
    },
    [adicionarNotificacao]
  );

  const mostrarInformacao = useCallback(
    (mensagem: string) => {
      adicionarNotificacao(mensagem, "informacao");
    },
    [adicionarNotificacao]
  );

  const definirHistoricoNotificacoes = useCallback(
    (notificacoesHistorico: NotificacaoHistoricoResposta[]) => {
      const ordenadas = [...notificacoesHistorico].sort(
        (notificacaoAtual, proximaNotificacao) =>
          new Date(proximaNotificacao.dataCriacao).getTime() -
          new Date(notificacaoAtual.dataCriacao).getTime()
      );

      setHistoricoNotificacoes(ordenadas);
    },
    []
  );

  const adicionarNotificacaoHistorico = useCallback(
    (notificacao: NotificacaoHistoricoResposta) => {
      setHistoricoNotificacoes((notificacoesAtuais) => {
        if (notificacoesAtuais.some((item) => item.id === notificacao.id)) {
          return notificacoesAtuais;
        }

        const proximasNotificacoes = [notificacao, ...notificacoesAtuais]
          .sort(
            (notificacaoAtual, proximaNotificacao) =>
              new Date(proximaNotificacao.dataCriacao).getTime() -
              new Date(notificacaoAtual.dataCriacao).getTime()
          )
          .slice(0, 50);

        return proximasNotificacoes;
      });
    },
    []
  );

  const valorContexto = useMemo<ValorContextoNotificacao>(
    () => ({
      notificacoes,
      historicoNotificacoes,
      mostrarSucesso,
      mostrarErro,
      mostrarInformacao,
      removerNotificacao,
      definirHistoricoNotificacoes,
      adicionarNotificacaoHistorico,
    }),
    [
      notificacoes,
      historicoNotificacoes,
      mostrarSucesso,
      mostrarErro,
      mostrarInformacao,
      removerNotificacao,
      definirHistoricoNotificacoes,
      adicionarNotificacaoHistorico,
    ]
  );

  return (
    <ContextoNotificacao.Provider value={valorContexto}>
      {children}
    </ContextoNotificacao.Provider>
  );
}
