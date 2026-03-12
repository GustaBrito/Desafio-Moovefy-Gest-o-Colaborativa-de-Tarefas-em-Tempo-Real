import { useEffect, useState } from "react";
import { Outlet } from "react-router-dom";
import { MenuPrincipal } from "./MenuPrincipal";

const CHAVE_MENU_EXPANDIDO = "layout_menu_expandido_v1";

export function LayoutAutenticado(): JSX.Element {
  const [menuExpandido, setMenuExpandido] = useState<boolean>(() => {
    const valorSalvo = window.localStorage.getItem(CHAVE_MENU_EXPANDIDO);
    if (valorSalvo === null) {
      return true;
    }

    return valorSalvo === "1";
  });

  useEffect(() => {
    window.localStorage.setItem(CHAVE_MENU_EXPANDIDO, menuExpandido ? "1" : "0");
  }, [menuExpandido]);

  return (
    <div
      className={`layout-autenticado ${
        menuExpandido ? "layout-menu-expandido" : "layout-menu-recolhido"
      }`}
    >
      <MenuPrincipal
        menuExpandido={menuExpandido}
        aoAlternarMenu={() => setMenuExpandido((valorAtual) => !valorAtual)}
      />
      <main className="area-conteudo">
        <Outlet />
      </main>
    </div>
  );
}
