using System.Management.Automation;

namespace ADEffectiveAccess;

public sealed class ADObjectTransform : ArgumentTransformationAttribute
{
    public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
    {
        if (inputData is not PSObject pso)
        {
            return LanguagePrimitives.ConvertTo<string>(inputData);
        }

        PSMemberInfoCollection<PSPropertyInfo> properties = pso.Properties;
        object? value = properties["ObjectGuid"]?.Value ?? properties["DistinguishedName"]?.Value;
        return value is not null
            ? LanguagePrimitives.ConvertTo<string>(value)
            : throw inputData.ToInvalidIdentityException();
    }
}
