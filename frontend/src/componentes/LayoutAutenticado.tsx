import { Outlet } from "react-router-dom";
import { MenuPrincipal } from "./MenuPrincipal";

export function LayoutAutenticado(): JSX.Element {
  return (
    <div className="layout-autenticado">
      <MenuPrincipal />
      <main className="area-conteudo">
        <Outlet />
      </main>
    </div>
  );
}
