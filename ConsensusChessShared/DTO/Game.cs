namespace ConsensusChessShared.DTO;

public class Game : AbstractDTO
{
    public DateTime ScheduledStart { get; set; }
    public DateTime? Finished { get; set; }

    public IEnumerable<Move> Moves { get; set; }

    IEnumerable<Network> BlackNetworks { get; set; }
    IEnumerable<Network> WhiteNetworks { get; set; }

    public bool Active => DateTime.Now > ScheduledStart && Finished == null;
    public Move CurrentMove => Moves.OrderBy(m => m.Deadline).Last();
    public Board CurrentBoard => CurrentMove.From;

}
