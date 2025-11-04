public interface ICollidable
{
    bool isCollided { get; set; }
    void SetIsCollided(bool isCollided, bool isImmediate = false);
}