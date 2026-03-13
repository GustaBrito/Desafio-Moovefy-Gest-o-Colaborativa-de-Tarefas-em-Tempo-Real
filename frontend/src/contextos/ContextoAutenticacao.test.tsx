import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { useContext } from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { PerfilGlobalUsuario } from "../tipos/autenticacao";
import {
  ContextoAutenticacao,
  ProvedorAutenticacao,
} from "./ContextoAutenticacao";

const realizarLoginApiMock = vi.fn();
const obterSessaoArmazenadaMock = vi.fn();
const salvarSessaoMock = vi.fn();
const removerSessaoMock = vi.fn();
const sessaoExpirouMock = vi.fn();

vi.mock("../servicos/servicoAutenticacao", () => ({
  realizarLogin: (...args: unknown[]) => realizarLoginApiMock(...args),
}));

vi.mock("../servicos/servicoSessao", () => ({
  obterSessaoArmazenada: () => obterSessaoArmazenadaMock(),
  salvarSessao: (...args: unknown[]) => salvarSessaoMock(...args),
  removerSessao: () => removerSessaoMock(),
  sessaoExpirou: (...args: unknown[]) => sessaoExpirouMock(...args),
}));

function ComponenteTesteAutenticacao(): JSX.Element {
  const contexto = useContext(ContextoAutenticacao);

  if (!contexto) {
    throw new Error("Contexto ausente");
  }

  return (
    <div>
      <button
        type="button"
        onClick={() => contexto.realizarLogin("admin@empresa.com", "Senha@123", false)}
      >
        login
      </button>
      <button type="button" onClick={() => contexto.realizarLogout()}>
        logout
      </button>
      <span data-testid="autenticado">{String(contexto.estaAutenticado)}</span>
      <span data-testid="super-admin">{String(contexto.ehSuperAdmin)}</span>
      <span data-testid="admin">{String(contexto.ehAdmin)}</span>
      <span data-testid="colaborador">{String(contexto.ehColaborador)}</span>
    </div>
  );
}

describe("ContextoAutenticacao", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    obterSessaoArmazenadaMock.mockReturnValue(null);
    sessaoExpirouMock.mockReturnValue(false);
    realizarLoginApiMock.mockResolvedValue({
      usuarioId: "1",
      nome: "Admin",
      email: "admin@empresa.com",
      perfilGlobal: PerfilGlobalUsuario.Admin,
      areaIds: ["area-1"],
      tokenAcesso: "token",
      tipoToken: "Bearer",
      expiraEmUtc: new Date(Date.now() + 60000).toISOString(),
    });
  });

  it("deve realizar login, salvar sessao e atualizar perfil", async () => {
    render(
      <ProvedorAutenticacao>
        <ComponenteTesteAutenticacao />
      </ProvedorAutenticacao>
    );

    fireEvent.click(screen.getByRole("button", { name: "login" }));

    await waitFor(() => {
      expect(salvarSessaoMock).toHaveBeenCalledTimes(1);
    });
    expect(screen.getByTestId("autenticado")).toHaveTextContent("true");
    expect(screen.getByTestId("admin")).toHaveTextContent("true");
    expect(screen.getByTestId("super-admin")).toHaveTextContent("false");
    expect(screen.getByTestId("colaborador")).toHaveTextContent("false");
  });

  it("deve realizar logout explicitamente", () => {
    render(
      <ProvedorAutenticacao>
        <ComponenteTesteAutenticacao />
      </ProvedorAutenticacao>
    );

    fireEvent.click(screen.getByRole("button", { name: "logout" }));
    expect(removerSessaoMock).toHaveBeenCalledTimes(1);
  });

  it("deve encerrar sessao automaticamente quando expirar", async () => {
    const sessaoExpirada = {
      usuarioId: "2",
      nome: "Super",
      email: "super@empresa.com",
      perfilGlobal: PerfilGlobalUsuario.SuperAdmin,
      areaIds: [],
      tokenAcesso: "token",
      tipoToken: "Bearer",
      expiraEmUtc: new Date(Date.now() - 1000).toISOString(),
    };
    obterSessaoArmazenadaMock.mockReturnValue(sessaoExpirada);
    sessaoExpirouMock.mockReturnValue(true);

    render(
      <ProvedorAutenticacao>
        <ComponenteTesteAutenticacao />
      </ProvedorAutenticacao>
    );

    await waitFor(() => {
      expect(removerSessaoMock).toHaveBeenCalledTimes(1);
    });
    expect(screen.getByTestId("autenticado")).toHaveTextContent("false");
  });
});
