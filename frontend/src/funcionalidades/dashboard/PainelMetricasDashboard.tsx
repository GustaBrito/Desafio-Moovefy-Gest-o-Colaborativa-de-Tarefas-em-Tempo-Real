import { CartaoMetrica } from "../../componentes/CartaoMetrica";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Legend,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { PrioridadeTarefa, StatusTarefa } from "../../tipos/tarefas";

export interface CartaoIndicadorDashboard {
  id: string;
  titulo: string;
  valor: string;
  subtitulo: string;
  variacao?: number;
  tipoDestaque?: "neutro" | "alerta" | "sucesso";
}

export interface TotalStatusDashboard {
  status: StatusTarefa;
  rotulo: string;
  total: number;
  cor: string;
}

export interface TotalPrioridadeDashboard {
  prioridade: PrioridadeTarefa;
  rotulo: string;
  total: number;
  cor: string;
}

export interface TarefaResumoDashboard {
  id: string;
  titulo: string;
  projetoNome?: string;
  dataPrazo: string;
  dataConclusao?: string | null;
  status: StatusTarefa;
  prioridade: PrioridadeTarefa;
  estaAtrasada: boolean;
}

interface PropriedadesPainelMetricasDashboard {
  cartoes: CartaoIndicadorDashboard[];
  totaisStatus: TotalStatusDashboard[];
  totaisPrioridade: TotalPrioridadeDashboard[];
  tarefasAtencao: TarefaResumoDashboard[];
  tarefasConcluidasRecentes: TarefaResumoDashboard[];
  aoExportarCsv: () => void;
  aoCopiarResumo: () => void;
  aoIrParaTarefas: () => void;
}

export function PainelMetricasDashboard({
  cartoes,
  totaisStatus,
  totaisPrioridade,
  tarefasAtencao,
  tarefasConcluidasRecentes,
  aoExportarCsv,
  aoCopiarResumo,
  aoIrParaTarefas,
}: PropriedadesPainelMetricasDashboard): JSX.Element {
  return (
    <section className="painel-dashboard">
      <div className="grade-cartoes-metricas">
        {cartoes.map((cartao) => (
          <CartaoMetrica
            key={cartao.id}
            titulo={cartao.titulo}
            valor={cartao.valor}
            subtitulo={cartao.subtitulo}
            variacao={cartao.variacao}
            tipoDestaque={cartao.tipoDestaque}
          />
        ))}
      </div>

      <div className="grade-graficos-dashboard">
        <article className="cartao-dashboard-grafico">
          <header className="cabecalho-cartao-dashboard">
            <h3>Total por status</h3>
            <span>Distribuicao atual</span>
          </header>
          <div className="container-grafico-dashboard">
            <ResponsiveContainer width="100%" height={250}>
              <PieChart>
                <Pie
                  data={totaisStatus}
                  dataKey="total"
                  nameKey="rotulo"
                  cx="50%"
                  cy="50%"
                  outerRadius={85}
                  innerRadius={48}
                  paddingAngle={3}
                >
                  {totaisStatus.map((item) => (
                    <Cell key={item.status} fill={item.cor} />
                  ))}
                </Pie>
                <Tooltip formatter={(valor) => [`${valor ?? 0}`, "Total"]} />
                <Legend />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </article>

        <article className="cartao-dashboard-grafico">
          <header className="cabecalho-cartao-dashboard">
            <h3>Distribuicao por prioridade</h3>
            <span>Carteira ativa</span>
          </header>
          <div className="container-grafico-dashboard">
            <ResponsiveContainer width="100%" height={250}>
              <BarChart data={totaisPrioridade} layout="vertical" margin={{ left: 12 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
                <XAxis type="number" allowDecimals={false} />
                <YAxis type="category" dataKey="rotulo" width={80} tick={{ fontSize: 12 }} />
                <Tooltip formatter={(valor) => [`${valor ?? 0}`, "Total"]} />
                <Bar dataKey="total" radius={[0, 4, 4, 0]}>
                  {totaisPrioridade.map((item) => (
                    <Cell key={item.prioridade} fill={item.cor} />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </div>
        </article>
      </div>

      <div className="acoes-painel-dashboard">
        <button type="button" className="botao-secundario" onClick={aoExportarCsv}>
          Exportar CSV
        </button>
        <button type="button" className="botao-secundario" onClick={aoCopiarResumo}>
          Copiar resumo
        </button>
        <button type="button" className="botao-texto" onClick={aoIrParaTarefas}>
          Ir para tarefas
        </button>
      </div>

      <div className="grade-listas-dashboard">
        <article className="cartao-dashboard-lista">
          <header className="cabecalho-cartao-dashboard">
            <h3>Tarefas que exigem atencao</h3>
            <span>Atrasadas ou em andamento</span>
          </header>

          {tarefasAtencao.length === 0 ? (
            <p className="mensagem-vazia-dashboard">
              Nenhuma tarefa critica no periodo selecionado.
            </p>
          ) : (
            <ul>
              {tarefasAtencao.map((tarefa) => (
                <li key={tarefa.id}>
                  <strong>{tarefa.titulo}</strong>
                  <span>
                    Prazo: {formatarData(tarefa.dataPrazo)}
                    {tarefa.projetoNome ? ` | Projeto: ${tarefa.projetoNome}` : ""}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </article>

        <article className="cartao-dashboard-lista">
          <header className="cabecalho-cartao-dashboard">
            <h3>Concluidas recentemente</h3>
            <span>Ultimas entregas</span>
          </header>

          {tarefasConcluidasRecentes.length === 0 ? (
            <p className="mensagem-vazia-dashboard">
              Sem conclusoes recentes no periodo selecionado.
            </p>
          ) : (
            <ul>
              {tarefasConcluidasRecentes.map((tarefa) => (
                <li key={tarefa.id}>
                  <strong>{tarefa.titulo}</strong>
                  <span>
                    Conclusao: {formatarData(tarefa.dataConclusao)}
                    {tarefa.projetoNome ? ` | Projeto: ${tarefa.projetoNome}` : ""}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </article>
      </div>
    </section>
  );
}

function formatarData(data?: string | null): string {
  if (!data) {
    return "Nao informado";
  }

  return new Date(data).toLocaleDateString("pt-BR");
}
