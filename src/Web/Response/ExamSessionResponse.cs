using System.Text.Json.Serialization;

namespace OsuTaikoDaniDojo.Web.Response;

public class ExamSessionResponse
{
    [JsonPropertyName("exam_session_id")]
    public int ExamSessionId { get; init; }
}
