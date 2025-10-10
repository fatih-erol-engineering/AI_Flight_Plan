using System;
public interface IGameModeHooks 
{
    public Action Init();
    public Action Tick();
    public Action Apply();
    public Action Cancel();
}

