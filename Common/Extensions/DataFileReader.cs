using System;
using Hl7.Fhir.Model;

namespace Common.Extensions
{
	public static class DataFileReader
	{
        public static async Task<Bundle> ReadDataFileAsync(this string path)
        {
            using (var streamReader = new StreamReader(path))
            {
                var jsonString = await streamReader.ReadToEndAsync();
                var bundle = jsonString.ToBundle();

                return bundle;
            }
        }
    }
}

