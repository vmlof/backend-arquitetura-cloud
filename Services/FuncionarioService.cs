using GestaoRH.Models;
using GestaoRH.Models.DTOs;
using GestaoRH.Repositories;

namespace GestaoRH.Services;

public class FuncionarioService
{
    private readonly IUnitOfWork _uof;

    private static readonly HashSet<string> GenerosValidos = new(StringComparer.OrdinalIgnoreCase)
        { "masculino", "feminino", "sem_genero" };
    private static readonly HashSet<string> TurnosValidos = new(StringComparer.OrdinalIgnoreCase)
        { "matutino", "vespertino", "noturno" };

    public FuncionarioService(IUnitOfWork uof) => _uof = uof;

    private static string GerarSenhaTemporaria(string cpf)
    {
        var digits = new string(cpf.Where(char.IsDigit).ToArray());
        return $"{(digits.Length >= 4 ? digits[..4] : digits)}senha#";
    }

    public async Task<Funcionario> Cadastrar(FuncionarioCadastroDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Cpf) || string.IsNullOrWhiteSpace(dto.Nome) ||
            string.IsNullOrWhiteSpace(dto.Email) || dto.SetorId <= 0)
            throw new ArgumentException("CPF, Nome, Email e Setor sao obrigatorios.");

        if (!GenerosValidos.Contains(dto.Genero)) throw new ArgumentException("Genero invalido.");
        if (!TurnosValidos.Contains(dto.Turno))   throw new ArgumentException("Turno invalido.");

        if (await _uof.FuncionarioRepository.ObterPorCpfAtivoAsync(dto.Cpf) != null)
            throw new InvalidOperationException("CPF ja cadastrado por um funcionario ativo.");
        if (await _uof.FuncionarioRepository.ObterPorEmailAtivoAsync(dto.Email) != null)
            throw new InvalidOperationException("E-mail ja cadastrado por um funcionario ativo.");
        if (await _uof.SetorRepository.ObterPorIdAsync(dto.SetorId) == null)
            throw new KeyNotFoundException("Setor informado nao encontrado.");

        var senhaTemp = GerarSenhaTemporaria(dto.Cpf);
        var func = new Funcionario
        {
            Cpf             = dto.Cpf,
            Nome            = dto.Nome,
            Telefone        = dto.Telefone ?? string.Empty,
            Email           = dto.Email,
            Genero          = dto.Genero.ToLower(),
            Turno           = dto.Turno.ToLower(),
            SetorId         = dto.SetorId,
            SenhaTemporaria = senhaTemp,
            Senha           = BCrypt.Net.BCrypt.HashPassword(senhaTemp),
            SenhaTrocada    = false,
            IsChefe         = dto.IsChefe,
            Ativo           = true,
            CriadoEm       = DateTime.UtcNow
        };

        var id = await _uof.FuncionarioRepository.CriarAsync(func);
        return await _uof.FuncionarioRepository.ObterPorIdAsync(id)
               ?? throw new Exception("Falha ao recuperar funcionario apos cadastro.");
    }

    public async Task<Funcionario> Login(string cpf, string senha)
    {
        // Busca direto por CPF ativo — não carrega todos na memória
        var func = await _uof.FuncionarioRepository.ObterPorCpfAtivoAsync(cpf);

        if (func == null)
            throw new UnauthorizedAccessException("CPF ou senha invalidos.");

        if (!BCrypt.Net.BCrypt.Verify(senha, func.Senha))
            throw new UnauthorizedAccessException("CPF ou senha invalidos.");

        return func;
    }

    public async Task TrocarSenha(int id, FuncionarioTrocarSenhaDto dto)
    {
        var func = await _uof.FuncionarioRepository.ObterPorIdAsync(id)
                   ?? throw new KeyNotFoundException("Funcionario nao encontrado.");
        if (!BCrypt.Net.BCrypt.Verify(dto.SenhaAtual, func.Senha))
            throw new UnauthorizedAccessException("Senha atual incorreta.");
        if (string.IsNullOrWhiteSpace(dto.NovaSenha) || dto.NovaSenha.Length < 6)
            throw new ArgumentException("A nova senha deve ter pelo menos 6 caracteres.");
        await _uof.FuncionarioRepository.AtualizarSenhaAsync(id, BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha));
    }

    public async Task<Funcionario> ObterPorId(int id)
        => await _uof.FuncionarioRepository.ObterPorIdAsync(id)
           ?? throw new KeyNotFoundException("Funcionario nao encontrado.");

    public async Task<IEnumerable<Funcionario>> Listar()
        => await _uof.FuncionarioRepository.ListarAsync();

    public async Task<IEnumerable<Funcionario>> ListarTodos()
        => await _uof.FuncionarioRepository.ListarTodosAsync();

    public async Task<IEnumerable<Funcionario>> ListarPorSetor(int setorId)
        => await _uof.FuncionarioRepository.ListarPorSetorAsync(setorId);

    public async Task<Funcionario> Atualizar(int id, FuncionarioAtualizarDto dto)
    {
        var func = await _uof.FuncionarioRepository.ObterPorIdAsync(id)
                   ?? throw new KeyNotFoundException("Funcionario nao encontrado.");
        if (!GenerosValidos.Contains(dto.Genero)) throw new ArgumentException("Genero invalido.");
        if (!TurnosValidos.Contains(dto.Turno))   throw new ArgumentException("Turno invalido.");

        if (!string.Equals(func.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            var dup = await _uof.FuncionarioRepository.ObterPorEmailAtivoAsync(dto.Email);
            if (dup != null && dup.Id != id)
                throw new InvalidOperationException("E-mail ja em uso por outro funcionario ativo.");
        }

        func.Nome     = dto.Nome;
        func.Telefone = dto.Telefone ?? string.Empty;
        func.Email    = dto.Email;
        func.Genero   = dto.Genero.ToLower();
        func.Turno    = dto.Turno.ToLower();
        func.SetorId  = dto.SetorId;
        func.IsChefe  = dto.IsChefe;
        func.Ativo    = dto.Ativo;

        await _uof.FuncionarioRepository.AtualizarAsync(func);
        return func;
    }

    public async Task Desativar(int id)
    {
        _ = await _uof.FuncionarioRepository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException("Funcionario nao encontrado.");
        await _uof.FuncionarioRepository.DesativarAsync(id);
    }

    public static FuncionarioResponseDto ToResponse(Funcionario f) => new()
    {
        Id=f.Id, Cpf=f.Cpf, Nome=f.Nome, Telefone=f.Telefone, Email=f.Email,
        Genero=f.Genero, Turno=f.Turno, SetorId=f.SetorId, NomeSetor=f.NomeSetor,
        SenhaTrocada=f.SenhaTrocada, IsChefe=f.IsChefe, Ativo=f.Ativo, CriadoEm=f.CriadoEm
    };

    public static FuncionarioRhResponseDto ToRhResponse(Funcionario f) => new()
    {
        Id=f.Id, Cpf=f.Cpf, Nome=f.Nome, Telefone=f.Telefone, Email=f.Email,
        Genero=f.Genero, Turno=f.Turno, SetorId=f.SetorId, NomeSetor=f.NomeSetor,
        SenhaTrocada=f.SenhaTrocada, IsChefe=f.IsChefe, Ativo=f.Ativo, CriadoEm=f.CriadoEm,
        SenhaTemporaria=f.SenhaTemporaria
    };
}