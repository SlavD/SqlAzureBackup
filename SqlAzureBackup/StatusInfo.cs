using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlAzureBackup.Enums;

namespace SqlAzureBackup.SqlAzure.ImportExport
{
    public partial class StatusInfo
    {
        public ExportRequestStatus ExportRequestStatus
        {
            get
            {
                switch (Status)
                {
                    case "Failed":
                        return ExportRequestStatus.Failed;
                    case "Completed":
                        return ExportRequestStatus.Complete;
                    case "Pending":
                        return ExportRequestStatus.Running;
                    default:
                        return ExportRequestStatus.Unknown;
                }
            }
        }
    }
}
