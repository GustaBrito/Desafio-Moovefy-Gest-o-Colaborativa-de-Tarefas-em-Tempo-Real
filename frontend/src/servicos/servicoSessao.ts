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
    const sessao = JSON.parse(sessaoSerializada) as Partial<SessaoAutenticacao>;

    if (!sessao.usuarioId || !sessao.tokenAcesso || !sessao.email || !sessao.nome) {
      removerSessao();
      return null;
    }

    const sessaoNormalizada: SessaoAutenticacao = {
      usuarioId: sessao.usuarioId,
      nome: sessao.nome,
      email: sessao.email,
      perfilGlobal: sessao.perfilGlobal ?? 3,
      areaIds: sessao.areaIds ?? [],
      tokenAcesso: sessao.tokenAcesso,
      tipoToken: sessao.tipoToken ?? "Bearer",
      expiraEmUtc: sessao.expiraEmUtc ?? new Date().toISOString(),
    };

    if (sessaoExpirou(sessaoNormalizada)) {
      removerSessao();
      return null;
    }

    return sessaoNormalizada;
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

export function sessaoExpirou(sessao: SessaoAutenticacao): boolean {
  const instanteExpiracao = Date.parse(sessao.expiraEmUtc);

  if (Number.isNaN(instanteExpiracao)) {
    return true;
  }

  return Date.now() >= instanteExpiracao;
}
