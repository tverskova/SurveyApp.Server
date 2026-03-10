using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Server.Services.Interfaces;

namespace SurveyApp.Server.Controllers
{
    [ApiController]
    [Route("api/surveys")]
    [Authorize]
    public class SurveyController : ControllerBase
    {
        private readonly ISurveyService _surveyService;

        public SurveyController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        [HttpGet("published")]
        public async Task<IActionResult> GetPublished()
        {
            var surveys = await _surveyService.GetPublishedSurveysAsync();
            return Ok(surveys);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var survey = await _surveyService.GetSurveyDetailsAsync(id);

            if (survey == null)
                return NotFound("Опрос не найден.");

            return Ok(survey);
        }
    }
}