import { usarNotificacao } from "../ganchos/usarNotificacao";

export function ListaNotificacoesToast(): JSX.Element {
  const { notificacoes, removerNotificacao } = usarNotificacao();

  function obterTituloToast(tipo: "sucesso" | "erro" | "informacao"): string {
    if (tipo === "sucesso") {
      return "Sucesso";
    }

    if (tipo === "erro") {
      return "Erro";
    }

    return "Informacao";
  }

  function obterIconeToast(tipo: "sucesso" | "erro" | "informacao"): JSX.Element {
    if (tipo === "sucesso") {
      return (
        <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true" focusable="false">
          <path
            d="M20 6L9 17L4 12"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      );
    }

    if (tipo === "erro") {
      return (
        <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true" focusable="false">
          <path
            d="M12 8V12M12 16H12.01M10.29 3.86L1.82 18A2 2 0 0 0 3.53 21H20.47A2 2 0 0 0 22.18 18L13.71 3.86A2 2 0 0 0 10.29 3.86Z"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.8"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      );
    }

    return (
      <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true" focusable="false">
        <path
          d="M12 7V12M12 16H12.01M21 12C21 16.97 16.97 21 12 21C7.03 21 3 16.97 3 12C3 7.03 7.03 3 12 3C16.97 3 21 7.03 21 12Z"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.8"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
      </svg>
    );
  }

  return (
    <aside className="pilha-toasts" aria-live="polite" aria-atomic="true">
      {notificacoes.map((notificacao) => (
        <article
          className={`toast toast-${notificacao.tipo}`}
          key={notificacao.id}
          role="status"
        >
          <div className="conteudo-toast">
            <span className={`icone-toast icone-toast-${notificacao.tipo}`}>
              {obterIconeToast(notificacao.tipo)}
            </span>
            <div className="texto-toast">
              <strong>{obterTituloToast(notificacao.tipo)}</strong>
              <p>{notificacao.mensagem}</p>
            </div>
          </div>
          <button
            type="button"
            className="botao-fechar-toast"
            onClick={() => removerNotificacao(notificacao.id)}
            aria-label="Fechar notificacao"
          >
            ×
          </button>
        </article>
      ))}
    </aside>
  );
}
