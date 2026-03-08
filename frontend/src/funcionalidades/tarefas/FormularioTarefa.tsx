import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import type { ProjetoResposta } from "../../tipos/projetos";
import { PrioridadeTarefa } from "../../tipos/tarefas";

const esquemaFormularioTarefa = z.object({
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
  responsavelId: z.string().uuid("O responsavel deve possuir um identificador valido."),
  dataPrazo: z.string().min(1, "A data de prazo deve ser informada."),
});

type DadosFormularioTarefa = z.infer<typeof esquemaFormularioTarefa>;

interface PropriedadesFormularioTarefa {
  projetos: ProjetoResposta[];
  responsavelIdPadrao: string;
  emEnvio: boolean;
  aoEnviar: (dados: DadosFormularioTarefa) => Promise<void>;
}

export function FormularioTarefa({
  projetos,
  responsavelIdPadrao,
  emEnvio,
  aoEnviar,
}: PropriedadesFormularioTarefa): JSX.Element {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<DadosFormularioTarefa>({
    resolver: zodResolver(esquemaFormularioTarefa),
    defaultValues: {
      titulo: "",
      descricao: "",
      prioridade: PrioridadeTarefa.Media,
      projetoId: "",
      responsavelId: responsavelIdPadrao,
      dataPrazo: "",
    },
  });

  async function enviar(dados: DadosFormularioTarefa): Promise<void> {
    await aoEnviar(dados);
    reset({
      titulo: "",
      descricao: "",
      prioridade: PrioridadeTarefa.Media,
      projetoId: "",
      responsavelId: responsavelIdPadrao,
      dataPrazo: "",
    });
  }

  return (
    <form className="formulario-padrao" onSubmit={handleSubmit(enviar)}>
      <h3>Nova tarefa</h3>

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
      <input id="dataPrazoTarefa" type="date" {...register("dataPrazo")} />
      {errors.dataPrazo && (
        <span className="mensagem-erro">{errors.dataPrazo.message}</span>
      )}

      <button type="submit" disabled={emEnvio}>
        {emEnvio ? "Salvando..." : "Salvar tarefa"}
      </button>
    </form>
  );
}
