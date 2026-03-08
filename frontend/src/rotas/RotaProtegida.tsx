import { Navigate, Outlet, useLocation } from "react-router-dom";
import { usarAutenticacao } from "../ganchos/usarAutenticacao";

export function RotaProtegida(): JSX.Element {
  const { estaAutenticado } = usarAutenticacao();
  const localizacao = useLocation();

  if (!estaAutenticado) {
    return <Navigate to="/login" replace state={{ origem: localizacao.pathname }} />;
  }

  return <Outlet />;
}
