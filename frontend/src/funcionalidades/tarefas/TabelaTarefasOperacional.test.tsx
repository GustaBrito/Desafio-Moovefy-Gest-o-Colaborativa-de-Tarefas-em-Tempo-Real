import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import {
  CampoOrdenacaoTarefa,
  DirecaoOrdenacaoTarefa,
  PrioridadeTarefa,
  StatusTarefa,
  type TarefaResposta,
} from "../../tipos/tarefas";
import { TabelaTarefasOperacional } from "./TabelaTarefasOperacional";

const tarefaBase: TarefaResposta = {
  id: "tarefa-1",
  titulo: "Ajustar pipeline",
  descricao: "Executar validacao CI",
  status: StatusTarefa.Pendente,
  prioridade: PrioridadeTarefa.Alta,
  projetoId: "proj-1",
  responsavelUsuarioId: "user-1",
  responsavelNome: "Dev Fullstack",
  responsavelEmail: "dev@empresa.com",
  areaNome: "Desenvolvimento",
  dataCriacao: "2026-03-10T10:00:00Z",
  dataPrazo: "2026-03-20T10:00:00Z",
  dataConclusao: null,
  estaAtrasada: false,
};

describe("TabelaTarefasOperacional", () => {
  it("deve exibir listagem e acionar callbacks de interacao", async () => {
    const usuario = userEvent.setup();
    const aoOrdenar = vi.fn();
    const aoAlterarStatus = vi.fn();
    const aoEditar = vi.fn();
    const aoExcluir = vi.fn();
    const aoAlternarSelecaoTodas = vi.fn();
    const aoAlternarSelecao = vi.fn();

    render(
      <TabelaTarefasOperacional
        tarefas={[tarefaBase]}
        idsSelecionados={[]}
        todasVisiveisSelecionadas={false}
        mapaProjetos={new Map([["proj-1", "Projeto Portal"]])}
        carregandoAtualizacaoStatus={false}
        carregandoEdicao={false}
        carregandoExclusao={false}
        campoOrdenacao={CampoOrdenacaoTarefa.DataCriacao}
        direcaoOrdenacao={DirecaoOrdenacaoTarefa.Ascendente}
        obterStatusPermitidos={() => [
          StatusTarefa.Pendente,
          StatusTarefa.EmAndamento,
          StatusTarefa.Cancelada,
        ]}
        aoAlternarSelecaoTodas={aoAlternarSelecaoTodas}
        aoAlternarSelecao={aoAlternarSelecao}
        aoAlterarStatus={aoAlterarStatus}
        aoEditar={aoEditar}
        aoExcluir={aoExcluir}
        aoOrdenar={aoOrdenar}
      />
    );

    expect(screen.getByText("Ajustar pipeline")).toBeInTheDocument();
    expect(screen.getByText("Projeto Portal")).toBeInTheDocument();
    expect(screen.getByText("Dev Fullstack")).toBeInTheDocument();

    await usuario.click(screen.getByRole("button", { name: /Titulo/i }));
    expect(aoOrdenar).toHaveBeenCalledWith(CampoOrdenacaoTarefa.Titulo);

    const linha = screen.getByText("Ajustar pipeline").closest("tr");
    expect(linha).not.toBeNull();

    const seletorStatus = within(linha as HTMLElement).getByRole("combobox");
    await usuario.selectOptions(seletorStatus, String(StatusTarefa.EmAndamento));
    expect(aoAlterarStatus).toHaveBeenCalledWith(
      tarefaBase,
      StatusTarefa.EmAndamento
    );

    await usuario.click(screen.getByRole("button", { name: /Editar tarefa Ajustar pipeline/i }));
    await usuario.click(screen.getByRole("button", { name: /Excluir tarefa Ajustar pipeline/i }));

    expect(aoEditar).toHaveBeenCalledWith(tarefaBase);
    expect(aoExcluir).toHaveBeenCalledWith(tarefaBase);
  });
});
