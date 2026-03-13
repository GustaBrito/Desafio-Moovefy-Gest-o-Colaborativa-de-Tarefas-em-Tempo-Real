import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { Aplicacao } from "./Aplicacao";

vi.mock("../contextos/ContextoAutenticacao", () => ({
  ProvedorAutenticacao: ({ children }: { children: import("react").ReactNode }) => (
    <div data-testid="provedor-autenticacao">{children}</div>
  ),
}));

vi.mock("../contextos/ContextoNotificacao", () => ({
  ProvedorNotificacao: ({ children }: { children: import("react").ReactNode }) => (
    <div data-testid="provedor-notificacao">{children}</div>
  ),
}));

vi.mock("../funcionalidades/notificacoes/InicializadorNotificacoesTempoReal", () => ({
  InicializadorNotificacoesTempoReal: () => <div data-testid="inicializador-notificacoes" />,
}));

vi.mock("../componentes/ListaNotificacoesToast", () => ({
  ListaNotificacoesToast: () => <div data-testid="lista-toasts" />,
}));

vi.mock("../rotas/RotasAplicacao", () => ({
  RotasAplicacao: () => <div data-testid="rotas-aplicacao" />,
}));

describe("Aplicacao", () => {
  it("deve montar arvore principal com provedores e rotas", () => {
    render(<Aplicacao />);

    expect(screen.getByTestId("provedor-autenticacao")).toBeInTheDocument();
    expect(screen.getByTestId("provedor-notificacao")).toBeInTheDocument();
    expect(screen.getByTestId("inicializador-notificacoes")).toBeInTheDocument();
    expect(screen.getByTestId("rotas-aplicacao")).toBeInTheDocument();
    expect(screen.getByTestId("lista-toasts")).toBeInTheDocument();
  });
});
