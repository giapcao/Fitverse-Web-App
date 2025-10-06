using System.Linq;
using Application.Features;
using Domain.Persistence.Models;
using Mapster;

namespace Application.Common.Mapper;

public static class MappingConfig
{
    public static void RegisterMappings(TypeAdapterConfig config)
    {
        config.NewConfig<CoachMedium, CoachMediaDto>();
        config.NewConfig<CoachCertification, CoachCertificationDto>();
        config.NewConfig<KycRecord, KycRecordDto>();
        config.NewConfig<Sport, SportDto>();
        config.NewConfig<CoachProfile, CoachProfileDto>()
            .Map(dest => dest.CoachId, src => src.UserId)
            .Map(dest => dest.Media, src => src.CoachMedia)
            .Map(dest => dest.KycRecords, src => src.KycRecords)
            .Map(dest => dest.Services, src => src.CoachServices)
            .Map(dest => dest.SportIds, src => src.Sports.Select(s => s.Id));
        config.NewConfig<CoachService, CoachServiceDto>();
    }
}
