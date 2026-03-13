import { StatusTarefa, type TarefaResposta } from "../../tipos/tarefas";

export function obterStatusPermitidos(statusAtual: StatusTarefa): StatusTarefa[] {
  if (statusAtual === StatusTarefa.Pendente) {
    return [StatusTarefa.Pendente, StatusTarefa.EmAndamento, StatusTarefa.Cancelada];
  }
  if (statusAtual === StatusTarefa.EmAndamento) {
    return [StatusTarefa.EmAndamento, StatusTarefa.Concluida, StatusTarefa.Cancelada];
  }
  if (statusAtual === StatusTarefa.Concluida) {
    return [StatusTarefa.Concluida];
  }
  return [StatusTarefa.Cancelada];
}

export function statusPermitido(
  statusAtual: StatusTarefa,
  novoStatus: StatusTarefa
): boolean {
  if (statusAtual === novoStatus) {
    return true;
  }
  return obterStatusPermitidos(statusAtual).includes(novoStatus);
}

export function tarefaVenceHoje(tarefa: TarefaResposta): boolean {
  const hoje = new Date();
  const prazo = new Date(tarefa.dataPrazo);

  return (
    prazo.getUTCFullYear() === hoje.getUTCFullYear() &&
    prazo.getUTCMonth() === hoje.getUTCMonth() &&
    prazo.getUTCDate() === hoje.getUTCDate()
  );
}

export function normalizarTexto(texto: string): string {
  return texto
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .trim()
    .toLowerCase();
}

export function converterParaDataInput(data: Date): string {
  return `${data.getFullYear()}-${String(data.getMonth() + 1).padStart(2, "0")}-${String(
    data.getDate()
  ).padStart(2, "0")}`;
}

export function converterIsoParaDataInput(dataIso: string): string {
  const data = new Date(dataIso);
  return converterParaDataInput(data);
}

export function escaparValorCsv(valor: string): string {
  return `"${valor.replace(/"/g, '""')}"`;
}

export function obterMensagemErro(
  excecao: unknown,
  mensagemPadrao: string
): string {
  if (excecao instanceof Error && excecao.message.trim().length > 0) {
    return excecao.message;
  }

  return mensagemPadrao;
}
