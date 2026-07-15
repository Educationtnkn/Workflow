namespace AppFW.Utilities.RuleEngine
{
    public record EntityContext(
        string? EntityId,
        string? EntityType,
        IReadOnlyDictionary<string, object?>? Facts
    )
    {
        public static EntityContext From(
            string? entityId,
            string? entityType,
            IReadOnlyDictionary<string, object?>? facts = null) => new(entityId, entityType, facts ?? new Dictionary<string, object?>()
        );

    }
}
