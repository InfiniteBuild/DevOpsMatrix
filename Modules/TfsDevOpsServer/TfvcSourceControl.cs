using DevOpsInterface;
using DevOpsSoapInterface;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Linq.Expressions;
using System.Text;

namespace TfsDevOpsServer
{
    public class TfvcSourceControl : TfsSourceControl, ITfvcSourceControl
    {
        public static string FeatureSoapCommit = "SoapCommit";

        public TfvcSourceControl(IDevOpsSettings settings) : base(settings)
        {
            DevOpsFeatureToggle.Instance.DisableFeature(FeatureSoapCommit);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private TfvcHttpClient GetHttpClient()
        {
            VssConnection connection = TfsServiceTools.CreateConnection(m_settings);
            TfvcHttpClient tfvcClient = connection.GetClient<TfvcHttpClient>();
            return tfvcClient;
        }

        public override List<string> GetBranchList()
        {
            List<string> retList = new List<string>();

            TfvcHttpClient tfvcClient = GetHttpClient();

            List<TfvcItem> rootchildren = tfvcClient.GetItemsAsync(m_settings.ProjectName, "$/" + m_settings.ProjectName).Result;
            foreach (TfvcItem item in rootchildren)
            {
                IEnumerable<TfvcBranchRef> branches = tfvcClient.GetBranchRefsAsync(item.Path).Result;
                foreach (TfvcBranchRef branchRef in branches)
                    if (!retList.Contains(branchRef.Path))
                        retList.Add(branchRef.Path);
            }

            return retList;
        }

        public override ISourceCodeItem? GetParentBranch(ISourceCodeItem svritem)
        {
            TfvcHttpClient tfvcClient = GetHttpClient();
            ISourceCodeItem parent = null;

            try
            {
                TfvcBranch? branch = null;
                string currentPath = svritem.ItemPath;
                while ((branch == null) && !string.IsNullOrWhiteSpace(currentPath))
                {
                    try
                    {
                        branch = tfvcClient.GetBranchAsync(currentPath, includeParent: true).Result;
                    }
                    catch (Exception ex)
                    {
                    }
                    currentPath = Path.GetDirectoryName(currentPath);
                }
                parent = GetItem(branch.Parent.Path);
            }
            catch (Exception ex)
            {

            }

            return parent;
        }

        public override ISourceCodeItem GetItem(string itemPath, bool includeContent = false)
        {
            TfvcHttpClient tfvcClient = GetHttpClient();
            TfvcItem serverItem = null;

            bool retry = true;
            int retryCount = 0;
            while ((retry) && retryCount < 5)
            {
                try
                {
                    serverItem = tfvcClient.GetItemAsync(itemPath).Result;
                    retry = false;
                }
                catch (AggregateException ex)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Thread.Sleep(1000);
                }
            }

            TfvcSourceItem retItem = new TfvcSourceItem(serverItem);
            if (retItem != null)
            {
                if (includeContent == true)
                {
                    retItem.ContentType = serverItem.ContentMetadata.ContentType;
                    retItem.IsBinary = serverItem.ContentMetadata.IsBinary;
                    retItem.Encoding = serverItem.ContentMetadata.Encoding;

                    using (Stream content = tfvcClient.GetItemContentAsync(itemPath).Result)
                    {
                        using (MemoryStream memStream = new MemoryStream())
                        {
                            content.CopyTo(memStream);
                            memStream.Position = 0;

                            byte[] bytes = new byte[memStream.Length];
                            memStream.Read(bytes, 0, (int)memStream.Length);
                            retItem.Content = bytes;
                        }
                    }

                    try
                    {
                        if ((retItem.Encoding == -1) && (!retItem.IsBinary))
                        {
                            retItem.Content = Encoding.UTF8.GetBytes(serverItem.Content);
                            retItem.Encoding = Encoding.UTF8.CodePage;
                            return retItem;
                        }
                    }
                    catch (Exception ex)
                    {
                        retItem.IsBinary = true;
                    }
                }
                else
                {
                    retItem.Content = null;
                }
            }

            return retItem;
        }

        public override ISourceCodeItem GetItem(string itemPath, int version, bool includeContent = false)
        {
            TfvcHttpClient tfvcClient = GetHttpClient();
            TfvcItem serverItem = null;

            TfvcVersionDescriptor versiondesc = new TfvcVersionDescriptor();
            versiondesc.Version = version.ToString();

            bool retry = true;
            int retryCount = 0;
            while((retry) && retryCount < 5)
            {
                try
                {
                    serverItem = tfvcClient.GetItemAsync(itemPath, versionDescriptor: versiondesc).Result;
                    retry = false;
                }
                catch (AggregateException ex)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Thread.Sleep(1000);
                }
            }

            if (serverItem == null)
                return null;

            TfvcSourceItem retItem = new TfvcSourceItem(serverItem);
            if (retItem != null)
            {
                if (includeContent == true)
                {
                    retItem.ContentType = serverItem.ContentMetadata.ContentType;
                    retItem.IsBinary = serverItem.ContentMetadata.IsBinary;
                    retItem.Encoding = serverItem.ContentMetadata.Encoding;

                    retry = true;
                    retryCount = 0;
                    while ((retry) && (retryCount < 5))
                    {
                        try
                        {
                            using (Stream content = tfvcClient.GetItemContentAsync(itemPath, versionDescriptor: versiondesc).Result)
                            {
                                using (MemoryStream memStream = new MemoryStream())
                                {
                                    content.CopyTo(memStream);
                                    memStream.Position = 0;

                                    byte[] bytes = new byte[memStream.Length];
                                    memStream.Read(bytes, 0, (int)memStream.Length);
                                    retItem.Content = bytes;
                                }
                            }

                            try
                            {
                                if ((retItem.Encoding == -1) && (!retItem.IsBinary))
                                {
                                    retItem.Content = Encoding.UTF8.GetBytes(serverItem.Content);
                                    retItem.Encoding = Encoding.UTF8.CodePage;
                                    return retItem;
                                }
                            }
                            catch (Exception exc)
                            {
                                retItem.IsBinary = true;
                            }

                            retry = false;
                        }
                        catch (Exception ex)
                        {
                            retItem.Content = null;
                            retryCount++;
                            Thread.Sleep(1000);
                        }
                    }
                }
                else
                {
                    retItem.Content = null;
                }
            }

            return retItem;
        }

        public override byte[]? GetItemContent(ISourceCodeItem item)
        {
            TfvcHttpClient tfvcClient = GetHttpClient();
            byte[]? itemContent = null;

            TfvcVersionDescriptor versiondesc = new TfvcVersionDescriptor();
            versiondesc.Version = item.Version.ToString();

            bool retry = true;
            int retryCount = 0;
            while ((retry) && (retryCount < 5))
            {
                try
                {
                    using (Stream content = tfvcClient.GetItemContentAsync(item.ItemPath, versionDescriptor: versiondesc).Result)
                    {
                        using (MemoryStream memStream = new MemoryStream())
                        {
                            content.CopyTo(memStream);
                            memStream.Position = 0;

                            byte[] bytes = new byte[memStream.Length];
                            memStream.Read(bytes, 0, (int)memStream.Length);
                            itemContent = bytes;
                        }
                    }

                    retry = false;
                }
                catch (Exception ex)
                {
                    itemContent = null;
                    retryCount++;
                    Thread.Sleep(1000);
                }
            }

            return itemContent; 
        }

        public override List<int> CommitChanges(ISourceCodePendingChangeSet pendingchanges)
        {
            if (DevOpsFeatureToggle.IsEnabled(FeatureSoapCommit))
                return CommitChanges_SoapApi(pendingchanges);
            else
                return CommitChanges_RestAPI(pendingchanges);
        }

        public override ISourceCodePendingChangeSet CreatePendingChangeset()
        {
            if (DevOpsFeatureToggle.IsEnabled(FeatureSoapCommit))
            {
                SoapExecutor executor = new SoapExecutor(m_settings.ServerUri.ToString(), m_settings.ProjectName, m_settings.AccessToken);
                return new TfvcSourceCodePendingChangeSet(executor);
            }
            else
            {
                return new TfvcSourceCodePendingChangeSet();
            }
        }

        public override List<ISourceCodeHistory> GetHistory(ISourceCodeItem sourceItem)
        {
            TfvcHttpClient tfvcClient = GetHttpClient();

            List<ISourceCodeHistory> retList = new List<ISourceCodeHistory>();

            TfvcChangesetSearchCriteria criteria = new TfvcChangesetSearchCriteria();
            criteria.ItemPath = sourceItem.ItemPath;

            int currentcount = 0;
            List<TfvcChangesetRef> changesetList = new List<TfvcChangesetRef>();
            List<TfvcChangesetRef> sessionlist = tfvcClient.GetChangesetsAsync(searchCriteria: criteria, skip: 0, top: 1000).Result;
            while ((sessionlist != null) && (sessionlist.Count > 0))
            {
                currentcount += sessionlist.Count;
                changesetList.AddRange(sessionlist);
                sessionlist = tfvcClient.GetChangesetsAsync(searchCriteria: criteria, skip: currentcount, top: 1000).Result;
            }

            changesetList.OrderBy(x => x.ChangesetId);

            foreach (TfvcChangesetRef csref in changesetList)
            {
                TfvcSourceCodeHistory csInfo = new TfvcSourceCodeHistory();
                csInfo.Id = csref.ChangesetId;
                csInfo.Comment = csref.Comment;
                csInfo.Timestamp = csref.CreatedDate;
                csInfo.Owner = csref.Author.DisplayName;

                int changeCounter = 0;
                List<TfvcChange> changeList = tfvcClient.GetChangesetChangesAsync(csref.ChangesetId, 0, 2000).Result;
                while (changeList.Count > 0)
                {
                    foreach (TfvcChange change in changeList)
                    {
                        TfvcSourceCodeHistoryItem item = new TfvcSourceCodeHistoryItem();
                        item.ItemPath = change.Item.Path;
                        item.ChangeType = MapChangeType(change.ChangeType);
                        item.OriginalPath = change.SourceServerItem;
                        item.ItemType = change.Item.IsFolder ? "Folder" : "File";

                        // Load merge information
                        if (change.MergeSources != null)
                        {
                            foreach (TfvcMergeSource source in change.MergeSources)
                            {
                                TfsVcMergeInfo info = new TfsVcMergeInfo(source.ServerItem, source.VersionFrom, source.VersionTo);
                                csInfo.MergeInfo.Add(info);
                            }
                        }

                        csInfo.Changes.Add(item);
                    }
                    changeCounter = changeCounter + changeList.Count;
                    changeList = tfvcClient.GetChangesetChangesAsync(csref.ChangesetId, changeCounter, 2000).Result;
                }

                retList.Add(csInfo);
            }

            return retList;
        }

        public override List<ISourceCodeHistory> GetHistory(ISourceCodeItem sourceItem, int count = 100, int fromId = -1, int toId = -1)
        {
            TfvcHttpClient tfvcClient = GetHttpClient();

            List<ISourceCodeHistory> retList = new List<ISourceCodeHistory>();

            TfvcChangesetSearchCriteria criteria = new TfvcChangesetSearchCriteria();
            criteria.ItemPath = sourceItem.ItemPath;
            if (fromId > 0) criteria.FromId = fromId;
            if (toId > 0) criteria.ToId = toId;

            List<TfvcChangesetRef> changesetList;
            if (count > 0)
            {
                changesetList = tfvcClient.GetChangesetsAsync(top: count, searchCriteria: criteria).Result;
            }
            else
            {
                int currentcount = 0;
                changesetList = new List<TfvcChangesetRef>();
                List<TfvcChangesetRef> sessionlist = tfvcClient.GetChangesetsAsync(searchCriteria: criteria, skip: 0, top: 1000).Result;
                while ((sessionlist != null) && (sessionlist.Count > 0))
                {
                    currentcount += sessionlist.Count;
                    changesetList.AddRange(sessionlist);
                    sessionlist = tfvcClient.GetChangesetsAsync(searchCriteria: criteria, skip: currentcount, top: 1000).Result;
                }
            }
            
            changesetList.OrderBy(x => x.ChangesetId);

            foreach (TfvcChangesetRef csref in changesetList)
            {
                TfvcSourceCodeHistory csInfo = new TfvcSourceCodeHistory();
                csInfo.Id = csref.ChangesetId;
                csInfo.Comment = csref.Comment;
                csInfo.Timestamp = csref.CreatedDate;
                csInfo.Owner = csref.Author.DisplayName;

                int changeCounter = 0;
                List<TfvcChange> changeList = tfvcClient.GetChangesetChangesAsync(csref.ChangesetId, 0, 2000).Result;
                while (changeList.Count > 0)
                {
                    foreach (TfvcChange change in changeList)
                    {
                        TfvcSourceCodeHistoryItem item = new TfvcSourceCodeHistoryItem();
                        item.ItemPath = change.Item.Path;
                        item.ChangeType = MapChangeType(change.ChangeType);
                        item.OriginalPath = change.SourceServerItem;
                        item.ItemType = change.Item.IsFolder ? "Folder" : "File";

                        // Load merge information
                        if (change.MergeSources != null)
                        {
                            foreach (TfvcMergeSource source in change.MergeSources)
                            {
                                TfsVcMergeInfo info = new TfsVcMergeInfo(source.ServerItem, source.VersionFrom, source.VersionTo);
                                csInfo.MergeInfo.Add(info);
                            }
                        }

                        csInfo.Changes.Add(item);
                    }
                    changeCounter = changeCounter + changeList.Count;
                    changeList = tfvcClient.GetChangesetChangesAsync(csref.ChangesetId, changeCounter, 2000).Result;
                }

                retList.Add(csInfo);
            }

            return retList;
        }

        public override List<ISourceCodeHistory> GetHistory(ISourceCodeItem sourceItem, string from = "", string until = "")
        {
            TfvcHttpClient tfvcClient = GetHttpClient();

            List<ISourceCodeHistory> retList = new List<ISourceCodeHistory>();

            TfvcChangesetSearchCriteria criteria = new TfvcChangesetSearchCriteria();
            criteria.ItemPath = sourceItem.ItemPath;
            if (!string.IsNullOrWhiteSpace(from)) { criteria.FromDate = from; }
            if (!string.IsNullOrWhiteSpace(until)) { criteria.ToDate = until; }

            List<TfvcChangesetRef> changesetList;

            int currentcount = 0;
            changesetList = new List<TfvcChangesetRef>();
            List<TfvcChangesetRef> sessionlist = tfvcClient.GetChangesetsAsync(searchCriteria: criteria, skip: 0, top: 1000).Result;
            while ((sessionlist != null) && (sessionlist.Count > 0))
            {
                currentcount += sessionlist.Count;
                changesetList.AddRange(sessionlist);
                sessionlist = tfvcClient.GetChangesetsAsync(searchCriteria: criteria, skip: currentcount, top: 1000).Result;
            }

            changesetList.OrderBy(x => x.ChangesetId);

            foreach (TfvcChangesetRef csref in changesetList)
            {
                TfvcSourceCodeHistory csInfo = new TfvcSourceCodeHistory();
                csInfo.Id = csref.ChangesetId;
                csInfo.Comment = csref.Comment;
                csInfo.Timestamp = csref.CreatedDate;
                csInfo.Owner = csref.Author.DisplayName;

                int changeCounter = 0;
                List<TfvcChange> changeList = tfvcClient.GetChangesetChangesAsync(csref.ChangesetId, 0, 2000).Result;
                while (changeList.Count > 0)
                {
                    foreach (TfvcChange change in changeList)
                    {
                        TfvcSourceCodeHistoryItem item = new TfvcSourceCodeHistoryItem();
                        item.ItemPath = change.Item.Path;
                        item.ChangeType = MapChangeType(change.ChangeType);
                        item.OriginalPath = change.SourceServerItem;
                        item.ItemType = change.Item.IsFolder ? "Folder" : "File";

                        // Load merge information
                        if (change.MergeSources != null)
                        {
                            foreach (TfvcMergeSource source in change.MergeSources)
                            {
                                TfsVcMergeInfo info = new TfsVcMergeInfo(source.ServerItem, source.VersionFrom, source.VersionTo);
                                csInfo.MergeInfo.Add(info);
                            }
                        }

                        csInfo.Changes.Add(item);
                    }
                    changeCounter = changeCounter + changeList.Count;
                    changeList = tfvcClient.GetChangesetChangesAsync(csref.ChangesetId, changeCounter, 2000).Result;
                }

                retList.Add(csInfo);
            }

            return retList;
        }

        private SourceCodeChangeType MapChangeType(VersionControlChangeType tfsChangeType)
        {
            SourceCodeChangeType changetype = SourceCodeChangeType.None;

            if ((VersionControlChangeType.None & tfsChangeType) == VersionControlChangeType.None)
                changetype |= SourceCodeChangeType.None;

            if ((VersionControlChangeType.Add & tfsChangeType) == VersionControlChangeType.Add)
                changetype |= SourceCodeChangeType.Add;

            if (((VersionControlChangeType.Encoding & tfsChangeType) == VersionControlChangeType.Encoding) ||
                ((VersionControlChangeType.Edit & tfsChangeType) == VersionControlChangeType.Edit))
                changetype |= SourceCodeChangeType.Edit;

            if ((VersionControlChangeType.Rename & tfsChangeType) == VersionControlChangeType.Rename)
                changetype |= SourceCodeChangeType.Rename;

            if ((VersionControlChangeType.SourceRename & tfsChangeType) == VersionControlChangeType.SourceRename)
                changetype |= SourceCodeChangeType.SourceRename;

            if ((VersionControlChangeType.TargetRename & tfsChangeType) == VersionControlChangeType.TargetRename)
                changetype |= SourceCodeChangeType.TargetRename;

            if ((VersionControlChangeType.Delete & tfsChangeType) == VersionControlChangeType.Delete)
                changetype |= SourceCodeChangeType.Delete;

            if ((VersionControlChangeType.Undelete & tfsChangeType) == VersionControlChangeType.Undelete)
                changetype |= SourceCodeChangeType.Undelete;

            if ((VersionControlChangeType.Branch & tfsChangeType) == VersionControlChangeType.Branch)
                changetype |= SourceCodeChangeType.Branch;

            if ((VersionControlChangeType.Merge & tfsChangeType) == VersionControlChangeType.Merge)
                changetype |= SourceCodeChangeType.Merge;

            if ((VersionControlChangeType.Rollback & tfsChangeType) == VersionControlChangeType.Rollback)
                changetype |= SourceCodeChangeType.Undo;

            return changetype;
        }

        private VersionControlChangeType MapChangeType(SourceCodeChangeType changeType)
        {
            VersionControlChangeType tfschangetype = VersionControlChangeType.None;

            if ((SourceCodeChangeType.None & changeType) == SourceCodeChangeType.None)
                tfschangetype |= VersionControlChangeType.None;

            if ((SourceCodeChangeType.Add & changeType) == SourceCodeChangeType.Add)
                tfschangetype |= VersionControlChangeType.Add;

            if ((SourceCodeChangeType.Edit & changeType) == SourceCodeChangeType.Edit)
                tfschangetype |= VersionControlChangeType.Edit;

            if ((SourceCodeChangeType.Rename & changeType) == SourceCodeChangeType.Rename)
                tfschangetype |= VersionControlChangeType.Rename;

            if ((SourceCodeChangeType.SourceRename & changeType) == SourceCodeChangeType.SourceRename)
                tfschangetype |= VersionControlChangeType.SourceRename;

            if ((SourceCodeChangeType.TargetRename & changeType) == SourceCodeChangeType.TargetRename)
                tfschangetype |= VersionControlChangeType.TargetRename;

            if ((SourceCodeChangeType.Delete & changeType) == SourceCodeChangeType.Delete)
                tfschangetype |= VersionControlChangeType.Delete;

            if ((SourceCodeChangeType.Undelete & changeType) == SourceCodeChangeType.Undelete)
                tfschangetype |= VersionControlChangeType.Undelete;

            if ((SourceCodeChangeType.Branch & changeType) == SourceCodeChangeType.Branch)
                tfschangetype |= VersionControlChangeType.Branch;

            if ((SourceCodeChangeType.Merge & changeType) == SourceCodeChangeType.Merge)
                tfschangetype |= VersionControlChangeType.Merge;

            if ((SourceCodeChangeType.Undo & changeType) == SourceCodeChangeType.Undo)
                tfschangetype |= VersionControlChangeType.Rollback;

            return tfschangetype;
        }

        private List<int> CommitChanges_RestAPI(ISourceCodePendingChangeSet pendingchanges)
        {
            TfvcHttpClient tfvcClient = GetHttpClient();

            List<TfvcChange> changeList = new List<TfvcChange>();
            List<int> checkinCs = new List<int>();

            // Create a new TfvcChangeset
            TfvcChangeset changeset = new TfvcChangeset();
            if (!string.IsNullOrWhiteSpace(pendingchanges.Comment))
                changeset.Comment = pendingchanges.Comment;

            // Add the changes
            foreach (string key in pendingchanges.Changes.Keys)
            {
                ISourceCodePendingChange pending = pendingchanges.Changes[key];
                TfvcChange change = new TfvcChange();

                // It doesn't matter what type of change is being made - it's controlled by the change type
                change.ChangeType = MapChangeType(pending.ChangeType);

                change.Item = new TfvcItem();
                change.Item.Path = pending.OriginalItem.ItemPath;
                change.Item.ContentMetadata = new FileContentMetadata
                {
                    ContentType = pending.OriginalItem.ContentType,
                    IsBinary = pending.OriginalItem.IsBinary
                };
                if (pending.OriginalItem.Content != null)
                {
                    if (pending.OriginalItem.IsBinary)
                    {
                        change.Item.ContentMetadata.Encoding = -1;
                        change.NewContent = new ItemContent
                        {
                            Content = Convert.ToBase64String(pending.OriginalItem.Content),
                            ContentType = ItemContentType.Base64Encoded
                        };
                    }
                    else
                    {
                        change.Item.ContentMetadata.Encoding = Encoding.UTF8.CodePage;
                        change.NewContent = new ItemContent
                        {
                            Content = Encoding.UTF8.GetString(pending.OriginalItem.Content),
                            ContentType = ItemContentType.RawText
                        };
                    }
                }

                if (pending.ChangeType != SourceCodeChangeType.Add)
                    change.Item.ChangesetVersion = pending.OriginalItem.Version;

                if (pending.ChangeType == SourceCodeChangeType.Rename)
                {
                    change.Item.Path = pending.NewItemPath;
                    change.SourceServerItem = pending.OriginalItem.ItemPath;
                }

                changeList.Add(change);
            };

            changeset.Changes = changeList;
            
            // Commit the changeset
            TfvcChangesetRef changesetRef = tfvcClient.CreateChangesetAsync(changeset).Result;
            checkinCs.Add(changesetRef.ChangesetId);

            return checkinCs;
        }

        private List<int> CommitChanges_SoapApi(ISourceCodePendingChangeSet changeset)
        {
            TfvcSourceCodePendingChangeSet pending = (TfvcSourceCodePendingChangeSet)changeset;
            List<int> changesetIds = pending.CommitChanges();

            return changesetIds;
        }

    }

}
