import {
  CampoOrdenacaoTarefa,
  DirecaoOrdenacaoTarefa,
  PrioridadeTarefa,
  StatusTarefa,
  type TarefaResposta,
} from "../../tipos/tarefas";

interface PropriedadesTabelaTarefasOperacional {
  tarefas: TarefaResposta[];
  idsSelecionados: string[];
  todasVisiveisSelecionadas: boolean;
  mapaProjetos: Map<string, string>;
  carregandoAtualizacaoStatus: boolean;
  carregandoEdicao: boolean;
  carregandoExclusao: boolean;
  campoOrdenacao: CampoOrdenacaoTarefa;
  direcaoOrdenacao: DirecaoOrdenacaoTarefa;
  obterStatusPermitidos: (statusAtual: StatusTarefa) => StatusTarefa[];
  aoAlternarSelecaoTodas: () => void;
  aoAlternarSelecao: (id: string) => void;
  aoAlterarStatus: (tarefa: TarefaResposta, novoStatus: StatusTarefa) => void;
  aoExcluir: (tarefa: TarefaResposta) => void;
  aoEditar: (tarefa: TarefaResposta) => void;
  aoOrdenar: (campo: CampoOrdenacaoTarefa) => void;
}

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

export function TabelaTarefasOperacional({
  tarefas,
  idsSelecionados,
  todasVisiveisSelecionadas,
  mapaProjetos,
  carregandoAtualizacaoStatus,
  carregandoEdicao,
  carregandoExclusao,
  campoOrdenacao,
  direcaoOrdenacao,
  obterStatusPermitidos,
  aoAlternarSelecaoTodas,
  aoAlternarSelecao,
  aoAlterarStatus,
  aoExcluir,
  aoEditar,
  aoOrdenar,
}: PropriedadesTabelaTarefasOperacional): JSX.Element {
  return (
    <div className="container-tabela-tarefas">
      <table className="tabela-tarefas">
        <thead>
          <tr>
            <th>
              <input
                type="checkbox"
                checked={todasVisiveisSelecionadas}
                onChange={aoAlternarSelecaoTodas}
                aria-label="Selecionar todas as tarefas visiveis"
              />
            </th>
            <th>
              <button
                type="button"
                className="botao-ordenacao-coluna"
                onClick={() => aoOrdenar(CampoOrdenacaoTarefa.Titulo)}
              >
                Titulo {indicadorOrdenacao(campoOrdenacao, direcaoOrdenacao, CampoOrdenacaoTarefa.Titulo)}
              </button>
            </th>
            <th>Projeto</th>
            <th>
              <button
                type="button"
                className="botao-ordenacao-coluna"
                onClick={() => aoOrdenar(CampoOrdenacaoTarefa.Prioridade)}
              >
                Prioridade{" "}
                {indicadorOrdenacao(campoOrdenacao, direcaoOrdenacao, CampoOrdenacaoTarefa.Prioridade)}
              </button>
            </th>
            <th>
              <button
                type="button"
                className="botao-ordenacao-coluna"
                onClick={() => aoOrdenar(CampoOrdenacaoTarefa.DataPrazo)}
              >
                Prazo {indicadorOrdenacao(campoOrdenacao, direcaoOrdenacao, CampoOrdenacaoTarefa.DataPrazo)}
              </button>
            </th>
            <th>
              <button
                type="button"
                className="botao-ordenacao-coluna"
                onClick={() => aoOrdenar(CampoOrdenacaoTarefa.Status)}
              >
                Status {indicadorOrdenacao(campoOrdenacao, direcaoOrdenacao, CampoOrdenacaoTarefa.Status)}
              </button>
            </th>
            <th>Acoes</th>
          </tr>
        </thead>
        <tbody>
          {tarefas.map((tarefa) => {
            const statusPermitidos = obterStatusPermitidos(tarefa.status);
            const indicadorPrazo = obterIndicadorPrazo(tarefa);

            return (
              <tr
                key={tarefa.id}
                className={`linha-tarefa${tarefa.estaAtrasada ? " linha-tarefa-atrasada" : ""}`}
              >
                <td>
                  <input
                    type="checkbox"
                    checked={idsSelecionados.includes(tarefa.id)}
                    onChange={() => aoAlternarSelecao(tarefa.id)}
                    aria-label={`Selecionar tarefa ${tarefa.titulo}`}
                  />
                </td>
                <td>
                  <div className="coluna-titulo-tarefa">
                    <strong>{tarefa.titulo}</strong>
                    <span>{tarefa.descricao || "Sem descricao."}</span>
                  </div>
                </td>
                <td>{mapaProjetos.get(tarefa.projetoId) ?? "Projeto nao encontrado"}</td>
                <td>
                  <span className={`selo-prioridade selo-prioridade-${tarefa.prioridade}`}>
                    {nomesPrioridade[tarefa.prioridade]}
                  </span>
                </td>
                <td>
                  <div className="coluna-prazo-tarefa">
                    <span>{formatarData(tarefa.dataPrazo)}</span>
                    {indicadorPrazo && <small className={indicadorPrazo.classe}>{indicadorPrazo.texto}</small>}
                  </div>
                </td>
                <td>
                  <div className="coluna-status-tarefa">
                    <span className={`selo-status selo-status-${tarefa.status}`}>
                      {nomesStatus[tarefa.status]}
                    </span>
                    <select
                      value={tarefa.status}
                      onChange={(evento) =>
                        aoAlterarStatus(tarefa, Number(evento.target.value) as StatusTarefa)
                      }
                      disabled={carregandoAtualizacaoStatus}
                    >
                      {Object.values(StatusTarefa)
                        .filter((valor) => typeof valor === "number")
                        .map((status) => (
                          <option
                            key={status}
                            value={status}
                            disabled={!statusPermitidos.includes(status as StatusTarefa)}
                          >
                            {nomesStatus[status as StatusTarefa]}
                          </option>
                        ))}
                    </select>
                  </div>
                </td>
                <td>
                  <div className="acoes-item-listagem">
                    <button
                      type="button"
                      className="botao-secundario"
                      onClick={() => aoEditar(tarefa)}
                      disabled={carregandoEdicao || carregandoExclusao}
                    >
                      Editar
                    </button>

                    <button
                      type="button"
                      className="botao-perigo"
                      onClick={() => aoExcluir(tarefa)}
                      disabled={carregandoExclusao || carregandoEdicao}
                    >
                      Excluir
                    </button>
                  </div>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}

function obterIndicadorPrazo(
  tarefa: TarefaResposta
): { texto: string; classe: string } | null {
  if (tarefa.estaAtrasada) {
    return { texto: "Atrasada", classe: "indicador-prazo-atrasado" };
  }

  if (tarefa.status === StatusTarefa.Concluida || tarefa.status === StatusTarefa.Cancelada) {
    return null;
  }

  const agora = new Date().getTime();
  const prazo = new Date(tarefa.dataPrazo).getTime();
  const diferenca = prazo - agora;

  if (diferenca <= 86400000 && diferenca > 0) {
    return { texto: "Vence em ate 24h", classe: "indicador-prazo-proximo" };
  }

  if (diferenca <= 172800000 && diferenca > 0) {
    return { texto: "Vence em ate 48h", classe: "indicador-prazo-alerta" };
  }

  return null;
}

function indicadorOrdenacao(
  campoAtual: CampoOrdenacaoTarefa,
  direcaoAtual: DirecaoOrdenacaoTarefa,
  campoReferencia: CampoOrdenacaoTarefa
): string {
  if (campoAtual !== campoReferencia) {
    return "";
  }

  return direcaoAtual === DirecaoOrdenacaoTarefa.Ascendente ? "ASC" : "DESC";
}

function formatarData(data: string): string {
  return new Date(data).toLocaleDateString("pt-BR");
}
