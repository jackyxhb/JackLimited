using JackLimited.Domain;

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
}