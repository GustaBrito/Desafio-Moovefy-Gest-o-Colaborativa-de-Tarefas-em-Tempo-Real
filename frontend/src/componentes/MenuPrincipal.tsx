import { NavLink } from "react-router-dom";
import { usarAutenticacao } from "../ganchos/usarAutenticacao";
import { usarNotificacao } from "../ganchos/usarNotificacao";

export function MenuPrincipal(): JSX.Element {
  const { sessao, realizarLogout, ehSuperAdmin, ehAdmin } = usarAutenticacao();
  const { historicoNotificacoes } = usarNotificacao();

  return (
    <header className="menu-principal">
      <div className="marca-sistema">
        <strong>Moovefy Tarefas</strong>
        <span>{sessao?.nome}</span>
        <small className="contador-notificacoes-menu">
          {historicoNotificacoes.length} notificacao(oes)
        </small>
      </div>

      <nav className="navegacao-principal">
        <NavLink to="/dashboard">Dashboard</NavLink>
        <NavLink to="/projetos">Projetos</NavLink>
        <NavLink to="/tarefas">Tarefas</NavLink>
        {(ehSuperAdmin || ehAdmin) && <NavLink to="/usuarios">Usuarios</NavLink>}
        {ehSuperAdmin && <NavLink to="/areas">Areas</NavLink>}
      </nav>

      <button className="botao-logout" onClick={realizarLogout} type="button">
        Sair
      </button>
    </header>
  );
}
