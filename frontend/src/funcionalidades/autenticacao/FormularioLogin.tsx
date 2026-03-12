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
          className="botao-icone-senha"
          onClick={() => setSenhaVisivel((valorAtual) => !valorAtual)}
          aria-label={senhaVisivel ? "Ocultar senha" : "Mostrar senha"}
        >
          {senhaVisivel ? (
            <svg
              viewBox="0 0 24 24"
              width="18"
              height="18"
              aria-hidden="true"
              focusable="false"
            >
              <path
                d="M3 4L20 21M10.58 10.59C10.21 10.95 10 11.45 10 12C10 13.1 10.9 14 12 14C12.55 14 13.05 13.79 13.41 13.42M9.88 5.09C10.56 4.94 11.27 4.86 12 4.86C16.78 4.86 20.87 8 22.5 12C21.87 13.53 20.88 14.88 19.64 15.94M6.7 6.71C4.93 8.02 3.57 9.85 2.5 12C4.13 16 8.22 19.14 13 19.14C14.52 19.14 15.96 18.82 17.25 18.25"
                fill="none"
                stroke="currentColor"
                strokeWidth="1.8"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
            </svg>
          ) : (
            <svg
              viewBox="0 0 24 24"
              width="18"
              height="18"
              aria-hidden="true"
              focusable="false"
            >
              <path
                d="M2.5 12C4.13 8 8.22 4.86 13 4.86C17.78 4.86 21.87 8 23.5 12C21.87 16 17.78 19.14 13 19.14C8.22 19.14 4.13 16 2.5 12Z"
                fill="none"
                stroke="currentColor"
                strokeWidth="1.8"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
              <circle
                cx="13"
                cy="12"
                r="3"
                fill="none"
                stroke="currentColor"
                strokeWidth="1.8"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
            </svg>
          )}
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
