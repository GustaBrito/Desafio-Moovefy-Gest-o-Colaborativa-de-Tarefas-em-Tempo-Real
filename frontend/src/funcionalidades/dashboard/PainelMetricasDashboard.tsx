import { CartaoMetrica } from "../../componentes/CartaoMetrica";
import type { MetricasDashboardResposta } from "../../tipos/dashboard";
import { StatusTarefa } from "../../tipos/tarefas";

interface PropriedadesPainelMetricasDashboard {
  metricas: MetricasDashboardResposta;
}

const nomesStatus: Record<StatusTarefa, string> = {
  [StatusTarefa.Pendente]: "Pendente",
  [StatusTarefa.EmAndamento]: "Em andamento",
  [StatusTarefa.Concluida]: "Concluida",
  [StatusTarefa.Cancelada]: "Cancelada",
};

export function PainelMetricasDashboard({
  metricas,
}: PropriedadesPainelMetricasDashboard): JSX.Element {
  return (
    <section className="painel-dashboard">
      <div className="grade-cartoes-metricas">
        <CartaoMetrica titulo="Tarefas atrasadas" valor={`${metricas.tarefasAtrasadas}`} />
        <CartaoMetrica
          titulo="Concluidas no prazo"
          valor={`${metricas.tarefasConcluidasNoPrazo}`}
        />
        <CartaoMetrica
          titulo="Taxa de conclusao"
          valor={`${metricas.taxaConclusao.toFixed(2)}%`}
        />
      </div>

      <article className="cartao-lista-status">
        <h3>Total por status</h3>
        <ul>
          {metricas.totalTarefasPorStatus.map((item) => (
            <li key={item.status}>
              <span>{nomesStatus[item.status]}</span>
              <strong>{item.total}</strong>
            </li>
          ))}
        </ul>
      </article>
    </section>
  );
}
