using Common;
using Common.Extensions;
using Hl7.Fhir.Model;
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

//  Read the JSON file for all required custom Search Parameters to a bundle. 
var bundle = await path.ReadDataFileAsync();

//  Cast to a List of type SearchParameter.
var searchParameters = bundle.ToFhirResourceWherePosible<SearchParameter>();

//  Comma delimited string of Id for the Fhir Service query. This is MVP. There may come a time when there are too many and a batching strategy will be required.
var ids = string.Join(',', searchParameters
    .Select(p => p.Id)
    .ToList());

var httpClient = HttpExtensions.CreateHttpClient(baseUrl, username, password);

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

//  Done.
Console.WriteLine("\n\nFhir Utility completed");
Console.ReadKey();

