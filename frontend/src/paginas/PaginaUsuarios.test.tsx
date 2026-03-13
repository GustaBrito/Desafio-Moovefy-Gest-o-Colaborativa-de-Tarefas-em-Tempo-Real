import { screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { PerfilGlobalUsuario } from "../tipos/autenticacao";
import { renderizarComProvedores } from "../testes/utilitariosRenderizacao";
import { PaginaUsuarios } from "./PaginaUsuarios";

const listarUsuariosMock = vi.fn();
const listarAreasMock = vi.fn();
const criarUsuarioMock = vi.fn();
const atualizarUsuarioMock = vi.fn();
const alterarStatusUsuarioMock = vi.fn();
const mostrarErroMock = vi.fn();
const mostrarSucessoMock = vi.fn();

vi.mock("../ganchos/usarAutenticacao", () => ({
  usarAutenticacao: () => ({
    sessao: null,
    estaAutenticado: true,
    ehSuperAdmin: true,
    ehAdmin: false,
    ehColaborador: false,
    realizarLogin: vi.fn(),
    realizarLogout: vi.fn(),
  }),
}));

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
}));

vi.mock("../servicos/servicoUsuarios", () => ({
  listarUsuarios: (...args: unknown[]) => listarUsuariosMock(...args),
  criarUsuario: (...args: unknown[]) => criarUsuarioMock(...args),
  atualizarUsuario: (...args: unknown[]) => atualizarUsuarioMock(...args),
  alterarStatusUsuario: (...args: unknown[]) => alterarStatusUsuarioMock(...args),
}));

describe("PaginaUsuarios", () => {
  beforeEach(() => {
    listarUsuariosMock.mockReset();
    listarAreasMock.mockReset();
    criarUsuarioMock.mockReset();
    atualizarUsuarioMock.mockReset();
    alterarStatusUsuarioMock.mockReset();
    mostrarErroMock.mockReset();
    mostrarSucessoMock.mockReset();

    listarUsuariosMock.mockResolvedValue([
      {
        id: "u1",
        nome: "Ana Silva",
        email: "ana@empresa.com",
        perfilGlobal: PerfilGlobalUsuario.Admin,
        ativo: true,
        dataCriacao: new Date().toISOString(),
        areaIds: ["a1"],
        areaNomes: ["Marketing"],
      },
      {
        id: "u2",
        nome: "Bruno Dev",
        email: "bruno@empresa.com",
        perfilGlobal: PerfilGlobalUsuario.Colaborador,
        ativo: true,
        dataCriacao: new Date().toISOString(),
        areaIds: ["a2"],
        areaNomes: ["Desenvolvimento"],
      },
    ]);

    listarAreasMock.mockResolvedValue([
      { id: "a1", nome: "Marketing", codigo: "MKT", ativa: true },
      { id: "a2", nome: "Desenvolvimento", codigo: "DEV", ativa: true },
    ]);

    criarUsuarioMock.mockResolvedValue({});
    atualizarUsuarioMock.mockResolvedValue({});
    alterarStatusUsuarioMock.mockResolvedValue({});
  });

  it("deve listar usuarios e aplicar filtro por nome", async () => {
    const usuario = userEvent.setup();
    renderizarComProvedores(<PaginaUsuarios />);

    expect(await screen.findByText("Ana Silva")).toBeInTheDocument();
    expect(screen.getByText("Bruno Dev")).toBeInTheDocument();

    await usuario.type(screen.getByLabelText("Nome"), "Ana");

    await waitFor(() => {
      expect(screen.getByText("Ana Silva")).toBeInTheDocument();
      expect(screen.queryByText("Bruno Dev")).not.toBeInTheDocument();
    });
  });

  it("deve criar usuario pelo modal", async () => {
    const usuario = userEvent.setup();
    renderizarComProvedores(<PaginaUsuarios />);

    await screen.findByText("Usuarios cadastrados");
    await usuario.click(screen.getByRole("button", { name: "+ Novo usuario" }));

    const modal = await screen.findByRole("dialog", { name: "Novo usuario" });

    await usuario.type(within(modal).getByLabelText("Nome"), "Carlos QA");
    await usuario.type(within(modal).getByLabelText("Email"), "carlos@empresa.com");
    await usuario.type(within(modal).getByLabelText("Senha"), "SenhaForte@123");

    await usuario.type(
      within(modal).getByLabelText("Areas vinculadas"),
      "desenv"
    );
    await usuario.click(
      within(modal).getByRole("button", { name: "Desenvolvimento" })
    );

    await usuario.click(within(modal).getByRole("button", { name: "Criar usuario" }));

    await waitFor(() => {
      expect(criarUsuarioMock).toHaveBeenCalledTimes(1);
    });

    const primeiroArgumento = criarUsuarioMock.mock.calls[0]?.[0];
    expect(primeiroArgumento).toMatchObject({
      nome: "Carlos QA",
      email: "carlos@empresa.com",
      senha: "SenhaForte@123",
      areaIds: ["a2"],
    });
  });
});
