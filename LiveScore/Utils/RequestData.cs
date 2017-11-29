using LiveScore.Classes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System;
using System.IO;
using Firebase.Auth;
using System.Windows.Forms;
using Firebase.Auth.Payloads;
using System.Threading.Tasks;

namespace LiveScore.Utils
{
    public class RequestData
    {
        private const string EMAIL = "ivailoinfo@gmail.com";
        private const string PASSWORD = "123123123";

        private string prematch;
        private string inplay;
        private string today;

        private List<Tournament> tournamentList;
        private List<League> leagueList;
        internal List<Event> eventList;

        public RequestData(string prematch, string inplay, string today)
        {
            this.prematch = prematch;
            this.inplay = inplay;
            this.today = today;
            tournamentList = new List<Tournament>();
            leagueList = new List<League>();
            eventList = new List<Event>();
        }

        #region Json Classes
        public class JsonTournamentMain
        {
            public string status { get; set; }
            public string time { get; set; }
            public string ms { get; set; }
            public List<JsonTournament> data { get; set; }
        }

        public class JsonTournament
        {
            public int id { get; set; }
            public string name { get; set; }
            public int prematch_leagues_cnt { get; set; }
            public int inplay_leagues_cnt { get; set; }
        }

        public class JsonLeagueMain
        {
            public string status { get; set; }
            public string time { get; set; }
            public string ms { get; set; }
            public List<JsonLeague> data { get; set; }
        }

        public class JsonLeague
        {
            public int id { get; set; }
            public int tournament_id { get; set; }
            public string name { get; set; }
            public int prematch_matches_cnt { get; set; }
            public int inplay_matches_cnt { get; set; }
        }

        public class JsonEventMain
        {
            public string status { get; set; }
            public string time { get; set; }
            public string ms { get; set; }
            public List<JsonEvent> data { get; set; }
        }

        public class JsonEvent
        {
            public int id { get; set; }
            public int league_id { get; set; }
            public Teams teams { get; set; }
            public string start { get; set; }
            public Active active { get; set; }
            public Status stats { get; set; }
        }

        public class Status
        {
            public string start { get; set; }
            public Runtime runtime { get; set; }
        }

        public class Runtime
        {
            public Play play { get; set; }
            public Data goals { get; set; }
            public Data corners { get; set; }
        }

        public class Data
        {
            public int home { get; set; }
            public int away { get; set; }
            public string change { get; set; }
        }

        public class Play
        {
            public int codec { get; set; }
            public string display { get; set; }
        }

        public class Active
        {
            public int prematch { get; set; }
            public int inplay { get; set; }
        }

        public class Teams
        {
            public string home { get; set; }
            public string away { get; set; }
        }
        #endregion

        private static async Task<string> Execute(DownloadTask downloadTask, string result)
        {
            try
            {
                result = await downloadTask.ExecuteRequestAsync();
            }
            catch (WebException e)
            {
                //throw new WebException();
            }

            return result;
        }

        private async Task<JsonTournamentMain> GetTournamentsAsync()
        {
            DownloadTask downloadTask;
            try
            {
                downloadTask = new DownloadTask("http://www.oddstorm.com/feeds_api/", "POST");
                downloadTask.SetPostParams("{\"prematch\": " + this.prematch + ",\"inplay\": " + this.inplay + " }");
            }
            catch
            {
                return null;
            }

            string result = String.Empty;
            result = await Execute(downloadTask, result);

            if (result == String.Empty)
                return null;

            return JsonConvert.DeserializeObject<JsonTournamentMain>(result);
        }

        private async Task<JsonLeagueMain> GetLeaguesAsync(JsonTournamentMain tournamentsResult)
        {
            if (tournamentsResult == null || tournamentsResult.data.Count <= 0)
            {
                return null;
            }

            List<int> tournamentIds = new List<int>(tournamentsResult.data.Count);

            foreach (JsonTournament tournament in tournamentsResult.data)
            {
                tournamentList.Add(new Tournament(tournament.id, tournament.name, tournament.inplay_leagues_cnt));
                tournamentIds.Add(tournament.id);
            }

            string ids = string.Join(",", tournamentIds);

            DownloadTask downloadTask;
            try
            {
                downloadTask = new DownloadTask("http://www.oddstorm.com/feeds_api/", "POST");
                downloadTask.SetPostParams("{\"prematch\": " + this.prematch + ",\"inplay\": " + this.inplay + ",  \"scope\": \"leagues\",  \"tournament_ids\": [" + ids + "] }");
            }
            catch
            {
                return null;
            }
            string result = String.Empty;
            result = await Execute(downloadTask, result);

            if (result == String.Empty)
                return null;

            return JsonConvert.DeserializeObject<JsonLeagueMain>(result);
        }

        private async Task<JsonEventMain> GetEventsAsync(JsonLeagueMain leagues)
        {
            if (leagues == null || leagues.data.Count <= 0)
            {
                return null;
            }

            List<int> leaguesIds = new List<int>();

            foreach (JsonLeague league in leagues.data)
            {
                leagueList.Add(new League(league.id, league.name, league.tournament_id));
                leaguesIds.Add(league.id);
            }

            string ids = string.Join(",", leaguesIds);

            DownloadTask downloadTask;
            try
            {
                downloadTask = new DownloadTask("http://www.oddstorm.com/feeds_api/", "POST");
                downloadTask.SetPostParams("{\"prematch\": " + this.prematch + ",\"inplay\": " + this.inplay + ",  \"scope\": \"matches\",  \"league_ids\": [" + ids + "] }");
            }
            catch
            {
                return null;
            }
            string result = String.Empty;
            result = await Execute(downloadTask, result);

            if (result == String.Empty)
                return null;

            //var result = downloadTask.Response;
            return JsonConvert.DeserializeObject<JsonEventMain>(result);
        }

        internal async Task<List<Event>> GetDataAsync()
        {
            JsonTournamentMain tournamentMain = await GetTournamentsAsync();
            JsonLeagueMain leagueMain = await GetLeaguesAsync(tournamentMain);
            JsonEventMain eventMain = new JsonEventMain();

            if (leagueMain.data.Count > 100)
            {
                List<JsonLeague> jsonLeaguesMain = new List<JsonLeague>();
                JsonEventMain currentEvents = new JsonEventMain();
                JsonLeagueMain currentJsonLeagueMain = new JsonLeagueMain();

                jsonLeaguesMain = leagueMain.data;

                while (jsonLeaguesMain.Count > 100)
                {
                    currentJsonLeagueMain.data = jsonLeaguesMain.GetRange(0, 100);
                    currentEvents = await GetEventsAsync(currentJsonLeagueMain);

                    eventMain.data = currentEvents.data.ToList();
                    jsonLeaguesMain.RemoveRange(0, 100);
                }
                currentJsonLeagueMain.data.Clear();
                currentJsonLeagueMain.data = jsonLeaguesMain.ToList();

                currentEvents = await GetEventsAsync(currentJsonLeagueMain);
                eventMain.data.AddRange(currentEvents.data);
            }
            else
            {
                eventMain = await GetEventsAsync(leagueMain);
            }

            if (tournamentMain == null || leagueMain == null || eventMain == null)
            {
                return null;
            }

            if (this.inplay.Equals("true"))
            {
                FillLiveEvents(eventMain);
            }
            else
            {
                FillProgramEvents(eventMain);
            }

            return eventList;
        }

        private void FillProgramEvents(JsonEventMain eventMain)
        {
            foreach (JsonEvent evnt in eventMain.data)
            {
                bool hasEvent = false;
                foreach (Event e in eventList)
                {
                    if (evnt.id == e.Id)
                    {
                        hasEvent = true;
                    }
                }

                long id = evnt.id;
                int leagueId = evnt.league_id;
                string homeTeam = evnt.teams.home;
                string awayTeam = evnt.teams.away;
                string startTime = evnt.start;

                if (!hasEvent && startTime.Contains("T"))
                {
                    if (this.today.Equals("true"))
                    {
                        DateTime currentDateTime = DateTime.UtcNow;
                        DateTime eventDateTime = DateTime.ParseExact(startTime, "yyyy-MM-ddTHH:mm:ss", null);

                        if (currentDateTime.Day == eventDateTime.Day && currentDateTime.Month == eventDateTime.Month && currentDateTime.Hour < eventDateTime.Hour)
                            eventList.Add(new Event(id, leagueId, homeTeam, awayTeam, startTime));
                    }
                    else
                    {
                        eventList.Add(new Event(id, leagueId, homeTeam, awayTeam, startTime));
                    }
                }

            }

            foreach (Event evnt in eventList)
            {
                foreach (League league in leagueList)
                {
                    if (evnt.LeagueId == league.Id)
                    {
                        evnt.LeagueName = league.Name;

                        foreach (Tournament tournament in tournamentList)
                        {
                            if (tournament.Id == league.TournamentId)
                            {
                                evnt.TournamentName = tournament.Name;
                                league.TournamentName = tournament.Name;
                                break;
                            }
                        }
                    }
                }

            }
        }

        private void FillLiveEvents(JsonEventMain eventMain)
        {
            foreach (JsonEvent evnt in eventMain.data)
            {
                bool hasEvent = false;
                foreach (Event e in eventList)
                {
                    if (evnt.id == e.Id)
                    {
                        hasEvent = true;
                    }
                }

                long id = evnt.id;
                int leagueId = evnt.league_id;
                string homeTeam = evnt.teams.home;
                string awayTeam = evnt.teams.away;
                int homeGoals = evnt.stats.runtime.goals.home;
                int awayGoals = evnt.stats.runtime.goals.away;
                int homeCorners = evnt.stats.runtime.corners.home;
                int awayCorners = evnt.stats.runtime.corners.away;
                string playTime = evnt.stats.runtime.play.display;
                string startTime = evnt.start;

                if (!hasEvent && !playTime.Equals("n/a") && startTime.Contains("T") && homeGoals > -1 && awayGoals > -1)
                    eventList.Add(new Event(id, leagueId, homeTeam, awayTeam, homeGoals, awayGoals, homeCorners, awayCorners, playTime, startTime));
            }

            foreach (Event evnt in eventList)
            {
                foreach (League league in leagueList)
                {
                    if (evnt.LeagueId == league.Id)
                    {
                        evnt.LeagueName = league.Name;

                        foreach (Tournament tournament in tournamentList)
                        {
                            if (tournament.Id == league.TournamentId)
                            {
                                evnt.TournamentName = tournament.Name;
                                league.TournamentName = tournament.Name;
                                break;
                            }
                        }
                    }
                }

            }
        }

        internal async Task UploadDataAsync(string authToken, string url)
        {
            eventList = SortEventsList(eventList);

            var json = JsonConvert.SerializeObject(new
            {
                eventList
            });

            var request = WebRequest.CreateHttp(url + authToken);

            try
            {
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = 3000; //milliseconds
                var buffer = Encoding.UTF8.GetBytes(json);
                request.ContentLength = buffer.Length;
                request.GetRequestStream().Write(buffer, 0, buffer.Length);
                var response = await request.GetResponseAsync();
                json = (new StreamReader(response.GetResponseStream())).ReadToEnd();
                response.Close();
            }
            catch (WebException e)
            {
                throw new WebException(e.Message);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        private static List<Event> SortEventsList(List<Event> events)
        {
            return events.OrderBy(x => x.TournamentName).ThenBy(x => x.LeagueName).ToList();
        }

        internal async Task ClearData(string authToken, string url, string apiKey)
        {
            if (authToken == String.Empty)
            {
                await ConnectApiAsync(apiKey);
            }

            WebRequest request = WebRequest.CreateHttp(url + authToken);
            request.Method = "DELETE";
            WebResponse response = request.GetResponse();
            response.Close();
        }

        internal async Task<string> ConnectApiAsync(string apiKey)
        {
            var authOptions = new FirebaseAuthOptions(apiKey);
            var firebase = new FirebaseAuthService(authOptions);
            var request = new VerifyPasswordRequest()
            {
                Email = EMAIL,
                Password = PASSWORD
            };

            string authToken = String.Empty;
            try
            {
                var response = await firebase.VerifyPassword(request);
                authToken = response.IdToken;

                //authToken = response.RefreshToken;                
            }
            catch (FirebaseAuthException e)
            {
                Application.Restart();
            }

            return authToken;
        }

        internal async Task CheckAuthToken()
        {
            //connect to API to get the AUTH_TOKEN
            if (Program.AUTH_TOKEN == String.Empty)
            {
                Program.AUTH_TOKEN = await ConnectApiAsync(Program.API_KEY);
            }
        }
    }
}
