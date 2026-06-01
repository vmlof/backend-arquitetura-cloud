using System.Security.Claims;
using GestaoRH.Models.DTOs;
using GestaoRH.Repositories;
using GestaoRH.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestaoRH.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentoController : ControllerBase
{
    private readonly DocumentoService _documentoService;
    private readonly PdfService       _pdfService;
    private readonly IUnitOfWork      _uof;

    public DocumentoController(DocumentoService documentoService, PdfService pdfService, IUnitOfWork uof)
    {
        _documentoService = documentoService;
        _pdfService       = pdfService;
        _uof              = uof;
    }

    private ClaimsPrincipal? Claims => HttpContext.Items["Claims"] as ClaimsPrincipal;

    private (int id, string tipo, string perfil) GetSignerInfo()
    {
        var claims = Claims ?? throw new UnauthorizedAccessException("Nao autenticado.");
        var id     = int.Parse(claims.FindFirstValue("Id") ?? "0");
        var perfil = claims.FindFirstValue("Perfil") ?? string.Empty;
        var tipo   = perfil == "empresa" ? "empresa" : "funcionario";
        return (id, tipo, perfil);
    }

    [HttpPost("gerar")]
    public async Task<IActionResult> Gerar([FromBody] GerarDocumentoDto dto)
    {
        try
        {
            var (empresaId, _, perfil) = GetSignerInfo();
            if (perfil != "empresa") return Forbid();
            if (dto is null) return BadRequest("Body vazio.");

            if (dto.SetorId.HasValue && dto.SetorId > 0)
            {
                var lote = await _documentoService.GerarLote(dto, empresaId);
                await _uof.CommitAsync();
                return Ok(new { loteId = lote.Id, total = lote.Total, mensagem = $"{lote.Total} documento(s) gerado(s)." });
            }
            else
            {
                var inst = await _documentoService.GerarIndividual(dto, empresaId);
                await _uof.CommitAsync();
                return CreatedAtAction(nameof(ObterPorId), new { id = inst.Id },
                    DocumentoService.ToDetalheDto(inst, empresaId, "empresa"));
            }
        }
        catch (KeyNotFoundException ex)      { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (ArgumentException ex)         { return BadRequest(ex.Message); }
        catch (UnauthorizedAccessException)  { return Forbid(); }
        catch (Exception ex)                 { return StatusCode(500, ex.Message); }
    }

    [HttpGet]
    public async Task<IActionResult> ListarTodos()
    {
        try
        {
            var (_, _, perfil) = GetSignerInfo();
            if (perfil != "empresa") return Forbid();

            var lista = await _documentoService.ListarTodos();
            return Ok(lista.Select(i => DocumentoService.ToListagemDto(i)));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception ex)                { return StatusCode(500, ex.Message); }
    }

    [HttpGet("meus")]
    public async Task<IActionResult> MeusDocumentos()
    {
        try
        {
            var (signerId, signerTipo, perfil) = GetSignerInfo();
            if (perfil == "empresa") return Forbid();

            var lista = await _documentoService.ListarPorFuncionario(signerId);
            return Ok(lista.Select(i => DocumentoService.ToListagemDto(i)));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception ex)                { return StatusCode(500, ex.Message); }
    }

    [HttpGet("setor/{setorId:int}")]
    public async Task<IActionResult> ListarPorSetor(int setorId)
    {
        try
        {
            var (signerId, _, perfil) = GetSignerInfo();
            if (perfil != "chefe" && perfil != "empresa") return Forbid();

            if (perfil == "chefe")
            {
                var func = await _uof.FuncionarioRepository.ObterPorIdAsync(signerId);
                if (func?.SetorId != setorId) return Forbid();
            }

            var lista = await _documentoService.ListarPorSetor(setorId);
            return Ok(lista.Select(i => DocumentoService.ToListagemDto(i)));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception ex)                { return StatusCode(500, ex.Message); }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        try
        {
            var (signerId, signerTipo, perfil) = GetSignerInfo();
            var inst = await _documentoService.ObterPorId(id);

            if (perfil == "funcionario" && inst.FuncionarioId != signerId)
                return Forbid();

            if (perfil == "chefe")
            {
                var func    = await _uof.FuncionarioRepository.ObterPorIdAsync(signerId);
                var funcDoc = await _uof.FuncionarioRepository.ObterPorIdAsync(inst.FuncionarioId);
                if (func?.SetorId != funcDoc?.SetorId) return Forbid();
            }

            var conteudoHtml = inst.Status == "concluido"
                ? DocumentoService.InjetarAssinaturasNoHtml(inst.ConteudoHtml, inst.Assinaturas)
                : inst.ConteudoHtml;

            return Ok(DocumentoService.ToDetalheDto(inst, signerId, signerTipo, conteudoHtml));
        }
        catch (KeyNotFoundException ex)     { return NotFound(ex.Message); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception ex)                { return StatusCode(500, ex.Message); }
    }

    [HttpPost("{instanciaId:int}/assinar/{assinaturaId:int}")]
    public async Task<IActionResult> Assinar(int instanciaId, int assinaturaId, [FromBody] AssinarDocumentoDto dto)
    {
        try
        {
            var (signerId, signerTipo, _) = GetSignerInfo();
            if (dto is null) return BadRequest("Body vazio.");

            // Processa assinatura (UPDATE no banco, ainda dentro da transação)
            var inst = await _documentoService.Assinar(instanciaId, assinaturaId, dto, signerId, signerTipo);

            // Commita a assinatura no banco
            await _uof.CommitAsync();

            // FIX: se concluído, recarrega a instância APÓS o commit para garantir que
            // assinatura_base64 seja lido do banco (o objeto "inst" em memória ainda
            // tem AssinaturaBase64 = null porque o SELECT ocorreu antes do UPDATE ser commitado)
            if (inst.Status == "concluido")
            {
                var instComAssinaturas = await _documentoService.ObterPorId(inst.Id);

                var htmlFinal = DocumentoService.InjetarAssinaturasNoHtml(
                    instComAssinaturas.ConteudoHtml,
                    instComAssinaturas.Assinaturas);

                var pdf = await _pdfService.HtmlParaPdfBase64Async(htmlFinal);

                await _uof.DocumentoRepository.SalvarPdfAsync(inst.Id, pdf);
                await _uof.CommitAsync();

                // Retorna com os dados atualizados
                return Ok(DocumentoService.ToDetalheDto(instComAssinaturas, signerId, signerTipo, htmlFinal));
            }

            return Ok(DocumentoService.ToDetalheDto(inst, signerId, signerTipo));
        }
        catch (KeyNotFoundException ex)        { return NotFound(ex.Message); }
        catch (InvalidOperationException ex)   { return Conflict(ex.Message); }
        catch (UnauthorizedAccessException)    { return Forbid(); }
        catch (ArgumentException ex)           { return BadRequest(ex.Message); }
        catch (Exception ex)                   { return StatusCode(500, ex.Message); }
    }

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> BaixarPdf(int id)
    {
        try
        {
            var (signerId, signerTipo, perfil) = GetSignerInfo();
            var inst = await _documentoService.ObterPorId(id);

            if (perfil == "funcionario" && inst.FuncionarioId != signerId) return Forbid();

            if (string.IsNullOrEmpty(inst.PdfBase64))
                return NotFound("PDF ainda nao gerado. O documento precisa ser totalmente assinado.");

            return Ok(new { pdfBase64 = inst.PdfBase64, nomeArquivo = $"documento_{id}.pdf" });
        }
        catch (KeyNotFoundException ex)     { return NotFound(ex.Message); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception ex)                { return StatusCode(500, ex.Message); }
    }

    [HttpGet("assinatura-perfil")]
    public async Task<IActionResult> ObterAssinaturaPerfil()
    {
        try
        {
            var (signerId, _, perfil) = GetSignerInfo();
            if (perfil == "empresa") return Forbid();

            var base64 = await _documentoService.ObterAssinaturaPerfil(signerId);
            return Ok(new AssinaturaPerfilResponseDto
            {
                Possui           = !string.IsNullOrEmpty(base64),
                AssinaturaBase64 = base64,
            });
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception ex)                { return StatusCode(500, ex.Message); }
    }

    [HttpPost("assinatura-perfil")]
    public async Task<IActionResult> SalvarAssinaturaPerfil([FromBody] SalvarAssinaturaPerfilDto dto)
    {
        try
        {
            var (signerId, _, perfil) = GetSignerInfo();
            if (perfil == "empresa") return Forbid();
            if (dto is null) return BadRequest("Body vazio.");

            await _documentoService.SalvarAssinaturaPerfil(signerId, dto.AssinaturaBase64);
            await _uof.CommitAsync();
            return Ok(new { mensagem = "Assinatura salva no perfil." });
        }
        catch (ArgumentException ex)        { return BadRequest(ex.Message); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception ex)                { return StatusCode(500, ex.Message); }
    }
}
