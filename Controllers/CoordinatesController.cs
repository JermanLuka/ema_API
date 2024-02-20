using System.Globalization;
using API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API;

[ApiController]
[Route("api/[controller]")]
public class CoordinatesController : ControllerBase
{

    private readonly DataContext _context;
    private readonly CoordinateService _coordinateService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoordinatesController"/> class.
    /// </summary>
    public CoordinatesController(DataContext context, CoordinateService coordinateService)
    {
        _context = context;
        _coordinateService = coordinateService;
    }

    /// <summary>
    /// Retrieves all coordinates.
    /// </summary>
    /// <returns>An action result containing the list of coordinates.</returns>
    [HttpGet("getCoordinates")]
    public async Task<IActionResult> GetCoordinates()
    {
        try
        {
            var coordinates = await _context.Coordinates.OrderBy(c => c.TimeStamp).ToListAsync();

            if (coordinates == null || coordinates.Count == 0)
            {
                return Ok(new List<Coordinate>());
            }

            // Update the format of the coordinates
            foreach (var coordinate in coordinates)
            {
                // Replace comma with decimal point for Lateral and Longitudinal
                coordinate.Lateral = ParseCoordinate(coordinate.Lateral);
                coordinate.Longitudinal = ParseCoordinate(coordinate.Longitudinal);
            }

            return Ok(coordinates);
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching coordinates.");
        }
    }

    /// <summary>
    /// Replace comma with decimal point and parse to double
    /// </summary>
    /// <param name="coordinateString"></param>
    private string ParseCoordinate(string coordinateString)
    {
        return coordinateString.Replace(',', '.');
    }

    /// <summary>
    /// Imports CSV/JSON file to sqlite DB
    /// </summary>
    /// <param name="coordinates"></param>
    [HttpPost]
    [Route("saveBulk")]
    public async Task<IActionResult> BulkImportCoordinates(List<Coordinate> coordinates)
    {
        try
        {
            if (coordinates == null || coordinates.Count == 0)
            {
                return BadRequest("No coordinates provided.");
            }

            // Validate coordinates using CoordinateService
            if (!_coordinateService.ValidateCoordinates(coordinates))
            {
                return BadRequest("Invalid coordinates provided.");
            }

            foreach (var coordinate in coordinates)
            {
                _context.Coordinates.Add(coordinate);
            }

            await _context.SaveChangesAsync();

            return Ok("Coordinates saved successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [Obsolete("We now implement the sliding window algorithm on the frontend. Requesting this API on each interval is too hard on the server.")]
    [HttpGet("getCoordinatesByIdAndCount")]
    public ActionResult<IEnumerable<Coordinate>> GetCoordinatesByIdAndCountController([FromBody] CoordinateRequest request)
    {
        try
        {
            // Call the service method to retrieve coordinates
            List<Coordinate> coordinates = _coordinateService.GetCoordinatesByIdAndCount(request.StartId, request.Count);
            return Ok(coordinates); // Return the retrieved coordinates
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

public class CoordinateRequest
{
    public int StartId { get; set; }
    public int Count { get; set; }
}
