using Elsa.Agents.Persistence.Entities;

namespace Elsa.Agents.Persistence.Filters;

public class ApiKeyDefinitionFilter
{
    public string? Id { get; set; }
    public string? NotId { get; set; }
    public string? Name { get; set; }
    
    public IQueryable<ApiKeyDefinition> Apply(IQueryable<ApiKeyDefinition> queryable)
    {
        if (!string.IsNullOrWhiteSpace(Id)) queryable = queryable.Where(x => x.Id == Id);
        if (!string.IsNullOrWhiteSpace(NotId)) queryable = queryable.Where(x => x.Id != NotId);
        if (!string.IsNullOrWhiteSpace(Name)) queryable = queryable.Where(x => x.Name == Name);
        return queryable;
    }
}