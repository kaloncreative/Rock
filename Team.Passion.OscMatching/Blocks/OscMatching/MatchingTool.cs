using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Web.Cache;
using Team.Passion.OscMatching.Helpers;
using Team.Passion.OscMatching.Models.MatchingTool;

namespace Rock.Blocks.OscMatching
{
    /// <summary>
    /// OSC Matching Tool.
    /// </summary>
    /// <seealso cref="RockPluginBlockType" />

    [DisplayName("OSC Matching Tool")]
    [Category("OSC Matching")]
    [Description("OSC Matching Tool.")]
    [IconCssClass("fa fa-code-compare")]
    [SupportedSiteTypes(SiteType.Web)]

    #region Block Attributes

    [GroupField(
        "Project Group",
        Description = "Choose a project group from the list to specify which projects will be used for matching",
        IsRequired = true,
        Category = "Project Group Configuration",
        Order = 0,
        Key = AttributeKey.ProjectGroup)]

    [GroupRoleField(
        null,
        "Assigned OSC Group Role",
        Description = "The group role for the assigned OSCs.",
        IsRequired = true,
        Category = "Project Group Configuration",
        Order = 1,
        Key = AttributeKey.AssignedOscGroupRole)]

    [AttributeField(
        SystemGuid.EntityType.GROUP,
        "Project Group Start Date Field",
        Description = "Map the attribute that stores the project group start date.",
        IsRequired = true,
        Category = "Project Group Configuration",
        Order = 2,
        Key = AttributeKey.ProjectGroupStartDateAttribute)]

    [AttributeField(
        SystemGuid.EntityType.GROUP,
        "Project Group End Date Field",
        Description = "Map the attribute that stores the project group end date.",
        IsRequired = true,
        Category = "Project Group Configuration",
        Order = 3,
        Key = AttributeKey.ProjectGroupEndDateAttribute)]

    [AttributeField(
        SystemGuid.EntityType.GROUP,
        "Project Group Gender Field",
        Description = "Map the attribute that stores the project group gender preference.",
        IsRequired = true,
        Category = "Project Group Configuration",
        Order = 4,
        Key = AttributeKey.ProjectGroupGenderAttribute)]

    [AttributeField(
        SystemGuid.EntityType.GROUP,
        "Project Group Partner Field",
        Description = "Map the attribute that stores the project group partner field.",
        IsRequired = true,
        Category = "Project Group Configuration",
        Order = 5,
        Key = AttributeKey.ProjectGroupPartnerAttribute)]

    [GroupField(
        "OSC Project Group",
        Description = "Choose a project group from the list to pull potential OSCs ",
        IsRequired = true,
        Category = "OSC Configuration",
        Order = 0,
        Key = AttributeKey.OscProjectGroup)]

    [GroupRoleField(
        null,
        "OSC Group Role",
        Description = "The group role for the OSCs.",
        IsRequired = true,
        Category = "OSC Configuration",
        Order = 1,
        Key = AttributeKey.PotentialOscGroupRole)]

    [AttributeField(
        SystemGuid.EntityType.PERSON,
        "OSC Campus Field",
        Description = "Map the attribute that stores the OSC's campus preference.",
        IsRequired = true,
        Category = "OSC Configuration",
        Order = 2,
        Key = AttributeKey.OscCamputAttribute)]

    [AttributeField(
        SystemGuid.EntityType.PERSON,
        "OSC Preferred Days Field",
        Description = "Map the attribute that stores the OSC's days preference.",
        IsRequired = true,
        Category = "OSC Configuration",
        Order = 3,
        Key = AttributeKey.OscPreferredDaysAttribute)]

    [AttributeField(
        SystemGuid.EntityType.PERSON,
        "OSC Preferred Times Field",
        Description = "Map the attribute that stores the OSC's times preference.",
        IsRequired = true,
        Category = "OSC Configuration",
        Order = 4,
        Key = AttributeKey.OscPreferredTimesAttribute)]

    [AttributeField(
        SystemGuid.EntityType.PERSON,
        "OSC Extra Info Field",
        Description = "Map the attribute that stores the OSC's extra information.",
        IsRequired = true,
        Category = "OSC Configuration",
        Order = 5,
        Key = AttributeKey.OscExtraInfoAttribute)]

    [AttributeField(
        SystemGuid.EntityType.PERSON,
        "OSC Max Projects Field",
        Description = "Map the attribute that stores the OSC's maximum projects.",
        IsRequired = true,
        Category = "OSC Configuration",
        Order = 6,
        Key = AttributeKey.OscMaxProjectsAttribute)]

    [WorkflowTypeField(
        "Workflow Type",
        Description = "The workflow type used for the matching tool.",
        IsRequired = false,
        Category = "OSC Configuration",
        Order = 7,
        Key = AttributeKey.WorkflowType)]

    #endregion

    public class MatchingTool : RockPluginBlockType
    {
        #region Constants

        private static class AttributeKey
        {
            public const string ProjectGroup = "ProjectGroup";
            public const string WorkflowType = "WorkflowType";
            public const string OscProjectGroup = "OscProjectGroup";
            public const string ProjectGroupStartDateAttribute = "ProjectGroupStartDate";
            public const string ProjectGroupEndDateAttribute = "ProjectGroupEndDate";
            public const string ProjectGroupGenderAttribute = "ProjectGroupGender";
            public const string ProjectGroupPartnerAttribute = "ProjectGroupPartner";
            public const string AssignedOscGroupRole = "AssignedOscGroupRole";
            public const string PotentialOscGroupRole = "PotentialOscGroupRole";
            public const string OscCamputAttribute = "OscCampus";
            public const string OscPreferredDaysAttribute = "OscPreferredDays";
            public const string OscPreferredTimesAttribute = "OscPreferredTimes";
            public const string OscExtraInfoAttribute = "OscExtraInfo";
            public const string OscMaxProjectsAttribute = "OscMaxProjects";
        }

        private static class Constants
        {
            public static string WorkflowPersonIdKey = "PersonId";
            public static string WorkflowProjectGroupIdKey = "ProjectGroupId";
            public static string GroupStatusValue = "Draft";
            public static string[] GroupGenderFemaleValues = { "female" };
            public static string[] GroupGenderMenValues = { "men", "male" };
            public static MatchingScoreModel DetractScores = new MatchingScoreModel
            {
                Gender = -1000,
                Campus = -10,
                Day = -20,
                Time = -5,
                DayTime = 0,
                Selection = -5
            };
            public static MatchingScoreModel AffirmScores = new MatchingScoreModel
            {
                Gender = 100,
                Campus = 20,
                Day = 50,
                Time = 10,
                DayTime = 15,
                Selection = 0
            };
        }

        #endregion

        #region Private Fields

        private Guid? _projectGroupGuid;
        private Guid? _workflowTypeGuid;
        private Guid? _oscProjectGroupGuid;
        private Guid? _projectGroupStartDate;
        private Guid? _projectGroupEndDate;
        private Guid? _projectGroupGender;
        private Guid? _projectGroupPartner;
        private Guid? _assignedOscGroupRole;
        private Guid? _potentialOscGroupRole;
        private Guid? _oscCampusAttribute;
        private Guid? _oscPreferredDaysAttribute;
        private Guid? _oscPreferredTimesAttribute;
        private Guid? _oscExtraInfoAttribute;
        private Guid? _oscMaxProjectsAttribute;
        private Dictionary<int, int> _projectMaxScoreCache;
        private BlockService _blockService;
        private GroupService _groupService;
        private GroupMemberService _groupMemberService;
        private AttributeValueService _attributeValueService;
        private WorkflowService _workflowService;
        private CampusService _campusService;
        private PersonService _personService;
        private GroupTypeRoleService _groupTypeRoleService;
        private BlockHelper _blockHelper;

        #endregion

        #region Methods

        /// <inheritdoc />
        public override object GetObsidianBlockInitialization()
        {
            InitServicesndAttributes();

            return new
            {
                IsInitialConfigSet = AreBlockAttributesSet(),
                ProjectsGridDefinition = GetProjectsGridBuilder().BuildDefinition(),
                OnSiteCoordinatorsGridDefinition = GetOnSiteCoordinatorsGridBuilder().BuildDefinition(),
            };
        }

        #endregion

        #region Actions

        /// <summary>
        /// Gets the projects row data.
        /// </summary>
        /// <param name="assigned">if set to true, returns only assigned projects.</param>
        /// <returns>Grid data bag with projects data</returns>
        [BlockAction]
        public async Task<BlockActionResult> GetProjectsRowData(bool? assigned = null)
        {
            InitServicesndAttributes();

            var projects = await GetProjects(assigned);

            var builder = GetProjectsGridBuilder();
            var gridDataBag = builder.Build(projects);

            return ActionOk(gridDataBag);
        }

        /// <summary>
        /// Gets the potential on-site coordinators row data.
        /// </summary>
        /// <param name="assigned">if set to true, returns only assigned projects.</param>
        /// <returns>Grid data bag with the potential on-site coordinators data</returns>
        [BlockAction]
        public async Task<BlockActionResult> GetOnSiteCoordinatorsRowData(int projectId)
        {
            InitServicesndAttributes();

            var project = await GetProject(projectId);
            var oscs = GetIncludedOnSiteCoordiantors(await GetPotentialOnSiteCoordinators());

            foreach (var osc in oscs)
            {
                CalculateMatchPercentage(project, osc);
            }

            var builder = GetOnSiteCoordinatorsGridBuilder();
            var gridDataBag = builder.Build(oscs.OrderByDescending(x => x.MatchPercentage));

            return ActionOk(gridDataBag);
        }

        /// <summary>
        /// Gets the oscs with missing key attributes that are excluded from matching
        /// </summary>
        /// <returns>A BlockActionResult instance</returns>
        [BlockAction]
        public async Task<BlockActionResult> GetExcludedOnSiteCoordinators()
        {
            InitServicesndAttributes();

            var oscs = GetExcludedOnSiteCoordiantors(await GetPotentialOnSiteCoordinators(fetchAssignedProject: false));

            return ActionOk(oscs);
        }

        /// <summary>
        /// Optimizes for the highest aggregate score across all projects and caches the result
        /// </summary>
        /// <returns>A BlockActionResult instance</returns>
        [BlockAction]
        public async Task<BlockActionResult> RunOptimizations()
        {
            InitServicesndAttributes();

            Dictionary<int, OnSiteCoordinatorResponseBag> topOscPerProject = new Dictionary<int, OnSiteCoordinatorResponseBag>();
            OnSiteCoordinatorResponseBag topOsc;
            var projects = await GetProjects(false);
            var oscs = GetIncludedOnSiteCoordiantors(await GetPotentialOnSiteCoordinators());
            var assignedOscProjects = await GetAssignedOnSiteCoordinators(_projectGroupGuid, oscs.Select(x => x.Id).ToList());
            var assignedOscProjectIds = assignedOscProjects.Select(x => x.ProjectId).Distinct().ToList();
            var assignedOscProjectAttributes = await GetProjectAttributeValues(assignedOscProjectIds);

            foreach (var project in projects)
            {
                topOsc = null;

                foreach (var osc in oscs)
                {
                    if (!osc.CanBeAssigned)
                    {
                        continue;
                    }

                    var overlapExists = false;
                    var oscProjectIds = assignedOscProjects.Where(x => x.PersonId == osc.Id).Select(x => x.ProjectId).Distinct().ToList();
                    foreach (var oscProjectId in oscProjectIds)
                    {
                        var oscProjectAttribute = assignedOscProjectAttributes.GetValueOrNull(oscProjectId);
                        if (oscProjectAttribute != null && ProjectsOverlap(project.StartDate, project.EndDate, oscProjectAttribute.StartDate.AsDateTime(), oscProjectAttribute.EndDate.AsDateTime()))
                        {
                            overlapExists = true;
                            break;
                        }
                    }

                    if (overlapExists)
                    {
                        continue;
                    }

                    CalculateMatchPercentage(project, osc);

                    if (osc.MatchPercentage > 0 && (topOsc == null || osc.MatchPercentage > topOsc.MatchPercentage))
                    {
                        topOsc = osc;
                    }
                }

                if (topOsc != null)
                {
                    project.SuggestedOscId = topOsc.Id;
                    project.SuggestedOscName = $"{topOsc.Name} ({topOsc.FormattedMatchPercentage})";
                    topOscPerProject.Add(project.Id, topOsc.Clone());
                }
                else
                {
                    project.SuggestedOscId = null;
                    project.SuggestedOscName = null;
                }
            }

            _blockHelper.SetBlockAdditionalSettings(BlockId, topOscPerProject);

            var builder = GetProjectsGridBuilder();
            var gridDataBag = builder.Build(projects);

            return ActionOk(gridDataBag);
        }

        /// <summary>
        /// Assigns the on-site coordinator to the project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="oscId"></param>
        /// <returns>A BlockActionResult instance</returns>
        [BlockAction]
        public async Task<BlockActionResult> AssignOnSiteCoordinator(int projectId, int oscId)
        {
            InitServicesndAttributes();

            var project = _groupService.Get(projectId);
            var person = _personService.Get(oscId);

            if (project == null)
            {
                return ActionBadRequest("Invalid project");
            }
            if (person == null)
            {
                return ActionBadRequest("Invalid person");
            }

            await AddGroupMember(project, person);

            if (_workflowTypeGuid != null)
            {
                var errorMessages = new List<string>();
                var workflowType = WorkflowTypeCache.Get(_workflowTypeGuid.Value);
                var workflow = Rock.Model.Workflow.Activate(workflowType, null);

                if (workflow == null)
                {
                    return ActionBadRequest("No workflow set for workflow type");
                }

                workflow.SetAttributeValue("ProjectGroupId", projectId.ToString());
                workflow.SetAttributeValue("PersonId", oscId.ToString());

                _workflowService.Process(workflow, out errorMessages);

                if (errorMessages.Any())
                {
                    return ActionBadRequest(errorMessages.JoinStrings(", "));
                }
            }

            return ActionOk();
        }

        /// <summary>
        /// Validates and returns a list of warnings related to assigning the on-site coordinator to the project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="oscId"></param>
        /// <returns>A BlockActionResult instance</returns>
        [BlockAction]
        public async Task<BlockActionResult> ValidateAssignOnSiteCoordinator(int projectId, int oscId)
        {
            InitServicesndAttributes();

            var result = await ValidateAddGroupMember(projectId, oscId);

            return ActionOk(result);
        }

        /// <summary>
        /// Gets the selected on-site coordinator for the project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="personId"></param>
        /// <returns>A BlockActionResult instance</returns>
        [BlockAction]
        public async Task<BlockActionResult> GetSelectedOnSiteCoordinator(int projectId, int personId)
        {
            InitServicesndAttributes();

            var project = await GetProject(projectId);

            if (project == null)
            {
                return ActionBadRequest("Invalid project");
            }

            var osc = await GetOnSiteCoordinator(personId);

            if (osc == null)
            {
                return ActionBadRequest("Invalid on-site coordinator");
            }

            CalculateMatchPercentage(project, osc, true);

            return ActionOk(osc);
        }

        /// <summary>
        /// Gets the on-site coordinator assigned projects
        /// </summary>
        /// <param name="personId"></param>
        /// <returns>A BlockActionResult instance</returns>
        [BlockAction]
        public async Task<BlockActionResult> GetOnSiteCoordinatorProjects(int personId)
        {
            InitServicesndAttributes();

            var oscProjects = await GetOnSiteCoordinatorAssignedProjects(personId);

            return ActionOk(oscProjects);
        }

        #endregion

        #region Helpers

        public GridBuilder<ProjectResponseBag> GetProjectsGridBuilder()
        {
            return new GridBuilder<ProjectResponseBag>()
                .AddField("id", c => c.Id)
                .AddTextField("name", c => c.Name)
                .AddTextField("partner", c => c.Partner)
                .AddTextField("location", c => c.Location)
                .AddField("gender", c => c.Gender)
                .AddTextField("genderString", c => c.GenderString)
                .AddField("suggestedOscId", c => c.SuggestedOscId)
                .AddTextField("suggestedOscName", c => c.SuggestedOscName)
                .AddField("selectedOscId", c => c.SelectedOscId)
                .AddTextField("selectedOscName", c => c.SelectedOscName)
                .AddTextField("fullDate", c => c.FullDate)
                .AddTextField("day", c => c.Day)
                .AddTextField("time", c => c.Time)
                .AddField("timeOfDay", c => c.TimeOfDay);
        }

        public GridBuilder<OnSiteCoordinatorResponseBag> GetOnSiteCoordinatorsGridBuilder()
        {
            return new GridBuilder<OnSiteCoordinatorResponseBag>()
                .AddField("id", c => c.Id)
                .AddTextField("name", c => c.Name)
                .AddField("gender", c => c.Gender)
                .AddField("genderString", c => c.GenderString)
                .AddTextField("location", c => c.Location)
                .AddField("dayPreference", c => c.DayPreference)
                .AddField("timePreference", c => c.TimePreference)
                .AddField("currentProjects", c => c.CurrentProjects)
                .AddField("maxProjects", c => c.MaxProjects)
                .AddField("matchPercentage", c => c.MatchPercentage)
                .AddTextField("formattedMatchPercentage", c => c.FormattedMatchPercentage)
                .AddTextField("extraInfo", c => c.ExtraInfo);
        }

        private async Task<ProjectResponseBag> GetProject(int projectId)
        {
            return (await GetProjects(new List<int> { projectId })).FirstOrDefault();
        }

        private async Task<List<ProjectResponseBag>> GetProjects(List<int> projectIds)
        {
            if (projectIds == null || !projectIds.Any())
            {
                return new List<ProjectResponseBag>();
            }

            var projects = await _groupService
                .Queryable()
                .Where(x => projectIds.Contains(x.Id))
                .Select(x => new ProjectResponseBag
                {
                    Id = x.Id,
                    Name = x.Name,
                    Location = x.Campus != null ? x.Campus.ShortCode : "Not Provided",
                    CampusGuid = x.Campus.Guid,
                    Capacity = x.GroupCapacity
                })
                .ToListAsync();

            if (!projects.Any())
            {
                return projects;
            }

            var projectAttributesCache = await GetProjectAttributeValues(projectIds);

            foreach (var project in projects)
            {
                var projectAttributes = projectAttributesCache.GetValueOrNull(project.Id);

                project.Partner = projectAttributes?.Partner;
                project.Gender = GetGender(projectAttributes?.Gender);
                project.StartDate = projectAttributes?.StartDate.AsDateTime();
                project.EndDate = projectAttributes?.EndDate.AsDateTime();
            }

            return projects;
        }

        private async Task<List<ProjectResponseBag>> GetProjects(bool? assigned = null)
        {
            var result = new List<ProjectResponseBag>();

            result = await GetProjects(_projectGroupGuid);

            var projectIds = result.Select(x => x.Id).ToList();

            var projectAttributesCache = await GetProjectAttributeValues(projectIds);

            var selectedOscsCache = await _groupMemberService
                .Queryable()
                .Where(x => projectIds.Contains(x.GroupId))
                .Where(x => x.GroupRole.Guid == _assignedOscGroupRole)
                .Where(x => x.GroupMemberStatus == GroupMemberStatus.Active && !x.IsArchived)
                .Select(x => new
                {
                    ProjectId = x.GroupId,
                    x.PersonId,
                    x.Person.FirstName,
                    x.Person.LastName
                }).ToListAsync();

            var topOscPerProject = _blockHelper.GetBlockAdditionalSettings<Dictionary<int, OnSiteCoordinatorResponseBag>>(BlockId);

            foreach (var project in result)
            {
                var projectAttributes = projectAttributesCache.GetValueOrNull(project.Id);
                var selectedOsc = selectedOscsCache.FirstOrDefault(x => x.ProjectId == project.Id);
                var suggestedOsc = topOscPerProject?.GetValueOrNull(project.Id);

                project.Partner = projectAttributes?.Partner;
                project.Gender = GetGender(projectAttributes?.Gender);
                project.StartDate = projectAttributes?.StartDate.AsDateTime();
                project.EndDate = projectAttributes?.EndDate.AsDateTime();
                if (selectedOsc != null)
                {
                    project.SelectedOscId = selectedOsc.PersonId;
                    project.SelectedOscName = $"{selectedOsc.FirstName} {selectedOsc.LastName}";
                }
                if (suggestedOsc != null)
                {
                    project.SuggestedOscId = suggestedOsc.Id;
                    project.SuggestedOscName = $"{suggestedOsc.Name} ({suggestedOsc.FormattedMatchPercentage})";
                }
            }

            if (assigned == true)
            {
                result = result
                    .Where(x => x.SelectedOscId != null)
                    .ToList();
            }
            else if (assigned == false)
            {
                result = result
                    .Where(x => x.SelectedOscId == null)
                    .ToList();
            }

            return result;
        }

        private async Task<List<ProjectResponseBag>> GetProjects(Guid? parentGuid)
        {
            if (parentGuid == null)
            {
                return new List<ProjectResponseBag>();
            }

            var projects = await _groupService
                .Queryable()
                .Where(x => x.IsActive && !x.IsArchived)
                .Where(x => x.ParentGroup != null && x.ParentGroup.Guid == parentGuid)
                .Where(x => x.StatusValue == null || x.StatusValue.Value != Constants.GroupStatusValue)
                .Select(x => new ProjectResponseBag
                {
                    Id = x.Id,
                    Guid = x.Guid,
                    Name = x.Name,
                    Location = x.Campus != null ? x.Campus.ShortCode : "Not Provided",
                    CampusGuid = x.Campus.Guid,
                    Capacity = x.GroupCapacity
                })
                .ToListAsync();

            if (!projects.Any())
            {
                return projects;
            }

            var guids = projects.Select(x => x.Guid).ToList();
            foreach (var guid in guids)
            {
                projects.AddRange(await GetProjects(guid));
            }

            return projects;
        }

        private async Task<Dictionary<int, ProjectAttributesModel>> GetProjectAttributeValues(List<int> projectIds)
        {
            var attributeGuids = new List<Guid?>
            {
                _projectGroupPartner,
                _projectGroupGender,
                _projectGroupStartDate,
                _projectGroupEndDate
            };

            return await _attributeValueService
                .Queryable()
                .Where(x => attributeGuids.Contains(x.Attribute.Guid))
                .Where(x => x.EntityId != null && projectIds.Contains(x.EntityId.Value))
                .GroupBy(x => x.EntityId)
                .ToDictionaryAsync(x => x.Key.Value, x => new ProjectAttributesModel
                {
                    Partner = x.FirstOrDefault(a => a.Attribute != null && a.Attribute.Guid == _projectGroupPartner)?.PersistedCondensedTextValue,
                    Gender = x.FirstOrDefault(a => a.Attribute != null && a.Attribute.Guid == _projectGroupGender)?.Value,
                    StartDate = x.FirstOrDefault(a => a.Attribute != null && a.Attribute.Guid == _projectGroupStartDate)?.Value,
                    EndDate = x.FirstOrDefault(a => a.Attribute != null && a.Attribute.Guid == _projectGroupEndDate)?.Value
                });
        }

        private async Task<OnSiteCoordinatorResponseBag> GetOnSiteCoordinator(int personId)
        {
            return (await GetPotentialOnSiteCoordinators(new List<int> { personId })).FirstOrDefault();
        }

        private async Task<List<OnSiteCoordinatorResponseBag>> GetPotentialOnSiteCoordinators(List<int> personIds = null, bool fetchAssignedProject = true)
        {
            var potentialOscsQuery = _groupMemberService.Queryable();

            if (personIds != null)
            {
                potentialOscsQuery = potentialOscsQuery.Where(x => personIds.Contains(x.PersonId));
            }
            else
            {
                potentialOscsQuery = potentialOscsQuery
                    .Where(x => x.Group.Guid == _oscProjectGroupGuid)
                    .Where(x => x.GroupRole.Guid == _potentialOscGroupRole)
                    .Where(x => x.GroupMemberStatus == GroupMemberStatus.Active && !x.IsArchived);
            }

            var potentialOscs = await potentialOscsQuery
                .Select(x => new OnSiteCoordinatorResponseBag
                {
                    Id = x.PersonId,
                    Name = x.Person.FirstName + " " + x.Person.LastName,
                    Gender = x.Person.Gender,
                    Age = x.Person.Age,
                }).ToListAsync();

            var potentialOscIds = potentialOscs.Select(x => x.Id).ToList();

            var potentialOscAttributesCache = await GetOnSiteCoordinatorAttributeValues(potentialOscIds);

            var campusGuids = potentialOscAttributesCache.Values
                .Select(x => x.CampusGuid.AsGuidOrNull())
                .Where(x => x != null)
                .Distinct()
                .ToList();
            var campusCache = await _campusService
                .Queryable()
                .Where(x => campusGuids.Contains(x.Guid))
                .ToDictionaryAsync(x => x.Guid, x => x.ShortCode);

            var assignedOscsCache = new Dictionary<int, int>();
            if (fetchAssignedProject)
            {
                var assignedOscs = await GetAssignedOnSiteCoordinators(_projectGroupGuid, personIds);
                assignedOscsCache = assignedOscs
                    .GroupBy(x => x.PersonId)
                    .ToDictionary(x => x.Key, x => x.Select(y => y.ProjectGuid).Distinct().Count());
            }

            foreach (var potentialOsc in potentialOscs)
            {
                var potentialOscAttributes = potentialOscAttributesCache.GetValueOrNull(potentialOsc.Id);

                potentialOsc.CampusGuid = potentialOscAttributes?.CampusGuid.AsGuidOrNull();
                if (potentialOsc.CampusGuid != null && campusCache.ContainsKey(potentialOsc.CampusGuid.Value))
                {
                    potentialOsc.Location = campusCache.GetValueOrNull(potentialOsc.CampusGuid.Value);
                }
                if (!string.IsNullOrWhiteSpace(potentialOscAttributes?.DayPreference))
                {
                    potentialOsc.DayPreference = potentialOscAttributes.DayPreference.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                }
                if (!string.IsNullOrWhiteSpace(potentialOscAttributes?.TimePreference))
                {
                    potentialOsc.TimePreference = potentialOscAttributes.TimePreference.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                }
                potentialOsc.MaxProjects = potentialOscAttributes?.MaxProjects?.ToIntSafe(-1) ?? -1;
                potentialOsc.ExtraInfo = potentialOscAttributes?.ExtraInfo;
                potentialOsc.CurrentProjects = assignedOscsCache.GetValueOrDefault(potentialOsc.Id, 0);
                potentialOsc.CanBeAssigned = potentialOsc.MaxProjects < 0 || potentialOsc.CurrentProjects < potentialOsc.MaxProjects;
            }

            return potentialOscs;
        }

        private async Task<List<OnSiteCoordinatorProjectModel>> GetAssignedOnSiteCoordinators(Guid? parentGuid, List<int> personIds = null)
        {
            if (parentGuid == null)
            {
                return new List<OnSiteCoordinatorProjectModel>();
            }

            var assignedOscsQuery = _groupMemberService
                .Queryable()
                .Where(x => x.Group.ParentGroup != null && x.Group.ParentGroup.Guid == parentGuid)
                .Where(x => x.GroupRole.Guid == _assignedOscGroupRole)
                .Where(x => x.GroupMemberStatus == GroupMemberStatus.Active && !x.IsArchived);

            if (personIds != null)
            {
                assignedOscsQuery = assignedOscsQuery.Where(x => personIds.Contains(x.PersonId));
            }

            var assignedOscs = await assignedOscsQuery
                .Select(x => new OnSiteCoordinatorProjectModel
                {
                    PersonId = x.PersonId,
                    ProjectId = x.GroupId,
                    ProjectGuid = x.Group.Guid
                })
                .ToListAsync();

            if (!assignedOscs.Any())
            {
                return assignedOscs;
            }

            var guids = assignedOscs.Select(x => x.ProjectGuid).Distinct().ToList();
            foreach (var guid in guids)
            {
                assignedOscs.AddRange(await GetAssignedOnSiteCoordinators(guid, personIds));
            }

            return assignedOscs;
        }

        private async Task<List<ProjectResponseBag>> GetOnSiteCoordinatorAssignedProjects(int personId)
        {
            var assignedOscProjects = await GetAssignedOnSiteCoordinators(_projectGroupGuid, new List<int> { personId });

            return await GetProjects(assignedOscProjects.Select(x => x.ProjectId).ToList());
        }

        private async Task<Dictionary<int, OnSiteCoordinatorAttributesModel>> GetOnSiteCoordinatorAttributeValues(List<int> oscIds)
        {
            var attributeGuids = new List<Guid?>
            {
                _oscCampusAttribute,
                _oscPreferredDaysAttribute,
                _oscPreferredTimesAttribute,
                _oscExtraInfoAttribute,
                _oscMaxProjectsAttribute
            };
            return await _attributeValueService
                .Queryable()
                .Where(x => attributeGuids.Contains(x.Attribute.Guid))
                .Where(x => x.EntityId != null && oscIds.Contains(x.EntityId.Value))
                .GroupBy(x => x.EntityId)
                .ToDictionaryAsync(x => x.Key.Value, x => new OnSiteCoordinatorAttributesModel
                {
                    CampusGuid = x.FirstOrDefault(a => a.Attribute != null && a.Attribute.Guid == _oscCampusAttribute)?.Value,
                    DayPreference = x.FirstOrDefault(a => a.Attribute != null && a.Attribute.Guid == _oscPreferredDaysAttribute)?.PersistedTextValue,
                    TimePreference = x.FirstOrDefault(a => a.Attribute != null && a.Attribute.Guid == _oscPreferredTimesAttribute)?.PersistedTextValue,
                    ExtraInfo = x.FirstOrDefault(a => a.Attribute != null && a.Attribute.Guid == _oscExtraInfoAttribute)?.Value,
                    MaxProjects = x.FirstOrDefault(a => a.Attribute != null && a.Attribute.Guid == _oscMaxProjectsAttribute)?.Value
                });
        }

        private List<OnSiteCoordinatorResponseBag> GetExcludedOnSiteCoordiantors(List<OnSiteCoordinatorResponseBag> oscs)
        {
            return oscs
                .Where(x => x.Gender == Gender.Unknown
                            || x.DayPreference == null
                            || !x.DayPreference.Any()
                            || x.TimePreference == null
                            || !x.TimePreference.Any())
                .ToList();
        }

        private List<OnSiteCoordinatorResponseBag> GetIncludedOnSiteCoordiantors(List<OnSiteCoordinatorResponseBag> oscs)
        {
            return oscs
                .Where(x => x.Gender != Gender.Unknown
                            && x.DayPreference != null
                            && x.DayPreference.Any()
                            && x.TimePreference != null
                            && x.TimePreference.Any())
                .ToList();
        }

        private async Task AddGroupMember(Group project, Person person)
        {
            var groupRoleId = await _groupTypeRoleService
                .Queryable()
                .Where(x => x.Guid == _assignedOscGroupRole)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            var groupMember = new GroupMember
            {
                GroupId = project.Id,
                PersonId = person.Id,
                GroupTypeId = project.GroupTypeId,
                GroupRoleId = groupRoleId,
                GroupMemberStatus = GroupMemberStatus.Active,
                CreatedByPersonAliasId = person.PrimaryAliasId,
                ModifiedByPersonAliasId = person.PrimaryAliasId
            };

            _groupMemberService.Add(groupMember);

            RockContext.SaveChanges();
        }

        private async Task<List<string>> ValidateAddGroupMember(int projectId, int personId)
        {
            var result = new List<string>();
            var assignedOscProjectIds = (await GetAssignedOnSiteCoordinators(_projectGroupGuid, new List<int> { personId }))
                .Select(x => x.ProjectId)
                .Distinct()
                .ToList();

            if (assignedOscProjectIds.Contains(projectId))
            {
                result.Add("Person is already assigned to this project");
            }

            var projectAttributes = await GetProjectAttributeValues(assignedOscProjectIds.Concat(new List<int> { projectId }).ToList());
            var oscAttributes = await GetOnSiteCoordinatorAttributeValues(new List<int> { personId });
            var maxProjects = oscAttributes.GetValueOrNull(personId)?.MaxProjects?.ToIntSafe(-1) ?? -1;

            if (maxProjects >= 0 && assignedOscProjectIds.Count >= maxProjects)
            {
                result.Add("Person has reached the maximum number of projects");
            }

            var primaryProject = projectAttributes.GetValueOrNull(projectId);
            if (primaryProject != null)
            {
                foreach (var assignedOscProjectId in assignedOscProjectIds)
                {
                    var assignedProject = projectAttributes.GetValueOrNull(assignedOscProjectId);
                    if (assignedProject != null && ProjectsOverlap(primaryProject.StartDate.AsDateTime(), primaryProject.EndDate.AsDateTime(), assignedProject.StartDate.AsDateTime(), assignedProject.EndDate.AsDateTime()))
                    {
                        result.Add($"Person is already assigned to a project (Id {assignedOscProjectId}) that overlaps time with this project");
                    }
                }
            }

            return result;
        }

        private bool ProjectsOverlap(DateTime? start1, DateTime? end1, DateTime? start2, DateTime? end2)
        {
            return start1.GetValueOrDefault(DateTime.MinValue) < end2.GetValueOrDefault(DateTime.MaxValue) && end1.GetValueOrDefault(DateTime.MaxValue) > start2.GetValueOrDefault(DateTime.MinValue);
        }

        private void InitServices()
        {
            _projectMaxScoreCache = new Dictionary<int, int>();
            _groupService = new GroupService(RockContext);
            _groupMemberService = new GroupMemberService(RockContext);
            _attributeValueService = new AttributeValueService(RockContext);
            _workflowService = new WorkflowService(RockContext);
            _campusService = new CampusService(RockContext);
            _personService = new PersonService(RockContext);
            _groupTypeRoleService = new GroupTypeRoleService(RockContext);
            _blockHelper = new BlockHelper(RockContext);
        }

        private void InitServicesndAttributes()
        {
            InitServices();

            if (BlockCache != null)
            {
                _projectGroupGuid = GetAttributeValue(AttributeKey.ProjectGroup).AsGuidOrNull();
                _workflowTypeGuid = GetAttributeValue(AttributeKey.WorkflowType).AsGuidOrNull();
                _oscProjectGroupGuid = GetAttributeValue(AttributeKey.OscProjectGroup).AsGuidOrNull();
                _projectGroupStartDate = GetAttributeValue(AttributeKey.ProjectGroupStartDateAttribute).AsGuidOrNull();
                _projectGroupEndDate = GetAttributeValue(AttributeKey.ProjectGroupEndDateAttribute).AsGuidOrNull();
                _projectGroupGender = GetAttributeValue(AttributeKey.ProjectGroupGenderAttribute).AsGuidOrNull();
                _projectGroupPartner = GetAttributeValue(AttributeKey.ProjectGroupPartnerAttribute).AsGuidOrNull();
                _assignedOscGroupRole = GetAttributeValue(AttributeKey.AssignedOscGroupRole).AsGuidOrNull();
                _potentialOscGroupRole = GetAttributeValue(AttributeKey.PotentialOscGroupRole).AsGuidOrNull();
                _oscCampusAttribute = GetAttributeValue(AttributeKey.OscCamputAttribute).AsGuidOrNull();
                _oscPreferredDaysAttribute = GetAttributeValue(AttributeKey.OscPreferredDaysAttribute).AsGuidOrNull();
                _oscPreferredTimesAttribute = GetAttributeValue(AttributeKey.OscPreferredTimesAttribute).AsGuidOrNull();
                _oscExtraInfoAttribute = GetAttributeValue(AttributeKey.OscExtraInfoAttribute).AsGuidOrNull();
                _oscMaxProjectsAttribute = GetAttributeValue(AttributeKey.OscMaxProjectsAttribute).AsGuidOrNull();
            }
        }

        private bool AreBlockAttributesSet()
        {
            return _projectGroupGuid != null
                && _oscProjectGroupGuid != null
                && _projectGroupStartDate != null
                && _projectGroupEndDate != null
                && _projectGroupGender != null
                && _projectGroupPartner != null
                && _assignedOscGroupRole != null
                && _potentialOscGroupRole != null
                && _oscCampusAttribute != null
                && _oscPreferredDaysAttribute != null
                && _oscPreferredTimesAttribute != null
                && _oscExtraInfoAttribute != null
                && _oscMaxProjectsAttribute != null;
        }

        private void CalculateMatchPercentage(ProjectResponseBag project, OnSiteCoordinatorResponseBag osc, bool isSelected = false)
        {
            var matchScore = 0;

            if (project.Gender != Gender.Unknown)
            {
                if (project.Gender == osc.Gender)
                {
                    matchScore += Constants.AffirmScores.Gender;
                }
                else
                {
                    matchScore += Constants.DetractScores.Gender;
                }
            }

            if (project.CampusGuid != null)
            {
                if (project.CampusGuid == osc.CampusGuid)
                {
                    matchScore += Constants.AffirmScores.Campus;
                }
                else
                {
                    matchScore += Constants.DetractScores.Campus;
                }
            }

            if (project.StartDate != null && project.EndDate != null)
            {
                var dayMatches = osc.DayPreference != null && osc.DayPreference.Any(x => x.IndexOf(project.Day, StringComparison.OrdinalIgnoreCase) >= 0);
                var timeMatchesCount = osc.TimePreference?.Where(x => project.TimeOfDay.Any(y => y.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0)).Count();
                var timeMatches = timeMatchesCount.GetValueOrDefault(0) > 0;

                if (dayMatches)
                {
                    matchScore += Constants.AffirmScores.Day;
                }
                else
                {
                    matchScore += Constants.DetractScores.Day;
                }

                if (timeMatches)
                {
                    matchScore += Constants.AffirmScores.Time * timeMatchesCount.Value;
                }
                else
                {
                    matchScore += Constants.DetractScores.Time;
                }

                if (dayMatches && timeMatches)
                {
                    matchScore += Constants.AffirmScores.DayTime * timeMatchesCount.Value;
                }
                else
                {
                    matchScore += Constants.DetractScores.DayTime;
                }
            }

            var projectCount = osc.CurrentProjects;

            if (projectCount > 0 && isSelected)
            {
                projectCount--;
            }

            matchScore += Constants.DetractScores.Selection * projectCount;

            if (matchScore < 0)
            {
                matchScore = 0;
            }

            var projectMaxScore = GetProjectMaxScore(project);
            if (projectMaxScore > 0)
            {
                osc.MatchPercentage = Math.Round((decimal)matchScore / projectMaxScore * 100, 1);
            }
        }

        private int GetProjectMaxScore(ProjectResponseBag project)
        {
            if (_projectMaxScoreCache.ContainsKey(project.Id))
            {
                return _projectMaxScoreCache[project.Id];
            }

            var maxScore = 0;
            if (project.Gender != Gender.Unknown)
            {
                maxScore += Constants.AffirmScores.Gender;
            }
            if (project.CampusGuid != null)
            {
                maxScore += Constants.AffirmScores.Campus;
            }
            if (project.StartDate != null && project.EndDate != null)
            {
                maxScore += Constants.AffirmScores.Day;
                maxScore += Constants.AffirmScores.Time * project.TimeOfDay.Count;
                maxScore += Constants.AffirmScores.DayTime * project.TimeOfDay.Count;
            }

            _projectMaxScoreCache.Add(project.Id, maxScore);

            return maxScore;
        }

        private Gender GetGender(string gender)
        {
            if (gender.IsNullOrWhiteSpace())
            {
                return Gender.Unknown;
            }

            gender = gender.ToLower();

            return Constants.GroupGenderMenValues.Contains(gender)
                ? Gender.Male : Constants.GroupGenderFemaleValues.Contains(gender)
                ? Gender.Female : Gender.Unknown;
        }

        #endregion
    }
}
