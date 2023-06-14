namespace Common
{
    public static class ConstantValues
    {
        private const string FHIR_RESOURCE_BASE = "api/resource/";

        public readonly static string FHIR_RESOURCE_PATIENT_AREA = $"{FHIR_RESOURCE_BASE}patients";
        public readonly static string FHIR_RESOURCE_EPISODE_OF_CARE_AREA = $"{FHIR_RESOURCE_BASE}episodeofcare";
        public readonly static string FHIR_RESOURCE_CARE_PLAN_AREA = $"{FHIR_RESOURCE_BASE}care-plan";
        public readonly static string FHIR_RESOURCE_GROUPS_AREA = $"{FHIR_RESOURCE_BASE}groups";
        public readonly static string FHIR_RESOURCE_CARE_TEAM_AREA = $"{FHIR_RESOURCE_BASE}care-teams";
        public readonly static string FHIR_RESOURCE_TASK_AREA = $"{FHIR_RESOURCE_BASE}task";
        public readonly static string FHIR_RESOURCE_CONDITION_AREA = $"{FHIR_RESOURCE_BASE}conditions";
        public readonly static string FHIR_RESOURCE_PRACTITIONER_AREA = $"{FHIR_RESOURCE_BASE}practitioners";
        public readonly static string FHIR_RESOURCE_DETECTED_ISSUE_AREA = $"{FHIR_RESOURCE_BASE}detected-issues";
        public readonly static string FHIR_RESOURCE_DOCUMENT_REFERENCE_AREA = $"{FHIR_RESOURCE_BASE}document-references";
        public readonly static string FHIR_RESOURCE_OBSERVATION_AREA = $"{FHIR_RESOURCE_BASE}observation";
        public readonly static string FHIR_RESOURCE_ORGANIZATION_AREA = $"{FHIR_RESOURCE_BASE}organizations";
        public readonly static string FHIR_RESOURCE_PLAN_DEFINITION_AREA = $"{FHIR_RESOURCE_BASE}plan-definitions";
        public readonly static string FHIR_RESOURCE_PRACTITIONER_ROLE_AREA = $"{FHIR_RESOURCE_BASE}practitioner-roles";
        public readonly static string FHIR_RESOURCE_QUESTIONNAIRE_AREA = $"{FHIR_RESOURCE_BASE}questionnaire";
        public readonly static string FHIR_RESOURCE_RELATED_PEOPLE_AREA = $"{FHIR_RESOURCE_BASE}related-people";
        public readonly static string FHIR_RESOURCE_TRIAGE_DASHBOARD_AREA = $"{FHIR_RESOURCE_BASE}triage-dashboard";
        public readonly static string FHIR_RESOURCE_FLAG_AREA = $"{FHIR_RESOURCE_BASE}flag";

        private const string URL_TELSTRA_HEALTH = "http://health.telstra.com/";

        public static readonly string UrlUpdatedByDefinition = $"{URL_TELSTRA_HEALTH}StructureDefinition/updated-by";
    }
}
