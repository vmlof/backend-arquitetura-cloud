using System.Data;
using Npgsql;

namespace GestaoRH.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly string _connStr;
    private IDbConnection  _connection;
    private IDbTransaction _transaction;
    private bool _disposed;

    private IEmpresaRepository?     _empresaRepository;
    private ISetorRepository?       _setorRepository;
    private IFuncionarioRepository? _funcionarioRepository;
    private IModeloRepository?      _modeloRepository;
    private IDocumentoRepository?   _documentoRepository;

    public UnitOfWork(IConfiguration configuration)
    {
        _connStr = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionString nao configurada.");
        _connection = new NpgsqlConnection(_connStr);
        _connection.Open();
        _transaction = _connection.BeginTransaction();
    }

    public IEmpresaRepository     EmpresaRepository     => _empresaRepository     ??= new EmpresaRepository(_transaction);
    public ISetorRepository       SetorRepository       => _setorRepository       ??= new SetorRepository(_transaction);
    public IFuncionarioRepository FuncionarioRepository => _funcionarioRepository ??= new FuncionarioRepository(_transaction);
    public IModeloRepository      ModeloRepository      => _modeloRepository      ??= new ModeloRepository(_transaction);
    public IDocumentoRepository   DocumentoRepository   => _documentoRepository   ??= new DocumentoRepository(_transaction);

    public async Task CommitAsync()
    {
        try   { await ((NpgsqlTransaction)_transaction).CommitAsync(); }
        catch { await ((NpgsqlTransaction)_transaction).RollbackAsync(); throw; }
        finally
        {
            _transaction.Dispose();
            _empresaRepository = null; _setorRepository = null;
            _funcionarioRepository = null; _modeloRepository = null;
            _documentoRepository = null;
            _transaction = _connection.BeginTransaction();
        }
    }

    public void Rollback()
    {
        try { _transaction.Rollback(); } catch { }
        _transaction.Dispose();
        _empresaRepository = null; _setorRepository = null;
        _funcionarioRepository = null; _modeloRepository = null;
        _documentoRepository = null;
        _transaction = _connection.BeginTransaction();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            try { _transaction?.Rollback(); } catch { }
            _transaction?.Dispose();
            _connection?.Dispose();
            _disposed = true;
        }
    }
}