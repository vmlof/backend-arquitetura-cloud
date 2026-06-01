using AutoMapper;
using GestaoRH.Application.Common.DTOs;
using GestaoRH.Domain.Entities;

namespace GestaoRH.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Funcionario, FuncionarioResponseDto>();
        CreateMap<Funcionario, FuncionarioRhResponseDto>();
        
        CreateMap<Empresa, EmpresaResponseDto>();
        CreateMap<Setor, SetorResponseDto>();
        
        CreateMap<DocumentoModelo, ModeloResponseDto>();
        CreateMap<DocumentoInstancia, InstanciaListagemDto>();
        CreateMap<DocumentoInstancia, InstanciaDetalheDto>();
    }
}
