import { useEffect, useMemo, useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import type { AreaResposta } from "../../tipos/areas";

interface PessoaOpcao {
  id: string;
  nome: string;
  email: string;
}

const esquemaFormularioProjeto = z.object({
  nome: z
    .string()
    .min(1, "O nome do projeto deve ser informado.")
    .max(150, "O nome do projeto deve ter no maximo 150 caracteres."),
  descricao: z
    .string()
    .max(1000, "A descricao do projeto deve ter no maximo 1000 caracteres.")
    .optional(),
  areaIds: z
    .array(z.string().uuid("As areas vinculadas devem ser validas."))
    .min(1, "Selecione ao menos uma area."),
  usuarioIdsVinculados: z.array(
    z.string().uuid("Os usuarios vinculados devem ser validos.")
  ),
});

type DadosFormularioProjeto = z.infer<typeof esquemaFormularioProjeto>;

interface PropriedadesFormularioProjeto {
  areas: AreaResposta[];
  pessoas: PessoaOpcao[];
  emEnvio: boolean;
  aoEnviar: (dados: DadosFormularioProjeto) => Promise<void>;
  valoresIniciais?: {
    nome: string;
    descricao?: string | null;
    areaIds: string[];
    usuarioIdsVinculados: string[];
  };
  titulo?: string;
  rotuloBotao?: string;
  rotuloBotaoEmEnvio?: string;
  aoCancelarEdicao?: () => void;
}

export function FormularioProjeto({
  areas,
  pessoas,
  emEnvio,
  aoEnviar,
  valoresIniciais,
  titulo = "Novo projeto",
  rotuloBotao = "Salvar projeto",
  rotuloBotaoEmEnvio = "Salvando...",
  aoCancelarEdicao,
}: PropriedadesFormularioProjeto): JSX.Element {
  const [termoBuscaArea, setTermoBuscaArea] = useState("");
  const [termoBuscaPessoa, setTermoBuscaPessoa] = useState("");
  const valoresPadraoFormulario = useMemo<DadosFormularioProjeto>(
    () => ({
      nome: valoresIniciais?.nome ?? "",
      descricao: valoresIniciais?.descricao ?? "",
      areaIds: valoresIniciais?.areaIds ?? [],
      usuarioIdsVinculados: valoresIniciais?.usuarioIdsVinculados ?? [],
    }),
    [
      valoresIniciais?.nome,
      valoresIniciais?.descricao,
      valoresIniciais?.areaIds,
      valoresIniciais?.usuarioIdsVinculados,
    ]
  );

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors },
  } = useForm<DadosFormularioProjeto>({
    resolver: zodResolver(esquemaFormularioProjeto),
    defaultValues: valoresPadraoFormulario,
  });

  const descricaoDigitada = watch("descricao");
  const areaIdsSelecionadas = watch("areaIds");
  const usuarioIdsSelecionados = watch("usuarioIdsVinculados");
  const quantidadeCaracteresDescricao = descricaoDigitada?.length ?? 0;

  const areasFiltradas = useMemo(
    () =>
      areas
        .filter((area) => !areaIdsSelecionadas.includes(area.id))
        .filter((area) =>
          normalizarTexto(area.nome).includes(normalizarTexto(termoBuscaArea))
        )
        .slice(0, 8),
    [areas, areaIdsSelecionadas, termoBuscaArea]
  );

  const pessoasFiltradas = useMemo(
    () =>
      pessoas
        .filter((pessoa) => !usuarioIdsSelecionados.includes(pessoa.id))
        .filter((pessoa) => {
          const termoNormalizado = normalizarTexto(termoBuscaPessoa);
          return (
            normalizarTexto(pessoa.nome).includes(termoNormalizado) ||
            normalizarTexto(pessoa.email).includes(termoNormalizado)
          );
        })
        .slice(0, 8),
    [pessoas, termoBuscaPessoa, usuarioIdsSelecionados]
  );

  const areasSelecionadas = useMemo(
    () => areas.filter((area) => areaIdsSelecionadas.includes(area.id)),
    [areas, areaIdsSelecionadas]
  );

  const pessoasSelecionadas = useMemo(
    () => pessoas.filter((pessoa) => usuarioIdsSelecionados.includes(pessoa.id)),
    [pessoas, usuarioIdsSelecionados]
  );

  useEffect(() => {
    reset(valoresPadraoFormulario);
    setTermoBuscaArea("");
    setTermoBuscaPessoa("");
  }, [reset, valoresPadraoFormulario]);

  function adicionarArea(areaId: string): void {
    if (areaIdsSelecionadas.includes(areaId)) {
      return;
    }

    setValue("areaIds", [...areaIdsSelecionadas, areaId], {
      shouldDirty: true,
      shouldValidate: true,
    });
    setTermoBuscaArea("");
  }

  function removerArea(areaId: string): void {
    setValue(
      "areaIds",
      areaIdsSelecionadas.filter((idAtual) => idAtual !== areaId),
      {
        shouldDirty: true,
        shouldValidate: true,
      }
    );
  }

  function adicionarPessoa(usuarioId: string): void {
    if (usuarioIdsSelecionados.includes(usuarioId)) {
      return;
    }

    setValue("usuarioIdsVinculados", [...usuarioIdsSelecionados, usuarioId], {
      shouldDirty: true,
      shouldValidate: true,
    });
    setTermoBuscaPessoa("");
  }

  function removerPessoa(usuarioId: string): void {
    setValue(
      "usuarioIdsVinculados",
      usuarioIdsSelecionados.filter((idAtual) => idAtual !== usuarioId),
      {
        shouldDirty: true,
        shouldValidate: true,
      }
    );
  }

  async function enviar(dados: DadosFormularioProjeto): Promise<void> {
    await aoEnviar(dados);

    if (!valoresIniciais) {
      reset({
        nome: "",
        descricao: "",
        areaIds: [],
        usuarioIdsVinculados: [],
      });
      setTermoBuscaArea("");
      setTermoBuscaPessoa("");
    }
  }

  return (
    <form className="formulario-padrao" onSubmit={handleSubmit(enviar)}>
      <div className="cabecalho-formulario-projeto">
        <h3>{titulo}</h3>
        <p>Mantenha nomes objetivos para facilitar busca e rastreabilidade.</p>
      </div>

      <label htmlFor="nomeProjeto">Nome</label>
      <input
        id="nomeProjeto"
        type="text"
        maxLength={150}
        placeholder="Ex.: Implantacao do modulo financeiro"
        autoFocus={!valoresIniciais}
        {...register("nome")}
      />
      {errors.nome && <span className="mensagem-erro">{errors.nome.message}</span>}

      <label htmlFor="descricaoProjeto">Descricao</label>
      <textarea
        id="descricaoProjeto"
        rows={4}
        maxLength={1000}
        placeholder="Contexto, objetivo e informacoes relevantes para a equipe."
        {...register("descricao")}
      />
      <span className="contador-caracteres">{quantidadeCaracteresDescricao}/1000</span>
      {errors.descricao && (
        <span className="mensagem-erro">{errors.descricao.message}</span>
      )}

      <label htmlFor="buscaAreaProjeto">Areas vinculadas</label>
      <input
        id="buscaAreaProjeto"
        type="text"
        placeholder="Digite para filtrar e clique para adicionar area"
        value={termoBuscaArea}
        onChange={(evento) => setTermoBuscaArea(evento.target.value)}
      />
      <div className="lista-opcoes-vinculo">
        {areasFiltradas.map((area) => (
          <button
            key={area.id}
            type="button"
            className="opcao-vinculo"
            onClick={() => adicionarArea(area.id)}
          >
            {area.nome}
          </button>
        ))}
        {areasFiltradas.length === 0 && (
          <span className="mensagem-vazia-vinculo">Nenhuma area disponivel para o filtro.</span>
        )}
      </div>
      <div className="lista-chips-vinculo">
        {areasSelecionadas.map((area) => (
          <button
            key={area.id}
            type="button"
            className="chip-vinculo"
            onClick={() => removerArea(area.id)}
            title="Remover area"
          >
            {area.nome} <span aria-hidden="true">×</span>
          </button>
        ))}
      </div>
      {errors.areaIds && <span className="mensagem-erro">{errors.areaIds.message}</span>}

      <label htmlFor="buscaPessoaProjeto">Pessoas vinculadas (opcional)</label>
      <input
        id="buscaPessoaProjeto"
        type="text"
        placeholder="Digite nome ou email para filtrar e adicionar pessoa"
        value={termoBuscaPessoa}
        onChange={(evento) => setTermoBuscaPessoa(evento.target.value)}
      />
      <div className="lista-opcoes-vinculo">
        {pessoasFiltradas.map((pessoa) => (
          <button
            key={pessoa.id}
            type="button"
            className="opcao-vinculo"
            onClick={() => adicionarPessoa(pessoa.id)}
          >
            <span>{pessoa.nome}</span>
            <small>{pessoa.email}</small>
          </button>
        ))}
        {pessoasFiltradas.length === 0 && (
          <span className="mensagem-vazia-vinculo">
            Nenhuma pessoa disponivel para o filtro.
          </span>
        )}
      </div>
      <div className="lista-chips-vinculo">
        {pessoasSelecionadas.map((pessoa) => (
          <button
            key={pessoa.id}
            type="button"
            className="chip-vinculo"
            onClick={() => removerPessoa(pessoa.id)}
            title="Remover pessoa"
          >
            {pessoa.nome} <span aria-hidden="true">×</span>
          </button>
        ))}
      </div>
      {errors.usuarioIdsVinculados && (
        <span className="mensagem-erro">{errors.usuarioIdsVinculados.message}</span>
      )}

      <div className="linha-botoes-formulario">
        <button type="submit" disabled={emEnvio || areas.length === 0}>
          {emEnvio ? rotuloBotaoEmEnvio : rotuloBotao}
        </button>

        {aoCancelarEdicao && (
          <button
            type="button"
            className="botao-secundario"
            disabled={emEnvio}
            onClick={aoCancelarEdicao}
          >
            Cancelar
          </button>
        )}
      </div>
    </form>
  );
}

function normalizarTexto(texto: string): string {
  return texto
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .trim()
    .toLowerCase();
}
