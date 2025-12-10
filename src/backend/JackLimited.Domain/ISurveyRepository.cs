using System.Collections.Generic;
using System.Threading.Tasks;

namespace JackLimited.Domain;

public interface ISurveyRepository
{
    Task<Survey> AddAsync(Survey survey);
    Task<IEnumerable<int>> GetAllRatingsAsync();
    Task<double> GetAverageRatingAsync();
    Task<IDictionary<int, int>> GetRatingDistributionAsync();
}