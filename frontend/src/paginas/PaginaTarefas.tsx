import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { FormularioTarefa } from "../funcionalidades/tarefas/FormularioTarefa";
import { usarAutenticacao } from "../ganchos/usarAutenticacao";
import { listarProjetos } from "../servicos/servicoProjetos";
import { criarTarefa, listarTarefas } from "../servicos/servicoTarefas";
import { PrioridadeTarefa, StatusTarefa } from "../tipos/tarefas";

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

export function PaginaTarefas(): JSX.Element {
  const clienteConsulta = useQueryClient();
  const { sessao } = usarAutenticacao();

  const consultaProjetos = useQuery({
    queryKey: ["projetos"],
    queryFn: listarProjetos,
  });

  const consultaTarefas = useQuery({
    queryKey: ["tarefas"],
    queryFn: listarTarefas,
  });

  const mutacaoCriarTarefa = useMutation({
    mutationFn: criarTarefa,
    onSuccess: async () => {
      await Promise.all([
        clienteConsulta.invalidateQueries({ queryKey: ["tarefas"] }),
        clienteConsulta.invalidateQueries({ queryKey: ["dashboard"] }),
      ]);
    },
  });

  return (
    <section className="pagina-conteudo">
      <header className="cabecalho-pagina">
        <h1>Tarefas</h1>
        <p>Cadastro inicial e consulta das tarefas do sistema.</p>
      </header>

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
          {consultaTarefas.isError && (
            <p className="mensagem-erro">Falha ao carregar tarefas.</p>
          )}

          <ul>
            {consultaTarefas.data?.itens.map((tarefa) => (
              <li key={tarefa.id}>
                <strong>{tarefa.titulo}</strong>
                <span>
                  {nomesStatus[tarefa.status]} | {nomesPrioridade[tarefa.prioridade]}
                </span>
              </li>
            ))}
          </ul>
        </article>
      </div>
    </section>
  );
}
