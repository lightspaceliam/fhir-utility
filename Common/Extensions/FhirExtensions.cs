using System.Reflection.Metadata;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;

namespace Common;

public static class FhirExtensions
{
    public static Bundle ToBundle(this string content)
    {
        var fhirJsonParser = new FhirJsonParser(new ParserSettings
        {
            AllowUnrecognizedEnums = true,
            PermissiveParsing = true,
            AcceptUnknownMembers = true,
            ExceptionHandler = HandleErrors
        });

        var bundle = fhirJsonParser.Parse<Bundle>(content);
        return bundle;
    }

    public static List<T> ToFhirResourceWherePosible<T>(this Bundle bundle)
        where T : Resource
    {
        if (bundle == null || bundle?.Entry == null || !bundle.Entry.Any()) return new();

        var resources = bundle.Entry
            .Where(p => p.Resource != null
                && p.Resource is T)
            .Select(p => (T)p.Resource)
            .ToList();

        return resources;
    }

    private static void HandleErrors(object source, ExceptionNotification args) => throw new Exception("Fhir deserialization error", args.Exception);
}

