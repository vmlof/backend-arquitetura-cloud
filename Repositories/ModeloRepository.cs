using System.Data;
using Dapper;
using GestaoRH.Models;

namespace GestaoRH.Repositories;

public class ModeloRepository : IModeloRepository
{
    private readonly IDbTransaction _transaction;
    private IDbConnection Conn => _transaction.Connection!;

    public ModeloRepository(IDbTransaction transaction) { _transaction = transaction; }

    // ── Modelo ───────────────────────────────────────────────

    public async Task<int> CriarAsync(DocumentoModelo m)
    {
        const string sql = @"
            INSERT INTO documento_modelo (nome, descricao, categoria, tipo_uso, status, versao, criado_em, atualizado_em)
            VALUES (@Nome, @Descricao, @Categoria, @TipoUso, @Status, @Versao, @CriadoEm, @AtualizadoEm)
            RETURNING id";
        return await Conn.ExecuteScalarAsync<int>(sql, m, transaction: _transaction);
    }

    public async Task<DocumentoModelo?> ObterPorIdAsync(int id)
    {
        // Busca o modelo
        const string sqlModelo = @"
            SELECT id AS ""Id"", nome AS ""Nome"", descricao AS ""Descricao"",
                   categoria AS ""Categoria"", tipo_uso AS ""TipoUso"", status AS ""Status"",
                   versao AS ""Versao"", criado_em AS ""CriadoEm"", atualizado_em AS ""AtualizadoEm""
            FROM documento_modelo WHERE id = @Id";

        var modelo = await Conn.QueryFirstOrDefaultAsync<DocumentoModelo>(
            sqlModelo, new { Id = id }, transaction: _transaction);
        if (modelo == null) return null;

        // Busca seções ordenadas
        const string sqlSecoes = @"
            SELECT id AS ""Id"", modelo_id AS ""ModeloId"", titulo AS ""Titulo"",
                   tipo AS ""Tipo"", conteudo AS ""Conteudo"", ordem AS ""Ordem""
            FROM documento_modelo_secao WHERE modelo_id = @ModeloId ORDER BY ordem";

        var secoes = (await Conn.QueryAsync<DocumentoModeloSecao>(
            sqlSecoes, new { ModeloId = id }, transaction: _transaction)).ToList();

        // Busca campos de todas as seções de uma vez
        if (secoes.Count > 0)
        {
            var secaoIds = secoes.Select(s => s.Id).ToArray();
            const string sqlCampos = @"
                SELECT id AS ""Id"", secao_id AS ""SecaoId"", label AS ""Label"",
                       tipo_campo AS ""TipoCampo"", obrigatorio AS ""Obrigatorio"",
                       ordem AS ""Ordem"", config_json AS ""ConfigJson""
                FROM documento_modelo_campo
                WHERE secao_id = ANY(@SecaoIds) ORDER BY secao_id, ordem";

            var campos = await Conn.QueryAsync<DocumentoModeloCampo>(
                sqlCampos, new { SecaoIds = secaoIds }, transaction: _transaction);

            var camposPorSecao = campos.GroupBy(c => c.SecaoId)
                                       .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var sec in secoes)
                sec.Campos = camposPorSecao.TryGetValue(sec.Id, out var cs) ? cs : [];
        }

        modelo.Secoes = secoes;

        // Busca assinantes
        const string sqlAssin = @"
            SELECT id AS ""Id"", modelo_id AS ""ModeloId"", papel AS ""Papel"",
                   rotulo AS ""Rotulo"", obrigatorio AS ""Obrigatorio"",
                   ordem AS ""Ordem"", exibir_pdf AS ""ExibirPdf""
            FROM documento_modelo_assinante WHERE modelo_id = @ModeloId ORDER BY ordem";

        modelo.Assinantes = (await Conn.QueryAsync<DocumentoModeloAssinante>(
            sqlAssin, new { ModeloId = id }, transaction: _transaction)).ToList();

        return modelo;
    }

    public async Task<IEnumerable<DocumentoModelo>> ListarAsync()
    {
        // Listagem simples sem seções/campos (para a tabela)
        const string sql = @"
            SELECT id AS ""Id"", nome AS ""Nome"", descricao AS ""Descricao"",
                   categoria AS ""Categoria"", tipo_uso AS ""TipoUso"", status AS ""Status"",
                   versao AS ""Versao"", criado_em AS ""CriadoEm"", atualizado_em AS ""AtualizadoEm""
            FROM documento_modelo
            ORDER BY criado_em DESC";
        return await Conn.QueryAsync<DocumentoModelo>(sql, transaction: _transaction);
    }

    public async Task AtualizarAsync(DocumentoModelo m)
    {
        const string sql = @"
            UPDATE documento_modelo
            SET nome=@Nome, descricao=@Descricao, categoria=@Categoria,
                tipo_uso=@TipoUso, atualizado_em=@AtualizadoEm
            WHERE id = @Id";
        await Conn.ExecuteAsync(sql, m, transaction: _transaction);
    }

    public async Task PublicarAsync(int id)
    {
        const string sql = @"
            UPDATE documento_modelo
            SET status='publicado', atualizado_em=NOW()
            WHERE id = @Id";
        await Conn.ExecuteAsync(sql, new { Id = id }, transaction: _transaction);
    }

    public async Task ArquivarAsync(int id)
    {
        const string sql = @"
            UPDATE documento_modelo
            SET status='arquivado', atualizado_em=NOW()
            WHERE id = @Id";
        await Conn.ExecuteAsync(sql, new { Id = id }, transaction: _transaction);
    }

    // ── Seções ────────────────────────────────────────────────

    public async Task<int> CriarSecaoAsync(DocumentoModeloSecao s)
    {
        const string sql = @"
            INSERT INTO documento_modelo_secao (modelo_id, titulo, tipo, conteudo, ordem)
            VALUES (@ModeloId, @Titulo, @Tipo, @Conteudo, @Ordem)
            RETURNING id";
        return await Conn.ExecuteScalarAsync<int>(sql, s, transaction: _transaction);
    }

    public async Task DeletarSecoesPorModeloAsync(int modeloId)
    {
        // CASCADE deleta campos automaticamente
        await Conn.ExecuteAsync(
            "DELETE FROM documento_modelo_secao WHERE modelo_id = @ModeloId",
            new { ModeloId = modeloId }, transaction: _transaction);
    }

    // ── Campos ────────────────────────────────────────────────

    public async Task<int> CriarCampoAsync(DocumentoModeloCampo c)
    {
        const string sql = @"
            INSERT INTO documento_modelo_campo (secao_id, label, tipo_campo, obrigatorio, ordem, config_json)
            VALUES (@SecaoId, @Label, @TipoCampo, @Obrigatorio, @Ordem, @ConfigJson::jsonb)
            RETURNING id";
        return await Conn.ExecuteScalarAsync<int>(sql, c, transaction: _transaction);
    }

    public async Task DeletarCamposPorSecaoAsync(int secaoId)
    {
        await Conn.ExecuteAsync(
            "DELETE FROM documento_modelo_campo WHERE secao_id = @SecaoId",
            new { SecaoId = secaoId }, transaction: _transaction);
    }

    // ── Assinantes ────────────────────────────────────────────

    public async Task<int> CriarAssinanteAsync(DocumentoModeloAssinante a)
    {
        const string sql = @"
            INSERT INTO documento_modelo_assinante (modelo_id, papel, rotulo, obrigatorio, ordem, exibir_pdf)
            VALUES (@ModeloId, @Papel, @Rotulo, @Obrigatorio, @Ordem, @ExibirPdf)
            RETURNING id";
        return await Conn.ExecuteScalarAsync<int>(sql, a, transaction: _transaction);
    }

    public async Task DeletarAssinantesPorModeloAsync(int modeloId)
    {
        await Conn.ExecuteAsync(
            "DELETE FROM documento_modelo_assinante WHERE modelo_id = @ModeloId",
            new { ModeloId = modeloId }, transaction: _transaction);
    }
}
