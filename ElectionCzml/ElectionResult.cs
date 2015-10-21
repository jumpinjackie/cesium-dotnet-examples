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

        //Key - Divison Name - Value - Division ID
        private Dictionary<string, int> _divIdToNameMap;

        public ElectionResultSet()
        {
            _results = new Dictionary<int, List<DatedElectionResult>>();
            _divIdToNameMap = new Dictionary<string, int>();
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
