namespace AsanaDemo.Controllers.WebhookTransformers
{
    public class Event
    {
        public string action { get; set; }

        public Resource parent { get; set; }

        public Change change { get; set; }

        public DateTime created_at { get; set; }

        public User user { get; set; }

        public Resource resource { get; set; }
    }
}
