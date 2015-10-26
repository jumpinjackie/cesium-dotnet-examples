using CsvHelper;
using Newtonsoft.Json;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionCzml
{
    class Program
    {
        //Our story starts on 9 Oct 2004
        static string _sceneStart = "2004-10-09T00:00:00Z";

        //And ends on 8 Feb 2014
        static string _sceneEnd = "2014-02-08T00:00:00Z";

        static DateTime _dtStart = new DateTime(2004, 10, 9);
        static DateTime _dtEnd = new DateTime(2014, 2, 8);

        static void WriteCzmlDocumentHeader(JsonTextWriter writer)
        {
            

            /*
            {
                "id": "document",
                "version": "1.0"
                "clock": {
                    "interval": "<start>/<end>",
                    "currentTime": "<start>",
                    "range": "LOOP_STOP",
                    "step": "SYSTEM_CLOCK_MULTIPLIER"
                }
            }
            
            */
            writer.WriteStartObject();
            {
                writer.WritePropertyName("id");
                writer.WriteValue("document");
                writer.WritePropertyName("version");
                writer.WriteValue("1.0");
                writer.WritePropertyName("clock");
                writer.WriteStartObject();
                {
                    writer.WritePropertyName("interval");
                    writer.WriteValue($"{_sceneStart}/{_sceneEnd}");
                    writer.WritePropertyName("currentTime");
                    writer.WriteValue(_sceneStart);
                    writer.WritePropertyName("multiplier");
                    writer.WriteValue(2592000); // 1s -> 30 days
                    writer.WritePropertyName("range");
                    writer.WriteValue("LOOP_STOP");
                    writer.WritePropertyName("step");
                    writer.WriteValue("SYSTEM_CLOCK_MULTIPLIER");
                }
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }

        const int COLOR_ALPHA = 160;

        const double EXTRUSION_MULTIPLIER = 1.0;

        //static Dictionary<string, double> _areas = new Dictionary<string, double>();

        //This value was determined through a test run with geometric areas collected and sampling the
        //smallest value among the biggest values of each electorate
        const double MIN_AREA = 0.0028;

        static void WriteComponentGeometryFragment(JsonTextWriter writer, Geometry geom, string name, DatedElectionResult[] results, DatedTPPResult[] tppResults, int? no = null)
        {
            if (results.Length == 0)
                return;

            int number = no.HasValue ? no.Value : 0;
            double area = geom.Area();

            //To keep the CZML payload to only the main areas (and not islands), don't write CZML packets for
            //island parts
            if (area < MIN_AREA)
                return;

            //Debug.WriteLine($"{name}: {number} - {area}");

            //if (!_areas.ContainsKey(name))
            //    _areas[name] = area;
            //else
            //    _areas[name] = Math.Max(_areas[name], area);

            // {
            writer.WriteStartObject();
            {
                var ring = geom.GetGeometryRef(0);
                var ptCount = ring.GetPointCount();

                // "id": "Electorate/<electorate>/<number>"
                writer.WritePropertyName("id");
                writer.WriteValue($"Electorate/{name}/{number}");
                // "name": "<electorate>"
                writer.WritePropertyName("name");
                writer.WriteValue("Federal Electorate");
                // "description": "<electorate>"
                writer.WritePropertyName("description");
                writer.WriteValue(name);
                // "parent": "Electorate/<electorate>"
                writer.WritePropertyName("parent");
                writer.WriteValue($"Electorate/{name}");
                
                // "polygon": {
                writer.WritePropertyName("polygon");
                writer.WriteStartObject();
                {
                    // "outline" {
                    writer.WritePropertyName("outline");
                    writer.WriteStartObject();
                    {
                        writer.WritePropertyName("boolean");
                        writer.WriteValue(true);
                    }
                    writer.WriteEndObject(); // }

                    // "outlineColor" {
                    writer.WritePropertyName("outlineColor");
                    writer.WriteStartObject();
                    {
                        // "rgba": [
                        writer.WritePropertyName("rgba");
                        writer.WriteStartArray();
                        {
                            //Black
                            writer.WriteValue(0);
                            writer.WriteValue(0);
                            writer.WriteValue(0);
                            writer.WriteValue(255);
                        }
                        writer.WriteEndArray(); // ]
                    }
                    writer.WriteEndObject(); // }

                    // "material": {
                    writer.WritePropertyName("material");
                    writer.WriteStartObject();
                    {
                        // "solidColor": {
                        writer.WritePropertyName("solidColor");
                        writer.WriteStartObject();
                        {
                            // "color": {
                            writer.WritePropertyName("color");
                            writer.WriteStartObject();
                            {
                                // "epoch": <start>
                                writer.WritePropertyName("epoch");
                                writer.WriteValue(_sceneStart);

                                // "rgba": [
                                writer.WritePropertyName("rgba");
                                writer.WriteStartArray();
                                {
                                    //HACK: This example doesn't take into account that electorate redistribution occurs
                                    //Meaning that our 2015 snapshot of Australian Federal Electorates may contain electorates
                                    //that either have changed or did not exist in earlier Federal Elections. Since the AEC does
                                    //not publicly provide such data, we'll denote such electorates as unknown (White)
                                    //
                                    //So peek at the first result, if it's dated on our first Federal Election we know the
                                    //electorate existed back then
                                    bool bDidNotExistIn2004 = !(results[0].ElectionDate.Year == _dtStart.Year &&
                                        results[0].ElectionDate.Month == _dtStart.Month &&
                                        results[0].ElectionDate.Day == _dtStart.Day);

                                    if (bDidNotExistIn2004)
                                    {
                                        //0 seconds since epoch
                                        writer.WriteValue(0);
                                        //White
                                        writer.WriteValue(255);
                                        writer.WriteValue(255);
                                        writer.WriteValue(255);
                                        writer.WriteValue(COLOR_ALPHA);
                                    }
                                    else
                                    {
                                        //0 seconds since epoch
                                        writer.WriteValue(0);

                                        int[] color = GetPartyColor(results[0].PartyNm);
                                        foreach (var c in color)
                                        {
                                            writer.WriteValue(c);
                                        }
                                    }

                                    foreach (var result in results.Skip(1))
                                    {
                                        int dt = (int)result.ElectionDate.Subtract(_dtStart).TotalSeconds;
                                        writer.WriteValue(dt);
                                        int[] color = GetPartyColor(result.PartyNm);
                                        foreach (var c in color)
                                        {
                                            writer.WriteValue(c);
                                        }
                                    }
                                }
                                writer.WriteEndArray(); // ]
                            }
                            writer.WriteEndObject(); // }
                        }
                        writer.WriteEndObject(); // }
                    }
                    writer.WriteEndObject(); // }

                    // "extrudedHeight": {
                    /*
                    writer.WritePropertyName("extrudedHeight");
                    writer.WriteStartObject();
                    {
                        // "epoch": <start>
                        writer.WritePropertyName("epoch");
                        writer.WriteValue(_sceneStart);

                        // "number": [
                        writer.WritePropertyName("number");
                        writer.WriteStartArray();
                        {
                            //HACK: This example doesn't take into account that electorate redistribution occurs
                            //Meaning that our 2015 snapshot of Australian Federal Electorates may contain electorates
                            //that either have changed or did not exist in earlier Federal Elections. Since the AEC does
                            //not publicly provide such data, we'll denote such electorates as unknown (White)
                            //
                            //So peek at the first result, if it's dated on our first Federal Election we know the
                            //electorate existed back then
                            bool bDidNotExistIn2004 = !(tppResults[0].ElectionDate.Year == _dtStart.Year &&
                                tppResults[0].ElectionDate.Month == _dtStart.Month &&
                                tppResults[0].ElectionDate.Day == _dtStart.Day);

                            if (bDidNotExistIn2004)
                            {
                                //0 seconds since epoch
                                writer.WriteValue(0);
                                //0 height
                                writer.WriteValue(0);
                            }
                            else
                            {
                                //0 seconds since epoch
                                writer.WriteValue(0);
                                //The winning party's percentage
                                writer.WriteValue(Math.Max(tppResults[0].LaborPc, tppResults[0].CoalitionPc) * EXTRUSION_MULTIPLIER);
                            }

                            foreach (var tppResult in tppResults.Skip(1))
                            {
                                int dt = (int)tppResult.ElectionDate.Subtract(_dtStart).TotalSeconds;
                                writer.WriteValue(dt);
                                //The winning party's percentage
                                writer.WriteValue(Math.Max(tppResults[0].LaborPc, tppResults[0].CoalitionPc) * EXTRUSION_MULTIPLIER);
                            }
                        }
                        writer.WriteEndArray(); // ]
                    }
                    writer.WriteEndObject(); // }
                    */
                    // "positions": {
                    writer.WritePropertyName("positions");
                    writer.WriteStartObject();
                    {
                        // "cartographicDegrees": [
                        writer.WritePropertyName("cartographicDegrees");
                        writer.WriteStartArray();
                        {
                            double[] coords = new double[3];
                            foreach (int idx in Enumerable.Range(0, ptCount - 1))
                            {
                                ring.GetPoint(idx, coords);
                                foreach (double coord in coords)
                                {
                                    writer.WriteValue(coord);
                                }
                            }
                        }
                        writer.WriteEndArray(); // ]
                    }
                    writer.WriteEndObject(); // }
                }
                writer.WriteEndObject(); // }
            }
            writer.WriteEndObject(); // }
        }
        
        //static string[] _parties;

        private static int[] GetPartyColor(string partyNm)
        {
            switch(partyNm)
            {
                case "Australian Labor Party":
                case "Australian Labor Party (State of Queensland)":
                    return new int[] { 255, 0, 0, COLOR_ALPHA }; //Red
                case "Liberal":
                case "Liberal National Party of Queensland":
                case "Liberal National Party":
                case "Country Liberals (NT)":
                case "CLP - The Territory Party":
                case "The Nationals":
                    return new int[] { 0, 0, 255, COLOR_ALPHA }; //Blue
                case "The Greens":
                    return new int[] { 0, 255, 0, COLOR_ALPHA }; //Green
                case "Palmer United Party":
                    return new int[] { 255, 255, 0, COLOR_ALPHA }; //Yellow
                default:
                    return new int[] { 84, 84, 84, COLOR_ALPHA }; //Grey
            }
        }

        static void WritePacket(JsonTextWriter writer, Feature feat, string name, DatedElectionResult[] results, DatedTPPResult[] tppResults)
        {
            Console.WriteLine($"Writing CZML packets for electorate: {name} ");
            
            writer.WriteStartObject(); // {
            {
                // "id": "Electorate/<electorate>"
                writer.WritePropertyName("id");
                writer.WriteValue($"Electorate/{name}");
                // "name": "<electorate>"
                writer.WritePropertyName("name");
                writer.WriteValue(name);
            }
            writer.WriteEndObject(); // }

            var geom = feat.GetGeometryRef();
            string geomName = geom.GetGeometryName();
            if (geomName == "MULTIPOLYGON")
            {
                var subGeomCount = geom.GetGeometryCount();
                foreach (int idx in Enumerable.Range(0, subGeomCount - 1))
                {
                    var subGeom = geom.GetGeometryRef(idx);
                    WriteComponentGeometryFragment(writer, subGeom, name, results, tppResults, idx);
                    Console.Write(".");
                }
                Console.WriteLine();
            }
            else if (geomName == "POLYGON")
            {
                WriteComponentGeometryFragment(writer, geom, name, results, tppResults);
            }
        }

        class InputFile
        {
            public DateTime Date { get; set; }

            public StreamReader Source { get; set; }

            public StreamReader TPPSource { get; set; }

            public bool General { get; set; }

            public void Close()
            {
                Source?.Close();
                TPPSource?.Close();
            }
        }

        static void Main(string[] args)
        {
            GdalConfiguration.ConfigureOgr();
            var dataSource = Ogr.Open("Electorates.shp", 0);
            try
            {
                ElectionResultSet results = new ElectionResultSet();

                var resultFiles = new InputFile[]
                {
                    //2004 Federal Election (9 October 2004)
                    new InputFile
                    {
                        Date = new DateTime(2004, 10, 9),
                        Source = File.OpenText("2004-HouseMembersElectedDownload-12246.csv"),
                        TPPSource = File.OpenText("2004-HouseTppByDivisionDownload-12246.csv"),
                        General = true
                    },
                    //2007 Federal Election (24 November 2007)
                    new InputFile
                    {
                        Date = new DateTime(2007, 11, 24),
                        Source = File.OpenText("2007-HouseMembersElectedDownload-13745.csv"),
                        TPPSource = File.OpenText("2007-HouseTppByDivisionDownload-13745.csv"),
                        General = true
                    },
                    //2008 Gippsland By-Election (28 June 2008)
                    new InputFile
                    {
                        Date = new DateTime(2008, 6, 28),
                        Source = File.OpenText("2008-Gippsland-HouseCandidatesDownload-13813.csv"),
                        TPPSource = File.OpenText("2008-Gippsland-HouseTppByPollingPlaceDownload-13813.csv"),
                        General = false
                    },
                    //2008 Lyne By-Election (6 September 2008)
                    new InputFile
                    {
                        Date = new DateTime(2008, 9, 6),
                        Source = File.OpenText("2008-Lyne-HouseCandidatesDownload-13827.csv"),
                        TPPSource = null,
                        General = false
                    },
                    //2008 Mayo By-Election (6 September 2008)
                    new InputFile
                    {
                        Date = new DateTime(2008, 9, 6),
                        Source = File.OpenText("2008-Mayo-HouseCandidatesDownload-13826.csv"),
                        TPPSource = null,
                        General = false
                    },
                    //2009 Bradfield By-Election (5 December 2009)
                    new InputFile
                    {
                        Date = new DateTime(2009, 12, 5),
                        Source = File.OpenText("2009-Bradfield-HouseCandidatesDownload-14357.csv"),
                        TPPSource = null,
                        General = false
                    },
                    //2009 Higgins By-Election (5 December 2009)
                    new InputFile
                    {
                        Date = new DateTime(2009, 12, 5),
                        Source = File.OpenText("2009-Higgins-HouseCandidatesDownload-14358.csv"),
                        TPPSource = null,
                        General = false
                    },
                    //2010 Federal Election (21 August 2010)
                    new InputFile
                    {
                        Date = new DateTime(2010, 8, 21),
                        Source = File.OpenText("2010-HouseMembersElectedDownload-15508.csv"),
                        TPPSource = File.OpenText("2010-HouseTppByDivisionDownload-15508.csv"),
                        General = true
                    },
                    //2013 Federal Election (7 September 2013)
                    new InputFile
                    {
                        Date = new DateTime(2013, 9, 7),
                        Source = File.OpenText("2013-HouseMembersElectedDownload-17496.csv"),
                        TPPSource = File.OpenText("2013-HouseTppByDivisionDownload-17496.csv"),
                        General = true
                    },
                    //2014 Griffith By-Election (8 February 2014)
                    new InputFile
                    {
                        Date = new DateTime(2014, 2, 8),
                        Source = File.OpenText("2014-Griffith-HouseCandidatesDownload-17552.csv"),
                        TPPSource = File.OpenText("2014-Griffith-HouseTppByPollingPlaceDownload-17552.csv"),
                        General = false
                    }
                };

                foreach (var resFile in resultFiles)
                {
                    try
                    {
                        var csvr = new CsvReader(resFile.Source);
                        if (resFile.General)
                        {
                            results.LoadElectionResults(resFile.Date, csvr.GetRecords<ElectionResult>());

                            if (resFile.TPPSource != null)
                            {
                                var tppr = new CsvReader(resFile.TPPSource);
                                tppr.Configuration.RegisterClassMap<FederalTPPResultClassMap>();
                                results.LoadTPPResults(resFile.Date, tppr.GetRecords<FederalTPPResult>());
                            }
                        }
                        else
                        {
                            results.LoadElectionResults(resFile.Date, csvr.GetRecords<ByElectionResult>().Where(e => e.Elected == "Y"));

                            if (resFile.TPPSource != null)
                            {
                                var tppr = new CsvReader(resFile.TPPSource);
                                tppr.Configuration.RegisterClassMap<ByElectionPollingBoothTPPResultClassMap>();
                                results.LoadTPPResults(resFile.Date, tppr.GetRecords<ByElectionPollingBoothTPPResult>());
                            }
                        }
                    }
                    finally
                    {
                        resFile.Close();
                    }
                }

                //_parties = results.GetParties();

                var layer = dataSource.GetLayerByIndex(0);
                using (var fw = new StreamWriter("elections.czml", false))
                {
                    using (var writer = new JsonTextWriter(fw))
                    {
                        writer.Formatting = Formatting.Indented;

                        //The root element of a CZML document is a JSON array
                        writer.WriteStartArray();
                        
                        //Which contains a series of CZML packets, uniquely identified

                        //Starts with the header
                        WriteCzmlDocumentHeader(writer);

                        var feat = layer.GetNextFeature();
                        while (feat != null)
                        {
                            string electorateName = feat.GetFieldAsString("ELECT_DIV");
                            var elecResults = results.GetResultsForDivison(electorateName);
                            var tppResults = results.GetTPPForDivison(electorateName);
                            if (elecResults.Length > 0)
                            {
                                WritePacket(writer, feat, electorateName, elecResults, tppResults);
                            }
                            feat = layer.GetNextFeature();
                        }

                        writer.WriteEndArray();
                    }
                }
            }
            finally
            {
                dataSource.Dispose();
            }

            //double smallestArea = _areas.Values.Min();
            //Debug.WriteLine($"Smallest area: {smallestArea}");
        }
    }
}
