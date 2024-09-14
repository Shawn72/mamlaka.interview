using Mamlaka.API.Paging;

namespace Mamlaka.API.Interfaces;
public interface ITransactionPagination
{
    PagedList<object> GetPageTransactionsList(PagingParameters pagingParameters);
    (int count, int totalPages) GetTransactionPageDetails(int pageSize);
}
