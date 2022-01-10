using GitHubApp.Models;
using Microsoft.AspNetCore.Mvc;
using Octokit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHubApp.Controllers
{
    public class HomeController : Controller
    {
        // !!! TO DO:
        //
        // 1. Register an application at https://github.com/settings/applications/new to get values for clientId and clientSecret fields.
        // 2. Create CLIENT_ID and CLIENT_SECRET user enviroment variables.
        // 3. Set user enviroment variables values with the values from your application registration.
        // *4. After changing enviroment variables you might need to restart Visual Studio, 
        //     because it doesn't detect changes to the environment variables while it is running.

        readonly string clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
        private readonly string clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");

        readonly GitHubClient client = new (new ProductHeaderValue("GitHubApp"), new Uri("https://github.com/"));

        // GET
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST 
        [HttpPost]
        public async Task<ActionResult> Index(string login)
        {
            var accessToken = (string)TempData["OAuthToken"];
            if (accessToken != null)
            {
                // This allows the client to make requests to the GitHub API on the user's behalf
                // without ever having the user's OAuth credentials.
                client.Credentials = new Credentials(accessToken);

                if (!string.IsNullOrEmpty(login) || !string.IsNullOrWhiteSpace(login))
                {
                    try
                    {
                        var gitHubRepositories = await client.Repository.GetAllForUser(login);

                        List<UserRepository> userRepositories = new();

                        Dictionary<string, long> languagesBytesTotal = new();

                        foreach (var repo in gitHubRepositories)
                        {
                            var languagesBytesRepo = await client.Repository.GetAllLanguages(repo.Id);

                            UserRepository userRepository = new() { Name = repo.Name, Languages = CalculatePercentageOfLanguageUse(languagesBytesRepo) };

                            userRepositories.Add(userRepository);

                            foreach (var lang in languagesBytesRepo)
                            {
                                if (!languagesBytesTotal.ContainsKey(lang.Name))
                                {
                                    languagesBytesTotal.Add(lang.Name, lang.NumberOfBytes);
                                }
                                else
                                {
                                    languagesBytesTotal[lang.Name] += lang.NumberOfBytes;
                                }
                            }
                        }

                        var user = await client.User.Get(login);

                        var model = new IndexViewModel(user.Login, user.Name, userRepositories, CalculateTotalPercentageOfLanguageUse(languagesBytesTotal));

                        return View(model);
                    }
                    catch (AuthorizationException)
                    {
                        // Either the accessToken is null or it's invalid. This redirects
                        // to the GitHub OAuth login page. That page will redirect back to the
                        // Authorize action.
                        return Redirect(GetOauthLoginUrl());
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return Redirect(GetOauthLoginUrl());
        }

        // This is the Callback URL that the GitHub OAuth Login page will redirect back to.
        public async Task<ActionResult> Authorize(string code, string state)
        {
            if (!string.IsNullOrEmpty(code))
            {
                var expectedState = TempData["CSRF:State"] as string;
                if (state != expectedState) throw new InvalidOperationException("SECURITY FAIL!");
                TempData["CSRF:State"] = null;

                var token = await client.Oauth.CreateAccessToken(
                    new OauthTokenRequest(clientId, clientSecret, code));
                TempData["OAuthToken"] = token.AccessToken;
            }

            return RedirectToAction(nameof(Index));
        }

        private string GetOauthLoginUrl()
        {
            string csrf = Password.Generate(24, 1);

            TempData["CSRF:State"] = csrf;

            // Redirect users to request GitHub access.
            var request = new OauthLoginRequest(clientId)
            {
                Scopes = { },   // Here you can set scopes that will be requested, default: only public data.
                State = csrf
            };
            var oauthLoginUrl = client.Oauth.GetGitHubLoginUrl(request);
            return oauthLoginUrl.ToString();
        }

        private static Dictionary<string, float> CalculateTotalPercentageOfLanguageUse(Dictionary<string, long> languagesBytes)
        {
            Dictionary<string, float> languagePercentageUsage = new();

            long sum = 0;
            foreach (var lang in languagesBytes)
            {
                sum += lang.Value;
            }

            if (sum == 0)
            {
                return languagePercentageUsage;
            }

            foreach (var lang in languagesBytes)
            {
                languagePercentageUsage.Add(lang.Key, (float)lang.Value / sum * 100);
            }

            return languagePercentageUsage;
        }

        private static Dictionary<string, float> CalculatePercentageOfLanguageUse(IReadOnlyList<RepositoryLanguage> languagesBytes)
        {
            Dictionary<string, float> languagePercentageUsage = new();

            long sum = 0;
            foreach (var lang in languagesBytes)
            {
                sum += lang.NumberOfBytes;
            }

            if (sum == 0)
            {
                return languagePercentageUsage;
            }

            foreach (var lang in languagesBytes)
            {
                languagePercentageUsage.Add(lang.Name, (float)lang.NumberOfBytes / sum * 100);
            }

            return languagePercentageUsage;
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
