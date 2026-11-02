namespace TrustSystem.Backend.Application.Interfaces;

public record SelectMenuRecord
{
    public required string Code { get; init; }
    public required string Label { get; init; }
}