using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Web;
using System.Xml;
using CommandLine;
using SqlAzureBackup.Enums;
using SqlAzureBackup.SqlAzure.ImportExport;

namespace SqlAzureBackup
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (args == null || args.Length == 0 || !CommandLineParser.Default.ParseArguments(args, options))
            {
                PrintUsage(options);
                return;
            }

            var blobUri = String.Format(@"https://{0}.blob.core.windows.net/{1}/{2}-{3}.bacpac",
                                        options.StorageAccount, 
                                        options.ExportContainerName,
                                        options.ExportFileName,
                                        options.AppendTimestamp
                                            ? DateTime.UtcNow.ToString("yyyyMMddHHmmssffff")
                                            : string.Empty);


            Export(blobUri, options.DataCenterEndpointUri, options.StorageKey, options.DatabaseName, options.ServerName, options.UserName, options.Password);


        }

        private static void PrintUsage(Options options)
        {
            Console.WriteLine("Invalid Arguments!");
            Console.WriteLine(options.GetUsage());
        }

        private static string Export(string blobUri, string endpointUri, string storageKey, string databaseName, string serverName, string userName, string password)
        {
            Console.Write("Starting Export of the database: {0} at {1}\n\r", databaseName, DateTime.Now);
            bool exportComplete = false;
            string exportedBlobPath = null;

            WebRequest webRequest = WebRequest.Create(endpointUri + @"/Export");
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.ContentType = @"application/xml";

            var exportInputs = new ExportInput
            {
                BlobCredentials = new BlobStorageAccessKeyCredentials
                {
                    StorageAccessKey = storageKey,
                    Uri = blobUri
                },
                ConnectionInfo = new ConnectionInfo
                {
                    ServerName = string.Format("{0}.database.windows.net", serverName),
                    DatabaseName = databaseName,
                    UserName = userName,
                    Password = password
                }
            };

            using (var webRequestStream = webRequest.GetRequestStream())
            {
                var dataContractSerializer = new DataContractSerializer(exportInputs.GetType());
                dataContractSerializer.WriteObject(webRequestStream, exportInputs);
                webRequestStream.Close();

                Console.WriteLine("Extracting export request id...");

                try
                {
                    var webResponse = webRequest.GetResponse();
                    var xmlStreamReader = XmlReader.Create(webResponse.GetResponseStream());

                    xmlStreamReader.ReadToFollowing("guid");
                    string requestGuid = xmlStreamReader.ReadElementContentAsString();
                    Console.WriteLine("Export id is: {0}", requestGuid);

                    Console.Write("Export in progress: ");

                    while (!exportComplete)
                    {
                        System.Threading.Thread.Sleep(1000);

                        var statusMessage = CheckRequestStatus(requestGuid, endpointUri, exportInputs.ConnectionInfo.ServerName, userName, password).FirstOrDefault();
                        if (statusMessage != null)
                        {
                            switch (statusMessage.ExportRequestStatus)
                            {
                                case ExportRequestStatus.Unknown:
                                    break;
                                case ExportRequestStatus.Running:
                                    Console.Write(".");
                                    break;
                                case ExportRequestStatus.Complete:
                                    exportedBlobPath = statusMessage.BlobUri;
                                    Console.WriteLine("\r\nExport Complete - Database exported to: {0}\n\r", exportedBlobPath);
                                    exportComplete = true;
                                    break;
                                case ExportRequestStatus.Failed:
                                    Console.WriteLine("\r\nDatabase export failed: {0}", statusMessage.ErrorMessage);
                                    exportComplete = true;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }
                    return exportedBlobPath;
                }
                catch (WebException responseException)
                {
                    Console.WriteLine("Request Falied:{0}", responseException.Message);
                    if (responseException.Response != null)
                    {
                        Console.WriteLine("Status Code: {0}", ((HttpWebResponse)responseException.Response).StatusCode);
                        Console.WriteLine("Status Description: {0}\n\r", ((HttpWebResponse)responseException.Response).StatusDescription);
                    }
                    return null;
                }
            }
        }



        private static IEnumerable<StatusInfo> CheckRequestStatus(string requestGuid, string endpointUri, string serverName, string userName, string password)
        {
            WebRequest webRequest = WebRequest.Create(endpointUri + string.Format("/Status?servername={0}&username={1}&password={2}&reqId={3}",
                    HttpUtility.UrlEncode(serverName),
                    HttpUtility.UrlEncode(userName),
                    HttpUtility.UrlEncode(password),
                    HttpUtility.UrlEncode(requestGuid)));

            webRequest.Method = WebRequestMethods.Http.Get;
            webRequest.ContentType = @"application/xml";
            WebResponse webResponse = webRequest.GetResponse();
            XmlReader xmlStreamReader = XmlReader.Create(webResponse.GetResponseStream());
            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(List<StatusInfo>));

            return (List<StatusInfo>)dataContractSerializer.ReadObject(xmlStreamReader, true);
        }
    }
}
