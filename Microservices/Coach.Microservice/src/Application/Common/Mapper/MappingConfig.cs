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
        config.NewConfig<Sport, CoachProfileSportDto>()
            .Map(dest => dest.SportId, src => src.Id)
            .Map(dest => dest.SportName, src => src.DisplayName);
        config.NewConfig<CoachProfile, CoachProfileDto>()
            .Map(dest => dest.CoachId, src => src.UserId)
            .Map(dest => dest.Media, src => src.CoachMedia)
            .Map(dest => dest.KycRecords, src => src.KycRecords)
            .Map(dest => dest.Services, src => src.CoachServices)
            .Map(dest => dest.Sports, src => src.Sports);
        config.NewConfig<CoachService, CoachServiceDto>();
    }
}
