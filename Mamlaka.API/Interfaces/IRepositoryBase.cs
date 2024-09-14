namespace Mamlaka.API.Interfaces;
public interface IRepositoryBase<T>
{
    IQueryable<T> GetPagedTransactionList();
}
