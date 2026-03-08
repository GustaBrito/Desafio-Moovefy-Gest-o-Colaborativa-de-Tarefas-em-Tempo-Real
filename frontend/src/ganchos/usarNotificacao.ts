import { useContext } from "react";
import {
  ContextoNotificacao,
  type ValorContextoNotificacao,
} from "../contextos/ContextoNotificacao";

export function usarNotificacao(): ValorContextoNotificacao {
  const contexto = useContext(ContextoNotificacao);

  if (!contexto) {
    throw new Error(
      "O gancho usarNotificacao deve ser utilizado dentro de ProvedorNotificacao."
    );
  }

  return contexto;
}
