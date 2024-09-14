using Mamlaka.API.RedisORM;

namespace Mamlaka.API.Interfaces;
public interface IRedisRepository
{
    Task<object> InsertTransactionToRedis(TransactionRedisModel redisModel);
    Task<object> InsertTransactionToRedisIfNotExist(TransactionRedisModel redisModel);
    Task<object> UpdateTransactionInRedis(TransactionRedisModel redisModel);

}
