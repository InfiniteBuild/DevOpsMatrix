using DevOpsMatrix.Interface;
using Microsoft.VisualStudio.Setup.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DevOpsMatrix.Tfs.Server
{
    public class TfVcLocalWorkspace : IDevOpsWorkspace
    {
        private static string tfExePath = string.Empty;
        private bool IsValid = false;
        private DevOpsSettings svrSettings;
        

        public string WorkspaceName { get; private set; } = string.Empty;
        public string LocalWorkspaceRoot { get; private set; } = string.Empty;
        public string ServerPathRoot { get; private set; } = string.Empty;
        public string BranchRoot { get; private set;} = string.Empty;

        public IDevOpsServer? DevOpsServer { get; private set; } = null;

        public TfVcLocalWorkspace()
        {

        }

        public TfVcLocalWorkspace(string localPath)
        {
            Initialize(localPath);
        }

        public void AddItem(string itemPath)
        {
            if (!IsValidWorkspace())
                throw new Exception("Invalid workspace.");

            string output;
            int exitCode = ExecuteTfCommand($"vc add \"{itemPath}\"", out output);
            if (exitCode != 0)
                throw new Exception($"Failed to add item '{itemPath}' to workspace. Exit code: {exitCode}");
        }

        public void Checkout(string itemPath)
        {
            if (!IsValidWorkspace())
                throw new Exception("Invalid workspace.");

            string output;
            int exitCode = ExecuteTfCommand($"vc checkout \"{itemPath}\"", out output);
            if (exitCode != 0)
                throw new Exception($"Failed to checkout item '{itemPath}' in workspace. Exit code: {exitCode}");
        }

        public void CommitChanges(string itemPath, string comment, bool recursive = false)
        {
            if (!IsValidWorkspace())
                throw new Exception("Invalid workspace.");

            string command = $"vc checkin \"{itemPath}\" /comment:\"{comment}\"";
            if ((Directory.Exists(itemPath)) && (recursive))
            {
                command += " /recursive"; 
            }

            string output;
            int exitCode = ExecuteTfCommand(command, out output);
            if (exitCode != 0)
                throw new Exception($"Failed to commit changes for item '{itemPath}' in workspace. Exit code: {exitCode}");
        }

        public void DeleteItem(string itemPath)
        {
            if (!IsValidWorkspace())
                throw new Exception("Invalid workspace.");

            string output;
            int exitCode = ExecuteTfCommand($"vc delete \"{itemPath}\"", out output);
            if (exitCode != 0)
                throw new Exception($"Failed to delete item '{itemPath}' from workspace. Exit code: {exitCode}");
        }

        public WorkspaceCreator GetCreator()
        {
            return (localPath) =>
            {
                return new TfVcLocalWorkspace(localPath);
            };
        }

        public List<ISourceCodePendingChange> GetPendingChanges(string itemPath)
        {
            if (!IsValidWorkspace())
                throw new Exception("Invalid workspace.");

            string output;
            int exitCode = ExecuteTfCommand($"vc status \"{itemPath}\" /recursive /format:xml", out output);

            List<ISourceCodePendingChange> pendingChanges = ParseTfStatusXml(output);
            return pendingChanges;
        }

        public bool IsValidWorkspace()
        {
            return IsValid;
        }

        public void RenameItem(string itemPath, string newName)
        {
            if (!IsValidWorkspace())
                throw new Exception("Invalid workspace.");
            string output;
            int exitCode = ExecuteTfCommand($"vc rename \"{itemPath}\" \"{newName}\"", out output);
            if (exitCode != 0)
                throw new Exception($"Failed to rename item '{itemPath}' to '{newName}' in workspace. Exit code: {exitCode}");
        }

        public void UndoChanges(string itemPath)
        {
            if (!IsValidWorkspace())
                throw new Exception("Invalid workspace.");

            string output;
            int exitCode = ExecuteTfCommand($"vc undo \"{itemPath}\" /recursive", out output);
            if (exitCode != 0)
                throw new Exception($"Failed to undo changes for item '{itemPath}' in workspace. Exit code: {exitCode}");
        }

        public string GetServerPath(string localPath)
        {
            if (!IsValidWorkspace())
                throw new Exception("Invalid workspace.");

            ISourceCodeControl sourceControl = DevOpsServer.GetDevOpsService<ITfvcSourceControl>();

            Match? svrPathMatch = ExecuteTfCommand($"vc workfold {localPath}", new Regex(@"^\s*(\$/[^\s:]+)", RegexOptions.Multiline));
            string svrPath = svrPathMatch.Result("$1").Trim();

            return svrPath;
        }

        public string GetLocalPath(string serverPath)
        {
            if (!IsValidWorkspace())
                throw new Exception("Invalid workspace.");

            ISourceCodeControl sourceControl = DevOpsServer.GetDevOpsService<ITfvcSourceControl>();

            Match? localBranchRootMatch = ExecuteTfCommand($"vc workfold /collection:{svrSettings.ServerUri} /workspace:{WorkspaceName} {serverPath}", new Regex(":\\s*(?<localpath>[A-Z]:\\\\[^\\r\\n]+)", RegexOptions.Multiline));
            string localpath = localBranchRootMatch?.Groups["localpath"].Value.Trim() ?? string.Empty;

            return localpath;
        }

        private void Initialize(string localPath)
        {
            tfExePath = GetTfExePath();
            if (string.IsNullOrWhiteSpace(tfExePath))
            {
                IsValid = false;
                return;
            }

            TfvcLocalWorkspaceInfo localWorkspace = GetWorkspaceRoot(localPath);
            if (localWorkspace != null)
            {
                IsValid = true;
                WorkspaceName = localWorkspace.WorkspaceName;
                LocalWorkspaceRoot = localWorkspace.ServerToLocalPathMap.FirstOrDefault(x => localPath.Contains(x.Value)).Value;
                ServerPathRoot = localWorkspace.ServerToLocalPathMap.FirstOrDefault(x => localPath.Contains(x.Value)).Key;

                var match = Regex.Match(ServerPathRoot, @"^\$/([^\\]+)");
                string projectName = match.Success ? match.Groups[1].Value : string.Empty;

                svrSettings = new DevOpsSettings
                {
                    Name = localWorkspace.CollectionUrl,
                    ServerType = DevOpsServerType.Tfs,
                    ServerUri = new Uri(localWorkspace.CollectionUrl),
                    ProjectName = projectName,
                };

                DevOpsServer = new TfsDevOpsServer(svrSettings);

                ISourceCodeControl sourceControl = DevOpsServer.GetDevOpsService<ITfvcSourceControl>();
                Match? svrPathMatch = ExecuteTfCommand($"vc workfold {localPath}", new Regex(@"^\s*(\$/[^\s:]+)", RegexOptions.Multiline));
                string svrPath = svrPathMatch.Result("$1").Trim();
                ISourceCodeItem? branchitem = sourceControl.GetItemBranch(svrPath);

                Match? localBranchRootMatch = ExecuteTfCommand($"vc workfold /collection:{localWorkspace.CollectionUrl} /workspace:{localWorkspace.WorkspaceName} {branchitem.ItemPath}", new Regex(":\\s*(?<localpath>[A-Z]:\\\\[^\\r\\n]+)", RegexOptions.Multiline));
                BranchRoot = localBranchRootMatch?.Groups["localpath"].Value.Trim() ?? string.Empty;
            }    
        }

        private TfvcLocalWorkspaceInfo GetWorkspaceRoot(string localPath)
        {
            List<TfvcLocalWorkspaceInfo> allWorkspaces = ParseAllTfvcWorkspaces();
            foreach(TfvcLocalWorkspaceInfo workspace in allWorkspaces)
            {
                foreach (string key in workspace.ServerToLocalPathMap.Keys)
                {
                    if (localPath.ToLower().Contains(workspace.ServerToLocalPathMap[key].ToLower()))
                    {
                        IsValid = true;
                        LocalWorkspaceRoot = workspace.ServerToLocalPathMap[key];
                        return workspace;
                    }
                }
            }
            return null;
        }

        private string GetTfExePath()
        {
            ISetupInstance VsLatest = null;
            ISetupConfiguration setupConfiguration = new SetupConfiguration();
            IEnumSetupInstances enumerator = setupConfiguration.EnumInstances();
            int count;
            do
            {
                ISetupInstance[] setupInstances = new ISetupInstance[1];
                enumerator.Next(1, setupInstances, out count);
                if (count == 1 && setupInstances[0] != null)
                {
                    string installPath = setupInstances[0].GetInstallationPath();
                    string version = setupInstances[0].GetInstallationVersion();

                    ISetupInstanceCatalog setupInstanceCatalog = (ISetupInstanceCatalog)setupInstances[0];
                    bool prerelease = setupInstanceCatalog.IsPrerelease();

                    if (!prerelease)
                    {
                        if (VsLatest == null)
                        {
                            VsLatest = setupInstances[0];
                        }
                        else
                        {
                            string latestver = VsLatest.GetInstallationVersion();
                            Version latest = Version.Parse(latestver);
                            Version check = Version.Parse(version);

                            if (check > latest)
                                VsLatest = setupInstances[0];
                        }
                    }
                }
            }
            while (count == 1);

            if (VsLatest != null)
            {
                string installPath = VsLatest.GetInstallationPath();
                string tfexePath = Path.Combine(installPath, "Common7\\IDE\\CommonExtensions\\Microsoft\\TeamFoundation\\Team Explorer\\TF.exe");
                if (File.Exists(tfexePath))
                {
                    return tfexePath;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Executes 'tf vc workspaces' to get all collection URLs, then for each collection,
        /// executes 'tf vc workspaces /collection:<URL> /format:xml' to get detailed info.
        /// This method performs the potentially slow TFVC command calls and parsing.
        /// </summary>
        /// <returns>A list of all known TFVC workspaces with their mappings.</returns>
        private static List<TfvcLocalWorkspaceInfo> ParseAllTfvcWorkspaces()
        {
            List<TfvcLocalWorkspaceInfo> allWorkspaces = new List<TfvcLocalWorkspaceInfo>();
            List<string> collectionUrls = new List<string>();

            // --- Step 1: Get all Collection URLs from 'tf vc workspaces' (text output) ---
            // This command outputs collections, then workspaces under them. We only need the collection URLs first.
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = tfExePath,
                    Arguments = "vc workspaces", // List all workspaces and collections globally accessible
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null) return allWorkspaces;

                    process.WaitForExit(15000); // Give it up to 15 seconds to complete
                    if (!process.HasExited)
                    {
                        process.Kill();
                        Console.WriteLine("Error: 'tf vc workspaces' command timed out during collection discovery.");
                        return allWorkspaces;
                    }

                    if (process.ExitCode != 0)
                    {
                        string error = process.StandardError.ReadToEnd();
                        Console.WriteLine($"Error executing 'tf vc workspaces' for collection discovery: {error.Trim()}");
                        return allWorkspaces;
                    }

                    string output = process.StandardOutput.ReadToEnd();
                    // Regex to find "Collection: <URL>" lines.
                    // It looks for "Collection:" at the start of a line, followed by whitespace, then captures the URL.
                    MatchCollection matches = Regex.Matches(output, @"^Collection:\s*(?<url>https?://[^\s]+)", RegexOptions.Multiline | RegexOptions.IgnoreCase);

                    foreach (Match match in matches)
                    {
                        string url = match.Groups["url"].Value.Trim();
                        if (!string.IsNullOrWhiteSpace(url) && !collectionUrls.Contains(url))
                        {
                            collectionUrls.Add(url);
                        }
                    }

                    if (!collectionUrls.Any())
                    {
                        Console.WriteLine("No TFVC collections found or could not parse collection URLs from 'tf vc workspaces' output. This is expected if no TFS/Azure DevOps connections are configured.");
                        // In some environments, 'tf vc workspaces' without arguments might list workspaces without a preceding "Collection:" header.
                        // This scenario is harder to parse reliably without fixed-width column knowledge.
                        // For now, if no collection URLs are found, we stop here.
                        return allWorkspaces;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Warning: 'tf.exe' not found in system PATH. Cannot discover TFVC collections. Please ensure Visual Studio or Team Explorer is installed and 'tf.exe' is in your PATH.");
                return allWorkspaces;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred during TFVC collection discovery: {ex.Message}");
                return allWorkspaces;
            }

            // --- Step 2: Get detailed workspace info for each collection (XML output) ---
            foreach (string collectionUrl in collectionUrls)
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = tfExePath,
                        // Now, explicitly specify the collection and request XML format.
                        // This is the part you've indicated might work for specific collections.
                        Arguments = $"vc workspaces /collection:\"{collectionUrl}\" /format:xml",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (var process = Process.Start(startInfo))
                    {
                        if (process == null) continue; // Skip to next collection

                        process.WaitForExit(15000); // Give it time
                        if (!process.HasExited)
                        {
                            process.Kill();
                            Console.WriteLine($"Error: 'tf vc workspaces' command timed out for collection {collectionUrl}.");
                            continue;
                        }

                        if (process.ExitCode != 0)
                        {
                            string error = process.StandardError.ReadToEnd();
                            Console.WriteLine($"Error executing 'tf vc workspaces /collection:\"{collectionUrl}\"': {error.Trim()}");
                            continue; // Skip to next collection
                        }

                        string xmlOutput = process.StandardOutput.ReadToEnd();
                        if (string.IsNullOrWhiteSpace(xmlOutput))
                        {
                            // This collection might have no workspaces for the current user, or output was suppressed.
                            continue;
                        }

                        XDocument doc = XDocument.Parse(xmlOutput);
                        foreach (var workspaceElement in doc.Descendants("Workspace"))
                        {
                            TfvcLocalWorkspaceInfo workspaceInfo = new TfvcLocalWorkspaceInfo
                            {
                                WorkspaceName = workspaceElement.Attribute("name")?.Value,
                                OwnerName = workspaceElement.Attribute("owner")?.Value,
                                CollectionUrl = collectionUrl, // Use the actual collection URL used in the command
                                Comment = workspaceElement.Element("Comment")?.Value // Element for comment
                            };

                            foreach (var mappingElement in workspaceElement.Descendants("WorkingFolder"))
                            {
                                string serverPath = mappingElement.Attribute("item")?.Value;
                                string localPath = mappingElement.Attribute("local")?.Value;
                                if (!string.IsNullOrEmpty(serverPath) && !string.IsNullOrEmpty(localPath))
                                {
                                    workspaceInfo.ServerToLocalPathMap[serverPath] = Path.GetFullPath(localPath); // Normalize local path
                                }
                            }
                            allWorkspaces.Add(workspaceInfo);
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine($"Warning: 'tf.exe' not found in system PATH. Cannot get TFVC workspace details for collection {collectionUrl}.");
                }
                catch (System.Xml.XmlException ex)
                {
                    Console.WriteLine($"Error parsing TFVC XML output for collection {collectionUrl}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred while getting TFVC info for collection {collectionUrl}: {ex.Message}");
                }
            }

            return allWorkspaces;
        }

        private Match? ExecuteTfCommand(string command, Regex retValMatcher)
        {
            Match retMatch = null;

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = tfExePath,
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null) return null; // Skip to next collection

                    process.WaitForExit(15000); // Give it time
                    if (!process.HasExited)
                    {
                        process.Kill();
                        Console.WriteLine($"Timeout Error: {command}");
                    }

                    if (process.ExitCode != 0)
                    {
                        string error = process.StandardError.ReadToEnd();
                        Console.WriteLine($"Error executing '{command}': {error.Trim()}");
                    }

                    string output = process.StandardOutput.ReadToEnd();
                    retMatch = retValMatcher.Match(output);
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Warning: 'tf.exe' not found in system PATH. Cannot discover TFVC collections. Please ensure Visual Studio or Team Explorer is installed and 'tf.exe' is in your PATH.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred during TFVC collection discovery: {ex.Message}");
            }

            return retMatch;
        }

        private int ExecuteTfCommand(string command, out string output)
        {
            int exitCode = 0;
            output = string.Empty;

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = tfExePath,
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        output = "Failed to launch tf.exe process.";
                        return 1; // Skip to next collection
                    }

                    process.WaitForExit(15000); // Give it time
                    if (!process.HasExited)
                    {
                        process.Kill();
                        Console.WriteLine($"Timeout Error: {command}");
                    }

                    output = process.StandardOutput.ReadToEnd();
                    exitCode = process.ExitCode;
                    if (process.ExitCode != 0)
                    {
                        string error = process.StandardError.ReadToEnd();
                        Console.WriteLine($"Error executing '{command}': {error.Trim()}");
                    }

                }
            }
            catch (FileNotFoundException)
            {
                exitCode = 1;
                output = "Warning: 'tf.exe' not found.";
                Console.WriteLine("Warning: 'tf.exe' not found in system PATH. Cannot discover TFVC collections. Please ensure Visual Studio or Team Explorer is installed and 'tf.exe' is in your PATH.");
            }
            catch (Exception ex)
            {
                exitCode = 1;
                output = $"An unexpected error occurred during Tf.exe execution: {ex.Message}";
                Console.WriteLine($"An unexpected error occurred during Tf.exe execution: {ex.Message}");
            }

            return exitCode;
        }

        public List<ISourceCodePendingChange> ParseTfStatusXml(string xml)
        {
            var pendingChanges = new List<ISourceCodePendingChange>();
            var doc = XDocument.Parse(xml);

            // Each <pendingchange> element represents a pending change
            foreach (var changeElem in doc.Descendants("PendingChange"))
            {
                var changeTypeStr = changeElem.Attribute("chg")?.Value ?? string.Empty;
                SourceCodeChangeType changeType = SourceCodeChangeType.None;
                Enum.TryParse(changeTypeStr, true, out changeType);

                pendingChanges.Add(new TfvcSourceCodePendingChange
                {
                    OriginalItem = new TfvcSourceItem(Path.GetDirectoryName(changeElem.Attribute("local")?.Value), Path.GetFileName(changeElem.Attribute("local")?.Value), null, string.Empty, false, 0),
                    NewItemPath = changeElem.Attribute("local")?.Value ?? string.Empty,
                    ChangeType = changeType,
                    Status = OperationStatus.Pending // Set as appropriate
                });
            }

            return pendingChanges;
        }
    }
}
