interface PropriedadesCartaoMetrica {
  titulo: string;
  valor: string;
}

export function CartaoMetrica({
  titulo,
  valor,
}: PropriedadesCartaoMetrica): JSX.Element {
  return (
    <article className="cartao-metrica">
      <h3>{titulo}</h3>
      <strong>{valor}</strong>
    </article>
  );
}
