namespace JackLimited.Domain;

public static class NpsCalculator
{
    public static double CalculateNps(IEnumerable<int> ratings)
    {
        if (!ratings.Any()) return 0;

        var promoters = ratings.Count(r => r >= 9);
        var detractors = ratings.Count(r => r <= 6);
        var total = ratings.Count();

        return Math.Round((double)(promoters - detractors) / total * 100, 2);
    }
}