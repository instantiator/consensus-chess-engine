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
        BlackParticipantNetworkServers = new List<StoredString>();
        WhiteParticipantNetworkServers = new List<StoredString>();
        BlackPostingNodeShortcodes = new List<StoredString>();
        WhitePostingNodeShortcodes = new List<StoredString>();
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
    public virtual List<StoredString> BlackParticipantNetworkServers { get; set; }
    public virtual List<StoredString> WhiteParticipantNetworkServers { get; set; }
    public virtual List<StoredString> BlackPostingNodeShortcodes { get; set; }
    public virtual List<StoredString> WhitePostingNodeShortcodes { get; set; }

    [NotMapped] public bool Active => DateTime.Now >= ScheduledStart && Finished == null;
    [NotMapped] public Move CurrentMove => Moves.OrderBy(m => m.Deadline).Last();
    [NotMapped] public Board CurrentBoard => CurrentMove.From;
    [NotMapped] public Side CurrentSide => CurrentBoard.ActiveSide;

    [NotMapped]
    public List<StoredString> CurrentParticipantNetworkServers =>
        CurrentSide == Side.White
            ? WhiteParticipantNetworkServers
            : BlackParticipantNetworkServers;

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
        game.BlackPostingNodeShortcodes.AddRange(blackPostingNodeShortcodes.Select(s => (StoredString)s));
        game.WhitePostingNodeShortcodes.AddRange(whitePostingNodeShortcodes.Select(s => (StoredString)s));
        game.BlackParticipantNetworkServers.AddRange(blackSideNetworkServers?.Select(s => (StoredString)s) ?? new List<StoredString>());
        game.WhiteParticipantNetworkServers.AddRange(whiteSideNetworkServers?.Select(s => (StoredString)s) ?? new List<StoredString>());
        return game;
    }
}
