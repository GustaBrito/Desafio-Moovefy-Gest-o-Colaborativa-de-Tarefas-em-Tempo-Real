export function normalizarTexto(texto: string): string {
  return texto
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .trim()
    .toLowerCase();
}

export function obterMensagemErro(
  excecao: unknown,
  mensagemPadrao: string
): string {
  if (excecao instanceof Error && excecao.message.trim().length > 0) {
    return excecao.message;
  }

  return mensagemPadrao;
}
