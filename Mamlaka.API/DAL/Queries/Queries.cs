namespace Mamlaka.API.DAL.Queries;
public partial class Queries
{
    public const string UPDATE_TRANSACTION = "UPDATE Transactions SET TransactionStatus = @_transactionStatus, ModifiedAt = @_modifiedAt, ModifiedBy = @_modifiedBy WHERE TransactionRefId = @_token";
}
