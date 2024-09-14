namespace Mamlaka.API.Paging;
public class PagingParameters
{
    const int maxPageSize = 500;
    public int PageNumber { get; set; } = 1;

    private int _pageSize = 8;
    public int PageSize
    {
        get { return _pageSize; }
        set { _pageSize = (value > maxPageSize) ? maxPageSize : value; }
    }
}
