import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { FormularioProjeto } from "../funcionalidades/projetos/FormularioProjeto";
import { criarProjeto, listarProjetos } from "../servicos/servicoProjetos";

export function PaginaProjetos(): JSX.Element {
  const clienteConsulta = useQueryClient();

  const consultaProjetos = useQuery({
    queryKey: ["projetos"],
    queryFn: listarProjetos,
  });

  const mutacaoCriarProjeto = useMutation({
    mutationFn: criarProjeto,
    onSuccess: async () => {
      await clienteConsulta.invalidateQueries({ queryKey: ["projetos"] });
    },
  });

  return (
    <section className="pagina-conteudo">
      <header className="cabecalho-pagina">
        <h1>Projetos</h1>
        <p>Cadastro e visualizacao de projetos da equipe.</p>
      </header>

      <div className="grade-duas-colunas">
        <FormularioProjeto
          emEnvio={mutacaoCriarProjeto.isPending}
          aoEnviar={mutacaoCriarProjeto.mutateAsync}
        />

        <article className="cartao-listagem">
          <h3>Projetos cadastrados</h3>

          {consultaProjetos.isLoading && <p>Carregando projetos...</p>}
          {consultaProjetos.isError && (
            <p className="mensagem-erro">Falha ao carregar projetos.</p>
          )}

          <ul>
            {consultaProjetos.data?.map((projeto) => (
              <li key={projeto.id}>
                <strong>{projeto.nome}</strong>
                <span>{projeto.descricao || "Sem descricao."}</span>
              </li>
            ))}
          </ul>
        </article>
      </div>
    </section>
  );
}
