import { useEffect, useMemo, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { usarNotificacao } from "../ganchos/usarNotificacao";
import { atualizarArea, criarArea, listarAreas } from "../servicos/servicoAreas";
import type { AreaResposta } from "../tipos/areas";

interface EstadoFormularioArea {
  nome: string;
  codigo: string;
  ativa: boolean;
}

const formularioPadrao: EstadoFormularioArea = {
  nome: "",
  codigo: "",
  ativa: true,
};

export function PaginaAreas(): JSX.Element {
  const clienteConsulta = useQueryClient();
  const { mostrarErro, mostrarSucesso } = usarNotificacao();
  const [areaEmEdicao, setAreaEmEdicao] = useState<AreaResposta | null>(null);
  const [modalAreaAberto, setModalAreaAberto] = useState(false);
  const [formulario, setFormulario] = useState<EstadoFormularioArea>(formularioPadrao);
  const [filtroNome, setFiltroNome] = useState("");
  const [filtroCodigo, setFiltroCodigo] = useState("");
  const [filtroSituacao, setFiltroSituacao] = useState("");

  const consultaAreas = useQuery({
    queryKey: ["areas", "administracao"],
    queryFn: () => listarAreas(false),
  });

  const mutacaoCriar = useMutation({
    mutationFn: criarArea,
    onSuccess: async () => {
      await clienteConsulta.invalidateQueries({ queryKey: ["areas"] });
      mostrarSucesso("Area criada com sucesso.");
      fecharModalArea();
    },
    onError: (erro) => mostrarErro(obterMensagemErro(erro, "Falha ao criar area.")),
  });

  const mutacaoAtualizar = useMutation({
    mutationFn: ({
      id,
      dados,
    }: {
      id: string;
      dados: Parameters<typeof atualizarArea>[1];
    }) => atualizarArea(id, dados),
    onSuccess: async () => {
      await clienteConsulta.invalidateQueries({ queryKey: ["areas"] });
      mostrarSucesso("Area atualizada com sucesso.");
      fecharModalArea();
    },
    onError: (erro) =>
      mostrarErro(obterMensagemErro(erro, "Falha ao atualizar area.")),
  });

  useEffect(() => {
    if (!modalAreaAberto) {
      return undefined;
    }

    function tratarTeclaEscape(evento: KeyboardEvent): void {
      if (evento.key !== "Escape") {
        return;
      }

      fecharModalArea();
    }

    window.addEventListener("keydown", tratarTeclaEscape);
    return () => window.removeEventListener("keydown", tratarTeclaEscape);
  }, [modalAreaAberto]);

  const areasOrdenadas = useMemo(
    () =>
      [...(consultaAreas.data ?? [])].sort((atual, proxima) =>
        atual.nome.localeCompare(proxima.nome, "pt-BR")
      ),
    [consultaAreas.data]
  );

  const areasFiltradas = useMemo(() => {
    const termoNome = normalizarTexto(filtroNome);
    const termoCodigo = normalizarTexto(filtroCodigo);

    return areasOrdenadas.filter((area) => {
      if (termoNome && !normalizarTexto(area.nome).includes(termoNome)) {
        return false;
      }

      if (termoCodigo && !normalizarTexto(area.codigo ?? "").includes(termoCodigo)) {
        return false;
      }

      if (filtroSituacao === "ativas" && !area.ativa) {
        return false;
      }

      if (filtroSituacao === "inativas" && area.ativa) {
        return false;
      }

      return true;
    });
  }, [areasOrdenadas, filtroCodigo, filtroNome, filtroSituacao]);

  function limparFormulario(): void {
    setAreaEmEdicao(null);
    setFormulario(formularioPadrao);
  }

  function abrirModalNovaArea(): void {
    limparFormulario();
    setModalAreaAberto(true);
  }

  function fecharModalArea(): void {
    setModalAreaAberto(false);
    limparFormulario();
  }

  function iniciarEdicao(area: AreaResposta): void {
    setAreaEmEdicao(area);
    setFormulario({
      nome: area.nome,
      codigo: area.codigo ?? "",
      ativa: area.ativa,
    });
    setModalAreaAberto(true);
  }

  async function alternarStatusArea(area: AreaResposta): Promise<void> {
    await mutacaoAtualizar.mutateAsync({
      id: area.id,
      dados: {
        nome: area.nome,
        codigo: area.codigo ?? null,
        ativa: !area.ativa,
      },
    });
  }

  function limparFiltrosPesquisa(): void {
    setFiltroNome("");
    setFiltroCodigo("");
    setFiltroSituacao("");
  }

  async function enviarFormulario(evento: FormEvent<HTMLFormElement>): Promise<void> {
    evento.preventDefault();

    if (areaEmEdicao) {
      await mutacaoAtualizar.mutateAsync({
        id: areaEmEdicao.id,
        dados: {
          nome: formulario.nome,
          codigo: formulario.codigo || null,
          ativa: formulario.ativa,
        },
      });
      return;
    }

    await mutacaoCriar.mutateAsync({
      nome: formulario.nome,
      codigo: formulario.codigo || null,
      ativa: formulario.ativa,
    });
  }

  return (
    <section className="pagina-conteudo pagina-areas">
      <header className="cabecalho-pagina cabecalho-areas">
        <div>
          <h1>Areas</h1>
          <p>Cadastro e manutencao de areas organizacionais.</p>
        </div>

        <div className="acoes-cabecalho-projetos">
          <button
            type="button"
            className="botao-secundario botao-acao-principal-projeto"
            onClick={abrirModalNovaArea}
          >
            + Nova area
          </button>
        </div>
      </header>

      <article className="cartao-filtros painel-filtros-areas">
        <h3>Filtros</h3>
        <div className="grade-filtros grade-filtros-areas">
          <label htmlFor="filtroNomeArea">
            Nome
            <input
              id="filtroNomeArea"
              type="text"
              placeholder="Digite para buscar por nome"
              value={filtroNome}
              onChange={(evento) => setFiltroNome(evento.target.value)}
            />
          </label>

          <label htmlFor="filtroCodigoArea">
            Codigo
            <input
              id="filtroCodigoArea"
              type="text"
              placeholder="Digite para buscar por codigo"
              value={filtroCodigo}
              onChange={(evento) => setFiltroCodigo(evento.target.value)}
            />
          </label>

          <label htmlFor="filtroSituacaoArea">
            Situacao
            <select
              id="filtroSituacaoArea"
              value={filtroSituacao}
              onChange={(evento) => setFiltroSituacao(evento.target.value)}
            >
              <option value="">Todas</option>
              <option value="ativas">Ativas</option>
              <option value="inativas">Inativas</option>
            </select>
          </label>
        </div>
        <button
          type="button"
          className="botao-secundario"
          onClick={limparFiltrosPesquisa}
        >
          Limpar filtros
        </button>
      </article>

      <article className="cartao-listagem cartao-listagem-areas-modernizado">
        <header className="cabecalho-listagem-projetos">
          <h3>Areas cadastradas</h3>
          <span>{areasFiltradas.length} registro(s)</span>
        </header>

        {consultaAreas.isLoading && <p>Carregando areas...</p>}
        {consultaAreas.isError && (
          <p className="mensagem-erro">
            {obterMensagemErro(consultaAreas.error, "Falha ao carregar areas.")}
          </p>
        )}

        {!consultaAreas.isLoading && !consultaAreas.isError && (
          <div className="container-tabela-projetos container-tabela-areas">
            <table className="tabela-projetos tabela-areas">
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>Codigo</th>
                  <th>Situacao</th>
                  <th>Acoes</th>
                </tr>
              </thead>
              <tbody>
                {areasFiltradas.map((area) => (
                  <tr key={area.id}>
                    <td>{area.nome}</td>
                    <td>{area.codigo ?? "-"}</td>
                    <td>{area.ativa ? "Ativa" : "Inativa"}</td>
                    <td>
                      <div className="acoes-item-listagem acoes-icones-projeto">
                        <button
                          type="button"
                          className="botao-icone-acao botao-icone-editar"
                          onClick={() => iniciarEdicao(area)}
                          aria-label={`Editar area ${area.nome}`}
                          title="Editar area"
                        >
                          <IconeEditar />
                        </button>
                        <button
                          type="button"
                          className="botao-icone-acao botao-icone-excluir"
                          onClick={() => {
                            void alternarStatusArea(area);
                          }}
                          aria-label={area.ativa ? `Desativar area ${area.nome}` : `Ativar area ${area.nome}`}
                          title={area.ativa ? "Desativar area" : "Ativar area"}
                        >
                          <IconeExcluir />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </article>

      {modalAreaAberto && (
        <div className="sobreposicao-modal" role="presentation">
          <section
            className="cartao-modal cartao-modal-area"
            role="dialog"
            aria-modal="true"
            aria-labelledby="tituloModalArea"
          >
            <header className="cabecalho-modal">
              <h3 id="tituloModalArea">
                {areaEmEdicao ? "Editar area" : "Nova area"}
              </h3>
              <button
                type="button"
                className="botao-fechar-modal"
                onClick={fecharModalArea}
                aria-label="Fechar modal de area"
              >
                x
              </button>
            </header>

            <form className="formulario-padrao" onSubmit={(evento) => void enviarFormulario(evento)}>
              <label htmlFor="nomeArea">Nome</label>
              <input
                id="nomeArea"
                value={formulario.nome}
                onChange={(evento) =>
                  setFormulario((atual) => ({ ...atual, nome: evento.target.value }))
                }
              />

              <label htmlFor="codigoArea">Codigo</label>
              <input
                id="codigoArea"
                value={formulario.codigo}
                onChange={(evento) =>
                  setFormulario((atual) => ({ ...atual, codigo: evento.target.value }))
                }
              />

              <label className="opcao-filtro-projeto">
                <input
                  type="checkbox"
                  checked={formulario.ativa}
                  onChange={(evento) =>
                    setFormulario((atual) => ({ ...atual, ativa: evento.target.checked }))
                  }
                />
                Area ativa
              </label>

              <div className="linha-botoes-formulario">
                <button
                  type="submit"
                  disabled={mutacaoCriar.isPending || mutacaoAtualizar.isPending}
                >
                  {areaEmEdicao ? "Atualizar area" : "Criar area"}
                </button>
                <button
                  type="button"
                  className="botao-secundario"
                  onClick={fecharModalArea}
                  disabled={mutacaoCriar.isPending || mutacaoAtualizar.isPending}
                >
                  Cancelar
                </button>
              </div>
            </form>
          </section>
        </div>
      )}
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

function obterMensagemErro(excecao: unknown, mensagemPadrao: string): string {
  if (excecao instanceof Error && excecao.message.trim().length > 0) {
    return excecao.message;
  }

  return mensagemPadrao;
}

function IconeEditar(): JSX.Element {
  return (
    <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true" focusable="false">
      <path
        d="M4 20h4l10-10-4-4L4 16v4zm14.7-11.3 1.6-1.6a1 1 0 0 0 0-1.4l-2-2a1 1 0 0 0-1.4 0l-1.6 1.6 3.4 3.4z"
        fill="currentColor"
      />
    </svg>
  );
}

function IconeExcluir(): JSX.Element {
  return (
    <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true" focusable="false">
      <path
        d="M9 3h6l1 2h4v2H4V5h4l1-2zm-2 6h10l-1 11a2 2 0 0 1-2 2H10a2 2 0 0 1-2-2L7 9zm3 2v8h2v-8h-2zm4 0v8h2v-8h-2z"
        fill="currentColor"
      />
    </svg>
  );
}
