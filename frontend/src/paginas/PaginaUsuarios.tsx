import { useMemo, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
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
  const [formulario, setFormulario] = useState<EstadoFormularioUsuario>(formularioPadrao);

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
      limparFormulario();
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
      limparFormulario();
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

  const usuariosOrdenados = useMemo(
    () =>
      [...(consultaUsuarios.data ?? [])].sort((usuarioAtual, proximoUsuario) =>
        usuarioAtual.nome.localeCompare(proximoUsuario.nome, "pt-BR")
      ),
    [consultaUsuarios.data]
  );

  function limparFormulario(): void {
    setUsuarioEmEdicao(null);
    setFormulario(formularioPadrao);
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

  function alternarAreaSelecionada(areaId: string): void {
    setFormulario((formularioAtual) => {
      const areaJaSelecionada = formularioAtual.areaIds.includes(areaId);
      if (areaJaSelecionada) {
        return {
          ...formularioAtual,
          areaIds: formularioAtual.areaIds.filter((idAtual) => idAtual !== areaId),
        };
      }

      return {
        ...formularioAtual,
        areaIds: [...formularioAtual.areaIds, areaId],
      };
    });
  }

  function podeSelecionarPerfil(perfil: PerfilGlobalUsuario): boolean {
    if (ehSuperAdmin) {
      return true;
    }

    return perfil === PerfilGlobalUsuario.Colaborador;
  }

  return (
    <section className="pagina-conteudo">
      <header className="cabecalho-pagina">
        <div>
          <h1>Usuarios</h1>
          <p>Administracao de usuarios com escopo por area.</p>
        </div>
      </header>

      <div className="grade-duas-colunas">
        <form className="formulario-padrao" onSubmit={(evento) => void enviarFormulario(evento)}>
          <h3>{usuarioEmEdicao ? "Editar usuario" : "Novo usuario"}</h3>

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
            <option value={PerfilGlobalUsuario.SuperAdmin} disabled={!podeSelecionarPerfil(PerfilGlobalUsuario.SuperAdmin)}>
              SuperAdmin
            </option>
            <option value={PerfilGlobalUsuario.Admin} disabled={!podeSelecionarPerfil(PerfilGlobalUsuario.Admin)}>
              Admin
            </option>
            <option value={PerfilGlobalUsuario.Colaborador}>
              Colaborador
            </option>
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

          <fieldset>
            <legend>Areas</legend>
            <div className="grade-filtros">
              {(consultaAreas.data ?? []).map((area) => (
                <label className="opcao-filtro-projeto" key={area.id}>
                  <input
                    type="checkbox"
                    checked={formulario.areaIds.includes(area.id)}
                    onChange={() => alternarAreaSelecionada(area.id)}
                  />
                  {area.nome}
                </label>
              ))}
            </div>
          </fieldset>

          <div className="linha-botoes-formulario">
            <button
              type="submit"
              disabled={mutacaoCriar.isPending || mutacaoAtualizar.isPending}
            >
              {usuarioEmEdicao ? "Atualizar usuario" : "Criar usuario"}
            </button>
            {usuarioEmEdicao && (
              <button
                type="button"
                className="botao-secundario"
                onClick={limparFormulario}
              >
                Cancelar
              </button>
            )}
          </div>
        </form>

        <article className="cartao-listagem">
          <header className="cabecalho-listagem-projetos">
            <h3>Usuarios cadastrados</h3>
            <span>{usuariosOrdenados.length} registro(s)</span>
          </header>

          {consultaUsuarios.isLoading && <p>Carregando usuarios...</p>}
          {consultaUsuarios.isError && (
            <p className="mensagem-erro">
              {obterMensagemErro(consultaUsuarios.error, "Falha ao carregar usuarios.")}
            </p>
          )}

          {!consultaUsuarios.isLoading && !consultaUsuarios.isError && (
            <ul className="lista-com-acoes">
              {usuariosOrdenados.map((usuario) => (
                <li className="item-listagem" key={usuario.id}>
                  <div className="conteudo-item-listagem">
                    <strong>{usuario.nome}</strong>
                    <span>{usuario.email}</span>
                    <small>
                      Perfil: {PerfilGlobalUsuario[usuario.perfilGlobal]} |{" "}
                      {usuario.ativo ? "Ativo" : "Inativo"}
                    </small>
                    <small>Areas: {usuario.areaNomes.join(", ") || "Sem area"}</small>
                  </div>
                  <div className="acoes-item-listagem">
                    <button
                      type="button"
                      className="botao-secundario"
                      onClick={() => iniciarEdicao(usuario)}
                    >
                      Editar
                    </button>
                    <button
                      type="button"
                      className={usuario.ativo ? "botao-perigo" : "botao-secundario"}
                      onClick={() =>
                        mutacaoStatus.mutate({
                          id: usuario.id,
                          ativo: !usuario.ativo,
                        })
                      }
                    >
                      {usuario.ativo ? "Desativar" : "Ativar"}
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

function obterMensagemErro(excecao: unknown, mensagemPadrao: string): string {
  if (excecao instanceof Error && excecao.message.trim().length > 0) {
    return excecao.message;
  }

  return mensagemPadrao;
}
