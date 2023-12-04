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
    [Table("SysShift")]
    public class ShiftEntity : BaseExtensionEntity
    {
        [JsonConverter(typeof(StringJsonConverter))]
        public long? DepartmentId { get; set; }

        [JsonIgnore]
        public int IsSubmit { get; set; }

        public string PeriodBegin { get; set; }
        public string PeriodEnd { get; set; }

        public int TotalHours { get; set; }
        public int RegularHours { get; set; }
        public int BreakHours { get; set; }

        [JsonConverter(typeof(StringJsonConverter))]
        public long? ApproverId { get; set; }

        [JsonConverter(typeof(DateTimeJsonConverter))]
        public DateTime? ApprovalTime { get; set; }

        [JsonIgnore]
        public int IsApproval { get; set; }
    }

    [Table("SysShiftDetail")]
    public class ShiftDetailEntity : BaseExtensionEntity
    {
        [JsonConverter(typeof(StringJsonConverter))]
        public long? ShiftId { get; set; }

        [JsonConverter(typeof(StringJsonConverter))]
        public long? UserId { get; set; }

        // public string RealName { get; set; }
        public int TotalHours { get; set; }
        public int RegularHours { get; set; }
        public int BreakHours { get; set; }

        public string Remark { get; set; }

        public string MondayBegin { get; set; }
        public string MondayEnd { get; set; }
        public string TuesdayBegin { get; set; }
        public string TuesdayEnd { get; set; }
        public string WednesdayBegin { get; set; }
        public string WednesdayEnd { get; set; }
        public string ThursdayBegin { get; set; }
        public string ThursdayEnd { get; set; }
        public string FridayBegin { get; set; }
        public string FridayEnd { get; set; }
        public string SaturdayBegin { get; set; }
        public string SaturdayEnd { get; set; }
        public string SundayBegin { get; set; }
        public string SundayEnd { get; set; }
    }

    public class ShiftAllDetailEntity: BaseEntity
    {
        [JsonConverter(typeof(StringJsonConverter))]
        public long? ShiftId { get; set; }

        [JsonConverter(typeof(StringJsonConverter))]
        public long? UserId { get; set; }

        [JsonConverter(typeof(StringJsonConverter))]
        public long? DepartmentId { get; set; }

        public string Remark { get; set; }
        public string RealName { get; set; }
        public string DepartmentName { get; set; }

        public string BtrustId { get; set; }
        public int TotalHours { get; set; }
        public int RegularHours { get; set; }
        public int BreakHours { get; set; }

        public string MondayBegin { get; set; }
        public string MondayEnd { get; set; }
        public string TuesdayBegin { get; set; }
        public string TuesdayEnd { get; set; }
        public string WednesdayBegin { get; set; }
        public string WednesdayEnd { get; set; }
        public string ThursdayBegin { get; set; }
        public string ThursdayEnd { get; set; }
        public string FridayBegin { get; set; }
        public string FridayEnd { get; set; }
        public string SaturdayBegin { get; set; }
        public string SaturdayEnd { get; set; }
        public string SundayBegin { get; set; }
        public string SundayEnd { get; set; }
    }

    public class ShiftAllEntity
    {
        public ShiftAllDetailEntity[] Shifts { get; set; }

        [JsonConverter(typeof(StringJsonConverter))]
        public long? DepartmentId { get; set; }

        public string PeriodBegin { get; set; }
        public string PeriodEnd { get; set; }

        public int TotalHours { get; set; }
        public int RegularHours { get; set; }
        public int BreakHours { get; set; }

        //[JsonConverter(typeof(StringJsonConverter))]
        //public long? ShiftId { get; set; }
    }
}
