using GestaoRH.Models;
using GestaoRH.Models.DTOs;
using GestaoRH.Repositories;

namespace GestaoRH.Services;

public class SetorService
{
    private readonly IUnitOfWork _uof;

    public SetorService(IUnitOfWork uof) => _uof = uof;

    public async Task<Setor> Cadastrar(SetorCadastroDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
            throw new ArgumentException("Nome do setor e obrigatorio.");

        // Valida duplicidade apenas entre setores ATIVOS
        var jaExiste = await _uof.SetorRepository.ObterPorNomeAtivoAsync(dto.Nome);
        if (jaExiste != null)
            throw new InvalidOperationException($"Ja existe um setor ativo com o nome '{dto.Nome}'.");

        var setor = new Setor
        {
            Nome      = dto.Nome.Trim(),
            Descricao = dto.Descricao?.Trim() ?? string.Empty,
            Ativo     = true,
            CriadoEm = DateTime.UtcNow
        };

        var id = await _uof.SetorRepository.CriarAsync(setor);
        return await _uof.SetorRepository.ObterPorIdAsync(id)
               ?? throw new Exception("Falha ao recuperar setor apos cadastro.");
    }

    public async Task<Setor> ObterPorId(int id)
        => await _uof.SetorRepository.ObterPorIdAsync(id)
           ?? throw new KeyNotFoundException("Setor nao encontrado.");

    // Retorna apenas ativos — usado pelo select do frontend (cadastro de funcionario)
    public async Task<IEnumerable<Setor>> Listar()
        => await _uof.SetorRepository.ListarAsync();

    // Retorna todos — usado pela tela de gestão de setores
    public async Task<IEnumerable<Setor>> ListarTodos()
        => await _uof.SetorRepository.ListarTodosAsync();

    public async Task<Setor> Atualizar(int id, SetorAtualizarDto dto)
    {
        var setor = await _uof.SetorRepository.ObterPorIdAsync(id)
                    ?? throw new KeyNotFoundException("Setor nao encontrado.");

        // Valida nome duplicado apenas entre ativos (exceto o próprio)
        if (!string.Equals(setor.Nome, dto.Nome, StringComparison.OrdinalIgnoreCase))
        {
            var duplicado = await _uof.SetorRepository.ObterPorNomeAtivoAsync(dto.Nome);
            if (duplicado != null && duplicado.Id != id)
                throw new InvalidOperationException($"Ja existe um setor ativo com o nome '{dto.Nome}'.");
        }

        setor.Nome      = dto.Nome.Trim();
        setor.Descricao = dto.Descricao?.Trim() ?? string.Empty;
        setor.Ativo     = dto.Ativo;

        await _uof.SetorRepository.AtualizarAsync(setor);
        return setor;
    }

    public async Task Desativar(int id)
    {
        _ = await _uof.SetorRepository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException("Setor nao encontrado.");
        await _uof.SetorRepository.DesativarAsync(id);
    }

    public static SetorResponseDto ToResponse(Setor s) => new()
    {
        Id        = s.Id,
        Nome      = s.Nome,
        Descricao = s.Descricao,
        Ativo     = s.Ativo,
        CriadoEm = s.CriadoEm
    };
}
