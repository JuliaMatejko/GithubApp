using System.Collections.Generic;

namespace GitHubApp.Models
{
    public class IndexViewModel
    {
        public IndexViewModel(string login, string fullAccountName, IEnumerable<UserRepository> repositories, Dictionary<string, float> tlangPercUsage)
        {
            Login = login;
            FullAccountName = fullAccountName;
            Repositories = repositories;
            PercentageOfLanguageUse = tlangPercUsage;
        }

        public IEnumerable<UserRepository> Repositories { get; private set; }
        public string Login { get; set; }
        public string FullAccountName { get; set; }
        public Dictionary<string, float> PercentageOfLanguageUse { get; set; }

    }
}