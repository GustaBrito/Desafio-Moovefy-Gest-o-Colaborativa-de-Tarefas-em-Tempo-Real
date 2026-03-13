import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi, beforeEach } from "vitest";
import { ErroRequisicaoApi } from "../servicos/clienteApi";
import { PaginaLogin } from "./PaginaLogin";

const navegarMock = vi.fn();
const realizarLoginMock = vi.fn();
const mostrarSucessoMock = vi.fn();
const mostrarErroMock = vi.fn();
const mostrarInformacaoMock = vi.fn();

vi.mock("../ganchos/usarAutenticacao", () => ({
  usarAutenticacao: () => ({
    sessao: null,
    estaAutenticado: false,
    ehSuperAdmin: false,
    ehAdmin: false,
    ehColaborador: false,
    realizarLogin: realizarLoginMock,
    realizarLogout: vi.fn(),
  }),
}));

vi.mock("../ganchos/usarNotificacao", () => ({
  usarNotificacao: () => ({
    notificacoes: [],
    historicoNotificacoes: [],
    mostrarSucesso: mostrarSucessoMock,
    mostrarErro: mostrarErroMock,
    mostrarInformacao: mostrarInformacaoMock,
    removerNotificacao: vi.fn(),
    definirHistoricoNotificacoes: vi.fn(),
    adicionarNotificacaoHistorico: vi.fn(),
  }),
}));

vi.mock("react-router-dom", async () => {
  const moduloReal = await vi.importActual("react-router-dom");

  return {
    ...moduloReal,
    useNavigate: () => navegarMock,
  };
});

describe("PaginaLogin", () => {
  beforeEach(() => {
    realizarLoginMock.mockReset();
    mostrarSucessoMock.mockReset();
    mostrarErroMock.mockReset();
    mostrarInformacaoMock.mockReset();
    navegarMock.mockReset();

    vi.spyOn(window, "fetch").mockResolvedValue({
      ok: true,
    } as Response);
  });

  it("deve autenticar com sucesso e redirecionar para dashboard", async () => {
    const usuario = userEvent.setup();
    realizarLoginMock.mockResolvedValue(undefined);

    render(<PaginaLogin />);

    await usuario.type(screen.getByLabelText("Email"), "superadmin@gerenciadortarefas.local");
    await usuario.type(screen.getByLabelText("Senha"), "SuperAdmin@123");
    await usuario.click(screen.getByRole("button", { name: "Entrar no sistema" }));

    await waitFor(() => {
      expect(realizarLoginMock).toHaveBeenCalledWith(
        "superadmin@gerenciadortarefas.local",
        "SuperAdmin@123",
        false
      );
    });

    expect(mostrarSucessoMock).toHaveBeenCalledWith("Login realizado com sucesso.");
    expect(navegarMock).toHaveBeenCalledWith("/dashboard", { replace: true });
  });

  it("deve exibir mensagem amigavel quando login retorna limitacao de tentativas", async () => {
    const usuario = userEvent.setup();
    realizarLoginMock.mockRejectedValue(
      new ErroRequisicaoApi({
        mensagem: "Muitas tentativas.",
        status: 429,
        codigo: "limite_taxa_excedido",
        retryAfterSegundos: 15,
      })
    );

    render(<PaginaLogin />);

    await usuario.type(screen.getByLabelText("Email"), "superadmin@gerenciadortarefas.local");
    await usuario.type(screen.getByLabelText("Senha"), "SenhaInvalida@123");
    await usuario.click(screen.getByRole("button", { name: "Entrar no sistema" }));

    expect(
      await screen.findByText(
        "Muitas tentativas de login. Aguarde 15s para tentar novamente."
      )
    ).toBeInTheDocument();
    expect(mostrarErroMock).toHaveBeenCalled();
  });
});
