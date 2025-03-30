using Reloaded.Hooks.Definitions;

namespace P5R.CostumeFramework.Hooks;

internal class MultiAsmHook : IAsmHook
{
    private readonly IAsmHook[] hooks;

    public MultiAsmHook(params IAsmHook[] hooks)
    {
        this.hooks = hooks;
    }

    public bool IsEnabled
    {
        get => this.hooks.First().IsEnabled;
    }

    public IAsmHook Activate()
    {
        foreach (var hook in this.hooks)
        {
            hook.Activate();
        }

        return this;
    }

    public void Disable()
    {
        foreach (var hook in this.hooks)
        {
            hook.Disable();
        }
    }

    public void Enable()
    {
        foreach (var hook in this.hooks)
        {
            hook.Enable();
        }
    }
}
