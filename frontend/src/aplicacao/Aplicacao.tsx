import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter } from "react-router-dom";
import { ListaNotificacoesToast } from "../componentes/ListaNotificacoesToast";
import { ProvedorAutenticacao } from "../contextos/ContextoAutenticacao";
import { ProvedorNotificacao } from "../contextos/ContextoNotificacao";
import { RotasAplicacao } from "../rotas/RotasAplicacao";

const clienteConsulta = new QueryClient();

export function Aplicacao(): JSX.Element {
  return (
    <QueryClientProvider client={clienteConsulta}>
      <ProvedorAutenticacao>
        <ProvedorNotificacao>
          <BrowserRouter>
            <RotasAplicacao />
          </BrowserRouter>
          <ListaNotificacoesToast />
        </ProvedorNotificacao>
      </ProvedorAutenticacao>
    </QueryClientProvider>
  );
}
