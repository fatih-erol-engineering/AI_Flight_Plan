using System;
public interface IGameModeHooks
{
    public void Init();
    public bool Tick(out ExitMode exitMode);
    public void Apply();
    public void Cancel();
    public ExitMode GetExitMode();
}
public delegate bool TickDelegate(out ExitMode exitMode);
public class ModeHooks
{
    public string modeName;
    public Action Init;
    public TickDelegate Tick;
    public Action Apply;
    public Action Cancel;
    public Func<ExitMode> GetExitMode;

}