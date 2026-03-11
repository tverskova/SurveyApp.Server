using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Server.Services.Interfaces;

namespace SurveyApp.Server.Controllers
{
    [ApiController]
    [Route("api/export")]
    [Authorize(Roles = "Admin")]
    public class ExportController : ControllerBase
    {
        private readonly IQuestionBankService _questionBankService;
        private readonly IStatisticsService _statisticsService;

        public ExportController(
            IQuestionBankService questionBankService,
            IStatisticsService statisticsService)
        {
            _questionBankService = questionBankService;
            _statisticsService = statisticsService;
        }

        [HttpGet("question-bank/excel")]
        public async Task<IActionResult> ExportQuestionBankExcel()
        {
            var bytes = await _questionBankService.ExportToExcelAsync();

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "QuestionBank.xlsx");
        }

        [HttpGet("question-bank/xml")]
        public async Task<IActionResult> ExportQuestionBankXml()
        {
            var bytes = await _questionBankService.ExportToXmlAsync();

            return File(
                bytes,
                "application/xml",
                "QuestionBank.xml");
        }

        [HttpGet("statistics/{surveyId:int}/pdf")]
        public async Task<IActionResult> ExportSurveyStatisticsPdf(int surveyId)
        {
            var bytes = await _statisticsService.ExportSurveyStatisticsToPdfAsync(surveyId);

            return File(
                bytes,
                "application/pdf",
                $"SurveyStatistics_{surveyId}.pdf");
        }
        [HttpGet("statistics/{surveyId:int}/excel")]
        public async Task<IActionResult> ExportSurveyStatisticsExcel(int surveyId)
        {
            var bytes = await _statisticsService.ExportSurveyStatisticsToExcelAsync(surveyId);

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"SurveyStatistics_{surveyId}.xlsx");
        }
    }
}