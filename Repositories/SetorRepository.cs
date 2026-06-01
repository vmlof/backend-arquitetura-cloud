using System.Data;
using Dapper;
using GestaoRH.Models;

namespace GestaoRH.Repositories;

public class SetorRepository : ISetorRepository
{
    private readonly IDbTransaction _transaction;
    private IDbConnection Connection => _transaction.Connection!;

    public SetorRepository(IDbTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task<int> CriarAsync(Setor setor)
    {
        const string sql = @"
            INSERT INTO setor (nome, descricao, ativo, criado_em)
            VALUES (@Nome, @Descricao, @Ativo, @CriadoEm)
            RETURNING id";

        return await Connection.ExecuteScalarAsync<int>(sql, setor, transaction: _transaction);
    }

    public async Task<Setor?> ObterPorIdAsync(int id)
    {
        const string sql = @"
            SELECT id AS ""Id"", nome AS ""Nome"", descricao AS ""Descricao"",
                   ativo AS ""Ativo"", criado_em AS ""CriadoEm""
            FROM setor WHERE id = @Id";

        return await Connection.QueryFirstOrDefaultAsync<Setor>(
            sql, new { Id = id }, transaction: _transaction);
    }

    // Busca setor ativo pelo nome (para validar duplicidade apenas entre ativos)
    public async Task<Setor?> ObterPorNomeAtivoAsync(string nome)
    {
        const string sql = @"
            SELECT id AS ""Id"", nome AS ""Nome"", descricao AS ""Descricao"",
                   ativo AS ""Ativo"", criado_em AS ""CriadoEm""
            FROM setor
            WHERE LOWER(nome) = LOWER(@Nome) AND ativo = true";

        return await Connection.QueryFirstOrDefaultAsync<Setor>(
            sql, new { Nome = nome }, transaction: _transaction);
    }

    public async Task AtualizarAsync(Setor setor)
    {
        const string sql = @"
            UPDATE setor
            SET nome = @Nome, descricao = @Descricao, ativo = @Ativo
            WHERE id = @Id";

        await Connection.ExecuteAsync(sql, setor, transaction: _transaction);
    }

    public async Task DesativarAsync(int id)
    {
        const string sql = @"UPDATE setor SET ativo = false WHERE id = @Id";
        await Connection.ExecuteAsync(sql, new { Id = id }, transaction: _transaction);
    }

    // Lista apenas ativos (para selects do front)
    public async Task<IEnumerable<Setor>> ListarAsync()
    {
        const string sql = @"
            SELECT id AS ""Id"", nome AS ""Nome"", descricao AS ""Descricao"",
                   ativo AS ""Ativo"", criado_em AS ""CriadoEm""
            FROM setor
            WHERE ativo = true
            ORDER BY nome";

        return await Connection.QueryAsync<Setor>(sql, transaction: _transaction);
    }

    // Lista todos (ativos e inativos) — para a tela de gestão
    public async Task<IEnumerable<Setor>> ListarTodosAsync()
    {
        const string sql = @"
            SELECT id AS ""Id"", nome AS ""Nome"", descricao AS ""Descricao"",
                   ativo AS ""Ativo"", criado_em AS ""CriadoEm""
            FROM setor
            ORDER BY ativo DESC, nome";

        return await Connection.QueryAsync<Setor>(sql, transaction: _transaction);
    }
}
