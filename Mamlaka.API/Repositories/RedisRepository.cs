using System.Net;

using Redis.OM;
using Redis.OM.Searching;

using Mamlaka.API.RedisORM;
using Mamlaka.API.Interfaces;
using Mamlaka.API.Exceptions;

namespace Mamlaka.API.Repositories;
public class RedisRepository : IRedisRepository
{
    private readonly RedisConnectionProvider _provider;
    private readonly RedisCollection<TransactionRedisModel> _redisTransaction;
    public RedisRepository(RedisConnectionProvider provider)
    {
        _provider = provider;
        _redisTransaction = (RedisCollection<TransactionRedisModel>)_provider.RedisCollection<TransactionRedisModel>();
    }
    public async Task<object> InsertTransactionToRedis(TransactionRedisModel redisModel)
    {
        return await _redisTransaction.InsertAsync(redisModel);
    }

    public async Task<object> InsertTransactionToRedisIfNotExist(TransactionRedisModel redisModel)
    {
        //check first if transaction exists there
        TransactionRedisModel _transactionInRedis = _redisTransaction.Where(x => x.Id == redisModel.Id).FirstOrDefault();
        if (redisModel is not null)
        {
            //transaction is there, just update
            return await UpdateTransactionInRedis(redisModel);
        }
        else
        {
            //add afresh
            return await _redisTransaction.InsertAsync(redisModel);
        }
    }

    public async Task<object> UpdateTransactionInRedis(TransactionRedisModel redisModel)
    {
        try
        {
            TransactionRedisModel transactionInRedis = _redisTransaction.Where(x => x.Id == redisModel.Id).FirstOrDefault();

            if (redisModel is not null)
            {
                if (string.IsNullOrWhiteSpace(redisModel.Id.ToString()))
                    transactionInRedis.Id = redisModel.Id;

                if (string.IsNullOrWhiteSpace(redisModel.UserId))
                    transactionInRedis.UserId = transactionInRedis.UserId;

                if (string.IsNullOrWhiteSpace(redisModel.Amount.ToString()))
                    transactionInRedis.Amount = transactionInRedis.Amount;

                if (string.IsNullOrWhiteSpace(redisModel.TransactionRefId))
                    transactionInRedis.TransactionRefId = transactionInRedis.TransactionRefId;

                if (string.IsNullOrWhiteSpace(redisModel.TransactionDate))
                    transactionInRedis.TransactionDate = transactionInRedis.TransactionDate;

                if (string.IsNullOrWhiteSpace(redisModel.CreatedAt))
                    transactionInRedis.CreatedAt = transactionInRedis.CreatedAt;

                if (string.IsNullOrWhiteSpace(redisModel.ModifiedAt))
                    transactionInRedis.ModifiedAt = transactionInRedis.ModifiedAt;

                if (string.IsNullOrWhiteSpace(redisModel.ModifiedBy))
                    transactionInRedis.ModifiedBy = transactionInRedis.ModifiedBy;

                transactionInRedis.Id = transactionInRedis.Id;
                transactionInRedis.UserId = transactionInRedis.UserId;
                transactionInRedis.Amount = transactionInRedis.Amount;
                transactionInRedis.TransactionRefId = transactionInRedis.TransactionRefId;
                transactionInRedis.TransactionDate = transactionInRedis.TransactionDate;
                transactionInRedis.CreatedAt = transactionInRedis.CreatedAt;
                transactionInRedis.ModifiedAt = transactionInRedis.ModifiedAt;
                transactionInRedis.ModifiedBy = transactionInRedis.ModifiedBy;

                await _redisTransaction.SaveAsync();
                return "Transaction cache refreshed";
            }
            return $"error*{HttpStatusCode.BadRequest}";
        }
        catch (Exception ex)
        {
            throw new CustomException($"Exception: {ex.Message}", "ERR500", HttpStatusCode.InternalServerError);
        }
    }
}
