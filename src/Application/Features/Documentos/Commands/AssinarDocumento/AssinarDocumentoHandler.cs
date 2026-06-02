using GestaoRH.Application.Common.DTOs;
using GestaoRH.Application.Features.Documentos.Common;
using GestaoRH.Domain.Entities;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Documentos.Commands.AssinarDocumento;

public class AssinarDocumentoHandler : IRequestHandler<AssinarDocumentoCommand, InstanciaDetalheDto>
{
    private readonly IUnitOfWork _uof;
    private readonly IPdfService _pdfService;

    public AssinarDocumentoHandler(IUnitOfWork uof, IPdfService pdfService)
    {
        _uof = uof;
        _pdfService = pdfService;
    }

    public async Task<InstanciaDetalheDto> Handle(AssinarDocumentoCommand request, CancellationToken cancellationToken)
    {
        var instanciaId = request.InstanciaId;
        var assinaturaId = request.AssinaturaId;
        var dto = request.Dto;
        var signerId = request.SignerId;
        var signerTipo = request.SignerTipo;

        var assinatura = await _uof.DocumentoRepository.ObterAssinaturaPorIdAsync(assinaturaId)
                         ?? throw new KeyNotFoundException("Registro de assinatura nao encontrado.");

        if (assinatura.InstanciaId != instanciaId)
            throw new InvalidOperationException("Assinatura nao pertence a este documento.");

        if (assinatura.SignerId != signerId || assinatura.SignerTipo != signerTipo)
            throw new UnauthorizedAccessException("Voce nao tem permissao para assinar este documento.");

        if (assinatura.Status == "assinado")
            throw new InvalidOperationException("Este documento ja foi assinado.");

        if (string.IsNullOrWhiteSpace(dto.AssinaturaBase64))
            throw new ArgumentException("Imagem da assinatura e obrigatoria.");

        await _uof.DocumentoRepository.RegistrarAssinaturaAsync(
            assinaturaId, dto.AssinaturaBase64, DateTime.UtcNow, dto.IpAddress);

        if (dto.SalvarNoPerfil && signerTipo == "funcionario")
            await _uof.DocumentoRepository.SalvarAssinaturaPerfilAsync(signerId, dto.AssinaturaBase64);

        var todasAssinaturas = (await _uof.DocumentoRepository
            .ListarAssinaturasPorInstanciaAsync(instanciaId)).ToList();

        var todasObrigatoriasConcluidas = todasAssinaturas
            .Where(a => a.Obrigatorio)
            .All(a => a.Status == "assinado" || a.Id == assinaturaId);

        if (todasObrigatoriasConcluidas)
        {
            await _uof.DocumentoRepository.AtualizarStatusInstanciaAsync(
                instanciaId, "concluido", DateTime.UtcNow);
        }
        else
        {
            await _uof.DocumentoRepository.AtualizarStatusInstanciaAsync(
                instanciaId, "parcialmente_assinado");
        }

        await _uof.CommitAsync();

        var inst = await _uof.DocumentoRepository.ObterInstanciaPorIdAsync(instanciaId)
               ?? throw new Exception("Falha ao recuperar instancia apos assinatura.");

        if (inst.Status == "concluido")
        {
            var htmlFinal = DocumentoHelpers.InjetarAssinaturasNoHtml(inst.ConteudoHtml, inst.Assinaturas);
            var pdf = await _pdfService.HtmlParaPdfBase64Async(htmlFinal);
            await _uof.DocumentoRepository.SalvarPdfAsync(inst.Id, pdf);
            await _uof.CommitAsync();

            inst = await _uof.DocumentoRepository.ObterInstanciaPorIdAsync(instanciaId)
                   ?? throw new Exception("Falha ao recuperar instancia apos salvar PDF.");
        }

        return DocumentoHelpers.ToDetalheDto(inst, signerId, signerTipo);
    }
}
