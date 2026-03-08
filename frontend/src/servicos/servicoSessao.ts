import type { SessaoAutenticacao } from "../tipos/autenticacao";

const chaveSessao = "gerenciador_tarefas_sessao";

export function obterSessaoArmazenada(): SessaoAutenticacao | null {
  const sessaoSerializada = window.localStorage.getItem(chaveSessao);
  if (!sessaoSerializada) {
    return null;
  }

  try {
    return JSON.parse(sessaoSerializada) as SessaoAutenticacao;
  } catch {
    window.localStorage.removeItem(chaveSessao);
    return null;
  }
}

export function salvarSessao(sessao: SessaoAutenticacao): void {
  window.localStorage.setItem(chaveSessao, JSON.stringify(sessao));
}

export function removerSessao(): void {
  window.localStorage.removeItem(chaveSessao);
}
