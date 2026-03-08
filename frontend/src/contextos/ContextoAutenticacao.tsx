import {
  createContext,
  useCallback,
  useMemo,
  useState,
  type PropsWithChildren,
} from "react";
import { realizarLogin as realizarLoginApi } from "../servicos/servicoAutenticacao";
import {
  obterSessaoArmazenada,
  removerSessao,
  salvarSessao,
} from "../servicos/servicoSessao";
import type { SessaoAutenticacao } from "../tipos/autenticacao";

export interface ValorContextoAutenticacao {
  sessao: SessaoAutenticacao | null;
  estaAutenticado: boolean;
  realizarLogin: (email: string, senha: string) => Promise<void>;
  realizarLogout: () => void;
}

export const ContextoAutenticacao = createContext<ValorContextoAutenticacao | null>(
  null
);

export function ProvedorAutenticacao({
  children,
}: PropsWithChildren): JSX.Element {
  const [sessao, setSessao] = useState<SessaoAutenticacao | null>(
    obterSessaoArmazenada()
  );

  const realizarLogin = useCallback(async (email: string, senha: string) => {
    const respostaLogin = await realizarLoginApi({ email, senha });

    const novaSessao: SessaoAutenticacao = {
      usuarioId: respostaLogin.usuarioId,
      nome: respostaLogin.nome,
      email: respostaLogin.email,
      perfil: respostaLogin.perfil,
      tokenAcesso: respostaLogin.tokenAcesso,
      tipoToken: respostaLogin.tipoToken,
      expiraEmUtc: respostaLogin.expiraEmUtc,
    };

    salvarSessao(novaSessao);
    setSessao(novaSessao);
  }, []);

  const realizarLogout = useCallback(() => {
    removerSessao();
    setSessao(null);
  }, []);

  const valorContexto = useMemo<ValorContextoAutenticacao>(
    () => ({
      sessao,
      estaAutenticado: Boolean(sessao?.tokenAcesso),
      realizarLogin,
      realizarLogout,
    }),
    [sessao, realizarLogin, realizarLogout]
  );

  return (
    <ContextoAutenticacao.Provider value={valorContexto}>
      {children}
    </ContextoAutenticacao.Provider>
  );
}
