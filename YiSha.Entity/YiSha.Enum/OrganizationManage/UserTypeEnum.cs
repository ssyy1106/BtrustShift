using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiSha.Enum.OrganizationManage
{
    public enum UserTypeEnum
    {
        [Description("学生20小时")]
        A20 = 0,
        [Description("A30小时")]
        A30 = 1,
        [Description("A36小时")]
        A36 = 2,
        [Description("A40小时")]
        A40 = 3,
        [Description("A44小时")]
        A44 = 4,
        [Description("B")]
        B = 5,
        [Description("C")]
        C = 6,
        [Description("D")]
        D = 7
    }
}
