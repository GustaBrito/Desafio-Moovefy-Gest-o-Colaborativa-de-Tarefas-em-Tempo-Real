import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient } from "@tanstack/react-query";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { renderizarComProvedores } from "../testes/utilitariosRenderizacao";
import { PrioridadeTarefa, StatusTarefa, type TarefaResposta } from "../tipos/tarefas";
import { PaginaTarefas } from "./PaginaTarefas";

const listarProjetosMock = vi.fn();
const listarUsuariosMock = vi.fn();
const listarTarefasMock = vi.fn();
const criarTarefaMock = vi.fn();
const atualizarTarefaMock = vi.fn();
const atualizarStatusTarefaMock = vi.fn();
const excluirTarefaMock = vi.fn();
const mostrarErroMock = vi.fn();
const mostrarInformacaoMock = vi.fn();
const mostrarSucessoMock = vi.fn();
const clientesConsultaTeste: QueryClient[] = [];

let estadoAutenticacao = {
  ehColaborador: false,
  sessao: {
    usuarioId: "user-admin",
    nome: "Admin",
    email: "admin@empresa.com",
    perfilGlobal: 1,
    areaIds: ["area-1"],
  },
};

const tarefaBase: TarefaResposta = {
  id: "tarefa-1",
  titulo: "Planejar release",
  descricao: "Revisar backlog",
  status: StatusTarefa.Pendente,
  prioridade: PrioridadeTarefa.Alta,
  projetoId: "projeto-1",
  responsavelUsuarioId: "user-admin",
  responsavelNome: "Admin",
  dataCriacao: "2026-03-12T10:00:00Z",
  dataPrazo: "2026-03-30T23:59:59Z",
  estaAtrasada: false,
};

vi.mock("../ganchos/usarAutenticacao", () => ({
  usarAutenticacao: () => ({
    estaAutenticado: true,
    ehSuperAdmin: !estadoAutenticacao.ehColaborador,
    ehAdmin: !estadoAutenticacao.ehColaborador,
    ehColaborador: estadoAutenticacao.ehColaborador,
    sessao: estadoAutenticacao.sessao,
    realizarLogin: vi.fn(),
    realizarLogout: vi.fn(),
  }),
}));

vi.mock("../ganchos/usarNotificacao", () => ({
  usarNotificacao: () => ({
    notificacoes: [],
    historicoNotificacoes: [
      {
        id: "notif-1",
        responsavelUsuarioId: "user-admin",
        tarefaId: "tarefa-1",
        projetoId: "projeto-1",
        tituloTarefa: "Planejar release",
        mensagem: "Nova atribuicao",
        reatribuicao: false,
        dataCriacao: "2026-03-12T11:00:00Z",
      },
    ],
    mostrarErro: mostrarErroMock,
    mostrarInformacao: mostrarInformacaoMock,
    mostrarSucesso: mostrarSucessoMock,
    removerNotificacao: vi.fn(),
    definirHistoricoNotificacoes: vi.fn(),
    adicionarNotificacaoHistorico: vi.fn(),
  }),
}));

vi.mock("../servicos/servicoProjetos", () => ({
  listarProjetos: (...args: unknown[]) => listarProjetosMock(...args),
}));

vi.mock("../servicos/servicoUsuarios", () => ({
  listarUsuarios: (...args: unknown[]) => listarUsuariosMock(...args),
}));

vi.mock("../servicos/servicoTarefas", () => ({
  listarTarefas: (...args: unknown[]) => listarTarefasMock(...args),
  criarTarefa: (...args: unknown[]) => criarTarefaMock(...args),
  atualizarTarefa: (...args: unknown[]) => atualizarTarefaMock(...args),
  atualizarStatusTarefa: (...args: unknown[]) => atualizarStatusTarefaMock(...args),
  excluirTarefa: (...args: unknown[]) => excluirTarefaMock(...args),
}));

vi.mock("../funcionalidades/tarefas/TabelaTarefasOperacional", () => ({
  TabelaTarefasOperacional: ({
    tarefas,
    aoEditar,
    aoExcluir,
  }: {
    tarefas: TarefaResposta[];
    aoEditar: (tarefa: TarefaResposta) => void;
    aoExcluir: (tarefa: TarefaResposta) => void;
  }) => (
    <div>
      <span>tabela-operacional</span>
      {tarefas.map((tarefa) => (
        <div key={tarefa.id}>
          <strong>{tarefa.titulo}</strong>
          <button type="button" onClick={() => aoEditar(tarefa)}>
            editar-{tarefa.id}
          </button>
          <button type="button" onClick={() => aoExcluir(tarefa)}>
            excluir-{tarefa.id}
          </button>
        </div>
      ))}
    </div>
  ),
}));

vi.mock("../funcionalidades/tarefas/FormularioTarefa", () => ({
  FormularioTarefa: ({
    aoEnviar,
    aoCancelarEdicao,
    rotuloBotao,
  }: {
    aoEnviar: (dados: {
      titulo: string;
      descricao: string;
      prioridade: number;
      projetoId: string;
      responsavelUsuarioId: string;
      dataPrazo: string;
    }) => Promise<void>;
    aoCancelarEdicao: () => void;
    rotuloBotao: string;
  }) => (
    <div>
      <button
        type="button"
        onClick={() =>
          void aoEnviar({
            titulo: "Nova tarefa de teste",
            descricao: "Descricao teste",
            prioridade: 2,
            projetoId: "projeto-1",
            responsavelUsuarioId: "user-admin",
            dataPrazo: "2026-04-10",
          })
        }
      >
        {rotuloBotao}
      </button>
      <button type="button" onClick={aoCancelarEdicao}>
        cancelar-formulario
      </button>
    </div>
  ),
}));

describe("PaginaTarefas", () => {
  afterEach(async () => {
    for (const clienteConsulta of clientesConsultaTeste) {
      await clienteConsulta.cancelQueries();
      clienteConsulta.clear();
    }

    clientesConsultaTeste.length = 0;
    window.localStorage.clear();
    vi.clearAllTimers();
    vi.useRealTimers();
  });

  beforeEach(() => {
    vi.clearAllMocks();
    estadoAutenticacao = {
      ehColaborador: false,
      sessao: {
        usuarioId: "user-admin",
        nome: "Admin",
        email: "admin@empresa.com",
        perfilGlobal: 1,
        areaIds: ["area-1"],
      },
    };

    listarProjetosMock.mockResolvedValue([
      {
        id: "projeto-1",
        nome: "Projeto Operacional",
        descricao: "Projeto",
        areaId: "area-1",
        areaNome: "Desenvolvimento",
        areaIds: ["area-1"],
        areasNomes: ["Desenvolvimento"],
        dataCriacao: "2026-03-10T10:00:00Z",
        usuarioIdsVinculados: ["user-admin"],
        usuariosNomesVinculados: ["Admin"],
      },
    ]);
    listarUsuariosMock.mockResolvedValue([
      {
        id: "user-admin",
        nome: "Admin",
        email: "admin@empresa.com",
        perfilGlobal: 1,
        ativo: true,
        areaIds: ["area-1"],
        areaNomes: ["Desenvolvimento"],
        dataCriacao: "2026-03-10T10:00:00Z",
      },
    ]);
    listarTarefasMock.mockResolvedValue({
      itens: [tarefaBase],
      totalRegistros: 1,
      numeroPagina: 1,
      tamanhoPagina: 10,
      totalPaginas: 1,
    });
    criarTarefaMock.mockResolvedValue({});
    atualizarTarefaMock.mockResolvedValue({});
    atualizarStatusTarefaMock.mockResolvedValue({});
    excluirTarefaMock.mockResolvedValue({});
  });

  it("deve listar tarefas e atualizar dados manualmente", async () => {
    const usuario = userEvent.setup();
    const renderizacao = renderizarComProvedores(<PaginaTarefas />);
    clientesConsultaTeste.push(renderizacao.clienteConsulta);

    expect(await screen.findByText("tabela-operacional")).toBeInTheDocument();
    expect(screen.getByText("Planejar release")).toBeInTheDocument();
    expect(screen.getByText("Notificacoes recentes")).toBeInTheDocument();

    await usuario.click(screen.getByRole("button", { name: "Atualizar" }));
    await waitFor(() => {
      expect(listarTarefasMock).toHaveBeenCalledTimes(2);
    });
    expect(mostrarInformacaoMock).toHaveBeenCalledWith("Dados de tarefas atualizados.");
  });

  it("deve criar tarefa via modal quando usuario nao e colaborador", async () => {
    const usuario = userEvent.setup();
    const renderizacao = renderizarComProvedores(<PaginaTarefas />);
    clientesConsultaTeste.push(renderizacao.clienteConsulta);

    await screen.findByText("tabela-operacional");
    await usuario.click(screen.getByRole("button", { name: "+ Nova tarefa" }));
    expect(await screen.findByRole("dialog", { name: "Nova tarefa" })).toBeInTheDocument();

    await usuario.click(screen.getByRole("button", { name: "Criar tarefa" }));

    await waitFor(() => {
      expect(criarTarefaMock).toHaveBeenCalledTimes(1);
    });
    expect(criarTarefaMock).toHaveBeenCalledWith(
      expect.objectContaining({
        titulo: "Nova tarefa de teste",
        projetoId: "projeto-1",
        responsavelUsuarioId: "user-admin",
        dataPrazo: "2026-04-10T23:59:59.000Z",
      })
    );
  });

  it("deve esconder criacao para colaborador e nao consultar usuarios ativos", async () => {
    estadoAutenticacao = {
      ehColaborador: true,
      sessao: {
        usuarioId: "user-colab",
        nome: "Colaborador",
        email: "colaborador@empresa.com",
        perfilGlobal: 3,
        areaIds: ["area-1"],
      },
    };

    const renderizacao = renderizarComProvedores(<PaginaTarefas />);
    clientesConsultaTeste.push(renderizacao.clienteConsulta);
    await screen.findByText("tabela-operacional");

    expect(screen.queryByRole("button", { name: "+ Nova tarefa" })).not.toBeInTheDocument();
    expect(
      screen.getByText(
        "Seu perfil possui permissao para atualizar apenas o status das tarefas atribuidas a voce."
      )
    ).toBeInTheDocument();
    expect(listarUsuariosMock).not.toHaveBeenCalled();
  });
});
