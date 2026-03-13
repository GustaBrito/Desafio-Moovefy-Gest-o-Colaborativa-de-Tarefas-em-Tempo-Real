import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, type RenderOptions } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import type { PropsWithChildren, ReactElement } from "react";

function criarClienteConsultaTeste(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        gcTime: 0,
      },
      mutations: {
        retry: false,
      },
    },
  });
}

interface OpcoesRenderComProvedores extends Omit<RenderOptions, "wrapper"> {
  rotaInicial?: string;
}

export function renderizarComProvedores(
  elemento: ReactElement,
  opcoes: OpcoesRenderComProvedores = {}
) {
  const { rotaInicial = "/", ...opcoesRender } = opcoes;
  const clienteConsulta = criarClienteConsultaTeste();

  function Wrapper({ children }: PropsWithChildren): JSX.Element {
    return (
      <QueryClientProvider client={clienteConsulta}>
        <MemoryRouter initialEntries={[rotaInicial]}>{children}</MemoryRouter>
      </QueryClientProvider>
    );
  }

  return {
    ...render(elemento, { wrapper: Wrapper, ...opcoesRender }),
    clienteConsulta,
  };
}
