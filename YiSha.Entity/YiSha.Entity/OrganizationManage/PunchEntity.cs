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
    [Table("SysPunch")]
    public class PunchEntity : BaseEntity
    {
        public string BtrustId { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public string Day { get; set; }
        public string Hour { get; set; }
        public string Minute { get; set; }
        public string Second { get; set; }
        public string SiteNumber { get; set; }

        //[NotMapped]
        //public string DepartmentName { get; set; }

        //[NotMapped]
        //public string RealName { get; set; }
    }

    public class PunchAllEntity
    {
        [JsonConverter(typeof(StringJsonConverter))]
        public long? UserId { get; set; }
        public string BtrustId { get; set; }
        public string RealName { get; set; }
        public string DepartmentName { get; set; }
        public string SiteNumber { get; set; }
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
        public string MondayBeginPunch { get; set; }
        public string MondayEndPunch { get; set; }
        public string TuesdayBeginPunch { get; set; }
        public string TuesdayEndPunch { get; set; }
        public string WednesdayBeginPunch { get; set; }
        public string WednesdayEndPunch { get; set; }
        public string ThursdayBeginPunch { get; set; }
        public string ThursdayEndPunch { get; set; }
        public string FridayBeginPunch { get; set; }
        public string FridayEndPunch { get; set; }
        public string SaturdayBeginPunch { get; set; }
        public string SaturdayEndPunch { get; set; }
        public string SundayBeginPunch { get; set; }
        public string SundayEndPunch { get; set; }
        public int MondayBeginPunchStatus { get; set; }
        public int MondayEndPunchStatus { get; set; }
        public int TuesdayBeginPunchStatus { get; set; }
        public int TuesdayEndPunchStatus { get; set; }
        public int WednesdayBeginPunchStatus { get; set; }
        public int WednesdayEndPunchStatus { get; set; }
        public int ThursdayBeginPunchStatus { get; set; }
        public int ThursdayEndPunchStatus { get; set; }
        public int FridayBeginPunchStatus { get; set; }
        public int FridayEndPunchStatus { get; set; }
        public int SaturdayBeginPunchStatus { get; set; }
        public int SaturdayEndPunchStatus { get; set; }
        public int SundayBeginPunchStatus { get; set; }
        public int SundayEndPunchStatus { get; set; }
        [JsonConverter(typeof(StringJsonConverter))]
        public long? MondayPunchProblemId { get; set; }
        [JsonConverter(typeof(StringJsonConverter))]
        public long? TuesdayPunchProblemId { get; set; }
        [JsonConverter(typeof(StringJsonConverter))]
        public long? WednesdayPunchProblemId { get; set; }
        [JsonConverter(typeof(StringJsonConverter))]
        public long? ThursdayPunchProblemId { get; set; }
        [JsonConverter(typeof(StringJsonConverter))]
        public long? FridayPunchProblemId { get; set; }
        [JsonConverter(typeof(StringJsonConverter))]
        public long? SaturdayPunchProblemId { get; set; }
        [JsonConverter(typeof(StringJsonConverter))]
        public long? SundayPunchProblemId { get; set; }
    }
    [Table("SysPunchProblem")]
    public class PunchProblemEntity : BaseExtensionEntity
    {
        public PunchProblemEntity()
        {
            this.Punches = new();
            //Punches.Add("abc");
        }
        [JsonConverter(typeof(StringJsonConverter))]
        public long? UserId { get; set; }
        public string BtrustId { get; set; }
        public string PunchDate { get; set; }
        public int DailyReason { get; set; }
        public string DailyMessage { get; set; }
        public int BeginReason { get; set; }
        public string BeginMessage { get; set; }
        public int EndReason { get; set; }
        public string EndMessage { get; set; }
        public int Status { get; set; }
        public string Remark { get; set; }
        //[NotMapped]
        //public string DepartmentName { get; set; }

        [NotMapped]
        public string RealName { get; set; }
        [NotMapped]
        public string BeginTime { get; set; }
        [NotMapped]
        public string EndTime { get; set; }
        [NotMapped]
        public string FirstPunchTime { get; set; }
        [NotMapped]
        public string LastPunchTime { get; set; }
        [NotMapped]
        public List<string> Punches { get; set; }
    }
}
