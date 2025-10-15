using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Management.Automation;
using System.Security.Principal;

namespace ADEffectiveAccess;

internal static class Extensions
{
    internal static bool TryAdd<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue value)
    {
        bool result;
        if (result = !dictionary.ContainsKey(key))
            dictionary.Add(key, value);

        return !result;
    }

    internal static void ThrowGuidResolverError(this Exception exception, PSCmdlet cmdlet)
        => cmdlet.ThrowTerminatingError(
            new ErrorRecord(exception, "CreateGuidResolverError", ErrorCategory.ConnectionError, null));

    internal static void WriteInvalidSecurityDescriptorError(this SearchResult obj, PSCmdlet cmdlet)
        => cmdlet.WriteError(
            new ErrorRecord(
                new InvalidOperationException($"No Security Descriptor found for '{obj.Path}'."),
                "InvalidSecurityDescriptorType", ErrorCategory.InvalidResult, obj));

    internal static void WriteIdentityNotFoundError(this IdentityNotMappedException exception, PSCmdlet cmdlet)
        => cmdlet.WriteError(
            new ErrorRecord(exception, "IvalidIdentity", ErrorCategory.InvalidResult, null));

    internal static void WriteUnderterminedError(this Exception exception, PSCmdlet cmdlet)
        => cmdlet.WriteError(
            new ErrorRecord(exception, "UnderterminedError", ErrorCategory.NotSpecified, null));

    internal static IdentityNotMappedException ToIdentityNotFoundException(this string identity, string? rootDn)
        => new($"Cannot find an object with identity: '{identity}' under: '{rootDn}'.");
}
