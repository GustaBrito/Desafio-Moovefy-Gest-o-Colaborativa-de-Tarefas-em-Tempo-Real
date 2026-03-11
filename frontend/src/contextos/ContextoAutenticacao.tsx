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
import {
  PerfilGlobalUsuario,
  type SessaoAutenticacao,
} from "../tipos/autenticacao";

export interface ValorContextoAutenticacao {
  sessao: SessaoAutenticacao | null;
  estaAutenticado: boolean;
  ehSuperAdmin: boolean;
  ehAdmin: boolean;
  ehColaborador: boolean;
  realizarLogin: (
    email: string,
    senha: string,
    lembrarSessao?: boolean
  ) => Promise<void>;
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

  const realizarLogin = useCallback(
    async (email: string, senha: string, lembrarSessao: boolean = true) => {
      const respostaLogin = await realizarLoginApi({ email, senha });

      const novaSessao: SessaoAutenticacao = {
        usuarioId: respostaLogin.usuarioId,
        nome: respostaLogin.nome,
        email: respostaLogin.email,
        perfilGlobal: respostaLogin.perfilGlobal,
        areaIds: respostaLogin.areaIds ?? [],
        tokenAcesso: respostaLogin.tokenAcesso,
        tipoToken: respostaLogin.tipoToken,
        expiraEmUtc: respostaLogin.expiraEmUtc,
      };

      salvarSessao(novaSessao, lembrarSessao);
      setSessao(novaSessao);
    },
    []
  );

  const realizarLogout = useCallback(() => {
    removerSessao();
    setSessao(null);
  }, []);

  const valorContexto = useMemo<ValorContextoAutenticacao>(
    () => ({
      sessao,
      estaAutenticado: Boolean(sessao?.tokenAcesso),
      ehSuperAdmin: sessao?.perfilGlobal === PerfilGlobalUsuario.SuperAdmin,
      ehAdmin: sessao?.perfilGlobal === PerfilGlobalUsuario.Admin,
      ehColaborador: sessao?.perfilGlobal === PerfilGlobalUsuario.Colaborador,
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
