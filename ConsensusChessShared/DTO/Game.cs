namespace ConsensusChessShared.DTO;

public class Game : AbstractDTO
{
    public DateTime ScheduledStart { get; set; }
    public DateTime Finished { get; set; }
    public DateTime? NextMoveDeadline { get; set; }
    public Board CurrentBoard { get; set; }
    public IEnumerable<Move> History { get; set; }
    public bool Active { get; set; }
    public Side NextSide { get; set; }
    public IDictionary<Side, IEnumerable<Network>> Sides { get; set; }
}
