using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SiMax.Api.Services;

public class GitHubService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GitHubService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task UpdateFileAsync(
        string content,
        string commitMessage)
    {
        var token = _configuration["GITHUB_TOKEN"];
        var owner = _configuration["GITHUB_OWNER"];
        var repo = _configuration["GITHUB_REPO"];
        var path = _configuration["GITHUB_EVENTS_PATH"];

        if (string.IsNullOrWhiteSpace(token))
            throw new Exception("GITHUB_TOKEN non configurato.");

        if (string.IsNullOrWhiteSpace(owner))
            throw new Exception("GITHUB_OWNER non configurato.");

        if (string.IsNullOrWhiteSpace(repo))
            throw new Exception("GITHUB_REPO non configurato.");

        if (string.IsNullOrWhiteSpace(path))
            throw new Exception("GITHUB_EVENTS_PATH non configurato.");

        var url =
            $"https://api.github.com/repos/{owner}/{repo}/contents/{path}";

        using var getRequest = new HttpRequestMessage(
            HttpMethod.Get,
            url);

        getRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        getRequest.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue(
                "application/vnd.github+json"));

        getRequest.Headers.Add(
            "X-GitHub-Api-Version",
            "2026-03-10");

        var getResponse =
            await _httpClient.SendAsync(getRequest);

        getResponse.EnsureSuccessStatusCode();

        var existingFile =
            await getResponse.Content.ReadFromJsonAsync<JsonElement>();

        var sha = existingFile
            .GetProperty("sha")
            .GetString();

        var encodedContent = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(content));

        var body = new
        {
            message = commitMessage,
            content = encodedContent,
            sha = sha
        };

        using var putRequest = new HttpRequestMessage(
            HttpMethod.Put,
            url);

        putRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        putRequest.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue(
                "application/vnd.github+json"));

        putRequest.Headers.Add(
            "X-GitHub-Api-Version",
            "2026-03-10");

        putRequest.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        var putResponse =
            await _httpClient.SendAsync(putRequest);

        var responseBody =
            await putResponse.Content.ReadAsStringAsync();

        if (!putResponse.IsSuccessStatusCode)
        {
            throw new Exception(
                $"GitHub API error {(int)putResponse.StatusCode}: {responseBody}");
        }
    }
}