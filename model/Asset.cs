using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AzureDevOps
{
    public record class Asset(
        [property: JsonPropertyName("deviceId")] string deviceId,
        [property: JsonPropertyName("assetId")] string assetId
    );
}