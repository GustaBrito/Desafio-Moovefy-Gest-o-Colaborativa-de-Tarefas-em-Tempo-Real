import { useEffect, useMemo, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useFiltrosUsuarios } from "../funcionalidades/usuarios/useFiltrosUsuarios";
import { normalizarTexto, obterMensagemErro } from "../funcionalidades/usuarios/utilitariosUsuarios";
import { usarAutenticacao } from "../ganchos/usarAutenticacao";
import { usarNotificacao } from "../ganchos/usarNotificacao";
import { listarAreas } from "../servicos/servicoAreas";
import {
  alterarStatusUsuario,
  atualizarUsuario,
  criarUsuario,
  listarUsuarios,
} from "../servicos/servicoUsuarios";
import { PerfilGlobalUsuario } from "../tipos/autenticacao";
import type { CriarUsuarioRequisicao, UsuarioResposta } from "../tipos/usuarios";

interface EstadoFormularioUsuario {
  nome: string;
  email: string;
  senha: string;
  perfilGlobal: PerfilGlobalUsuario;
  ativo: boolean;
  areaIds: string[];
}

const formularioPadrao: EstadoFormularioUsuario = {
  nome: "",
  email: "",
  senha: "",
  perfilGlobal: PerfilGlobalUsuario.Colaborador,
  ativo: true,
  areaIds: [],
};

export function PaginaUsuarios(): JSX.Element {
  const clienteConsulta = useQueryClient();
  const { ehSuperAdmin } = usarAutenticacao();
  const { mostrarErro, mostrarSucesso } = usarNotificacao();
  const [usuarioEmEdicao, setUsuarioEmEdicao] = useState<UsuarioResposta | null>(null);
  const [modalUsuarioAberto, setModalUsuarioAberto] = useState(false);
  const [formulario, setFormulario] = useState<EstadoFormularioUsuario>(formularioPadrao);
  const [termoBuscaAreaFormulario, setTermoBuscaAreaFormulario] = useState("");

  const consultaUsuarios = useQuery({
    queryKey: ["usuarios", "administracao"],
    queryFn: () => listarUsuarios(false),
  });

  const consultaAreas = useQuery({
    queryKey: ["areas", "ativas"],
    queryFn: () => listarAreas(true),
  });

  const mutacaoCriar = useMutation({
    mutationFn: criarUsuario,
    onSuccess: async () => {
      await clienteConsulta.invalidateQueries({ queryKey: ["usuarios"] });
      mostrarSucesso("Usuario criado com sucesso.");
      fecharModalUsuario();
    },
    onError: (erro) => mostrarErro(obterMensagemErro(erro, "Falha ao criar usuario.")),
  });

  const mutacaoAtualizar = useMutation({
    mutationFn: ({
      id,
      dados,
    }: {
      id: string;
      dados: Parameters<typeof atualizarUsuario>[1];
    }) => atualizarUsuario(id, dados),
    onSuccess: async () => {
      await clienteConsulta.invalidateQueries({ queryKey: ["usuarios"] });
      mostrarSucesso("Usuario atualizado com sucesso.");
      fecharModalUsuario();
    },
    onError: (erro) => mostrarErro(obterMensagemErro(erro, "Falha ao atualizar usuario.")),
  });

  const mutacaoStatus = useMutation({
    mutationFn: ({ id, ativo }: { id: string; ativo: boolean }) =>
      alterarStatusUsuario(id, { ativo }),
    onSuccess: async () => {
      await clienteConsulta.invalidateQueries({ queryKey: ["usuarios"] });
      mostrarSucesso("Status do usuario atualizado.");
    },
    onError: (erro) =>
      mostrarErro(obterMensagemErro(erro, "Falha ao atualizar status do usuario.")),
  });

  useEffect(() => {
    if (!modalUsuarioAberto) {
      return undefined;
    }

    function tratarTeclaEscape(evento: KeyboardEvent): void {
      if (evento.key !== "Escape") {
        return;
      }

      fecharModalUsuario();
    }

    window.addEventListener("keydown", tratarTeclaEscape);
    return () => window.removeEventListener("keydown", tratarTeclaEscape);
  }, [modalUsuarioAberto]);

  const usuariosOrdenados = useMemo(
    () =>
      [...(consultaUsuarios.data ?? [])].sort((usuarioAtual, proximoUsuario) =>
        usuarioAtual.nome.localeCompare(proximoUsuario.nome, "pt-BR")
      ),
    [consultaUsuarios.data]
  );

  const {
    filtroNome,
    filtroAreaId,
    filtroPerfilGlobal,
    usuariosFiltrados,
    setFiltroNome,
    setFiltroAreaId,
    setFiltroPerfilGlobal,
    limparFiltrosPesquisa,
  } = useFiltrosUsuarios(usuariosOrdenados);

  const areasFiltradasFormulario = useMemo(
    () =>
      (consultaAreas.data ?? [])
        .filter((area) => !formulario.areaIds.includes(area.id))
        .filter((area) =>
          normalizarTexto(area.nome).includes(normalizarTexto(termoBuscaAreaFormulario))
        )
        .slice(0, 8),
    [consultaAreas.data, formulario.areaIds, termoBuscaAreaFormulario]
  );

  const areasSelecionadasFormulario = useMemo(
    () =>
      (consultaAreas.data ?? []).filter((area) =>
        formulario.areaIds.includes(area.id)
      ),
    [consultaAreas.data, formulario.areaIds]
  );

  function abrirModalNovoUsuario(): void {
    setUsuarioEmEdicao(null);
    setFormulario(formularioPadrao);
    setTermoBuscaAreaFormulario("");
    setModalUsuarioAberto(true);
  }

  function limparFormulario(): void {
    setUsuarioEmEdicao(null);
    setFormulario(formularioPadrao);
    setTermoBuscaAreaFormulario("");
  }

  function fecharModalUsuario(): void {
    setModalUsuarioAberto(false);
    limparFormulario();
  }

  function iniciarEdicao(usuario: UsuarioResposta): void {
    setUsuarioEmEdicao(usuario);
    setFormulario({
      nome: usuario.nome,
      email: usuario.email,
      senha: "",
      perfilGlobal: usuario.perfilGlobal,
      ativo: usuario.ativo,
      areaIds: usuario.areaIds,
    });
    setTermoBuscaAreaFormulario("");
    setModalUsuarioAberto(true);
  }

  async function enviarFormulario(evento: FormEvent<HTMLFormElement>): Promise<void> {
    evento.preventDefault();

    const payloadBase: CriarUsuarioRequisicao = {
      nome: formulario.nome,
      email: formulario.email,
      senha: formulario.senha,
      perfilGlobal: formulario.perfilGlobal,
      ativo: formulario.ativo,
      areaIds: formulario.areaIds,
    };

    if (usuarioEmEdicao) {
      await mutacaoAtualizar.mutateAsync({
        id: usuarioEmEdicao.id,
        dados: {
          nome: formulario.nome,
          email: formulario.email,
          perfilGlobal: formulario.perfilGlobal,
          ativo: formulario.ativo,
          novaSenha: formulario.senha.trim().length > 0 ? formulario.senha : null,
          areaIds: formulario.areaIds,
        },
      });
      return;
    }

    await mutacaoCriar.mutateAsync(payloadBase);
  }

  function adicionarAreaSelecionada(areaId: string): void {
    if (formulario.areaIds.includes(areaId)) {
      return;
    }

    setFormulario((atual) => ({
      ...atual,
      areaIds: [...atual.areaIds, areaId],
    }));
    setTermoBuscaAreaFormulario("");
  }

  function removerAreaSelecionada(areaId: string): void {
    setFormulario((atual) => ({
      ...atual,
      areaIds: atual.areaIds.filter((idAtual) => idAtual !== areaId),
    }));
  }

  function podeSelecionarPerfil(perfil: PerfilGlobalUsuario): boolean {
    if (ehSuperAdmin) {
      return true;
    }

    return perfil === PerfilGlobalUsuario.Colaborador;
  }

  return (
    <section className="pagina-conteudo pagina-usuarios">
      <header className="cabecalho-pagina cabecalho-usuarios">
        <div>
          <h1>Usuarios</h1>
          <p>Administracao de usuarios com escopo por area.</p>
        </div>

        <div className="acoes-cabecalho-projetos">
          <button
            type="button"
            className="botao-secundario botao-acao-principal-projeto"
            onClick={abrirModalNovoUsuario}
          >
            + Novo usuario
          </button>
        </div>
      </header>

      <article className="cartao-filtros painel-filtros-usuarios">
        <h3>Filtros</h3>
        <div className="grade-filtros grade-filtros-usuarios">
          <label htmlFor="filtroNomeUsuario">
            Nome
            <input
              id="filtroNomeUsuario"
              type="text"
              placeholder="Digite para buscar por nome"
              value={filtroNome}
              onChange={(evento) => setFiltroNome(evento.target.value)}
            />
          </label>

          <label htmlFor="filtroAreaUsuario">
            Area
            <select
              id="filtroAreaUsuario"
              value={filtroAreaId}
              onChange={(evento) => setFiltroAreaId(evento.target.value)}
            >
              <option value="">Todas</option>
              {(consultaAreas.data ?? []).map((area) => (
                <option key={area.id} value={area.id}>
                  {area.nome}
                </option>
              ))}
            </select>
          </label>

          <label htmlFor="filtroPerfilUsuario">
            Perfil global
            <select
              id="filtroPerfilUsuario"
              value={filtroPerfilGlobal}
              onChange={(evento) => setFiltroPerfilGlobal(evento.target.value)}
            >
              <option value="">Todos</option>
              <option value={PerfilGlobalUsuario.SuperAdmin}>SuperAdmin</option>
              <option value={PerfilGlobalUsuario.Admin}>Admin</option>
              <option value={PerfilGlobalUsuario.Colaborador}>Colaborador</option>
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

      <article className="cartao-listagem cartao-listagem-usuarios-modernizado">
        <header className="cabecalho-listagem-projetos">
          <h3>Usuarios cadastrados</h3>
          <span>{usuariosFiltrados.length} registro(s)</span>
        </header>

        {consultaUsuarios.isLoading && <p>Carregando usuarios...</p>}
        {consultaUsuarios.isError && (
          <p className="mensagem-erro">
            {obterMensagemErro(consultaUsuarios.error, "Falha ao carregar usuarios.")}
          </p>
        )}

        {!consultaUsuarios.isLoading && !consultaUsuarios.isError && (
          <div className="container-tabela-projetos container-tabela-usuarios">
            <table className="tabela-projetos tabela-usuarios">
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>Email</th>
                  <th>Perfil</th>
                  <th>Status</th>
                  <th>Areas</th>
                  <th>Acoes</th>
                </tr>
              </thead>
              <tbody>
                {usuariosFiltrados.map((usuario) => (
                  <tr key={usuario.id}>
                    <td>{usuario.nome}</td>
                    <td>{usuario.email}</td>
                    <td>{PerfilGlobalUsuario[usuario.perfilGlobal]}</td>
                    <td>{usuario.ativo ? "Ativo" : "Inativo"}</td>
                    <td>{usuario.areaNomes.join(", ") || "Sem area"}</td>
                    <td>
                      <div className="acoes-item-listagem acoes-icones-projeto">
                        <button
                          type="button"
                          className="botao-icone-acao botao-icone-editar"
                          onClick={() => iniciarEdicao(usuario)}
                          aria-label={`Editar usuario ${usuario.nome}`}
                          title="Editar usuario"
                        >
                          <IconeEditar />
                        </button>
                        <button
                          type="button"
                          className="botao-icone-acao botao-icone-excluir"
                          onClick={() =>
                            mutacaoStatus.mutate({
                              id: usuario.id,
                              ativo: !usuario.ativo,
                            })
                          }
                          aria-label={usuario.ativo ? `Desativar usuario ${usuario.nome}` : `Ativar usuario ${usuario.nome}`}
                          title={usuario.ativo ? "Desativar usuario" : "Ativar usuario"}
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

      {modalUsuarioAberto && (
        <div className="sobreposicao-modal" role="presentation">
          <section
            className="cartao-modal cartao-modal-usuario"
            role="dialog"
            aria-modal="true"
            aria-labelledby="tituloModalUsuario"
          >
            <header className="cabecalho-modal">
              <h3 id="tituloModalUsuario">
                {usuarioEmEdicao ? "Editar usuario" : "Novo usuario"}
              </h3>
              <button
                type="button"
                className="botao-fechar-modal"
                onClick={fecharModalUsuario}
                aria-label="Fechar modal de usuario"
              >
                x
              </button>
            </header>

            <form className="formulario-padrao" onSubmit={(evento) => void enviarFormulario(evento)}>
              <label htmlFor="nomeUsuario">Nome</label>
              <input
                id="nomeUsuario"
                value={formulario.nome}
                onChange={(evento) =>
                  setFormulario((atual) => ({ ...atual, nome: evento.target.value }))
                }
              />

              <label htmlFor="emailUsuario">Email</label>
              <input
                id="emailUsuario"
                type="email"
                value={formulario.email}
                onChange={(evento) =>
                  setFormulario((atual) => ({ ...atual, email: evento.target.value }))
                }
              />

              <label htmlFor="senhaUsuario">
                {usuarioEmEdicao ? "Nova senha (opcional)" : "Senha"}
              </label>
              <input
                id="senhaUsuario"
                type="password"
                value={formulario.senha}
                onChange={(evento) =>
                  setFormulario((atual) => ({ ...atual, senha: evento.target.value }))
                }
              />

              <label htmlFor="perfilUsuario">Perfil global</label>
              <select
                id="perfilUsuario"
                value={formulario.perfilGlobal}
                onChange={(evento) =>
                  setFormulario((atual) => ({
                    ...atual,
                    perfilGlobal: Number(evento.target.value) as PerfilGlobalUsuario,
                  }))
                }
              >
                <option
                  value={PerfilGlobalUsuario.SuperAdmin}
                  disabled={!podeSelecionarPerfil(PerfilGlobalUsuario.SuperAdmin)}
                >
                  SuperAdmin
                </option>
                <option
                  value={PerfilGlobalUsuario.Admin}
                  disabled={!podeSelecionarPerfil(PerfilGlobalUsuario.Admin)}
                >
                  Admin
                </option>
                <option value={PerfilGlobalUsuario.Colaborador}>Colaborador</option>
              </select>

              <label className="opcao-filtro-projeto">
                <input
                  type="checkbox"
                  checked={formulario.ativo}
                  onChange={(evento) =>
                    setFormulario((atual) => ({ ...atual, ativo: evento.target.checked }))
                  }
                />
                Usuario ativo
              </label>

              <label htmlFor="buscaAreaUsuario">Areas vinculadas</label>
              <input
                id="buscaAreaUsuario"
                type="text"
                placeholder="Digite para filtrar e adicionar area"
                value={termoBuscaAreaFormulario}
                onChange={(evento) => setTermoBuscaAreaFormulario(evento.target.value)}
              />
              <div className="lista-opcoes-vinculo">
                {areasFiltradasFormulario.map((area) => (
                  <button
                    key={area.id}
                    type="button"
                    className="opcao-vinculo"
                    onClick={() => adicionarAreaSelecionada(area.id)}
                  >
                    {area.nome}
                  </button>
                ))}
                {areasFiltradasFormulario.length === 0 && (
                  <span className="mensagem-vazia-vinculo">
                    Nenhuma area disponivel para o filtro.
                  </span>
                )}
              </div>

              <div className="lista-chips-vinculo">
                {areasSelecionadasFormulario.map((area) => (
                  <button
                    key={area.id}
                    type="button"
                    className="chip-vinculo"
                    onClick={() => removerAreaSelecionada(area.id)}
                    title="Remover area"
                  >
                    {area.nome} <span aria-hidden="true">x</span>
                  </button>
                ))}
              </div>

              <div className="linha-botoes-formulario">
                <button
                  type="submit"
                  disabled={mutacaoCriar.isPending || mutacaoAtualizar.isPending}
                >
                  {usuarioEmEdicao ? "Atualizar usuario" : "Criar usuario"}
                </button>
                <button
                  type="button"
                  className="botao-secundario"
                  onClick={fecharModalUsuario}
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
