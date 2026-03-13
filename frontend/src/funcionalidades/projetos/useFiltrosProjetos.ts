import { useMemo, useState } from "react";
import type { ProjetoResposta } from "../../tipos/projetos";
import {
  type CriterioOrdenacaoProjeto,
  normalizarTexto,
} from "./utilitariosProjetos";

interface ResultadoUseFiltrosProjetos {
  textoBusca: string;
  criterioOrdenacao: CriterioOrdenacaoProjeto;
  somenteComDescricao: boolean;
  projetosFiltrados: ProjetoResposta[];
  setTextoBusca: (valor: string) => void;
  setCriterioOrdenacao: (valor: CriterioOrdenacaoProjeto) => void;
  setSomenteComDescricao: (valor: boolean) => void;
  limparFiltros: () => void;
}

export function useFiltrosProjetos(
  projetos: ProjetoResposta[]
): ResultadoUseFiltrosProjetos {
  const [textoBusca, setTextoBusca] = useState("");
  const [criterioOrdenacao, setCriterioOrdenacao] =
    useState<CriterioOrdenacaoProjeto>("data_mais_recente");
  const [somenteComDescricao, setSomenteComDescricao] = useState(false);

  const projetosFiltrados = useMemo(() => {
    const textoBuscaNormalizado = normalizarTexto(textoBusca);

    const projetosComFiltros = projetos.filter((projeto) => {
      if (
        somenteComDescricao &&
        (!projeto.descricao || projeto.descricao.trim().length === 0)
      ) {
        return false;
      }

      if (!textoBuscaNormalizado) {
        return true;
      }

      const nomeNormalizado = normalizarTexto(projeto.nome);
      const descricaoNormalizada = normalizarTexto(projeto.descricao ?? "");

      return (
        nomeNormalizado.includes(textoBuscaNormalizado) ||
        descricaoNormalizada.includes(textoBuscaNormalizado)
      );
    });

    return projetosComFiltros.sort((projetoAtual, proximoProjeto) => {
      if (criterioOrdenacao === "nome_ascendente") {
        return projetoAtual.nome.localeCompare(proximoProjeto.nome, "pt-BR");
      }

      if (criterioOrdenacao === "nome_descendente") {
        return proximoProjeto.nome.localeCompare(projetoAtual.nome, "pt-BR");
      }

      if (criterioOrdenacao === "data_mais_antiga") {
        return (
          new Date(projetoAtual.dataCriacao).getTime() -
          new Date(proximoProjeto.dataCriacao).getTime()
        );
      }

      return (
        new Date(proximoProjeto.dataCriacao).getTime() -
        new Date(projetoAtual.dataCriacao).getTime()
      );
    });
  }, [criterioOrdenacao, projetos, somenteComDescricao, textoBusca]);

  function limparFiltros(): void {
    setTextoBusca("");
    setSomenteComDescricao(false);
    setCriterioOrdenacao("data_mais_recente");
  }

  return {
    textoBusca,
    criterioOrdenacao,
    somenteComDescricao,
    projetosFiltrados,
    setTextoBusca,
    setCriterioOrdenacao,
    setSomenteComDescricao,
    limparFiltros,
  };
}
