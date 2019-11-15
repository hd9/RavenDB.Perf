namespace Perf.Core.Entities
{
    public class User
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string FullName() => $"{FirstName} {LastName}";
    }
}
