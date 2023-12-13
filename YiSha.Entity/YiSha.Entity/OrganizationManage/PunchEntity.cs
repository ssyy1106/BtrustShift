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
    }
}
