# GithubApp

GitHub API / OAuth / Octokit.net

### Application

This app gets informations about user repositories (repository name and used languages) from GitHub API and calculates percentage of language usage among all repositories.

### Application start-up instruction on localhost

1. Download repository content

2. Register an application at https://github.com/settings/applications/new to get values for clientId and clientSecret HomeController fields.
  (For more information check: https://docs.github.com/en/developers/apps/building-oauth-apps/creating-an-oauth-app)
  - set 'Application name' field value whatever you like
  - set 'Homepage URL' field value as local andress and port you you will run application, for example: https://localhost:44362
  - set 'Authorization callback URL' as https://localhost:44362/home/authorize
  
3. Create CLIENT_ID and CLIENT_SECRET user enviroment variables.

4. Set user enviroment variables values with the values from your application registration.

5. After changing enviroment variables you might need to restart Visual Studio and start-up application again, 
    because it doesn't detect changes to the environment variables while it is running.


