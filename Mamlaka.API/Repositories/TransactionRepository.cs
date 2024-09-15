using System.Net;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Mamlaka.API.CommonObjects.Requests;

using Mamlaka.API.Exceptions;
using Mamlaka.API.Extensions;
using Mamlaka.API.Interfaces;
using Mamlaka.API.DAL.DbContexts;

using Dapper;
using Redis.OM;
using AutoMapper;
using Redis.OM.Searching;

using Mamlaka.API.RedisORM;
using Mamlaka.API.DAL.DTOs;
using Mamlaka.API.DAL.Enums;
using Mamlaka.API.DAL.Models;
using Mamlaka.API.DAL.Entities;
using Mamlaka.API.Services.GatewayService;
using Constants = Mamlaka.API.DAL.Constants.Constants; //to remove ambinguity
using Transaction = Mamlaka.API.DAL.Entities.Transactions.Transaction;
using Mamlaka.API.DAL.Queries;
using System.Data;

namespace Mamlaka.API.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly IMapper _mapper;
    private readonly Constants _constants;
    private readonly string connectionString;
    private readonly IConfiguration _configuration;
    private readonly MySqlDbContext _databaseContext;
    private readonly IUserRepository _userRepository;
    private readonly RedisConnectionProvider _provider;
    private readonly IRedisRepository _redisRepository;
    private readonly PaypalGatewayService _payPalService;
    private readonly RedisCollection<TransactionRedisModel> _redisTransaction;

    public TransactionRepository(
        IMapper mapper,
        IConfiguration configuration,
        MySqlDbContext databaseContext,
        IUserRepository userRepository,
        RedisConnectionProvider provider,
        IRedisRepository redisRepository,
        PaypalGatewayService payPalService
        )
    {
        _mapper = mapper;
        _provider = provider;
        _configuration = configuration;
        _payPalService = payPalService;
        _userRepository = userRepository;
        _databaseContext = databaseContext;
        _redisRepository = redisRepository;
        _constants = new Constants(configuration);
        _redisTransaction = (RedisCollection<TransactionRedisModel>)
            _provider.RedisCollection<TransactionRedisModel>();
        connectionString = _constants.SqlConnectionString(configuration);
    }
    public async Task<object> CreateTransaction(TransactionRequest request)
    {
        try
        {
            User _user = await _userRepository.GetUser(request.UserId);
            if (_user is null)
            {
                return "specified user is not found.";
            }

            if (request is not null)
            {
                string trxStatusString = request.TransactionStatus;
                TransactionStatus transactionStatus;
                bool success = Enum.TryParse(trxStatusString, out transactionStatus);

                if (!Enum.TryParse(trxStatusString, out transactionStatus))
                {
                    throw new CustomException($"Invalid transaction status", "ERR412", HttpStatusCode.PreconditionFailed);
                }

                Transaction _transaction = new()
                {
                    UserId = request.UserId,
                    Amount = request.Amount,
                    TransactionRefId = request.TransactionRefId,
                    TransactionDate = DateTime.Parse(DateTime.UtcNow.ToEastAfricanTime().ToString("yyyy-MM-dd HH:mm:ss tt")),
                    TransactionStatus = transactionStatus,
                    CreatedAt = DateTime.Parse(DateTime.UtcNow.ToEastAfricanTime().ToString("yyyy-MM-dd HH:mm:ss tt")),
                    ModifiedAt = DateTime.Parse(DateTime.UtcNow.ToEastAfricanTime().ToString("yyyy-MM-dd HH:mm:ss tt")),
                    ModifiedBy = request.ModifiedBy is not null ? request.ModifiedBy : _constants._defaultActor
                };

                await _databaseContext.Transactions.AddAsync(_transaction).ConfigureAwait(false);
                if (await _databaseContext.SaveChangesAsync() > 0)
                {
                    return "success*transaction added successfully.";
                }
                else
                {
                    return "error*transaction could not be added.";
                }
            }
            else
                return $"error*bad request: {HttpStatusCode.BadRequest}";
        }
        catch (Exception ex)
        {
            throw new CustomException($"exception: {ex.Message}", "ERR500", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<object> DeleteTransaction(Transaction transaction)
    {
        try
        {
            if (transaction is not null)
            {
                _databaseContext.Transactions.Remove(transaction);
                if (await _databaseContext.SaveChangesAsync() > 0)
                { return $"success*transaction record removed successfully."; }
                else
                    return $"error*Error occurred while deleting transaction.";
            }
            else
                return $"error*bad request:{HttpStatusCode.BadRequest}";
        }
        catch (Exception ex)
        {
            throw new CustomException($"exception: {ex.Message}", "ERR500", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Transaction> GetTransaction(long transactionId)
    {
        try
        {
#nullable enable
            Transaction? transaction = await _databaseContext.Transactions.AsNoTracking().Include(x => x.User).FirstOrDefaultAsync(p => p.Id.Equals(transactionId));
            if (transaction is not null)
            {

                var transactionDto = new TransactionDto
                {
                    Id = transaction.Id,
                    UserId = transaction.UserId,
                    Amount = Math.Round(transaction.Amount, 2, MidpointRounding.AwayFromZero),
                    UserFullName = Regex.Replace($"{transaction.User.FirstName}{" "}{transaction.User.LastName}", @"\s+", " "),
                    TransactionRefId = transaction.TransactionRefId,
                    TransactionDate = transaction.TransactionDate.ToLongDateString(),
                    CreatedAt = transaction.TransactionDate.ToLongDateString(),
                    ModifiedAt = transaction.ModifiedAt.ToLongDateString(),
                    ModifiedBy = transaction.ModifiedBy
                };
                return _mapper.Map<Transaction>(transactionDto);
            }
            else return null;
        }
        catch (Exception ex)
        {
            throw new CustomException($"exception occured: {ex.Message}", "ERR500", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<object> GetTransactionsList()
    {
        try
        {
            List<Transaction> _transactions = await _databaseContext.Transactions.OrderByDescending(x => x.CreatedAt).Include(x => x.User).ToListAsync();

            if (_transactions.Count > 0)
            {
                List<TransactionDto> transactionDto = new List<TransactionDto>();
                foreach (var transaction in _transactions)
                {
                    transactionDto.Add(new TransactionDto
                    {
                        Id = transaction.Id,
                        UserId = transaction.UserId,
                        Amount = Math.Round(transaction.Amount, 2, MidpointRounding.AwayFromZero),
                        UserFullName = Regex.Replace($"{transaction.User.FirstName}{" "}{transaction.User.LastName}", @"\s+", " "),
                        TransactionRefId = transaction.TransactionRefId,
                        TransactionStatus = transaction.TransactionStatus,
                        TransactionDate = transaction.TransactionDate.ToLongDateString(),
                        CreatedAt = transaction.TransactionDate.ToLongDateString(),
                        ModifiedAt = transaction.ModifiedAt.ToLongDateString(),
                        ModifiedBy = transaction.ModifiedBy
                    });
                }
                return transactionDto;
            }
            else
            {
                return "No data found.";
            }
        }
        catch (Exception ex)
        {
            throw new CustomException($"exception: {ex.Message}", "ERR500", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<object> LoadTransactionsToRedisCache()
    {

        //first check if cached list exists
        IList<TransactionRedisModel> cachedTransactions = await _redisTransaction.ToListAsync();

        List<TransactionRedisModel> _redisListModel = new List<TransactionRedisModel>();

        //cache does not exist
        if (!cachedTransactions.Any())
        {
            //first load to cache - from the database - one trip

            List<Transaction> transactions = await _databaseContext.Transactions.ToListAsync();

            foreach (Transaction _transaction_ in transactions)
            {
                _redisListModel.Add(new TransactionRedisModel
                {
                    Id = _transaction_.Id,
                    UserId = _transaction_.UserId,
                    Amount = _transaction_.Amount,
                    TransactionRefId = _transaction_.TransactionRefId,
                    TransactionDate = _transaction_.TransactionDate.ToEastAfricanTime().ToLongDateString(),
                    ModifiedAt = _transaction_.ModifiedAt.ToEastAfricanTime().ToLongDateString(),
                    ModifiedBy = _transaction_.ModifiedBy
                });

                foreach (TransactionRedisModel transactionIntoRedis in _redisListModel)
                {
                    await _redisRepository.InsertTransactionToRedis(transactionIntoRedis);
                }
                //we don't need to return anything here, since we r just loading data to cache. our job is done.
            }
        }
        else
        {
            await ReLoadTransactionsToRedisCache();
        }
        return "cache loaded!";
    }

    public async Task<object> UpdateTransaction(TransactionEditRequest request, Transaction transaction)
    {
        try
        {
            if (request is not null)
            {
                if (string.IsNullOrWhiteSpace(request.Amount.ToString()))
                    request.Amount = transaction.Amount;

                if (string.IsNullOrEmpty(request.ModifiedBy))
                    request.ModifiedBy = transaction.ModifiedBy;

                transaction.ModifiedBy = request.ModifiedBy is not null ? request.ModifiedBy : _constants._defaultActor;
                transaction.ModifiedAt = DateTime.Parse(DateTime.UtcNow.ToEastAfricanTime().ToString("yyyy-MM-dd HH:mm:ss tt"));

                _databaseContext.Entry(transaction).State = EntityState.Modified;

                if (await _databaseContext.SaveChangesAsync() > 0)
                {
                    return "success*transaction updated successfully.";
                }
                else
                    return "error*error updating transaction";
            }
            else
                return $"error*bad request: {HttpStatusCode.BadRequest}";
        }
        catch (Exception ex)
        {
            throw new CustomException($"exception: {ex.Message}", "ERR500", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<object> ReLoadTransactionsToRedisCache()
    {
        List<TransactionRedisModel> cachedTransaction = _redisTransaction.ToList();

        if (cachedTransaction.Any())
        {
            //purge cache first
            foreach (TransactionRedisModel redisTransaction in cachedTransaction)
            {
                _provider.Connection.Unlink($"TRANSACTIONS:{redisTransaction.Id}");
            }
        }

        //then load again - simply refresh
        List<Transaction> _allTransactions = await _databaseContext.Transactions.OrderByDescending(i => i.Id).ToListAsync();

        //auto map list to redis model
        List<TransactionRedisModel> modelToLoadToRedis = _mapper.Map<List<TransactionRedisModel>>(_allTransactions);

        //insert list to cache
        foreach (TransactionRedisModel transaction in modelToLoadToRedis)
        {
            await _redisRepository.InsertTransactionToRedis(transaction);
        }
        return "cache reloaded";
    }

    public async Task<object> CreatePaypalPayment(PaymentModel paymentModel, string baseUrl)
    {
        var payment = _payPalService.CreatePayment(baseUrl, paymentModel);

        var approvalUrl = payment.links.FirstOrDefault(x => x.rel.Equals("approval_url", StringComparison.OrdinalIgnoreCase))?.href;

        //insert transaction with status pending

        TransactionRequest trxRequest = new TransactionRequest()
        {
            UserId = paymentModel.UserId,
            TransactionRefId = payment.token,
            TransactionStatus = nameof(TransactionStatus.Pending),
            Amount = paymentModel.Total,
            ModifiedBy = _constants._defaultActor
        };

        string trxResponse = (string)await CreateTransaction(trxRequest);
        string[] _split = trxResponse.ToString().Split("*");

        if (_split[0] == "success")
        {
            return $"{approvalUrl}";
        }
        else
        {
            return _split[1];
        }
    }

    public object ExcecutePaypalPayment(string paymentId, string token, string payerID)
    {
        //payerId: paypal email of the payer
        var payment = _payPalService.ExecutePayment(paymentId, payerID);

        if (payment.state.ToLower() != "approved")
        {
            return "failed";
        }

        //update transaction
        using (IDbConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            int response = connection.Execute(Queries.UPDATE_TRANSACTION,
                new
                {
                    _transactionStatus = nameof(TransactionStatus.Successful),
                    _modifiedAt = DateTime.UtcNow.ToEastAfricanTime().ToString("yyyy-MM-dd HH:mm:ss tt"),
                    _modifiedBy = _constants._defaultActor,
                    _token = token
                });
            connection.Close();

            if (response > 0)
            {
                return $"success*transaction status updated successfully.";
            }
            else
            {
                return "error*transaction update failed.";
            }
        }
    }

    public object CancelPaypalPayment(string token)
    {
        //update transaction
        using (IDbConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            int response = connection.Execute(Queries.UPDATE_TRANSACTION,
                new
                {
                    _transactionStatus = nameof(TransactionStatus.Cancelled),
                    _modifiedAt = DateTime.UtcNow.ToEastAfricanTime().ToString("yyyy-MM-dd HH:mm:ss tt"),
                    _modifiedBy = _constants._defaultActor,
                    _token = token
                });
            connection.Close();

            if (response > 0)
            {
                return $"success*transaction status updated successfully.";
            }
            else
            {
                return "error*transaction update failed.";
            }
        }
    }
}
