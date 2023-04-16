using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Borsdata.Api.Dal.Model
{
    public partial class ReportsR12RespV1
    {
        public long? Instrument { get; set; }
        public List<ReportR12V1> Reports { get; set; }
    }
}
