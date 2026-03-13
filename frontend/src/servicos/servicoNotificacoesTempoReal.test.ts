import { beforeEach, describe, expect, it, vi } from "vitest";

const withUrlMock = vi.fn();
const withAutomaticReconnectMock = vi.fn();
const configureLoggingMock = vi.fn();
const buildMock = vi.fn();
const mockConexao = { id: "conexao-teste" };

vi.mock("@microsoft/signalr", () => {
  class HubConnectionBuilderMock {
    withUrl = withUrlMock.mockReturnThis();
    withAutomaticReconnect = withAutomaticReconnectMock.mockReturnThis();
    configureLogging = configureLoggingMock.mockReturnThis();
    build = buildMock.mockReturnValue(mockConexao);
  }

  return {
    HubConnectionBuilder: HubConnectionBuilderMock,
    LogLevel: { Warning: "warning" },
  };
});

vi.mock("./clienteApi", () => ({
  obterUrlBaseApi: () => "https://api.exemplo.com",
}));

import { criarConexaoNotificacoesTempoReal } from "./servicoNotificacoesTempoReal";

describe("servicoNotificacoesTempoReal", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("deve criar conexao signalr com token e nivel de log esperado", () => {
    const conexao = criarConexaoNotificacoesTempoReal("token-de-teste");

    expect(withUrlMock).toHaveBeenCalledWith(
      "https://api.exemplo.com/hubs/notificacoes",
      expect.objectContaining({
        accessTokenFactory: expect.any(Function),
      })
    );
    const opcoes = withUrlMock.mock.calls[0][1] as { accessTokenFactory: () => string };
    expect(opcoes.accessTokenFactory()).toBe("token-de-teste");
    expect(withAutomaticReconnectMock).toHaveBeenCalledTimes(1);
    expect(configureLoggingMock).toHaveBeenCalledWith("warning");
    expect(buildMock).toHaveBeenCalledTimes(1);
    expect(conexao).toBe(mockConexao);
  });
});
