import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { PrioridadeTarefa } from "../../tipos/tarefas";
import { FormularioTarefa } from "./FormularioTarefa";

const projetoId = "11111111-1111-1111-1111-111111111111";
const usuarioId = "22222222-2222-2222-2222-222222222222";

function obterDataFutura(dias = 3): string {
  const data = new Date();
  data.setDate(data.getDate() + dias);
  return `${data.getFullYear()}-${String(data.getMonth() + 1).padStart(2, "0")}-${String(
    data.getDate()
  ).padStart(2, "0")}`;
}

describe("FormularioTarefa", () => {
  it("deve enviar criacao de tarefa com dados validos", async () => {
    const usuario = userEvent.setup();
    const aoEnviar = vi.fn().mockResolvedValue(undefined);

    render(
      <FormularioTarefa
        projetos={[
          {
            id: projetoId,
            nome: "Projeto Plataforma",
            areaId: "a",
            areaNome: "Desenvolvimento",
            dataCriacao: new Date().toISOString(),
          },
        ]}
        usuariosResponsaveis={[
          { id: usuarioId, nome: "Colaborador Dev", email: "dev@empresa.com" },
        ]}
        responsavelUsuarioIdPadrao={usuarioId}
        emEnvio={false}
        aoEnviar={aoEnviar}
      />
    );

    await usuario.type(screen.getByLabelText("Titulo"), "Implementar API de tarefas");
    await usuario.type(screen.getByLabelText("Descricao"), "Fluxo completo");
    await usuario.selectOptions(screen.getByLabelText("Projeto"), projetoId);
    await usuario.selectOptions(screen.getByLabelText("Prioridade"), String(PrioridadeTarefa.Alta));
    await usuario.selectOptions(screen.getByLabelText("Responsavel"), usuarioId);
    await usuario.type(screen.getByLabelText("Data de prazo"), obterDataFutura());

    await usuario.click(screen.getByRole("button", { name: "Salvar tarefa" }));

    await waitFor(() => {
      expect(aoEnviar).toHaveBeenCalledTimes(1);
    });

    expect(aoEnviar).toHaveBeenCalledWith(
      expect.objectContaining({
        titulo: "Implementar API de tarefas",
        descricao: "Fluxo completo",
        projetoId,
        prioridade: PrioridadeTarefa.Alta,
        responsavelUsuarioId: usuarioId,
      })
    );
  });

  it("deve validar prazo obrigatorio antes de enviar", async () => {
    const usuario = userEvent.setup();
    const aoEnviar = vi.fn().mockResolvedValue(undefined);

    render(
      <FormularioTarefa
        projetos={[
          {
            id: projetoId,
            nome: "Projeto Plataforma",
            areaId: "a",
            areaNome: "Desenvolvimento",
            dataCriacao: new Date().toISOString(),
          },
        ]}
        usuariosResponsaveis={[
          { id: usuarioId, nome: "Colaborador Dev", email: "dev@empresa.com" },
        ]}
        responsavelUsuarioIdPadrao={usuarioId}
        emEnvio={false}
        aoEnviar={aoEnviar}
      />
    );

    await usuario.type(screen.getByLabelText("Titulo"), "Tarefa com prazo invalido");
    await usuario.selectOptions(screen.getByLabelText("Projeto"), projetoId);
    await usuario.selectOptions(screen.getByLabelText("Responsavel"), usuarioId);

    await usuario.click(screen.getByRole("button", { name: "Salvar tarefa" }));

    expect(
      await screen.findByText("A data de prazo deve ser informada.")
    ).toBeInTheDocument();
    expect(aoEnviar).not.toHaveBeenCalled();
  });

  it("deve carregar dados de edicao e permitir cancelamento", async () => {
    const usuario = userEvent.setup();
    const aoEnviar = vi.fn().mockResolvedValue(undefined);
    const aoCancelar = vi.fn();

    render(
      <FormularioTarefa
        projetos={[
          {
            id: projetoId,
            nome: "Projeto Plataforma",
            areaId: "a",
            areaNome: "Desenvolvimento",
            dataCriacao: new Date().toISOString(),
          },
        ]}
        usuariosResponsaveis={[
          { id: usuarioId, nome: "Colaborador Dev", email: "dev@empresa.com" },
        ]}
        responsavelUsuarioIdPadrao={usuarioId}
        emEnvio={false}
        valoresIniciais={{
          titulo: "Tarefa em edicao",
          descricao: "Descricao atual",
          prioridade: PrioridadeTarefa.Media,
          projetoId,
          responsavelUsuarioId: usuarioId,
          dataPrazo: obterDataFutura(),
        }}
        aoCancelarEdicao={aoCancelar}
        aoEnviar={aoEnviar}
      />
    );

    expect(screen.getByLabelText("Titulo")).toHaveValue("Tarefa em edicao");
    expect(screen.getByLabelText("Descricao")).toHaveValue("Descricao atual");

    await usuario.click(screen.getByRole("button", { name: "Cancelar" }));
    expect(aoCancelar).toHaveBeenCalledTimes(1);
  });
});
