using System;
using System.Management.Automation.Runspaces;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ADEffectiveAccess;

internal sealed class RunspaceSpecificStorage<T>(Func<T> factory)
{
    private readonly Func<T> _factory = factory;

    private readonly LazyThreadSafetyMode _mode = LazyThreadSafetyMode.ExecutionAndPublication;

    internal readonly ConditionalWeakTable<Runspace, Lazy<T>> _map = new();

    internal T GetFromTLS() => GetForRunspace(Runspace.DefaultRunspace);

    internal T GetForRunspace(Runspace runspace)
        => _map.GetValue(runspace, _ => new Lazy<T>(() => _factory(), _mode)).Value;

    internal void ClearFromTLS() => _map.Remove(Runspace.DefaultRunspace);
}
