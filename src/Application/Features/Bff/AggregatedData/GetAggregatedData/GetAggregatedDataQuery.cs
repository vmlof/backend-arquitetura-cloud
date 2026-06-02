using GestaoRH.Application.Features.Bff.Common;
using MediatR;

namespace GestaoRH.Application.Features.Bff.AggregatedData.GetAggregatedData;

public record GetAggregatedDataQuery : IRequest<AggregatedDataResponseDto>;
