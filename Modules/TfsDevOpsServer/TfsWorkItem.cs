using DevOpsInterface;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace TfsDevOpsServer
{
    public static class TfsRelationNames
    {
        public static string Parent = "System.LinkTypes.Hierarchy-Reverse";
        public static string Child = "System.LinkTypes.Hierarchy-Forward";
        public static string Affects = "Microsoft.VSTS.Common.Affects-Forward";
        public static string AffectedBy = "Microsoft.VSTS.Common.Affects-Reverse";
        public static string Duplicate = "System.LinkTypes.Duplicate-Forward";
        public static string DuplicateOf = "System.LinkTypes.Duplicate-Reverse";
        public static string Successor = "System.LinkTypes.Dependency-Forward";
        public static string Predecessor = "System.LinkTypes.Dependency-Reverse";
        public static string Related = "System.LinkTypes.Related";
        public static string AttachedFile = "AttachedFile";
        public static string Hyperlink = "Hyperlink";
        public static string ArtifactLink = "ArtifactLink";

        // Link Information:
        //    https://learn.microsoft.com/en-us/azure/devops/boards/queries/link-type-reference?view=azure-devops

        public static Dictionary<string, string> RelationNameMap = new Dictionary<string, string>
        {
            // Workitem <==> WorkItem
            { "Parent", "System.LinkTypes.Hierarchy-Reverse" },
            { "Child", "System.LinkTypes.Hierarchy-Forward" },
            { "Affects", "Microsoft.VSTS.Common.Affects-Forward" },
            { "Affected By", "Microsoft.VSTS.Common.Affects-Reverse" },
            { "Duplicate", "System.LinkTypes.Duplicate-Forward" },
            { "Duplicate Of", "System.LinkTypes.Duplicate-Reverse" },
            { "Successor", "System.LinkTypes.Dependency-Forward" },
            { "Predecessor", "System.LinkTypes.Dependency-Reverse" },
            { "Related", "System.LinkTypes.Related" },

            // WorkItem <==> Resource
            { "Attached File", "AttachedFile" },
            { "Hyperlink", "Hyperlink" },
            { "Artifact Link", "ArtifactLink" },

            // WorkItem <==> Test
            { "Tested By", "Microsoft.VSTS.Common.TestedBy-Forward" },
            { "Tests", "Microsoft.VSTS.Common.TestedBy-Reverse" },

            // Test <==> Shared Parameters
            { "Referenced By", "Microsoft.VSTS.TestCase.SharedParameterReferencedBy-Forward" },
            { "References", "Microsoft.VSTS.TestCase.SharedParameterReferencedBy-Reverse" },

            // Test <==> Shared Steps
            { "Test Case", "Microsoft.VSTS.TestCase.SharedStepReferencedBy-Forward" },
            { "Shared Steps", "Microsoft.VSTS.TestCase.SharedStepReferencedBy-Reverse" },
        
            // Remote Links
            { "Remote Related", "System.LinkTypes.Remote.Related" },
            { "Produces For", "System.LinkTypes.Remote.Dependency-Forward" },
            { "Consumes From", "System.LinkTypes.Remote.Dependency-Reverse" },
        };

        public static string GetTfsRelName(string name)
        {
            if (RelationNameMap.ContainsKey(name))
                return RelationNameMap[name];

            foreach(string key in RelationNameMap.Keys)
            {
                if (key.ToLower() == name.ToLower())
                    return RelationNameMap[key];
            }

            return string.Empty;
        }

        public static bool IsTfsRelName(string name)
        {
            bool isTfsValue = false;

            if (RelationNameMap.ContainsKey(name))
                return false;

            foreach (string key in RelationNameMap.Keys)
                if (RelationNameMap[key] == name)
                    isTfsValue = true;

            return isTfsValue;
        }
    }

    public class TfsWorkItem : IDevOpsChangeRequest
    {
        internal WorkItem? ServerWorkitem { get; set; } = null;
        internal Dictionary<string, object> UpdatedFields { get; } = new Dictionary<string, object>();

        public int Id
        {
            get
            {
                if (ServerWorkitem != null)
                    return (int)ServerWorkitem.Id;
                return 0;
            }
        }

        public string AssignedTo
        {
            get
            {
                IdentityRef assignedTo = GetField<IdentityRef>("System.AssignedTo");
                if (assignedTo != null)
                    return assignedTo.DisplayName;
                return string.Empty;
            }
            set
            {
                SetField<string>("System.AssignedTo", value);
            }
        }

        public string State
        {
            get
            {
                return GetField<string>("System.State");
            }
            set
            {
                SetField("System.State", value);
            }
        }

        public string Title
        {
            get
            {
                return GetField<string>("System.Title");
            }
            set
            {
                SetField("System.Title", value);
            }
        }

        public string Description
        {
            get
            {
                return GetField<string>("System.Description");
            }
            set
            {
                SetField("System.Description", value);
            }
        }

        public string ItemType
        {
            get
            {
                return GetField<string>("System.WorkItemType");
            }
        }

        public string Tags
        {
            get
            {
                return GetField<string>("System.Tags");
            }
            set
            {
                SetField("System.Tags", value);
            }
        }

        public TfsWorkItem()
        {

        }

        public TfsWorkItem(WorkItem workitem) : this()
        {
            ServerWorkitem = workitem;
        }

        public IDictionary<string, object> GetAllFields()
        {
            if (ServerWorkitem == null)
                return UpdatedFields;

            return ServerWorkitem.Fields;
        }

        public T GetField<T>(string name)
        {
            T retval = default(T);

            if (ServerWorkitem == null)
            {
                if (UpdatedFields.ContainsKey(name))
                    return (T)UpdatedFields[name];
            }

            if (ServerWorkitem.Fields.ContainsKey(name))
                retval = (T)ServerWorkitem.Fields[name];

            return (T)retval;
        }

        public void SetField<T>(string name, T value)
        {
            if (ServerWorkitem != null)
            {
                if (ServerWorkitem.Fields.ContainsKey(name))
                {
                    if (ServerWorkitem.Fields[name].Equals(value))
                        return;
                }

                ServerWorkitem.Fields[name] = value;
            }

            UpdatedFields[name] = value;
        }

        internal List<int> GetLinkIds(string linktype)
        {
            List<int> retList = new List<int>();

            if (ServerWorkitem == null)
                return retList;

            string intLinkName = TfsRelationNames.GetTfsRelName(linktype);
            if (string.IsNullOrWhiteSpace(intLinkName))
                return retList;

            foreach (var relation in ServerWorkitem.Relations)
            {
                if (relation.Rel != intLinkName)
                    continue;

                // The URL of the child work item is in the Url property of the relation
                Uri childWorkItemUrl = new Uri(relation.Url);
                int childWorkItemId = int.Parse(childWorkItemUrl.Segments.Last());
                retList.Add(childWorkItemId);
            }

            return retList;
        }

    }
}
