import { act, fireEvent, render, screen } from "@testing-library/react";
import { useContext } from "react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  ContextoNotificacao,
  ProvedorNotificacao,
} from "./ContextoNotificacao";

function ComponenteTesteNotificacao(): JSX.Element {
  const contexto = useContext(ContextoNotificacao);

  if (!contexto) {
    throw new Error("Contexto ausente");
  }

  return (
    <div>
      <button type="button" onClick={() => contexto.mostrarSucesso("ok")}>
        sucesso
      </button>
      <button type="button" onClick={() => contexto.mostrarErro("erro")}>
        erro
      </button>
      <button type="button" onClick={() => contexto.mostrarInformacao("info")}>
        info
      </button>
      <button
        type="button"
        onClick={() =>
          contexto.definirHistoricoNotificacoes([
            {
              id: "2",
              responsavelUsuarioId: "u2",
              tarefaId: "t2",
              projetoId: "p2",
              tituloTarefa: "tarefa 2",
              mensagem: "m2",
              reatribuicao: false,
              dataCriacao: "2026-03-11T10:00:00Z",
            },
            {
              id: "1",
              responsavelUsuarioId: "u1",
              tarefaId: "t1",
              projetoId: "p1",
              tituloTarefa: "tarefa 1",
              mensagem: "m1",
              reatribuicao: false,
              dataCriacao: "2026-03-11T09:00:00Z",
            },
          ])
        }
      >
        historico
      </button>
      <button
        type="button"
        onClick={() =>
          contexto.adicionarNotificacaoHistorico({
            id: "3",
            responsavelUsuarioId: "u3",
            tarefaId: "t3",
            projetoId: "p3",
            tituloTarefa: "tarefa 3",
            mensagem: "m3",
            reatribuicao: false,
            dataCriacao: "2026-03-11T12:00:00Z",
          })
        }
      >
        adicionar-historico
      </button>
      <button
        type="button"
        onClick={() =>
          contexto.adicionarNotificacaoHistorico({
            id: "3",
            responsavelUsuarioId: "u3",
            tarefaId: "t3",
            projetoId: "p3",
            tituloTarefa: "tarefa 3",
            mensagem: "m3",
            reatribuicao: false,
            dataCriacao: "2026-03-11T12:00:00Z",
          })
        }
      >
        duplicado-historico
      </button>
      <span data-testid="qtd-toasts">{contexto.notificacoes.length}</span>
      <span data-testid="ids-historico">
        {contexto.historicoNotificacoes.map((item) => item.id).join(",")}
      </span>
    </div>
  );
}

describe("ContextoNotificacao", () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("deve adicionar toasts e remover automaticamente por timeout", () => {
    render(
      <ProvedorNotificacao>
        <ComponenteTesteNotificacao />
      </ProvedorNotificacao>
    );

    fireEvent.click(screen.getByRole("button", { name: "sucesso" }));
    fireEvent.click(screen.getByRole("button", { name: "erro" }));
    fireEvent.click(screen.getByRole("button", { name: "info" }));
    expect(screen.getByTestId("qtd-toasts")).toHaveTextContent("3");

    act(() => {
      vi.advanceTimersByTime(4600);
    });
    expect(screen.getByTestId("qtd-toasts")).toHaveTextContent("0");
  });

  it("deve ordenar historico e ignorar duplicados", () => {
    render(
      <ProvedorNotificacao>
        <ComponenteTesteNotificacao />
      </ProvedorNotificacao>
    );

    fireEvent.click(screen.getByRole("button", { name: "historico" }));
    expect(screen.getByTestId("ids-historico")).toHaveTextContent("2,1");

    fireEvent.click(screen.getByRole("button", { name: "adicionar-historico" }));
    expect(screen.getByTestId("ids-historico")).toHaveTextContent("3,2,1");

    fireEvent.click(screen.getByRole("button", { name: "duplicado-historico" }));
    expect(screen.getByTestId("ids-historico")).toHaveTextContent("3,2,1");
  });
});
