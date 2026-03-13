import { screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderizarComProvedores } from "../testes/utilitariosRenderizacao";
import { PaginaAreas } from "./PaginaAreas";

const listarAreasMock = vi.fn();
const criarAreaMock = vi.fn();
const atualizarAreaMock = vi.fn();
const mostrarErroMock = vi.fn();
const mostrarSucessoMock = vi.fn();

vi.mock("../ganchos/usarNotificacao", () => ({
  usarNotificacao: () => ({
    notificacoes: [],
    historicoNotificacoes: [],
    mostrarSucesso: mostrarSucessoMock,
    mostrarErro: mostrarErroMock,
    mostrarInformacao: vi.fn(),
    removerNotificacao: vi.fn(),
    definirHistoricoNotificacoes: vi.fn(),
    adicionarNotificacaoHistorico: vi.fn(),
  }),
}));

vi.mock("../servicos/servicoAreas", () => ({
  listarAreas: (...args: unknown[]) => listarAreasMock(...args),
  criarArea: (...args: unknown[]) => criarAreaMock(...args),
  atualizarArea: (...args: unknown[]) => atualizarAreaMock(...args),
}));

describe("PaginaAreas", () => {
  beforeEach(() => {
    listarAreasMock.mockReset();
    criarAreaMock.mockReset();
    atualizarAreaMock.mockReset();
    mostrarErroMock.mockReset();
    mostrarSucessoMock.mockReset();

    listarAreasMock.mockResolvedValue([
      { id: "a1", nome: "Marketing", codigo: "MKT", ativa: true },
      { id: "a2", nome: "Desenvolvimento", codigo: "DEV", ativa: true },
    ]);
    criarAreaMock.mockResolvedValue({});
    atualizarAreaMock.mockResolvedValue({});
  });

  it("deve listar areas e filtrar por nome", async () => {
    const usuario = userEvent.setup();
    renderizarComProvedores(<PaginaAreas />);

    expect(await screen.findByText("Marketing")).toBeInTheDocument();
    expect(screen.getByText("Desenvolvimento")).toBeInTheDocument();

    await usuario.type(screen.getByLabelText("Nome"), "Mark");

    await waitFor(() => {
      expect(screen.getByText("Marketing")).toBeInTheDocument();
      expect(screen.queryByText("Desenvolvimento")).not.toBeInTheDocument();
    });
  });

  it("deve criar area pelo modal", async () => {
    const usuario = userEvent.setup();
    renderizarComProvedores(<PaginaAreas />);

    await screen.findByText("Areas cadastradas");
    await usuario.click(screen.getByRole("button", { name: "+ Nova area" }));

    const modal = await screen.findByRole("dialog", { name: "Nova area" });
    await usuario.type(within(modal).getByLabelText("Nome"), "Gestao");
    await usuario.type(within(modal).getByLabelText("Codigo"), "GST");

    await usuario.click(within(modal).getByRole("button", { name: "Criar area" }));

    await waitFor(() => {
      expect(criarAreaMock).toHaveBeenCalledTimes(1);
    });

    const primeiroArgumento = criarAreaMock.mock.calls[0]?.[0];
    expect(primeiroArgumento).toMatchObject({
      nome: "Gestao",
      codigo: "GST",
      ativa: true,
    });
  });
});
