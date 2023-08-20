using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AzureDevOps
{
    public record class Assets(
        [property: JsonPropertyName("devices")] List<Asset> devices
    );
}
