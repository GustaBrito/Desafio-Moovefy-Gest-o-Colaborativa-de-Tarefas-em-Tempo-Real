import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";

const esquemaFormularioLogin = z.object({
  email: z
    .string()
    .min(1, "O email deve ser informado.")
    .email("O email informado e invalido."),
  senha: z.string().min(1, "A senha deve ser informada."),
});

type DadosFormularioLogin = z.infer<typeof esquemaFormularioLogin>;

interface PropriedadesFormularioLogin {
  emEnvio: boolean;
  erro?: string | null;
  aoEnviar: (dados: DadosFormularioLogin) => Promise<void>;
}

export function FormularioLogin({
  emEnvio,
  erro,
  aoEnviar,
}: PropriedadesFormularioLogin): JSX.Element {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<DadosFormularioLogin>({
    resolver: zodResolver(esquemaFormularioLogin),
    defaultValues: {
      email: "admin@gerenciadortarefas.local",
      senha: "Admin@123",
    },
  });

  return (
    <form className="formulario-padrao" onSubmit={handleSubmit(aoEnviar)}>
      <h2>Acesso ao sistema</h2>

      <label htmlFor="email">Email</label>
      <input id="email" type="email" {...register("email")} />
      {errors.email && <span className="mensagem-erro">{errors.email.message}</span>}

      <label htmlFor="senha">Senha</label>
      <input id="senha" type="password" {...register("senha")} />
      {errors.senha && <span className="mensagem-erro">{errors.senha.message}</span>}

      {erro && <p className="mensagem-erro">{erro}</p>}

      <button type="submit" disabled={emEnvio}>
        {emEnvio ? "Entrando..." : "Entrar"}
      </button>
    </form>
  );
}
