import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { FormularioProjeto } from "../funcionalidades/projetos/FormularioProjeto";
import { usarNotificacao } from "../ganchos/usarNotificacao";
import {
  atualizarProjeto,
  criarProjeto,
  excluirProjeto,
  listarProjetos,
} from "../servicos/servicoProjetos";
import type { CriarProjetoRequisicao, ProjetoResposta } from "../tipos/projetos";

export function PaginaProjetos(): JSX.Element {
  const clienteConsulta = useQueryClient();
  const { mostrarErro, mostrarSucesso } = usarNotificacao();
  const [projetoEmEdicao, setProjetoEmEdicao] = useState<ProjetoResposta | null>(null);

  const consultaProjetos = useQuery({
    queryKey: ["projetos"],
    queryFn: listarProjetos,
  });

  const mutacaoCriarProjeto = useMutation({
    mutationFn: criarProjeto,
    onSuccess: async () => {
      await clienteConsulta.invalidateQueries({ queryKey: ["projetos"] });
      mostrarSucesso("Projeto criado com sucesso.");
    },
    onError: (excecao) => {
      mostrarErro(obterMensagemErro(excecao, "Falha ao criar projeto."));
    },
  });

  const mutacaoAtualizarProjeto = useMutation({
    mutationFn: ({ id, dados }: { id: string; dados: CriarProjetoRequisicao }) =>
      atualizarProjeto(id, dados),
    onSuccess: async () => {
      await clienteConsulta.invalidateQueries({ queryKey: ["projetos"] });
      mostrarSucesso("Projeto atualizado com sucesso.");
    },
    onError: (excecao) => {
      mostrarErro(obterMensagemErro(excecao, "Falha ao atualizar projeto."));
    },
  });

  const mutacaoExcluirProjeto = useMutation({
    mutationFn: excluirProjeto,
    onSuccess: async () => {
      await clienteConsulta.invalidateQueries({ queryKey: ["projetos"] });
      mostrarSucesso("Projeto excluido com sucesso.");
    },
    onError: (excecao) => {
      mostrarErro(obterMensagemErro(excecao, "Falha ao excluir projeto."));
    },
  });

  async function enviarProjeto(dados: CriarProjetoRequisicao): Promise<void> {
    if (projetoEmEdicao) {
      await mutacaoAtualizarProjeto.mutateAsync({ id: projetoEmEdicao.id, dados });
      setProjetoEmEdicao(null);
      return;
    }

    await mutacaoCriarProjeto.mutateAsync(dados);
  }

  async function excluirProjetoComConfirmacao(id: string, nome: string): Promise<void> {
    const confirmouExclusao = window.confirm(
      `Deseja realmente excluir o projeto "${nome}"?`
    );

    if (!confirmouExclusao) {
      return;
    }

    await mutacaoExcluirProjeto.mutateAsync(id);

    if (projetoEmEdicao?.id === id) {
      setProjetoEmEdicao(null);
    }
  }

  const existeProjetos = (consultaProjetos.data?.length ?? 0) > 0;

  return (
    <section className="pagina-conteudo">
      <header className="cabecalho-pagina">
        <h1>Projetos</h1>
        <p>Cadastro e visualizacao de projetos da equipe.</p>
      </header>

      <div className="grade-duas-colunas">
        <FormularioProjeto
          emEnvio={mutacaoCriarProjeto.isPending || mutacaoAtualizarProjeto.isPending}
          aoEnviar={enviarProjeto}
          valoresIniciais={
            projetoEmEdicao
              ? {
                  nome: projetoEmEdicao.nome,
                  descricao: projetoEmEdicao.descricao ?? "",
                }
              : undefined
          }
          titulo={projetoEmEdicao ? "Editar projeto" : "Novo projeto"}
          rotuloBotao={projetoEmEdicao ? "Atualizar projeto" : "Salvar projeto"}
          rotuloBotaoEmEnvio={
            projetoEmEdicao ? "Atualizando..." : "Salvando..."
          }
          aoCancelarEdicao={
            projetoEmEdicao ? () => setProjetoEmEdicao(null) : undefined
          }
        />

        <article className="cartao-listagem">
          <h3>Projetos cadastrados</h3>

          {consultaProjetos.isLoading && <p>Carregando projetos...</p>}
          {consultaProjetos.isFetching && !consultaProjetos.isLoading && (
            <p>Atualizando lista de projetos...</p>
          )}
          {consultaProjetos.isError && (
            <p className="mensagem-erro">
              {obterMensagemErro(consultaProjetos.error, "Falha ao carregar projetos.")}
            </p>
          )}

          {!consultaProjetos.isLoading && !consultaProjetos.isError && !existeProjetos && (
            <p>Nenhum projeto cadastrado ate o momento.</p>
          )}

          <ul className="lista-com-acoes">
            {consultaProjetos.data?.map((projeto) => (
              <li className="item-listagem" key={projeto.id}>
                <div className="conteudo-item-listagem">
                  <strong>{projeto.nome}</strong>
                  <span>{projeto.descricao || "Sem descricao."}</span>
                </div>

                <div className="acoes-item-listagem">
                  <button
                    type="button"
                    className="botao-secundario"
                    onClick={() => setProjetoEmEdicao(projeto)}
                    disabled={mutacaoExcluirProjeto.isPending}
                  >
                    Editar
                  </button>

                  <button
                    type="button"
                    className="botao-perigo"
                    onClick={() => excluirProjetoComConfirmacao(projeto.id, projeto.nome)}
                    disabled={mutacaoExcluirProjeto.isPending}
                  >
                    Excluir
                  </button>
                </div>
              </li>
            ))}
          </ul>
        </article>
      </div>
    </section>
  );
}

function obterMensagemErro(excecao: unknown, mensagemPadrao: string): string {
  if (excecao instanceof Error && excecao.message.trim().length > 0) {
    return excecao.message;
  }

  return mensagemPadrao;
}
