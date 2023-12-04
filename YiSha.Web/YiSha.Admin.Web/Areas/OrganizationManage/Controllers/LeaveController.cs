using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using YiSha.Admin.Web.Controllers;
using YiSha.Business.OrganizationManage;
using YiSha.Business.SystemManage;
using YiSha.Entity.OrganizationManage;
using YiSha.Model.Param;
using YiSha.Model.Param.OrganizationManage;
using YiSha.Model.Result;
using YiSha.Model.Result.SystemManage;
using YiSha.Service.OrganizationManage;
using YiSha.Util;
using YiSha.Util.Extension;
using YiSha.Util.Model;
using YiSha.Web.Code;

namespace YiSha.Admin.Web.Areas.OrganizationManage.Controllers
{
    [Area("OrganizationManage")]
    public class LeaveController : BaseController
    {
        //private UserBLL userBLL = new UserBLL();
        private LeaveService LeaveService = new();
        private DepartmentBLL departmentBLL = new DepartmentBLL();

        #region 视图功能
        [AuthorizeFilter("organization:leave:view")]
        public IActionResult LeaveIndex()
        {
            return View();
        }

        public IActionResult LeaveForm()
        {
            return View();
        }

        public IActionResult LeaveDetail()
        {
            ViewBag.Ip = NetHelper.Ip;
            return View();
        }

        public IActionResult ChangeLeave()
        {
            return View();
        }
        #endregion

        #region 获取数据
        [HttpGet]
        [AuthorizeFilter("organization:leave:search")]
        public async Task<IActionResult> GetListJson(LeaveListParam param)
        {
            OperatorInfo user = await Operator.Instance.Current();
            if (param.DepartmentId is null)
            {
                param.DepartmentId = user.DepartmentId;
            }

            List<LeaveAllEntity> list = await LeaveService.GetList(param);
            TData<List<LeaveAllEntity>> obj = new TData<List<LeaveAllEntity>>();
            obj.Tag = 1;
            obj.Data = list.ToList();
            return Json(obj);
        }

        [HttpGet]
        [AuthorizeFilter("organization:leave:search")]
        public async Task<IActionResult> GetPageListJson(LeaveListParam param, Pagination pagination)
        {
            TData<List<LeaveAllEntity>> obj = new TData<List<LeaveAllEntity>>();
            obj.Tag = 1;
            obj.Data = await GetList(param, true, pagination);
            //if (param.DepartmentId is null)
            //{
            //    OperatorInfo user = await Operator.Instance.Current();
            //    if (user.DepartmentName == "HR")
            //    {
            //        param.DepartmentId = 0;
            //    }
            //    else
            //    {
            //        param.DepartmentId = user.DepartmentId;
            //    }
            //}
            //List<long> departmentList = await departmentBLL.GetChildrenDepartmentIdList(null, (long)param.DepartmentId);
            //foreach (long departmentId in departmentList)
            //{
            //    LeaveListParam p = new()
            //    {
            //        DepartmentId = departmentId,
            //        FromDay = param.FromDay,
            //        ToDay = param.ToDay,
            //        UserName = param.UserName,
            //        StartTime = param.StartTime,
            //        EndTime = param.EndTime
            //    };
            //    List<LeaveAllEntity> list = await LeaveService.GetPageList(p, pagination);
            //    try
            //    {
            //        obj.Data = obj.Data.Union(list).ToList();
            //    }
            //    catch
            //    {

            //    }
            //}
            //obj.Data = obj.Data.OrderBy(o => o.DepartmentName).ToList();
            return Json(obj);
        }

        [HttpGet]
        [AuthorizeFilter("organization:leave:search")]
        public async Task<IActionResult> GetFormJson(long id)
        {
            try
            {
                LeaveEntity leave = await LeaveService.GetEntity(id);
                TData<LeaveEntity> obj = new()
                {
                    Tag = 1,
                    Data = leave
                };
                return Json(obj);
            } catch
            {

            }
            return null;
        }
        #endregion

        #region 提交数据
        [HttpPost]
        [AuthorizeFilter("organization:leave:add,organization:leave:edit")]
        public async Task<IActionResult> SaveFormJson(LeaveEntity entity)
        {
            await LeaveService.SaveForm(entity);
            TData<LeaveEntity> obj = new()
            {
                Data = entity,
                Tag = 1
            };
            return Json(obj);
            //TData<string> obj = await userBLL.SaveForm(entity);
            //return Json(obj);
        }

        [HttpPost]
        [AuthorizeFilter("organization:leave:delete")]
        public async Task<IActionResult> DeleteFormJson(string ids)
        {
            TData obj = new TData();
            if (string.IsNullOrEmpty(ids))
            {
                obj.Message = "参数不能为空";
                return Json(obj);
            }
            await LeaveService.DeleteForm(ids);

            obj.Tag = 1;
            return Json(obj);
            //TData obj = await userBLL.DeleteForm(ids);
            //
        }

        [HttpPost]
        [AuthorizeFilter("organization:leave:export")]
        public async Task<IActionResult> ExportLeaveJson(LeaveListParam param)
        {
            TData<string> obj = new TData<string>();
            List<LeaveAllEntity> leaves = await GetList(param, false, null);
            //List<LeaveAllEntity> leaves = await LeaveService.GetList(param);
            //TData<List<UserEntity>> userObj = await userBLL.GetList(param);
            string file = new ExcelHelper<LeaveAllEntity>().ExportToExcel("休假列表.xls",
                       "休假列表",
                       leaves,
                       new string[]
                       { "UserName", "RealName", "DepartmentName", "FromDay", "ToDay", "StartTime",
                         "EndTime", "LeaveType", "LeaveKind", "Remark"
                       });
            obj.Data = file;
            obj.Tag = 1;
            return Json(obj);
        }
        #endregion

        private async Task<List<LeaveAllEntity>> GetList(LeaveListParam param, bool page, Pagination pagination)
        {
            List<LeaveAllEntity> res = new List<LeaveAllEntity>();
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
                LeaveListParam p = new()
                {
                    DepartmentId = departmentId,
                    FromDay = param.FromDay,
                    ToDay = param.ToDay,
                    UserName = param.UserName,
                    StartTime = param.StartTime,
                    EndTime = param.EndTime
                };
                List<LeaveAllEntity> list;
                if (page)
                {
                    list = await LeaveService.GetPageList(p, pagination);
                } else
                {
                    list = await LeaveService.GetList(p);
                }
                
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
    }
}