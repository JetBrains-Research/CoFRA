using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.AnalysisSpecific;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.Contracts.Messages.Responses;
using Cofra.Contracts.Messages.Requests;
using Cofra.ReSharperPlugin.RemoteService;
using JetBrains.Application.Threading;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Caches;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;
using JetBrains.ReSharper.Daemon.Impl;
using File = Cofra.AbstractIL.Common.ControlStructures.File;

namespace Cofra.ReSharperPlugin.SolutionComponents
{
    [SolutionComponent]
    public class CofraFacade
    {
        private readonly Lifetime myLifetime;
        private readonly IThreading myThreading;
        private readonly ISolution mySolution;
        private readonly DaemonImpl myDaemonImpl;

        public bool ResultsAvailable { get; private set; }

        private File myLastFile;
        
        private CofraClient myClient;

        private readonly Queue<Action<CofraClient>> mySuspendedAcitons;

        public CofraFacade(
            Lifetime lifetime,
            IThreading threading,
            ISolution solution)
        {
            myLifetime = lifetime;
            myThreading = threading;
            mySolution = solution;

            myDaemonImpl = mySolution.GetComponent<DaemonImpl>();
            mySuspendedAcitons = new Queue<Action<CofraClient>>();

            ResultsAvailable = false;

            lifetime.AddBracket(StartSession, Terminate);
        }

        public CofraClient Client => myClient;

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
        
        private ProcessStartInfo PrepareProcessStartInfo(
            bool dotNetCore, FileSystemPath servicePath, 
            FileSystemPath librariesPath, FileSystemPath databasePath, int servicePort)
        { 
            return dotNetCore ? 
                new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"\"{servicePath.FullPath}\" --as-service --port {servicePort} " +
                                $"--database \"{databasePath}\"",
                    WorkingDirectory = librariesPath.FullPath,
                    UseShellExecute = false,
                    CreateNoWindow = false
                } 
                :
                new ProcessStartInfo
                {
                    FileName = servicePath.FullPath,
                    Arguments = $"--as-service --port {servicePort} " +
                                $"--database \"{databasePath}\"",
                    WorkingDirectory = librariesPath.FullPath,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };
        }
        
        private void SessionBody()
        {
            //TODO: move to the class fields or settings
            const string executableFileNameExe = "Cofra.Core.exe";
            const string executableFileNameDll = "Cofra.Core.dll";
            const string serviceDirectoryName = "CoFRA";
            const string cacheDirectoryName = "InterproceduralAnalysis";
            const string databaseName = "database.zip";
            
            bool dotNetCore = false;

            var librariesPath = Assembly.GetExecutingAssembly().GetPath().Directory;
            var servicePath = librariesPath.Combine(serviceDirectoryName);
            if (!servicePath.Combine(executableFileNameExe).ExistsFile)
            {
                dotNetCore = true;
                servicePath = servicePath.Combine(executableFileNameDll);
            }
            else
            {
                servicePath = servicePath.Combine(executableFileNameExe);
            }
            
            var servicePort = GetServicePort();
            var caches = mySolution.GetComponent<SolutionCaches>();
            var databasePath = caches.GetCacheFolder()
                .Combine(cacheDirectoryName)
                .Combine(databaseName);

            if (servicePath.IsAbsolute && servicePath.ExistsFile)
            {
                var processInfo = PrepareProcessStartInfo(
                        dotNetCore, servicePath, librariesPath, databasePath, servicePort);

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

                foreach (var action in mySuspendedAcitons)
                {
                    action(myClient);
                }

                myClient.Start();
            }
        }

        public FileId GetFileId(IPsiSourceFile sourceFile)
        {
            return new FileId(sourceFile.GetPersistentID());
        }

        public void SubmitFile(File file)
        {
            myLastFile = file;
            UpdateFile(file.Id.Value.ToString(), file);
        }

        public void DropCaches()
        {
            Action<CofraClient> action = client =>
                {
                    var request = new DropCachesRequest();
                    client.EnqueueRequest(request, _ => { });
                };

            if (myClient != null)
            {
                action(myClient);
            }
            else
            {
                mySuspendedAcitons.Enqueue(action);
            }
        }

        public void PerformAnalysis(AnalysisType type)
        {
            var request = new PerformAnalysisRequest(type);
            myClient.EnqueueRequest(request, _ => {});
        }

        public void CheckIfFieldsAreTainted(
            IEnumerable<(ClassId, string)> requestedFields,
            Action<IEnumerable<bool>> resultsProcessingAction)
        {
            var request = new CheckIfTaintedRequest(requestedFields);
            myClient.EnqueueRequest(request, response => 
                resultsProcessingAction(((TaintedFieldsResponse) response).TaintingFlags));
        }
        
        public void GetTaintedSinks(int fileIndex, Action<IEnumerable<IEnumerable<Statement>>> tracesHandler)
        {
            var request = new AnalysisResultsRequest(AnalysisType.TaintChecking, fileIndex);

            void ResponseHandler(Response response)
            {
                if (response is StatementsTraceResponse traces) tracesHandler(traces.Traces);
            }

            myClient.EnqueueRequest(request, ResponseHandler);
        }

        public File GetLastFile() => myLastFile;
    }
    
}