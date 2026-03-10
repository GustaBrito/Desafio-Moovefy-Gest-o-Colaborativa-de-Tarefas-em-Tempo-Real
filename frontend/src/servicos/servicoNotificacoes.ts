import type { NotificacaoHistoricoResposta } from "../tipos/notificacoes";
import { requisitarApi } from "./clienteApi";

export async function listarHistoricoNotificacoes(
  limite: number = 20
): Promise<NotificacaoHistoricoResposta[]> {
  return requisitarApi<NotificacaoHistoricoResposta[]>(
    `/api/notificacoes?limite=${limite}`
  );
}
