using System.Text.Json.Nodes;

namespace Elsa.OrchardCore.Client;

public interface IRestApiClient
{
    Task<JsonNode> GetContentItemAsync(string contentItemId, CancellationToken cancellationToken = default);
    Task<JsonNode> PatchContentItemAsync(string contentItemId, PatchContentItemRequest request, CancellationToken cancellationToken = default);
}