using JackLimited.Domain;
using Microsoft.EntityFrameworkCore;

namespace JackLimited.Infrastructure;

public class SurveyRepository : ISurveyRepository
{
    private readonly JackLimitedDbContext _context;

    public SurveyRepository(JackLimitedDbContext context)
    {
        _context = context;
    }

    public async Task<Survey> AddAsync(Survey survey)
    {
        if (survey == null)
            throw new ArgumentNullException(nameof(survey));

        _context.Surveys.Add(survey);
        await _context.SaveChangesAsync();
        return survey;
    }

    public async Task<IEnumerable<int>> GetAllRatingsAsync()
    {
        return await _context.Surveys
            .Select(s => s.LikelihoodToRecommend)
            .ToListAsync();
    }

    public async Task<double> GetAverageRatingAsync()
    {
        var average = await _context.Surveys
            .Select(s => (double?)s.LikelihoodToRecommend)
            .AverageAsync();

        return average ?? 0;
    }

    public async Task<IDictionary<int, int>> GetRatingDistributionAsync()
    {
        return await _context.Surveys
            .GroupBy(s => s.LikelihoodToRecommend)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Rating, x => x.Count);
    }
}