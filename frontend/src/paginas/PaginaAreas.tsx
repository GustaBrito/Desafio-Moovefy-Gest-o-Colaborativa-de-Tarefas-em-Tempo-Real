import { useState, type FormEvent } from "react";
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
  const [formulario, setFormulario] = useState<EstadoFormularioArea>(formularioPadrao);

  const consultaAreas = useQuery({
    queryKey: ["areas", "administracao"],
    queryFn: () => listarAreas(false),
  });

  const mutacaoCriar = useMutation({
    mutationFn: criarArea,
    onSuccess: async () => {
      await clienteConsulta.invalidateQueries({ queryKey: ["areas"] });
      mostrarSucesso("Area criada com sucesso.");
      limparFormulario();
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
      limparFormulario();
    },
    onError: (erro) =>
      mostrarErro(obterMensagemErro(erro, "Falha ao atualizar area.")),
  });

  function limparFormulario(): void {
    setAreaEmEdicao(null);
    setFormulario(formularioPadrao);
  }

  function iniciarEdicao(area: AreaResposta): void {
    setAreaEmEdicao(area);
    setFormulario({
      nome: area.nome,
      codigo: area.codigo ?? "",
      ativa: area.ativa,
    });
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
    <section className="pagina-conteudo">
      <header className="cabecalho-pagina">
        <div>
          <h1>Areas</h1>
          <p>Cadastro e manutencao de areas organizacionais.</p>
        </div>
      </header>

      <div className="grade-duas-colunas">
        <form className="formulario-padrao" onSubmit={(evento) => void enviarFormulario(evento)}>
          <h3>{areaEmEdicao ? "Editar area" : "Nova area"}</h3>

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
            <button type="submit" disabled={mutacaoCriar.isPending || mutacaoAtualizar.isPending}>
              {areaEmEdicao ? "Atualizar area" : "Criar area"}
            </button>
            {areaEmEdicao && (
              <button type="button" className="botao-secundario" onClick={limparFormulario}>
                Cancelar
              </button>
            )}
          </div>
        </form>

        <article className="cartao-listagem">
          <header className="cabecalho-listagem-projetos">
            <h3>Areas cadastradas</h3>
            <span>{consultaAreas.data?.length ?? 0} registro(s)</span>
          </header>

          {consultaAreas.isLoading && <p>Carregando areas...</p>}
          {consultaAreas.isError && (
            <p className="mensagem-erro">
              {obterMensagemErro(consultaAreas.error, "Falha ao carregar areas.")}
            </p>
          )}

          {!consultaAreas.isLoading && !consultaAreas.isError && (
            <ul className="lista-com-acoes">
              {(consultaAreas.data ?? []).map((area) => (
                <li className="item-listagem" key={area.id}>
                  <div className="conteudo-item-listagem">
                    <strong>{area.nome}</strong>
                    <span>Codigo: {area.codigo ?? "-"}</span>
                    <small>{area.ativa ? "Ativa" : "Inativa"}</small>
                  </div>
                  <div className="acoes-item-listagem">
                    <button
                      type="button"
                      className="botao-secundario"
                      onClick={() => iniciarEdicao(area)}
                    >
                      Editar
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
