import { screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { PrioridadeTarefa, StatusTarefa, type TarefaResposta } from "../tipos/tarefas";
import { renderizarComProvedores } from "../testes/utilitariosRenderizacao";
import { PaginaDashboard } from "./PaginaDashboard";

const mostrarErroMock = vi.fn();
const mostrarInformacaoMock = vi.fn();
const mostrarSucessoMock = vi.fn();
const obterMetricasDashboardMock = vi.fn();
const listarProjetosMock = vi.fn();
const listarTodasTarefasMock = vi.fn();

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

vi.mock("../servicos/servicoDashboard", () => ({
  obterMetricasDashboard: (...args: unknown[]) => obterMetricasDashboardMock(...args),
}));

vi.mock("../servicos/servicoProjetos", () => ({
  listarProjetos: (...args: unknown[]) => listarProjetosMock(...args),
}));

vi.mock("../servicos/servicoTarefas", () => ({
  listarTodasTarefas: (...args: unknown[]) => listarTodasTarefasMock(...args),
}));

const tarefaExemplo: TarefaResposta = {
  id: "tarefa-dashboard-1",
  titulo: "Entrega sprint",
  descricao: "Preparar release",
  status: StatusTarefa.EmAndamento,
  prioridade: PrioridadeTarefa.Alta,
  projetoId: "projeto-1",
  responsavelUsuarioId: "usuario-1",
  responsavelNome: "Dev Team",
  responsavelEmail: "dev@empresa.com",
  areaNome: "Desenvolvimento",
  dataCriacao: "2026-03-10T10:00:00Z",
  dataPrazo: "2026-03-20T10:00:00Z",
  dataConclusao: null,
  estaAtrasada: false,
};

describe("PaginaDashboard", () => {
  beforeEach(() => {
    mostrarErroMock.mockReset();
    mostrarInformacaoMock.mockReset();
    mostrarSucessoMock.mockReset();
    obterMetricasDashboardMock.mockReset();
    listarProjetosMock.mockReset();
    listarTodasTarefasMock.mockReset();
    window.localStorage.clear();
  });

  it("deve exibir estado de loading durante carregamento inicial", async () => {
    obterMetricasDashboardMock.mockReturnValue(new Promise(() => {}));
    listarProjetosMock.mockResolvedValue([]);
    listarTodasTarefasMock.mockResolvedValue([]);

    const { container } = renderizarComProvedores(<PaginaDashboard />);

    expect(container.querySelector(".esqueleto-dashboard")).toBeInTheDocument();
  });

  it("deve exibir estado de erro quando uma consulta falha", async () => {
    obterMetricasDashboardMock.mockRejectedValue(new Error("falha"));
    listarProjetosMock.mockResolvedValue([]);
    listarTodasTarefasMock.mockResolvedValue([]);

    renderizarComProvedores(<PaginaDashboard />);

    expect(
      await screen.findByText("Falha ao carregar dados do dashboard. Tente novamente.")
    ).toBeInTheDocument();
  });

  it("deve renderizar painel com dados quando consultas retornam sucesso", async () => {
    obterMetricasDashboardMock.mockResolvedValue({
      totalTarefasPorStatus: [
        { status: StatusTarefa.Pendente, total: 1 },
        { status: StatusTarefa.EmAndamento, total: 2 },
        { status: StatusTarefa.Concluida, total: 3 },
        { status: StatusTarefa.Cancelada, total: 0 },
      ],
      tarefasAtrasadas: 0,
      tarefasConcluidasNoPrazo: 2,
      taxaConclusao: 60,
    });
    listarProjetosMock.mockResolvedValue([
      {
        id: "projeto-1",
        nome: "Projeto Plataforma",
        descricao: null,
        areaId: "area-1",
        areaNome: "Desenvolvimento",
        dataCriacao: "2026-03-01T10:00:00Z",
      },
    ]);
    listarTodasTarefasMock.mockResolvedValue([tarefaExemplo]);

    renderizarComProvedores(<PaginaDashboard />);

    await waitFor(() => {
      expect(screen.getByText("Total por status")).toBeInTheDocument();
    });
    expect(screen.getByText("Distribuicao por prioridade")).toBeInTheDocument();
    expect(screen.getByText("Tarefas que exigem atencao")).toBeInTheDocument();
  });
});
