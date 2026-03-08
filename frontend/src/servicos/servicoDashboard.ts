import type { MetricasDashboardResposta } from "../tipos/dashboard";
import { requisitarApi } from "./clienteApi";

export async function obterMetricasDashboard(): Promise<MetricasDashboardResposta> {
  return requisitarApi<MetricasDashboardResposta>("/api/dashboard/metricas");
}
