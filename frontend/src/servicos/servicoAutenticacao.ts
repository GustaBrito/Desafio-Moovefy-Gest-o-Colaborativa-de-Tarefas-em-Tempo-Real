import type { LoginRequisicao, LoginResposta } from "../tipos/autenticacao";
import { requisitarApi } from "./clienteApi";

export async function realizarLogin(
  requisicao: LoginRequisicao
): Promise<LoginResposta> {
  return requisitarApi<LoginResposta>("/api/autenticacao/login", {
    method: "POST",
    corpo: requisicao,
  });
}
