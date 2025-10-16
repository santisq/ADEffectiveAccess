using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Management.Automation;
using System.Security.Principal;
using System.Text;

namespace ADEffectiveAccess;

internal static class Extensions
{
#if !NETCOREAPP
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
#endif

    internal static string ToFilter(this Guid guid)
    {
        StringBuilder builder = new("(objectGuid=", capacity: 61);
        foreach (byte b in guid.ToByteArray())
        {
            builder.Append($"\\{b:X2}");
        }

        return builder.Append(')').ToString();
    }

    internal static string ToFilter(this SecurityIdentifier sid) => $"(objectSid={sid})";

    internal static string ToFilter(this string identity)
        => identity.Contains("=")
            ? $"(distinguishedName={identity})"
            : $"(samAccountName={identity})";

    internal static string? GetProperty(this DirectoryEntry entry, string property)
    {
        if (!entry.Properties.Contains(property))
        {
            return null;
        }

        return entry.Properties[property][0]?.ToString();
    }

    internal static T GetProperty<T>(this SearchResult search, string property)
        => LanguagePrimitives.ConvertTo<T>(search.Properties[property][0]);

    internal static bool TryGetProperty<T>(
        this SearchResult search,
        string property,
        [NotNullWhen(true)] out T? value)
    {
        value = default;
        if (!search.Properties.Contains(property))
        {
            return false;
        }

        return LanguagePrimitives.TryConvertTo(search.Properties[property][0], out value);
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

    internal static ArgumentTransformationMetadataException ToInvalidIdentityException(this object input)
        => new(
            $"Could not convert input '{LanguagePrimitives.ConvertTo<string>(input)}' to a valid Identity. " +
            "Expected 'ObjectGuid' or 'DistinguishedName' to be present.");

    internal static InvalidOperationException ToInitializeException(this string path, string attribute)
        => new(
            "Failed to initialize GuidResolver: " +
            $"The '{attribute}' attribute is missing or null in the RootDSE response for path '{path}'.");
}
