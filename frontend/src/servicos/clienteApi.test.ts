import { beforeEach, describe, expect, it, vi } from "vitest";
import type { SessaoAutenticacao } from "../tipos/autenticacao";
import { ErroRequisicaoApi, requisitarApi } from "./clienteApi";
import { salvarSessao } from "./servicoSessao";

function criarSessao(expiraEmUtc?: string): SessaoAutenticacao {
  return {
    usuarioId: "u1",
    nome: "Usuario",
    email: "usuario@teste.com",
    perfilGlobal: 1,
    areaIds: ["a1"],
    tokenAcesso: "token-teste",
    tipoToken: "Bearer",
    expiraEmUtc: expiraEmUtc ?? new Date(Date.now() + 60_000).toISOString(),
  };
}

describe("clienteApi", () => {
  beforeEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
    vi.restoreAllMocks();
  });

  it("deve retornar dados quando resposta de sucesso", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => ({ sucesso: true, dados: { valor: 42 } }),
    } as Response);

    const resposta = await requisitarApi<{ valor: number }>("/api/teste");

    expect(resposta.valor).toBe(42);
  });

  it("deve lancar falha de rede quando fetch falha", async () => {
    vi.spyOn(globalThis, "fetch").mockRejectedValue(new Error("offline"));

    await expect(requisitarApi("/api/teste")).rejects.toMatchObject({
      status: 0,
      codigo: "falha_rede",
    });
  });

  it("deve encerrar sessao local quando API responde 401", async () => {
    salvarSessao(criarSessao(), true);

    vi.spyOn(globalThis, "fetch").mockResolvedValue({
      ok: false,
      status: 401,
      headers: new Headers(),
      json: async () => ({
        status: 401,
        codigo: "nao_autenticado",
        mensagem: "Nao autenticado",
      }),
    } as Response);

    await expect(requisitarApi("/api/teste")).rejects.toBeInstanceOf(ErroRequisicaoApi);
    expect(window.localStorage.getItem("gerenciador_tarefas_sessao")).toBeNull();
  });

  it("deve ignorar sessao expirada e seguir requisicao sem cabecalho Authorization", async () => {
    salvarSessao(criarSessao(new Date(Date.now() - 60_000).toISOString()), true);
    const fetchSpy = vi.spyOn(globalThis, "fetch").mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => ({ sucesso: true, dados: { ok: true } }),
    } as Response);

    await expect(requisitarApi<{ ok: boolean }>("/api/teste")).resolves.toEqual({ ok: true });

    const opcoesRequisicao = fetchSpy.mock.calls[0]?.[1] as RequestInit | undefined;
    const headers = new Headers(opcoesRequisicao?.headers);
    expect(headers.get("Authorization")).toBeNull();
  });
});
