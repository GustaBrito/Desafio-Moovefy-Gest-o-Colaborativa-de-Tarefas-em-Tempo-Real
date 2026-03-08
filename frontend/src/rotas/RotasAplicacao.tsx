import { Navigate, Route, Routes } from "react-router-dom";
import { LayoutAutenticado } from "../componentes/LayoutAutenticado";
import { PaginaDashboard } from "../paginas/PaginaDashboard";
import { PaginaLogin } from "../paginas/PaginaLogin";
import { PaginaProjetos } from "../paginas/PaginaProjetos";
import { PaginaTarefas } from "../paginas/PaginaTarefas";
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
        </Route>
      </Route>

      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
}
