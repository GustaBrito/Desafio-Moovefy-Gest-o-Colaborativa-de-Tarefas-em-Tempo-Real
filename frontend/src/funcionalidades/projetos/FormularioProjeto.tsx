import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";

const esquemaFormularioProjeto = z.object({
  nome: z
    .string()
    .min(1, "O nome do projeto deve ser informado.")
    .max(150, "O nome do projeto deve ter no maximo 150 caracteres."),
  descricao: z
    .string()
    .max(1000, "A descricao do projeto deve ter no maximo 1000 caracteres.")
    .optional(),
});

type DadosFormularioProjeto = z.infer<typeof esquemaFormularioProjeto>;

interface PropriedadesFormularioProjeto {
  emEnvio: boolean;
  aoEnviar: (dados: DadosFormularioProjeto) => Promise<void>;
}

export function FormularioProjeto({
  emEnvio,
  aoEnviar,
}: PropriedadesFormularioProjeto): JSX.Element {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<DadosFormularioProjeto>({
    resolver: zodResolver(esquemaFormularioProjeto),
    defaultValues: {
      nome: "",
      descricao: "",
    },
  });

  async function enviar(dados: DadosFormularioProjeto): Promise<void> {
    await aoEnviar(dados);
    reset();
  }

  return (
    <form className="formulario-padrao" onSubmit={handleSubmit(enviar)}>
      <h3>Novo projeto</h3>

      <label htmlFor="nomeProjeto">Nome</label>
      <input id="nomeProjeto" type="text" {...register("nome")} />
      {errors.nome && <span className="mensagem-erro">{errors.nome.message}</span>}

      <label htmlFor="descricaoProjeto">Descricao</label>
      <textarea id="descricaoProjeto" rows={3} {...register("descricao")} />
      {errors.descricao && (
        <span className="mensagem-erro">{errors.descricao.message}</span>
      )}

      <button type="submit" disabled={emEnvio}>
        {emEnvio ? "Salvando..." : "Salvar projeto"}
      </button>
    </form>
  );
}
