import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { ListaNotificacoesToast } from "./ListaNotificacoesToast";

const removerNotificacaoMock = vi.fn();

vi.mock("../ganchos/usarNotificacao", () => ({
  usarNotificacao: () => ({
    notificacoes: [
      { id: 1, tipo: "sucesso", mensagem: "Operacao concluida" },
      { id: 2, tipo: "erro", mensagem: "Falha na operacao" },
      { id: 3, tipo: "informacao", mensagem: "Processamento em andamento" },
    ],
    historicoNotificacoes: [],
    mostrarSucesso: vi.fn(),
    mostrarErro: vi.fn(),
    mostrarInformacao: vi.fn(),
    removerNotificacao: removerNotificacaoMock,
    definirHistoricoNotificacoes: vi.fn(),
    adicionarNotificacaoHistorico: vi.fn(),
  }),
}));

describe("ListaNotificacoesToast", () => {
  it("deve exibir toasts com tipos e remover notificacao ao fechar", async () => {
    const usuario = userEvent.setup();
    render(<ListaNotificacoesToast />);

    expect(screen.getByText("Sucesso")).toBeInTheDocument();
    expect(screen.getByText("Erro")).toBeInTheDocument();
    expect(screen.getByText("Informacao")).toBeInTheDocument();
    expect(screen.getByText("Operacao concluida")).toBeInTheDocument();
    expect(screen.getByText("Falha na operacao")).toBeInTheDocument();
    expect(screen.getByText("Processamento em andamento")).toBeInTheDocument();

    await usuario.click(screen.getAllByRole("button", { name: "Fechar notificacao" })[1]);
    expect(removerNotificacaoMock).toHaveBeenCalledWith(2);
  });
});
