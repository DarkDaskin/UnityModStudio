using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace UnityModStudio.Common.GameSpecific.Extensions;

[InheritedExport]
public interface IGameExtensionResolver
{
    IReadOnlyCollection<GameExtension> GetGameExtensions(GameInformation gameInformation);
}