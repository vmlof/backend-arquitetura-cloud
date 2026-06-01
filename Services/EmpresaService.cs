using GestaoRH.Models;
using GestaoRH.Models.DTOs;
using GestaoRH.Repositories;

namespace GestaoRH.Services;

public class EmpresaService
{
    private readonly IUnitOfWork _uof;

    public EmpresaService(IUnitOfWork uof)
    {
        _uof = uof;
    }

    public async Task<Empresa> Cadastrar(EmpresaCadastroDto dto)
    {
        // Valida campos obrigatórios
        if (string.IsNullOrWhiteSpace(dto.Cnpj)
            || string.IsNullOrWhiteSpace(dto.RazaoSocial)
            || string.IsNullOrWhiteSpace(dto.Senha)
            || string.IsNullOrWhiteSpace(dto.ResponsavelNome)
            || string.IsNullOrWhiteSpace(dto.ResponsavelSobrenome))
            throw new ArgumentException("CNPJ, Razão Social, Responsável e Senha são obrigatórios.");

        // CNPJ já cadastrado?
        var jaExiste = await _uof.EmpresaRepository.ObterPorCnpjAsync(dto.Cnpj);
        if (jaExiste != null)
            throw new InvalidOperationException("CNPJ já cadastrado no sistema.");

        var empresa = new Empresa
        {
            Cnpj               = dto.Cnpj,
            RazaoSocial        = dto.RazaoSocial,
            Endereco           = dto.Endereco,
            Telefone           = dto.Telefone,
            LogoBase64         = dto.LogoBase64,
            ResponsavelNome    = dto.ResponsavelNome,
            ResponsavelSobrenome = dto.ResponsavelSobrenome,
            Senha              = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            Ativo              = true,
            CriadoEm          = DateTime.UtcNow
        };

        var id = await _uof.EmpresaRepository.CriarAsync(empresa);
        var criada = await _uof.EmpresaRepository.ObterPorIdAsync(id);

        return criada ?? throw new Exception("Falha ao recuperar empresa após cadastro.");
    }

    public async Task<Empresa> Login(string cnpj, string senha)
    {
        var empresa = await _uof.EmpresaRepository.ObterPorCnpjAsync(cnpj);

        if (empresa == null || !empresa.Ativo)
            throw new UnauthorizedAccessException("CNPJ ou senha inválidos.");

        if (!BCrypt.Net.BCrypt.Verify(senha, empresa.Senha))
            throw new UnauthorizedAccessException("CNPJ ou senha inválidos.");

        return empresa;
    }

    public async Task<Empresa> ObterPorId(int id)
    {
        var empresa = await _uof.EmpresaRepository.ObterPorIdAsync(id);
        return empresa ?? throw new KeyNotFoundException("Empresa não encontrada.");
    }

    public async Task<Empresa> Atualizar(int id, EmpresaAtualizarDto dto)
    {
        var empresa = await _uof.EmpresaRepository.ObterPorIdAsync(id);
        if (empresa == null) throw new KeyNotFoundException("Empresa não encontrada.");

        empresa.RazaoSocial           = dto.RazaoSocial;
        empresa.Endereco              = dto.Endereco;
        empresa.Telefone              = dto.Telefone;
        empresa.LogoBase64            = dto.LogoBase64;
        empresa.ResponsavelNome       = dto.ResponsavelNome;
        empresa.ResponsavelSobrenome  = dto.ResponsavelSobrenome;

        await _uof.EmpresaRepository.AtualizarAsync(empresa);
        return empresa;
    }

    public async Task Desativar(int id)
    {
        var empresa = await _uof.EmpresaRepository.ObterPorIdAsync(id);
        if (empresa == null) throw new KeyNotFoundException("Empresa não encontrada.");

        await _uof.EmpresaRepository.DesativarAsync(id);
    }

    public async Task<IEnumerable<Empresa>> Listar()
    {
        return await _uof.EmpresaRepository.ListarAsync();
    }

    // Helper: mapeia Empresa -> ResponseDto (sem expor senha)
    public static EmpresaResponseDto ToResponse(Empresa e) => new()
    {
        Id                   = e.Id,
        Cnpj                 = e.Cnpj,
        RazaoSocial          = e.RazaoSocial,
        Endereco             = e.Endereco,
        Telefone             = e.Telefone,
        LogoBase64           = e.LogoBase64,
        ResponsavelNome      = e.ResponsavelNome,
        ResponsavelSobrenome = e.ResponsavelSobrenome,
        Ativo                = e.Ativo,
        CriadoEm             = e.CriadoEm
    };
}
