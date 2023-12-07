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

namespace YiSha.Service.OrganizationManage
{
    public class LeaveService : RepositoryFactory
    {
        #region 获取数据
        public async Task<List<LeaveAllEntity>> GetList(LeaveListParam param)
        {
            var strSql = new StringBuilder();
            List<DbParameter> filter = ListFilter(param, strSql);
            try
            {
                var list = await this.BaseRepository().FindList<LeaveAllEntity>(strSql.ToString(), filter.ToArray());
                return list.ToList();
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<LeaveAllEntity>> GetPageList(LeaveListParam param, Pagination pagination)
        {
            var strSql = new StringBuilder();
            List<DbParameter> filter = ListFilter(param, strSql);
            try
            {
                var list = await this.BaseRepository().FindList<LeaveAllEntity>(strSql.ToString(), filter.ToArray(), pagination);
                return list.ToList();
            }
            catch
            {
                return null;
            }
        }

        public async Task<LeaveEntity> GetEntity(long id)
        {
            return await this.BaseRepository().FindEntity<LeaveEntity>(id);
        }

        public async Task<List<LeaveAllEntity>> GetListByUserId(long? userId, string periodBegin)
        {
            var strSql = new StringBuilder();
            List<DbParameter> filter = ListFilterByUserId(userId, periodBegin, strSql);
            var list = await this.BaseRepository().FindList<LeaveAllEntity>(strSql.ToString(), filter.ToArray());
            return list.ToList();
        }

        #endregion

        #region 提交数据
        //public async Task UpdateUser(UserEntity entity)
        //{
        //    await this.BaseRepository().Update(entity);
        //}

        public async Task SaveForm(LeaveEntity entity)
        {
            var db = await this.BaseRepository().BeginTrans();
            try
            {
                if (entity.Id.IsNullOrZero())
                {
                    await entity.Create();
                    await db.Insert(entity);
                }
                else
                {
                    await entity.Modify();
                    await db.Update(entity);
                }
                await db.CommitTrans();
            }
            catch
            {
                await db.RollbackTrans();
                throw;
            }
        }

        public async Task DeleteForm(string ids)
        {
            var db = await this.BaseRepository().BeginTrans();
            try
            {
                long[] idArr = TextHelper.SplitToArray<long>(ids, ',');
                await db.Delete<LeaveEntity>(idArr);
                await db.CommitTrans();
            }
            catch
            {
                await db.RollbackTrans();
                throw;
            }
        }

        //public async Task ChangeUser(UserEntity entity)
        //{
        //    await entity.Modify();
        //    await this.BaseRepository().Update(entity);
        //}
        #endregion

        #region 私有方法
        private List<DbParameter> ListFilter(LeaveListParam param, StringBuilder strSql)
        {
            //strSql.Append(@"select SysLeave.Id as Id, SysUser.UserName as UserName, 
            //                SysUser.RealName as RealName, SysLeave.UserId as UserId,
            //                SysDepartment.DepartmentName as DepartmentName, FromDay, ToDay, StartTime, 
            //                EndTime,
            //                case LeaveType when 0 then N'天' else N'时间段' end as LeaveType, 
            //                case LeaveKind when 0 then N'事假' when 1 then N'年假' when 2 then N'病假' else N'其它' end as LeaveKind,
            //                SysLeave.Remark  as Remark  
            //                from SysLeave inner join SysUser on SysUser.Id=SysLeave.UserId inner join 
            //                SysDepartment on SysDepartment.Id=SysUser.DepartmentId where 1=1");
            strSql.Append(@"select SysLeave.Id as Id, SysUser.UserName as UserName, 
                            SysUser.RealName as RealName, SysLeave.UserId as UserId,
                            SysDepartment.DepartmentName as DepartmentName, FromDay, ToDay, StartTime, 
                            EndTime, LeaveType, LeaveKind,
                            SysLeave.Remark  as Remark  
                            from SysLeave inner join SysUser on SysUser.Id=SysLeave.UserId inner join 
                            SysDepartment on SysDepartment.Id=SysUser.DepartmentId where 1=1");
            var parameter = new List<DbParameter>();
            if (param != null)
            {
                if (param.DepartmentId >= 0)
                {
                    strSql.Append(@" and SysDepartment.Id=" + param.DepartmentId);
                    parameter.Add(DbParameterExtension.CreateDbParameter("@DepartmentId", param.DepartmentId));
                }
                if (!string.IsNullOrEmpty(param.UserName))
                {
                    strSql.Append(@" and SysUser.UserName like '%" + param.UserName+ "%'" );
                    parameter.Add(DbParameterExtension.CreateDbParameter("@UserName", param.UserName));
                }
                if (!string.IsNullOrEmpty(param.FromDay))
                {
                    strSql.Append(@" and (SysLeave.FromDay >= '" + param.FromDay + "' or SysLeave.ToDay >= '" + param.FromDay + "')");
                    parameter.Add(DbParameterExtension.CreateDbParameter("@FromDay", param.FromDay));
                }
                if (!string.IsNullOrEmpty(param.ToDay))
                {
                    strSql.Append(@" and (SysLeave.FromDay <= '" + param.ToDay + "' or SysLeave.ToDay <= '" + param.ToDay + "')");
                    //parameter.Add(DbParameterExtension.CreateDbParameter("@FromDay", param.FromDay));
                }
            }
            return parameter;
        }

        private List<DbParameter> ListFilterByUserId(long? userId, string periodBegin, StringBuilder strSql)
        {
            string periodEnd = DateTime.Parse(periodBegin).AddDays(6).ToString("yyyy-MM-dd");
            //strSql.Append(@"select SysLeave.Id as Id, SysUser.UserName as UserName, SysUser.RealName as RealName, 
            //                SysDepartment.DepartmentName as DepartmentName, FromDay, ToDay, StartTime, 
            //                EndTime,
            //                case LeaveType when 0 then N'天' else N'时间段' end as LeaveType, 
            //                case LeaveKind when 0 then N'事假' when 1 then N'年假' when 2 then N'病假' else N'其它' end as LeaveKind,
            //                SysLeave.Remark  as Remark  
            //                from SysLeave inner join SysUser on SysUser.Id=SysLeave.UserId inner join 
            //                SysDepartment on SysDepartment.Id=SysUser.DepartmentId where 1=1");
            strSql.Append(@"select SysLeave.Id as Id, SysUser.UserName as UserName, SysUser.RealName as RealName, 
                            SysDepartment.DepartmentName as DepartmentName, FromDay, ToDay, StartTime, 
                            EndTime, LeaveType, LeaveKind,
                            SysLeave.Remark  as Remark  
                            from SysLeave inner join SysUser on SysUser.Id=SysLeave.UserId inner join 
                            SysDepartment on SysDepartment.Id=SysUser.DepartmentId where 1=1");
            var parameter = new List<DbParameter>();
            if (userId != null)
            {
                strSql.Append(@" and SysUser.Id=" + userId);
            }
            if (periodBegin != null)
            {
                strSql.Append(@" and SysLeave.FromDay<='" + periodEnd + "'");
                strSql.Append(@" and SysLeave.ToDay>='" + periodBegin + "'");
            }
            return parameter;
        }
        #endregion
    }
}
