import { useQuery } from "@tanstack/react-query";
import { PainelMetricasDashboard } from "../funcionalidades/dashboard/PainelMetricasDashboard";
import { obterMetricasDashboard } from "../servicos/servicoDashboard";

export function PaginaDashboard(): JSX.Element {
  const consultaMetricas = useQuery({
    queryKey: ["dashboard", "metricas"],
    queryFn: obterMetricasDashboard,
  });

  return (
    <section className="pagina-conteudo">
      <header className="cabecalho-pagina">
        <h1>Dashboard</h1>
        <p>Visao consolidada das metricas principais do time.</p>
      </header>

      {consultaMetricas.isLoading && <p>Carregando metricas...</p>}
      {consultaMetricas.isError && (
        <p className="mensagem-erro">Falha ao carregar metricas do dashboard.</p>
      )}

      {consultaMetricas.data && (
        <PainelMetricasDashboard metricas={consultaMetricas.data} />
      )}
    </section>
  );
}
