namespace JackLimited.Domain;

public class Survey
{
    public Guid Id { get; set; }
    public int LikelihoodToRecommend { get; set; }
    public string? Comments { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
}