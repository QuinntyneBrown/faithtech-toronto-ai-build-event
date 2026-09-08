namespace FaithTechTorontoAiBuildEvent.Application.Validation;

public sealed class InputValidationException(string field, string message) : Exception("Invalid input.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = new Dictionary<string, string[]> { [field] = [message] };
}
