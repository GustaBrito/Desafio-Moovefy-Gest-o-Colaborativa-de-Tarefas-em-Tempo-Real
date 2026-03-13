import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { RotasAplicacao } from "./RotasAplicacao";

vi.mock("../componentes/LayoutAutenticado", () => ({
  LayoutAutenticado: () => <div>layout-autenticado</div>,
}));

vi.mock("../rotas/RotaProtegida", () => ({
  RotaProtegida: () => <div>rota-protegida</div>,
}));

vi.mock("../paginas/PaginaLogin", () => ({
  PaginaLogin: () => <div>pagina-login</div>,
}));

vi.mock("../paginas/PaginaDashboard", () => ({
  PaginaDashboard: () => <div>pagina-dashboard</div>,
}));

vi.mock("../paginas/PaginaProjetos", () => ({
  PaginaProjetos: () => <div>pagina-projetos</div>,
}));

vi.mock("../paginas/PaginaTarefas", () => ({
  PaginaTarefas: () => <div>pagina-tarefas</div>,
}));

vi.mock("../paginas/PaginaUsuarios", () => ({
  PaginaUsuarios: () => <div>pagina-usuarios</div>,
}));

vi.mock("../paginas/PaginaAreas", () => ({
  PaginaAreas: () => <div>pagina-areas</div>,
}));

describe("RotasAplicacao", () => {
  it("deve renderizar pagina de login em /login", () => {
    render(
      <MemoryRouter initialEntries={["/login"]}>
        <RotasAplicacao />
      </MemoryRouter>
    );

    expect(screen.getByText("pagina-login")).toBeInTheDocument();
  });

  it("deve usar fallback para rotas protegidas quando caminho desconhecido", () => {
    render(
      <MemoryRouter initialEntries={["/rota-inexistente"]}>
        <RotasAplicacao />
      </MemoryRouter>
    );

    expect(screen.getByText("rota-protegida")).toBeInTheDocument();
  });
});
