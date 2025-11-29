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
}