using System.Collections.Generic;
using Kingdee.BOS.Core.Report;
using System.Data;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS;
using System.ComponentModel;

namespace Witt.Cloud.PlugIn.Report
{
    [Description("分页账表demo")]
    public class DemoMoveReportPlugIn : SysReportBaseService
    {
        public override void Initialize()
        {
            // 标记报表类型
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_MOVE;
            this.ReportProperty.IsGroupSummary = true;
        }

        /// <summary>
        /// 构建分页报表每个报表的临时表
        /// 首先从分页依据中拿到本次分页的条件，就是当前页报表的条件：this.CacheDataList[filter.CurrentPosition]
        /// 然后把条件拼装到SQL中，例如b.FLocaleId= dr["FLocaleId"] 语言id=当前报表的语言id
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="tableName"></param>
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            DataRow dr = this.CacheDataList[filter.CurrentPosition];
            string sSQL = @"select FFORMID , FSCHEMENAME,FISDEFAULT ,FSEQ , {0} 
                        into {1} 
                    from T_BAS_FILTERSCHEME 
                    where FISDEFAULT={2}";
            KSQL_SEQ = string.Format(KSQL_SEQ, "FISDEFAULT");
            sSQL = string.Format(sSQL, this.KSQL_SEQ, tableName, dr["FISDEFAULT"].ToString());
            DBUtils.Execute(this.Context, sSQL);
        }

        public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
        {
            List<SummaryField> fls = new List<SummaryField>();
            SummaryField fs = new SummaryField("FSEQ", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM);
            fls.Add(fs);
            return fls;
        }

        /// <summary>
        /// 分页报表必须实现的方法，此方法用于为报表提供分页依据。
        /// 比如以下示例：分别按语言来对部门分类，也就是说每种语言一个报表，中文的是一个报表、英文的一个报表，繁体的一个
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public override DataTable GetList(IRptParams filter)
        {
            DataTable dt;
            string sSQL = "select FISDEFAULT from T_BAS_FILTERSCHEME group by FISDEFAULT";
            dt = DBUtils.ExecuteDataSet(this.Context, sSQL).Tables[0];
            return dt;
        }

        public override ReportTitles GetReportTitles(IRptParams filter)
        {
            ReportTitles titles = new ReportTitles();
            if (CacheDataList == null)
            {
                DataTable dt = GetList(filter);
                if (dt != null && dt.Rows.Count > 0)
                {
                    //titles.AddTitle("FCondition", dt.Rows[0]["flocaleid"].ToString());
                    return titles;
                }
                return null;
            }
            DataRow dr = this.CacheDataList[filter.CurrentPosition];
            //titles.AddTitle("FCondition", dr["flocaleid"].ToString());
            return titles;
        }

        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            // TODO:
            ReportHeader header = new ReportHeader();
            header.AddChild("FFORMID", new LocaleValue(Kingdee.BOS.Resource.ResManager.LoadKDString("表单ID", "002460030014674", Kingdee.BOS.Resource.SubSystemType.BOS), this.Context.UserLocale.LCID));

            header.AddChild("FSCHEMENAME", new LocaleValue(Kingdee.BOS.Resource.ResManager.LoadKDString("过滤方案名称", "002460030014677", Kingdee.BOS.Resource.SubSystemType.BOS), this.Context.UserLocale.LCID));
            header.AddChild("FISDEFAULT", new LocaleValue(Kingdee.BOS.Resource.ResManager.LoadKDString("是否缺省", "002460030014680", Kingdee.BOS.Resource.SubSystemType.BOS), this.Context.UserLocale.LCID));

            header.AddChild("FSEQ", new LocaleValue(Kingdee.BOS.Resource.ResManager.LoadKDString("序号", "002460030014683", Kingdee.BOS.Resource.SubSystemType.BOS), this.Context.UserLocale.LCID), SqlStorageType.SqlInt);
            return header;
        }
    }
}

