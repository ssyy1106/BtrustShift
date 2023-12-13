using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Data;
using System.Data.Common;
using YiSha.Data.Repository;
using YiSha.Entity.OrganizationManage;
using YiSha.Enum.OrganizationManage;
using YiSha.Model.Param.OrganizationManage;
using YiSha.Util;
using YiSha.Util.Model;
using YiSha.Util.Extension;
using YiSha.Enum;
using YiSha.Entity;
using YiSha.Data.EF;
using YiSha.Service.SystemManage;
using YiSha.Data;
using YiSha.Cache.Factory;
using MathNet.Numerics.Statistics.Mcmc;
using YiSha.Web.Code;
using NPOI.POIFS.FileSystem;
using Org.BouncyCastle.Asn1.Mozilla;

namespace YiSha.Service.OrganizationManage
{
    public class PunchService : RepositoryFactory
    {
        #region 获取数据
        public async Task<List<PunchAllEntity>> GetPageList(PunchParam param)
        {
            //List<ShiftDetailEntity> res = new();
            var strSql = new StringBuilder();
            List<DbParameter> filter = ListFilter(param, strSql);
            List<ShiftAllDetailEntity> listShift = new();
            List<PunchEntity> listPunch = new();
            List<PunchAllEntity> res = new();
            try
            {
                var listShiftEnum = await this.BaseRepository().FindList<ShiftAllDetailEntity>(strSql.ToString(), filter.ToArray());
                listShift = listShiftEnum.ToList();
            }
            catch
            {
                return null;
            }
            strSql = new StringBuilder();
            filter = ListFilterPunch(param, strSql);
            try
            {
                var listPunchEnum = await this.BaseRepository().FindList<PunchEntity>(strSql.ToString(), filter.ToArray());
                listPunch = listPunchEnum.ToList();
                Dictionary<string, SortedList<string, PunchEntity>> dic = new();
                foreach(var punch in listPunch)
                {
                    string punchDate = punch.Year + punch.Month + punch.Day;
                    if (!dic.ContainsKey(punch.BtrustId))
                    {
                        dic[punch.BtrustId] = new SortedList<string, PunchEntity>();

                    }
                    dic[punch.BtrustId].Add(punchDate, punch);

                }
                // calculate 'punch all entity' with shift and punch information
                foreach( var shift in listShift)
                {
                    PunchAllEntity val = new()
                    {
                        BtrustId = shift.BtrustId,
                        RealName = shift.RealName,
                        DepartmentName = shift.DepartmentName,
                        MondayBegin = shift.MondayBegin,
                        MondayEnd = shift.MondayEnd,
                        TuesdayBegin = shift.TuesdayBegin,
                        TuesdayEnd = shift.TuesdayEnd,
                        WednesdayBegin = shift.WednesdayBegin,
                        WednesdayEnd = shift.WednesdayEnd,
                        ThursdayBegin = shift.ThursdayBegin,
                        ThursdayEnd = shift.ThursdayEnd,
                        FridayBegin = shift.FridayBegin,
                        FridayEnd = shift.FridayEnd,
                        SaturdayBegin = shift.SaturdayBegin,
                        SaturdayEnd = shift.SaturdayEnd,
                        SundayBegin = shift.SundayBegin,
                        SundayEnd = shift.SundayEnd,

                    };
                    res.Add(val);
                }
                return res;
            }
            catch
            { 
                return null; 
            }
        }
        #endregion

        #region 私有方法
        private List<DbParameter> ListFilterPunch(PunchParam param, StringBuilder strSql)
        {
            strSql.Append(@"select SysPunch.BtrustId as BtrustId, Year, Month, Day, Hour, Minute, SiteNumber 
                            from SysPunch inner join SysUser on SysUser.BtrustId=SysPunch.BtrustId where 1=1 ");
            if (param != null)
            {
                if (!string.IsNullOrEmpty(param.PeriodBegin))
                {
                    int year = int.Parse(param.PeriodBegin.Substring(0, 4));
                    int month = int.Parse(param.PeriodBegin.Substring(5, 2));
                    int day = int.Parse(param.PeriodBegin.Substring(8, 2));
                    DateTime beginDate = new DateTime(year, month, day);
                    DateTime endDate = beginDate.AddDays(7);
                    strSql.Append(" AND CONVERT(DATETIME, SysPunch.Year + "+ "'-'" + " + SysPunch.Month + "+ "'-'" + " + SysPunch.Day)  <='" 
                        + endDate + "' AND CONVERT(DATETIME, SysPunch.Year + " + "'-'" + " + SysPunch.Month + " + "'-'" + " + SysPunch.Day)  >='"
                        + beginDate + "'");
                }
                if (param.DepartmentId >= 0)
                {
                    strSql.Append(" AND SysUser.DepartmentId = " + param.DepartmentId);
                }
            }
            var parameter = new List<DbParameter>();
            return parameter;
        }

        private List<DbParameter> ListFilter(PunchParam param, StringBuilder strSql)
        {
            strSql.Append(@"select info.Remark as Remark, SysDepartment.DepartmentName as DepartmentName, SysUser.BtrustId as BtrustId, info.TotalHours as TotalHours, 
                            info.RegularHours as RegularHours, info.BreakHours as BreakHours, 
                            info.PeriodBegin as PeriodBegin, ISNULL(info.Id, 0) as Id, SysUser.Id as UserId,
                            RealName, SysUser.DepartmentId as DepartmentId, info.ShiftId as ShiftId
                            , ISNULL(MondayBegin, '00:00') as MondayBegin, ISNULL(MondayEnd, '00:00') as MondayEnd
                            , ISNULL(TuesdayBegin, '00:00') as TuesdayBegin, ISNULL(TuesdayEnd, '00:00') as TuesdayEnd
                            , ISNULL(WednesdayBegin, '00:00') as WednesdayBegin, ISNULL(WednesdayEnd, '00:00') as WednesdayEnd
                            , ISNULL(ThursdayBegin, '00:00') as ThursdayBegin, ISNULL(ThursdayEnd, '00:00') as ThursdayEnd
                            , ISNULL(FridayBegin, '00:00') as FridayBegin, ISNULL(FridayEnd, '00:00') as FridayEnd
                            , ISNULL(SaturdayBegin, '00:00') as SaturdayBegin, ISNULL(SaturdayEnd, '00:00') as SaturdayEnd
                            , ISNULL(SundayBegin, '00:00') as SundayBegin, ISNULL(SundayEnd, '00:00') as SundayEnd
                            from SysUser inner join SysUserBelong on SysUserBelong.UserId=SysUser.Id 
                            inner join SysPosition on SysPosition.Id = SysUserBelong.BelongId
                            inner join SysDepartment on SysDepartment.Id = SysUser.DepartmentId
                            left join
                            (select SysShiftDetail.Remark as Remark, SysShiftDetail.TotalHours as TotalHours, 
                            SysShiftDetail.RegularHours as RegularHours, 
                            SysShiftDetail.BreakHours as BreakHours, 
                            SysShift.PeriodBegin as PeriodBegin, SysShiftDetail.Id as Id, ShiftId, 
                            UserId, MondayBegin, MondayEnd, TuesdayBegin, TuesdayEnd
                            , WednesdayBegin, WednesdayEnd, ThursdayBegin, ThursdayEnd
                            , FridayBegin, FridayEnd, SaturdayBegin, SaturdayEnd
                            , SundayBegin, SundayEnd from SysShift 
                            inner join SysShiftDetail on SysShift.Id = SysShiftDetail.ShiftId where 1=1 ");
            if (param != null)
            {
                if (!string.IsNullOrEmpty(param.PeriodBegin))
                {
                    strSql.Append(@" AND SysShift.PeriodBegin ='" + param.PeriodBegin + "'");
                }
                if (param.DepartmentId >= 0)
                {
                    strSql.Append(" AND SysShift.DepartmentId = " + param.DepartmentId);
                }
            }
            strSql.Append(@") as info
                            on SysUser.id = info.UserId 
                            where SysUser.BaseIsDelete = 0 and SysPosition.PositionName = 'Employee'");
            var parameter = new List<DbParameter>();
            if (param != null)
            {
                if (param.DepartmentId >= 0)
                {
                    // get children departmentids
                    strSql.Append(" AND SysUser.DepartmentId = " + param.DepartmentId);
                    parameter.Add(DbParameterExtension.CreateDbParameter("@DepartmentId", param.DepartmentId));
                }
                if (!string.IsNullOrEmpty(param.PeriodBegin))
                {
                    //strSql.Append(" AND info.PeriodBegin = '" + param.PeriodBegin +"'");
                    parameter.Add(DbParameterExtension.CreateDbParameter("@PeriodBegin", param.PeriodBegin));
                }
            }
            strSql.Append(@" order by SysUser.DepartmentId asc");
            return parameter;
        }
        #endregion
    }
}
