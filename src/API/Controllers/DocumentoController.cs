using System.Security.Claims;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Application.Features.Documentos.Commands.AssinarDocumento;
using GestaoRH.Application.Features.Documentos.Commands.GerarDocumento;
using GestaoRH.Application.Features.Documentos.Commands.SalvarAssinaturaPerfil;
using GestaoRH.Application.Features.Documentos.Queries.ListarDocumentosPorSetor;
using GestaoRH.Application.Features.Documentos.Queries.ListarMeusDocumentos;
using GestaoRH.Application.Features.Documentos.Queries.ListarTodosDocumentos;
using GestaoRH.Application.Features.Documentos.Queries.ObterAssinaturaPerfil;
using GestaoRH.Application.Features.Documentos.Queries.ObterDocumentoPorId;
using GestaoRH.Application.Features.Documentos.Queries.ObterPdfDocumento;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GestaoRH.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentoController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private ClaimsPrincipal? Claims => HttpContext.Items["Claims"] as ClaimsPrincipal;

    private (int id, string tipo, string perfil) GetSignerInfo()
    {
        var claims = Claims ?? throw new UnauthorizedAccessException("Nao autenticado.");
        var id = int.Parse(claims.FindFirstValue("Id") ?? "0");
        var perfil = claims.FindFirstValue("Perfil") ?? string.Empty;
        var tipo = perfil == "empresa" ? "empresa" : "funcionario";
        return (id, tipo, perfil);
    }

    [HttpPost("gerar")]
    public async Task<IActionResult> Gerar([FromBody] GerarDocumentoDto dto)
    {
        var (empresaId, _, perfil) = GetSignerInfo();
        if (perfil != "empresa") return Forbid();

        var command = new GerarDocumentoCommand(dto, empresaId);
        var result = await _mediator.Send(command);

        if (dto.SetorId.HasValue && dto.SetorId > 0)
        {
            return Ok(result);
        }
        else
        {
            var detail = (InstanciaDetalheDto)result;
            return CreatedAtAction(nameof(ObterPorId), new { id = detail.Id }, detail);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ListarTodos()
    {
        var (_, _, perfil) = GetSignerInfo();
        if (perfil != "empresa") return Forbid();

        var result = await _mediator.Send(new ListarTodosDocumentosQuery());
        return Ok(result);
    }

    [HttpGet("meus")]
    public async Task<IActionResult> MeusDocumentos()
    {
        var (signerId, _, perfil) = GetSignerInfo();
        if (perfil == "empresa") return Forbid();

        var result = await _mediator.Send(new ListarMeusDocumentosQuery(signerId));
        return Ok(result);
    }

    [HttpGet("setor/{setorId:int}")]
    public async Task<IActionResult> ListarPorSetor(int setorId)
    {
        var (_, _, perfil) = GetSignerInfo();
        if (perfil != "chefe" && perfil != "empresa") return Forbid();

        var result = await _mediator.Send(new ListarDocumentosPorSetorQuery(setorId));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var (signerId, signerTipo, perfil) = GetSignerInfo();
        var result = await _mediator.Send(new ObterDocumentoPorIdQuery(id, signerId, signerTipo));

        if (perfil == "funcionario" && result.FuncionarioId != signerId)
            return Forbid();

        return Ok(result);
    }

    [HttpPost("{instanciaId:int}/assinar/{assinaturaId:int}")]
    public async Task<IActionResult> Assinar(int instanciaId, int assinaturaId, [FromBody] AssinarDocumentoDto dto)
    {
        var (signerId, signerTipo, _) = GetSignerInfo();
        var command = new AssinarDocumentoCommand(instanciaId, assinaturaId, dto, signerId, signerTipo);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> BaixarPdf(int id)
    {
        var (signerId, _, perfil) = GetSignerInfo();
        
        // We do a permission check by fetching the doc info
        var doc = await _mediator.Send(new ObterDocumentoPorIdQuery(id, signerId, perfil == "empresa" ? "empresa" : "funcionario"));
        if (perfil == "funcionario" && doc.FuncionarioId != signerId) return Forbid();

        var pdfBase64 = await _mediator.Send(new ObterPdfDocumentoQuery(id));
        return Ok(new { pdfBase64 = pdfBase64, nomeArquivo = $"documento_{id}.pdf" });
    }

    [HttpGet("assinatura-perfil")]
    public async Task<IActionResult> ObterAssinaturaPerfil()
    {
        var (signerId, _, perfil) = GetSignerInfo();
        if (perfil == "empresa") return Forbid();

        var base64 = await _mediator.Send(new ObterAssinaturaPerfilQuery(signerId));
        return Ok(new AssinaturaPerfilResponseDto
        {
            Possui = !string.IsNullOrEmpty(base64),
            AssinaturaBase64 = base64,
        });
    }

    [HttpPost("assinatura-perfil")]
    public async Task<IActionResult> SalvarAssinaturaPerfil([FromBody] SalvarAssinaturaPerfilDto dto)
    {
        var (signerId, _, perfil) = GetSignerInfo();
        if (perfil == "empresa") return Forbid();

        var command = new SalvarAssinaturaPerfilCommand(signerId, dto.AssinaturaBase64);
        await _mediator.Send(command);
        return Ok(new { mensagem = "Assinatura salva no perfil." });
    }
}
