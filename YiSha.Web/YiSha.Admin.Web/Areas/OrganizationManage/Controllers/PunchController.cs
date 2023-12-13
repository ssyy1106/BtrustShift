using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SixLabors.Fonts.Tables.AdvancedTypographic;
using YiSha.Business.OrganizationManage;
using YiSha.Entity.OrganizationManage;
using YiSha.Model.Param.OrganizationManage;
using YiSha.Service.OrganizationManage;
using YiSha.Util;
using YiSha.Util.Model;
using YiSha.Cache.Factory;
using YiSha.Web.Code;
using YiSha.Admin.Web.Controllers;

namespace YiSha.Admin.Web.Areas.OrganizationManage.Controllers
{
    [Area("OrganizationManage")]
    public class PunchController : Controller
    {
        // private ShiftService shiftService = new ShiftService();
        private DepartmentBLL departmentBLL = new DepartmentBLL();
        private UserBLL userBLL = new UserBLL();
        private PunchService punchService = new PunchService();

        #region 视图功能
        [AuthorizeFilter("organization:punch:view")]
        public IActionResult PunchIndex()
        {
            return View();
        }

        #endregion

        #region 获取数据
        [HttpGet]
        [AuthorizeFilter("organization:punch:view")]
        public async Task<IActionResult> GetPageListJson(PunchParam param)
        {
            TData<List<PunchAllEntity>> obj = new TData<List<PunchAllEntity>>();
            obj.Tag = 1;
            obj.Data = await GetListJson(param);
            return Json(obj);
        }
        #endregion

        #region 私有函数
        private async Task<List<PunchAllEntity>> GetListJson(PunchParam param)
        {
            List<PunchAllEntity> res = new List<PunchAllEntity>();
            // if param.departmentId is not null, not use user's departmentId
            if (param.DepartmentId is null)
            {
                OperatorInfo user = await Operator.Instance.Current();
                if (user.DepartmentName == "HR")
                {
                    param.DepartmentId = 0;
                }
                else
                {
                    param.DepartmentId = user.DepartmentId;
                }
            }

            List<long> departmentList = await departmentBLL.GetChildrenDepartmentIdList(null, (long)param.DepartmentId);

            foreach (long departmentId in departmentList)
            {
                PunchParam p = new();
                p.PeriodBegin = param.PeriodBegin;
                p.DepartmentId = departmentId;
                List<PunchAllEntity> list = await punchService.GetPageList(p);
                try
                {
                    res = res.Union(list).ToList();
                }
                catch
                {

                }
            }
            res = res.OrderBy(o => o.DepartmentName).ToList();
            return res;
        }
        #endregion
    }
}