using TrustSystem.Backend.DTOs;
using TrustSystem.Backend.Features.Policies;

namespace TrustSystem.Backend.Application.Services;

public class PolicyService(
    PolicyRepository repo
    )
{
    private readonly PolicyRepository _repo = repo;

    public async Task<List<AgentViewPolicy>> GetAllCustomerPortalPolicies()
    {
        var results = new List<AgentViewPolicy>();

        var singlePolicies = await _repo.GetAllSinglePolicies(new InquirySinglePolicyRequestDTO(), customerPortalPolicies: true);
        var groupPolicies = await _repo.GetAllGroupPolicies(new InquiryGroupPolicyRequestDTO(), customerPortalPolicies: true);

        results.AddRange(singlePolicies.Select(v => new AgentViewSinglePolicy(v)));
        results.AddRange(groupPolicies.Select(v => new AgentViewGroupPolicy(v)));

        return [.. results.OrderByDescending(v => v.issueDate)];
    }
}