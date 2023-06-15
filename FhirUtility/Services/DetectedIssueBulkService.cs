using Common;
using Common.Extensions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System.Net;

namespace FhirUtility.Services
{
    public class DetectedIssueBulkService
    {
        public async Task<(int recordCount, List<EntryResponse>? entityResponses)> DoWorkAsync(HttpClient httpClient, string patientId, ObservationStatus findStatus, ObservationStatus setStatus)
        {
            //  Get the DetectedIssue/s data by Patient.Id & Status.
            var detetedIssueQuery = $"{typeof(DetectedIssue).Name}?patient={typeof(Patient).Name}/{patientId}&status={findStatus.ToString().ToLower()}";
            var content = await HttpExtensions.GetRequestAsync(httpClient, detetedIssueQuery);

            if (string.IsNullOrEmpty(content))
                throw new FhirOperationException("Bulk update DetectedIssue date not found.", HttpStatusCode.BadRequest);

            var detectedIssues = content
                .ToBundle()
                .ToFhirResourceWherePosible<DetectedIssue>();

            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Transaction,
            };

            var entries = new List<Bundle.EntryComponent>();

            //  Update each record and add it back to the bundle for bulk update.
            foreach (var detectedIssue in detectedIssues)
            {
                Console.WriteLine($"{typeof(DetectedIssue).Name}.Id: {detectedIssue.Id}");

                var absoluteUrl = $"{httpClient.BaseAddress}{typeof(DetectedIssue).Name}/{detectedIssue.Id}";
                var absoluteResourceId = $"{typeof(DetectedIssue).Name}/{detectedIssue.Id}";

                //  Update status & Auditing.
                detectedIssue.Status = setStatus;
                detectedIssue.Extension.ToUpdatedAuditResourceExtensions("Bulk-data-user-Fhir-Utility-Hando");
        
                var entry = new Bundle.EntryComponent
                {
                    FullUrl = absoluteUrl,
                    Request = new Bundle.RequestComponent
                    {
                        Method = Bundle.HTTPVerb.PUT,
                        Url = absoluteResourceId
                    },
                    Resource = detectedIssue
                };
                entries.Add(entry);
            }

            //  Add the entries back to the bundle.
            bundle.Entry = entries;

            //  ALL DATA OPERATIONS WITH BUNDLE NEED TO BE POSTED AT THE ROOT/BASE URL {BASE-URL}/  of type TRANSACTION  - unless type: COLLECTION
            content = await HttpExtensions.PostRequestAsync(httpClient, "", bundle.ToPayload());
            var entryResponses = content.HandleBundlePersistanceResourceAsync();

            return (detectedIssues.Count, entryResponses);
        }
    }
}
