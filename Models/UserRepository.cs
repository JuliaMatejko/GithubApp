using System.Collections.Generic;

namespace GitHubApp.Models
{
    public class UserRepository
    {
        public string Name { get; set; }
        public Dictionary<string, float> Languages { get; set; } = new();
    }
}
