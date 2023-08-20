using System.Collections.Generic;

namespace AzureDevOps
{
    public record class jsonContent(
        Dictionary<string, Device>.KeyCollection deviceIds = null);
}