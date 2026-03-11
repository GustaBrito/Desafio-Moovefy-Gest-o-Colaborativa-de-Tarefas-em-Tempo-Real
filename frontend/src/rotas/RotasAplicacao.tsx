import { Navigate, Route, Routes } from "react-router-dom";
import { LayoutAutenticado } from "../componentes/LayoutAutenticado";
import { PerfilGlobalUsuario } from "../tipos/autenticacao";
import { PaginaAreas } from "../paginas/PaginaAreas";
import { PaginaDashboard } from "../paginas/PaginaDashboard";
import { PaginaLogin } from "../paginas/PaginaLogin";
import { PaginaProjetos } from "../paginas/PaginaProjetos";
import { PaginaTarefas } from "../paginas/PaginaTarefas";
import { PaginaUsuarios } from "../paginas/PaginaUsuarios";
import { RotaProtegida } from "./RotaProtegida";

export function RotasAplicacao(): JSX.Element {
  return (
    <Routes>
      <Route path="/login" element={<PaginaLogin />} />

      <Route element={<RotaProtegida />}>
        <Route element={<LayoutAutenticado />}>
          <Route path="/dashboard" element={<PaginaDashboard />} />
          <Route path="/projetos" element={<PaginaProjetos />} />
          <Route path="/tarefas" element={<PaginaTarefas />} />

          <Route
            element={
              <RotaProtegida
                perfisPermitidos={[
                  PerfilGlobalUsuario.SuperAdmin,
                  PerfilGlobalUsuario.Admin,
                ]}
              />
            }
          >
            <Route path="/usuarios" element={<PaginaUsuarios />} />
          </Route>

          <Route
            element={
              <RotaProtegida
                perfisPermitidos={[PerfilGlobalUsuario.SuperAdmin]}
              />
            }
          >
            <Route path="/areas" element={<PaginaAreas />} />
          </Route>
        </Route>
      </Route>

      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
}
