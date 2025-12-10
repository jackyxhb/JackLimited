using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace JackLimited.Api.Observability;

public sealed class SurveyMetrics
{
    private const string OutcomeDimension = "outcome";
    private const string EndpointDimension = "endpoint";

    private readonly Counter<long> _surveySubmissionCounter;
    private readonly Histogram<double> _surveySubmissionDuration;
    private readonly Counter<long> _analyticsRequestCounter;
    private readonly Histogram<double> _analyticsRequestDuration;

    public SurveyMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("JackLimited.Api", "1.0.0");

        _surveySubmissionCounter = meter.CreateCounter<long>(
            "survey_submission_requests_total",
            description: "Count of survey submission attempts");

        _surveySubmissionDuration = meter.CreateHistogram<double>(
            "survey_submission_duration_ms",
            unit: "ms",
            description: "Latency of survey submission processing");

        _analyticsRequestCounter = meter.CreateCounter<long>(
            "analytics_requests_total",
            description: "Count of analytics endpoint invocations");

        _analyticsRequestDuration = meter.CreateHistogram<double>(
            "analytics_request_duration_ms",
            unit: "ms",
            description: "Latency of analytics endpoint processing");
    }

    public void RecordSubmission(TimeSpan duration, SubmissionOutcome outcome)
    {
        var tags = new TagList
        {
            { OutcomeDimension, outcome.ToString().ToLowerInvariant() }
        };

        _surveySubmissionCounter.Add(1, tags);
        _surveySubmissionDuration.Record(duration.TotalMilliseconds, tags);
    }

    public void RecordAnalytics(TimeSpan duration, AnalyticsOutcome outcome, string endpoint)
    {
        var tags = new TagList
        {
            { OutcomeDimension, outcome.ToString().ToLowerInvariant() },
            { EndpointDimension, endpoint }
        };

        _analyticsRequestCounter.Add(1, tags);
        _analyticsRequestDuration.Record(duration.TotalMilliseconds, tags);
    }

    public enum SubmissionOutcome
    {
        Success,
        ValidationError,
        Failure
    }

    public enum AnalyticsOutcome
    {
        Success,
        Failure
    }
}
