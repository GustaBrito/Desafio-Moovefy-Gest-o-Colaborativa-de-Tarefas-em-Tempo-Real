import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { FormularioLogin } from "../funcionalidades/autenticacao/FormularioLogin";
import { usarAutenticacao } from "../ganchos/usarAutenticacao";

export function PaginaLogin(): JSX.Element {
  const { estaAutenticado, realizarLogin } = usarAutenticacao();
  const navegar = useNavigate();
  const [erro, setErro] = useState<string | null>(null);
  const [emEnvio, setEmEnvio] = useState(false);

  useEffect(() => {
    if (estaAutenticado) {
      navegar("/dashboard", { replace: true });
    }
  }, [estaAutenticado, navegar]);

  async function enviarFormulario(dados: {
    email: string;
    senha: string;
  }): Promise<void> {
    setErro(null);
    setEmEnvio(true);

    try {
      await realizarLogin(dados.email, dados.senha);
      navegar("/dashboard", { replace: true });
    } catch (excecao) {
      const mensagem =
        excecao instanceof Error
          ? excecao.message
          : "Nao foi possivel autenticar. Verifique os dados.";
      setErro(mensagem);
    } finally {
      setEmEnvio(false);
    }
  }

  return (
    <main className="pagina-login">
      <FormularioLogin emEnvio={emEnvio} erro={erro} aoEnviar={enviarFormulario} />
    </main>
  );
}
