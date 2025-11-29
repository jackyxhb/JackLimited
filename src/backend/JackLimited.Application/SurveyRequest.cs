namespace JackLimited.Application;

public record SurveyRequest(int LikelihoodToRecommend, string? Comments, string? Email);