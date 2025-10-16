namespace Restaurant.Utility;

public class PaginatedList<TEntity>
{
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }
    private int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public IEnumerable<TEntity> paginatedData { get; private set; }


    public PaginatedList(IEnumerable<TEntity> items, int count, int pageNumber, int pageSize)
    {
        paginatedData = items;
        TotalCount = count;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}