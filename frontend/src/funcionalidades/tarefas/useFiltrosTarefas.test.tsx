import { act, renderHook } from "@testing-library/react";
import { describe, expect, it, beforeEach } from "vitest";
import {
  CampoOrdenacaoTarefa,
  DirecaoOrdenacaoTarefa,
  StatusTarefa,
} from "../../tipos/tarefas";
import { useFiltrosTarefas } from "./useFiltrosTarefas";

describe("useFiltrosTarefas", () => {
  beforeEach(() => {
    window.localStorage.clear();
  });

  it("deve montar filtro de consulta com os valores informados", () => {
    const { result } = renderHook(() => useFiltrosTarefas());

    act(() => {
      result.current.setProjetoIdFiltro("projeto-1");
      result.current.setStatusFiltro(String(StatusTarefa.EmAndamento));
      result.current.setResponsavelUsuarioIdFiltro("usuario-1");
      result.current.setDataPrazoInicialFiltro("2026-03-01");
      result.current.setDataPrazoFinalFiltro("2026-03-31");
      result.current.setCampoOrdenacao(CampoOrdenacaoTarefa.Prioridade);
      result.current.setDirecaoOrdenacao(DirecaoOrdenacaoTarefa.Descendente);
      result.current.setTamanhoPagina(20);
      result.current.setTextoBusca("relatorio");
      result.current.setNumeroPagina(3);
    });

    expect(result.current.filtroConsulta).toMatchObject({
      projetoId: "projeto-1",
      status: StatusTarefa.EmAndamento,
      responsavelUsuarioId: "usuario-1",
      dataPrazoInicial: "2026-03-01",
      dataPrazoFinal: "2026-03-31",
      campoOrdenacao: CampoOrdenacaoTarefa.Prioridade,
      direcaoOrdenacao: DirecaoOrdenacaoTarefa.Descendente,
      numeroPagina: 3,
      tamanhoPagina: 20,
    });
  });

  it("deve alternar chips e limpar filtros para os valores padrao", () => {
    const { result } = renderHook(() => useFiltrosTarefas());

    act(() => {
      result.current.alternarChipRapido("atrasadas");
      result.current.alternarChipRapido("urgentes");
    });

    expect(result.current.chipsAtivos).toEqual(["atrasadas", "urgentes"]);

    act(() => {
      result.current.alternarChipRapido("atrasadas");
    });

    expect(result.current.chipsAtivos).toEqual(["urgentes"]);

    act(() => {
      result.current.limparFiltros();
    });

    expect(result.current.projetoIdFiltro).toBe("");
    expect(result.current.statusFiltro).toBe("");
    expect(result.current.responsavelUsuarioIdFiltro).toBe("");
    expect(result.current.campoOrdenacao).toBe(CampoOrdenacaoTarefa.DataCriacao);
    expect(result.current.direcaoOrdenacao).toBe(DirecaoOrdenacaoTarefa.Ascendente);
    expect(result.current.numeroPagina).toBe(1);
    expect(result.current.tamanhoPagina).toBe(10);
    expect(result.current.textoBusca).toBe("");
    expect(result.current.chipsAtivos).toEqual([]);
  });
});
