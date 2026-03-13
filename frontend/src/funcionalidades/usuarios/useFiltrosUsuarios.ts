import { useMemo, useState } from "react";
import type { UsuarioResposta } from "../../tipos/usuarios";
import { normalizarTexto } from "./utilitariosUsuarios";

interface ResultadoUseFiltrosUsuarios {
  filtroNome: string;
  filtroAreaId: string;
  filtroPerfilGlobal: string;
  usuariosFiltrados: UsuarioResposta[];
  setFiltroNome: (valor: string) => void;
  setFiltroAreaId: (valor: string) => void;
  setFiltroPerfilGlobal: (valor: string) => void;
  limparFiltrosPesquisa: () => void;
}

export function useFiltrosUsuarios(
  usuariosOrdenados: UsuarioResposta[]
): ResultadoUseFiltrosUsuarios {
  const [filtroNome, setFiltroNome] = useState("");
  const [filtroAreaId, setFiltroAreaId] = useState("");
  const [filtroPerfilGlobal, setFiltroPerfilGlobal] = useState("");

  const usuariosFiltrados = useMemo(() => {
    const termoNome = normalizarTexto(filtroNome);

    return usuariosOrdenados.filter((usuario) => {
      if (termoNome && !normalizarTexto(usuario.nome).includes(termoNome)) {
        return false;
      }

      if (filtroAreaId && !usuario.areaIds.includes(filtroAreaId)) {
        return false;
      }

      if (
        filtroPerfilGlobal &&
        usuario.perfilGlobal !== Number(filtroPerfilGlobal)
      ) {
        return false;
      }

      return true;
    });
  }, [filtroAreaId, filtroNome, filtroPerfilGlobal, usuariosOrdenados]);

  function limparFiltrosPesquisa(): void {
    setFiltroNome("");
    setFiltroAreaId("");
    setFiltroPerfilGlobal("");
  }

  return {
    filtroNome,
    filtroAreaId,
    filtroPerfilGlobal,
    usuariosFiltrados,
    setFiltroNome,
    setFiltroAreaId,
    setFiltroPerfilGlobal,
    limparFiltrosPesquisa,
  };
}
