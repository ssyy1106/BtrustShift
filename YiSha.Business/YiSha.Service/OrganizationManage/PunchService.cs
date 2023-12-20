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
using NPOI.SS.Formula.Functions;
using System.Reflection;
using NLog.Filters;

namespace YiSha.Service.OrganizationManage
{
    public class PunchService : RepositoryFactory
    {
        public UserService userService = new();
        #region 获取数据
        public async Task<PunchProblemEntity> GetEntity(long id)
        {
            return await this.BaseRepository().FindEntity<PunchProblemEntity>(id);
        }
        public async Task<PunchProblemEntity> GetAllInfo(long id, long userId, string punchDate)
        {
            UserEntity user = await userService.GetEntity(userId);
            PunchProblemEntity problem = new();
            if (id > 0)
            {
                problem = await GetEntity(id);
            }
            else
            {
                problem.UserId = user.Id;
                problem.BtrustId = user.BtrustId;
                problem.PunchDate = punchDate;
            }
            problem.RealName = user.RealName;

            var strSql = new StringBuilder();
            FilterPunches(strSql, userId, punchDate);
            var listPunchEnum = await this.BaseRepository().FindList<PunchEntity>(strSql.ToString());
            List<PunchEntity> listPunch = listPunchEnum.ToList();
            foreach(PunchEntity punch in listPunch)
            {
                problem.Punches.Add(punch.Year + "-" + punch.Month + "-" + punch.Day);
            }
            return problem;
        }
        public async Task<List<PunchAllEntity>> GetPageList(PunchParam param)
        {
            //List<ShiftDetailEntity> res = new();
            var strSql = new StringBuilder();
            List<DbParameter> filter = ListFilter(param, strSql);
            List<ShiftAllDetailEntity> listShift = new();
            List<PunchEntity> listPunch = new();
            List<PunchProblemEntity> listPunchProblem = new();
            List<PunchAllEntity> res = new();
            try
            {
                var listShiftEnum = await this.BaseRepository().FindList<ShiftAllDetailEntity>(strSql.ToString(), filter.ToArray());
                listShift = listShiftEnum.ToList();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return null;
            }
            strSql = new StringBuilder();
            filter = ListFilterPunch(param, strSql);
            try
            {
                var listPunchEnum = await this.BaseRepository().FindList<PunchEntity>(strSql.ToString(), filter.ToArray());
                listPunch = listPunchEnum.ToList();
                //Dictionary<string, SortedList<string, PunchEntity>> dic = new();
                Dictionary<string, List<string>> dic = new();
                foreach (var punch in listPunch)
                {
                    string punchDate = punch.Year + "," + punch.Month + "," + punch.Day + "," + punch.Hour + "," + punch.Minute;
                    if (!dic.ContainsKey(punch.BtrustId))
                    {
                        dic[punch.BtrustId] = new List<string>();

                    }
                    dic[punch.BtrustId].Add(punchDate);
                }
                strSql = new StringBuilder();
                filter = ListFilterPunchProblem(param, strSql);
                var listPunchProblemEnum = await this.BaseRepository().FindList<PunchProblemEntity>(strSql.ToString(), filter.ToArray());
                listPunchProblem = listPunchProblemEnum.ToList();
                Dictionary<string, List<PunchProblemEntity>> dicProblem = new();
                foreach (var punchProblem in listPunchProblem)
                {
                    if (!dicProblem.ContainsKey(punchProblem.BtrustId))
                    {
                        dicProblem[punchProblem.BtrustId] = new List<PunchProblemEntity>();

                    }
                    dicProblem[punchProblem.BtrustId].Add(punchProblem);
                }
                // calculate 'punch all entity' with shift and punch information
                foreach ( var shift in listShift)
                {
                    string[,] punches = new string[7, 2];
                    if (shift.BtrustId is not null && dic.ContainsKey(shift.BtrustId) && dic[shift.BtrustId].Count > 0)
                    {
                        for (int i = 0; i < dic[shift.BtrustId].Count; i++)
                        {
                            int day = getDay(dic[shift.BtrustId][i], param.PeriodBegin);
                            string[] items = dic[shift.BtrustId][i].Split(',');
                            string hour = items[3];
                            string minute = items[4];
                            string hourMinute = getHour(hour, minute);
                            if (day >= 0)
                            {
                                if (punches[day, 0] is null || punches[day, 0] == "" || getMinutes(punches[day, 0]) > getMinutes(hourMinute))
                                {
                                    punches[day, 0] = hourMinute;
                                }
                                if (punches[day, 1] is null || punches[day, 1] == "" || getMinutes(punches[day, 1]) < getMinutes(hourMinute))
                                {
                                    punches[day, 1] = hourMinute;
                                }
                            }
                        }
                    }
                    PunchAllEntity val = new()
                    {
                        UserId = shift.UserId,
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

                        MondayBeginPunch = punches[0, 0],
                        MondayEndPunch = punches[0, 1],
                        TuesdayBeginPunch = punches[1, 0],
                        TuesdayEndPunch = punches[1, 1],
                        WednesdayBeginPunch = punches[2, 0],
                        WednesdayEndPunch = punches[2, 1],
                        ThursdayBeginPunch = punches[3, 0],
                        ThursdayEndPunch = punches[3, 1],
                        FridayBeginPunch = punches[4, 0],
                        FridayEndPunch = punches[4, 1],
                        SaturdayBeginPunch = punches[5, 0],
                        SaturdayEndPunch = punches[5, 1],
                        SundayBeginPunch = punches[6, 0],
                        SundayEndPunch = punches[6, 1]
                    };
                    if (val.BtrustId == null || !dicProblem.ContainsKey(val.BtrustId))
                    {
                        calculateStatus(val, null, param.PeriodBegin);
                    } else
                    {
                        calculateStatus(val, dicProblem[val.BtrustId], param.PeriodBegin);
                    }
                        
                    
                    res.Add(val);
                }
                return res;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return null;
            }
        }
        #endregion

        #region 私有方法
        private void calculateStatus(PunchAllEntity val, List<PunchProblemEntity> punchProblem, string periodBegin)
        {
            string[] days = new string[7] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            bool[] punches = new bool[7];
            int[] status = new int[7];
            long[] problemIds = new long[7];
            if (punchProblem != null)
            {
                foreach (PunchProblemEntity problem in punchProblem)
                {
                    int day = getDay(problem.PunchDate, periodBegin);
                    punches[day] = true;
                    status[day] = problem.Status;
                    problemIds[day] = (long)problem.Id;
                }
            }
            
            // 提早30分钟至晚到10分钟都是正常打卡
            int BEFOREBEGIN = 30;
            int AFTERBEGIN = 10;
            // 提早5分钟至晚走30分钟都是正常打卡
            int BEFOREEND = 5;
            int AFTEREND = 30;
            Type t = typeof(PunchAllEntity);
            
            for (int i = 0; i < 7; i++)
            {
                PropertyInfo filedInfoProblemId = t.GetProperty(days[i] + "PunchProblemId");
                PropertyInfo filedInfoBeginStatus = t.GetProperty(days[i] + "BeginPunchStatus");
                PropertyInfo filedInfoEndStatus = t.GetProperty(days[i] + "EndPunchStatus");
                PropertyInfo filedInfoBegin = t.GetProperty(days[i] + "Begin");
                PropertyInfo filedInfoEnd = t.GetProperty(days[i] + "End");
                PropertyInfo filedInfoBeginPunch = t.GetProperty(days[i] + "BeginPunch");
                PropertyInfo filedInfoEndPunch = t.GetProperty(days[i] + "EndPunch");
                if (punches[i])
                {
                    filedInfoBeginStatus.SetValue(val, status[i]);
                    filedInfoEndStatus.SetValue(val, status[i]);
                    filedInfoProblemId.SetValue(val, problemIds[i]);
                } else
                {
                    filedInfoProblemId.SetValue(val, (long)0);
                    if (getMinutes((string)filedInfoBeginPunch.GetValue(val)) <= 
                        getMinutes((string)filedInfoBegin.GetValue(val)) + AFTERBEGIN &&
                        getMinutes((string)filedInfoBeginPunch.GetValue(val)) >=
                        getMinutes((string)filedInfoBegin.GetValue(val)) - BEFOREBEGIN)
                    {
                        filedInfoBeginStatus.SetValue(val, 0);
                    } else
                    {
                        filedInfoBeginStatus.SetValue(val, 1);
                    }
                    if (getMinutes((string)filedInfoEndPunch.GetValue(val)) <=
                        getMinutes((string)filedInfoEnd.GetValue(val)) + AFTEREND &&
                        getMinutes((string)filedInfoEndPunch.GetValue(val)) >=
                        getMinutes((string)filedInfoEnd.GetValue(val)) - BEFOREEND)
                    {
                        filedInfoEndStatus.SetValue(val, 0);
                    }
                    else
                    {
                        filedInfoEndStatus.SetValue(val, 1);
                    }
                }
            }
        }
        private int getMinutes(string hourMinute)
        {
            if (hourMinute == null || hourMinute.Length != 5) 
            { 
                return 0; 
            }
            string hour = hourMinute.Substring(0, 2);
            string minute = hourMinute.Substring(3, 2);
            return int.Parse(hour) * 60 + int.Parse(minute);
        }
        private string getHour(string hour, string minute)
        {
            if (hour is not null && hour != "" && hour.Length == 1)
            {
                hour = "0" + hour;
            }
            if (minute is not null && minute != "" && minute.Length == 1)
            {
                minute = "0" + minute;
            }
            return hour + ":" + minute;
        }
        private int getDay(string dateStr, string PeriodBegin)
        {
            string[] items = dateStr.Split(',');
            if (items.Length <= 1)
            {
                items = dateStr.Split('-');
            }
            string year = items[0];
            string month = items[1];
            string day = items[2];
            items = PeriodBegin.Split('-');
            string periodYear = items[0];
            string periodMonth = items[1];
            string periodDay = items[2];
            DateTime punchDate = DateTime.Parse(year + "-" + month + "-" + day);
            DateTime periodDate = DateTime.Parse(periodYear + "-" + periodMonth + "-" + periodDay);
            for (int i = 0; i < 7; i++)
            {
                if (periodDate.AddDays(i) == punchDate)
                {
                    return i;
                }
            }
            return -1;
        }

        private void FilterPunches(StringBuilder strSql, long btrustId, string punchDate)
        {
            string year = punchDate.Substring(0, 4);
            string month = punchDate.Substring(5, 2);
            string day = punchDate.Substring(8, 2);
            strSql.Append(@"select BtrustId, Year, Month, Day, Hour, Minute from Syspunch where BtrustId = "
                          + btrustId + " and Year = " + year + " and Month = " + month + " and Day=" + day);
            return;
        }
        private List<DbParameter> ListFilterPunchProblem(PunchParam param, StringBuilder strSql)
        {
            strSql.Append(@"select SysPunchProblem.Id as Id, SysPunchProblem.BtrustId as BtrustId, PunchDate, 
                            SysPunchProblem.UserId as UserId, SysPunchProblem.Status as Status
                            from SysPunchProblem inner join SysUser on SysUser.Id=SysPunchProblem.UserId where 1=1 ");
            if (param != null)
            {
                if (!string.IsNullOrEmpty(param.PeriodBegin))
                {
                    int year = int.Parse(param.PeriodBegin.Substring(0, 4));
                    int month = int.Parse(param.PeriodBegin.Substring(5, 2));
                    int day = int.Parse(param.PeriodBegin.Substring(8, 2));
                    DateTime beginDate = new DateTime(year, month, day);
                    DateTime endDate = beginDate.AddDays(7);
                    strSql.Append(" AND CONVERT(DATETIME, SysPunchProblem.PunchDate)  <='"
                        + endDate.ToString("yyyy-MM-dd") + "' AND CONVERT(DATETIME, SysPunchProblem.PunchDate)  >='"
                        + beginDate.ToString("yyyy-MM-dd") + "'");
                }
                if (param.DepartmentId >= 0)
                {
                    strSql.Append(" AND SysUser.DepartmentId = " + param.DepartmentId);
                }
            }
            var parameter = new List<DbParameter>();
            return parameter;
        }
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
                        + endDate.ToString("yyyy-MM-dd") + "' AND CONVERT(DATETIME, SysPunch.Year + " + "'-'" + " + SysPunch.Month + " + "'-'" + " + SysPunch.Day)  >='"
                        + beginDate.ToString("yyyy-MM-dd") + "'");
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
