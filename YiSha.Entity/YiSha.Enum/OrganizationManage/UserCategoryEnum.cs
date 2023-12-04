using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiSha.Enum.OrganizationManage
{
    public enum UserCategoryEnum
    {
        [Description("时薪")]
        Hourly = 0,
        [Description("周薪")]
        Weekly = 1,
        [Description("年薪")]
        Salary = 2
    }
}