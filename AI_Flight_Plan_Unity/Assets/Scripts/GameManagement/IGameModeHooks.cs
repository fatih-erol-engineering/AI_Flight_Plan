using System;
public interface IGameModeHooks
{
    public void Init();
    public void Tick();
    public void Apply();
    public void Cancel();
}

