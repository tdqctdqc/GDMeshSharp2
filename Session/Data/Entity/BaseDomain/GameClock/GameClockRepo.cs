
public class GameClockRepo : SingletonRepo<GameClock>
{
    public GameClockRepo(Domain domain, Data data) : base(domain, data)
    {
    }
}
