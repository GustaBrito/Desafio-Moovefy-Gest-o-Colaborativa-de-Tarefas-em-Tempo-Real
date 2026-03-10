interface PropriedadesCartaoMetrica {
  titulo: string;
  valor: string;
  subtitulo?: string;
  variacao?: number;
  tipoDestaque?: "neutro" | "alerta" | "sucesso";
}

export function CartaoMetrica({
  titulo,
  valor,
  subtitulo,
  variacao,
  tipoDestaque = "neutro",
}: PropriedadesCartaoMetrica): JSX.Element {
  const possuiVariacao = typeof variacao === "number" && !Number.isNaN(variacao);
  const prefixoVariacao = (variacao ?? 0) > 0 ? "+" : "";

  return (
    <article className={`cartao-metrica cartao-metrica-${tipoDestaque}`}>
      <h3>{titulo}</h3>
      <strong>{valor}</strong>
      {subtitulo && <span className="subtitulo-cartao-metrica">{subtitulo}</span>}
      {possuiVariacao && (
        <span className="variacao-cartao-metrica">
          {prefixoVariacao}
          {variacao}
        </span>
      )}
    </article>
  );
}
