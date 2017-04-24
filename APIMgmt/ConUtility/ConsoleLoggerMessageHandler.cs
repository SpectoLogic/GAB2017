using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Cache;
using System.Net.Http.Headers;

namespace ConUtility
{
    /// <summary>
    /// DelegationHandler to show infos about the request in the Console.
    /// </summary>
    /// <Author>Andreas Pollak, SpectoLogic</Author>
    public class ConsoleLoggerMessageHandler : DelegatingHandler
    {
        public ConsoleLoggerMessageHandler(WebRequestHandler clientRequest) : base(clientRequest)
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(request.Method + " ");
            Console.WriteLine(request.RequestUri.ToString());
            Console.WriteLine("Headers:");
            foreach (var header in request.Headers)
                Console.WriteLine(String.Format("{0} : {1}", header.Key, String.Join(",", header.Value.ToArray())));

            var task = base.SendAsync(request, cancellationToken).ContinueWith<HttpResponseMessage>(
                requestTask =>
                {
                    #region xx
                    if (requestTask.IsCanceled)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Request was canceled");
                    }
                    if (requestTask.IsFaulted)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Request faulted");
                    }
                    if (requestTask.IsCompleted)
                    {
                        var response = requestTask.Result;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n => \n");
                        if (!(((int)response.StatusCode >= 200) && ((int)response.StatusCode <= 400)))
                            Console.ForegroundColor = ConsoleColor.Red;

                        Console.WriteLine(((int)response.StatusCode).ToString() + " " + response.StatusCode.ToString("F"));
                        foreach (var rh in response.Headers)
                            Console.WriteLine(String.Format("{0} : {1}", rh.Key, String.Join(",", rh.Value.ToArray())));
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("\n=============================================================\n");
                    }
                    return requestTask.Result;
                    #endregion
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

            return task;
        }
    }
}
