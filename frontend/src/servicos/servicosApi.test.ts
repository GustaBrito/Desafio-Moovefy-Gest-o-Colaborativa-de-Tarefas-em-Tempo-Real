import { beforeEach, describe, expect, it, vi } from "vitest";
import { StatusTarefa } from "../tipos/tarefas";
import { requisitarApi } from "./clienteApi";
import { atualizarArea, criarArea, listarAreas } from "./servicoAreas";
import { realizarLogin } from "./servicoAutenticacao";
import { obterMetricasDashboard } from "./servicoDashboard";
import { listarHistoricoNotificacoes } from "./servicoNotificacoes";
import {
  atualizarProjeto,
  criarProjeto,
  excluirProjeto,
  listarProjetos,
} from "./servicoProjetos";
import {
  atualizarStatusTarefa,
  atualizarTarefa,
  criarTarefa,
  excluirTarefa,
  listarTarefas,
  listarTodasTarefas,
} from "./servicoTarefas";
import {
  alterarStatusUsuario,
  atualizarUsuario,
  criarUsuario,
  listarUsuarios,
  obterUsuarioPorId,
} from "./servicoUsuarios";

vi.mock("./clienteApi", () => ({
  requisitarApi: vi.fn(),
}));

describe("servicos api", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("deve montar requisicoes de areas", async () => {
    vi.mocked(requisitarApi).mockResolvedValue([]);

    await listarAreas();
    await listarAreas(true);
    await criarArea({ nome: "Nova Area", codigo: "NVA", ativa: true });
    await atualizarArea("area-1", { nome: "Area Atualizada", codigo: "ATU", ativa: true });

    expect(requisitarApi).toHaveBeenNthCalledWith(1, "/api/areas");
    expect(requisitarApi).toHaveBeenNthCalledWith(2, "/api/areas?somenteAtivas=true");
    expect(requisitarApi).toHaveBeenNthCalledWith(3, "/api/areas", {
      method: "POST",
      corpo: { nome: "Nova Area", codigo: "NVA", ativa: true },
    });
    expect(requisitarApi).toHaveBeenNthCalledWith(4, "/api/areas/area-1", {
      method: "PUT",
      corpo: { nome: "Area Atualizada", codigo: "ATU", ativa: true },
    });
  });

  it("deve montar requisicao de autenticacao", async () => {
    vi.mocked(requisitarApi).mockResolvedValue({ tokenAcesso: "token" });
    await realizarLogin({ email: "admin@empresa.com", senha: "senha" });

    expect(requisitarApi).toHaveBeenCalledWith("/api/autenticacao/login", {
      method: "POST",
      corpo: { email: "admin@empresa.com", senha: "senha" },
    });
  });

  it("deve montar requisicao de dashboard e notificacoes", async () => {
    vi.mocked(requisitarApi).mockResolvedValue({});

    await obterMetricasDashboard();
    await listarHistoricoNotificacoes();
    await listarHistoricoNotificacoes(5);

    expect(requisitarApi).toHaveBeenNthCalledWith(1, "/api/dashboard/metricas");
    expect(requisitarApi).toHaveBeenNthCalledWith(2, "/api/notificacoes?limite=20");
    expect(requisitarApi).toHaveBeenNthCalledWith(3, "/api/notificacoes?limite=5");
  });

  it("deve montar requisicoes de projetos", async () => {
    vi.mocked(requisitarApi).mockResolvedValue({});

    await listarProjetos();
    await criarProjeto({
      nome: "Projeto A",
      descricao: "Descricao",
      areaId: "area-1",
      areaIds: ["area-1"],
      usuarioIdsVinculados: ["user-1"],
    });
    await atualizarProjeto("projeto-1", {
      nome: "Projeto B",
      descricao: "Descricao B",
      areaId: "area-2",
      areaIds: ["area-2"],
      usuarioIdsVinculados: [],
    });
    await excluirProjeto("projeto-9");

    expect(requisitarApi).toHaveBeenNthCalledWith(1, "/api/projetos");
    expect(requisitarApi).toHaveBeenNthCalledWith(2, "/api/projetos", {
      method: "POST",
      corpo: {
        nome: "Projeto A",
        descricao: "Descricao",
        areaId: "area-1",
        areaIds: ["area-1"],
        usuarioIdsVinculados: ["user-1"],
      },
    });
    expect(requisitarApi).toHaveBeenNthCalledWith(3, "/api/projetos/projeto-1", {
      method: "PUT",
      corpo: {
        nome: "Projeto B",
        descricao: "Descricao B",
        areaId: "area-2",
        areaIds: ["area-2"],
        usuarioIdsVinculados: [],
      },
    });
    expect(requisitarApi).toHaveBeenNthCalledWith(4, "/api/projetos/projeto-9", {
      method: "DELETE",
    });
  });

  it("deve montar requisicoes de tarefas com query string", async () => {
    vi.mocked(requisitarApi).mockResolvedValue({
      itens: [],
      numeroPagina: 1,
      tamanhoPagina: 10,
      totalPaginas: 1,
      totalRegistros: 0,
    });

    await listarTarefas({
      projetoId: "proj-1",
      status: StatusTarefa.Pendente,
      responsavelUsuarioId: "user-1",
      dataPrazoInicial: "2026-03-01",
      dataPrazoFinal: "2026-03-30",
      campoOrdenacao: 5,
      direcaoOrdenacao: 2,
      numeroPagina: 2,
      tamanhoPagina: 20,
    });
    await listarTarefas({});
    await criarTarefa({
      titulo: "Tarefa A",
      prioridade: 2,
      projetoId: "proj-1",
      responsavelUsuarioId: "user-1",
      dataPrazo: "2026-03-30T23:59:59Z",
    });
    await atualizarTarefa("tarefa-1", {
      titulo: "Tarefa B",
      status: StatusTarefa.EmAndamento,
      prioridade: 3,
      responsavelUsuarioId: "user-2",
      dataPrazo: "2026-03-31T23:59:59Z",
    });
    await atualizarStatusTarefa("tarefa-1", { status: StatusTarefa.Concluida });
    await excluirTarefa("tarefa-2");

    expect(requisitarApi).toHaveBeenNthCalledWith(
      1,
      "/api/tarefas?projetoId=proj-1&status=1&responsavelUsuarioId=user-1&dataPrazoInicial=2026-03-01&dataPrazoFinal=2026-03-30&campoOrdenacao=5&direcaoOrdenacao=2&numeroPagina=2&tamanhoPagina=20"
    );
    expect(requisitarApi).toHaveBeenNthCalledWith(2, "/api/tarefas");
    expect(requisitarApi).toHaveBeenNthCalledWith(3, "/api/tarefas", {
      method: "POST",
      corpo: {
        titulo: "Tarefa A",
        prioridade: 2,
        projetoId: "proj-1",
        responsavelUsuarioId: "user-1",
        dataPrazo: "2026-03-30T23:59:59Z",
      },
    });
    expect(requisitarApi).toHaveBeenNthCalledWith(4, "/api/tarefas/tarefa-1", {
      method: "PUT",
      corpo: {
        titulo: "Tarefa B",
        status: StatusTarefa.EmAndamento,
        prioridade: 3,
        responsavelUsuarioId: "user-2",
        dataPrazo: "2026-03-31T23:59:59Z",
      },
    });
    expect(requisitarApi).toHaveBeenNthCalledWith(5, "/api/tarefas/tarefa-1/status", {
      method: "PATCH",
      corpo: { status: StatusTarefa.Concluida },
    });
    expect(requisitarApi).toHaveBeenNthCalledWith(6, "/api/tarefas/tarefa-2", {
      method: "DELETE",
    });
  });

  it("deve percorrer paginas ao listar todas as tarefas", async () => {
    vi.mocked(requisitarApi)
      .mockResolvedValueOnce({
        itens: [
          {
            id: "tarefa-1",
            titulo: "Tarefa 1",
            status: StatusTarefa.Pendente,
            prioridade: 1,
            projetoId: "proj-1",
            responsavelUsuarioId: "user-1",
            dataCriacao: "2026-03-01T00:00:00Z",
            dataPrazo: "2026-03-10T00:00:00Z",
            estaAtrasada: false,
          },
        ],
        numeroPagina: 1,
        tamanhoPagina: 100,
        totalPaginas: 2,
        totalRegistros: 2,
      })
      .mockResolvedValueOnce({
        itens: [
          {
            id: "tarefa-2",
            titulo: "Tarefa 2",
            status: StatusTarefa.Pendente,
            prioridade: 2,
            projetoId: "proj-1",
            responsavelUsuarioId: "user-1",
            dataCriacao: "2026-03-01T00:00:00Z",
            dataPrazo: "2026-03-11T00:00:00Z",
            estaAtrasada: false,
          },
        ],
        numeroPagina: 2,
        tamanhoPagina: 100,
        totalPaginas: 2,
        totalRegistros: 2,
      });

    const resultado = await listarTodasTarefas({ status: StatusTarefa.Pendente });

    expect(resultado).toHaveLength(2);
    expect(requisitarApi).toHaveBeenNthCalledWith(
      1,
      "/api/tarefas?status=1&numeroPagina=1&tamanhoPagina=100"
    );
    expect(requisitarApi).toHaveBeenNthCalledWith(
      2,
      "/api/tarefas?status=1&numeroPagina=2&tamanhoPagina=100"
    );
  });

  it("deve montar requisicoes de usuarios", async () => {
    vi.mocked(requisitarApi).mockResolvedValue({});

    await listarUsuarios();
    await listarUsuarios(true);
    await obterUsuarioPorId("user-1");
    await criarUsuario({
      nome: "Colaborador",
      email: "colaborador@empresa.com",
      senha: "Senha@123",
      perfilGlobal: 3,
      areaIds: ["area-1"],
      ativo: true,
    });
    await atualizarUsuario("user-2", {
      nome: "Colaborador 2",
      email: "colaborador2@empresa.com",
      perfilGlobal: 3,
      areaIds: ["area-2"],
      ativo: true,
    });
    await alterarStatusUsuario("user-3", { ativo: false });

    expect(requisitarApi).toHaveBeenNthCalledWith(1, "/api/usuarios");
    expect(requisitarApi).toHaveBeenNthCalledWith(2, "/api/usuarios?somenteAtivos=true");
    expect(requisitarApi).toHaveBeenNthCalledWith(3, "/api/usuarios/user-1");
    expect(requisitarApi).toHaveBeenNthCalledWith(4, "/api/usuarios", {
      method: "POST",
      corpo: {
        nome: "Colaborador",
        email: "colaborador@empresa.com",
        senha: "Senha@123",
        perfilGlobal: 3,
        areaIds: ["area-1"],
        ativo: true,
      },
    });
    expect(requisitarApi).toHaveBeenNthCalledWith(5, "/api/usuarios/user-2", {
      method: "PUT",
      corpo: {
        nome: "Colaborador 2",
        email: "colaborador2@empresa.com",
        perfilGlobal: 3,
        areaIds: ["area-2"],
        ativo: true,
      },
    });
    expect(requisitarApi).toHaveBeenNthCalledWith(6, "/api/usuarios/user-3/status", {
      method: "PATCH",
      corpo: { ativo: false },
    });
  });
});
