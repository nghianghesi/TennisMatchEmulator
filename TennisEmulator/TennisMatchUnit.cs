using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TennisEmulator
{
    public abstract class MatchUnit
    {
        public List<MatchTeam> MatchTeams
        { get; private set; }

        public MatchUnit()
        {
            this.MatchTeams = new List<MatchTeam>();
        }

        public abstract void AddScore(MatchTeam team);
        public abstract MatchTeam GetWinTeam();
        public abstract void Play();
    }

    public abstract class CompositMatchUnit: MatchUnit, IEventHandler<MatchUnit, MatchUnitFinishedEvent>
    {
        protected List<MatchUnit> SubUnits;
        protected MatchUnit CurrentSubUnit => SubUnits?.LastOrDefault();

        public CompositMatchUnit()
        {
            this.SubUnits = new List<MatchUnit>();
        }

        public void Handle(MatchUnit source, MatchUnitFinishedEvent e)
        {
            if (source == this.CurrentSubUnit)
            {
                MatchTeam team = this.MatchTeams.Where(t => t.Player == e.WinningTeam.Player).FirstOrDefault();
                if (team!=null)
                {
                    this.AddScore(team);
                }
            }
        }
    }

    public class Match : CompositMatchUnit
    {
        public int WinScore { get; private set; }
        public Match(List<Player> players, int winScore)
        {
            if (players == null || players.Count != 2 || players[0] == null || players[1] == null || players[0] == players[1])
            {
                throw new InvalidOperationException("Need 2 players");
            }

            foreach (Player p in players)
            {
                this.MatchTeams.Add(new MatchTeam(p));
            }

            this.WinScore = winScore;
        }

        public override void AddScore(MatchTeam team)
        {
            throw new NotImplementedException();
        }

        public override MatchTeam GetWinTeam()
        {
            throw new NotImplementedException();
        }

        public override void Play()
        {
            throw new NotImplementedException();
        }
    }

    public class Set : CompositMatchUnit, IEventHandler<Game, MatchUnitFinishedEvent>
    {
        public Set(List<Player> players)
        {
            foreach (Player p in players)
            {
                this.MatchTeams.Add(new MatchTeam(p));
            }
        }

        public override void AddScore(MatchTeam team)
        {
            MatchTeam otherTeam = this.MatchTeams.Where(t => t != team).FirstOrDefault();
            team.Score += 1;
            if (this.IsWin(team, otherTeam))
            {
                DI.Resolver.Resolve<IEventDispatcher>().Dispatch(this, new MatchUnitFinishedEvent(team));
                DI.Resolver.Resolve<IEventDispatcher>().CleanupSource(this);
            }
            else
            {
                this.Play();
            }
        }

        public override MatchTeam GetWinTeam()
        {
            MatchTeam team = this.MatchTeams[0];
            MatchTeam otherTeam = this.MatchTeams[0];
            if (team.Score < this.MatchTeams[1].Score)
            {
                team = this.MatchTeams[1];
                otherTeam = this.MatchTeams[0];
            }

            if (team!=null && otherTeam != null && ((team.Score == 6 && team.Score - otherTeam.Score >= 2) || team.Score == 7))
            { 
                return team; 
            }

            return null;
        }

        public override void Play()
        {
            Game lastGame = this.SubUnits.LastOrDefault() as Game;
            Game newGame = null;
            if (this.MatchTeams[0].Score == 6 && this.MatchTeams[0].Score == 6)
            {
                newGame = new TieBreaker(this.MatchTeams, Game.SwitchServingPlayer(lastGame.ServingPlayer));
            }
            else
            {
                newGame = new Game(this.MatchTeams, Game.SwitchServingPlayer(lastGame.ServingPlayer));
            }

            this.SubUnits.Add(newGame);
            DI.Resolver.Resolve<IEventDispatcher>().AddHandler<Game, MatchUnitFinishedEvent>(newGame, this);
            newGame.Play();
        }

        public void Handle(Game source, MatchUnitFinishedEvent e)
        {
            MatchTeam team = this.MatchTeams.FirstOrDefault(t => t.Player == e.WinningTeam.Player);
            this.AddScore(team);
        }

        private bool IsWin(MatchTeam team, MatchTeam otherTeam)
        {
            return (team.Score == 6 && team.Score - otherTeam.Score >= 2) || team.Score == 7;
        }
    }

    public class Game : MatchUnit, IEventHandler<Player, BallHittedEvent>, IEventHandler<Player, FailedHitEvent>
    {
        public static int SwitchServingPlayer(int currentServingPlayer)
        {
            return (currentServingPlayer + 1) % 2;
        }

        public int ServingPlayer
        { get; protected set; }

        public Game(List<MatchTeam> setTeam, int servingPlayer)
        {
            this.ServingPlayer = servingPlayer;
            foreach (MatchTeam p in setTeam)
            {
                this.MatchTeams.Add(new MatchTeam(p.Player));
                DI.Resolver.Resolve<IEventDispatcher>().AddHandler<Player, BallHittedEvent>(p.Player, this);
                DI.Resolver.Resolve<IEventDispatcher>().AddHandler<Player, FailedHitEvent>(p.Player, this);
            }
        }

        public override void AddScore(MatchTeam team)
        {
            MatchTeam otherTeam = this.MatchTeams.Where(t => t != team).FirstOrDefault();
            if (team.Score == 0)
            { 
                team.Score = 15;
            }
            else if (team.Score == 15)
            {
                team.Score = 30;
            }
            else if (team.Score == 30)
            {
                team.Score = 40;
            }
            else if (team.Score == 40)
            {
                if (otherTeam.Score < 40)
                {
                    team.Score = 42;

                    // remove handlers
                    foreach(MatchTeam t in this.MatchTeams)
                    {
                        DI.Resolver.Resolve<IEventDispatcher>().RemoveHandler<Player, BallHittedEvent>(t.Player, this);
                        DI.Resolver.Resolve<IEventDispatcher>().RemoveHandler<Player, FailedHitEvent>(t.Player, this);
                    }

                    DI.Resolver.Resolve<IEventDispatcher>().Dispatch(this, new MatchUnitFinishedEvent(team));
                    DI.Resolver.Resolve<IEventDispatcher>().CleanupSource(this);

                    return;
                }
                else if (otherTeam.Score == 40)
                {
                    team.Score = 41;
                }
                else
                {
                    team.Score = 40;
                    otherTeam.Score = 40;
                }
            }
            this.Play();
        }

        public override MatchTeam GetWinTeam()
        {
            return this.MatchTeams.Where(t => t.Score == 42).FirstOrDefault();
        }

        public virtual void Handle(Player source, BallHittedEvent e)
        {
            Player otherPlayer = this.MatchTeams.Where(t => t.Player != source).FirstOrDefault()?.Player;
            if(otherPlayer != null)
            {
                otherPlayer.HanleBall();
            }
        }

        public virtual void Handle(Player source, FailedHitEvent e)
        {
            MatchTeam otherTeam = this.MatchTeams.Where(t => t.Player != source).FirstOrDefault();
            if (otherTeam != null)
            {
                this.AddScore(otherTeam);
            }
        }

        public override void Play()
        {
            this.MatchTeams[this.ServingPlayer].Player.HanleBall();
        }
    }

    public class TieBreaker : Game
    {
        public TieBreaker(List<MatchTeam> setTeam, int servingPlayer) : base(setTeam, servingPlayer)
        {
        }

        public override void AddScore(MatchTeam team)
        {
            MatchTeam otherTeam = this.MatchTeams.Where(t => t != team).FirstOrDefault();
            team.Score += 1;
            if ((team.Score >= 7 && team.Score - otherTeam.Score >= 2))
            {
                DI.Resolver.Resolve<IEventDispatcher>().Dispatch(this, new MatchUnitFinishedEvent(team));
                DI.Resolver.Resolve<IEventDispatcher>().CleanupSource(this);
            }
            else
            {
                this.SwitchServingPlayer();
                this.Play();
            }
        }

        private void SwitchServingPlayer()
        {
            this.ServingPlayer = SwitchServingPlayer(this.ServingPlayer);
        }
    }

    public class MatchTeam 
    {
        public Player Player
        {
            get; private set;
        }

        public int Score
        {
            get; set;
        }

        public MatchTeam(Player player)
        {
            this.Player = player;
        }
    }
        

}
