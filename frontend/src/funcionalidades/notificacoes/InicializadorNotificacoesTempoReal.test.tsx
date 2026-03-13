import { render, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import {
  EVENTO_TAREFA_ATRIBUIDA,
  criarConexaoNotificacoesTempoReal,
} from "../../servicos/servicoNotificacoesTempoReal";
import { listarHistoricoNotificacoes } from "../../servicos/servicoNotificacoes";
import { InicializadorNotificacoesTempoReal } from "./InicializadorNotificacoesTempoReal";

const mostrarErroMock = vi.fn();
const mostrarInformacaoMock = vi.fn();
const definirHistoricoNotificacoesMock = vi.fn();
const adicionarNotificacaoHistoricoMock = vi.fn();
const listarHistoricoNotificacoesMock = vi.fn();

let estadoAutenticacao = {
  estaAutenticado: true,
  sessao: {
    usuarioId: "user-1",
    tokenAcesso: "token-1",
  },
};

const conexaoMock = {
  on: vi.fn(),
  off: vi.fn(),
  start: vi.fn(),
  stop: vi.fn(),
};

vi.mock("../../ganchos/usarAutenticacao", () => ({
  usarAutenticacao: () => ({
    ...estadoAutenticacao,
    ehSuperAdmin: false,
    ehAdmin: true,
    ehColaborador: false,
    realizarLogin: vi.fn(),
    realizarLogout: vi.fn(),
  }),
}));

vi.mock("../../ganchos/usarNotificacao", () => ({
  usarNotificacao: () => ({
    notificacoes: [],
    historicoNotificacoes: [],
    mostrarErro: mostrarErroMock,
    mostrarInformacao: mostrarInformacaoMock,
    removerNotificacao: vi.fn(),
    mostrarSucesso: vi.fn(),
    definirHistoricoNotificacoes: definirHistoricoNotificacoesMock,
    adicionarNotificacaoHistorico: adicionarNotificacaoHistoricoMock,
  }),
}));

vi.mock("../../servicos/servicoNotificacoesTempoReal", () => ({
  EVENTO_TAREFA_ATRIBUIDA: "tarefaAtribuida",
  criarConexaoNotificacoesTempoReal: vi.fn(),
}));

vi.mock("../../servicos/servicoNotificacoes", () => ({
  listarHistoricoNotificacoes: vi.fn(),
}));

describe("InicializadorNotificacoesTempoReal", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    estadoAutenticacao = {
      estaAutenticado: true,
      sessao: {
        usuarioId: "user-1",
        tokenAcesso: "token-1",
      },
    };
    conexaoMock.on.mockReset();
    conexaoMock.off.mockReset();
    conexaoMock.start.mockReset();
    conexaoMock.stop.mockReset();
    conexaoMock.start.mockResolvedValue(undefined);
    conexaoMock.stop.mockResolvedValue(undefined);
    vi.mocked(criarConexaoNotificacoesTempoReal).mockReturnValue(conexaoMock as never);
    vi.mocked(listarHistoricoNotificacoes).mockImplementation(listarHistoricoNotificacoesMock);
    listarHistoricoNotificacoesMock.mockResolvedValue([
      {
        id: "n1",
        responsavelUsuarioId: "user-1",
        tarefaId: "t1",
        projetoId: "p1",
        tituloTarefa: "Tarefa 1",
        mensagem: "Mensagem",
        reatribuicao: false,
        dataCriacao: "2026-03-12T12:00:00Z",
      },
    ]);
  });

  it("deve carregar historico, iniciar conexao e tratar evento de tempo real", async () => {
    render(<InicializadorNotificacoesTempoReal />);

    await waitFor(() => {
      expect(listarHistoricoNotificacoesMock).toHaveBeenCalledWith(30);
    });
    expect(definirHistoricoNotificacoesMock).toHaveBeenCalledTimes(1);
    expect(conexaoMock.on).toHaveBeenCalledWith(EVENTO_TAREFA_ATRIBUIDA, expect.any(Function));
    expect(conexaoMock.start).toHaveBeenCalledTimes(1);

    const handlerEvento = conexaoMock.on.mock.calls[0][1] as (evento: {
      tarefaId: string;
      responsavelUsuarioId: string;
      projetoId: string;
      tituloTarefa: string;
      mensagem: string;
      reatribuicao: boolean;
      dataOcorrencia: string;
    }) => void;

    handlerEvento({
      tarefaId: "t2",
      responsavelUsuarioId: "user-1",
      projetoId: "p1",
      tituloTarefa: "Tarefa 2",
      mensagem: "Nova atribuicao",
      reatribuicao: true,
      dataOcorrencia: "2026-03-12T13:00:00Z",
    });

    expect(adicionarNotificacaoHistoricoMock).toHaveBeenCalledTimes(1);
    expect(mostrarInformacaoMock).toHaveBeenCalledWith("Nova atribuicao");
  });

  it("deve limpar historico quando nao autenticado", () => {
    estadoAutenticacao = {
      estaAutenticado: false,
      sessao: {
        usuarioId: "",
        tokenAcesso: "",
      },
    };

    render(<InicializadorNotificacoesTempoReal />);
    expect(definirHistoricoNotificacoesMock).toHaveBeenCalledWith([]);
    expect(criarConexaoNotificacoesTempoReal).not.toHaveBeenCalled();
  });
});
