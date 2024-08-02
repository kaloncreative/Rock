using System;
using System.Collections.Generic;

using Rock.Model;

namespace Team.Passion.OscMatching.Models.MatchingTool
{
    public class OnSiteCoordinatorResponseBag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public int? Age { get; set; }
        public string Location { get; set; }
        public List<string> DayPreference { get; set; }
        public List<string> TimePreference { get; set; }
        public Guid? CampusGuid { get; set; }
        public int MaxProjects { get; set; }
        public int CurrentProjects { get; set; }
        public decimal MatchPercentage { get; set; }
        public string ExtraInfo { get; set; }
        public bool CanBeAssigned { get; set; }

        public string GenderString => Gender.ToString();
        public string FormattedMatchPercentage => $"{MatchPercentage:F1} %";

        public OnSiteCoordinatorResponseBag Clone()
        {
            return (OnSiteCoordinatorResponseBag)MemberwiseClone();
        }
    }
}
