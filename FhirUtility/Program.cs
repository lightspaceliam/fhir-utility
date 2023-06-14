using Common;
using Common.Extensions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using Microsoft.Extensions.Configuration;

//  Register access to User Secrets.
var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
var builder = new ConfigurationBuilder()
            .AddUserSecrets<Program>();
            
var configurationRoot = builder.Build();

//  Extract User Secrets and warn if they don't exist.
var baseUrl = configurationRoot.GetSection("Fhir:Base_Url")?.Value;
var username = configurationRoot.GetSection("Fhir:username")?.Value;
var password = configurationRoot.GetSection("Fhir:password")?.Value;
var path = configurationRoot.GetSection("SearchParameters_Data_Path")?.Value;

if (string.IsNullOrEmpty(baseUrl)
    || string.IsNullOrEmpty(username)
    || string.IsNullOrEmpty(password)
    || string.IsNullOrEmpty(path))
{
    Console.WriteLine("Please read the readme for required user secrets properties");
    return;
}
var httpClient = HttpExtensions.CreateHttpClient(baseUrl, username, password);
var runSearchParams = false;
var runBulkDataOp = true;

#region Bulk Data Operation

if(runBulkDataOp == true)
{
    //  Use case: update a collection of DetectedIssue/s. Update Status & Extension with updated-by
    const string PATIENT_ID = "9e1ab93f-ce36-4028-88bb-aff23a433d55";
    const string SITE_ID = "fadf8070-2b16-412e-b1d6-fad64e4d7afe";
    var preliminaryStatus = ObservationStatus.Preliminary.ToString().ToLower();
    var registeredStatus = ObservationStatus.Registered.ToString().ToLower();

    //  Get the DetectedIssue/s data by Patient.Id
    var detetedIssueQuery = $"{typeof(DetectedIssue).Name}?patient={typeof(Patient).Name}/{PATIENT_ID}&status={preliminaryStatus}";
    var content = await HttpExtensions.GetRequestAsync(httpClient, detetedIssueQuery);

    if (string.IsNullOrEmpty(content))
    {
        Console.WriteLine("Bulk update DetectedIssue date not found.");
        return;
    }

    var detectedIssues = content
        .ToBundle()
        .ToFhirResourceWherePosible<DetectedIssue>();

    var bundle = new Bundle
    {
        Type = Bundle.BundleType.Transaction,
    };
    var entries = new List<Bundle.EntryComponent>();

    //  Update each record and add it back to the bundle for bulk update.
    foreach(var detectedIssue in detectedIssues)
    {
        Console.WriteLine($"Id: {detectedIssue.Id}");
        var absoluteUrl = $"{httpClient.BaseAddress}{typeof(DetectedIssue).Name}/{detectedIssue.Id}";
        var absoluteResourceId = $"{typeof(DetectedIssue).Name}/{detectedIssue.Id}";
        var uuid = $"urn:uuid:{detectedIssue.Id}";

        //  You could do this however you can't add request.
        
        detectedIssue.Status = ObservationStatus.Registered;
        detectedIssue.Extension.ToUpdatedAuditResourceExtensions("FhirUtility-Hando");
        bundle.AddResourceEntry(detectedIssue, uuid);

        //  Why is this not working (persisting the bundle)? What is wrong witn my composition of the Bundle? 
        //var entry = new Bundle.EntryComponent
        //{
        //    FullUrl = absoluteUrl,
        //    Request = new Bundle.RequestComponent
        //    {
        //        Method = Bundle.HTTPVerb.PUT,
        //        Url = absoluteResourceId
        //    },
        //    Resource = detectedIssue
        //};
        //entries.Add(entry);
    }
    //  Add the entries back to the bundle.
    bundle.Entry = entries;

    //  When persisting with a bundle of type "Transaction" the Url must be {baseUrl}/ There is no reference to a specific Fhir Resource.
    //  POST off to the Fhir Service
    content = await HttpExtensions.PutRequestAsync(httpClient, "", bundle.ToPayload());
    var entryResponses = content.HandleBundlePersistanceResourceAsync();

    if (entryResponses.IsNullOrEmpty())
    {
        Console.WriteLine($"{nameof(entryResponses)} is empty.");
        return;
    }

    foreach(var entry in entryResponses)
    {
        Console.WriteLine($"Status: {entry.Status}, Location: {entry.Location}");
    }
    //  add to HttpClient to persist
}

#endregion
#region SearchParameter.

if (runSearchParams == true)
{
    //  Read the JSON file for all required custom Search Parameters to a bundle. 
    var bundle = await path.ReadDataFileAsync();

    //  Cast to a List of type SearchParameter.
    var searchParameters = bundle.ToFhirResourceWherePosible<SearchParameter>();

    //  Comma delimited string of Id for the Fhir Service query. This is MVP. There may come a time when there are too many and a batching strategy will be required.
    var ids = string.Join(',', searchParameters
        .Select(p => p.Id)
        .ToList());

    

    //  Execute the request.
    var content = await httpClient.GetRequestAsync($"{typeof(SearchParameter).Name}?_id={ids}");

    //  Cast the response back to a List of type SearchParameter.
    var response = content
        .ToBundle()
        .ToFhirResourceWherePosible<SearchParameter>();

    //  Calculate missing SearchParameter/s.
    var missingSearchParameters = response
        .Where(p => ids.Contains(p.Id))
        .ToList();

    //  Write results.
    Console.WriteLine($"Environment: {baseUrl}\n" +
        $"Custom {typeof(SearchParameter).Name}'s: {searchParameters.Count}\n" +
        $"Missing {typeof(SearchParameter).Name}'s: {missingSearchParameters.Count}\n\n");

    //  Write individual missing SearchParameter/s.
    var i = 1;
    foreach (var searchParameter in missingSearchParameters)
    {
        Console.WriteLine($"Nbr: {i} Id: {searchParameter.Id}, Code: {searchParameter.Code}, Resource: {string.Join(", ", searchParameter.Base)}");
        i++;
    }
}


#endregion

//  Done.
Console.WriteLine("\n\nFhir Utility completed");
Console.ReadKey();

