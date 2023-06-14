using System.Reflection.Metadata;
using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
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

    public static StringContent ToPayload<T>(this T resource)
            where T : Resource
    {
        var fhirSerializer = new FhirJsonSerializer(new SerializerSettings
        {
            Pretty = true,
            AppendNewLine = true,
        });

        var payload = new StringContent(fhirSerializer.SerializeToString(resource), Encoding.UTF8, "application/json");

        return payload;
    }

    public static StringContent ToPayload(this Bundle bundle)
    {
        var serializer = new FhirJsonSerializer(new SerializerSettings
        {
            IncludeMandatoryInElementsSummary = true,
            Pretty = true,
            AppendNewLine = true,
        });
        var payload = new StringContent(serializer.SerializeToString(bundle), Encoding.UTF8, "application/json");

        return payload;
    }

    public static void ToUpdatedAuditResourceExtensions(this List<Extension> extensions, string name)
    {
        var utcNowWithOffset = new DateTimeOffset(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc));

        var modifiedByExtension = extensions
            .FirstOrDefault(p => p.Url.Equals(ConstantValues.UrlUpdatedByDefinition, StringComparison.InvariantCultureIgnoreCase));

        if (modifiedByExtension != null)
        {
            modifiedByExtension.Value = new HumanName { Text = name };
            return;
        }

        extensions.Add(new Extension(
            url: ConstantValues.UrlUpdatedByDefinition,
            value: new HumanName { Text = name }));
    }

    public static List<EntryResponse>? HandleBundlePersistanceResourceAsync(this string content)
    {
        var fhirJsonParser = new FhirJsonParser(new ParserSettings
        {
            AllowUnrecognizedEnums = true,
            PermissiveParsing = true,
            AcceptUnknownMembers = true
        });

        var transactionResponseBundle = fhirJsonParser.Parse<Bundle>(content);
        var entryResponses = transactionResponseBundle.Entry
            .Where(p => p.Response != null
                && p.TypeName == "Bundle#Entry"
                && p.Response.TypeName == "Bundle#Response")
            .Select(p => new EntryResponse
            {
                Location = ((dynamic)p.Response)?.Location,
                Status = ((dynamic)p.Response)?.Status,
                LastModified = ((dynamic)p.Response)?.LastModified,
                Etag = ((dynamic)p.Response)?.Etag
            })
            .ToList();

        return entryResponses;
    }
    private static void HandleErrors(object source, ExceptionNotification args) => throw new Exception("Fhir deserialization error", args.Exception);
}

