using Mamlaka.API.CommonObjects.Requests;
using Mamlaka.API.DAL.Entities.Transactions;
using Mamlaka.API.DAL.Models;

namespace Mamlaka.API.Interfaces;
public interface ITransactionRepository
{
    Task<object> CreateTransaction(TransactionRequest request);
    Task <Transaction> GetTransaction(long transactionId);
    Task<object> GetTransactionsList();
    Task<object> UpdateTransaction(TransactionEditRequest request, Transaction transaction);
    Task<object> DeleteTransaction(Transaction transaction);
    Task<object> LoadTransactionsToRedisCache();
    Task<object> CreatePaypalPayment(PaymentModel paymentModel, string baseUrl);
    object ExcecutePaypalPayment(string paymentId, string token, string payerID);
    object CancelPaypalPayment(string token);
}
