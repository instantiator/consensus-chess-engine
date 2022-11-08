namespace ConsensusChessShared.DTO;

public class Game : AbstractDTO
{
    public static readonly TimeSpan DEFAULT_MOVE_DURATION = TimeSpan.FromDays(2);

    public Game(IEnumerable<string> whites, IEnumerable<string> blacks, SideRules sideRules) : this()
    {
        WhiteNetworks.AddRange(whites);
        BlackNetworks.AddRange(blacks);
        SideRules = sideRules;
    }

    public Game() : base()
    {
        ScheduledStart = DateTime.Now.ToUniversalTime();
        MoveDuration = DEFAULT_MOVE_DURATION;
        Moves = new List<Move>()
        {
            Move.CreateStartingMove(MoveDuration)
        };
        BlackNetworks = new List<string>();
        WhiteNetworks = new List<string>();
    }

    public DateTime ScheduledStart { get; set; }
    public DateTime? Finished { get; set; }
    public TimeSpan MoveDuration { get; set; }
    public List<Move> Moves { get; set; }
    public SideRules SideRules { get; set; }

    public List<string> BlackNetworks { get; set; }
    public List<string> WhiteNetworks { get; set; }

    public bool Active => DateTime.Now > ScheduledStart && Finished == null;
    public Move CurrentMove => Moves.OrderBy(m => m.Deadline).Last();
    public Board CurrentBoard => CurrentMove.From;
}
