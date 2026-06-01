using System.Data;
using Dapper;
using GestaoRH.Models;

namespace GestaoRH.Repositories;

public class EmpresaRepository : IEmpresaRepository
{
    private readonly IDbTransaction _transaction;
    private IDbConnection Connection => _transaction.Connection!;

    public EmpresaRepository(IDbTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task<int> CriarAsync(Empresa empresa)
    {
        const string sql = @"
            INSERT INTO empresa (
                cnpj,
                razao_social,
                endereco,
                telefone,
                logo_base64,
                responsavel_nome,
                responsavel_sobrenome,
                senha,
                ativo,
                criado_em
            ) VALUES (
                @Cnpj,
                @RazaoSocial,
                @Endereco,
                @Telefone,
                @LogoBase64,
                @ResponsavelNome,
                @ResponsavelSobrenome,
                @Senha,
                @Ativo,
                @CriadoEm
            ) RETURNING id";

        return await Connection.ExecuteScalarAsync<int>(sql, empresa, transaction: _transaction);
    }

    public async Task<Empresa?> ObterPorIdAsync(int id)
    {
        const string sql = @"
            SELECT
                id                    AS ""Id"",
                cnpj                  AS ""Cnpj"",
                razao_social          AS ""RazaoSocial"",
                endereco              AS ""Endereco"",
                telefone              AS ""Telefone"",
                logo_base64           AS ""LogoBase64"",
                responsavel_nome      AS ""ResponsavelNome"",
                responsavel_sobrenome AS ""ResponsavelSobrenome"",
                senha                 AS ""Senha"",
                ativo                 AS ""Ativo"",
                criado_em             AS ""CriadoEm""
            FROM empresa
            WHERE id = @Id";

        return await Connection.QueryFirstOrDefaultAsync<Empresa>(
            sql, new { Id = id }, transaction: _transaction);
    }

    public async Task<Empresa?> ObterPorCnpjAsync(string cnpj)
    {
        const string sql = @"
            SELECT
                id                    AS ""Id"",
                cnpj                  AS ""Cnpj"",
                razao_social          AS ""RazaoSocial"",
                endereco              AS ""Endereco"",
                telefone              AS ""Telefone"",
                logo_base64           AS ""LogoBase64"",
                responsavel_nome      AS ""ResponsavelNome"",
                responsavel_sobrenome AS ""ResponsavelSobrenome"",
                senha                 AS ""Senha"",
                ativo                 AS ""Ativo"",
                criado_em             AS ""CriadoEm""
            FROM empresa
            WHERE cnpj = @Cnpj";

        return await Connection.QueryFirstOrDefaultAsync<Empresa>(
            sql, new { Cnpj = cnpj }, transaction: _transaction);
    }

    public async Task AtualizarAsync(Empresa empresa)
    {
        const string sql = @"
            UPDATE empresa
            SET
                razao_social          = @RazaoSocial,
                endereco              = @Endereco,
                telefone              = @Telefone,
                logo_base64           = @LogoBase64,
                responsavel_nome      = @ResponsavelNome,
                responsavel_sobrenome = @ResponsavelSobrenome
            WHERE id = @Id";

        await Connection.ExecuteAsync(sql, empresa, transaction: _transaction);
    }

    public async Task DesativarAsync(int id)
    {
        const string sql = @"UPDATE empresa SET ativo = false WHERE id = @Id";
        await Connection.ExecuteAsync(sql, new { Id = id }, transaction: _transaction);
    }

    public async Task<IEnumerable<Empresa>> ListarAsync()
    {
        const string sql = @"
            SELECT
                id                    AS ""Id"",
                cnpj                  AS ""Cnpj"",
                razao_social          AS ""RazaoSocial"",
                endereco              AS ""Endereco"",
                telefone              AS ""Telefone"",
                logo_base64           AS ""LogoBase64"",
                responsavel_nome      AS ""ResponsavelNome"",
                responsavel_sobrenome AS ""ResponsavelSobrenome"",
                ativo                 AS ""Ativo"",
                criado_em             AS ""CriadoEm""
            FROM empresa
            ORDER BY razao_social";

        return await Connection.QueryAsync<Empresa>(sql, transaction: _transaction);
    }
}
