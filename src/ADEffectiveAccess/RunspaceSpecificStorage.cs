using System;
using System.Management.Automation.Runspaces;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ADEffectiveAccess;

internal class RunspaceSpecificStorage<T>(Func<T> factory)
{
    private readonly ConditionalWeakTable<Runspace, Lazy<T>> _map = new();

    private readonly Func<T> _factory = factory;

    private readonly LazyThreadSafetyMode _mode = LazyThreadSafetyMode.ExecutionAndPublication;

    internal T GetFromTLS() => GetForRunspace(Runspace.DefaultRunspace);

    internal T GetForRunspace(Runspace runspace)
        => _map.GetValue(runspace, _ => new Lazy<T>(() => _factory(), _mode)).Value;
}
