using Common;
using Common.Extensions;
using Hl7.Fhir.Model;

namespace FhirUtility.Services
{
    public class SearchParameterService
    {
        public async Task<List<SearchParameter>?> DoWork(HttpClient httpClient, string dataFilePath)
        {
            var bundle = await dataFilePath.ReadDataFileAsync();

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

            return missingSearchParameters;
        }
    }
}
