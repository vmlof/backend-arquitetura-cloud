namespace GestaoRH.Repositories;

public interface IUnitOfWork : IDisposable
{
    IEmpresaRepository     EmpresaRepository     { get; }
    ISetorRepository       SetorRepository       { get; }
    IFuncionarioRepository FuncionarioRepository { get; }
    IModeloRepository      ModeloRepository      { get; }
    IDocumentoRepository   DocumentoRepository   { get; }

    Task CommitAsync();
    void Rollback();
}
