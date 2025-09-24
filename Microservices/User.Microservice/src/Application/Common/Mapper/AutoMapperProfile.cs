using Application.Users.Commands;
using Application.Users.Queries;
using Domain.Entities;
using Mapster;

namespace Application.Common.Mapper
{
    public static class MappingConfig
    {
        public static void RegisterMappings(TypeAdapterConfig config)
        {
            config.NewConfig<CreateUserCommand, User>();
            config.NewConfig<User, CreateUserCommand>();

            config.NewConfig<GetUserResponse, User>();
            config.NewConfig<User, GetUserResponse>();
        }
    }
}