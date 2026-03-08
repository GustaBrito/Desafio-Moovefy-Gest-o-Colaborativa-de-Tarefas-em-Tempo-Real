import { useContext } from "react";
import {
  ContextoAutenticacao,
  type ValorContextoAutenticacao,
} from "../contextos/ContextoAutenticacao";

export function usarAutenticacao(): ValorContextoAutenticacao {
  const contexto = useContext(ContextoAutenticacao);

  if (!contexto) {
    throw new Error(
      "O gancho usarAutenticacao deve ser utilizado dentro do ProvedorAutenticacao."
    );
  }

  return contexto;
}
