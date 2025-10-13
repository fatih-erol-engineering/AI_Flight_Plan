using System;
using Unity.VisualScripting;
public interface IGameModeHooks
{
    public void Init();
    public bool Tick();
    public void Apply();
    public void Cancel();
    public ExitMode GetExitMode();
}

