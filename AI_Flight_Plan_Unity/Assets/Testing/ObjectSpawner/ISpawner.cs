public interface ISpawnerState
{
    void OnEnter(Spawner spawner);
    void OnExit(Spawner spawner, bool isCancelled);
    void Tick(Spawner spawner);
}

