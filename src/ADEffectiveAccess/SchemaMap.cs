using System;
using System.Collections.Generic;
using System.DirectoryServices;

namespace ADEffectiveAccess;

internal sealed class SchemaMap
{
    private readonly Dictionary<Guid, string> _schemaMap = [];

    internal SchemaMap(string? server = null)
    {
        string path = server is null ? "LDAP://RootDSE" : $"LDAP://{server}/RootDSE";
        using DirectoryEntry root = new(path);
        string? ctx = root.Properties["schemaNamingContext"][0]?.ToString();
        if (ctx is not null) PopulateMap(ctx, _schemaMap);
    }

    internal string Translate(Guid guid, string defaultValue)
    {
        if (guid == Guid.Empty || _schemaMap.TryGetValue(guid, out defaultValue))
        {
            return defaultValue;
        }

        return guid.ToString();
    }

    private static void PopulateMap(
        string schemaNamingContext,
        Dictionary<Guid, string> map)
    {
        using DirectoryEntry root = new($"LDAP://{schemaNamingContext}");
        using DirectorySearcher searcher = new(
            searchRoot: root,
            filter: "(schemaIDGUID=*)",
            propertiesToLoad: ["cn", "schemaIDGuid"]);

        foreach (SearchResult result in searcher.FindAll())
        {
            map.Add(
                new Guid((byte[])result.Properties["schemaIDGUID"][0]),
                result.Properties["cn"][0].ToString());
        }
    }
}
