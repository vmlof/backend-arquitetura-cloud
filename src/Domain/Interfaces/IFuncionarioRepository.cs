using GestaoRH.Domain.Entities;
using GestaoRH.Application.Common.DTOs;

namespace GestaoRH.Domain.Interfaces;

public interface IFuncionarioRepository
{
    Task<int>                      CriarAsync(Funcionario funcionario);
    Task<Funcionario?>             ObterPorIdAsync(int id);
    Task<Funcionario?>             ObterPorCpfAtivoAsync(string cpf);
    Task<Funcionario?>             ObterPorEmailAtivoAsync(string email);
    Task                           AtualizarAsync(Funcionario funcionario);
    Task                           AtualizarSenhaAsync(int id, string senhaHash);
    Task                           DesativarAsync(int id);
    Task<IEnumerable<Funcionario>> ListarAsync();
    Task<IEnumerable<Funcionario>> ListarTodosAsync();
    Task<IEnumerable<Funcionario>> ListarPorSetorAsync(int setorId);
}

