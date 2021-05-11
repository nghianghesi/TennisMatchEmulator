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
        List<MatchUnit> SubUnits;
        MatchUnit CurrentSubUnit => SubUnits?.LastOrDefault();

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
        public Match(List<Player> players)
        {
            if (players == null || players.Count != 2 || players[0] == null || players[1] == null || players[0] == players[1])
            {
                throw new InvalidOperationException("Need 2 players");
            }
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

    public class Set : CompositMatchUnit
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
            if (team.Score >= 6 && team.Score - otherTeam.Score >= 2)
            {
                DI.Resolver.Resolve<IEventDispatcher>().Dispatch(this, new MatchUnitFinishedEvent(team));
            }
            else
            {
                this.Play();
            }
        }

        public override MatchTeam GetWinTeam()
        {
            MatchTeam team = this.MatchTeams.Where(t => t.Score >= 6).FirstOrDefault();
            MatchTeam otherTeam = this.MatchTeams.Where(t => t != team).FirstOrDefault();
            if (team!=null && otherTeam != null && team.Score - otherTeam.Score >= 2)
            { 
                return team; 
            }

            return null;
        }

        public override void Play()
        {
            
        }
    }

    public class Game : MatchUnit, IEventHandler<Player, BallHittedEvent>, IEventHandler<Player, FailedHitEvent>
    {
        private int servingPlayer;
        public Game(List<Player> players, int servingPlayer)
        {
            this.servingPlayer = servingPlayer;
            foreach (Player p in players)
            {
                this.MatchTeams.Add(new MatchTeam(p));
                DI.Resolver.Resolve<IEventDispatcher>().AddHandler<Player, BallHittedEvent>(p, this);
                DI.Resolver.Resolve<IEventDispatcher>().AddHandler<Player, FailedHitEvent>(p, this);
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

        public void Handle(Player source, BallHittedEvent e)
        {
            Player otherPlayer = this.MatchTeams.Where(t => t.Player != source).FirstOrDefault()?.Player;
            if(otherPlayer != null)
            {
                otherPlayer.HanleBall();
            }
        }

        public void Handle(Player source, FailedHitEvent e)
        {
            MatchTeam otherTeam = this.MatchTeams.Where(t => t.Player != source).FirstOrDefault();
            if (otherTeam != null)
            {
                this.AddScore(otherTeam);
            }
        }

        public override void Play()
        {
            this.MatchTeams[this.servingPlayer].Player.HanleBall();
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
