import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { MenuPrincipal } from "./MenuPrincipal";

const realizarLogoutMock = vi.fn();

let estadoAutenticacao = {
  sessao: {
    usuarioId: "1",
    nome: "Super Admin",
    email: "superadmin@empresa.com",
    perfilGlobal: 1,
    areaIds: [],
  },
  ehSuperAdmin: true,
  ehAdmin: false,
};

vi.mock("../ganchos/usarAutenticacao", () => ({
  usarAutenticacao: () => ({
    ...estadoAutenticacao,
    estaAutenticado: true,
    ehColaborador: !estadoAutenticacao.ehSuperAdmin && !estadoAutenticacao.ehAdmin,
    realizarLogin: vi.fn(),
    realizarLogout: realizarLogoutMock,
  }),
}));

vi.mock("../ganchos/usarNotificacao", () => ({
  usarNotificacao: () => ({
    notificacoes: [],
    historicoNotificacoes: [{ id: "1" }, { id: "2" }],
    mostrarSucesso: vi.fn(),
    mostrarErro: vi.fn(),
    mostrarInformacao: vi.fn(),
    removerNotificacao: vi.fn(),
    definirHistoricoNotificacoes: vi.fn(),
    adicionarNotificacaoHistorico: vi.fn(),
  }),
}));

describe("MenuPrincipal", () => {
  beforeEach(() => {
    realizarLogoutMock.mockReset();
    estadoAutenticacao = {
      sessao: {
        usuarioId: "1",
        nome: "Super Admin",
        email: "superadmin@empresa.com",
        perfilGlobal: 1,
        areaIds: [],
      },
      ehSuperAdmin: true,
      ehAdmin: false,
    };
  });

  it("deve exibir itens administrativos para super admin", async () => {
    const usuario = userEvent.setup();
    const aoAlternarMenu = vi.fn();

    render(
      <MemoryRouter>
        <MenuPrincipal menuExpandido aoAlternarMenu={aoAlternarMenu} />
      </MemoryRouter>
    );

    expect(screen.getByText("Dashboard")).toBeInTheDocument();
    expect(screen.getByText("Projetos")).toBeInTheDocument();
    expect(screen.getByText("Tarefas")).toBeInTheDocument();
    expect(screen.getByText("Usuarios")).toBeInTheDocument();
    expect(screen.getByText("Areas")).toBeInTheDocument();
    expect(screen.getByText("2 notificacao(oes)")).toBeInTheDocument();

    await usuario.click(screen.getByRole("button", { name: "Recolher menu lateral" }));
    expect(aoAlternarMenu).toHaveBeenCalledTimes(1);

    await usuario.click(screen.getByRole("button", { name: "Sair" }));
    expect(realizarLogoutMock).toHaveBeenCalledTimes(1);
  });

  it("deve ocultar itens administrativos para colaborador", () => {
    estadoAutenticacao = {
      sessao: {
        usuarioId: "2",
        nome: "Colaborador",
        email: "colaborador@empresa.com",
        perfilGlobal: 3,
        areaIds: [],
      },
      ehSuperAdmin: false,
      ehAdmin: false,
    };

    render(
      <MemoryRouter>
        <MenuPrincipal menuExpandido={false} aoAlternarMenu={vi.fn()} />
      </MemoryRouter>
    );

    expect(screen.queryByText("Usuarios")).not.toBeInTheDocument();
    expect(screen.queryByText("Areas")).not.toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Expandir menu lateral" })).toBeInTheDocument();
  });
});
