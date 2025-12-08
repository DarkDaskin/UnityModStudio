using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace UnityModStudio.Common.GameSpecific;

[InheritedExport]
public interface IGameExtensionResolver
{
    IReadOnlyCollection<GameExtension> GetGameExtensions(GameInformation gameInformation);
}