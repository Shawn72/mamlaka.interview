using Mamlaka.API.Paging;

namespace Mamlaka.API.DAL.DTOs;
public class PaginationDto
{
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
    public PagedList<object> Data { get; set; }
}
