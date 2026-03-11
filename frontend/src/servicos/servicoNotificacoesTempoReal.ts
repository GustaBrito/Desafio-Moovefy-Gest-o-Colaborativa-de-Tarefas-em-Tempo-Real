import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";
import { obterUrlBaseApi } from "./clienteApi";

export const EVENTO_TAREFA_ATRIBUIDA = "tarefaAtribuida";
export const METODO_ENTRAR_CANAL_RESPONSAVEL = "EntrarNoCanalUsuarioAtualAsync";
export const METODO_SAIR_CANAL_RESPONSAVEL = "SairDoCanalUsuarioAtualAsync";

export function criarConexaoNotificacoesTempoReal(
  tokenAcesso: string
): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(`${obterUrlBaseApi()}/hubs/notificacoes`, {
      accessTokenFactory: () => tokenAcesso,
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
}
