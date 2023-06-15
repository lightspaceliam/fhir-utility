using Common;
using Common.Extensions;
using FhirUtility.Services;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Utility;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

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
var PATIENT_ID = configurationRoot.GetSection("PATIENT_ID")?.Value;

if (string.IsNullOrEmpty(baseUrl)
    || string.IsNullOrEmpty(username)
    || string.IsNullOrEmpty(password)
    || string.IsNullOrEmpty(path)
    || string.IsNullOrEmpty(PATIENT_ID))
{
    Console.WriteLine("Please read the readme for required user secrets properties");
    return;
}

var httpClient = HttpExtensions.CreateHttpClient(baseUrl, username, password);
var runSearchParams = false;
var runBulkDataOp = false;
var runBundleQuery = true;

#region Batch Bundle query strategy.

if (runBundleQuery)
{
    var bundleQeryService = new BundleQueryService();

    var resources = await bundleQeryService.DoWork(httpClient, new List<string> { PATIENT_ID });

    Console.WriteLine($"Entries: {resources?.Count}\n\n");
    
    foreach (var resource in resources)
    {
        Console.WriteLine($"Type: {resource.TypeName}, Id: {resource.Id}");

        if(resource is Patient)
        {
            var patient = (Patient)resource;
            Console.WriteLine($"\n\n{patient.Name.FirstOrDefault().Given.FirstOrDefault()} {patient.Name.FirstOrDefault().Family}\n\n");
        }
    }
}

#endregion

#region Bulk Data Operation

if (runBulkDataOp == true)
{
    //  Use case: update a collection of DetectedIssue/s. Update Status & Extension with updated-by
    

    var detectedIssueService = new DetectedIssueBulkService();
    var stopwatch = new Stopwatch();
    stopwatch.Start();

    var (recordCount, entryResponses) = await detectedIssueService.DoWorkAsync(
        httpClient: httpClient, 
        patientId: PATIENT_ID, 
        findStatus: ObservationStatus.Registered,
        setStatus: ObservationStatus.Preliminary);

    stopwatch.Stop();
    Console.WriteLine($"\n\nTime elapsed: {stopwatch.Elapsed} on {recordCount} {typeof(DetectedIssue).Name}\n\n");

    if (entryResponses.IsNullOrEmpty())
    {
        Console.WriteLine($"{nameof(entryResponses)} is empty.");
        return;
    }

    foreach(var entry in entryResponses)
    {
        Console.WriteLine($"Status: {entry.Status}, Location: {entry.Location}");
    }
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

