import { NavLink } from "react-router-dom";
import { usarAutenticacao } from "../ganchos/usarAutenticacao";
import { usarNotificacao } from "../ganchos/usarNotificacao";

interface PropriedadesMenuPrincipal {
  menuExpandido: boolean;
  aoAlternarMenu: () => void;
}

export function MenuPrincipal({
  menuExpandido,
  aoAlternarMenu,
}: PropriedadesMenuPrincipal): JSX.Element {
  const { sessao, realizarLogout, ehSuperAdmin, ehAdmin } = usarAutenticacao();
  const { historicoNotificacoes } = usarNotificacao();

  const itensNavegacao = [
    {
      rota: "/dashboard",
      rotulo: "Dashboard",
      visivel: true,
      icone: (
        <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true" focusable="false">
          <path
            d="M3 13H10V21H3V13ZM14 3H21V10H14V3ZM14 13H21V21H14V13ZM3 3H10V10H3V3Z"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.8"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      ),
    },
    {
      rota: "/projetos",
      rotulo: "Projetos",
      visivel: true,
      icone: (
        <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true" focusable="false">
          <path
            d="M3 7H9L11 9H21V19H3V7Z"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.8"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      ),
    },
    {
      rota: "/tarefas",
      rotulo: "Tarefas",
      visivel: true,
      icone: (
        <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true" focusable="false">
          <path
            d="M8 6H21M8 12H21M8 18H21M3 6H3.01M3 12H3.01M3 18H3.01"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.8"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      ),
    },
    {
      rota: "/usuarios",
      rotulo: "Usuarios",
      visivel: ehSuperAdmin || ehAdmin,
      icone: (
        <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true" focusable="false">
          <path
            d="M16 21V19C16 17.34 14.66 16 13 16H6C4.34 16 3 17.34 3 19V21M9.5 12C11.43 12 13 10.43 13 8.5C13 6.57 11.43 5 9.5 5C7.57 5 6 6.57 6 8.5C6 10.43 7.57 12 9.5 12ZM18 8V14M21 11H15"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.8"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      ),
    },
    {
      rota: "/areas",
      rotulo: "Areas",
      visivel: ehSuperAdmin,
      icone: (
        <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true" focusable="false">
          <path
            d="M3 3H10V10H3V3ZM14 3H21V10H14V3ZM3 14H10V21H3V14ZM14 14H21V21H14V14Z"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.8"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      ),
    },
  ];

  return (
    <header
      className={`menu-principal ${
        menuExpandido ? "menu-principal-expandido" : "menu-principal-recolhido"
      }`}
    >
      <div className="cabecalho-menu-lateral">
        <div className="marca-sistema">
          <strong>{menuExpandido ? "Moovefy Tarefas" : "MT"}</strong>
          <span>{sessao?.nome}</span>
          <small className="contador-notificacoes-menu">
            {historicoNotificacoes.length} notificacao(oes)
          </small>
        </div>

        <button
          type="button"
          className="botao-alternar-menu"
          onClick={aoAlternarMenu}
          aria-label={menuExpandido ? "Recolher menu lateral" : "Expandir menu lateral"}
          title={menuExpandido ? "Recolher menu" : "Expandir menu"}
        >
          {menuExpandido ? "◀" : "▶"}
        </button>
      </div>

      <nav className="navegacao-principal">
        {itensNavegacao
          .filter((item) => item.visivel)
          .map((item) => (
            <NavLink
              key={item.rota}
              to={item.rota}
              title={item.rotulo}
              className={({ isActive }) =>
                `link-navegacao-menu${isActive ? " active" : ""}`
              }
            >
              <span className="icone-link-menu">{item.icone}</span>
              <span className="texto-link-menu">{item.rotulo}</span>
            </NavLink>
          ))}
      </nav>

      <button className="botao-logout" onClick={realizarLogout} type="button">
        <span className="icone-link-menu" aria-hidden="true">
          <svg viewBox="0 0 24 24" width="16" height="16" focusable="false">
            <path
              d="M9 21H5C3.9 21 3 20.1 3 19V5C3 3.9 3.9 3 5 3H9M16 17L21 12L16 7M21 12H9"
              fill="none"
              stroke="currentColor"
              strokeWidth="1.8"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
        </span>
        <span className="texto-botao-logout">Sair</span>
      </button>
    </header>
  );
}
