using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Server.DTOs.Admin;
using SurveyApp.Server.Services.Interfaces;

namespace SurveyApp.Server.Controllers
{
    [ApiController]
    [Route("api/admin/surveys")]
    [Authorize(Roles = "Admin")]
    public class AdminSurveyController : ControllerBase
    {
        private readonly ISurveyService _surveyService;

        public AdminSurveyController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var surveys = await _surveyService.GetAllSurveysAsync();
            return Ok(surveys);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSurveyDto dto)
        {
            if (dto == null)
                return BadRequest("Данные опроса не переданы.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Название опроса обязательно.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var surveyId = await _surveyService.CreateSurveyAsync(dto, userId);

            return Ok(new
            {
                Message = "Опрос успешно создан.",
                SurveyId = surveyId
            });
        }

        [HttpPost("{id:int}/publish")]
        public async Task<IActionResult> Publish(int id)
        {
            var result = await _surveyService.PublishSurveyAsync(id);

            if (!result)
                return NotFound("Опрос не найден.");

            return Ok(new { Message = "Опрос опубликован." });
        }
    }
}