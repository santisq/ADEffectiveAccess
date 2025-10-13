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
        string? ctxSchema = root.Properties["schemaNamingContext"][0]?.ToString();
        string? ctxConfig = root.Properties["configurationNamingContext"][0]?.ToString();
        if (ctxSchema is not null) PopulateSchema(ctxSchema, _schemaMap);
        if (ctxConfig is not null) PopulateExtendedRights(ctxConfig, _schemaMap);
    }

    internal string Translate(Guid guid, string defaultValue)
    {
        if (guid == Guid.Empty || _schemaMap.TryGetValue(guid, out defaultValue))
        {
            return defaultValue;
        }

        return guid.ToString();
    }

    private static void PopulateSchema(
        string schemaNamingContext,
        Dictionary<Guid, string> map)
    {
        using DirectoryEntry root = new($"LDAP://{schemaNamingContext}");
        using DirectorySearcher searcher = new(
            searchRoot: root,
            filter: "(&(schemaIdGuid=*)(|(objectClass=attributeSchema)(objectClass=classSchema)))",
            propertiesToLoad: ["cn", "schemaIdGuid"])
        {
            PageSize = 1000
        };

        foreach (SearchResult result in searcher.FindAll())
        {
            map.TryAdd(
                new Guid((byte[])result.Properties["schemaIdGuid"][0]),
                result.Properties["cn"][0].ToString());
        }
    }

    private static void PopulateExtendedRights(
        string configurationNamingContext,
        Dictionary<Guid, string> map)
    {
        using DirectoryEntry root = new($"LDAP://CN=Extended-Rights,{configurationNamingContext}");
        using DirectorySearcher searcher = new(
            searchRoot: root,
            filter: "(objectClass=controlAccessRight)",
            propertiesToLoad: ["cn", "rightsGuid"])
        {
            PageSize = 1000
        };

        foreach (SearchResult result in searcher.FindAll())
        {
            map.TryAdd(
                Guid.Parse(result.Properties["rightsGuid"][0].ToString()),
                result.Properties["cn"][0].ToString());
        }
    }
}
