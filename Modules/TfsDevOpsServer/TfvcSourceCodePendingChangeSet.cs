using DevOpsInterface;
using DevOpsSoapInterface;

namespace TfsDevOpsServer
{
    public class TfvcSourceCodePendingChangeSet : ISourceCodePendingChangeSet, IDisposable
    {
        private SoapExecutor m_executor = null;
        private Thread m_changeExecutorThread = null;
        private List<string> m_errorMsgs = new List<string>();

        private Dictionary<string, ISourceCodePendingChange> checkinPart2 = new Dictionary<string, ISourceCodePendingChange>();

        public Dictionary<string, ISourceCodePendingChange> Changes { get; } = new Dictionary<string, ISourceCodePendingChange>();
        public string Comment { get; set; } = string.Empty;

        public TfvcSourceCodePendingChangeSet()
        {
        }

        public TfvcSourceCodePendingChangeSet(SoapExecutor executor, string comment = "") : this()
        {
            Comment = comment;
            m_executor = executor;
        }

        public TfvcSourceCodePendingChangeSet(string comment) : this()
        {
            Comment = comment;
        }
        
        public void AddItem(string itemPath, string itemName, byte[] itemContent, string contentType, bool isBinary, int encoding)
        {
            TfvcSourceCodePendingOperation operation = new TfvcSourceCodePendingOperation(SourceCodeChangeType.Add);
            operation.Properties["Content"] = itemContent;
            operation.Properties["ContentType"] = contentType;
            operation.Properties["Encoding"] = encoding;

            string pathKey = itemPath.Trim('/') + "/" + itemName.Trim('/');
            pathKey = pathKey.ToLower();

            if (Changes.ContainsKey(pathKey))
            {
                TfvcSourceCodePendingChange change = (TfvcSourceCodePendingChange)Changes[pathKey];
                change.PendingOperations[SourceCodeChangeType.Add] = operation;
            }
            else
            {
                TfvcSourceCodePendingChange change = new TfvcSourceCodePendingChange();
                change.OriginalItem = new TfvcSourceItem(itemPath, itemName, itemContent, contentType, isBinary, encoding);

                change.PendingOperations[SourceCodeChangeType.Add] = operation;
                Changes[pathKey] = change;
            }
        }

        public void DeleteItem(ISourceCodeItem item)
        {
            TfvcSourceCodePendingOperation operation = new TfvcSourceCodePendingOperation(SourceCodeChangeType.Delete);

            string pathkey = item.ItemPath.ToLower();
            if (Changes.ContainsKey(pathkey))
            {
                TfvcSourceCodePendingChange change = (TfvcSourceCodePendingChange)Changes[pathkey];
                change.PendingOperations[SourceCodeChangeType.Delete] = operation;
            }
            else
            {
                TfvcSourceCodePendingChange change = new TfvcSourceCodePendingChange();
                change.OriginalItem = item;

                change.PendingOperations[SourceCodeChangeType.Delete] = operation;
                Changes[pathkey] = change;
            }
        }

        public void UndeleteItem(string serverItemPath)
        {
            UndeleteItem(serverItemPath, null);
        }

        public void UndeleteItem(string serverItemPath, byte[] itemContent)
        {
            TfvcSourceCodePendingOperation operation = new TfvcSourceCodePendingOperation(SourceCodeChangeType.Undelete);
            if (itemContent != null)
                operation.Properties["Content"] = itemContent;

            string pathkey = serverItemPath.ToLower();
            if (Changes.ContainsKey(pathkey))
            {
                TfvcSourceCodePendingChange change = (TfvcSourceCodePendingChange)Changes[pathkey];
                change.PendingOperations[SourceCodeChangeType.Undelete] = operation;
            }
            else
            {
                TfvcSourceCodePendingChange change = new TfvcSourceCodePendingChange();
                change.OriginalItem = new TfvcSourceItem(serverItemPath);

                change.PendingOperations[SourceCodeChangeType.Undelete] = operation;
                Changes[pathkey] = change;
            }
        }

        public void RenameItem(ISourceCodeItem item, string newName)
        {
            TfvcSourceCodePendingOperation operation = new TfvcSourceCodePendingOperation(SourceCodeChangeType.Rename);
            operation.Properties["NewItemPath"] = newName;

            string pathkey = item.ItemPath.ToLower();
            if (Changes.ContainsKey(pathkey))
            {
                TfvcSourceCodePendingChange change = (TfvcSourceCodePendingChange)Changes[pathkey];
                change.PendingOperations[SourceCodeChangeType.Rename] = operation;
            }
            else
            {
                TfvcSourceCodePendingChange change = new TfvcSourceCodePendingChange();
                change.OriginalItem = item;

                change.PendingOperations[SourceCodeChangeType.Rename] = operation;
                Changes[pathkey] = change;
            }
        }

        public void UpdateItem(ISourceCodeItem item)
        {
            TfvcSourceCodePendingOperation operation = new TfvcSourceCodePendingOperation(SourceCodeChangeType.Edit);
            operation.Properties["Content"] = item.Content;
            operation.Properties["ChangeEncoding"] = false;

            string pathkey = item.ItemPath.ToLower();
            if (Changes.ContainsKey(pathkey))
            {
                TfvcSourceCodePendingChange change = (TfvcSourceCodePendingChange)Changes[pathkey];
                change.PendingOperations[SourceCodeChangeType.Edit] = operation;
            }
            else
            {
                TfvcSourceCodePendingChange change = new TfvcSourceCodePendingChange();
                change.OriginalItem = item;

                change.PendingOperations[SourceCodeChangeType.Edit] = operation;
                Changes[pathkey] = change;
            }
        }

        public void UpdateItem(ISourceCodeItem item, int codePage)
        {
            TfvcSourceCodePendingOperation operation = new TfvcSourceCodePendingOperation(SourceCodeChangeType.Edit);
            operation.Properties["Content"] = item.Content;
            operation.Properties["Encoding"] = codePage;
            operation.Properties["ChangeEncoding"] = true;

            string pathkey = item.ItemPath.ToLower();
            if (Changes.ContainsKey(pathkey))
            {
                TfvcSourceCodePendingChange change = (TfvcSourceCodePendingChange)Changes[pathkey];
                change.PendingOperations[SourceCodeChangeType.Edit] = operation;
            }
            else
            {
                TfvcSourceCodePendingChange change = new TfvcSourceCodePendingChange();
                change.OriginalItem = item;

                change.PendingOperations[SourceCodeChangeType.Edit] = operation;
                Changes[pathkey] = change;
            }
        }

        internal List<int> CommitChanges()
        {
            if (m_executor != null)
                m_executor.Startup();

            List<int> checkinCsIds = new List<int>();
            m_errorMsgs.Clear();
            List<TfvcPendingChangeOpWrapper> OpList = SequenceChangeList(Changes);
            m_changeExecutorThread = new Thread(()=> { ChangeExecutorThread(OpList, true); });
            m_changeExecutorThread.Start();

            while (m_changeExecutorThread.IsAlive)
            {
                Thread.Sleep(500);
            }
            m_changeExecutorThread = null;

            int csId = -1;
            if (m_errorMsgs.Count == 0)
            {
                if (checkinPart2.Count > 0)
                    csId = m_executor.CheckIn(Comment + " - Part 1");
                else
                    csId = m_executor.CheckIn(Comment);

                checkinCsIds.Add(csId);
            }

            if (checkinPart2.Count > 0)
            {
                OpList = SequenceChangeList(checkinPart2);
                m_changeExecutorThread = new Thread(() => { ChangeExecutorThread(OpList, false); });
                m_changeExecutorThread.Start();

                while (m_changeExecutorThread.IsAlive)
                {
                    Thread.Sleep(500);
                }

                m_changeExecutorThread = null;

                if (m_errorMsgs.Count == 0)
                {
                    csId = m_executor.CheckIn(Comment + " - Part 2");
                    checkinCsIds.Add(csId);
                }
                else
                {
                    m_errorMsgs.Add("Failed to commit the changes in part 2.");
                }
            }

            if (m_executor != null)
                m_executor.Shutdown();
            m_executor = null;

            if (m_errorMsgs.Count > 0)
                throw new InvalidOperationException(string.Join(Environment.NewLine, m_errorMsgs));

            return checkinCsIds;
        }

        private void ChangeExecutorThread(List<TfvcPendingChangeOpWrapper> opList, bool allowPart2)
        {
            List<Thread> threads = new List<Thread>();

            int index = 0;
            while ((index < opList.Count) && (opList.Count > 0))
            {
                TfvcPendingChangeOpWrapper pending = null;
                if (threads.Count < 10)
                {
                    while ((pending == null) && (index < opList.Count))
                    {
                        bool depndOK = true;
                        if (opList[index].Dependencies.Count > 0)
                        {
                            foreach (TfvcSourceCodePendingChange dep in opList[index].Dependencies)
                            {
                                if ((dep.Status == OperationStatus.Pending) || (dep.Status == OperationStatus.InWork))
                                {
                                    depndOK = false;
                                    break;
                                }
                            }
                            
                        }
                        
                        if (depndOK)
                        {
                            pending = opList[index];
                            pending.Status = OperationStatus.InWork;
                        }
                        else
                        {
                            index++;
                        }
                    }

                    if (pending == null)
                        continue;

                    Thread itemThread = new Thread(() => OperationThread(pending, allowPart2));
                    threads.Add(itemThread);
                    itemThread.Start();

                    if (pending.PendingChange.OriginalItem.Itemtype == SourceCodeItemType.Folder)
                    {
                        if(pending.PendingChange.PendingOperations.ContainsKey(SourceCodeChangeType.Rename))
                        {
                            // Wait for the rename operation to finish
                            while (itemThread.IsAlive)
                                Thread.Sleep(500);

                            foreach (TfvcPendingChangeOpWrapper wrapper in opList)
                            {
                                if (wrapper.PendingChange.OriginalItem.ItemPath.Contains(pending.PendingChange.OriginalItem.ItemPath))
                                    wrapper.PendingChange.NewItemPath = wrapper.PendingChange.OriginalItem.ItemPath.Replace(pending.PendingChange.OriginalItem.ItemPath, pending.PendingChange.PendingOperations[SourceCodeChangeType.Rename].Properties["NewItemPath"].ToString());
                            }
                        }
                    }

                    opList.RemoveAt(index);

                    if (index >= opList.Count)
                        index = 0;
                }

                // clean up finished threads
                for (int i = 0; i < threads.Count; i++)
                {
                    if (!threads[i].IsAlive)
                    {
                        threads.RemoveAt(i);
                        i--;
                    }
                }

                Thread.Sleep(500);
            }

            foreach(Thread thread in threads)
            {
                if (thread.IsAlive)
                    thread.Join();
            }
            threads.Clear();
        }

        public void Dispose()
        {
            if ((m_changeExecutorThread != null) && (m_changeExecutorThread.IsAlive))
            {
                m_changeExecutorThread.Join();
                m_changeExecutorThread = null;
            }

            if (m_executor != null)
            {
                m_executor.Shutdown();
                m_executor = null;
            }
        }

        private List<TfvcPendingChangeOpWrapper> SequenceChangeList(Dictionary<string, ISourceCodePendingChange> changeList)
        {
            Dictionary<string, TfvcPendingChangeOpWrapper> processlist = new Dictionary<string, TfvcPendingChangeOpWrapper>();
            List<TfvcPendingChangeOpWrapper> retlist = new List<TfvcPendingChangeOpWrapper>();

            foreach(string key in changeList.Keys)
            {
                TfvcPendingChangeOpWrapper wrapper = new TfvcPendingChangeOpWrapper((TfvcSourceCodePendingChange)changeList[key]);
                processlist[key] = wrapper;
            }

            foreach (string procKey in processlist.Keys)
            {
                string itemPath = procKey;

                bool found = false;
                List<string> parts = new List<string>(itemPath.Split('/', StringSplitOptions.RemoveEmptyEntries));
                parts.RemoveAt(parts.Count - 1);

                TfvcPendingChangeOpWrapper topParent = null;
                while ((!found) && (parts.Count > 0))
                {
                    string checkKey = string.Join("/", parts).ToLower();
                    if (processlist.ContainsKey(checkKey))
                    {
                        topParent = processlist[checkKey];

                        //if (processlist[checkKey].PendingChange.PendingOperations.ContainsKey(SourceCodeChangeType.Delete))
                        //{
                        //    // Set parent items to be changed last
                        //    processlist[checkKey].Dependencies.Add(processlist[procKey].PendingChange);
                        //    found = true;
                        //}
                        //else
                        //{
                        //    // Set child items to be changed last
                        //    processlist[procKey].Dependencies.Add(processlist[checkKey].PendingChange);
                        //    found = true;
                        //}
                    }

                    parts.RemoveAt(parts.Count - 1);
                }

                if (topParent != null)
                {
                    if (topParent.PendingChange.PendingOperations.ContainsKey(SourceCodeChangeType.Delete))
                    {
                        if (processlist[procKey].PendingChange.PendingOperations.ContainsKey(SourceCodeChangeType.Delete))
                        {
                            processlist[procKey].Dependencies.Add(topParent.PendingChange);
                        }
                        else
                        {
                            // Set parent items to be changed last
                            topParent.Dependencies.Add(processlist[procKey].PendingChange);
                        }
                    }
                    else
                    {
                        // Set child items to be changed last
                        processlist[procKey].Dependencies.Add(topParent.PendingChange);
                    }
                }
            }

            foreach(string key in processlist.Keys)
                retlist.Add(processlist[key]);

            return retlist;
        }

        private void OperationThread(TfvcPendingChangeOpWrapper opWrapper, bool allowPart2)
        {
            string itemPath = opWrapper.PendingChange.OriginalItem.ItemPath;

            if (!string.IsNullOrWhiteSpace(opWrapper.PendingChange.NewItemPath))
                itemPath = opWrapper.PendingChange.NewItemPath;

            string encodingPage = null;
            if ((opWrapper.PendingChange.OriginalItem.Encoding != -1) && (opWrapper.PendingChange.OriginalItem.ChangeEncoding))
            {
                encodingPage = opWrapper.PendingChange.OriginalItem.Encoding.ToString();
            }

            if (opWrapper.PendingChange.PendingOperations.ContainsKey(SourceCodeChangeType.Undelete))
            {
                TfvcSourceCodePendingOperation op = opWrapper.PendingChange.PendingOperations[SourceCodeChangeType.Undelete];
                op.Status = OperationStatus.InWork;
                SoapResult result = null;

                if (op.Properties.ContainsKey("Content"))
                    result = m_executor.UndeleteFile(itemPath, (byte[])op.Properties["Content"]);
                else
                    result = m_executor.UndeleteFile(itemPath, null);

                if (result.Result == "success")
                {
                    op.Status = OperationStatus.Success;
                }
                else
                {
                    op.Status = OperationStatus.Failure;
                    m_errorMsgs.Add(result.Message);
                }
            }

            if (opWrapper.PendingChange.PendingOperations.ContainsKey(SourceCodeChangeType.Add))
            {
                TfvcSourceCodePendingOperation op = opWrapper.PendingChange.PendingOperations[SourceCodeChangeType.Add];
                op.Status = OperationStatus.InWork;
                SoapResult result = null;
                
                if ((op.Properties["Encoding"] != null) && ((int)op.Properties["Encoding"] == -1))
                    result = m_executor.AddFile(itemPath, (byte[])op.Properties["Content"], null);
                else
                    result = m_executor.AddFile(itemPath, (byte[])op.Properties["Content"], op.Properties["Encoding"].ToString());

                if (result.Result == "success")
                {
                    op.Status = OperationStatus.Success;
                }
                else
                {
                    op.Status = OperationStatus.Failure;
                    m_errorMsgs.Add(result.Message);
                }
            }

            if (opWrapper.PendingChange.PendingOperations.ContainsKey(SourceCodeChangeType.Rename))
            {
                TfvcSourceCodePendingOperation op = opWrapper.PendingChange.PendingOperations[SourceCodeChangeType.Rename];
                op.Status = OperationStatus.InWork;
                SoapResult result = null;
                if (opWrapper.PendingChange.NewItemPath == op.Properties["NewItemPath"].ToString())
                {
                    op.Status = OperationStatus.Success;
                }
                else
                {
                    result = m_executor.RenameFile(itemPath, (string)op.Properties["NewItemPath"]);
                    op.Status = OperationStatus.Success;
                    itemPath = (string)op.Properties["NewItemPath"];

                    if (result.Result == "success")
                    {
                        op.Status = OperationStatus.Success;
                    }
                    else
                    {
                        op.Status = OperationStatus.Failure;
                        m_errorMsgs.Add(result.Message);
                    }
                }
            }

            if (opWrapper.PendingChange.PendingOperations.ContainsKey(SourceCodeChangeType.Delete))
            {
                TfvcSourceCodePendingOperation op = opWrapper.PendingChange.PendingOperations[SourceCodeChangeType.Delete];
                op.Status = OperationStatus.InWork;
                SoapResult result = null;
                
                result = m_executor.DeleteFile(itemPath);
                if (result.Result == "success")
                {
                    op.Status = OperationStatus.Success;
                }
                else
                {
                    if ((result.Message.Equals("No items were deleted.", StringComparison.InvariantCultureIgnoreCase)) && (allowPart2))
                    {
                            checkinPart2[itemPath] = opWrapper.PendingChange;
                            op.Status = OperationStatus.Success;
                    }
                    else
                    {
                        op.Status = OperationStatus.Failure;
                        m_errorMsgs.Add(result.Message);
                    }
                }
            }

            if (opWrapper.PendingChange.PendingOperations.ContainsKey(SourceCodeChangeType.Edit))
            {
                TfvcSourceCodePendingOperation op = opWrapper.PendingChange.PendingOperations[SourceCodeChangeType.Edit];
                op.Status = OperationStatus.InWork;
                SoapResult result = null;

                if ((bool)op.Properties["ChangeEncoding"] == true)
                    result = m_executor.UpdateFile(itemPath, (byte[])op.Properties["Content"], op.Properties["Encoding"].ToString());
                else
                    result = m_executor.UpdateFile(itemPath, (byte[])op.Properties["Content"], null);

                if (result.Result == "success")
                {
                    op.Status = OperationStatus.Success;
                }
                else
                {
                    op.Status = OperationStatus.Failure;
                    m_errorMsgs.Add(result.Message);
                }

            }

            opWrapper.Status = OperationStatus.Success;
            foreach(SourceCodeChangeType type in opWrapper.PendingChange.PendingOperations.Keys)
            {
                if (opWrapper.PendingChange.PendingOperations[type].Status != OperationStatus.Success)
                {
                    opWrapper.Status = OperationStatus.Failure;
                    break;
                }
            }
        }
    }
}
