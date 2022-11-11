using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsensusChessShared.DTO;

public class Game : IDTO
{
    public static readonly TimeSpan DEFAULT_MOVE_DURATION = TimeSpan.FromDays(2);

    public Game()
    {
        Created = DateTime.Now.ToUniversalTime();
        Moves = new List<Move>();
        BlackNetworks = new List<string>();
        WhiteNetworks = new List<string>();
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public DateTime Created { get; set; }

    public DateTime ScheduledStart { get; set; }
    public DateTime? Finished { get; set; }
    public TimeSpan MoveDuration { get; set; }
    public SideRules SideRules { get; set; }
    public virtual List<Move> Moves { get; set; }

    public virtual List<string> BlackNetworks { get; set; }
    public virtual List<string> WhiteNetworks { get; set; }

    public bool Active => DateTime.Now > ScheduledStart && Finished == null;
    public Move CurrentMove => Moves.OrderBy(m => m.Deadline).Last();
    public Board CurrentBoard => CurrentMove.From;

    public static Game NewGame(IEnumerable<string> whites, IEnumerable<string> blacks, SideRules sideRules)
    {
        var game = new Game()
        {
            ScheduledStart = DateTime.Now.ToUniversalTime(),
            MoveDuration = DEFAULT_MOVE_DURATION,
            Moves = new List<Move>()
            {
                Move.CreateStartingMove(DEFAULT_MOVE_DURATION)
            },
            BlackNetworks = new List<string>(),
            WhiteNetworks = new List<string>(),
        };
        game.WhiteNetworks.AddRange(whites);
        game.BlackNetworks.AddRange(blacks);
        game.SideRules = sideRules;
        return game;
    }
}
