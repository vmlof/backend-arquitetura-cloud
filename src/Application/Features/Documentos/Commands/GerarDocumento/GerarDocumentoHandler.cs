using System.Text;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Application.Features.Documentos.Common;
using GestaoRH.Domain.Entities;
using GestaoRH.Domain.Interfaces;
using MediatR;

namespace GestaoRH.Application.Features.Documentos.Commands.GerarDocumento;

public class GerarDocumentoHandler : IRequestHandler<GerarDocumentoCommand, object>
{
    private readonly IUnitOfWork _uof;

    public GerarDocumentoHandler(IUnitOfWork uof)
    {
        _uof = uof;
    }

    public async Task<object> Handle(GerarDocumentoCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var empresaId = request.EmpresaId;

        if (dto.SetorId.HasValue && dto.SetorId > 0)
        {
            var lote = await GerarLote(dto, empresaId);
            return new { loteId = lote.Id, total = lote.Total, mensagem = $"{lote.Total} documento(s) gerado(s)." };
        }
        else
        {
            var inst = await GerarIndividual(dto, empresaId);
            return DocumentoHelpers.ToDetalheDto(inst, empresaId, "empresa");
        }
    }

    private async Task<DocumentoInstancia> GerarIndividual(GerarDocumentoDto dto, int empresaId)
    {
        var modelo = await _uof.ModeloRepository.ObterPorIdAsync(dto.ModeloId)
                     ?? throw new KeyNotFoundException("Modelo nao encontrado.");

        if (modelo.Status != "publicado")
            throw new InvalidOperationException("Apenas modelos publicados podem gerar documentos.");

        var funcionario = await _uof.FuncionarioRepository.ObterPorIdAsync(dto.FuncionarioId)
                          ?? throw new KeyNotFoundException("Funcionario nao encontrado.");

        var empresa = await _uof.EmpresaRepository.ObterPorIdAsync(empresaId)
                      ?? throw new KeyNotFoundException("Empresa nao encontrada.");

        var variaveis = MontarVariaveisAutomaticas(funcionario, empresa);

        foreach (var v in dto.VariaveisManuals)
            variaveis[v.Token] = v.Valor;

        var html = GerarHtml(modelo, variaveis);

        var instancia = new DocumentoInstancia
        {
            ModeloId           = modelo.Id,
            ModeloVersao       = modelo.Versao,
            ModeloNomeSnapshot = modelo.Nome,
            FuncionarioId      = funcionario.Id,
            Status             = "aguardando_assinatura",
            ConteudoHtml       = html,
            CriadoEm          = DateTime.UtcNow,
        };

        var instId = await _uof.DocumentoRepository.CriarInstanciaAsync(instancia);

        foreach (var v in dto.VariaveisManuals)
            await _uof.DocumentoRepository.InserirVariavelAsync(new DocumentoVariavelValor
            {
                InstanciaId = instId, Token = v.Token, Valor = v.Valor
            });

        await CriarRegistrosAssinatura(instId, modelo, funcionario, empresa);

        await _uof.CommitAsync();

        return await _uof.DocumentoRepository.ObterInstanciaPorIdAsync(instId)
               ?? throw new Exception("Falha ao recuperar instancia apos criacao.");
    }

    private async Task<DocumentoLote> GerarLote(GerarDocumentoDto dto, int empresaId)
    {
        if (dto.SetorId == null)
            throw new ArgumentException("SetorId obrigatorio para geracao em lote.");

        var modelo = await _uof.ModeloRepository.ObterPorIdAsync(dto.ModeloId)
                     ?? throw new KeyNotFoundException("Modelo nao encontrado.");

        if (modelo.Status != "publicado")
            throw new InvalidOperationException("Apenas modelos publicados podem gerar documentos.");

        if (modelo.TipoUso == "individual")
            throw new InvalidOperationException("Este modelo nao suporta geracao em lote.");

        var funcionarios = (await _uof.FuncionarioRepository.ListarPorSetorAsync(dto.SetorId.Value)).ToList();
        if (funcionarios.Count == 0)
            throw new InvalidOperationException("Nenhum funcionario ativo encontrado no setor.");

        var empresa = await _uof.EmpresaRepository.ObterPorIdAsync(empresaId)
                      ?? throw new KeyNotFoundException("Empresa nao encontrada.");

        var lote = new DocumentoLote
        {
            ModeloId  = modelo.Id,
            SetorId   = dto.SetorId,
            CriadoPor = empresaId,
            Total     = funcionarios.Count,
            Status    = "gerado",
            CriadoEm = DateTime.UtcNow,
        };
        var loteId = await _uof.DocumentoRepository.CriarLoteAsync(lote);

        foreach (var funcionario in funcionarios)
        {
            var variaveis = MontarVariaveisAutomaticas(funcionario, empresa);
            foreach (var v in dto.VariaveisManuals)
                variaveis[v.Token] = v.Valor;

            var html = GerarHtml(modelo, variaveis);

            var instancia = new DocumentoInstancia
            {
                ModeloId           = modelo.Id,
                ModeloVersao       = modelo.Versao,
                ModeloNomeSnapshot = modelo.Nome,
                LoteId             = loteId,
                FuncionarioId      = funcionario.Id,
                Status             = "aguardando_assinatura",
                ConteudoHtml       = html,
                CriadoEm          = DateTime.UtcNow,
            };
            var instId = await _uof.DocumentoRepository.CriarInstanciaAsync(instancia);

            foreach (var v in dto.VariaveisManuals)
                await _uof.DocumentoRepository.InserirVariavelAsync(new DocumentoVariavelValor
                {
                    InstanciaId = instId, Token = v.Token, Valor = v.Valor
                });

            await CriarRegistrosAssinatura(instId, modelo, funcionario, empresa);
        }

        await _uof.CommitAsync();

        return await _uof.DocumentoRepository.ObterLotePorIdAsync(loteId)
               ?? throw new Exception("Falha ao recuperar lote apos criacao.");
    }

    private static Dictionary<string, string> MontarVariaveisAutomaticas(Funcionario f, Empresa e)
    {
        var hoje = DateTime.Now;
        return new Dictionary<string, string>
        {
            ["{funcionario_nome}"]       = f.Nome,
            ["{funcionario_cpf}"]        = f.Cpf,
            ["{funcionario_email}"]      = f.Email,
            ["{funcionario_telefone}"]   = f.Telefone,
            ["{funcionario_turno}"]      = DocumentoHelpers.CapitalizarPrimeiraLetra(f.Turno),
            ["{funcionario_genero}"]     = DocumentoHelpers.CapitalizarPrimeiraLetra(f.Genero),
            ["{funcionario_admissao}"]   = f.CriadoEm.ToString("dd/MM/yyyy"),
            ["{setor_nome}"]             = f.NomeSetor ?? string.Empty,
            ["{empresa_razao_social}"]   = e.RazaoSocial,
            ["{empresa_cnpj}"]           = e.Cnpj,
            ["{empresa_endereco}"]       = e.Endereco,
            ["{empresa_telefone}"]       = e.Telefone,
            ["{data_atual}"]             = hoje.ToString("dd/MM/yyyy"),
            ["{hora_atual}"]             = hoje.ToString("HH:mm"),
        };
    }

    private static string GerarHtml(DocumentoModelo modelo, Dictionary<string, string> variaveis)
    {
        var sb = new StringBuilder();

        sb.Append(@"<!DOCTYPE html><html lang=""pt-BR""><head>
<meta charset=""UTF-8"">
<style>
  body { font-family: Arial, sans-serif; font-size: 14px; color: #1a1a1a; margin: 40px; line-height: 1.7; }
  h1 { text-align: center; font-size: 18px; margin-bottom: 4px; }
  .empresa-info { text-align: center; font-size: 12px; color: #555; margin-bottom: 32px; }
  .secao-titulo { font-size: 15px; font-weight: bold; border-bottom: 1px solid #ccc; padding-bottom: 4px; margin: 24px 0 12px; }
  .conteudo-texto { white-space: pre-wrap; }
  .campo-linha { margin-bottom: 12px; }
  .campo-label { font-weight: bold; font-size: 12px; color: #555; margin-bottom: 2px; }
  .campo-valor { border-bottom: 1px solid #333; min-height: 22px; padding: 2px 4px; font-size: 13px; }
  .assinaturas { margin-top: 48px; page-break-inside: avoid; }
  .assinatura-bloco { display: inline-block; width: 220px; margin: 0 24px 24px 0; vertical-align: top; text-align: center; }
  .assinatura-area { height: 64px; border-bottom: 1px solid #333; margin-bottom: 6px; }
  .assinatura-nome { font-size: 12px; font-weight: bold; }
  .assinatura-papel { font-size: 11px; color: #666; }
  .assinatura-data  { font-size: 11px; color: #666; }
  .assinatura-img   { max-height: 60px; max-width: 200px; }
</style></head><body>");

        sb.Append($"<h1>{DocumentoHelpers.Escapar(modelo.Nome)}</h1>");
        sb.Append($@"<div class=""empresa-info"">{DocumentoHelpers.Escapar(variaveis.GetValueOrDefault("{empresa_razao_social}", ""))}</div>");

        foreach (var sec in modelo.Secoes.OrderBy(s => s.Ordem))
        {
            if (!string.IsNullOrWhiteSpace(sec.Titulo))
                sb.Append($@"<div class=""secao-titulo"">{DocumentoHelpers.Escapar(sec.Titulo)}</div>");

            if (sec.Tipo == "texto")
            {
                var conteudo = ResolverTokens(sec.Conteudo, variaveis);
                sb.Append($@"<div class=""conteudo-texto"">{conteudo}</div>");
            }
            else if (sec.Tipo == "campos")
            {
                foreach (var campo in sec.Campos.OrderBy(c => c.Ordem))
                {
                    sb.Append($@"<div class=""campo-linha"">
                        <div class=""campo-label"">{DocumentoHelpers.Escapar(campo.Label)}{(campo.Obrigatorio ? " *" : "")}</div>
                        <div class=""campo-valor""></div>
                    </div>");
                }
            }
            else if (sec.Tipo == "assinaturas")
            {
                sb.Append(@"<!-- BLOCO_ASSINATURAS -->");
            }
        }

        if (!modelo.Secoes.Any(s => s.Tipo == "assinaturas"))
            sb.Append(@"<!-- BLOCO_ASSINATURAS -->");

        sb.Append("</body></html>");
        return sb.ToString();
    }

    private static string ResolverTokens(string texto, Dictionary<string, string> vars)
    {
        foreach (var kv in vars)
            texto = texto.Replace(kv.Key, DocumentoHelpers.Escapar(kv.Value));
        return texto;
    }

    private async Task CriarRegistrosAssinatura(
        int instId, DocumentoModelo modelo, Funcionario funcionario, Empresa empresa)
    {
        foreach (var assinante in modelo.Assinantes.OrderBy(a => a.Ordem))
        {
            int    signerId;
            string signerTipo;
            string signerNome;
            string signerEmail;

            switch (assinante.Papel)
            {
                case "funcionario":
                    signerId   = funcionario.Id;
                    signerTipo = "funcionario";
                    signerNome = funcionario.Nome;
                    signerEmail = funcionario.Email;
                    break;

                case "rh":
                    signerId   = empresa.Id;
                    signerTipo = "empresa";
                    signerNome = $"{empresa.ResponsavelNome} {empresa.ResponsavelSobrenome}".Trim();
                    signerEmail = string.Empty;
                    break;

                case "chefe":
                    var chefeSetor = (await _uof.FuncionarioRepository
                        .ListarPorSetorAsync(funcionario.SetorId))
                        .FirstOrDefault(f => f.IsChefe && f.Id != funcionario.Id);

                    if (chefeSetor == null)
                    {
                        signerId   = empresa.Id;
                        signerTipo = "empresa";
                        signerNome = $"{empresa.ResponsavelNome} {empresa.ResponsavelSobrenome}".Trim();
                        signerEmail = string.Empty;
                    }
                    else
                    {
                        signerId   = chefeSetor.Id;
                        signerTipo = "funcionario";
                        signerNome = chefeSetor.Nome;
                        signerEmail = chefeSetor.Email;
                    }
                    break;

                default:
                    continue;
            }

            await _uof.DocumentoRepository.CriarAssinaturaAsync(new DocumentoAssinatura
            {
                InstanciaId         = instId,
                Papel               = assinante.Papel,
                SignerId            = signerId,
                SignerTipo          = signerTipo,
                SignerNomeSnapshot  = signerNome,
                SignerEmailSnapshot = signerEmail,
                Status              = "pendente",
                Obrigatorio         = assinante.Obrigatorio,
                Ordem               = assinante.Ordem,
                CriadoEm           = DateTime.UtcNow,
            });
        }
    }
}
