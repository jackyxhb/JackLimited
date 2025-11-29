using JackLimited.Domain;

namespace JackLimited.Domain;

public interface ISurveyRepository
{
    Task<Survey> AddAsync(Survey survey);
}