using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionCzml
{
    public class ElectionResultSet
    {
        //Key - Division, Value - Election result
        private Dictionary<int, List<DatedElectionResult>> _results;

        //Key - Divison Name, Value - Division ID
        private Dictionary<string, int> _divIdToNameMap;

        //Key - Divison Name, Value - TPP result
        private Dictionary<string, List<DatedTPPResult>> _tpp;

        public ElectionResultSet()
        {
            _results = new Dictionary<int, List<DatedElectionResult>>();
            _divIdToNameMap = new Dictionary<string, int>();
            _tpp = new Dictionary<string, List<DatedTPPResult>>();
        }

        public string[] GetParties()
        {
            return _results.SelectMany(r => r.Value).Select(r => r.PartyNm).Distinct().ToArray();
        }

        public void LoadElectionResults<T>(DateTime date, IEnumerable<T> results) where T : IElectionResult
        {
            foreach (var result in results)
            {
                if (!_results.ContainsKey(result.DivisionID))
                    _results[result.DivisionID] = new List<DatedElectionResult>();

                _results[result.DivisionID].Add(new DatedElectionResult(date, result));

                string nname = result.DivisionNm.ToLower();
                if (!_divIdToNameMap.ContainsKey(nname))
                    _divIdToNameMap[nname] = result.DivisionID;
            }
        }

        public void LoadTPPResults<T>(DateTime date, IEnumerable<T> results) where T : ITPPResult
        {
            foreach (var result in results)
            {
                string nname = result.DivisionNm.ToLower();
                if (!_tpp.ContainsKey(nname))
                    _tpp[nname] = new List<DatedTPPResult>();

                _tpp[nname].Add(new DatedTPPResult(date, result));
            }
        }
        
        public DatedTPPResult[] GetTPPForDivison(string name)
        {
            string nname = name.ToLower();
            if (_tpp.ContainsKey(nname))
            {
                return _tpp[nname].OrderBy(r => r.ElectionDate).ToArray();
            }
            return new DatedTPPResult[0];
        }

        public DatedElectionResult[] GetResultsForDivison(string name)
        {
            string nname = name.ToLower();
            if (_divIdToNameMap.ContainsKey(nname))
            {
                int id = _divIdToNameMap[nname];
                if (_results.ContainsKey(id))
                {
                    return _results[id].OrderBy(r => r.ElectionDate).ToArray();
                }
            }
            return new DatedElectionResult[0];
        }
    }

    public interface ITPPResult
    {
        string DivisionNm { get; }

        int CoalitionVotes { get; }

        double CoalitionPc { get; }

        int LaborVotes { get; }

        double LaborPc { get; }

        int TotalVotes { get; }

        double Swing { get; }
    }

    public class DatedTPPResult : ITPPResult
    {
        public DatedTPPResult(DateTime date, ITPPResult result)
        {
            this.ElectionDate = date;
            this.DivisionNm = result.DivisionNm;
            this.CoalitionPc = result.CoalitionPc;
            this.CoalitionVotes = result.CoalitionVotes;
            this.LaborPc = result.LaborPc;
            this.LaborVotes = result.LaborVotes;
            this.Swing = result.Swing;
            this.TotalVotes = result.TotalVotes;
        }

        public DateTime ElectionDate { get; set; }

        public string DivisionNm { get; set; }

        public int CoalitionVotes { get; set; }

        public double CoalitionPc { get; set; }

        public int LaborVotes { get; set; }

        public double LaborPc { get; set; }

        public int TotalVotes { get; set; }

        public double Swing { get; set; }
    }

    public class ByElectionPollingBoothTPPResultClassMap : CsvClassMap<ByElectionPollingBoothTPPResult>
    {
        public ByElectionPollingBoothTPPResultClassMap()
        {
            Map(m => m.StateAb);
            Map(m => m.DivisionID);
            Map(m => m.DivisionNm);
            Map(m => m.PollingPlaceID);
            Map(m => m.PollingPlace);
            Map(m => m.LaborVotes).Name("Australian Labor Party Votes");
            Map(m => m.LaborPc).Name("Australian Labor Party Percentage");
            Map(m => m.CoalitionVotes).Name("Liberal/National Coalition Votes");
            Map(m => m.CoalitionPc).Name("Liberal/National Coalition Percentage");
            Map(m => m.TotalVotes);
            Map(m => m.Swing);
        }
    }

    public class ByElectionPollingBoothTPPResult : ITPPResult
    {
        public string StateAb { get; set; }

        public int DivisionID { get; set; }

        public string DivisionNm { get; set; }

        public int PollingPlaceID { get; set; }

        public string PollingPlace { get; set; }

        public int CoalitionVotes { get; set; }

        public double CoalitionPc { get; set; }

        public int LaborVotes { get; set; }

        public double LaborPc { get; set; }

        public int TotalVotes { get; set; }

        public double Swing { get; set; }
    }

    public class FederalTPPResultClassMap : CsvClassMap<FederalTPPResult>
    {
        public FederalTPPResultClassMap()
        {
            Map(m => m.DivisionID);
            Map(m => m.DivisionNm);
            Map(m => m.StateAb);
            Map(m => m.PartyAb);
            Map(m => m.CoalitionVotes).Name("Liberal/National Coalition Votes");
            Map(m => m.CoalitionPc).Name("Liberal/National Coalition Percentage");
            Map(m => m.LaborVotes).Name("Australian Labor Party Votes");
            Map(m => m.LaborPc).Name("Australian Labor Party Percentage");
            Map(m => m.TotalVotes);
            Map(m => m.Swing);
        }
    }

    public class FederalTPPResult : ITPPResult
    {
        public int DivisionID { get; set; }

        public string DivisionNm { get; set; }

        public string StateAb { get; set; }

        public string PartyAb { get; set; }

        public int CoalitionVotes { get; set; }

        public double CoalitionPc { get; set; }

        public int LaborVotes { get; set; }

        public double LaborPc { get; set; }

        public int TotalVotes { get; set; }

        public double Swing { get; set; }
    }
    
    public interface IElectionResult
    {
        int DivisionID { get; }

        string DivisionNm { get; }

        string StateAb { get; }

        string PartyNm { get; }

        string Surname { get; }
        
        string GivenNm { get; }
    }

    public class DatedElectionResult : IElectionResult
    {
        public DatedElectionResult(DateTime date, IElectionResult result)
        {
            this.ElectionDate = date;
            this.DivisionID = result.DivisionID;
            this.GivenNm = result.GivenNm;
            this.PartyNm = result.PartyNm;
            this.StateAb = result.StateAb;
            this.Surname = result.Surname;
            this.DivisionNm = result.DivisionNm;
        }

        public int DivisionID { get; set; }

        public string GivenNm { get; set; }

        public string PartyNm { get; set; }

        public string StateAb { get; set; }

        public string Surname { get; set; }

        public DateTime ElectionDate { get; set; }
        
        public string DivisionNm { get; set; }
    }
    
    public class ElectionResult : IElectionResult
    {
        public int DivisionID { get; set; }

        public string DivisionNm { get; set; }

        public string StateAb { get; set; }

        public int CandidateID { get; set; }

        public string GivenNm { get; set; }

        public string Surname { get; set; }

        public string PartyNm { get; set; }

        public string PartyAb { get; set; }
    }
    
    public class ByElectionResult : IElectionResult
    {
        public string StateAb { get; set; }

        public int DivisionID { get; set; }

        public string DivisionNm { get; set; }

        public string PartyAb { get; set; }

        public string PartyNm { get; set; }

        public int CandidateID { get; set; }

        public string Surname { get; set; }

        public string GivenNm { get; set; }

        public string Elected { get; set; }

        public string HistoricElected { get; set; }
    }
}
