import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { Aplicacao } from "./aplicacao/Aplicacao";
import "./estilos/globais.css";

const elementoRaiz = document.getElementById("raiz");

if (!elementoRaiz) {
  throw new Error("Elemento raiz nao encontrado.");
}

createRoot(elementoRaiz).render(
  <StrictMode>
    <Aplicacao />
  </StrictMode>
);
