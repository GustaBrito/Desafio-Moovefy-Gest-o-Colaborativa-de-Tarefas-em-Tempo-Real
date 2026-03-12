namespace GerenciadorTarefas.TestesIntegracao.Infraestrutura;

internal sealed class EnvelopeSucessoRespostaTeste<TDados>
{
    public bool Sucesso { get; init; }
    public string Mensagem { get; init; } = string.Empty;
    public TDados? Dados { get; init; }
    public string? CodigoRastreio { get; init; }
}

internal sealed class EnvelopeErroRespostaTeste
{
    public bool Sucesso { get; init; }
    public int Status { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Mensagem { get; init; } = string.Empty;
    public string? Detalhe { get; init; }
    public string? CodigoRastreio { get; init; }
}

internal sealed class LoginDadosRespostaTeste
{
    public Guid UsuarioId { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int PerfilGlobal { get; init; }
    public IReadOnlyCollection<Guid> AreaIds { get; init; } = [];
    public string TokenAcesso { get; init; } = string.Empty;
    public string TipoToken { get; init; } = string.Empty;
    public DateTime ExpiraEmUtc { get; init; }
}

internal sealed class ProjetoDadosRespostaTeste
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public Guid AreaId { get; init; }
    public string AreaNome { get; init; } = string.Empty;
    public IReadOnlyCollection<Guid> AreaIds { get; init; } = [];
    public IReadOnlyCollection<string> AreasNomes { get; init; } = [];
    public Guid? GestorUsuarioId { get; init; }
    public string? GestorNome { get; init; }
    public IReadOnlyCollection<Guid> UsuarioIdsVinculados { get; init; } = [];
    public IReadOnlyCollection<string> UsuariosNomesVinculados { get; init; } = [];
    public DateTime DataCriacao { get; init; }
}

internal sealed class TarefaDadosRespostaTeste
{
    public Guid Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public int Status { get; init; }
    public int Prioridade { get; init; }
    public Guid ProjetoId { get; init; }
    public Guid ResponsavelUsuarioId { get; init; }
    public string? ResponsavelNome { get; init; }
    public string? ResponsavelEmail { get; init; }
    public string? AreaNome { get; init; }
    public DateTime DataCriacao { get; init; }
    public DateTime DataPrazo { get; init; }
    public DateTime? DataConclusao { get; init; }
    public bool EstaAtrasada { get; init; }
}

internal sealed class ResultadoPaginadoDadosRespostaTeste<TDados>
{
    public IReadOnlyCollection<TDados> Itens { get; init; } = [];
    public int TotalRegistros { get; init; }
    public int NumeroPagina { get; init; }
    public int TamanhoPagina { get; init; }
    public int TotalPaginas { get; init; }
}

internal sealed class MetricasDashboardDadosRespostaTeste
{
    public IReadOnlyCollection<TotalTarefasPorStatusDadosRespostaTeste> TotalTarefasPorStatus { get; init; } = [];
    public int TarefasAtrasadas { get; init; }
    public int TarefasConcluidasNoPrazo { get; init; }
    public decimal TaxaConclusao { get; init; }
}

internal sealed class TotalTarefasPorStatusDadosRespostaTeste
{
    public int Status { get; init; }
    public int Total { get; init; }
}
