using AutoMapper;
using Mamlaka.API.DAL.DTOs;
using Mamlaka.API.RedisORM;
using Mamlaka.API.DAL.Entities;
using Mamlaka.API.DAL.Entities.Transactions;

namespace Mamlaka.API.Configs;
public class AutoMapperProfile:Profile
{
    public AutoMapperProfile()
    {
        CreateMap<UserDto, User>();
        CreateMap<Transaction, TransactionRedisModel>();
    }
}
