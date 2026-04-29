using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace UnityModStudio.Common.GameSpecific.Versions;

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