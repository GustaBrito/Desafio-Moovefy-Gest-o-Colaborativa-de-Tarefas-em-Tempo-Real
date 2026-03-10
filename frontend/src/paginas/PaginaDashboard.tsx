import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import {
  PainelMetricasDashboard,
  type CartaoIndicadorDashboard,
  type PontoEvolucaoDashboard,
  type TarefaResumoDashboard,
  type TotalPrioridadeDashboard,
  type TotalStatusDashboard,
} from "../funcionalidades/dashboard/PainelMetricasDashboard";
import { usarNotificacao } from "../ganchos/usarNotificacao";
import { obterMetricasDashboard } from "../servicos/servicoDashboard";
import { listarProjetos } from "../servicos/servicoProjetos";
import { listarTodasTarefas } from "../servicos/servicoTarefas";
import type { ProjetoResposta } from "../tipos/projetos";
import { PrioridadeTarefa, StatusTarefa, type TarefaResposta } from "../tipos/tarefas";

type PeriodoDashboard = 7 | 15 | 30;

interface ResumoSnapshotDashboard {
  totalTarefas: number;
  taxaConclusao: number;
  tarefasAtrasadas: number;
  tarefasConcluidasNoPrazo: number;
}

const CHAVE_SNAPSHOT_DASHBOARD = "dashboard_snapshot_metricas_v1";

const nomesStatus: Record<StatusTarefa, string> = {
  [StatusTarefa.Pendente]: "Pendente",
  [StatusTarefa.EmAndamento]: "Em andamento",
  [StatusTarefa.Concluida]: "Concluida",
  [StatusTarefa.Cancelada]: "Cancelada",
};

const nomesPrioridade: Record<PrioridadeTarefa, string> = {
  [PrioridadeTarefa.Baixa]: "Baixa",
  [PrioridadeTarefa.Media]: "Media",
  [PrioridadeTarefa.Alta]: "Alta",
  [PrioridadeTarefa.Urgente]: "Urgente",
};

const coresStatus: Record<StatusTarefa, string> = {
  [StatusTarefa.Pendente]: "#f59e0b",
  [StatusTarefa.EmAndamento]: "#3b82f6",
  [StatusTarefa.Concluida]: "#10b981",
  [StatusTarefa.Cancelada]: "#94a3b8",
};

const coresPrioridade: Record<PrioridadeTarefa, string> = {
  [PrioridadeTarefa.Baixa]: "#22c55e",
  [PrioridadeTarefa.Media]: "#3b82f6",
  [PrioridadeTarefa.Alta]: "#f59e0b",
  [PrioridadeTarefa.Urgente]: "#ef4444",
};

export function PaginaDashboard(): JSX.Element {
  const navegar = useNavigate();
  const { mostrarErro, mostrarInformacao, mostrarSucesso } = usarNotificacao();
  const [periodoSelecionado, setPeriodoSelecionado] =
    useState<PeriodoDashboard>(15);
  const [projetoSelecionado, setProjetoSelecionado] = useState("");
  const [atualizacaoAutomatica, setAtualizacaoAutomatica] = useState(true);
  const [snapshotInicial] = useState<ResumoSnapshotDashboard | null>(
    () => lerSnapshotDashboard()
  );

  const consultaMetricas = useQuery({
    queryKey: ["dashboard", "metricas"],
    queryFn: obterMetricasDashboard,
    refetchInterval: atualizacaoAutomatica ? 30000 : false,
  });

  const consultaProjetos = useQuery({
    queryKey: ["projetos", "resumo"],
    queryFn: listarProjetos,
    staleTime: 120000,
    refetchInterval: atualizacaoAutomatica ? 30000 : false,
  });

  const consultaTarefas = useQuery({
    queryKey: ["dashboard", "tarefas", projetoSelecionado],
    queryFn: () =>
      listarTodasTarefas({
        projetoId: projetoSelecionado || undefined,
      }),
    refetchInterval: atualizacaoAutomatica ? 30000 : false,
  });

  const tarefasBase = consultaTarefas.data ?? [];
  const tarefasPeriodo = filtrarPorPeriodo(tarefasBase, periodoSelecionado);

  const metricasPeriodo = useMemo(
    () => calcularMetricasPorPeriodo(tarefasPeriodo),
    [tarefasPeriodo]
  );

  const totaisStatusApi = useMemo(() => {
    if (projetoSelecionado || !consultaMetricas.data) {
      return null;
    }

    const totalPorStatusApi = new Map<StatusTarefa, number>(
      consultaMetricas.data.totalTarefasPorStatus.map((item) => [item.status, item.total])
    );

    return Object.values(StatusTarefa)
      .filter((valor): valor is StatusTarefa => typeof valor === "number")
      .map((status) => ({
        status,
        rotulo: nomesStatus[status],
        total: totalPorStatusApi.get(status) ?? 0,
        cor: coresStatus[status],
      }));
  }, [consultaMetricas.data, projetoSelecionado]);

  const totaisStatus = useMemo<TotalStatusDashboard[]>(
    () => {
      if (totaisStatusApi) {
        return totaisStatusApi;
      }

      return Object.values(StatusTarefa)
        .filter((valor): valor is StatusTarefa => typeof valor === "number")
        .map((status) => ({
          status,
          rotulo: nomesStatus[status],
          total: metricasPeriodo.totalPorStatus.get(status) ?? 0,
          cor: coresStatus[status],
        }));
    },
    [metricasPeriodo.totalPorStatus, totaisStatusApi]
  );

  const totaisPrioridade = useMemo<TotalPrioridadeDashboard[]>(
    () =>
      Object.values(PrioridadeTarefa)
        .filter((valor): valor is PrioridadeTarefa => typeof valor === "number")
        .map((prioridade) => ({
          prioridade,
          rotulo: nomesPrioridade[prioridade],
          total: metricasPeriodo.totalPorPrioridade.get(prioridade) ?? 0,
          cor: coresPrioridade[prioridade],
        })),
    [metricasPeriodo.totalPorPrioridade]
  );

  const serieEvolucao = useMemo<PontoEvolucaoDashboard[]>(
    () => gerarSerieEvolucaoDiaria(tarefasPeriodo, periodoSelecionado),
    [tarefasPeriodo, periodoSelecionado]
  );

  const mapaProjetos = useMemo(
    () => criarMapaProjetos(consultaProjetos.data ?? []),
    [consultaProjetos.data]
  );

  const tarefasAtencao = useMemo<TarefaResumoDashboard[]>(
    () =>
      tarefasPeriodo
        .filter(
          (tarefa) =>
            tarefa.estaAtrasada || tarefa.status === StatusTarefa.EmAndamento
        )
        .sort(
          (a, b) =>
            new Date(a.dataPrazo).getTime() - new Date(b.dataPrazo).getTime()
        )
        .slice(0, 6)
        .map((tarefa) => mapearTarefaResumo(tarefa, mapaProjetos)),
    [mapaProjetos, tarefasPeriodo]
  );

  const tarefasConcluidasRecentes = useMemo<TarefaResumoDashboard[]>(
    () =>
      tarefasPeriodo
        .filter(
          (tarefa) =>
            tarefa.status === StatusTarefa.Concluida && Boolean(tarefa.dataConclusao)
        )
        .sort(
          (a, b) =>
            new Date(b.dataConclusao ?? 0).getTime() -
            new Date(a.dataConclusao ?? 0).getTime()
        )
        .slice(0, 6)
        .map((tarefa) => mapearTarefaResumo(tarefa, mapaProjetos)),
    [mapaProjetos, tarefasPeriodo]
  );

  const snapshotAtual: ResumoSnapshotDashboard = useMemo(
    () => ({
      totalTarefas: metricasPeriodo.totalTarefas,
      taxaConclusao: metricasPeriodo.taxaConclusao,
      tarefasAtrasadas: metricasPeriodo.tarefasAtrasadas,
      tarefasConcluidasNoPrazo: metricasPeriodo.tarefasConcluidasNoPrazo,
    }),
    [metricasPeriodo]
  );

  useEffect(() => {
    salvarSnapshotDashboard(snapshotAtual);
  }, [snapshotAtual]);

  const cartoes = useMemo<CartaoIndicadorDashboard[]>(
    () => [
      {
        id: "total_tarefas",
        titulo: "Total no periodo",
        valor: `${metricasPeriodo.totalTarefas}`,
        subtitulo: `${periodoSelecionado} dias`,
        variacao: calcularVariacao(
          snapshotInicial?.totalTarefas,
          metricasPeriodo.totalTarefas
        ),
      },
      {
        id: "taxa_conclusao",
        titulo: "Taxa de conclusao",
        valor: `${metricasPeriodo.taxaConclusao.toFixed(2)}%`,
        subtitulo: "Entrega no periodo",
        variacao: calcularVariacao(
          snapshotInicial?.taxaConclusao,
          metricasPeriodo.taxaConclusao
        ),
        tipoDestaque: "sucesso",
      },
      {
        id: "tarefas_atrasadas",
        titulo: "Tarefas atrasadas",
        valor: `${metricasPeriodo.tarefasAtrasadas}`,
        subtitulo: "Demandas criticas",
        variacao: calcularVariacao(
          snapshotInicial?.tarefasAtrasadas,
          metricasPeriodo.tarefasAtrasadas
        ),
        tipoDestaque: "alerta",
      },
      {
        id: "concluidas_no_prazo",
        titulo: "Concluidas no prazo",
        valor: `${metricasPeriodo.tarefasConcluidasNoPrazo}`,
        subtitulo: "Qualidade de entrega",
        variacao: calcularVariacao(
          snapshotInicial?.tarefasConcluidasNoPrazo,
          metricasPeriodo.tarefasConcluidasNoPrazo
        ),
        tipoDestaque: "sucesso",
      },
    ],
    [metricasPeriodo, periodoSelecionado, snapshotInicial]
  );

  const dataUltimaAtualizacao = useMemo(() => {
    const referenciaAtualizacao = Math.max(
      consultaMetricas.dataUpdatedAt || 0,
      consultaProjetos.dataUpdatedAt || 0,
      consultaTarefas.dataUpdatedAt || 0
    );

    if (!referenciaAtualizacao) {
      return "Aguardando primeira carga";
    }

    return new Date(referenciaAtualizacao).toLocaleString("pt-BR");
  }, [
    consultaMetricas.dataUpdatedAt,
    consultaProjetos.dataUpdatedAt,
    consultaTarefas.dataUpdatedAt,
  ]);

  const estaCarregando =
    consultaMetricas.isLoading ||
    consultaProjetos.isLoading ||
    consultaTarefas.isLoading;

  const houveErro =
    consultaMetricas.isError || consultaProjetos.isError || consultaTarefas.isError;

  const semDados = !estaCarregando && !houveErro && tarefasPeriodo.length === 0;

  function atualizarDashboard(): void {
    void Promise.all([
      consultaMetricas.refetch(),
      consultaProjetos.refetch(),
      consultaTarefas.refetch(),
    ]);
    mostrarInformacao("Atualizando dados do dashboard...");
  }

  function exportarCsv(): void {
    const conteudoCsv = gerarCsvDashboard(
      metricasPeriodo,
      totaisStatus,
      totaisPrioridade,
      tarefasAtencao
    );
    const blob = new Blob([conteudoCsv], { type: "text/csv;charset=utf-8;" });
    const url = window.URL.createObjectURL(blob);
    const ancora = document.createElement("a");
    ancora.href = url;
    ancora.download = `dashboard_${new Date().toISOString().slice(0, 10)}.csv`;
    ancora.click();
    window.URL.revokeObjectURL(url);
    mostrarSucesso("CSV do dashboard exportado.");
  }

  async function copiarResumo(): Promise<void> {
    const resumo = gerarResumoTexto(metricasPeriodo, periodoSelecionado);

    try {
      await navigator.clipboard.writeText(resumo);
      mostrarSucesso("Resumo do dashboard copiado.");
    } catch {
      mostrarErro("Nao foi possivel copiar o resumo automaticamente.");
    }
  }

  function irParaTarefas(): void {
    navegar("/tarefas");
  }

  return (
    <section className="pagina-conteudo">
      <header className="cabecalho-pagina cabecalho-dashboard">
        <div>
          <h1>Dashboard</h1>
          <p>Indicadores consolidados para acompanhamento da operacao.</p>
        </div>

        <div className="acoes-cabecalho-dashboard">
          <span className="rotulo-atualizacao-dashboard">
            Ultima atualizacao: {dataUltimaAtualizacao}
          </span>

          <button
            type="button"
            className="botao-secundario"
            onClick={atualizarDashboard}
            disabled={consultaMetricas.isFetching || consultaTarefas.isFetching}
          >
            Atualizar
          </button>
        </div>
      </header>

      <article className="cartao-filtros painel-filtros-dashboard">
        <h3>Filtros do painel</h3>

        <div className="grade-filtros">
          <label htmlFor="filtroPeriodoDashboard">
            Periodo
            <select
              id="filtroPeriodoDashboard"
              value={periodoSelecionado}
              onChange={(evento) => {
                setPeriodoSelecionado(Number(evento.target.value) as PeriodoDashboard);
              }}
            >
              <option value={7}>Ultimos 7 dias</option>
              <option value={15}>Ultimos 15 dias</option>
              <option value={30}>Ultimos 30 dias</option>
            </select>
          </label>

          <label htmlFor="filtroProjetoDashboard">
            Projeto
            <select
              id="filtroProjetoDashboard"
              value={projetoSelecionado}
              onChange={(evento) => {
                setProjetoSelecionado(evento.target.value);
              }}
            >
              <option value="">Todos os projetos</option>
              {(consultaProjetos.data ?? []).map((projeto) => (
                <option key={projeto.id} value={projeto.id}>
                  {projeto.nome}
                </option>
              ))}
            </select>
          </label>

          <label className="opcao-atualizacao-dashboard">
            Atualizacao automatica
            <div>
              <input
                type="checkbox"
                checked={atualizacaoAutomatica}
                onChange={(evento) => {
                  setAtualizacaoAutomatica(evento.target.checked);
                }}
              />
              <span>Recarregar a cada 30 segundos</span>
            </div>
          </label>
        </div>
      </article>

      {estaCarregando && <EsqueletoDashboard />}

      {houveErro && (
        <article className="cartao-listagem estado-erro-dashboard">
          <p className="mensagem-erro">
            Falha ao carregar dados do dashboard. Tente novamente.
          </p>
          <button type="button" className="botao-secundario" onClick={atualizarDashboard}>
            Tentar novamente
          </button>
        </article>
      )}

      {semDados && (
        <article className="cartao-listagem estado-vazio-dashboard">
          <h3>Sem dados para o filtro atual</h3>
          <p>
            Ajuste o periodo ou remova o filtro de projeto para visualizar metricas.
          </p>
        </article>
      )}

      {!estaCarregando && !houveErro && !semDados && (
        <PainelMetricasDashboard
          cartoes={cartoes}
          totaisStatus={totaisStatus}
          totaisPrioridade={totaisPrioridade}
          serieEvolucao={serieEvolucao}
          tarefasAtencao={tarefasAtencao}
          tarefasConcluidasRecentes={tarefasConcluidasRecentes}
          periodoDias={periodoSelecionado}
          aoExportarCsv={exportarCsv}
          aoCopiarResumo={() => {
            void copiarResumo();
          }}
          aoIrParaTarefas={irParaTarefas}
        />
      )}
    </section>
  );
}

function filtrarPorPeriodo(
  tarefas: TarefaResposta[],
  periodoDias: PeriodoDashboard
): TarefaResposta[] {
  const agora = new Date();
  const limite = new Date(agora);
  limite.setDate(limite.getDate() - periodoDias);

  return tarefas.filter((tarefa) => new Date(tarefa.dataCriacao) >= limite);
}

function calcularMetricasPorPeriodo(tarefas: TarefaResposta[]) {
  const totalTarefas = tarefas.length;
  const totalConcluidas = tarefas.filter(
    (tarefa) => tarefa.status === StatusTarefa.Concluida
  ).length;

  const tarefasAtrasadas = tarefas.filter((tarefa) => tarefa.estaAtrasada).length;

  const tarefasConcluidasNoPrazo = tarefas.filter((tarefa) => {
    if (tarefa.status !== StatusTarefa.Concluida || !tarefa.dataConclusao) {
      return false;
    }

    return (
      new Date(tarefa.dataConclusao).getTime() <=
      new Date(tarefa.dataPrazo).getTime()
    );
  }).length;

  const taxaConclusao =
    totalTarefas > 0
      ? Number(((totalConcluidas * 100) / totalTarefas).toFixed(2))
      : 0;

  const totalPorStatus = new Map<StatusTarefa, number>();
  Object.values(StatusTarefa)
    .filter((valor): valor is StatusTarefa => typeof valor === "number")
    .forEach((status) => {
      totalPorStatus.set(
        status,
        tarefas.filter((tarefa) => tarefa.status === status).length
      );
    });

  const totalPorPrioridade = new Map<PrioridadeTarefa, number>();
  Object.values(PrioridadeTarefa)
    .filter((valor): valor is PrioridadeTarefa => typeof valor === "number")
    .forEach((prioridade) => {
      totalPorPrioridade.set(
        prioridade,
        tarefas.filter((tarefa) => tarefa.prioridade === prioridade).length
      );
    });

  return {
    totalTarefas,
    totalConcluidas,
    tarefasAtrasadas,
    tarefasConcluidasNoPrazo,
    taxaConclusao,
    totalPorStatus,
    totalPorPrioridade,
  };
}

function gerarSerieEvolucaoDiaria(
  tarefas: TarefaResposta[],
  periodoDias: PeriodoDashboard
): PontoEvolucaoDashboard[] {
  const hoje = new Date();
  const serie: PontoEvolucaoDashboard[] = [];

  for (let indice = periodoDias - 1; indice >= 0; indice -= 1) {
    const dataPonto = new Date(hoje);
    dataPonto.setHours(0, 0, 0, 0);
    dataPonto.setDate(dataPonto.getDate() - indice);

    const diaSeguinte = new Date(dataPonto);
    diaSeguinte.setDate(diaSeguinte.getDate() + 1);

    const criadas = tarefas.filter((tarefa) => {
      const dataCriacao = new Date(tarefa.dataCriacao);
      return dataCriacao >= dataPonto && dataCriacao < diaSeguinte;
    }).length;

    const concluidas = tarefas.filter((tarefa) => {
      if (!tarefa.dataConclusao) {
        return false;
      }

      const dataConclusao = new Date(tarefa.dataConclusao);
      return dataConclusao >= dataPonto && dataConclusao < diaSeguinte;
    }).length;

    serie.push({
      periodo: dataPonto.toLocaleDateString("pt-BR", {
        day: "2-digit",
        month: "2-digit",
      }),
      criadas,
      concluidas,
    });
  }

  return serie;
}

function criarMapaProjetos(projetos: ProjetoResposta[]): Map<string, ProjetoResposta> {
  return new Map(projetos.map((projeto) => [projeto.id, projeto]));
}

function mapearTarefaResumo(
  tarefa: TarefaResposta,
  mapaProjetos: Map<string, ProjetoResposta>
): TarefaResumoDashboard {
  return {
    id: tarefa.id,
    titulo: tarefa.titulo,
    projetoNome: mapaProjetos.get(tarefa.projetoId)?.nome,
    dataPrazo: tarefa.dataPrazo,
    dataConclusao: tarefa.dataConclusao,
    status: tarefa.status,
    prioridade: tarefa.prioridade,
    estaAtrasada: tarefa.estaAtrasada,
  };
}

function calcularVariacao(
  valorAnterior: number | undefined,
  valorAtual: number
): number | undefined {
  if (valorAnterior === undefined) {
    return undefined;
  }

  return Number((valorAtual - valorAnterior).toFixed(2));
}

function lerSnapshotDashboard(): ResumoSnapshotDashboard | null {
  const valorArmazenado = window.localStorage.getItem(CHAVE_SNAPSHOT_DASHBOARD);
  if (!valorArmazenado) {
    return null;
  }

  try {
    return JSON.parse(valorArmazenado) as ResumoSnapshotDashboard;
  } catch {
    return null;
  }
}

function salvarSnapshotDashboard(snapshot: ResumoSnapshotDashboard): void {
  window.localStorage.setItem(
    CHAVE_SNAPSHOT_DASHBOARD,
    JSON.stringify(snapshot)
  );
}

function gerarCsvDashboard(
  metricas: {
    totalTarefas: number;
    taxaConclusao: number;
    tarefasAtrasadas: number;
    tarefasConcluidasNoPrazo: number;
  },
  totaisStatus: TotalStatusDashboard[],
  totaisPrioridade: TotalPrioridadeDashboard[],
  tarefasAtencao: TarefaResumoDashboard[]
): string {
  const linhas = [
    "secao,indicador,valor",
    `resumo,total_tarefas,${metricas.totalTarefas}`,
    `resumo,taxa_conclusao,${metricas.taxaConclusao}`,
    `resumo,tarefas_atrasadas,${metricas.tarefasAtrasadas}`,
    `resumo,concluidas_no_prazo,${metricas.tarefasConcluidasNoPrazo}`,
    "",
    "status,rotulo,total",
    ...totaisStatus.map((item) => `status,${item.rotulo},${item.total}`),
    "",
    "prioridade,rotulo,total",
    ...totaisPrioridade.map((item) => `prioridade,${item.rotulo},${item.total}`),
    "",
    "atencao,titulo,projeto,prazo",
    ...tarefasAtencao.map(
      (item) =>
        `atencao,${item.titulo},${item.projetoNome ?? "Nao informado"},${new Date(
          item.dataPrazo
        ).toLocaleDateString("pt-BR")}`
    ),
  ];

  return linhas.join("\n");
}

function gerarResumoTexto(
  metricas: {
    totalTarefas: number;
    taxaConclusao: number;
    tarefasAtrasadas: number;
    tarefasConcluidasNoPrazo: number;
  },
  periodoDias: number
): string {
  return [
    `Resumo do dashboard (${periodoDias} dias):`,
    `- Total de tarefas: ${metricas.totalTarefas}`,
    `- Taxa de conclusao: ${metricas.taxaConclusao.toFixed(2)}%`,
    `- Tarefas atrasadas: ${metricas.tarefasAtrasadas}`,
    `- Concluidas no prazo: ${metricas.tarefasConcluidasNoPrazo}`,
  ].join("\n");
}

function EsqueletoDashboard(): JSX.Element {
  return (
    <section className="esqueleto-dashboard" aria-hidden="true">
      <div className="grade-cartoes-metricas">
        <div className="bloco-esqueleto bloco-esqueleto-cartao" />
        <div className="bloco-esqueleto bloco-esqueleto-cartao" />
        <div className="bloco-esqueleto bloco-esqueleto-cartao" />
        <div className="bloco-esqueleto bloco-esqueleto-cartao" />
      </div>

      <div className="grade-graficos-dashboard">
        <div className="bloco-esqueleto bloco-esqueleto-grafico" />
        <div className="bloco-esqueleto bloco-esqueleto-grafico" />
        <div className="bloco-esqueleto bloco-esqueleto-grafico" />
      </div>
    </section>
  );
}
