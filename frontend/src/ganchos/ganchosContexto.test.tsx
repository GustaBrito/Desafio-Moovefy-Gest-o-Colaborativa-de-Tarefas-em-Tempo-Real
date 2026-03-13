import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { ContextoAutenticacao } from "../contextos/ContextoAutenticacao";
import { ContextoNotificacao } from "../contextos/ContextoNotificacao";
import { usarAutenticacao } from "./usarAutenticacao";
import { usarNotificacao } from "./usarNotificacao";

function ComponenteAutenticacao(): JSX.Element {
  const contexto = usarAutenticacao();
  return <span>{contexto.sessao?.nome ?? "sem-sessao"}</span>;
}

function ComponenteNotificacao(): JSX.Element {
  const contexto = usarNotificacao();
  return <span>{contexto.notificacoes.length}</span>;
}

describe("ganchos de contexto", () => {
  it("deve retornar contexto de autenticacao quando provider existe", () => {
    render(
      <ContextoAutenticacao.Provider
        value={{
          sessao: {
            usuarioId: "1",
            nome: "Super Admin",
            email: "superadmin@empresa.com",
            tokenAcesso: "token",
            tipoToken: "Bearer",
            expiraEmUtc: new Date(Date.now() + 60000).toISOString(),
            areaIds: [],
            perfilGlobal: 1,
          },
          estaAutenticado: true,
          ehSuperAdmin: true,
          ehAdmin: false,
          ehColaborador: false,
          realizarLogin: vi.fn(),
          realizarLogout: vi.fn(),
        }}
      >
        <ComponenteAutenticacao />
      </ContextoAutenticacao.Provider>
    );

    expect(screen.getByText("Super Admin")).toBeInTheDocument();
  });

  it("deve retornar contexto de notificacao quando provider existe", () => {
    render(
      <ContextoNotificacao.Provider
        value={{
          notificacoes: [{ id: 1, tipo: "sucesso", mensagem: "ok" }],
          historicoNotificacoes: [],
          mostrarSucesso: vi.fn(),
          mostrarErro: vi.fn(),
          mostrarInformacao: vi.fn(),
          removerNotificacao: vi.fn(),
          definirHistoricoNotificacoes: vi.fn(),
          adicionarNotificacaoHistorico: vi.fn(),
        }}
      >
        <ComponenteNotificacao />
      </ContextoNotificacao.Provider>
    );

    expect(screen.getByText("1")).toBeInTheDocument();
  });
});
