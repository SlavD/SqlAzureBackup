using System;
using CommandLine;
using CommandLine.Text;
using SqlAzureBackup.Enums;

namespace SqlAzureBackup
{
    public class Options : CommandLineOptionsBase
    {
        [Option("u", "user-name", HelpText = "Sql Azure user name", Required = true)]
        public string UserName { get; set; }

        [Option("p", "password", HelpText = "Sql Azure user password", Required = true)]
        public string Password { get; set; }

        [Option("d", "data-center", HelpText = "Sql Azure DataCenter", Required = true)]
        public SqlAzureDatacenter Datacenter { get; set; }

        [Option("n", "db-name", HelpText = "Sql Azure dtabase name", Required = true)]
        public string DatabaseName { get; set; }

        [Option("s", "server-name", HelpText = "Sql Azure database server name", Required = true)]
        public string ServerName { get; set; }

        [Option("k", "storage-key", HelpText = "Azure table storage key", Required = true)]
        public string StorageKey { get; set; }

        [Option("a", "storage-account", HelpText = "Azure table storage account name", Required = true)]
        public string StorageAccount { get; set; }

        [Option("c", "export-container", HelpText = "Export container name", Required = true)]
        public string ExportContainerName { get; set; }

        [Option("f", "export-file-name", HelpText = "Export file name", Required = true)]
        public string ExportFileName { get; set; }

        [Option("t", "append-timestamp", DefaultValue = true, HelpText = "Append timestamp to export file name", Required = true)]
        public bool AppendTimestamp { get; set; }

        public string DataCenterEndpointUri {
            get
            {
                switch (Datacenter)
                {
                    case SqlAzureDatacenter.NorthCentralUS:
                        return "https://ch1prod-dacsvc.azure.com/DACWebService.svc";
                    case SqlAzureDatacenter.SouthCentralUS:
                        return "https://sn1prod-dacsvc.azure.com/DACWebService.svc";
                    case SqlAzureDatacenter.EastUS:
                        return "https://bl2prod-dacsvc.azure.com/DACWebService.svc";
                    case SqlAzureDatacenter.WestUS:
                        return "https://by1prod-dacsvc.azure.com/DACWebService.svc";
                    case SqlAzureDatacenter.NorthEU:
                        return "https://db3prod-dacsvc.azure.com/DACWebService.svc";
                    case SqlAzureDatacenter.WestEU:
                        return "https://am1prod-dacsvc.azure.com/DACWebService.svc";
                    case SqlAzureDatacenter.EastAsia:
                        return "https://hkgprod-dacsvc.azure.com/DACWebService.svc";
                    case SqlAzureDatacenter.SoutheastAsia:
                        return "https://sg1prod-dacsvc.azure.com/DACWebService.svc";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}