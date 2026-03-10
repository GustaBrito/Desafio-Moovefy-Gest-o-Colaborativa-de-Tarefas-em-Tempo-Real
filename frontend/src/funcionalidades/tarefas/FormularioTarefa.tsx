import { useEffect, useMemo } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import type { ProjetoResposta } from "../../tipos/projetos";
import { PrioridadeTarefa } from "../../tipos/tarefas";

function criarEsquemaFormularioTarefa(permitirPrazoPassado: boolean) {
  return z.object({
    titulo: z
      .string()
      .min(1, "O titulo da tarefa deve ser informado.")
      .max(200, "O titulo da tarefa deve ter no maximo 200 caracteres."),
    descricao: z
      .string()
      .max(2000, "A descricao da tarefa deve ter no maximo 2000 caracteres.")
      .optional(),
    prioridade: z.nativeEnum(PrioridadeTarefa),
    projetoId: z.string().uuid("Selecione um projeto valido."),
    responsavelId: z
      .string()
      .uuid("O responsavel deve possuir um identificador valido."),
    dataPrazo: z
      .string()
      .min(1, "A data de prazo deve ser informada.")
      .refine(
        (valor) => {
          if (permitirPrazoPassado) {
            return true;
          }

          const dataSelecionada = new Date(`${valor}T00:00:00`);
          const dataAtual = new Date();
          dataAtual.setHours(0, 0, 0, 0);

          return dataSelecionada >= dataAtual;
        },
        "A data de prazo nao pode estar no passado."
      ),
  });
}

const esquemaFormularioCriacaoTarefa = criarEsquemaFormularioTarefa(false);

type DadosFormularioTarefa = z.infer<typeof esquemaFormularioCriacaoTarefa>;

interface PropriedadesFormularioTarefa {
  projetos: ProjetoResposta[];
  responsavelIdPadrao: string;
  emEnvio: boolean;
  titulo?: string;
  rotuloBotao?: string;
  rotuloBotaoEmEnvio?: string;
  valoresIniciais?: DadosFormularioTarefa;
  permitirPrazoPassado?: boolean;
  aoCancelarEdicao?: () => void;
  aoEnviar: (dados: DadosFormularioTarefa) => Promise<void>;
}

export function FormularioTarefa({
  projetos,
  responsavelIdPadrao,
  emEnvio,
  titulo = "Nova tarefa",
  rotuloBotao = "Salvar tarefa",
  rotuloBotaoEmEnvio = "Salvando...",
  valoresIniciais,
  permitirPrazoPassado = false,
  aoCancelarEdicao,
  aoEnviar,
}: PropriedadesFormularioTarefa): JSX.Element {
  const esquemaFormulario = useMemo(
    () => criarEsquemaFormularioTarefa(permitirPrazoPassado),
    [permitirPrazoPassado]
  );

  const valoresPadrao = useMemo<DadosFormularioTarefa>(
    () => ({
      titulo: valoresIniciais?.titulo ?? "",
      descricao: valoresIniciais?.descricao ?? "",
      prioridade: valoresIniciais?.prioridade ?? PrioridadeTarefa.Media,
      projetoId: valoresIniciais?.projetoId ?? "",
      responsavelId: valoresIniciais?.responsavelId ?? responsavelIdPadrao,
      dataPrazo: valoresIniciais?.dataPrazo ?? "",
    }),
    [responsavelIdPadrao, valoresIniciais]
  );

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    formState: { errors },
  } = useForm<DadosFormularioTarefa>({
    resolver: zodResolver(esquemaFormulario),
    defaultValues: valoresPadrao,
  });

  useEffect(() => {
    if (!valoresIniciais && responsavelIdPadrao.trim().length > 0) {
      setValue("responsavelId", responsavelIdPadrao);
    }
  }, [valoresIniciais, responsavelIdPadrao, setValue]);

  useEffect(() => {
    reset(valoresPadrao);
  }, [reset, valoresPadrao]);

  async function enviar(dados: DadosFormularioTarefa): Promise<void> {
    await aoEnviar(dados);

    if (!valoresIniciais) {
      reset({
        titulo: "",
        descricao: "",
        prioridade: PrioridadeTarefa.Media,
        projetoId: "",
        responsavelId: responsavelIdPadrao,
        dataPrazo: "",
      });
    }
  }

  const dataAtual = new Date();
  const dataMinimaPrazo = `${dataAtual.getFullYear()}-${String(
    dataAtual.getMonth() + 1
  ).padStart(2, "0")}-${String(dataAtual.getDate()).padStart(2, "0")}`;

  return (
    <form className="formulario-padrao" onSubmit={handleSubmit(enviar)}>
      <h3>{titulo}</h3>

      <label htmlFor="tituloTarefa">Titulo</label>
      <input id="tituloTarefa" type="text" {...register("titulo")} />
      {errors.titulo && <span className="mensagem-erro">{errors.titulo.message}</span>}

      <label htmlFor="descricaoTarefa">Descricao</label>
      <textarea id="descricaoTarefa" rows={3} {...register("descricao")} />
      {errors.descricao && (
        <span className="mensagem-erro">{errors.descricao.message}</span>
      )}

      <label htmlFor="projetoTarefa">Projeto</label>
      <select id="projetoTarefa" {...register("projetoId")}>
        <option value="">Selecione</option>
        {projetos.map((projeto) => (
          <option key={projeto.id} value={projeto.id}>
            {projeto.nome}
          </option>
        ))}
      </select>
      {errors.projetoId && (
        <span className="mensagem-erro">{errors.projetoId.message}</span>
      )}
      {projetos.length === 0 && (
        <span className="mensagem-erro">
          Nenhum projeto disponivel. Cadastre um projeto antes de criar tarefas.
        </span>
      )}

      <label htmlFor="prioridadeTarefa">Prioridade</label>
      <select
        id="prioridadeTarefa"
        {...register("prioridade", { valueAsNumber: true })}
      >
        <option value={PrioridadeTarefa.Baixa}>Baixa</option>
        <option value={PrioridadeTarefa.Media}>Media</option>
        <option value={PrioridadeTarefa.Alta}>Alta</option>
        <option value={PrioridadeTarefa.Urgente}>Urgente</option>
      </select>

      <label htmlFor="responsavelTarefa">Responsavel (Id)</label>
      <input id="responsavelTarefa" type="text" {...register("responsavelId")} />
      {errors.responsavelId && (
        <span className="mensagem-erro">{errors.responsavelId.message}</span>
      )}

      <label htmlFor="dataPrazoTarefa">Data de prazo</label>
      <input
        id="dataPrazoTarefa"
        type="date"
        min={permitirPrazoPassado ? undefined : dataMinimaPrazo}
        {...register("dataPrazo")}
      />
      {errors.dataPrazo && (
        <span className="mensagem-erro">{errors.dataPrazo.message}</span>
      )}

      <div className="linha-botoes-formulario">
        <button type="submit" disabled={emEnvio || projetos.length === 0}>
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
