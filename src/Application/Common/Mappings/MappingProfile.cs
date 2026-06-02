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
        
        CreateMap<DocumentoModelo, ModeloListagemDto>();
        CreateMap<DocumentoModelo, ModeloResponseDto>()
            .ForMember(dest => dest.Secoes, opt => opt.MapFrom(src => src.Secoes.OrderBy(s => s.Ordem)))
            .ForMember(dest => dest.Assinantes, opt => opt.MapFrom(src => src.Assinantes.OrderBy(a => a.Ordem)));

        CreateMap<DocumentoModeloSecao, SecaoResponseDto>()
            .ForMember(dest => dest.Campos, opt => opt.MapFrom(src => src.Campos.OrderBy(c => c.Ordem)));

        CreateMap<DocumentoModeloCampo, CampoResponseDto>();
        CreateMap<DocumentoModeloAssinante, AssinanteResponseDto>();
        
        CreateMap<DocumentoInstancia, InstanciaListagemDto>();
        CreateMap<DocumentoInstancia, InstanciaDetalheDto>();
    }
}
