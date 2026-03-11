import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { useMemo, useState, type KeyboardEvent } from "react";

const esquemaFormularioLogin = z.object({
  email: z
    .string()
    .min(1, "O email deve ser informado.")
    .email("O email informado e invalido."),
  senha: z.string().min(1, "A senha deve ser informada."),
  lembrarSessao: z.boolean(),
});

type DadosFormularioLogin = z.infer<typeof esquemaFormularioLogin>;

interface PropriedadesFormularioLogin {
  emEnvio: boolean;
  estadoApi: "verificando" | "online" | "offline";
  mensagemStatusApi: string;
  erro?: string | null;
  aoEnviar: (dados: DadosFormularioLogin) => Promise<void>;
}

export function FormularioLogin({
  emEnvio,
  estadoApi,
  mensagemStatusApi,
  erro,
  aoEnviar,
}: PropriedadesFormularioLogin): JSX.Element {
  const [senhaVisivel, setSenhaVisivel] = useState(false);
  const [capsLockAtivo, setCapsLockAtivo] = useState(false);

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors, isValid },
  } = useForm<DadosFormularioLogin>({
    resolver: zodResolver(esquemaFormularioLogin),
    mode: "onChange",
    reValidateMode: "onChange",
    defaultValues: {
      email: "",
      senha: "",
      lembrarSessao: true,
    },
  });

  const email = watch("email");
  const senha = watch("senha");
  const registroSenha = register("senha");

  const botaoDesabilitado = useMemo(
    () => emEnvio || !isValid,
    [emEnvio, isValid]
  );

  function preencherAcessoRapido(tipoAcesso: "administrador" | "colaborador"): void {
    if (tipoAcesso === "administrador") {
      setValue("email", "superadmin@gerenciadortarefas.local", { shouldValidate: true });
      setValue("senha", "SuperAdmin@123", { shouldValidate: true });
      return;
    }

    setValue("email", "colaborador.dev@gerenciadortarefas.local", { shouldValidate: true });
    setValue("senha", "ColaboradorDev@123", { shouldValidate: true });
  }

  function atualizarIndicadorCapsLock(evento: KeyboardEvent<HTMLInputElement>): void {
    setCapsLockAtivo(evento.getModifierState("CapsLock"));
  }

  return (
    <form
      className="formulario-padrao formulario-login"
      onSubmit={handleSubmit(aoEnviar)}
      noValidate
    >
      <header className="cabecalho-formulario-login">
        <h2>Acessar plataforma</h2>
        <p>Entre com suas credenciais para continuar.</p>
      </header>

      <div className="status-api-login">
        <span
          className={`indicador-api-login ${estadoApi}`}
          aria-hidden="true"
        />
        <span>{mensagemStatusApi}</span>
      </div>

      <div className="acesso-rapido-login">
        <span>Acesso rapido</span>
        <div className="botoes-acesso-rapido">
          <button
            type="button"
            className="botao-secundario botao-acesso-rapido"
            onClick={() => preencherAcessoRapido("administrador")}
          >
            Preencher Admin
          </button>
          <button
            type="button"
            className="botao-secundario botao-acesso-rapido"
            onClick={() => preencherAcessoRapido("colaborador")}
          >
            Preencher Colaborador
          </button>
        </div>
      </div>

      <label htmlFor="email">Email</label>
      <input
        id="email"
        type="email"
        autoComplete="email"
        aria-invalid={Boolean(errors.email)}
        {...register("email")}
      />
      {errors.email && <span className="mensagem-erro">{errors.email.message}</span>}
      {!errors.email && email.length > 0 && (
        <span className="mensagem-sucesso">Formato de email valido.</span>
      )}

      <label htmlFor="senha">Senha</label>
      <div className="campo-senha-login">
        <input
          id="senha"
          type={senhaVisivel ? "text" : "password"}
          autoComplete="current-password"
          aria-invalid={Boolean(errors.senha)}
          {...registroSenha}
          onKeyDown={atualizarIndicadorCapsLock}
          onKeyUp={atualizarIndicadorCapsLock}
        />
        <button
          type="button"
          className="botao-visualizar-senha"
          onClick={() => setSenhaVisivel((valorAtual) => !valorAtual)}
          aria-label={senhaVisivel ? "Ocultar senha" : "Mostrar senha"}
        >
          {senhaVisivel ? "Ocultar" : "Mostrar"}
        </button>
      </div>
      {errors.senha && <span className="mensagem-erro">{errors.senha.message}</span>}
      {!errors.senha && senha.length > 0 && (
        <span className="mensagem-sucesso">Senha preenchida.</span>
      )}
      {capsLockAtivo && (
        <span className="mensagem-aviso">Caps Lock esta ativado.</span>
      )}

      <label className="opcao-lembrar-sessao">
        <input type="checkbox" {...register("lembrarSessao")} />
        <span>Lembrar sessao neste dispositivo</span>
      </label>

      {erro && <p className="mensagem-erro">{erro}</p>}

      <button type="submit" disabled={botaoDesabilitado}>
        {emEnvio ? "Entrando..." : "Entrar no sistema"}
      </button>
    </form>
  );
}
