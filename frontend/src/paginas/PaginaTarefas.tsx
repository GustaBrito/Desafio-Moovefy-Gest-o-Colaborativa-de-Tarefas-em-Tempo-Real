import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { FormularioTarefa } from "../funcionalidades/tarefas/FormularioTarefa";
import { usarAutenticacao } from "../ganchos/usarAutenticacao";
import { usarNotificacao } from "../ganchos/usarNotificacao";
import { listarProjetos } from "../servicos/servicoProjetos";
import {
  atualizarStatusTarefa,
  criarTarefa,
  excluirTarefa,
  listarTarefas,
} from "../servicos/servicoTarefas";
import {
  CampoOrdenacaoTarefa,
  DirecaoOrdenacaoTarefa,
  PrioridadeTarefa,
  StatusTarefa,
  type FiltroConsultaTarefas,
  type TarefaResposta,
} from "../tipos/tarefas";

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

const nomesCamposOrdenacao: Record<CampoOrdenacaoTarefa, string> = {
  [CampoOrdenacaoTarefa.DataCriacao]: "Data de criacao",
  [CampoOrdenacaoTarefa.DataPrazo]: "Data de prazo",
  [CampoOrdenacaoTarefa.Prioridade]: "Prioridade",
  [CampoOrdenacaoTarefa.Status]: "Status",
  [CampoOrdenacaoTarefa.Titulo]: "Titulo",
};

const nomesDirecaoOrdenacao: Record<DirecaoOrdenacaoTarefa, string> = {
  [DirecaoOrdenacaoTarefa.Ascendente]: "Ascendente",
  [DirecaoOrdenacaoTarefa.Descendente]: "Descendente",
};

export function PaginaTarefas(): JSX.Element {
  const clienteConsulta = useQueryClient();
  const { sessao } = usarAutenticacao();
  const { mostrarErro, mostrarSucesso } = usarNotificacao();

  const [projetoIdFiltro, setProjetoIdFiltro] = useState("");
  const [statusFiltro, setStatusFiltro] = useState("");
  const [responsavelIdFiltro, setResponsavelIdFiltro] = useState("");
  const [dataPrazoInicialFiltro, setDataPrazoInicialFiltro] = useState("");
  const [dataPrazoFinalFiltro, setDataPrazoFinalFiltro] = useState("");
  const [campoOrdenacao, setCampoOrdenacao] = useState<CampoOrdenacaoTarefa>(
    CampoOrdenacaoTarefa.DataPrazo
  );
  const [direcaoOrdenacao, setDirecaoOrdenacao] =
    useState<DirecaoOrdenacaoTarefa>(DirecaoOrdenacaoTarefa.Ascendente);
  const [numeroPagina, setNumeroPagina] = useState(1);
  const [tamanhoPagina, setTamanhoPagina] = useState(10);

  const filtroConsulta = useMemo<FiltroConsultaTarefas>(
    () => ({
      projetoId: projetoIdFiltro || undefined,
      status: statusFiltro ? (Number(statusFiltro) as StatusTarefa) : undefined,
      responsavelId: responsavelIdFiltro || undefined,
      dataPrazoInicial: dataPrazoInicialFiltro || undefined,
      dataPrazoFinal: dataPrazoFinalFiltro || undefined,
      campoOrdenacao,
      direcaoOrdenacao,
      numeroPagina,
      tamanhoPagina,
    }),
    [
      projetoIdFiltro,
      statusFiltro,
      responsavelIdFiltro,
      dataPrazoInicialFiltro,
      dataPrazoFinalFiltro,
      campoOrdenacao,
      direcaoOrdenacao,
      numeroPagina,
      tamanhoPagina,
    ]
  );

  const consultaProjetos = useQuery({
    queryKey: ["projetos"],
    queryFn: listarProjetos,
  });

  const consultaTarefas = useQuery({
    queryKey: ["tarefas", filtroConsulta],
    queryFn: () => listarTarefas(filtroConsulta),
    placeholderData: (dadosAnteriores) => dadosAnteriores,
  });

  const mutacaoCriarTarefa = useMutation({
    mutationFn: criarTarefa,
    onSuccess: async () => {
      await Promise.all([
        clienteConsulta.invalidateQueries({ queryKey: ["tarefas"] }),
        clienteConsulta.invalidateQueries({ queryKey: ["dashboard"] }),
      ]);
      mostrarSucesso("Tarefa criada com sucesso.");
    },
    onError: (excecao) => {
      mostrarErro(obterMensagemErro(excecao, "Falha ao criar tarefa."));
    },
  });

  const mutacaoAtualizarStatus = useMutation({
    mutationFn: ({ id, status }: { id: string; status: StatusTarefa }) =>
      atualizarStatusTarefa(id, { status }),
    onSuccess: async () => {
      await Promise.all([
        clienteConsulta.invalidateQueries({ queryKey: ["tarefas"] }),
        clienteConsulta.invalidateQueries({ queryKey: ["dashboard"] }),
      ]);
      mostrarSucesso("Status da tarefa atualizado.");
    },
    onError: (excecao) => {
      mostrarErro(
        obterMensagemErro(excecao, "Falha ao atualizar status da tarefa.")
      );
    },
  });

  const mutacaoExcluirTarefa = useMutation({
    mutationFn: excluirTarefa,
    onSuccess: async () => {
      await Promise.all([
        clienteConsulta.invalidateQueries({ queryKey: ["tarefas"] }),
        clienteConsulta.invalidateQueries({ queryKey: ["dashboard"] }),
      ]);
      mostrarSucesso("Tarefa excluida com sucesso.");
    },
    onError: (excecao) => {
      mostrarErro(obterMensagemErro(excecao, "Falha ao excluir tarefa."));
    },
  });

  async function alterarStatusDaTarefa(
    tarefa: TarefaResposta,
    novoStatus: StatusTarefa
  ): Promise<void> {
    if (novoStatus === tarefa.status) {
      return;
    }

    await mutacaoAtualizarStatus.mutateAsync({ id: tarefa.id, status: novoStatus });
  }

  async function excluirTarefaComConfirmacao(tarefa: TarefaResposta): Promise<void> {
    const confirmouExclusao = window.confirm(
      `Deseja realmente excluir a tarefa "${tarefa.titulo}"?`
    );

    if (!confirmouExclusao) {
      return;
    }

    await mutacaoExcluirTarefa.mutateAsync(tarefa.id);
  }

  function limparFiltros(): void {
    setProjetoIdFiltro("");
    setStatusFiltro("");
    setResponsavelIdFiltro("");
    setDataPrazoInicialFiltro("");
    setDataPrazoFinalFiltro("");
    setCampoOrdenacao(CampoOrdenacaoTarefa.DataPrazo);
    setDirecaoOrdenacao(DirecaoOrdenacaoTarefa.Ascendente);
    setNumeroPagina(1);
    setTamanhoPagina(10);
  }

  const tarefas = consultaTarefas.data?.itens ?? [];
  const totalPaginas = consultaTarefas.data?.totalPaginas ?? 1;
  const totalRegistros = consultaTarefas.data?.totalRegistros ?? 0;
  const semResultados =
    !consultaTarefas.isLoading && !consultaTarefas.isError && tarefas.length === 0;

  return (
    <section className="pagina-conteudo">
      <header className="cabecalho-pagina">
        <h1>Tarefas</h1>
        <p>Cadastro inicial e consulta das tarefas do sistema.</p>
      </header>

      <article className="cartao-filtros">
        <h3>Filtros e ordenacao</h3>

        <div className="grade-filtros">
          <label htmlFor="filtroProjetoId">
            Projeto
            <select
              id="filtroProjetoId"
              value={projetoIdFiltro}
              onChange={(evento) => {
                setProjetoIdFiltro(evento.target.value);
                setNumeroPagina(1);
              }}
            >
              <option value="">Todos</option>
              {consultaProjetos.data?.map((projeto) => (
                <option key={projeto.id} value={projeto.id}>
                  {projeto.nome}
                </option>
              ))}
            </select>
          </label>

          <label htmlFor="filtroStatus">
            Status
            <select
              id="filtroStatus"
              value={statusFiltro}
              onChange={(evento) => {
                setStatusFiltro(evento.target.value);
                setNumeroPagina(1);
              }}
            >
              <option value="">Todos</option>
              {Object.values(StatusTarefa)
                .filter((valor) => typeof valor === "number")
                .map((status) => (
                  <option key={status} value={status}>
                    {nomesStatus[status as StatusTarefa]}
                  </option>
                ))}
            </select>
          </label>

          <label htmlFor="filtroResponsavelId">
            Responsavel
            <input
              id="filtroResponsavelId"
              type="text"
              placeholder="Id do responsavel"
              value={responsavelIdFiltro}
              onChange={(evento) => {
                setResponsavelIdFiltro(evento.target.value);
                setNumeroPagina(1);
              }}
            />
          </label>

          <label htmlFor="filtroDataPrazoInicial">
            Prazo inicial
            <input
              id="filtroDataPrazoInicial"
              type="date"
              value={dataPrazoInicialFiltro}
              onChange={(evento) => {
                setDataPrazoInicialFiltro(evento.target.value);
                setNumeroPagina(1);
              }}
            />
          </label>

          <label htmlFor="filtroDataPrazoFinal">
            Prazo final
            <input
              id="filtroDataPrazoFinal"
              type="date"
              value={dataPrazoFinalFiltro}
              onChange={(evento) => {
                setDataPrazoFinalFiltro(evento.target.value);
                setNumeroPagina(1);
              }}
            />
          </label>

          <label htmlFor="filtroCampoOrdenacao">
            Ordenar por
            <select
              id="filtroCampoOrdenacao"
              value={campoOrdenacao}
              onChange={(evento) => {
                setCampoOrdenacao(Number(evento.target.value) as CampoOrdenacaoTarefa);
                setNumeroPagina(1);
              }}
            >
              {Object.values(CampoOrdenacaoTarefa)
                .filter((valor) => typeof valor === "number")
                .map((campo) => (
                  <option key={campo} value={campo}>
                    {nomesCamposOrdenacao[campo as CampoOrdenacaoTarefa]}
                  </option>
                ))}
            </select>
          </label>

          <label htmlFor="filtroDirecaoOrdenacao">
            Direcao
            <select
              id="filtroDirecaoOrdenacao"
              value={direcaoOrdenacao}
              onChange={(evento) => {
                setDirecaoOrdenacao(
                  Number(evento.target.value) as DirecaoOrdenacaoTarefa
                );
                setNumeroPagina(1);
              }}
            >
              {Object.values(DirecaoOrdenacaoTarefa)
                .filter((valor) => typeof valor === "number")
                .map((direcao) => (
                  <option key={direcao} value={direcao}>
                    {nomesDirecaoOrdenacao[direcao as DirecaoOrdenacaoTarefa]}
                  </option>
                ))}
            </select>
          </label>

          <label htmlFor="filtroTamanhoPagina">
            Itens por pagina
            <select
              id="filtroTamanhoPagina"
              value={tamanhoPagina}
              onChange={(evento) => {
                setTamanhoPagina(Number(evento.target.value));
                setNumeroPagina(1);
              }}
            >
              <option value={5}>5</option>
              <option value={10}>10</option>
              <option value={20}>20</option>
            </select>
          </label>
        </div>

        <button type="button" className="botao-secundario" onClick={limparFiltros}>
          Limpar filtros
        </button>
      </article>

      <div className="grade-duas-colunas">
        <FormularioTarefa
          projetos={consultaProjetos.data ?? []}
          responsavelIdPadrao={sessao?.usuarioId ?? ""}
          emEnvio={mutacaoCriarTarefa.isPending}
          aoEnviar={async (dados) => {
            await mutacaoCriarTarefa.mutateAsync({
              titulo: dados.titulo,
              descricao: dados.descricao || null,
              prioridade: dados.prioridade,
              projetoId: dados.projetoId,
              responsavelId: dados.responsavelId,
              dataPrazo: new Date(`${dados.dataPrazo}T23:59:59Z`).toISOString(),
            });
          }}
        />

        <article className="cartao-listagem">
          <h3>Tarefas cadastradas</h3>

          {consultaTarefas.isLoading && <p>Carregando tarefas...</p>}
          {consultaTarefas.isFetching && !consultaTarefas.isLoading && (
            <p>Atualizando tarefas...</p>
          )}
          {consultaTarefas.isError && (
            <p className="mensagem-erro">
              {obterMensagemErro(consultaTarefas.error, "Falha ao carregar tarefas.")}
            </p>
          )}

          {semResultados && <p>Nenhuma tarefa encontrada com os filtros atuais.</p>}

          <ul className="lista-tarefas">
            {tarefas.map((tarefa) => (
              <li
                className={`item-tarefa${tarefa.estaAtrasada ? " item-tarefa-atrasada" : ""}`}
                key={tarefa.id}
              >
                <div className="cabecalho-item-tarefa">
                  <strong>{tarefa.titulo}</strong>
                  {tarefa.estaAtrasada && <span className="selo-atrasada">Atrasada</span>}
                </div>

                <span>{tarefa.descricao || "Sem descricao."}</span>

                <div className="metadados-item-tarefa">
                  <span>Status: {nomesStatus[tarefa.status]}</span>
                  <span>Prioridade: {nomesPrioridade[tarefa.prioridade]}</span>
                  <span>Prazo: {formatarData(tarefa.dataPrazo)}</span>
                </div>

                <div className="acoes-item-tarefa">
                  <label htmlFor={`status-${tarefa.id}`}>
                    Status
                    <select
                      id={`status-${tarefa.id}`}
                      value={tarefa.status}
                      onChange={(evento) => {
                        void alterarStatusDaTarefa(
                          tarefa,
                          Number(evento.target.value) as StatusTarefa
                        );
                      }}
                      disabled={mutacaoAtualizarStatus.isPending}
                    >
                      {Object.values(StatusTarefa)
                        .filter((valor) => typeof valor === "number")
                        .map((status) => (
                          <option key={status} value={status}>
                            {nomesStatus[status as StatusTarefa]}
                          </option>
                        ))}
                    </select>
                  </label>

                  <button
                    type="button"
                    className="botao-perigo"
                    onClick={() => {
                      void excluirTarefaComConfirmacao(tarefa);
                    }}
                    disabled={mutacaoExcluirTarefa.isPending}
                  >
                    Excluir
                  </button>
                </div>
              </li>
            ))}
          </ul>

          {!consultaTarefas.isError && totalRegistros > 0 && (
            <footer className="rodape-listagem">
              <span>
                Pagina {numeroPagina} de {totalPaginas} | {totalRegistros} tarefas
              </span>

              <div className="acoes-paginacao">
                <button
                  type="button"
                  className="botao-secundario"
                  onClick={() => setNumeroPagina((paginaAtual) => paginaAtual - 1)}
                  disabled={numeroPagina <= 1}
                >
                  Anterior
                </button>

                <button
                  type="button"
                  className="botao-secundario"
                  onClick={() => setNumeroPagina((paginaAtual) => paginaAtual + 1)}
                  disabled={numeroPagina >= totalPaginas}
                >
                  Proxima
                </button>
              </div>
            </footer>
          )}
        </article>
      </div>
    </section>
  );
}

function formatarData(data: string): string {
  const dataConvertida = new Date(data);

  return dataConvertida.toLocaleDateString("pt-BR");
}

function obterMensagemErro(excecao: unknown, mensagemPadrao: string): string {
  if (excecao instanceof Error && excecao.message.trim().length > 0) {
    return excecao.message;
  }

  return mensagemPadrao;
}
