using GestaoRH.Domain.Entities;
using GestaoRH.Application.Common.DTOs;

namespace GestaoRH.Domain.Interfaces;

public interface IEmpresaRepository
{
    Task<int> CriarAsync(Empresa empresa);
    Task<Empresa?> ObterPorIdAsync(int id);
    Task<Empresa?> ObterPorCnpjAsync(string cnpj);
    Task AtualizarAsync(Empresa empresa);
    Task DesativarAsync(int id);
    Task<IEnumerable<Empresa>> ListarAsync();
}

