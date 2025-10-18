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

    internal void SetContext(string? server, DirectoryEntryBuilder builder)
    {
        using DirectoryEntry rootDSE = builder.Create(server, "RootDSE");
        string context = rootDSE.GetRootProperty(DefaultContext);

        if (_map.TryGetValue(context, out Dictionary<Guid, string>? current))
        {
            _current = current;
            return;
        }

        _current = _map[context] = [];
        string schemaNamingContext = rootDSE.GetRootProperty(SchemaContext);
        string extendedRights = $"CN=Extended-Rights,{rootDSE.GetRootProperty(ConfigurationContext)}";

        PopulateSchema(builder.Create(server, schemaNamingContext), _current);
        PopulateExtendedRights(builder.Create(server, extendedRights), _current);
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

    private static void PopulateSchema(DirectoryEntry root, Dictionary<Guid, string> map)
    {
        using (root)
        {
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
    }

    private static void PopulateExtendedRights(DirectoryEntry root, Dictionary<Guid, string> map)
    {
        using (root)
        {
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
}
