using GestaoRH.Models;
using GestaoRH.Models.DTOs;
using GestaoRH.Repositories;

namespace GestaoRH.Services;

public class ModeloService
{
    private readonly IUnitOfWork _uof;

    public ModeloService(IUnitOfWork uof) => _uof = uof;

    // ── Listar (simplificado — sem seções) ───────────────────
    public async Task<IEnumerable<ModeloListagemDto>> Listar()
    {
        var lista = await _uof.ModeloRepository.ListarAsync();
        return lista.Select(ToListagemDto);
    }

    // ── Obter por ID (completo — com seções/campos/assinantes) ──
    public async Task<ModeloResponseDto> ObterPorId(int id)
    {
        var modelo = await _uof.ModeloRepository.ObterPorIdAsync(id)
                     ?? throw new KeyNotFoundException("Modelo nao encontrado.");
        return ToResponseDto(modelo);
    }

    // ── Criar ────────────────────────────────────────────────
    public async Task<ModeloResponseDto> Criar(ModeloCadastroDto dto)
    {
        Validar(dto);

        var modelo = new DocumentoModelo
        {
            Nome         = dto.Nome.Trim(),
            Descricao    = dto.Descricao?.Trim() ?? string.Empty,
            Categoria    = dto.Categoria,
            TipoUso      = dto.TipoUso,
            Status       = "rascunho",
            Versao       = 1,
            CriadoEm    = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow,
        };

        var modeloId = await _uof.ModeloRepository.CriarAsync(modelo);
        await SalvarSecoesEAssinantes(modeloId, dto);

        var criado = await _uof.ModeloRepository.ObterPorIdAsync(modeloId)
                     ?? throw new Exception("Falha ao recuperar modelo apos cadastro.");
        return ToResponseDto(criado);
    }

    // ── Atualizar ────────────────────────────────────────────
    public async Task<ModeloResponseDto> Atualizar(int id, ModeloCadastroDto dto)
    {
        var modelo = await _uof.ModeloRepository.ObterPorIdAsync(id)
                     ?? throw new KeyNotFoundException("Modelo nao encontrado.");

        if (modelo.Status == "arquivado")
            throw new InvalidOperationException("Modelos arquivados nao podem ser editados.");

        Validar(dto);

        modelo.Nome         = dto.Nome.Trim();
        modelo.Descricao    = dto.Descricao?.Trim() ?? string.Empty;
        modelo.Categoria    = dto.Categoria;
        modelo.TipoUso      = dto.TipoUso;
        modelo.AtualizadoEm = DateTime.UtcNow;

        await _uof.ModeloRepository.AtualizarAsync(modelo);

        // Recria seções, campos e assinantes (estratégia delete+insert)
        await _uof.ModeloRepository.DeletarSecoesPorModeloAsync(id);
        await _uof.ModeloRepository.DeletarAssinantesPorModeloAsync(id);
        await SalvarSecoesEAssinantes(id, dto);

        var atualizado = await _uof.ModeloRepository.ObterPorIdAsync(id)
                         ?? throw new Exception("Falha ao recuperar modelo apos atualizacao.");
        return ToResponseDto(atualizado);
    }

    // ── Publicar ─────────────────────────────────────────────
    public async Task Publicar(int id)
    {
        var modelo = await _uof.ModeloRepository.ObterPorIdAsync(id)
                     ?? throw new KeyNotFoundException("Modelo nao encontrado.");

        if (modelo.Status == "arquivado")
            throw new InvalidOperationException("Modelos arquivados nao podem ser publicados.");

        // Valida: pelo menos 1 assinante obrigatório
        if (!modelo.Assinantes.Any(a => a.Obrigatorio))
            throw new InvalidOperationException("O modelo precisa ter pelo menos 1 assinante obrigatorio para ser publicado.");

        await _uof.ModeloRepository.PublicarAsync(id);
    }

    // ── Arquivar ─────────────────────────────────────────────
    public async Task Arquivar(int id)
    {
        _ = await _uof.ModeloRepository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException("Modelo nao encontrado.");
        await _uof.ModeloRepository.ArquivarAsync(id);
    }

    // ── Duplicar ─────────────────────────────────────────────
    public async Task<ModeloResponseDto> Duplicar(int id)
    {
        var original = await _uof.ModeloRepository.ObterPorIdAsync(id)
                       ?? throw new KeyNotFoundException("Modelo nao encontrado.");

        var dto = new ModeloCadastroDto
        {
            Nome      = $"{original.Nome} (cópia)",
            Descricao = original.Descricao,
            Categoria = original.Categoria,
            TipoUso   = original.TipoUso,
            Secoes    = original.Secoes.Select(s => new SecaoCadastroDto
            {
                Titulo   = s.Titulo,
                Tipo     = s.Tipo,
                Conteudo = s.Conteudo,
                Ordem    = s.Ordem,
                Campos   = s.Campos.Select(c => new CampoCadastroDto
                {
                    Label       = c.Label,
                    TipoCampo   = c.TipoCampo,
                    Obrigatorio = c.Obrigatorio,
                    Ordem       = c.Ordem,
                }).ToList(),
            }).ToList(),
            Assinantes = original.Assinantes.Select(a => new AssinanteCadastroDto
            {
                Papel       = a.Papel,
                Rotulo      = a.Rotulo,
                Obrigatorio = a.Obrigatorio,
                Ordem       = a.Ordem,
                ExibirPdf   = a.ExibirPdf,
            }).ToList(),
        };

        return await Criar(dto);
    }

    // ── Helpers privados ─────────────────────────────────────

    private static void Validar(ModeloCadastroDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
            throw new ArgumentException("Nome do modelo e obrigatorio.");
        if (string.IsNullOrWhiteSpace(dto.Categoria))
            throw new ArgumentException("Categoria e obrigatoria.");
        if (string.IsNullOrWhiteSpace(dto.TipoUso))
            throw new ArgumentException("Tipo de uso e obrigatorio.");
    }

    private async Task SalvarSecoesEAssinantes(int modeloId, ModeloCadastroDto dto)
    {
        // Salva seções
        for (int i = 0; i < dto.Secoes.Count; i++)
        {
            var secDto = dto.Secoes[i];
            var secao  = new DocumentoModeloSecao
            {
                ModeloId = modeloId,
                Titulo   = secDto.Titulo?.Trim() ?? $"Seção {i + 1}",
                Tipo     = secDto.Tipo,
                Conteudo = secDto.Conteudo ?? string.Empty,
                Ordem    = i,
            };
            var secaoId = await _uof.ModeloRepository.CriarSecaoAsync(secao);

            // Salva campos da seção
            for (int j = 0; j < secDto.Campos.Count; j++)
            {
                var campoDto = secDto.Campos[j];
                var campo    = new DocumentoModeloCampo
                {
                    SecaoId     = secaoId,
                    Label       = campoDto.Label?.Trim() ?? string.Empty,
                    TipoCampo   = campoDto.TipoCampo,
                    Obrigatorio = campoDto.Obrigatorio,
                    Ordem       = j,
                };
                await _uof.ModeloRepository.CriarCampoAsync(campo);
            }
        }

        // Salva assinantes
        for (int i = 0; i < dto.Assinantes.Count; i++)
        {
            var aDto     = dto.Assinantes[i];
            var assinante = new DocumentoModeloAssinante
            {
                ModeloId    = modeloId,
                Papel       = aDto.Papel,
                Rotulo      = aDto.Rotulo?.Trim() ?? string.Empty,
                Obrigatorio = aDto.Obrigatorio,
                Ordem       = i + 1,
                ExibirPdf   = aDto.ExibirPdf,
            };
            await _uof.ModeloRepository.CriarAssinanteAsync(assinante);
        }
    }

    // ── Mapeamentos ──────────────────────────────────────────

    public static ModeloListagemDto ToListagemDto(DocumentoModelo m) => new()
    {
        Id        = m.Id,
        Nome      = m.Nome,
        Descricao = m.Descricao,
        Categoria = m.Categoria,
        TipoUso   = m.TipoUso,
        Status    = m.Status,
        Versao    = m.Versao,
        CriadoEm = m.CriadoEm,
    };

    public static ModeloResponseDto ToResponseDto(DocumentoModelo m) => new()
    {
        Id           = m.Id,
        Nome         = m.Nome,
        Descricao    = m.Descricao,
        Categoria    = m.Categoria,
        TipoUso      = m.TipoUso,
        Status       = m.Status,
        Versao       = m.Versao,
        CriadoEm    = m.CriadoEm,
        AtualizadoEm = m.AtualizadoEm,
        Secoes       = m.Secoes.Select(s => new SecaoResponseDto
        {
            Id       = s.Id,
            Titulo   = s.Titulo,
            Tipo     = s.Tipo,
            Conteudo = s.Conteudo,
            Ordem    = s.Ordem,
            Campos   = s.Campos.Select(c => new CampoResponseDto
            {
                Id          = c.Id,
                Label       = c.Label,
                TipoCampo   = c.TipoCampo,
                Obrigatorio = c.Obrigatorio,
                Ordem       = c.Ordem,
            }).OrderBy(c => c.Ordem).ToList(),
        }).OrderBy(s => s.Ordem).ToList(),
        Assinantes = m.Assinantes.Select(a => new AssinanteResponseDto
        {
            Id          = a.Id,
            Papel       = a.Papel,
            Rotulo      = a.Rotulo,
            Obrigatorio = a.Obrigatorio,
            Ordem       = a.Ordem,
            ExibirPdf   = a.ExibirPdf,
        }).OrderBy(a => a.Ordem).ToList(),
    };
}
