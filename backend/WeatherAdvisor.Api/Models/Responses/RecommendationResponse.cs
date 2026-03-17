namespace WeatherAdvisor.Api.Models.Responses;

public class RecommendationResponse
{
    public string Activity { get; set; } = string.Empty;
    public string Verdict { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}
