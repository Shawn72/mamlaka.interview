using Redis.OM.Modeling;
using System.ComponentModel.DataAnnotations;

namespace Mamlaka.API.RedisORM;

[Document(StorageType = StorageType.Json, Prefixes = ["TRANSACTIONS"])]
public class TransactionRedisModel
{
    [RedisIdField][Indexed][Key] public long Id { get; set; }
    [Indexed] public string UserId { get; set; }
    [Indexed] public decimal Amount { get; set; }
    [Indexed] public string TransactionRefId { get; set; }
    [Indexed] public string TransactionDate { get; set; }
    [Indexed] public string CreatedAt { get; set; }
    [Indexed] public string ModifiedAt { get; set; }
    [Indexed] public string ModifiedBy { get; set; }

}
