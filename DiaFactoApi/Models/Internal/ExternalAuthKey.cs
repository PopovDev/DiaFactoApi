namespace DiaFactoApi.Models.Internal;

public record ExternalAuthKey
{
    public required int UserId { get; init; }
    public required string Key { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}