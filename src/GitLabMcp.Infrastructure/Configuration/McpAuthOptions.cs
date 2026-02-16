using System;
using System.Collections.Generic;
using System.Linq;

namespace GitLabMcp.Infrastructure.Configuration;

public sealed class McpAuthOptions
{
    public string? ApiKey { get; set; }
    public string? ApiKeys { get; set; }

    public IReadOnlyCollection<string> GetApiKeys()
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);

        if (!string.IsNullOrWhiteSpace(ApiKey))
        {
            keys.Add(ApiKey.Trim());
        }

        if (!string.IsNullOrWhiteSpace(ApiKeys))
        {
            var split = ApiKeys
                .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value));
            foreach (var key in split)
            {
                keys.Add(key);
            }
        }

        return keys;
    }
}
