using Microsoft.KernelMemory;

namespace DocTalk;

/// <summary>
/// Represent an answer from the AI engine
/// </summary>
public class AnswerDTO
{
    /// <summary>
    /// The original question
    /// </summary>
    public string Question { get; set; } = default!;

    /// <summary>
    /// Answer provided from the AI
    /// </summary>
    public string? Answer { get; set; } = default!;

    /// <summary>
    /// Relevan sources for the given answer
    /// </summary>
    public IEnumerable<string> RelevantSources { get; set; } = default!;

    /// <summary>
    /// True if the AI has no response for the given question
    /// </summary>
    public bool IsEmptyAnswer { get; set; } = default!;


    internal AnswerDTO(string question, MemoryAnswer answer)
    {
        this.Question = question;
        this.IsEmptyAnswer = answer.NoResult;
        this.Answer = (this.IsEmptyAnswer) ? answer.NoResultReason : answer.Result;
        this.RelevantSources = answer.RelevantSources.Select(s => $"{s.SourceName} - {s.Link}");
    }
}
