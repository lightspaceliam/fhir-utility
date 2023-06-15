using Common;
using Common.Extensions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;

namespace FhirUtility.Services
{
    public class BundleQueryService
    {
        public async Task<List<Resource>?> DoWork(HttpClient httpClient, List<string> patientIds)
        {
            //  Compose the Id's comma delimited.
            var absolutePatienIds = string.Join(",", patientIds
                .Select(p => $"{typeof(Patient).Name}/{p}")
                .ToList());

            //  Compose the batch query. 
            var batchRequestEntries = new List<Bundle.EntryComponent>
            {
                //  Patient
                new Bundle.EntryComponent
                {
                    Request = new Bundle.RequestComponent
                    {
                        Method = Bundle.HTTPVerb.GET,
                        Url = $"/{typeof(Patient).Name}?_id={absolutePatienIds}"
                    },
                },
                //  Conditions
                new Bundle.EntryComponent
                {
                    Request = new Bundle.RequestComponent
                    {
                        Method = Bundle.HTTPVerb.GET,
                        Url = $"/{typeof(Condition).Name}?patient={absolutePatienIds}"
                    }
                },
                //  DetectedIssue
                new Bundle.EntryComponent
                {
                    Request = new Bundle.RequestComponent
                    {
                        Method = Bundle.HTTPVerb.GET,
                        Url = $"/{typeof(DetectedIssue).Name}?patient={absolutePatienIds}&status={ObservationStatus.Preliminary.ToString().ToLower()}"
                    }
                },
                //  CareTeam
                new Bundle.EntryComponent
                {
                    Request = new Bundle.RequestComponent
                    {
                        Method = Bundle.HTTPVerb.GET,
                        Url = $"/{typeof(CareTeam).Name}?subject.member={absolutePatienIds}"
                    }
                }
            };

            // Create the bundle of type Batch
            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Batch,
                Entry = batchRequestEntries,
            };

            //  Execute the request via Http POST at the root of the base address: {base-url}/
            var content = await HttpExtensions.PostRequestAsync(httpClient, "", bundle.ToPayload<Bundle>());

            // Deserialize to Bundle. The above request will return a Bundle of Bundles.
            var responseBundle = content.ToBundle();

            //  Extract a list of Resource/s.
            var resources = responseBundle.Entry
                .Where(p => !p.IsNullOrEmpty()
                    && !p.Resource.IsNullOrEmpty()
                    && ((Bundle.ResponseComponent)p.Response).Status == "200 OK")
                .SelectMany(p => ((Bundle)p.Resource).Entry
                    .Where(e => !e.IsNullOrEmpty() && e.Count() > 0)
                    .Select(e => e.Resource))
                .ToList();

            return resources;
        }
    }
}
