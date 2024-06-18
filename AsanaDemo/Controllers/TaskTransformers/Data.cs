namespace AsanaDemo.Controllers.TaskTransformers
{
    public class Data
    {
        public string gid { get; set; }
        public object actual_time_minutes { get; set; }
        public object assignee { get; set; }
        public string assignee_status { get; set; }
        public bool completed { get; set; }
        public object completed_at { get; set; }
        public DateTime created_at { get; set; }
        public List<CustomField> custom_fields { get; set; }
        public object due_at { get; set; }
        public string due_on { get; set; }
        public List<Follower> followers { get; set; }
        public bool hearted { get; set; }
        public List<object> hearts { get; set; }
        public bool liked { get; set; }
        public List<object> likes { get; set; }
        public List<Membership> memberships { get; set; }
        public DateTime modified_at { get; set; }
        public string name { get; set; }
        public string notes { get; set; }
        public int num_hearts { get; set; }
        public int num_likes { get; set; }
        public object parent { get; set; }
        public string permalink_url { get; set; }
        public List<Project> projects { get; set; }
        public string resource_type { get; set; }
        public object start_at { get; set; }
        public string start_on { get; set; }
        public List<object> tags { get; set; }
        public string resource_subtype { get; set; }
        public Workspace workspace { get; set; }
    }
}
