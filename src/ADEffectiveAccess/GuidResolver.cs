using System;
using System.Collections.Generic;
using System.DirectoryServices;

namespace ADEffectiveAccess;

internal sealed class GuidResolver
{
    private const string DefaultContext = "defaultNamingContext";

    private const string SchemaContext = "schemaNamingContext";

    private const string ConfigurationContext = "configurationNamingContext";

    private readonly Dictionary<string, Dictionary<Guid, string>> _map = [];

    private Dictionary<Guid, string>? _current;

    private static readonly RunspaceSpecificStorage<GuidResolver> _state = new(() => new());

    private GuidResolver() { }

    internal static GuidResolver GetFromTLS() => _state.GetFromTLS();

    internal void SetCurrentContext(string? server, DirectoryEntryBuilder builder)
    {
        string path = server is null ? "RootDSE" : $"{server}/RootDSE";
        using DirectoryEntry root = builder.Create(path);
        string? ctxName = root.GetProperty(DefaultContext)
            ?? throw path.ToInitializeException(DefaultContext);

        if (_map.TryGetValue(ctxName, out Dictionary<Guid, string>? current))
        {
            _current = current;
            return;
        }

        current = [];

        PopulateSchema(
            schemaNamingContext: root.GetProperty(SchemaContext)
                ?? throw path.ToInitializeException(SchemaContext),
            map: current,
            builder: builder);

        PopulateExtendedRights(
            configurationNamingContext: root.GetProperty(ConfigurationContext)
                ?? throw path.ToInitializeException(ConfigurationContext),
            current,
            builder);

        _map[ctxName] = current;
        _current = current;
    }

    internal string Translate(Guid guid, string defaultValue)
    {
        if (guid == Guid.Empty)
        {
            return defaultValue;
        }

        if (_current!.TryGetValue(guid, out string? value))
        {
            return value;
        }

        return guid.ToString();
    }

    private static void PopulateSchema(
        string schemaNamingContext,
        Dictionary<Guid, string> map,
        DirectoryEntryBuilder builder)
    {
        using DirectoryEntry root = builder.Create(schemaNamingContext);
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
                new Guid(result.GetProperty<byte[]>("schemaIdGuid")),
                result.GetProperty<string>("cn"));
        }
    }

    private static void PopulateExtendedRights(
        string configurationNamingContext,
        Dictionary<Guid, string> map,
        DirectoryEntryBuilder builder)
    {
        using DirectoryEntry root = builder.Create($"CN=Extended-Rights,{configurationNamingContext}");
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
                result.GetProperty<Guid>("rightsGuid"),
                result.GetProperty<string>("cn"));
        }
    }
}
