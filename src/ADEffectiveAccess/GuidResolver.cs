using System;
using System.Collections.Generic;
using System.DirectoryServices;

namespace ADEffectiveAccess;

internal sealed class GuidResolver
{
    private readonly Dictionary<string?, Dictionary<Guid, string>> _map = [];

    private Dictionary<Guid, string>? _current;

    private readonly DirectoryEntryBuilder _builder;

    internal GuidResolver(DirectoryEntryBuilder builder) => _builder = builder;

    internal void SetCurrentContext(string? server)
    {
        string path = server is null ? "RootDSE" : $"{server}/RootDSE";
        using DirectoryEntry root = _builder.Create(path);
        string? ctxName = root.Properties["defaultNamingContext"][0]?.ToString();
        if (_map.TryGetValue(ctxName, out Dictionary<Guid, string> current))
        {
            _current = current;
            return;
        }

        current = [];
        string? ctxSchema = root.Properties["schemaNamingContext"][0]?.ToString();
        string? ctxConfig = root.Properties["configurationNamingContext"][0]?.ToString();
        if (ctxSchema is not null) PopulateSchema(ctxSchema, current);
        if (ctxConfig is not null) PopulateExtendedRights(ctxConfig, current);
        _map[ctxName] = current;
        _current = current;
    }

    internal string Translate(Guid guid, string defaultValue)
    {
        if (guid == Guid.Empty || _current!.TryGetValue(guid, out defaultValue))
        {
            return defaultValue;
        }

        return guid.ToString();
    }

    private void PopulateSchema(
        string schemaNamingContext,
        Dictionary<Guid, string> map)
    {
        using DirectoryEntry root = _builder.Create(schemaNamingContext);
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

    private void PopulateExtendedRights(
        string configurationNamingContext,
        Dictionary<Guid, string> map)
    {
        using DirectoryEntry root = _builder.Create($"CN=Extended-Rights,{configurationNamingContext}");
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
