namespace LiveScore.Classes
{
    class Event
    {
        private long id;
        private int leagueId;
        private string leagueName;
        private string tournamentName;

        private string homeTeam;
        private string awayTeam;
        private int homeGoals;
        private int awayGoals;
        private int homeCorners;
        private int awayCorners;

        private string playTime;
        private string startTime;

        public Event(long id, int leagueId, string homeTeam, string awayTeam, string startTime)
        {
            this.Id = id;
            this.LeagueId = leagueId;
            this.HomeTeam = homeTeam;
            this.AwayTeam = awayTeam;
            this.startTime = startTime;
        }

        public Event(long id, int leagueId, string homeTeam, string awayTeam, int homeGoals, int awayGoals, int homeCorners, int awayCorners, string playTime, string startTime)
        {
            this.Id = id;
            this.LeagueId = leagueId;
            this.HomeTeam = homeTeam;
            this.AwayTeam = awayTeam;
            this.HomeGoals = homeGoals;
            this.AwayGoals = awayGoals;
            this.HomeCorners = homeCorners;
            this.AwayCorners = awayCorners;
            this.PlayTime = playTime;
            this.StartTime = startTime;
        }



        public long Id { get => id; set => id = value; }
        public int LeagueId { get => leagueId; set => leagueId = value; }
        public string HomeTeam { get => homeTeam; set => homeTeam = value; }
        public string AwayTeam { get => awayTeam; set => awayTeam = value; }
        public int HomeGoals { get => homeGoals; set => homeGoals = value; }
        public int AwayGoals { get => awayGoals; set => awayGoals = value; }
        public int HomeCorners { get => homeCorners; set => homeCorners = value; }
        public int AwayCorners { get => awayCorners; set => awayCorners = value; }
        public string PlayTime { get => playTime; set => playTime = value; }
        public string StartTime { get => startTime; set => startTime = value; }
        public string LeagueName { get => leagueName; set => leagueName = value; }
        public string TournamentName { get => tournamentName; set => tournamentName = value; }
    }
}
