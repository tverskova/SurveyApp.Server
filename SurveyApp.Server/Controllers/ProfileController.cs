using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Server.DTOs.Profile;
using SurveyApp.Server.Services.Interfaces;
using System.Security.Claims;

namespace SurveyApp.Server.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var profile = await _profileService.GetProfileAsync(userId);

            if (profile == null)
                return NotFound("Профиль не найден.");

            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileDto dto)
        {
            if (dto == null)
                return BadRequest("Данные профиля не переданы.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var updated = await _profileService.UpdateProfileAsync(userId, dto);

            if (!updated)
                return NotFound("Пользователь не найден.");

            return Ok(new { Message = "Профиль успешно обновлён." });
        }
    }
}