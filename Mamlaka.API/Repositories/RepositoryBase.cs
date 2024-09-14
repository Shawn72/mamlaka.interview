using System.Net;
using Mamlaka.API.Exceptions;
using Mamlaka.API.Interfaces;
using Mamlaka.API.DAL.DbContexts;
using Mamlaka.API.DAL.Entities.Transactions;

namespace Mamlaka.API.Repositories;
public class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    protected MySqlDbContext _context { get; set; }

    public RepositoryBase(MySqlDbContext repoContext)
    {
        _context = repoContext;
    }

    public IQueryable<T> GetPagedTransactionList()
    {
        try
        {
            IQueryable<Transaction> _transactions = _context.Transactions.OrderByDescending(i => i.Id)
               .ToList().AsQueryable();

            return (IQueryable<T>)_transactions;
        }

        catch (Exception ex)
        {
            throw new CustomException($"exception occured: {ex.Message}", "ERR500", HttpStatusCode.InternalServerError);
        }
    }
}
