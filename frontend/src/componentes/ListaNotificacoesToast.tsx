import { usarNotificacao } from "../ganchos/usarNotificacao";

export function ListaNotificacoesToast(): JSX.Element {
  const { notificacoes, removerNotificacao } = usarNotificacao();

  return (
    <aside className="pilha-toasts" aria-live="polite" aria-atomic="true">
      {notificacoes.map((notificacao) => (
        <article
          className={`toast toast-${notificacao.tipo}`}
          key={notificacao.id}
          role="status"
        >
          <span>{notificacao.mensagem}</span>
          <button
            type="button"
            className="botao-fechar-toast"
            onClick={() => removerNotificacao(notificacao.id)}
          >
            Fechar
          </button>
        </article>
      ))}
    </aside>
  );
}
