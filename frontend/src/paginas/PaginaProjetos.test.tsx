import { screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderizarComProvedores } from "../testes/utilitariosRenderizacao";
import { PaginaProjetos } from "./PaginaProjetos";

const listarProjetosMock = vi.fn();
const listarAreasMock = vi.fn();
const listarUsuariosMock = vi.fn();
const criarProjetoMock = vi.fn();
const atualizarProjetoMock = vi.fn();
const excluirProjetoMock = vi.fn();
const mostrarErroMock = vi.fn();
const mostrarSucessoMock = vi.fn();

const areaId = "11111111-1111-1111-1111-111111111111";
const usuarioId = "22222222-2222-2222-2222-222222222222";
const projetoId = "33333333-3333-3333-3333-333333333333";

vi.mock("../ganchos/usarAutenticacao", () => ({
  usarAutenticacao: () => ({
    sessao: { usuarioId, nome: "Admin", email: "admin@empresa.com" },
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

vi.mock("../servicos/servicoProjetos", () => ({
  listarProjetos: (...args: unknown[]) => listarProjetosMock(...args),
  criarProjeto: (...args: unknown[]) => criarProjetoMock(...args),
  atualizarProjeto: (...args: unknown[]) => atualizarProjetoMock(...args),
  excluirProjeto: (...args: unknown[]) => excluirProjetoMock(...args),
}));

vi.mock("../servicos/servicoAreas", () => ({
  listarAreas: (...args: unknown[]) => listarAreasMock(...args),
}));

vi.mock("../servicos/servicoUsuarios", () => ({
  listarUsuarios: (...args: unknown[]) => listarUsuariosMock(...args),
}));

describe("PaginaProjetos", () => {
  beforeEach(() => {
    listarProjetosMock.mockReset();
    listarAreasMock.mockReset();
    listarUsuariosMock.mockReset();
    criarProjetoMock.mockReset();
    atualizarProjetoMock.mockReset();
    excluirProjetoMock.mockReset();
    mostrarErroMock.mockReset();
    mostrarSucessoMock.mockReset();

    listarProjetosMock.mockResolvedValue([
      {
        id: projetoId,
        nome: "Projeto Portal",
        descricao: "Portal interno",
        areaId,
        areaNome: "Desenvolvimento",
        areaIds: [areaId],
        areasNomes: ["Desenvolvimento"],
        gestorUsuarioId: usuarioId,
        gestorNome: "Admin",
        usuarioIdsVinculados: [usuarioId],
        usuariosNomesVinculados: ["Admin"],
        dataCriacao: new Date().toISOString(),
      },
    ]);
    listarAreasMock.mockResolvedValue([
      { id: areaId, nome: "Desenvolvimento", codigo: "DEV", ativa: true },
    ]);
    listarUsuariosMock.mockResolvedValue([
      {
        id: usuarioId,
        nome: "Admin",
        email: "admin@empresa.com",
        perfilGlobal: 1,
        ativo: true,
        dataCriacao: new Date().toISOString(),
        areaIds: [areaId],
        areaNomes: ["Desenvolvimento"],
      },
    ]);
    criarProjetoMock.mockResolvedValue({});
    atualizarProjetoMock.mockResolvedValue({});
    excluirProjetoMock.mockResolvedValue({});
  });

  it("deve listar projetos cadastrados", async () => {
    renderizarComProvedores(<PaginaProjetos />);

    const tabelaProjetos = await screen.findByRole("table");
    expect(within(tabelaProjetos).getByText("Projeto Portal")).toBeInTheDocument();
    expect(within(tabelaProjetos).getByText("Portal interno")).toBeInTheDocument();
    expect(within(tabelaProjetos).getAllByText("Desenvolvimento").length).toBeGreaterThan(0);
  });

  it("deve criar projeto via modal", async () => {
    const usuario = userEvent.setup();
    renderizarComProvedores(<PaginaProjetos />);

    await screen.findByText("Projetos cadastrados");
    await usuario.click(screen.getByRole("button", { name: "+ Novo projeto" }));

    const modal = await screen.findByRole("dialog", { name: "Novo projeto" });
    await usuario.type(within(modal).getByLabelText("Nome"), "Projeto Novo");
    await usuario.type(within(modal).getByLabelText("Descricao"), "Descricao do projeto");

    await usuario.click(
      within(modal).getByRole("button", { name: "Desenvolvimento" })
    );

    await usuario.click(within(modal).getByRole("button", { name: "Criar projeto" }));

    await waitFor(() => {
      expect(criarProjetoMock).toHaveBeenCalledTimes(1);
    });

    const payload = criarProjetoMock.mock.calls[0]?.[0];
    expect(payload).toMatchObject({
      nome: "Projeto Novo",
      descricao: "Descricao do projeto",
      areaId,
      areaIds: [areaId],
    });
  });
});
