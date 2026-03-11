import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { FormularioProjeto } from "../funcionalidades/projetos/FormularioProjeto";
import { usarAutenticacao } from "../ganchos/usarAutenticacao";
import { usarNotificacao } from "../ganchos/usarNotificacao";
import { listarAreas } from "../servicos/servicoAreas";
import {
  atualizarProjeto,
  criarProjeto,
  excluirProjeto,
  listarProjetos,
} from "../servicos/servicoProjetos";
import { listarUsuarios } from "../servicos/servicoUsuarios";
import type { AtualizarProjetoRequisicao, CriarProjetoRequisicao, ProjetoResposta } from "../tipos/projetos";

type CriterioOrdenacaoProjeto =
  | "nome_ascendente"
  | "nome_descendente"
  | "data_mais_recente"
  | "data_mais_antiga";

export function PaginaProjetos(): JSX.Element {
  const clienteConsulta = useQueryClient();
  const { ehColaborador } = usarAutenticacao();
  const { mostrarErro, mostrarSucesso } = usarNotificacao();
  const [projetoEmEdicao, setProjetoEmEdicao] = useState<ProjetoResposta | null>(null);
  const [textoBusca, setTextoBusca] = useState("");
  const [criterioOrdenacao, setCriterioOrdenacao] =
    useState<CriterioOrdenacaoProjeto>("data_mais_recente");
  const [somenteComDescricao, setSomenteComDescricao] = useState(false);

  const consultaProjetos = useQuery({
    queryKey: ["projetos"],
    queryFn: listarProjetos,
  });

  const consultaAreas = useQuery({
    queryKey: ["areas", "ativas"],
    queryFn: () => listarAreas(true),
    staleTime: 120000,
  });

  const consultaUsuarios = useQuery({
    queryKey: ["usuarios", "ativos"],
    queryFn: () => listarUsuarios(true),
    enabled: !ehColaborador,
    staleTime: 120000,
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
    mutationFn: ({ id, dados }: { id: string; dados: AtualizarProjetoRequisicao }) =>
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
    if (ehColaborador) {
      mostrarErro("Colaborador nao pode criar ou editar projetos.");
      return;
    }

    if (projetoEmEdicao) {
      await mutacaoAtualizarProjeto.mutateAsync({ id: projetoEmEdicao.id, dados });
      setProjetoEmEdicao(null);
      return;
    }

    await mutacaoCriarProjeto.mutateAsync(dados);
  }

  async function excluirProjetoComConfirmacao(id: string, nome: string): Promise<void> {
    if (ehColaborador) {
      mostrarErro("Colaborador nao pode excluir projetos.");
      return;
    }

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

  function limparFiltros(): void {
    setTextoBusca("");
    setSomenteComDescricao(false);
    setCriterioOrdenacao("data_mais_recente");
  }

  const projetosFiltrados = useMemo(() => {
    const textoBuscaNormalizado = normalizarTexto(textoBusca);
    const listaProjetos = consultaProjetos.data ?? [];

    const projetosComFiltros = listaProjetos.filter((projeto) => {
      if (
        somenteComDescricao &&
        (!projeto.descricao || projeto.descricao.trim().length === 0)
      ) {
        return false;
      }

      if (!textoBuscaNormalizado) {
        return true;
      }

      const nomeNormalizado = normalizarTexto(projeto.nome);
      const descricaoNormalizada = normalizarTexto(projeto.descricao ?? "");

      return (
        nomeNormalizado.includes(textoBuscaNormalizado) ||
        descricaoNormalizada.includes(textoBuscaNormalizado)
      );
    });

    return projetosComFiltros.sort((projetoAtual, proximoProjeto) => {
      if (criterioOrdenacao === "nome_ascendente") {
        return projetoAtual.nome.localeCompare(proximoProjeto.nome, "pt-BR");
      }

      if (criterioOrdenacao === "nome_descendente") {
        return proximoProjeto.nome.localeCompare(projetoAtual.nome, "pt-BR");
      }

      if (criterioOrdenacao === "data_mais_antiga") {
        return (
          new Date(projetoAtual.dataCriacao).getTime() -
          new Date(proximoProjeto.dataCriacao).getTime()
        );
      }

      return (
        new Date(proximoProjeto.dataCriacao).getTime() -
        new Date(projetoAtual.dataCriacao).getTime()
      );
    });
  }, [consultaProjetos.data, criterioOrdenacao, somenteComDescricao, textoBusca]);

  const totalProjetos = consultaProjetos.data?.length ?? 0;
  const totalProjetosComDescricao =
    consultaProjetos.data?.filter(
      (projeto) => Boolean(projeto.descricao && projeto.descricao.trim().length > 0)
    ).length ?? 0;
  const totalProjetosSemDescricao = totalProjetos - totalProjetosComDescricao;

  const projetoMaisRecente = useMemo(() => {
    const listaProjetos = consultaProjetos.data ?? [];

    if (listaProjetos.length === 0) {
      return null;
    }

    return [...listaProjetos].sort(
      (projetoAtual, proximoProjeto) =>
        new Date(proximoProjeto.dataCriacao).getTime() -
        new Date(projetoAtual.dataCriacao).getTime()
    )[0];
  }, [consultaProjetos.data]);

  const existemProjetosNoSistema = totalProjetos > 0;
  const existemProjetosFiltrados = projetosFiltrados.length > 0;
  const estaAtualizandoLista = consultaProjetos.isFetching && !consultaProjetos.isLoading;

  function exportarProjetosCsv(): void {
    if (!consultaProjetos.data || consultaProjetos.data.length === 0) {
      mostrarErro("Nao ha projetos para exportar.");
      return;
    }

    const cabecalho = ["id", "nome", "descricao", "data_criacao"];
    const linhas = consultaProjetos.data.map((projeto) => [
      projeto.id,
      projeto.nome,
      projeto.descricao ?? "",
      projeto.dataCriacao,
    ]);
    const conteudoCsv = [cabecalho, ...linhas]
      .map((linha) => linha.map((valor) => escaparValorCsv(valor)).join(";"))
      .join("\n");

    const arquivo = new Blob([`\uFEFF${conteudoCsv}`], {
      type: "text/csv;charset=utf-8;",
    });
    const urlArquivo = URL.createObjectURL(arquivo);
    const linkDownload = document.createElement("a");
    linkDownload.href = urlArquivo;
    linkDownload.download = `projetos-${new Date().toISOString().slice(0, 10)}.csv`;
    document.body.appendChild(linkDownload);
    linkDownload.click();
    document.body.removeChild(linkDownload);
    URL.revokeObjectURL(urlArquivo);
    mostrarSucesso("Exportacao concluida.");
  }

  return (
    <section className="pagina-conteudo pagina-projetos">
      <header className="cabecalho-pagina cabecalho-projetos">
        <div>
          <h1>Projetos</h1>
          <p>Gerencie o portfolio da equipe com mais controle e visibilidade.</p>
        </div>

        <div className="acoes-cabecalho-projetos">
          <button
            type="button"
            className="botao-secundario"
            onClick={() => consultaProjetos.refetch()}
            disabled={consultaProjetos.isFetching}
          >
            Atualizar lista
          </button>

          <button
            type="button"
            className="botao-secundario"
            onClick={exportarProjetosCsv}
            disabled={!existemProjetosNoSistema}
          >
            Exportar CSV
          </button>
        </div>
      </header>

      {estaAtualizandoLista && (
        <p className="rotulo-atualizacao-projetos">Atualizando dados de projetos...</p>
      )}

      <section className="grade-resumo-projetos">
        <article className="cartao-resumo-projeto">
          <h3>Total de projetos</h3>
          <strong>{totalProjetos}</strong>
          <span>Base ativa no sistema</span>
        </article>

        <article className="cartao-resumo-projeto">
          <h3>Com descricao</h3>
          <strong>{totalProjetosComDescricao}</strong>
          <span>Documentacao registrada</span>
        </article>

        <article className="cartao-resumo-projeto">
          <h3>Sem descricao</h3>
          <strong>{totalProjetosSemDescricao}</strong>
          <span>Itens para complementar</span>
        </article>

        <article className="cartao-resumo-projeto">
          <h3>Ultimo cadastro</h3>
          <strong>{projetoMaisRecente?.nome ?? "Sem dados"}</strong>
          <span>
            {projetoMaisRecente
              ? formatarDataHora(projetoMaisRecente.dataCriacao)
              : "Aguardando primeiro projeto"}
          </span>
        </article>
      </section>

      <article className="cartao-filtros painel-filtros-projetos">
        <h3>Busca e organizacao</h3>

        <div className="grade-filtros grade-filtros-projetos">
          <label htmlFor="buscaProjeto">
            Buscar por nome ou descricao
            <input
              id="buscaProjeto"
              type="text"
              placeholder="Ex.: Integracao ERP"
              value={textoBusca}
              onChange={(evento) => setTextoBusca(evento.target.value)}
            />
          </label>

          <label htmlFor="ordenacaoProjeto">
            Ordenar por
            <select
              id="ordenacaoProjeto"
              value={criterioOrdenacao}
              onChange={(evento) =>
                setCriterioOrdenacao(evento.target.value as CriterioOrdenacaoProjeto)
              }
            >
              <option value="data_mais_recente">Data mais recente</option>
              <option value="data_mais_antiga">Data mais antiga</option>
              <option value="nome_ascendente">Nome (A-Z)</option>
              <option value="nome_descendente">Nome (Z-A)</option>
            </select>
          </label>
        </div>

        <label className="opcao-filtro-projeto">
          <input
            type="checkbox"
            checked={somenteComDescricao}
            onChange={(evento) => setSomenteComDescricao(evento.target.checked)}
          />
          Mostrar somente projetos com descricao
        </label>

        <button type="button" className="botao-secundario" onClick={limparFiltros}>
          Limpar filtros
        </button>
      </article>

      <div className="grade-duas-colunas grade-projetos-principal">
        {!ehColaborador ? (
          <FormularioProjeto
            areas={consultaAreas.data ?? []}
            gestores={(consultaUsuarios.data ?? []).map((usuario) => ({
              id: usuario.id,
              nome: usuario.nome,
              email: usuario.email,
            }))}
            emEnvio={mutacaoCriarProjeto.isPending || mutacaoAtualizarProjeto.isPending}
            aoEnviar={async (dados) =>
              enviarProjeto({
                nome: dados.nome,
                descricao: dados.descricao || null,
                areaId: dados.areaId,
                gestorUsuarioId: dados.gestorUsuarioId || null,
              })
            }
            valoresIniciais={
              projetoEmEdicao
                ? {
                    nome: projetoEmEdicao.nome,
                    descricao: projetoEmEdicao.descricao ?? "",
                    areaId: projetoEmEdicao.areaId,
                    gestorUsuarioId: projetoEmEdicao.gestorUsuarioId ?? null,
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
        ) : (
          <article className="cartao-listagem">
            <h3>Permissao de colaborador</h3>
            <p>Seu perfil possui acesso somente de visualizacao para projetos.</p>
          </article>
        )}

        <article className="cartao-listagem cartao-listagem-projetos">
          <header className="cabecalho-listagem-projetos">
            <h3>Projetos cadastrados</h3>
            <span>{projetosFiltrados.length} encontrados</span>
          </header>

          {consultaProjetos.isLoading && (
            <div className="lista-esqueleto-projetos">
              <div className="bloco-esqueleto bloco-esqueleto-linha-projeto" />
              <div className="bloco-esqueleto bloco-esqueleto-linha-projeto" />
              <div className="bloco-esqueleto bloco-esqueleto-linha-projeto" />
            </div>
          )}

          {consultaProjetos.isError && (
            <div className="estado-vazio-projetos">
              <p className="mensagem-erro">
                {obterMensagemErro(consultaProjetos.error, "Falha ao carregar projetos.")}
              </p>
              <button
                type="button"
                className="botao-secundario"
                onClick={() => consultaProjetos.refetch()}
              >
                Tentar novamente
              </button>
            </div>
          )}

          {!consultaProjetos.isLoading &&
            !consultaProjetos.isError &&
            !existemProjetosNoSistema && (
              <div className="estado-vazio-projetos">
                <p>Nenhum projeto cadastrado ate o momento.</p>
                <span>
                  Use o formulario ao lado para iniciar a organizacao da carteira.
                </span>
              </div>
            )}

          {!consultaProjetos.isLoading &&
            !consultaProjetos.isError &&
            existemProjetosNoSistema &&
            !existemProjetosFiltrados && (
              <div className="estado-vazio-projetos">
                <p>Nenhum projeto encontrado com os filtros atuais.</p>
                <button
                  type="button"
                  className="botao-secundario"
                  onClick={limparFiltros}
                >
                  Limpar filtros
                </button>
              </div>
            )}

          {existemProjetosFiltrados && (
            <ul className="lista-com-acoes">
              {projetosFiltrados.map((projeto) => (
                <li
                  className={`item-listagem item-listagem-projeto${
                    projetoEmEdicao?.id === projeto.id
                      ? " item-listagem-projeto-edicao"
                      : ""
                  }`}
                  key={projeto.id}
                >
                  <div className="conteudo-item-listagem">
                    <div className="topo-item-projeto">
                      <strong>{projeto.nome}</strong>
                      <span className="selo-sucesso-projeto">{projeto.areaNome}</span>
                      {projeto.descricao && projeto.descricao.trim().length > 0 ? (
                        <span className="selo-sucesso-projeto">Descricao registrada</span>
                      ) : (
                        <span className="selo-alerta-projeto">Sem descricao</span>
                      )}
                    </div>

                    <span>{projeto.descricao || "Sem descricao."}</span>
                    <small>Criado em {formatarDataHora(projeto.dataCriacao)}</small>
                    {projeto.gestorNome && <small>Gestor: {projeto.gestorNome}</small>}
                  </div>

                  <div className="acoes-item-listagem">
                    <button
                      type="button"
                      className="botao-secundario"
                      onClick={() => setProjetoEmEdicao(projeto)}
                      disabled={mutacaoExcluirProjeto.isPending || ehColaborador}
                    >
                      Editar
                    </button>

                    <button
                      type="button"
                      className="botao-perigo"
                      onClick={() => excluirProjetoComConfirmacao(projeto.id, projeto.nome)}
                      disabled={mutacaoExcluirProjeto.isPending || ehColaborador}
                    >
                      Excluir
                    </button>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </article>
      </div>
    </section>
  );
}

function normalizarTexto(texto: string): string {
  return texto
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .trim()
    .toLowerCase();
}

function formatarDataHora(data: string): string {
  return new Date(data).toLocaleString("pt-BR");
}

function escaparValorCsv(valor: string): string {
  return `"${valor.replace(/"/g, '""')}"`;
}

function obterMensagemErro(excecao: unknown, mensagemPadrao: string): string {
  if (excecao instanceof Error && excecao.message.trim().length > 0) {
    return excecao.message;
  }

  return mensagemPadrao;
}
