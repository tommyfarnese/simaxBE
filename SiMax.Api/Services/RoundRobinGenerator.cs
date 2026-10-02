using SiMax.Api.DTOs.Admin;
using SiMax.Api.Models;

namespace SiMax.Api.Services;

public class RoundRobinGenerator
{
    public List<GeneratedMatchDto> Generate(
        IEnumerable<Pool> pools)
    {
        var matches = new List<GeneratedMatchDto>();

        foreach (var pool in pools.OrderBy(p => p.SortOrder))
        {
            var teams = pool.Entries
                .OrderBy(e => e.Position)
                .ToList();

            for (var i = 0; i < teams.Count; i++)
            {
                for (var j = i + 1; j < teams.Count; j++)
                {
                    matches.Add(new GeneratedMatchDto
                    {
                        PoolId = pool.Id,
                        Team1RegistrationId =
                            teams[i].RegistrationId,
                        Team2RegistrationId =
                            teams[j].RegistrationId
                    });
                }
            }
        }

        return matches;
    }
}