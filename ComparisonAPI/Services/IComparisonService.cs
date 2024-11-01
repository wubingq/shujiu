using ComparisonAPI.Models;
using Microsoft.AspNetCore.Http;

namespace ComparisonAPI.Services
{
    public interface IComparisonService
    {
        Task<List<ComparisonResult>> CompareFiles(IFormFile fileA, IFormFile fileB, 
            double lengthTolerance, double widthTolerance, string savePath);
        Task<List<ComparisonResult>> GetResults(DateTime startTime, DateTime endTime);
    }
}
