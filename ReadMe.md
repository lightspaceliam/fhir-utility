# Fhir Utility

## Search Parameters

As we deploy across mutiple environments, we want to ensure our SmileCDR custom SearchParameter's are consistent as well.

You will need to add a JSON file that once read, deserializes into a Hl7 Bundle containing a collection of Resources of type `SearchParameter`. This is the list of custom search parameters you want to check exist accross each Fhir Environment. 

**User Secrets:**

As we don't want to share our secrets, such as:

1. Fhir Service base URL
2. Username, Password
3. JSON file we use as a record of SearchParameters we have dependancies on and need to keep consistent accross multiple environments

You will need to manage the following User Seecrets:

```json
{
    "Fhir": {
        "Base_Url": "",
        "Username": "",
        "Password": ""
    },
    "PATIENT_ID": "",
    "SearchParameters_Data_Path": ""
}
```