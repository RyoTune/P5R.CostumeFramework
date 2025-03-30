using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

namespace P5R.CostumeFramework.Hooks;

internal interface IGameHook
{
    void Initialize(IStartupScanner scanner, IReloadedHooks hooks);
}
