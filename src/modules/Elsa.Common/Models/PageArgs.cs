namespace Elsa.Common.Models;

public record PageArgs(int? Page, int? PageSize)
{
    public int? Offset => Page * PageSize;
    public int? Limit => PageSize;

    public PageArgs Next() => this with { Page = Page + 1 };
}