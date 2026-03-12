import { useCallback, useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { FormularioLogin } from "../funcionalidades/autenticacao/FormularioLogin";
import { usarAutenticacao } from "../ganchos/usarAutenticacao";
import { usarNotificacao } from "../ganchos/usarNotificacao";
import {
  ErroRequisicaoApi,
  obterUrlBaseApi,
} from "../servicos/clienteApi";

type EstadoDisponibilidadeApi = "verificando" | "online" | "offline";

export function PaginaLogin(): JSX.Element {
  const { estaAutenticado, realizarLogin } = usarAutenticacao();
  const { mostrarErro, mostrarInformacao, mostrarSucesso } = usarNotificacao();
  const navegar = useNavigate();
  const [erro, setErro] = useState<string | null>(null);
  const [emEnvio, setEmEnvio] = useState(false);
  const [estadoApi, setEstadoApi] =
    useState<EstadoDisponibilidadeApi>("verificando");

  const mensagemStatusApi = useMemo(() => {
    if (estadoApi === "verificando") {
      return "Verificando conexao com a API...";
    }

    if (estadoApi === "online") {
      return "API disponivel para autenticacao.";
    }

    return "API indisponivel no momento.";
  }, [estadoApi]);

  useEffect(() => {
    if (estaAutenticado) {
      navegar("/dashboard", { replace: true });
    }
  }, [estaAutenticado, navegar]);

  const verificarDisponibilidadeApi = useCallback(async () => {
    const urlVerificacao = `${obterUrlBaseApi()}/api/autenticacao/saude`;
    const controladorAbort = new AbortController();
    const temporizadorAbort = window.setTimeout(() => {
      controladorAbort.abort();
    }, 4000);

    try {
      const resposta = await fetch(urlVerificacao, {
        method: "GET",
        signal: controladorAbort.signal,
      });

      setEstadoApi(resposta.ok ? "online" : "offline");
    } catch {
      setEstadoApi("offline");
    } finally {
      window.clearTimeout(temporizadorAbort);
    }
  }, []);

  useEffect(() => {
    void verificarDisponibilidadeApi();

    const identificadorIntervalo = window.setInterval(() => {
      void verificarDisponibilidadeApi();
    }, 30000);

    return () => {
      window.clearInterval(identificadorIntervalo);
    };
  }, [verificarDisponibilidadeApi]);

  function mapearMensagemErroLogin(excecao: unknown): string {
    if (excecao instanceof ErroRequisicaoApi) {
      if (excecao.status === 401) {
        return "Credenciais invalidas. Revise email e senha.";
      }

      if (excecao.status === 429) {
        if (excecao.retryAfterSegundos && excecao.retryAfterSegundos > 0) {
          return `Muitas tentativas de login. Aguarde ${excecao.retryAfterSegundos}s para tentar novamente.`;
        }

        return "Muitas tentativas de login. Aguarde alguns segundos e tente novamente.";
      }

      if (excecao.codigo === "falha_rede") {
        return "Sem conexao com a API. Verifique se o backend esta em execucao.";
      }
    }

    if (excecao instanceof Error) {
      return excecao.message;
    }

    return "Nao foi possivel autenticar. Verifique os dados.";
  }

  async function enviarFormulario(dados: {
    email: string;
    senha: string;
    lembrarSessao: boolean;
  }): Promise<void> {
    setErro(null);
    setEmEnvio(true);

    try {
      if (estadoApi === "offline") {
        mostrarInformacao("A API aparenta estar offline. Tentando autenticar mesmo assim...");
      }

      await realizarLogin(dados.email, dados.senha, dados.lembrarSessao);
      mostrarSucesso("Login realizado com sucesso.");
      navegar("/dashboard", { replace: true });
    } catch (excecao) {
      const mensagem = mapearMensagemErroLogin(excecao);
      setErro(mensagem);
      mostrarErro(mensagem);
    } finally {
      setEmEnvio(false);
    }
  }

  return (
    <main className="pagina-login">
      <section className="painel-login">
        <aside className="bloco-apresentacao-login">
          <span className="selo-apresentacao-login">Plataforma colaborativa</span>
          <h1>Gerenciador de tarefas para equipes de alta entrega</h1>
          <p>
            Centralize projetos, acompanhe metricas e monitore atividades em tempo
            real com uma experiencia objetiva e confiavel.
          </p>

          <ul className="lista-beneficios-login">
            <li>Visao unificada de projetos e tarefas</li>
            <li>Dashboard com indicadores de execucao</li>
            <li>Notificacoes instantaneas por responsavel</li>
            <li>Fluxo de autenticacao e auditoria por sessao</li>
          </ul>

          <div className="grade-indicadores-login">
            <article className="indicador-login">
              <strong>Tempo real</strong>
              <span>SignalR para notificacoes operacionais imediatas.</span>
            </article>
            <article className="indicador-login">
              <strong>Governanca</strong>
              <span>Controle de acesso por perfil e area da organizacao.</span>
            </article>
          </div>
        </aside>

        <section className="bloco-formulario-login">
          <FormularioLogin
            emEnvio={emEnvio}
            estadoApi={estadoApi}
            mensagemStatusApi={mensagemStatusApi}
            erro={erro}
            aoEnviar={enviarFormulario}
          />
        </section>
      </section>
    </main>
  );
}
