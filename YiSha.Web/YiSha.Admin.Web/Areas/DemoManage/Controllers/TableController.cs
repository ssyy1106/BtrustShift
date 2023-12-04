using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using YiSha.Entity.OrganizationManage;
using YiSha.Model.Param.OrganizationManage;
using YiSha.Util;
using YiSha.Util.Model;

namespace YiSha.Admin.Web.Areas.DemoManage.Controllers
{
    [Area("DemoManage")]
    public class TableController : Controller
    {
        #region 视图功能
        public IActionResult Editable()
        {
            return View();
        }

        public IActionResult Image()
        {
            return View();
        }

        public IActionResult Footer()
        {
            return View();
        }

        public IActionResult GroupHeader()
        {
            return View();
        }

        public IActionResult MultiToolbar()
        {
            return View();
        }
        #endregion

        #region 获取数据
        [HttpGet]
        public async Task<IActionResult> GetPageListJson(UserListParam param, Pagination pagination)
        {
            // 测试总共23条数据
            int total = 23;
            TData<List<ShiftDetailEntity>> obj = new TData<List<ShiftDetailEntity>>();
            obj.Total = total;
            obj.Data = new List<ShiftDetailEntity>();
            int id = (pagination.PageIndex - 1) * pagination.PageSize + 1;
            for (int i = id; i <= pagination.PageIndex * pagination.PageSize; i++)
            {
                if (i > total)
                {
                    break;
                }
                obj.Data.Add(new ShiftDetailEntity
                {
                    Id = i,
                    UserId = 100 + i,
                    //Mobile = "15612345678",
                    // Birthday = DateTime.Now.ToString("t"),
                    //UserStatus = i % 2,
                    //Hours = 0,
                    MondayBegin = "00:00",
                    MondayEnd = "00:00",
                    TuesdayBegin = "00:00",
                    TuesdayEnd = "00:00",
                    WednesdayBegin = "00:00",
                    WednesdayEnd = "00:00",
                    ThursdayBegin = "00:00",
                    ThursdayEnd = "00:00",
                    FridayBegin = "00:00",
                    FridayEnd = "00:00",
                    SaturdayBegin = "00:00",
                    SaturdayEnd = "00:00",
                    SundayBegin = "00:00",
                    SundayEnd = "00:00",
                });
            }
            obj.Tag = 1;
            return Json(obj);
        }
        #endregion
    }
}