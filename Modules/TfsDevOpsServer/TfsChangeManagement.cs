using DevOpsMatrix.Interface;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace DevOpsMatrix.Tfs.Server
{
    public class TfsChangeManagement : IChangeManagement
    {
        private IDevOpsSettings m_settings;

        public string ServiceName { get { return "changemangement"; } }

        public IChangeItemFactory ItemFactory { get; set; }

        public TfsChangeManagement(IDevOpsSettings settings)
        {
            m_settings = settings;
            ItemFactory = new TfsChangeItemFactory();
        }

        private WorkItemTrackingHttpClient GetHttpClient()
        {
            VssConnection connection = TfsServiceTools.CreateConnection(m_settings);
            WorkItemTrackingHttpClient client = connection.GetClient<WorkItemTrackingHttpClient>();
            return client;
        }

        public IDevOpsChangeRequest GetChangeRequest(int id)
        {
            WorkItemTrackingHttpClient client = GetHttpClient();

            WorkItem workitem = client.GetWorkItemAsync(id).Result;

            TfsWorkItem devItem = new TfsWorkItem(workitem);

            return devItem;
        }

        public void UpdateChangeRequest(IDevOpsChangeRequest request)
        {
            WorkItemTrackingHttpClient client = GetHttpClient();
            TfsWorkItem item = (TfsWorkItem)request;

            JsonPatchDocument patchDocument = new JsonPatchDocument();
            
            if (item.UpdatedFields.Count == 0)
                return;

            foreach (string changedField in item.UpdatedFields.Keys)
            {
                JsonPatchOperation op = new JsonPatchOperation()
                {
                    Operation = Operation.Replace,
                    Path = "/fields/" + changedField,
                    Value = item.UpdatedFields[changedField]
                };
                patchDocument.Add(op);
            }
            
            WorkItem updated = client.UpdateWorkItemAsync(patchDocument, item.Id).Result;
            item.UpdatedFields.Clear();
        }

        public IDevOpsChangeRequest CreateChangeRequest(IDevOpsChangeRequest request, string itemType)
        {
            JsonPatchDocument creationData = new JsonPatchDocument();

            IDictionary<string, object> fields = request.GetAllFields();
            foreach (string key in fields.Keys)
            {
                JsonPatchOperation patch = new JsonPatchOperation();
                patch.Path = "/fields/" + key;
                patch.Value = fields[key];
                patch.Operation = Operation.Add;

                creationData.Add(patch);
            }

            WorkItemTrackingHttpClient client = GetHttpClient();

            WorkItem tfsWi = client.CreateWorkItemAsync(creationData, m_settings.ProjectName, itemType).Result;
            TfsWorkItem retItem = new TfsWorkItem(tfsWi);

            return retItem;
        }

        public void PermenantlyDeleteChangeRequest(int id)
        {
            WorkItemTrackingHttpClient client = GetHttpClient();
            WorkItemDelete result = client.DeleteWorkItemAsync(id, true).Result;
        }

        public object? GetServerProperty(string itemId, Dictionary<string, object>? arguments)
        {
            object? retval = null;

            if (itemId == "CurrentIterationPath")
            {
                if (arguments == null)
                    return null;
                if ((!arguments.ContainsKey("IterationPathRoot") && arguments["IterationPathRoot"] == null))
                    return null;

                string iterpathroot = arguments["IterationPathRoot"].ToString();
                string iterationPath = GetIterationPathForDate(iterpathroot, DateTimeOffset.Now.LocalDateTime);
                retval = iterationPath;
            }

            return retval;
        }

        public Dictionary<string, List<IDevOpsChangeRequest>> GetLinkedItems(IDevOpsChangeRequest item)
        {
            Dictionary<string, List<IDevOpsChangeRequest>> retList = new Dictionary<string, List<IDevOpsChangeRequest>>();

            TfsWorkItem localitem = (TfsWorkItem)item;

            if (localitem.ServerWorkitem == null)
                return retList;

            if (localitem.ServerWorkitem.Relations == null)
                return retList;

            foreach (var relation in localitem.ServerWorkitem.Relations)
            {
                if (!retList.ContainsKey(relation.Rel))
                    retList[relation.Rel] = new List<IDevOpsChangeRequest>();

                // The URL of the child work item is in the Url property of the relation
                Uri childWorkItemUrl = new Uri(relation.Url);
                int childWorkItemId = int.Parse(childWorkItemUrl.Segments.Last());
                IDevOpsChangeRequest linkedItem = GetChangeRequest(childWorkItemId);
                retList[relation.Rel].Add(linkedItem);
            }

            return retList;
        }

        public void AddLinkedItem(string linktype, IDevOpsChangeRequest sourceItem, IDevOpsChangeRequest targetItem)
        {
            WorkItemTrackingHttpClient client = GetHttpClient();

            string? intLinkName = null;
            if (TfsRelationNames.IsTfsRelName(linktype))
                intLinkName = linktype;
            else
                intLinkName = TfsRelationNames.GetTfsRelName(linktype);

            if (string.IsNullOrWhiteSpace(intLinkName))
                return;

            // Create a JsonPatchDocument to represent the update
            JsonPatchDocument patchDocument =
            [
                // Add an Add operation to the patch document to add the link
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = intLinkName,
                        url = client.BaseAddress + "_apis/wit/workItems/" + targetItem.Id, // The URL of the target work item
                        attributes = new { comment = "Linked work item" }
                    }
                },
            ];

            WorkItem updated = client.UpdateWorkItemAsync(patchDocument, sourceItem.Id).Result;
            ((TfsWorkItem)sourceItem).ServerWorkitem = updated;
        }

        public void RemoveLinkedItem(string linktype, IDevOpsChangeRequest sourceItem, IDevOpsChangeRequest targetItem)
        {
            WorkItemTrackingHttpClient client = GetHttpClient();

            string? intLinkName = null;
            if (TfsRelationNames.IsTfsRelName(linktype))
                intLinkName = linktype;
            else
                intLinkName = TfsRelationNames.GetTfsRelName(linktype);

            if (string.IsNullOrWhiteSpace(intLinkName))
                return;

            TfsWorkItem srcItem = (TfsWorkItem)sourceItem;
            TfsWorkItem tgtItem = (TfsWorkItem)targetItem;

            int index = 0;
            bool found = false;
            while ((!found) && (index < srcItem.ServerWorkitem.Relations.Count))
            {
                var relation = srcItem.ServerWorkitem.Relations[index];
                if (relation.Url == ((TfsWorkItem)targetItem).ServerWorkitem.Url)
                    found = true;
                else
                    index++;
            }
            
            if (!found)
                return;

            // Create a JsonPatchDocument to represent the update
            JsonPatchDocument patchDocument =
            [
                // Add an Add operation to the patch document to add the link
                new JsonPatchOperation()
                {
                    Operation = Operation.Remove,
                    Path = "/relations/" + index,
                },
            ];

            WorkItem updated = client.UpdateWorkItemAsync(patchDocument, sourceItem.Id).Result;
            ((TfsWorkItem)sourceItem).ServerWorkitem = updated;
        }

        private string GetIterationPathForDate(string iterationPathRoot, DateTime date)
        {
            WorkItemTrackingHttpClient client = GetHttpClient();

            WorkItemClassificationNode? iterRoot = null;

            string cleanIterationRoot = iterationPathRoot;

            if (iterationPathRoot.StartsWith(m_settings.ProjectName, StringComparison.CurrentCultureIgnoreCase))
            {
                cleanIterationRoot = iterationPathRoot.Replace(m_settings.ProjectName, string.Empty, StringComparison.CurrentCultureIgnoreCase);
                cleanIterationRoot = cleanIterationRoot.Trim(new char[] { '\\', '/' });
            }

            string pathRoot = string.Empty;

            if (!string.IsNullOrWhiteSpace(cleanIterationRoot))
                pathRoot = cleanIterationRoot;

            if (string.IsNullOrWhiteSpace(pathRoot))
               iterRoot = client.GetClassificationNodeAsync(m_settings.ProjectName, TreeStructureGroup.Iterations, depth: 10).Result;
            else
                iterRoot = client.GetClassificationNodeAsync(m_settings.ProjectName, TreeStructureGroup.Iterations, path: pathRoot, depth: 10).Result;

            WorkItemClassificationNode retNode = iterRoot;
            foreach (WorkItemClassificationNode child in iterRoot.Children)
            {
                WorkItemClassificationNode subItem = SearchTreeBranch(child, date);
                if (subItem != null)
                {
                    retNode = subItem;
                    break;
                }
            }

            string retPath = retNode.Path.Replace(m_settings.ProjectName + "\\Iteration", m_settings.ProjectName).Trim('\\');
            return retPath;
        }

        private WorkItemClassificationNode? SearchTreeBranch(WorkItemClassificationNode item, DateTime current)
        {
            WorkItemClassificationNode? retNode = null;

            DateTime start = DateTime.MaxValue;
            DateTime end = DateTime.MaxValue;
            if (item.Attributes != null)
            {
                if (item.Attributes.ContainsKey("startDate"))
                    start = (DateTime)item.Attributes["startDate"];

                if (item.Attributes.ContainsKey("finishDate"))
                    end = (DateTime)item.Attributes["finishDate"];
            }

            if ((start <= current) && (current <= end))
            {
                retNode = item;
                if (item.Children != null)
                {
                    foreach (WorkItemClassificationNode child in item.Children)
                    {
                        WorkItemClassificationNode? subItem = SearchTreeBranch(child, current);
                        if (subItem != null)
                        {
                            retNode = subItem;
                            break;
                        }
                    }
                }
            }

            return retNode;
        }
    }
}
