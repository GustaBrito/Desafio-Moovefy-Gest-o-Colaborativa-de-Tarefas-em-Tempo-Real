import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter } from "react-router-dom";
import { ProvedorAutenticacao } from "../contextos/ContextoAutenticacao";
import { RotasAplicacao } from "../rotas/RotasAplicacao";

const clienteConsulta = new QueryClient();

export function Aplicacao(): JSX.Element {
  return (
    <QueryClientProvider client={clienteConsulta}>
      <ProvedorAutenticacao>
        <BrowserRouter>
          <RotasAplicacao />
        </BrowserRouter>
      </ProvedorAutenticacao>
    </QueryClientProvider>
  );
}
