using System.Collections.Generic;

namespace LiveScore.Classes
{
    class Tournament
    {
        private int id;
        private string name;
        private int leaguesCount;
        private List<League> leagueList;

        public Tournament(int id, string name, int leaguesCount)
        {
            this.Id = id;
            this.Name = name;
            this.LeaguesCount = leaguesCount;
            this.leagueList = new List<League>();
        }

        public int Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public int LeaguesCount { get => leaguesCount; set => leaguesCount = value; }

        public void SetLeagueInList(League league)
        {
            bool hasLeague = false;
            foreach (League currentLeague in leagueList)
            {
                if(currentLeague.Id == league.Id)
                {
                    hasLeague = true;
                    break;
                }
            }

            if (!hasLeague)
            {
                leagueList.Add(league);
            }
        }
    }
}
