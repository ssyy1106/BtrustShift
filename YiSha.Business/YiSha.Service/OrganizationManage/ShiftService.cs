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

namespace YiSha.Service.OrganizationManage
{
    public class ShiftService : RepositoryFactory
    {
        #region 获取数据
        public async Task<List<ShiftAllDetailEntity>> GetPageList(ShiftParam param)
        {
            //List<ShiftDetailEntity> res = new();
            var strSql = new StringBuilder();
            List<DbParameter> filter = ListFilter(param, strSql);
            try
            {
                var list = await this.BaseRepository().FindList<ShiftAllDetailEntity>(strSql.ToString(), filter.ToArray());
                return list.ToList();
            } catch
            {
                return null;
            }
        }
        #endregion

        #region 提交数据
        private async Task SaveForm(string periodBegin, long? departmentId, List<ShiftAllDetailEntity> entities)
        {
            //return ;
            long? shiftId = 0;
            int totalHours = 0;
            int regularHours = 0;
            int breakHours = 0;
            foreach (ShiftAllDetailEntity shift in entities)
            {
                if (shift.ShiftId > 0)
                {
                    shiftId = shift.ShiftId;
                    //break;
                }
                totalHours += shift.TotalHours;
                regularHours += shift.RegularHours;
                breakHours += shift.BreakHours;
            }
            var db = await this.BaseRepository().BeginTrans();
            try 
            {
                ShiftEntity shift = new ShiftEntity();
                shift.PeriodBegin = periodBegin;
                shift.TotalHours = totalHours;
                shift.RegularHours = regularHours;
                shift.BreakHours = breakHours;
                shift.DepartmentId = departmentId;
                if (shiftId.IsNullOrZero())
                {
                    await shift.Create();
                    await db.Insert(shift);
                    shiftId = shift.Id;
                }
                else
                {
                    await shift.Modify();
                    shift.Id = shiftId;
                    await db.Update(shift);
                }
                // add shift details record
                if (entities.Count > 0)
                {
                    foreach (ShiftAllDetailEntity shiftAllDetail in entities)
                    {
                        ShiftDetailEntity shiftDetail = new()
                        {
                            ShiftId = shiftId,
                            TotalHours = shiftAllDetail.TotalHours,
                            RegularHours = shiftAllDetail.RegularHours,
                            BreakHours = shiftAllDetail.BreakHours,
                            MondayBegin = shiftAllDetail.MondayBegin,
                            MondayEnd = shiftAllDetail.MondayEnd,
                            TuesdayBegin = shiftAllDetail.TuesdayBegin,
                            TuesdayEnd = shiftAllDetail.TuesdayEnd,
                            WednesdayBegin = shiftAllDetail.WednesdayBegin,
                            WednesdayEnd = shiftAllDetail.WednesdayEnd,
                            ThursdayBegin = shiftAllDetail.ThursdayBegin,
                            ThursdayEnd = shiftAllDetail.ThursdayEnd,
                            FridayBegin = shiftAllDetail.FridayBegin,
                            FridayEnd = shiftAllDetail.FridayEnd,
                            SaturdayBegin = shiftAllDetail.SaturdayBegin,
                            SaturdayEnd = shiftAllDetail.SaturdayEnd,
                            SundayBegin = shiftAllDetail.SundayBegin,
                            SundayEnd = shiftAllDetail.SundayEnd,
                            UserId = shiftAllDetail.UserId,
                            Remark = shiftAllDetail.Remark
                        };
                        if (shiftAllDetail.Id.IsNullOrZero())
                        {
                            await shiftDetail.Create();
                            await db.Insert(shiftDetail);
                        }
                        else
                        {
                            await shiftDetail.Modify();
                            shiftDetail.Id = shiftAllDetail.Id;
                            await db.Update(shiftDetail);
                        }
                    }
                }
                await db.CommitTrans();
            }
            catch
            {
                await db.RollbackTrans();
                throw;
            }
            
        }

        public async Task SaveForm(ShiftAllEntity entity)
        {
            var db = await this.BaseRepository().BeginTrans();
            try
            {
                // 按照部门分组，每组一个shiftid
                Dictionary<long?, List<ShiftAllDetailEntity>> groups = new();
                foreach (ShiftAllDetailEntity shiftAllDetail in entity.Shifts)
                {
                    if (!groups.ContainsKey(shiftAllDetail.DepartmentId))
                    {
                        groups[shiftAllDetail.DepartmentId] = new List<ShiftAllDetailEntity>();

                    }
                    groups[shiftAllDetail.DepartmentId].Add(shiftAllDetail);
                }
                foreach ( KeyValuePair<long?, List<ShiftAllDetailEntity>> group in groups)
                {
                    await SaveForm(entity.PeriodBegin, group.Key, group.Value);
                }
            }
            catch
            {
                await db.RollbackTrans();
                throw;
            }
        }
        #endregion

        #region 私有方法
        //private Expression<Func<ShiftEntity, bool>> GetShiftId(ShiftParam param)
        //{
        //    var expression = LinqExtensions.True<ShiftEntity>();
        //    if (param != null)
        //    {
        //        if (param.DepartmentId > 0)
        //        {
        //            expression = expression.And(t => t.DepartmentId == param.DepartmentId);
        //        } else
        //        {
        //            expression = expression.And(t => t.DepartmentId == 0);
        //        }
        //        if (!string.IsNullOrEmpty(param.PeriodBegin))
        //        {
        //            expression = expression.And(t => t.PeriodBegin == param.PeriodBegin);
        //        } else
        //        {
        //            expression = expression.And(t => t.PeriodBegin == "1900-01-01");
        //        }
        //    }
        //    return expression;
        //}

        private List<DbParameter> ListFilter(ShiftParam param, StringBuilder strSql)
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
