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
        class CheckResult
        {
            public bool ok { get; set; }
            public List<string> message { get; set; }

            public CheckResult()
            {
                message = new();
                ok = true;
            }
        }
        private ShiftService shiftService = new ShiftService();
        private DepartmentBLL departmentBLL = new DepartmentBLL();
        private UserBLL userBLL = new UserBLL();
        private LeaveService leaveService = new LeaveService();
        #region 视图功能
        [AuthorizeFilter("organization:shift:view")]
        public IActionResult ShiftIndex()
        {
            return View();
        }
        public IActionResult ExamineShift()
        {
            return View();
        }
        #endregion

        #region 获取数据
        [HttpGet]
        [AuthorizeFilter("organization:shift:view")]
        public async Task<IActionResult> GetPageListJson(ShiftParam param)
        {
            TData<List<ShiftAllDetailEntity>> obj = new TData<List<ShiftAllDetailEntity>>();
            obj.Tag = 1;
            obj.Data = await GetListJson(param);
            return Json(obj);
        }

        [HttpGet]
        [AuthorizeFilter("organization:shift:examine")]
        public async Task<IActionResult> ExamineShiftJson(string periodBegin)
        {
            TData<List<string>> obj = new()
            {
                Tag = 1
            };
            List<string> result = new();
            // 读取所有人的排班记录
            ShiftParam param = new() { PeriodBegin = periodBegin };
            List<ShiftAllDetailEntity> list = await shiftService.GetPageList(param);
            foreach (ShiftAllDetailEntity shift in list)
            {
                CheckResult res = await check(shift, periodBegin);
                if (!res.ok)
                {
                    result.AddRange(res.message);
                    //result.Add("\\n");
                }
            }
            obj.Data = result;

            return Json(obj);
        }
        #endregion

        #region 提交数据
        [HttpPost]
        [AuthorizeFilter("organization:shift:submit")]
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

        #region 私有函数
        private bool checkLeave(string fromDay, string toDay, ShiftAllDetailEntity shift, string startTime ="00:00", string endTime="23:59")
        {
            string[] days = {
                "Monday",
                "Tuesday",
                "Wednesday",
                "Thursday",
                "Friday",
                "Saturday",
                "Sunday"
            };
            return true;
        }
        private async Task<CheckResult> check(ShiftAllDetailEntity shift, string periodBegin)
        {
            CheckResult res = new();
            // 读取人员基本信息
            TData<UserEntity> obj = await userBLL.GetEntity((long)shift.UserId);
            if (obj.Tag == 1)
            {
                UserEntity user = obj.Data;
                // 检查人员type和category是否符合排班
                int? type = user.Type;
                int? category = user.Category;
                int totalHours = shift.TotalHours;
                if (category == 0 && totalHours > 44)
                {
                    string message = $"User {shift.RealName} with id {shift.BtrustId} has a category Hourly, but work over 44 hours, total hours is {totalHours} hours.";
                    res.message.Add(message);
                    res.ok = false;
                } else if (category == 1 && totalHours > 54 && shift.DepartmentName.Length >= 7
                    && shift.DepartmentName.Substring(shift.DepartmentName.Length - 7, 7) == "Kitchen")
                {
                    string message = $"User {shift.RealName} with id {shift.BtrustId} has a category Weekly and working in Kitchen, but work over 54 hours, total hours is {totalHours} hours.";
                    res.message.Add(message);
                    res.ok = false;
                }
                else if (category == 1 && totalHours > 66)
                {
                    string message = $"User {shift.RealName} with id {shift.BtrustId} has a category Weekly, but work over 66 hours, total hours is {totalHours} hours.";
                    res.message.Add(message);
                    res.ok = false;
                }
            }
            // 检查请假信息和排班是否吻合
            // 读取该用户的请假记录
            List<LeaveAllEntity> leaves = await leaveService.GetListByUserId(shift.UserId, periodBegin);
            foreach (LeaveAllEntity leave in leaves)
            {
                if (leave != null)
                {
                    if (leave.LeaveType == 0)
                    {
                        string fromDay = leave.FromDay;
                        string toDay = leave.ToDay;
                        if (!checkLeave(fromDay, toDay, shift))
                        {
                            string message = $"User {shift.RealName} with id {shift.BtrustId} has a leave from {fromDay} to {toDay}, cannot be set a shift.";
                            res.message.Add(message);
                            res.ok = false;
                        }
                    }
                    else
                    {
                        string fromDay = leave.FromDay;
                        string toDay = leave.ToDay;
                        string startTime = leave.StartTime;
                        string endTime = leave.EndTime;
                        if (!checkLeave(fromDay, toDay, shift, startTime, endTime))
                        {
                            string message = $"User {shift.RealName} with id {shift.BtrustId} has a leave from {fromDay} to {toDay} from {startTime} to {endTime}, cannot be set a shift.";
                            res.message.Add(message);
                            res.ok = false;
                        }
                    }
                }
            }
            return res;
        }
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
        #endregion
    }
}