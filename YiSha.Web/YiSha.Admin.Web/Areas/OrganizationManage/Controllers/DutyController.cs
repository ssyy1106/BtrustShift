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
using YiSha.Util.Extension;

namespace YiSha.Admin.Web.Areas.OrganizationManage.Controllers
{
    [Area("OrganizationManage")]
    public class DutyController : Controller
    {
        // private ShiftService shiftService = new ShiftService();
        private DepartmentBLL departmentBLL = new DepartmentBLL();
        private UserBLL userBLL = new UserBLL();
        //private PunchService punchService = new PunchService();

        #region 视图功能
        [AuthorizeFilter("organization:duty:view")]
        public IActionResult DutyIndex()
        {
            return View();
        }

        #endregion

        #region 获取数据
        [HttpGet]
        [AuthorizeFilter("organization:duty:view")]
        public async Task<IActionResult> GetPageListJson(PunchParam param)
        {
            TData<List<PunchAllEntity>> obj = new TData<List<PunchAllEntity>>();
            obj.Tag = 1;
            obj.Data = await GetListJson(param);
            return Json(obj);
        }

        [HttpGet]
        [AuthorizeFilter("organization:duty:view")]
        public async Task<IActionResult> GetFormJson(long id, long userId, string punchDate)
        {
            TData<PunchProblemEntity> obj = new TData<PunchProblemEntity>();
            obj.Tag = 1;
            //obj.Data = await GetListJson(param);
            //obj.Data = await punchService.GetAllInfo(id, userId, punchDate);
            return Json(obj);
        }
        #endregion

        #region 提交数据
        [HttpPost]
        [AuthorizeFilter("organization:punchreview:add,organization:punchreview:edit")]
        public async Task<IActionResult> SaveFormJson(PunchProblemEntity entity)
        {
            TData<string> obj = new TData<string>();
            obj.Data = entity.Id.ParseToString();
            obj.Tag = 1;
            //await punchService.SaveForm(entity);
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

            //foreach (long departmentId in departmentList)
            //{
            //    PunchParam p = new();
            //    p.PeriodBegin = param.PeriodBegin;
            //    p.DepartmentId = departmentId;
            //    List<PunchAllEntity> list = await punchService.GetPageList(p);
            //    try
            //    {
            //        res = res.Union(list).ToList();
            //    }
            //    catch (Exception ex)
            //    {
            //        LogHelper.Error(ex);
            //    }
            //}
            res = res.OrderBy(o => o.DepartmentName).ToList();
            return res;
        }
        #endregion
    }
}