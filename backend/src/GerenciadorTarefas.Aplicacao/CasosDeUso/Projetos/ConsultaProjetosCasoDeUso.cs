using GerenciadorTarefas.Aplicacao.Contratos.Projetos;
using GerenciadorTarefas.Aplicacao.Modelos.Projetos;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Projetos;

public sealed class ConsultaProjetosCasoDeUso : IConsultaProjetosCasoDeUso
{
    private readonly IRepositorioProjeto repositorioProjeto;

    public ConsultaProjetosCasoDeUso(IRepositorioProjeto repositorioProjeto)
    {
        this.repositorioProjeto = repositorioProjeto;
    }

    public async Task<IReadOnlyCollection<ProjetoResposta>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var projetos = await repositorioProjeto.ListarAsync(cancellationToken);
        return projetos.Select(MapearParaResposta).ToList();
    }

    public async Task<ProjetoResposta> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O identificador do projeto deve ser informado.", nameof(id));
        }

        var projeto = await repositorioProjeto.ObterPorIdAsync(id, cancellationToken);
        if (projeto is null)
        {
            throw new KeyNotFoundException($"Projeto com id '{id}' nao foi encontrado.");
        }

        return MapearParaResposta(projeto);
    }

    private static ProjetoResposta MapearParaResposta(Projeto projeto)
    {
        return new ProjetoResposta
        {
            Id = projeto.Id,
            Nome = projeto.Nome,
            Descricao = projeto.Descricao,
            DataCriacao = projeto.DataCriacao
        };
    }
}
