using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiMax.Api.Services;

namespace SiMax.Api.Controllers;

[ApiController]
[Route("api/github-test")]
[Authorize(Policy = "AdminOnly")]
public class GitHubTestController : ControllerBase
{
    private readonly GitHubService _gitHubService;

    public GitHubTestController(GitHubService gitHubService)
    {
        _gitHubService = gitHubService;
    }

    [HttpPost]
    public async Task<IActionResult> Test()
    {
        var json = """
        {
          "events": [
            {
              "id": 999,
              "name": "TEST GITHUB",
              "date": "2026-10-02",
              "active": true
            }
          ]
        }
        """;

        await _gitHubService.UpdateFileAsync(
            json,
            "Test update events.json from SiMax BE");

        return Ok(new
        {
            status = "ok"
        });
    }
}