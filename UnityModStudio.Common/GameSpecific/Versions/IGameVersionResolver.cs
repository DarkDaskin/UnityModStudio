using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace UnityModStudio.Common.GameSpecific.Versions;

/// <remarks>
/// Implementations of this interface should have a public parameterless constructor.
/// </remarks>
[InheritedExport]
public interface IGameVersionResolver
{
    string? GetGameVersion(GameInformation gameInformation);
}

public static class GameVersionResolverExtensions
{
    public static string? ResolveGameVersion(this IEnumerable<IGameVersionResolver> resolvers, GameInformation gameInformation) =>
        resolvers.Select(resolver => resolver.GetGameVersion(gameInformation)).FirstOrDefault(version => version != null);
}