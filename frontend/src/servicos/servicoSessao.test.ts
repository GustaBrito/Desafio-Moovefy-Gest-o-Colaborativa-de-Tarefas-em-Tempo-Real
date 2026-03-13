import { beforeEach, describe, expect, it } from "vitest";
import type { SessaoAutenticacao } from "../tipos/autenticacao";
import {
  obterSessaoArmazenada,
  removerSessao,
  salvarSessao,
  sessaoExpirou,
} from "./servicoSessao";

function criarSessaoBase(expiraEmUtc?: string): SessaoAutenticacao {
  return {
    usuarioId: "u1",
    nome: "Usuario Teste",
    email: "usuario@teste.com",
    perfilGlobal: 1,
    areaIds: ["a1"],
    tokenAcesso: "token",
    tipoToken: "Bearer",
    expiraEmUtc: expiraEmUtc ?? new Date(Date.now() + 60_000).toISOString(),
  };
}

describe("servicoSessao", () => {
  beforeEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
  });

  it("deve salvar em sessionStorage quando lembrarSessao = false", () => {
    const sessao = criarSessaoBase();

    salvarSessao(sessao, false);

    expect(window.sessionStorage.getItem("gerenciador_tarefas_sessao")).not.toBeNull();
    expect(window.localStorage.getItem("gerenciador_tarefas_sessao")).toBeNull();
    expect(obterSessaoArmazenada()?.usuarioId).toBe("u1");
  });

  it("deve salvar em localStorage quando lembrarSessao = true", () => {
    const sessao = criarSessaoBase();

    salvarSessao(sessao, true);

    expect(window.localStorage.getItem("gerenciador_tarefas_sessao")).not.toBeNull();
    expect(window.sessionStorage.getItem("gerenciador_tarefas_sessao")).toBeNull();
  });

  it("deve invalidar sessao expirada e limpar armazenamento", () => {
    const sessaoExpirada = criarSessaoBase(new Date(Date.now() - 60_000).toISOString());
    window.localStorage.setItem("gerenciador_tarefas_sessao", JSON.stringify(sessaoExpirada));

    const sessao = obterSessaoArmazenada();

    expect(sessao).toBeNull();
    expect(window.localStorage.getItem("gerenciador_tarefas_sessao")).toBeNull();
  });

  it("deve remover sessao manualmente", () => {
    const sessao = criarSessaoBase();
    salvarSessao(sessao, true);

    removerSessao();

    expect(window.localStorage.getItem("gerenciador_tarefas_sessao")).toBeNull();
    expect(window.sessionStorage.getItem("gerenciador_tarefas_sessao")).toBeNull();
  });

  it("deve identificar corretamente expiracao da sessao", () => {
    const ativa = criarSessaoBase(new Date(Date.now() + 60_000).toISOString());
    const expirada = criarSessaoBase(new Date(Date.now() - 60_000).toISOString());

    expect(sessaoExpirou(ativa)).toBe(false);
    expect(sessaoExpirou(expirada)).toBe(true);
  });
});
