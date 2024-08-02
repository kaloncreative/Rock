using System;
using System.Collections.Generic;

using Rock.Model;

namespace Team.Passion.OscMatching.Models.MatchingTool
{
    public class ProjectResponseBag
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Partner { get; set; }
        public string Location { get; set; }
        public Gender Gender { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? CampusGuid { get; set; }
        public int? SuggestedOscId { get; set; }
        public string SuggestedOscName { get; set; }
        public int? SelectedOscId { get; set; }
        public string SelectedOscName { get; set; }
        public int? Capacity { get; set; }

        public string GenderString => Gender.ToString();
        public string FullDate => StartDate?.ToString("D");
        public string Day => StartDate?.DayOfWeek.ToString();
        public string Time => StartDate?.ToString("htt") + " - " + EndDate?.ToString("htt");

        private List<ProjectTimeOfDay> _timeOfDay;
        public List<string> TimeOfDay
        {
            get
            {
                if (_timeOfDay != null)
                {
                    return _timeOfDay.ConvertAll(x => x.ToString());
                }

                if (StartDate == null || EndDate == null)
                {
                    return null;
                }

                var timeOfDay = new List<ProjectTimeOfDay>();
                var startHour = StartDate.Value.Hour;
                var endHour = EndDate.Value.Hour;

                if (startHour >= 0 && startHour < 12)
                {
                    timeOfDay.Add(ProjectTimeOfDay.Morning);

                    if (endHour > 12)
                    {
                        timeOfDay.Add(ProjectTimeOfDay.Afternoon);
                    }
                    if (endHour > 16)
                    {
                        timeOfDay.Add(ProjectTimeOfDay.Evening);
                    }
                }
                else if (startHour >= 12 && startHour < 16)
                {
                    timeOfDay.Add(ProjectTimeOfDay.Afternoon);

                    if (endHour > 16)
                    {
                        timeOfDay.Add(ProjectTimeOfDay.Evening);
                    }
                }
                else
                {
                    timeOfDay.Add(ProjectTimeOfDay.Evening);
                }

                _timeOfDay = timeOfDay;

                return _timeOfDay.ConvertAll(x => x.ToString());
            }
        }
    }

    public enum ProjectTimeOfDay
    {
        Morning = 0,
        Afternoon = 1,
        Evening = 2
    }
}
