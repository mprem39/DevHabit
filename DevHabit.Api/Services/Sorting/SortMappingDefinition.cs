namespace DevHabit.Api.Services.Sorting;

// Suppress Sonar rule S2326: TSource is intentionally unused in this generic declaration
#pragma warning disable S2326
public sealed class SortMappingDefinition<TSource, TDestination> : ISortMappingDefinition
{
    public required SortMapping[] Mappings { get; init; }
}
#pragma warning restore S2326
