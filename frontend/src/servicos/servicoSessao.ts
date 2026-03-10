import type { SessaoAutenticacao } from "../tipos/autenticacao";

const chaveSessao = "gerenciador_tarefas_sessao";

function obterArmazenamentoSessao(): Storage {
  return window.sessionStorage;
}

function obterArmazenamentoPersistente(): Storage {
  return window.localStorage;
}

export function obterSessaoArmazenada(): SessaoAutenticacao | null {
  const sessaoSerializada =
    obterArmazenamentoSessao().getItem(chaveSessao) ||
    obterArmazenamentoPersistente().getItem(chaveSessao);

  if (!sessaoSerializada) {
    return null;
  }

  try {
    return JSON.parse(sessaoSerializada) as SessaoAutenticacao;
  } catch {
    removerSessao();
    return null;
  }
}

export function salvarSessao(
  sessao: SessaoAutenticacao,
  lembrarSessao: boolean
): void {
  removerSessao();

  const armazenamento = lembrarSessao
    ? obterArmazenamentoPersistente()
    : obterArmazenamentoSessao();

  armazenamento.setItem(chaveSessao, JSON.stringify(sessao));
}

export function removerSessao(): void {
  obterArmazenamentoSessao().removeItem(chaveSessao);
  obterArmazenamentoPersistente().removeItem(chaveSessao);
}
