using GestaoRH.Domain.Entities;
using GestaoRH.Application.Common.DTOs;

namespace GestaoRH.Domain.Interfaces;

public interface ISetorRepository
{
    Task<int>              CriarAsync(Setor setor);
    Task<Setor?>           ObterPorIdAsync(int id);
    Task<Setor?>           ObterPorNomeAtivoAsync(string nome);
    Task                   AtualizarAsync(Setor setor);
    Task                   DesativarAsync(int id);
    Task<IEnumerable<Setor>> ListarAsync();
    Task<IEnumerable<Setor>> ListarTodosAsync();
}

