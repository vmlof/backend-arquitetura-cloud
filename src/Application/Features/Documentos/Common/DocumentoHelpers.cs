using System.Text;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Entities;

namespace GestaoRH.Application.Features.Documentos.Common;

public static class DocumentoHelpers
{
    public static InstanciaListagemDto ToListagemDto(DocumentoInstancia inst, List<DocumentoAssinatura>? assinaturas = null)
    {
        var assin = assinaturas ?? inst.Assinaturas;
        return new InstanciaListagemDto
        {
            Id                  = inst.Id,
            ModeloNome          = inst.ModeloNomeSnapshot,
            NomeFuncionario     = inst.NomeFuncionario ?? string.Empty,
            NomeSetor           = inst.NomeSetor ?? string.Empty,
            Status              = inst.Status,
            CriadoEm           = inst.CriadoEm,
            ConcluidoEm        = inst.ConcluidoEm,
            TotalAssinaturas    = assin.Count,
            AssinaturasPendentes = assin.Count(a => a.Status == "pendente" && a.Obrigatorio),
        };
    }

    public static InstanciaDetalheDto ToDetalheDto(
        DocumentoInstancia inst,
        int? loggedSignerId = null,
        string? loggedSignerTipo = null,
        string? conteudoHtml = null)
    {
        var conteudoHtmlFinal = conteudoHtml ?? inst.ConteudoHtml;
        if (inst.Status == "concluido" && conteudoHtmlFinal.Contains("<!-- BLOCO_ASSINATURAS -->"))
            conteudoHtmlFinal = InjetarAssinaturasNoHtml(conteudoHtmlFinal, inst.Assinaturas);

        int? minhaAssinaturaPendenteId = null;
        if (loggedSignerId.HasValue && loggedSignerTipo != null)
        {
            minhaAssinaturaPendenteId = inst.Assinaturas
                .Where(a => a.SignerId == loggedSignerId && a.SignerTipo == loggedSignerTipo && a.Status == "pendente")
                .OrderBy(a => a.Ordem)
                .Select(a => (int?)a.Id)
                .FirstOrDefault();
        }

        return new InstanciaDetalheDto
        {
            Id                       = inst.Id,
            ModeloId                 = inst.ModeloId,
            ModeloNome               = inst.ModeloNomeSnapshot,
            LoteId                   = inst.LoteId,
            FuncionarioId            = inst.FuncionarioId,
            NomeFuncionario          = inst.NomeFuncionario ?? string.Empty,
            NomeSetor                = inst.NomeSetor ?? string.Empty,
            Status                   = inst.Status,
            ConteudoHtml             = conteudoHtmlFinal,
            CriadoEm                = inst.CriadoEm,
            ConcluidoEm             = inst.ConcluidoEm,
            MinhaAssinaturaPendenteId = minhaAssinaturaPendenteId,
            Assinaturas = inst.Assinaturas.Select(a => new AssinaturaResponseDto
            {
                Id                 = a.Id,
                Papel              = a.Papel,
                SignerNomeSnapshot = a.SignerNomeSnapshot,
                Status             = a.Status,
                Obrigatorio        = a.Obrigatorio,
                Ordem              = a.Ordem,
                AssinadoEm        = a.AssinadoEm,
            }).OrderBy(a => a.Ordem).ToList(),
            Variaveis = inst.Variaveis.Select(v => new VariavelManualDto
            {
                Token = v.Token, Valor = v.Valor
            }).ToList(),
        };
    }

    public static string InjetarAssinaturasNoHtml(string html, List<DocumentoAssinatura> assinaturas)
    {
        var sb = new StringBuilder();
        sb.Append(@"<div class=""assinaturas""><div class=""secao-titulo"">ASSINATURAS</div>");

        foreach (var a in assinaturas.OrderBy(x => x.Ordem))
        {
            sb.Append(@"<div class=""assinatura-bloco"">");
            sb.Append(@"<div class=""assinatura-area"">");
            var assinaturaSrc = NormalizarAssinaturaSrc(a.AssinaturaBase64);
            if (!string.IsNullOrWhiteSpace(assinaturaSrc))
                sb.Append($@"<img class=""assinatura-img"" src=""{assinaturaSrc}"" />");
            sb.Append("</div>");
            sb.Append($@"<div class=""assinatura-nome"">{Escapar(a.SignerNomeSnapshot)}</div>");
            sb.Append($@"<div class=""assinatura-papel"">{CapitalizarPrimeiraLetra(a.Papel)}</div>");
            sb.Append($@"<div class=""assinatura-data"">{(a.AssinadoEm.HasValue ? a.AssinadoEm.Value.ToString("dd/MM/yyyy HH:mm") : "Pendente")}</div>");
            sb.Append("</div>");
        }

        sb.Append("</div>");
        return html.Replace("<!-- BLOCO_ASSINATURAS -->", sb.ToString());
    }

    public static string? NormalizarAssinaturaSrc(string? assinaturaBase64)
    {
        if (string.IsNullOrWhiteSpace(assinaturaBase64))
            return null;

        var valor = assinaturaBase64.Trim();
        if (valor.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            return valor;

        return $"data:image/png;base64,{valor}";
    }

    public static string Escapar(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    public static string CapitalizarPrimeiraLetra(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];
}
