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
    public class ShiftController : Controller
    {
        private ShiftService shiftService = new ShiftService();
        private DepartmentBLL departmentBLL = new DepartmentBLL();
        #region 视图功能
        public IActionResult ShiftIndex()
        {
            return View();
        }
        #endregion

        #region 获取数据
        [HttpGet]
        public async Task<IActionResult> GetPageListJson(ShiftParam param)
        {
            TData<List<ShiftAllDetailEntity>> obj = new TData<List<ShiftAllDetailEntity>>();
            obj.Tag = 1;
            obj.Data = await GetListJson(param);
            return Json(obj);
        }
        #endregion

        #region 提交数据
        [HttpPost]
        public async Task<IActionResult> SubmitFormJson(ShiftAllEntity entity)
        {
            await shiftService.SaveForm(entity);

            TData<List<ShiftDetailEntity>> obj = new TData<List<ShiftDetailEntity>>();
            obj.Tag = 1;
            return Json(obj);
        }

        [HttpPost]
        [AuthorizeFilter("organization:shift:export")]
        public async Task<IActionResult> ExportShiftJson(ShiftParam param)
        {
            TData<string> obj = new TData<string>();

            List<ShiftAllDetailEntity> shifts = await GetListJson(param);
            string file = new ExcelHelper<ShiftAllDetailEntity>().ExportToExcel("排班列表.xls",
                       "排班列表" + param.PeriodBegin,
                       shifts,
                       new string[]
                       { "BtrustId", "RealName", "DepartmentName", "TotalHours", "RegularHours", "BreakHours",
                         "MondayBegin", "MondayEnd", "TuesdayBegin", "TuesdayEnd", "WednesdayBegin",
                         "WednesdayEnd", "ThursdayBegin", "ThursdayEnd", "FridayBegin", "FridayEnd",
                         "SaturdayBegin", "SaturdayEnd", "SundayBegin", "SundayEnd"
                       });
            obj.Data = file;
            obj.Tag = 1;

            return Json(obj);
        }
        #endregion
        private async Task<List<ShiftAllDetailEntity>> GetListJson(ShiftParam param)
        {
            List<ShiftAllDetailEntity> res = new List<ShiftAllDetailEntity>();
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
                ShiftParam p = new();
                p.PeriodBegin = param.PeriodBegin;
                p.DepartmentId = departmentId;
                List<ShiftAllDetailEntity> list = await shiftService.GetPageList(p);
                try
                {
                    res = res.Union(list).ToList();
                }
                catch
                {

                }
            }
            res = res.OrderBy(o => o.DepartmentId).ToList();
            return res;
        }
    }
}