using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Data;

namespace API;
public class CoordinateService
{
    private readonly DataContext _context;

    public CoordinateService(DataContext context)
    {
        _context = context;
    }

    public bool ValidateCoordinates(List<Coordinate> coordinates)
    {
        foreach (var coordinate in coordinates)
        {
            if (coordinate == null ||
                string.IsNullOrEmpty(coordinate.Longitudinal) ||
                string.IsNullOrEmpty(coordinate.Lateral) ||
                string.IsNullOrEmpty(coordinate.TimeStamp))
            {
                return false;
            }

            // Add more validation logic here if needed
        }

        return true;
    }

    public List<Coordinate> GetCoordinatesByIdAndCount(int startId, int count)
    {
        List<Coordinate> resultCoordinates = new List<Coordinate>();

        // Logic to retrieve coordinates from the database using the provided startId and count
        for (int i = startId; i < startId + count; i++)
        {
            // Assuming _context is your database context, and Coordinates is your DbSet
            var coordinate = _context.Coordinates.FirstOrDefault(c => c.Id == i);

            // If coordinate with current ID is found, add it to the result list
            if (coordinate != null)
            {
                resultCoordinates.Add(coordinate);
            }
            else
            {
                // If coordinate with current ID is not found, break the loop
                break;
            }
        }

        return resultCoordinates;
    }
}