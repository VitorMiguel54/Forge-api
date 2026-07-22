using Forge.Application.DTOs.UserProfile;
using Forge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/user-profiles")]
public class UserProfileController(IUserProfileService userProfileService) : ControllerBase
{
    private const string GetUserProfileByIdRouteName = "GetUserProfileById";

    [HttpPost]
    public async Task<ActionResult<UserProfileResponse>> CreateAsync(
        CreateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userProfile = await userProfileService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute(GetUserProfileByIdRouteName, new { id = userProfile.Id }, userProfile);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserProfileResponse>>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var userProfiles = await userProfileService.GetAllAsync(cancellationToken);

        return Ok(userProfiles);
    }

    [HttpGet("{id:guid}", Name = GetUserProfileByIdRouteName)]
    public async Task<ActionResult<UserProfileResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userProfile = await userProfileService.GetByIdAsync(id, cancellationToken);
        if (userProfile is null)
        {
            return NotFound();
        }

        return Ok(userProfile);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserProfileResponse>> UpdateAsync(
        Guid id,
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userProfile = await userProfileService.UpdateAsync(id, request, cancellationToken);
        if (userProfile is null)
        {
            return NotFound();
        }

        return Ok(userProfile);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await userProfileService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
