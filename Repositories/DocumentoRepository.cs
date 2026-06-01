using System.Data;
using Dapper;
using GestaoRH.Models;

namespace GestaoRH.Repositories;

public class DocumentoRepository : IDocumentoRepository
{
    private readonly IDbTransaction _transaction;
    private IDbConnection Conn => _transaction.Connection!;

    public DocumentoRepository(IDbTransaction transaction) { _transaction = transaction; }

    public async Task<int> CriarLoteAsync(DocumentoLote l)
    {
        const string sql = @"
            INSERT INTO documento_lote (modelo_id, setor_id, criado_por, total, status, criado_em)
            VALUES (@ModeloId, @SetorId, @CriadoPor, @Total, @Status, @CriadoEm)
            RETURNING id";
        return await Conn.ExecuteScalarAsync<int>(sql, l, transaction: _transaction);
    }

    public async Task<DocumentoLote?> ObterLotePorIdAsync(int id)
    {
        const string sql = @"
            SELECT id AS ""Id"", modelo_id AS ""ModeloId"", setor_id AS ""SetorId"",
                   criado_por AS ""CriadoPor"", total AS ""Total"", status AS ""Status"",
                   criado_em AS ""CriadoEm""
            FROM documento_lote WHERE id = @Id";
        return await Conn.QueryFirstOrDefaultAsync<DocumentoLote>(sql, new { Id = id }, transaction: _transaction);
    }

    // ── Instância ─────────────────────────────────────────────

    public async Task<int> CriarInstanciaAsync(DocumentoInstancia inst)
    {
        const string sql = @"
            INSERT INTO documento_instancia (
                modelo_id, modelo_versao, modelo_nome_snapshot,
                lote_id, funcionario_id, status, conteudo_html, criado_em)
            VALUES (
                @ModeloId, @ModeloVersao, @ModeloNomeSnapshot,
                @LoteId, @FuncionarioId, @Status, @ConteudoHtml, @CriadoEm)
            RETURNING id";
        return await Conn.ExecuteScalarAsync<int>(sql, inst, transaction: _transaction);
    }

    public async Task<DocumentoInstancia?> ObterInstanciaPorIdAsync(int id)
    {
        const string sql = @"
            SELECT
                di.id AS ""Id"", di.modelo_id AS ""ModeloId"",
                di.modelo_versao AS ""ModeloVersao"",
                di.modelo_nome_snapshot AS ""ModeloNomeSnapshot"",
                di.lote_id AS ""LoteId"", di.funcionario_id AS ""FuncionarioId"",
                di.status AS ""Status"", di.conteudo_html AS ""ConteudoHtml"",
                di.pdf_base64 AS ""PdfBase64"",
                di.criado_em AS ""CriadoEm"", di.concluido_em AS ""ConcluidoEm"",
                f.nome AS ""NomeFuncionario"",
                s.nome AS ""NomeSetor"",
                dm.nome AS ""ModeloNome""
            FROM documento_instancia di
            JOIN funcionario f ON f.id = di.funcionario_id
            LEFT JOIN setor s ON s.id = f.setor_id
            LEFT JOIN documento_modelo dm ON dm.id = di.modelo_id
            WHERE di.id = @Id";

        var inst = await Conn.QueryFirstOrDefaultAsync<DocumentoInstancia>(sql, new { Id = id }, transaction: _transaction);
        if (inst == null) return null;

        inst.Assinaturas = (await ListarAssinaturasPorInstanciaAsync(id)).ToList();
        inst.Variaveis   = (await ListarVariaveisPorInstanciaAsync(id)).ToList();
        return inst;
    }

    public async Task<IEnumerable<DocumentoInstancia>> ListarInstanciasPorFuncionarioAsync(int funcionarioId)
    {
        const string sql = @"
            SELECT
                di.id AS ""Id"", di.modelo_id AS ""ModeloId"",
                di.modelo_nome_snapshot AS ""ModeloNomeSnapshot"",
                di.lote_id AS ""LoteId"", di.funcionario_id AS ""FuncionarioId"",
                di.status AS ""Status"", di.criado_em AS ""CriadoEm"",
                di.concluido_em AS ""ConcluidoEm"",
                f.nome AS ""NomeFuncionario"",
                s.nome AS ""NomeSetor"",
                dm.nome AS ""ModeloNome""
            FROM documento_instancia di
            JOIN funcionario f ON f.id = di.funcionario_id
            LEFT JOIN setor s ON s.id = f.setor_id
            LEFT JOIN documento_modelo dm ON dm.id = di.modelo_id
            WHERE di.funcionario_id = @FuncionarioId
            ORDER BY di.criado_em DESC";
        return await Conn.QueryAsync<DocumentoInstancia>(sql, new { FuncionarioId = funcionarioId }, transaction: _transaction);
    }

    public async Task<IEnumerable<DocumentoInstancia>> ListarInstanciasPorSetorAsync(int setorId)
    {
        const string sql = @"
            SELECT
                di.id AS ""Id"", di.modelo_id AS ""ModeloId"",
                di.modelo_nome_snapshot AS ""ModeloNomeSnapshot"",
                di.lote_id AS ""LoteId"", di.funcionario_id AS ""FuncionarioId"",
                di.status AS ""Status"", di.criado_em AS ""CriadoEm"",
                di.concluido_em AS ""ConcluidoEm"",
                f.nome AS ""NomeFuncionario"",
                s.nome AS ""NomeSetor"",
                dm.nome AS ""ModeloNome""
            FROM documento_instancia di
            JOIN funcionario f ON f.id = di.funcionario_id
            LEFT JOIN setor s ON s.id = f.setor_id
            LEFT JOIN documento_modelo dm ON dm.id = di.modelo_id
            WHERE f.setor_id = @SetorId
            ORDER BY di.criado_em DESC";
        return await Conn.QueryAsync<DocumentoInstancia>(sql, new { SetorId = setorId }, transaction: _transaction);
    }

    public async Task<IEnumerable<DocumentoInstancia>> ListarTodasInstanciasAsync()
    {
        const string sql = @"
            SELECT
                di.id AS ""Id"", di.modelo_id AS ""ModeloId"",
                di.modelo_nome_snapshot AS ""ModeloNomeSnapshot"",
                di.lote_id AS ""LoteId"", di.funcionario_id AS ""FuncionarioId"",
                di.status AS ""Status"", di.criado_em AS ""CriadoEm"",
                di.concluido_em AS ""ConcluidoEm"",
                f.nome AS ""NomeFuncionario"",
                s.nome AS ""NomeSetor"",
                dm.nome AS ""ModeloNome""
            FROM documento_instancia di
            JOIN funcionario f ON f.id = di.funcionario_id
            LEFT JOIN setor s ON s.id = f.setor_id
            LEFT JOIN documento_modelo dm ON dm.id = di.modelo_id
            ORDER BY di.criado_em DESC";
        return await Conn.QueryAsync<DocumentoInstancia>(sql, transaction: _transaction);
    }

    public async Task AtualizarStatusInstanciaAsync(int id, string status, DateTime? concluidoEm = null)
    {
        await Conn.ExecuteAsync(
            "UPDATE documento_instancia SET status=@Status, concluido_em=@ConcluidoEm WHERE id=@Id",
            new { Id = id, Status = status, ConcluidoEm = concluidoEm }, transaction: _transaction);
    }

    public async Task SalvarPdfAsync(int id, string pdfBase64)
    {
        await Conn.ExecuteAsync(
            "UPDATE documento_instancia SET pdf_base64=@PdfBase64 WHERE id=@Id",
            new { Id = id, PdfBase64 = pdfBase64 }, transaction: _transaction);
    }

    // ── Variáveis ─────────────────────────────────────────────

    public async Task InserirVariavelAsync(DocumentoVariavelValor v)
    {
        const string sql = @"
            INSERT INTO documento_variavel_valor (instancia_id, token, valor)
            VALUES (@InstanciaId, @Token, @Valor)";
        await Conn.ExecuteAsync(sql, v, transaction: _transaction);
    }

    private async Task<IEnumerable<DocumentoVariavelValor>> ListarVariaveisPorInstanciaAsync(int instanciaId)
    {
        const string sql = @"
            SELECT id AS ""Id"", instancia_id AS ""InstanciaId"",
                   token AS ""Token"", valor AS ""Valor""
            FROM documento_variavel_valor WHERE instancia_id = @InstanciaId";
        return await Conn.QueryAsync<DocumentoVariavelValor>(sql, new { InstanciaId = instanciaId }, transaction: _transaction);
    }

    // ── Assinaturas ───────────────────────────────────────────

    public async Task<int> CriarAssinaturaAsync(DocumentoAssinatura a)
    {
        const string sql = @"
            INSERT INTO documento_assinatura (
                instancia_id, papel, signer_id, signer_tipo,
                signer_nome_snapshot, signer_email_snapshot,
                status, obrigatorio, ordem, criado_em)
            VALUES (
                @InstanciaId, @Papel, @SignerId, @SignerTipo,
                @SignerNomeSnapshot, @SignerEmailSnapshot,
                @Status, @Obrigatorio, @Ordem, @CriadoEm)
            RETURNING id";
        return await Conn.ExecuteScalarAsync<int>(sql, a, transaction: _transaction);
    }

    public async Task<DocumentoAssinatura?> ObterAssinaturaPorIdAsync(int id)
    {
        const string sql = @"
            SELECT id AS ""Id"", instancia_id AS ""InstanciaId"", papel AS ""Papel"",
                   signer_id AS ""SignerId"", signer_tipo AS ""SignerTipo"",
                   signer_nome_snapshot AS ""SignerNomeSnapshot"",
                   signer_email_snapshot AS ""SignerEmailSnapshot"",
                   status AS ""Status"", obrigatorio AS ""Obrigatorio"",
                   ordem AS ""Ordem"", assinatura_base64 AS ""AssinaturaBase64"",
                   assinado_em AS ""AssinadoEm"", ip_address AS ""IpAddress"",
                   criado_em AS ""CriadoEm""
            FROM documento_assinatura WHERE id = @Id";
        return await Conn.QueryFirstOrDefaultAsync<DocumentoAssinatura>(sql, new { Id = id }, transaction: _transaction);
    }

    public async Task<IEnumerable<DocumentoAssinatura>> ListarAssinaturasPorInstanciaAsync(int instanciaId)
    {
        // FIX: assinatura_base64 estava ausente nesta query — sem ela o PDF gerava com a área de
        // assinatura vazia mesmo após o funcionário assinar. Campo adicionado abaixo.
        const string sql = @"
            SELECT id AS ""Id"", instancia_id AS ""InstanciaId"", papel AS ""Papel"",
                   signer_id AS ""SignerId"", signer_tipo AS ""SignerTipo"",
                   signer_nome_snapshot AS ""SignerNomeSnapshot"",
                   signer_email_snapshot AS ""SignerEmailSnapshot"",
                   status AS ""Status"", obrigatorio AS ""Obrigatorio"",
                   ordem AS ""Ordem"",
                   assinatura_base64 AS ""AssinaturaBase64"",
                   assinado_em AS ""AssinadoEm"",
                   ip_address AS ""IpAddress"", criado_em AS ""CriadoEm""
            FROM documento_assinatura
            WHERE instancia_id = @InstanciaId
            ORDER BY ordem";
        return await Conn.QueryAsync<DocumentoAssinatura>(sql, new { InstanciaId = instanciaId }, transaction: _transaction);
    }

    public async Task<IEnumerable<DocumentoAssinatura>> ListarAssinaturasPendentesPorSignerAsync(int signerId, string signerTipo)
    {
        const string sql = @"
            SELECT da.id AS ""Id"", da.instancia_id AS ""InstanciaId"",
                   da.papel AS ""Papel"", da.signer_id AS ""SignerId"",
                   da.signer_tipo AS ""SignerTipo"",
                   da.signer_nome_snapshot AS ""SignerNomeSnapshot"",
                   da.status AS ""Status"", da.obrigatorio AS ""Obrigatorio"",
                   da.ordem AS ""Ordem"", da.criado_em AS ""CriadoEm""
            FROM documento_assinatura da
            WHERE da.signer_id = @SignerId
              AND da.signer_tipo = @SignerTipo
              AND da.status = 'pendente'
            ORDER BY da.criado_em DESC";
        return await Conn.QueryAsync<DocumentoAssinatura>(
            sql, new { SignerId = signerId, SignerTipo = signerTipo }, transaction: _transaction);
    }

    public async Task RegistrarAssinaturaAsync(int id, string base64, DateTime assinadoEm, string? ip)
    {
        await Conn.ExecuteAsync(@"
            UPDATE documento_assinatura
            SET status='assinado', assinatura_base64=@Base64,
                assinado_em=@AssinadoEm, ip_address=@Ip
            WHERE id = @Id",
            new { Id = id, Base64 = base64, AssinadoEm = assinadoEm, Ip = ip },
            transaction: _transaction);
    }

    // ── Assinatura no perfil ──────────────────────────────────

    public async Task<string?> ObterAssinaturaPerfilAsync(int funcionarioId)
    {
        return await Conn.ExecuteScalarAsync<string?>(
            "SELECT assinatura_base64 FROM funcionario WHERE id = @Id",
            new { Id = funcionarioId }, transaction: _transaction);
    }

    public async Task SalvarAssinaturaPerfilAsync(int funcionarioId, string base64)
    {
        await Conn.ExecuteAsync(
            "UPDATE funcionario SET assinatura_base64 = @Base64 WHERE id = @Id",
            new { Id = funcionarioId, Base64 = base64 }, transaction: _transaction);
    }
}
