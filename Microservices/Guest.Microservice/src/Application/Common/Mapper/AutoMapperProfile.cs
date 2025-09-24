using Application.Guests.Commands;
using Application.Guests.Queries;
using Domain.Entities;
using Mapster;

namespace Application.Common.Mapper
{
    public static class MappingConfig
    {
        public static void RegisterMappings(TypeAdapterConfig config)
        {
            config.NewConfig<CreateGuestCommand, Guest>();
            config.NewConfig<Guest, CreateGuestCommand>();

            config.NewConfig<GetGuestResponse, Guest>();
            config.NewConfig<Guest, GetGuestResponse>();
        }
    }
}