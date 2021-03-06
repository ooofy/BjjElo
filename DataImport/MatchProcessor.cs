﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BaseClasses;

namespace DataImport
{
    public class MatchInformation
    {
        public string Fighter1 { get; set; } = null;
        public string Fighter2 { get; set; } = null;
        public string Result { get; set; } = null;
        public string Year { get; set; } = null;
        public string Round { get; set; } = null;
        public string Competition { get; set; } = null;
        public string Method { get; set; } = null;
        public string WeightClass { get; set; } = null;
    }



    public static class MatchProcessor
    {
        private static int NextFighterId = 0;

        public static HashSet<Fighter> Fighters { get; } = new HashSet<Fighter>();
        public static HashSet<MatchWithoutId> Matches { get; } = new HashSet<MatchWithoutId>();
        public static List<MatchInformation> RawMatchData { get; } = new List<MatchInformation>(100);


        public static void LoadAllData()
        {
            var filepath = "..\\..\\..\\Data\\";
            var bjjHeroesFiles = Directory.GetFiles(filepath, "*.csv", SearchOption.TopDirectoryOnly);

            foreach (var file in bjjHeroesFiles)
            {
                var index = file.LastIndexOf('\\');
                var fighterName = file.Substring(index + 1);
                fighterName = fighterName.Replace(".csv", string.Empty);

                index = fighterName.IndexOf("_");
                var firstName = fighterName.Substring(0, index);
                var lastName = fighterName.Substring(index + 1);


                //var filename = filepath + Regex.Replace(file, @"\s+", "") + ".csv";
                ProcessMatchInformations(BjjHeroesBio.LoadFile(file, firstName + " " + lastName));
            }

            //ProcessMatchInformations(Worlds2016.LoadFile());
        }




        public static void ProcessMatchInformations(IList<MatchInformation> matchInformations)
        {
            RawMatchData.AddRange(matchInformations);

            foreach (var matchInfo in matchInformations)
            {
                var fighter1 = GetOrCreateFighter(matchInfo.Fighter1);
                var fighter2 = GetOrCreateFighter(matchInfo.Fighter2);

                if (fighter1 == null || fighter2 == null)
                    continue;

                // Win / loss by points?
                var resultByPoints = matchInfo.Method.ToLower().Contains("pts") || matchInfo.Method.ToLower().Contains("points");

                MatchResult result;
                switch (matchInfo.Result)
                {
                    case ("W"):
                        result = resultByPoints ? MatchResult.WinByPoints : MatchResult.WinBySubmission;
                        break;

                    case ("L"):
                        result = resultByPoints ? MatchResult.LossByPoints : MatchResult.LossBySubmission;
                        break;

                    case ("D"):
                        result = MatchResult.Draw;
                        break;
                    default:
                        throw new ArgumentException("Result could not be analyzed!");
                }

                int year;
                if (!Int32.TryParse(matchInfo.Year, out year))
                    year = 0;

                var match = new MatchWithoutId()
                {
                    Fighter1 = fighter1,
                    Fighter2 = fighter2,
                    Result = result,
                    Year = year
                };


                Matches.Add(match);
            }
        }



        public static Fighter GetOrCreateFighter(string fullName)
        {
            if (fullName.Equals("Unknown") || fullName.Equals("Uknown"))
                return null;

            var index = fullName.LastIndexOf(' ');

            if (index < 0)
                return null;

            var firstname = fullName.Substring(0, index);
            var lastname = fullName.Substring(index + 1);

            var fighter
                = Fighters
                .FirstOrDefault(f => f.LastName.Equals(lastname) && (f.FirstName.Contains(firstname) || firstname.Contains(f.FirstName)));

            if (fighter == null)
            {
                fighter = new Fighter(NextFighterId++, firstname, lastname, null);
                Fighters.Add(fighter);
            }

            return fighter;
        }
    }
}
