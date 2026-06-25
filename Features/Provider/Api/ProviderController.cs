using FuelTrack.Api.Features.Provider.Domain;
using Microsoft.AspNetCore.Mvc;

namespace FuelTrack.Api.Features.Provider.Api;

[ApiController]
[Route("api/provider")]
public class ProviderController : ControllerBase
{
    private readonly IProviderRepository _repository;

    public ProviderController(IProviderRepository repository)
    {
        _repository = repository;
    }

    // GET /api/provider/sales-report?fromDate=2026-06-01&toDate=2026-06-30
    [HttpGet("sales-report")]
    public async Task<ActionResult<SalesReport>> GetSalesReport(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var report = await _repository.GetSalesReportAsync(fromDate, toDate);
        return Ok(report);
    }

    // GET /api/provider/sales-chart?fromDate=2026-06-01&toDate=2026-06-30
    [HttpGet("sales-chart")]
    public async Task<ActionResult<IEnumerable<SalesChartPoint>>> GetSalesChart(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var chart = await _repository.GetSalesChartAsync(fromDate, toDate);
        return Ok(chart);
    }

    // GET /api/provider/sales-report/pdf?fromDate=2026-06-01&toDate=2026-06-30
    [HttpGet("sales-report/pdf")]
    public async Task<IActionResult> DownloadSalesReportPdf(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var pdf = await _repository.GetSalesReportPdfAsync(fromDate, toDate);
        return File(pdf.Content, pdf.ContentType, pdf.FileName);
    }
}
