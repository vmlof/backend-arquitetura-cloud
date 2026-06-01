using System.Text;
using GestaoRH.Models;
using GestaoRH.Models.DTOs;
using GestaoRH.Repositories;

namespace GestaoRH.Services;

public class DocumentoService
{
    private readonly IUnitOfWork _uof;

    public DocumentoService(IUnitOfWork uof) => _uof = uof;

    // ── Geração individual ───────────────────────────────────

    public async Task<DocumentoInstancia> GerarIndividual(GerarDocumentoDto dto, int empresaId)
    {
        var modelo = await _uof.ModeloRepository.ObterPorIdAsync(dto.ModeloId)
                     ?? throw new KeyNotFoundException("Modelo nao encontrado.");

        if (modelo.Status != "publicado")
            throw new InvalidOperationException("Apenas modelos publicados podem gerar documentos.");

        var funcionario = await _uof.FuncionarioRepository.ObterPorIdAsync(dto.FuncionarioId)
                          ?? throw new KeyNotFoundException("Funcionario nao encontrado.");

        var empresa = await _uof.EmpresaRepository.ObterPorIdAsync(empresaId)
                      ?? throw new KeyNotFoundException("Empresa nao encontrada.");

        // Resolve placeholders automáticos
        var variaveis = MontarVariaveisAutomaticas(funcionario, empresa);

        // Adiciona variáveis manuais (sobrescreve se existir)
        foreach (var v in dto.VariaveisManuals)
            variaveis[v.Token] = v.Valor;

        // Gera conteúdo HTML congelado
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

        // Salva variáveis manuais
        foreach (var v in dto.VariaveisManuals)
            await _uof.DocumentoRepository.InserirVariavelAsync(new DocumentoVariavelValor
            {
                InstanciaId = instId, Token = v.Token, Valor = v.Valor
            });

        // Cria registros de assinatura para cada papel do modelo
        await CriarRegistrosAssinatura(instId, modelo, funcionario, empresa);

        return await _uof.DocumentoRepository.ObterInstanciaPorIdAsync(instId)
               ?? throw new Exception("Falha ao recuperar instancia apos criacao.");
    }

    // ── Geração em lote (por setor) ──────────────────────────

    public async Task<DocumentoLote> GerarLote(GerarDocumentoDto dto, int empresaId)
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

        // Cria o lote
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

        // Gera uma instância por funcionário
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

        return await _uof.DocumentoRepository.ObterLotePorIdAsync(loteId)
               ?? throw new Exception("Falha ao recuperar lote apos criacao.");
    }

    // ── Assinar ──────────────────────────────────────────────

    public async Task<DocumentoInstancia> Assinar(
        int instanciaId, int assinaturaId, AssinarDocumentoDto dto,
        int signerId, string signerTipo)
    {
        var assinatura = await _uof.DocumentoRepository.ObterAssinaturaPorIdAsync(assinaturaId)
                         ?? throw new KeyNotFoundException("Registro de assinatura nao encontrado.");

        if (assinatura.InstanciaId != instanciaId)
            throw new InvalidOperationException("Assinatura nao pertence a este documento.");

        // Valida que é o signer correto
        if (assinatura.SignerId != signerId || assinatura.SignerTipo != signerTipo)
            throw new UnauthorizedAccessException("Voce nao tem permissao para assinar este documento.");

        if (assinatura.Status == "assinado")
            throw new InvalidOperationException("Este documento ja foi assinado.");

        if (string.IsNullOrWhiteSpace(dto.AssinaturaBase64))
            throw new ArgumentException("Imagem da assinatura e obrigatoria.");

        // Salva a assinatura
        await _uof.DocumentoRepository.RegistrarAssinaturaAsync(
            assinaturaId, dto.AssinaturaBase64, DateTime.UtcNow, dto.IpAddress);

        // Salva no perfil do funcionário se solicitado
        if (dto.SalvarNoPerfil && signerTipo == "funcionario")
            await _uof.DocumentoRepository.SalvarAssinaturaPerfilAsync(signerId, dto.AssinaturaBase64);

        // Verifica se todas as assinaturas obrigatórias foram concluídas
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

        return await _uof.DocumentoRepository.ObterInstanciaPorIdAsync(instanciaId)
               ?? throw new Exception("Falha ao recuperar instancia apos assinatura.");
    }

    // ── Consultas ─────────────────────────────────────────────

    public async Task<DocumentoInstancia> ObterPorId(int id)
        => await _uof.DocumentoRepository.ObterInstanciaPorIdAsync(id)
           ?? throw new KeyNotFoundException("Documento nao encontrado.");

    public async Task<IEnumerable<DocumentoInstancia>> ListarTodos()
        => await _uof.DocumentoRepository.ListarTodasInstanciasAsync();

    public async Task<IEnumerable<DocumentoInstancia>> ListarPorFuncionario(int funcionarioId)
        => await _uof.DocumentoRepository.ListarInstanciasPorFuncionarioAsync(funcionarioId);

    public async Task<IEnumerable<DocumentoInstancia>> ListarPorSetor(int setorId)
        => await _uof.DocumentoRepository.ListarInstanciasPorSetorAsync(setorId);

    // ── Assinatura no perfil ──────────────────────────────────

    public async Task<string?> ObterAssinaturaPerfil(int funcionarioId)
        => await _uof.DocumentoRepository.ObterAssinaturaPerfilAsync(funcionarioId);

    public async Task SalvarAssinaturaPerfil(int funcionarioId, string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
            throw new ArgumentException("Assinatura nao pode ser vazia.");
        await _uof.DocumentoRepository.SalvarAssinaturaPerfilAsync(funcionarioId, base64);
    }

    // ── Helpers privados ─────────────────────────────────────

    private static Dictionary<string, string> MontarVariaveisAutomaticas(
        Funcionario f, Empresa e)
    {
        var hoje = DateTime.Now;
        return new Dictionary<string, string>
        {
            ["{funcionario_nome}"]       = f.Nome,
            ["{funcionario_cpf}"]        = f.Cpf,
            ["{funcionario_email}"]      = f.Email,
            ["{funcionario_telefone}"]   = f.Telefone,
            ["{funcionario_turno}"]      = CapitalizarPrimeiraLetra(f.Turno),
            ["{funcionario_genero}"]     = CapitalizarPrimeiraLetra(f.Genero),
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

        // Cabeçalho
        sb.Append($"<h1>{Escapar(modelo.Nome)}</h1>");
        sb.Append($@"<div class=""empresa-info"">{Escapar(variaveis.GetValueOrDefault("{empresa_razao_social}", ""))}</div>");

        // Seções
        foreach (var sec in modelo.Secoes.OrderBy(s => s.Ordem))
        {
            if (!string.IsNullOrWhiteSpace(sec.Titulo))
                sb.Append($@"<div class=""secao-titulo"">{Escapar(sec.Titulo)}</div>");

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
                        <div class=""campo-label"">{Escapar(campo.Label)}{(campo.Obrigatorio ? " *" : "")}</div>
                        <div class=""campo-valor""></div>
                    </div>");
                }
            }
            else if (sec.Tipo == "assinaturas")
            {
                // Placeholder — será substituído pelo bloco real de assinaturas no PDF final
                sb.Append(@"<!-- BLOCO_ASSINATURAS -->");
            }
        }

        // Se não tem seção assinatura explícita, adiciona ao final
        if (!modelo.Secoes.Any(s => s.Tipo == "assinaturas"))
            sb.Append(@"<!-- BLOCO_ASSINATURAS -->");

        sb.Append("</body></html>");
        return sb.ToString();
    }

    // Injeta o HTML das assinaturas reais no placeholder
    public static string InjetarAssinaturasNoHtml(
        string html, List<DocumentoAssinatura> assinaturas)
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

    private static string? NormalizarAssinaturaSrc(string? assinaturaBase64)
    {
        if (string.IsNullOrWhiteSpace(assinaturaBase64))
            return null;

        var valor = assinaturaBase64.Trim();
        if (valor.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            return valor;

        return $"data:image/png;base64,{valor}";
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
                    // Busca chefe ativo do setor do funcionário
                    var chefeSetor = (await _uof.FuncionarioRepository
                        .ListarPorSetorAsync(funcionario.SetorId))
                        .FirstOrDefault(f => f.IsChefe && f.Id != funcionario.Id);

                    // Se não há chefe diferente, usa o próprio funcionário (pode ser ele o chefe)
                    // ou o RH como fallback
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

    private static string ResolverTokens(string texto, Dictionary<string, string> vars)
    {
        foreach (var kv in vars)
            texto = texto.Replace(kv.Key, Escapar(kv.Value));
        return texto;
    }

    private static string Escapar(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    private static string CapitalizarPrimeiraLetra(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];

    // ── Mapeamentos para DTO ──────────────────────────────────

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
}
