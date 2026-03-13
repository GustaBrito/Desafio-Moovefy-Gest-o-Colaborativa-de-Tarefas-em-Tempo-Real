import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { LayoutAutenticado } from "./LayoutAutenticado";

vi.mock("./MenuPrincipal", () => ({
  MenuPrincipal: ({
    menuExpandido,
    aoAlternarMenu,
  }: {
    menuExpandido: boolean;
    aoAlternarMenu: () => void;
  }) => (
    <button type="button" onClick={aoAlternarMenu}>
      {menuExpandido ? "menu-expandido" : "menu-recolhido"}
    </button>
  ),
}));

describe("LayoutAutenticado", () => {
  beforeEach(() => {
    window.localStorage.clear();
  });

  it("deve iniciar expandido por padrao e persistir alternancia no localStorage", async () => {
    const usuario = userEvent.setup();

    const { container } = render(
      <MemoryRouter initialEntries={["/dashboard"]}>
        <Routes>
          <Route element={<LayoutAutenticado />}>
            <Route path="/dashboard" element={<div>conteudo dashboard</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByText("menu-expandido")).toBeInTheDocument();
    expect(container.querySelector(".layout-menu-expandido")).toBeTruthy();

    await usuario.click(screen.getByRole("button", { name: "menu-expandido" }));

    expect(screen.getByText("menu-recolhido")).toBeInTheDocument();
    expect(container.querySelector(".layout-menu-recolhido")).toBeTruthy();
    expect(window.localStorage.getItem("layout_menu_expandido_v1")).toBe("0");
  });
});
