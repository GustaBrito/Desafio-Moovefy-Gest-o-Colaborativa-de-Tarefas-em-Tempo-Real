import {
  createContext,
  useCallback,
  useMemo,
  useRef,
  useState,
  type PropsWithChildren,
} from "react";

const DURACAO_PADRAO_NOTIFICACAO_MS = 4500;

export type TipoNotificacao = "sucesso" | "erro" | "informacao";

export interface NotificacaoToast {
  id: number;
  mensagem: string;
  tipo: TipoNotificacao;
}

export interface ValorContextoNotificacao {
  notificacoes: NotificacaoToast[];
  mostrarSucesso: (mensagem: string) => void;
  mostrarErro: (mensagem: string) => void;
  mostrarInformacao: (mensagem: string) => void;
  removerNotificacao: (id: number) => void;
}

export const ContextoNotificacao = createContext<ValorContextoNotificacao | null>(
  null
);

export function ProvedorNotificacao({
  children,
}: PropsWithChildren): JSX.Element {
  const [notificacoes, setNotificacoes] = useState<NotificacaoToast[]>([]);
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

  const valorContexto = useMemo<ValorContextoNotificacao>(
    () => ({
      notificacoes,
      mostrarSucesso,
      mostrarErro,
      mostrarInformacao,
      removerNotificacao,
    }),
    [
      notificacoes,
      mostrarSucesso,
      mostrarErro,
      mostrarInformacao,
      removerNotificacao,
    ]
  );

  return (
    <ContextoNotificacao.Provider value={valorContexto}>
      {children}
    </ContextoNotificacao.Provider>
  );
}
