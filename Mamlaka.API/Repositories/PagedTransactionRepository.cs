using Mamlaka.API.Paging;
using Mamlaka.API.Interfaces;
using Mamlaka.API.DAL.DbContexts;
using Mamlaka.API.DAL.Entities.Transactions;

namespace Mamlaka.API.Repositories;
public class PagedTransactionRepository<T> : RepositoryBase<Transaction>, ITransactionPagination
{
    public PagedTransactionRepository(
        MySqlDbContext context) : base(context) { }

    public (int count, int totalPages) GetTransactionPageDetails(int pageSize)
    {
        return PagedList<Transaction>.GetPageDetails(GetPagedTransactionList(), pageSize);
    }

    public PagedList<object> GetPageTransactionsList(PagingParameters pagingParameters)
    {
        return PagedList<object>.ToPagedList(GetPagedTransactionList(), pagingParameters.PageNumber, pagingParameters.PageSize);
    }
}
