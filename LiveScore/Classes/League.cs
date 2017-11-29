using System.Collections.Generic;

namespace LiveScore.Classes
{
    class League
    {
        private int id;
        private string name;
        private int tournamentId;
        private string tournamentName;

        private List<Event> eventList;

        public League(int leagueId, string leagueName, int tournamentId)
        {
            this.Id = leagueId;
            this.Name = leagueName;
            this.TournamentId = tournamentId;
            this.EventList = new List<Event>();
        }

        public string Name { get => name; set => name = value; }
        public int Id { get => id; set => id = value; }
        public int TournamentId { get => tournamentId; set => tournamentId = value; }
        public string TournamentName { get => tournamentName; set => tournamentName = value; }
        internal List<Event> EventList { get => eventList; set => eventList = value; }

        public void AddEvent(Event evnt)
        {
            bool hasEvent = false;

            foreach(Event ev in eventList)
            {
                if(ev.Id == evnt.Id)
                {
                    hasEvent = true;
                    break;
                }
            }

            if (!hasEvent)
                this.EventList.Add(evnt);
        }

        public string GetFullLeagueName()
        {
            return TournamentName + ", " + Name;
        }



    }
}
