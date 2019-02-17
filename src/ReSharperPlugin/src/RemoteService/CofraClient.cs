using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml;
using Cofra.Contracts.Messages;
using Cofra.Contracts.Messages.Requests;
using Cofra.Contracts.Messages.Responses;
using JetBrains.Util;

namespace Cofra.ReSharperPlugin.RemoteService
{
    public class CofraClient
    {
        private readonly Stream myConnection;

        private readonly ConcurrentQueue<(Request, Action<Response>)> myMessageQueue;
        private readonly object myQueueNotifier = new object();

        private bool myIsProcessing;

        public CofraClient(Stream connection)
        {
            myConnection = connection;

            myMessageQueue = new ConcurrentQueue<(Request, Action<Response>)>();
        }

        public void EnqueueRequest(Request request, Action<Response> responseProcessor)
        {
            myMessageQueue.Enqueue((request, responseProcessor));

            //TODO: Interruptions
            /*
            if (myCurrentTaskIsInterruptable)
            {
              lock (myProcessingLock)
              {
                if (myCurrentTaskIsInterruptable && !myCurrentTaskInterrupted)
                {
                  myCurrentTaskInterrupted = true;
                  Monitor.Pulse(myProcessingLock);
                }
              }
            }
            */

            lock (myQueueNotifier)
            {
                Monitor.PulseAll(myQueueNotifier);
            }
        }

        public void Start()
        {
            var reader = new StreamReader(myConnection);

            var requestsSerializer = new DataContractSerializer(typeof(Request));
            var responsesSerializer = new DataContractSerializer(typeof(Response));

            myIsProcessing = true;
            while (myIsProcessing || !myMessageQueue.IsEmpty)
            {
                var request = GetNextRequest(myIsProcessing, out var processor);

                if (request == null) continue;

                requestsSerializer.WriteObject(myConnection, request);
                myConnection.Write(new[] {(byte) '\n'}, 0, 1);
                myConnection.Flush();

                var rawResponse = reader.ReadLine().NotNull();

                using (var stringReader = new StringReader(rawResponse))
                using (var xmlReader = XmlReader.Create(stringReader))
                {
                    var response = (Response) responsesSerializer.ReadObject(xmlReader);
                    processor(response);
                }
            }
        }

        public void Stop()
        {
            EnqueueRequest(new TerminatingRequest(), _ => { });

            myIsProcessing = false;
        }

        private Request GetNextRequest(bool waitMessage, out Action<Response> processor)
        {
            var exists = myMessageQueue.TryDequeue(out var message);
            if (!exists)
                lock (myQueueNotifier)
                {
                    exists = myMessageQueue.TryDequeue(out message);
                    if (!exists && waitMessage)
                    {
                        Monitor.Wait(myQueueNotifier);
                        myMessageQueue.TryDequeue(out message);
                    }
                }

            processor = message.Item2;
            return message.Item1;
        }
    }
}