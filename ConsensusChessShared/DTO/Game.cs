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
        BlackParticipantNetworkServers = new List<string>();
        WhiteParticipantNetworkServers = new List<string>();
        BlackPostingNodeShortcodes = new List<string>();
        WhitePostingNodeShortcodes = new List<string>();
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public string Shortcode { get; set; }
    public string Description { get; set; }

    public DateTime ScheduledStart { get; set; }
    public DateTime? Finished { get; set; }
    public TimeSpan MoveDuration { get; set; }
    public virtual List<Move> Moves { get; set; }

    public SideRules SideRules { get; set; }
    public virtual List<string> BlackParticipantNetworkServers { get; set; }
    public virtual List<string> WhiteParticipantNetworkServers { get; set; }
    public virtual List<string> BlackPostingNodeShortcodes { get; set; }
    public virtual List<string> WhitePostingNodeShortcodes { get; set; }

    public bool Active => DateTime.Now > ScheduledStart && Finished == null;
    public Move CurrentMove => Moves.OrderBy(m => m.Deadline).Last();
    public Board CurrentBoard => CurrentMove.From;
    public Side CurrentSide => CurrentBoard.ActiveSide;
    public IEnumerable<string> CurrentParticipantNetworkServers => CurrentSide == Side.White ? WhiteParticipantNetworkServers : BlackParticipantNetworkServers;

    public static Game NewGame(string shortcode, string description,
        IEnumerable<string>? whiteSideNetworkServers, IEnumerable<string>? blackSideNetworkServers,
        IEnumerable<string> whitePostingNodeShortcodes, IEnumerable<string> blackPostingNodeShortcodes,
        SideRules sideRules)
    {
        var game = new Game()
        {
            Shortcode = shortcode,
            Description = description,
            ScheduledStart = DateTime.Now.ToUniversalTime(),
            MoveDuration = DEFAULT_MOVE_DURATION,
            Moves = new List<Move>()
            {
                Move.CreateStartingMove(DEFAULT_MOVE_DURATION)
            },
            SideRules = sideRules,
        };
        game.BlackPostingNodeShortcodes.AddRange(blackPostingNodeShortcodes);
        game.WhitePostingNodeShortcodes.AddRange(whitePostingNodeShortcodes);
        game.BlackParticipantNetworkServers.AddRange(blackSideNetworkServers ?? new List<string>());
        game.WhiteParticipantNetworkServers.AddRange(whiteSideNetworkServers ?? new List<string>());
        return game;
    }
}
