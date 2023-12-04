using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YiSha.Util;

namespace YiSha.Entity.OrganizationManage
{
    [Table("SysLeave")]
    public class LeaveEntity : BaseExtensionEntity
    {
        [JsonConverter(typeof(StringJsonConverter))]
        public long UserId { get; set; }

        public int LeaveType { get; set; }

        public int LeaveKind { get; set; }
        public string FromDay { get; set; }

        public string ToDay { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Remark { get; set; }
    }

    public class LeaveAllEntity
    {
        [JsonConverter(typeof(StringJsonConverter))]
        public long Id { get; set; }
        [JsonConverter(typeof(StringJsonConverter))]
        public string UserName { get; set; }
        public string RealName { get; set; }
        public string DepartmentName { get; set; }
        public string LeaveType { get; set; }

        public string LeaveKind { get; set; }
        public string FromDay { get; set; }

        public string ToDay { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Remark { get; set; }
    }
}
