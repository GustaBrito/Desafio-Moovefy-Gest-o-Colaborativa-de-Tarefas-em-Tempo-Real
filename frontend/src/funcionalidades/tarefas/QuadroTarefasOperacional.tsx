import { PrioridadeTarefa, StatusTarefa, type TarefaResposta } from "../../tipos/tarefas";

interface PropriedadesQuadroTarefasOperacional {
  tarefasPorStatus: Map<StatusTarefa, TarefaResposta[]>;
  idsSelecionados: string[];
  mapaProjetos: Map<string, string>;
  aoAlternarSelecao: (id: string) => void;
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

export function QuadroTarefasOperacional({
  tarefasPorStatus,
  idsSelecionados,
  mapaProjetos,
  aoAlternarSelecao,
}: PropriedadesQuadroTarefasOperacional): JSX.Element {
  return (
    <div className="quadro-kanban-tarefas">
      {Object.values(StatusTarefa)
        .filter((valor): valor is StatusTarefa => typeof valor === "number")
        .map((status) => (
          <section className="coluna-kanban-tarefas" key={status}>
            <header>
              <h4>{nomesStatus[status]}</h4>
              <span>{tarefasPorStatus.get(status)?.length ?? 0}</span>
            </header>

            <ul>
              {(tarefasPorStatus.get(status) ?? []).map((tarefa) => {
                const indicadorPrazo = obterIndicadorPrazo(tarefa);

                return (
                  <li key={tarefa.id} className={tarefa.estaAtrasada ? "cartao-tarefa-atrasada" : ""}>
                    <div className="topo-cartao-kanban">
                      <strong>{tarefa.titulo}</strong>
                      <input
                        type="checkbox"
                        checked={idsSelecionados.includes(tarefa.id)}
                        onChange={() => aoAlternarSelecao(tarefa.id)}
                        aria-label={`Selecionar tarefa ${tarefa.titulo}`}
                      />
                    </div>
                    <span>{mapaProjetos.get(tarefa.projetoId) ?? "Projeto nao encontrado"}</span>
                    <small>{tarefa.areaNome ?? "Area nao informada"}</small>
                    <small>{tarefa.responsavelNome ?? tarefa.responsavelUsuarioId}</small>
                    <small>{tarefa.descricao || "Sem descricao."}</small>

                    <div className="rodape-cartao-kanban">
                      <span className={`selo-prioridade selo-prioridade-${tarefa.prioridade}`}>
                        {nomesPrioridade[tarefa.prioridade]}
                      </span>
                      <span>{formatarData(tarefa.dataPrazo)}</span>
                    </div>

                    {indicadorPrazo && <p className={indicadorPrazo.classe}>{indicadorPrazo.texto}</p>}
                  </li>
                );
              })}
            </ul>
          </section>
        ))}
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

function formatarData(data: string): string {
  return new Date(data).toLocaleDateString("pt-BR");
}
