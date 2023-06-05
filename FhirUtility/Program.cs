// See https://aka.ms/new-console-template for more information
using Common;
using Common.Extensions;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Configuration;

var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
var builder = new ConfigurationBuilder()
            .AddUserSecrets<Program>();
            
var configurationRoot = builder.Build();

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

var bundle = await path.ReadDataFileAsync();

var searchParameters = bundle.ToFhirResourceWherePosible<SearchParameter>();

var ids = string.Join(',', searchParameters
    .Select(p => p.Id)
    .ToList());

var httpClient = HttpExtensions.CreateHttpClient(baseUrl, username, password);
var content = await httpClient.GetRequestAsync($"{typeof(SearchParameter).Name}?_id={ids}");

var response = content
    .ToBundle()
    .ToFhirResourceWherePosible<SearchParameter>();

var missingSearchParameters = response
    .Where(p => ids.Contains(p.Id))
    .ToList();

Console.WriteLine($"Environment: {baseUrl}\n" +
    $"Custom {typeof(SearchParameter).Name}'s: {searchParameters.Count}\n" +
    $"Missing {typeof(SearchParameter).Name}'s: {missingSearchParameters.Count}\n\n");

var i = 1;
foreach (var searchParameter in missingSearchParameters)
{
    Console.WriteLine($"Nbr: {i} Id: {searchParameter.Id}, Code: {searchParameter.Code}, Resource: {string.Join(", ", searchParameter.Base)}");
    i++;
}

Console.WriteLine("\n\nFhir Utility completed");
Console.ReadKey();

