using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.AnalysisSpecific;
using Cofra.Contracts.Messages.Responses;
using Cofra.Contracts.Messages.Requests;
using Cofra.ReSharperPlugin.RemoteService;
using DevExpress.Utils.About;
using JetBrains.Application.changes;
using JetBrains.Application.Settings;
using JetBrains.Application.Threading;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Caches;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Caches.Persistence;
using JetBrains.Util;
using JetBrains.ProjectModel.Impl;
using JetBrains.ReSharper.Daemon.Impl;
using JetBrains.ReSharper.Daemon.SolutionAnalysis.FileImages;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Impl;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Tree;
using File = Cofra.AbstractIL.Common.ControlStructures.File;

namespace Cofra.ReSharperPlugin.SolutionComponents
{
    [SolutionComponent]
    public class CofraFacade
    {
        private readonly Lifetime myLifetime;
        private readonly IContextBoundSettingsStoreLive mySettingsStore;
        private readonly IThreading myThreading;
        private readonly ISolution mySolution;
        private readonly IPsiCaches myPsiCaches;
        private readonly ChangeManager myChangeManager;
        private readonly DaemonImpl myDaemonImpl;
        private readonly IPersistentIndexManager myPersistentIndexManager;

        private volatile bool mySweaEnabled;
        public bool ResultsAvailable { get; private set; }

        private File myLastFile;
        
        private CofraClient myClient;

        public CofraFacade(Lifetime lifetime,
            IThreading threading,
            ISolution solution,
            ISettingsStore settingsStore)
        {
            mySettingsStore = settingsStore.BindToContextLive(lifetime, ContextRange.Smart(solution.ToDataContext()));
            myLifetime = lifetime;
            myThreading = threading;
            mySolution = solution;

            myChangeManager = mySolution.GetComponent<ChangeManager>();
            myDaemonImpl = mySolution.GetComponent<DaemonImpl>();

            myPersistentIndexManager = mySolution.GetComponent<PersistentIndexManager>();

            try
            {
                System.IO.File.WriteAllText("C:\\work\\exceptions.txt","");
            }
            catch (Exception e)
            {
            }

            ResultsAvailable = false;

            var sweaConfiguration = mySolution.GetComponent<SolutionAnalysisConfiguration>();
            mySweaEnabled = sweaConfiguration.Enabled.Value;
            sweaConfiguration.Enabled.Change.Advise(lifetime, args => mySweaEnabled = args?.New ?? false);

            myChangeManager.Changed2.Advise(lifetime, OnChanged);

            lifetime.AddBracket(StartSession, Terminate);
        }

        private void OnPsiChange(ITreeNode changedElement, PsiChangedElementType type)
        {
        }

        private void OnChanged(ChangeEventArgs args)
        {
            if (!mySweaEnabled)
            {
                var delta = args.ChangeMap.GetChanges<PsiModuleChange>();
                var affected = delta
                    .SelectMany(change => change.FileChanges)
                    .Where(change => change.Type == PsiModuleChange.ChangeType.Invalidated || 
                                     change.Type == PsiModuleChange.ChangeType.Modified)
                    .Select(change => change.Item);

                foreach (var file in affected)
                {
                    myDaemonImpl.ForceReHighlight(file.Document);
                }
            }
        }

        public void UpdateMethod(Method method)
        {
            var request = new UpdateMethodRequest(method);
            myClient.EnqueueRequest(request, _ => { });
        }

        public void UpdateFile(string name, File file)
        {
            var request = new UpdateFileRequest(name, file);
            myClient.EnqueueRequest(request, _ => { });
        }

        private void StartSession()
        {
            var task = myThreading.Tasks.Create(myLifetime, SessionBody, options: TaskCreationOptions.LongRunning);
            task.Start();
        }

        private void Terminate()
        {
            myClient.Stop();
        }

        private FileSystemPath GetLibrariesPath()
        {
            return Assembly.GetExecutingAssembly().GetPath().Directory;
            //return FileSystemPath.Parse(mySettingsStore.GetValue((InterproceduralAnalysisSettings settings) => settings.PathToLockChecker));
        }

        static int FreeTcpPort()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint) listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
        
        private static int GetServicePort()
        {
            return FreeTcpPort();
        }
        
        private void SessionBody()
        {
            var librariesPath = GetLibrariesPath();
            var servicePath = librariesPath.Combine("CoFRA").Combine("Cofra.Core.exe");
            var servicePort = GetServicePort();

            var caches = mySolution.GetComponent<SolutionCaches>();
            var databasePath = caches.GetCacheFolder().Combine("InterproceduralAnalysis").Combine("database.zip");

            if (servicePath.IsAbsolute && servicePath.ExistsFile)
            {
                var processInfo =
                    new ProcessStartInfo
                    {
                        FileName = servicePath.FullPath,
                        Arguments = $"--as-service --port {servicePort} " +
                                    $"--analysis \"{librariesPath.FullPath}\" " +
                                    $"--database \"{databasePath}\"",
                        WorkingDirectory = librariesPath.FullPath,
                        UseShellExecute = false,
                        CreateNoWindow = false
                    };

                var process =
                    new Process
                    {
                        StartInfo = processInfo
                    };

                process.Start();

                myClient = null;
                while (myClient == null)
                {
                    try
                    {
                        myClient = SocketBasedCofraClient.Connect(IPAddress.Loopback, servicePort);
                    }
                    catch (SocketException)
                    {
                        Thread.Sleep(100);
                    }
                }

                myClient.Start();
            }
        }

        public FileId GetFileId(IPsiSourceFile sourceFile)
        {
            return new FileId(sourceFile.GetPersistentID());
        }

//        public void LogFile(File file, string fileName)
//        {
//            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//            System.IO.File.AppendAllText(fileName, $"ASSEMBLY {assemblyFolder}\n\n\n");
//            System.IO.File.AppendAllText(fileName, $"File {file.Id} methods:\n");
//            foreach (var method in file.Methods)
//            {
//                var baseMethod = method.BaseMethods.IsEmpty() ? ": " + method.BaseMethods.Select(x => x.ToString()).Join(", "): "";
//                System.IO.File.AppendAllText(fileName, $"  {method.Id} {baseMethod}\n");
//                System.IO.File.AppendAllText(fileName, $"  Initials:\n    {method.InitialInstructions} \n  Instructions:\n");
//                foreach (var instruction in method.Instructions)
//                    System.IO.File.AppendAllText(fileName, $"    {instruction.Id} | {instruction.Statement.Type} | {instruction.Continuation} \n");
//                System.IO.File.AppendAllText(fileName, "\n");
//            }
//        }

        public void SubmitFile(File file)
        {
            //LogFile(file, @"C:\work\logs\log.txt");
            
//            foreach (var method in file.Classes)
//            {
//                UpdateMethod(method);
//            }

            //var methodNames = file.Methods.Select(method => method.Id.Value);
            myLastFile = file;
            UpdateFile(file.Id.Value.ToString(), file);
            
        }

        public File GetLastFile() => myLastFile;
    }
    
}