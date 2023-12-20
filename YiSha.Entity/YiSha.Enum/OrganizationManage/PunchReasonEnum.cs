using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiSha.Enum.OrganizationManage
{
    public enum PunchReasonEnum
    {
        [Description("Attendence")]
        ATT = 0,
        [Description("Late")]
        LT = 1,
        [Description("Leave Early")]
        LE = 2,
        [Description("Over Time")]
        OT = 3,
        [Description("Missing Punch")]
        MP = 4,
        [Description("Leave")]
        LV = 5,
        [Description("Sick Leave")]
        SL = 6,
        [Description("Vacation")]
        VAC = 7,
        [Description("Absent")]
        AB = 8,
        [Description("Schedule")]
        SCH = 9,
        [Description("In early")]
        IE = 10,
        [Description("In late")]
        IL = 11,
        [Description("Out Early")]
        OE = 12,
        [Description("Out late")]
        OL = 13,
        [Description("LIEU DAY")]
        LD = 14,
        [Description("Schedule work day")]
        SWD = 15
    }
}