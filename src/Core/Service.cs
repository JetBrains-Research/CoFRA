using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types.AnalysisSpecific;
using Cofra.AbstractIL.Internal;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Dumps;
using Cofra.AbstractIL.Internal.Statements;
using Cofra.AbstractIL.Internal.Types;
using Cofra.Contracts.Messages;
using Cofra.Contracts.Messages.Requests;
using Cofra.Contracts.Messages.Responses;
using Cofra.Core.Analyzes;
using Cofra.Core.Util;
using File = System.IO.File;

namespace Cofra.Core
{
    internal class Service
    {
        private GraphStructuredProgramBuilder myProgramBuilder;
        private readonly TcpClient myClient;

        private readonly string myDatabasePath;
        private bool myIsProcessing;

        private readonly AnalyzesResultsCache myResultsCache;
        private volatile bool myProgramLock;
        private volatile bool myProgramHasChanged;

        private readonly Queue<Action> myQueuedCommits;

        public Service(TcpClient client, string databasePath)
        {
            myProgramBuilder = new GraphStructuredProgramBuilder();

            myClient = client;
            myDatabasePath = databasePath;

            myResultsCache = new AnalyzesResultsCache();
            myProgramHasChanged = true;
            myProgramLock = false;

            myQueuedCommits = new Queue<Action>();
        }

        public void Start()
        {
            RestoreProgram();
            Logging.Log("Restoring completed");

            //////////////////////////

            if (myClient == null)
            {
                var currentTime = DateTime.Now;
                //SaveProgram();
                //TODO: Analysis
                Console.WriteLine(
                    $"Analysis completed in {DateTime.Now.Subtract(currentTime).TotalMilliseconds} milliseconds");
                Console.ReadLine();
                return;
            }

            ///////////////////////////

            myIsProcessing = true;
            var stream = myClient.GetStream();
            var reader = new StreamReader(stream);

            var requestsSerializer = new DataContractSerializer(typeof(Request));
            var responsesSerializer = new DataContractSerializer(typeof(Response));

            //TODO: Errors processing
            while (myIsProcessing)
            {
                Response response = null;

                try
                {
                    var rawMessage = reader.ReadLine();
                    Logging.LogXML(rawMessage);

                    if (rawMessage == null)
                    {
                        myIsProcessing = false;
                        continue;
                    }

                    Request request;
                    using (var stringReader = new StringReader(rawMessage))
                    using (var xmlReader = XmlReader.Create(stringReader))
                    {
                        request = (Request) requestsSerializer.ReadObject(xmlReader);
                    }

                    response = ProcessMessage(request);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    response = new FailureResponse();
                }

                using (var stringWriter = new StringWriter())
                using (var xmlWriter = XmlWriter.Create(stringWriter))
                {
                    responsesSerializer.WriteObject(xmlWriter, response);
                    xmlWriter.Flush();
                    var serialized = stringWriter.ToString();

                    Logging.LogXML(serialized);

                    //TODO: Make new line sending more accurate
                    responsesSerializer.WriteObject(stream, response);
                    stream.Write(new[] {(byte) '\n'}, 0, 1);
                    stream.Flush();
                }
            }
        }

        private List<ResolvedMethod<int>> PerformTypePropagation(out List<SecondaryEntity> collectedSecondaryEntities)
        {
            var program = myProgramBuilder.GetProgram();
            var typePropagation = new TypePropagation(program);

            program.RestoreBaseClasses();
            program.PrepareStartsToNameMap();

            var methods = program.CollectAllMethods().ToList();
            foreach (var method in methods)
            {
                method.ResetInvocationMarker();
            }

            var allLocalVariables = methods
                .SelectMany(method => method.Variables.Values).ToList();
            var allClassFields = program.CollectAllClasses().SelectMany(clazz => clazz.Fields).ToList();
            var allRealVariables = allLocalVariables.Cast<SecondaryEntity>().Concat(allClassFields);

            var allAdditionalVariables = methods.SelectMany(method => method.AdditionalVariables);
            var allVariables = allRealVariables.Concat(allAdditionalVariables).ToList();

            foreach (var variable in allVariables) 
            {
                variable.ResetPropagatedTypes();
            }

            typePropagation.Analyze(methods);

            Logging.Log("Adding default types");

            int added = 0;
            foreach (var localVariable in allLocalVariables)
            {
                if (localVariable.DefaultClassType >= 0 && 
                    localVariable.HasSubscriptions() && 
                    !localVariable.CollectedPrimaries.Any())
                {
                    localVariable.AddPrimary(new ResolvedClassId(localVariable.DefaultClassType));

                    added += 1;
                    if (added % 500 == 0)
                    {
                        Logging.Log(added.ToString());
                    }
                }

            }

            foreach (var classField in allClassFields)
            {
                if (classField.DefaultClassType >= 0 && 
                    classField.HasSubscriptions() && 
                    !classField.CollectedPrimaries.Any())
                {
                    classField.AddPrimary(new ResolvedClassId(classField.DefaultClassType));

                    added += 1;
                    if (added % 500 == 0)
                    {
                        Logging.Log(added.ToString());
                    }
                }
            }

            typePropagation.FinishAnalysis();

            Logging.Log($"Variables: {allVariables.Count}, " +
                        $"Unresolved: {allVariables.Count(variable => !variable.CollectedPrimaries.Any())}, " +
                        $"Unrelated: {allVariables.Count(variable => !variable.AllRecipients.Any())}");

            var starts = methods.Where(method => !method.Invoked).ToList();
            Logging.Log($"{methods.Count} {starts.Count}");

            //TODO: locks
            Task.Run(() =>
            {
                foreach (var variable in allVariables)
                {
                    variable.DropTransitiveClosure();
                }
            });

            collectedSecondaryEntities = allVariables;
            return starts;
        }

        private Task RunAnalysis(PerformAnalysisRequest request)
        {
            myProgramLock = true;
            return Task.Run(() =>
            {
                try
                {
                    var program = myProgramBuilder.GetProgram();

                    var starts = PerformTypePropagation(out var secondaryEntities);

                    //TODO: locks
                    Task.Run(() =>
                    {
                        foreach (var secondaryEntity in secondaryEntities)
                        {
                            secondaryEntity.DropAllCollectedPrimaries();
                        }
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                myProgramLock = false;

                GC.Collect();
            });
        }

        private Response GetAnalysisResults(AnalysisResultsRequest request)
        {
            return new FailureResponse();
        }

        private void UpdateFile(UpdateFileRequest updateFileMessage)
        {
            Action commit = () =>
            {
                var fileName = updateFileMessage.Name;

                Logging.Log($"Update file: {fileName}");

                var containedMethods =
                    updateFileMessage.File.Classes.SelectMany(
                        updatedClass => myProgramBuilder.UpdateClass(updatedClass));

                myProgramBuilder.UpdateFile(fileName, containedMethods);
            };

            if (!myProgramLock)
            {
                commit();
            }
            else
            {
                myQueuedCommits.Enqueue(commit);
            }
        }

        //TODO: Implement message processing via attribute-based processors declaration
        private Response ProcessMessage(Request request)
        {
            if (!myProgramLock)
            {
                while (myQueuedCommits.Count > 0)
                {
                    var commit = myQueuedCommits.Dequeue();
                    commit();
                }
            }

            switch (request)
            {
                case UpdateMethodRequest updateMethodMessage:
                    /*
                    myProgramBuilder.Up(
                        updateMethodMessage.Method.VariableId.Value, 
                        updateMethodMessage.Method);
                        */
                    return new SuccessResponse();
                case UpdateFileRequest updateFileMessage:
                    UpdateFile(updateFileMessage);
                    return new SuccessResponse();
                case TerminatingRequest terminatingRequest:
                    myIsProcessing = false;
                    SaveProgram();
                    return new SuccessResponse();
                case PerformAnalysisRequest performAnalysisRequest:
                    if (!myProgramLock)
                    {
                        RunAnalysis(performAnalysisRequest);
                    }

                    return new SuccessResponse();
                case AnalysisResultsRequest analysisResultsRequest:
                    return GetAnalysisResults(analysisResultsRequest);
                default:
                    return new FailureResponse();
            }
        }

        private void SaveProgram()
        {
            var state = myProgramBuilder.DumpState();

            var serializer = new DataContractJsonSerializer(
                typeof(GraphStructuredProgramBuilderState),
                new []{typeof(InternalStatement), typeof(ParameterIndicesSet)});

            var directory = Path.GetDirectoryName(myDatabasePath);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
            }

            using (var file = new FileStream(myDatabasePath, FileMode.Create))
            {
                using (var archive = new ZipArchive(file, ZipArchiveMode.Create))
                {
                    var entry = archive.CreateEntry("solution.db");
                    using (var stream = entry.Open())
                    {
                        serializer.WriteObject(stream, state);
                    }
                }
            }
        }

        private void RestoreProgram()
        {
            var serializer = new DataContractJsonSerializer(
                typeof(GraphStructuredProgramBuilderState),
                new []{typeof(InternalStatement), typeof(ParameterIndicesSet)});

            var directory = Path.GetDirectoryName(myDatabasePath);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
            }

            try
            {
                using (ZipArchive zip = ZipFile.OpenRead(myDatabasePath))
                {
                    ZipArchiveEntry e = zip.GetEntry("solution.db");
                    var state = (GraphStructuredProgramBuilderState) serializer.ReadObject(e.Open());
                    myProgramBuilder = new GraphStructuredProgramBuilder(state);
                }
            }
            catch (FileNotFoundException exception)
            {
                Logging.Log("Database not found");
            }
            //Logging.Log("Database load is disabled");
        }
    }
}
