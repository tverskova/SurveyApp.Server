using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Server.DTOs.Responses;
using SurveyApp.Server.Services.Interfaces;

namespace SurveyApp.Server.Controllers
{
    [ApiController]
    [Route("api/responses")]
    [Authorize]
    public class ResponseController : ControllerBase
    {
        private readonly IResponseService _responseService;

        public ResponseController(IResponseService responseService)
        {
            _responseService = responseService;
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitSurveyResponseDto dto)
        {
            if (dto == null)
                return BadRequest("Данные ответа не переданы.");

            if (dto.Answers == null || dto.Answers.Count == 0)
                return BadRequest("Список ответов пуст.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _responseService.SubmitResponseAsync(dto, userId);

            if (result == null)
                return NotFound("Опрос не найден.");

            return Ok(result);
        }
    }
}