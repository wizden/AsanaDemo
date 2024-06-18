namespace AsanaDemo.Controllers.TaskTransformers
{
    public class CustomField
    {
        public string gid { get; set; }

        public bool enabled { get; set; }

        public List<EnumOption> enum_options { get; set; }

        public EnumValue enum_value { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public CreatedBy created_by { get; set; }

        public string display_value { get; set; }

        public string resource_subtype { get; set; }

        public string resource_type { get; set; }

        public bool is_formula_field { get; set; }

        public bool is_value_read_only { get; set; }

        public string type { get; set; }

    }
}
