import type { StatusTarefa } from "./tarefas";

export interface TotalTarefasPorStatusResposta {
  status: StatusTarefa;
  total: number;
}

export interface MetricasDashboardResposta {
  totalTarefasPorStatus: TotalTarefasPorStatusResposta[];
  tarefasAtrasadas: number;
  tarefasConcluidasNoPrazo: number;
  taxaConclusao: number;
}
