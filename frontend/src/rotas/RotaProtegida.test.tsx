import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { PerfilGlobalUsuario } from "../tipos/autenticacao";
import { RotaProtegida } from "./RotaProtegida";

let estadoAutenticacao = {
  estaAutenticado: false,
  sessao: null as
    | {
        perfilGlobal: PerfilGlobalUsuario;
      }
    | null,
};

vi.mock("../ganchos/usarAutenticacao", () => ({
  usarAutenticacao: () => ({
    ...estadoAutenticacao,
    ehSuperAdmin: false,
    ehAdmin: false,
    ehColaborador: true,
    realizarLogin: vi.fn(),
    realizarLogout: vi.fn(),
  }),
}));

describe("RotaProtegida", () => {
  beforeEach(() => {
    estadoAutenticacao = {
      estaAutenticado: false,
      sessao: null,
    };
  });

  function renderizarRota(perfisPermitidos?: PerfilGlobalUsuario[]) {
    render(
      <MemoryRouter initialEntries={["/restrita"]}>
        <Routes>
          <Route element={<RotaProtegida perfisPermitidos={perfisPermitidos} />}>
            <Route path="/restrita" element={<div>pagina restrita</div>} />
          </Route>
          <Route path="/login" element={<div>pagina login</div>} />
          <Route path="/dashboard" element={<div>pagina dashboard</div>} />
        </Routes>
      </MemoryRouter>
    );
  }

  it("deve redirecionar para login quando nao autenticado", () => {
    renderizarRota();
    expect(screen.getByText("pagina login")).toBeInTheDocument();
  });

  it("deve redirecionar para dashboard quando perfil nao permitido", () => {
    estadoAutenticacao = {
      estaAutenticado: true,
      sessao: { perfilGlobal: PerfilGlobalUsuario.Colaborador },
    };

    renderizarRota([PerfilGlobalUsuario.SuperAdmin]);
    expect(screen.getByText("pagina dashboard")).toBeInTheDocument();
  });

  it("deve renderizar conteudo quando autenticado e perfil permitido", () => {
    estadoAutenticacao = {
      estaAutenticado: true,
      sessao: { perfilGlobal: PerfilGlobalUsuario.Admin },
    };

    renderizarRota([PerfilGlobalUsuario.Admin, PerfilGlobalUsuario.SuperAdmin]);
    expect(screen.getByText("pagina restrita")).toBeInTheDocument();
  });
});
