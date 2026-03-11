import { Navigate, Outlet, useLocation } from "react-router-dom";
import { usarAutenticacao } from "../ganchos/usarAutenticacao";
import type { PerfilGlobalUsuario } from "../tipos/autenticacao";

interface PropriedadesRotaProtegida {
  perfisPermitidos?: PerfilGlobalUsuario[];
}

export function RotaProtegida({
  perfisPermitidos,
}: PropriedadesRotaProtegida = {}): JSX.Element {
  const { estaAutenticado, sessao } = usarAutenticacao();
  const localizacao = useLocation();

  if (!estaAutenticado) {
    return <Navigate to="/login" replace state={{ origem: localizacao.pathname }} />;
  }

  if (
    perfisPermitidos &&
    perfisPermitidos.length > 0 &&
    sessao &&
    !perfisPermitidos.includes(sessao.perfilGlobal)
  ) {
    return <Navigate to="/dashboard" replace />;
  }

  return <Outlet />;
}
