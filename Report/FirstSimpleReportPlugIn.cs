using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Util;

namespace Witt.Cloud.PlugIn.Report
{
    [Description("first report demo")]
    public class FirstSimpleReportPlugIn : SysReportBaseService
    {
        public override void Initialize()
        {
            base.Initialize();
            // 简单账表类型：普通、树形、分页
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;

            this.IsCreateTempTableByPlugin = true;
            this.ReportProperty.IsUIDesignerColumns = false;
            this.ReportProperty.IsGroupSummary = true;
            // 单据主键：两行FID相同，则为同一单的两条分录，单据编号可以不重复显示
            this.ReportProperty.PrimaryKeyFieldName = "FID";
            // 
            this.ReportProperty.IsDefaultOnlyDspSumAndDetailData = true;
        }

        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);
            if (filter.FilterParameter.SortString.IsNullOrEmpty())
            {
                this.KSQL_SEQ = string.Format(this.KSQL_SEQ, " t1.FMATERIALID asc");
            }
            else
            {

                this.KSQL_SEQ = string.Format(this.KSQL_SEQ, filter.FilterParameter.SortString);
            }
            string sql = string.Format(@"/*dialect*/
                                select t0.FID, t1.FENTRYID
                                       ,t0.FBILLNO
                                      ,t0.FDate
                                      ,t0.FDOCUMENTSTATUS
                                      ,t2.FLOCALCURRID
                                      ,ISNULL(t20.FPRICEDIGITS,4) AS FPRICEDIGITS
                                      ,ISNULL(t20.FAMOUNTDIGITS,2) AS FAMOUNTDIGITS
                                      ,t1.FMATERIALID
                                      ,t1M_L.FNAME as FMaterialName
                                      ,t1.FQTY
                                       ,{0}
                                        INTO {1}
                                 from T_PUR_POORDER t0
                                inner join T_PUR_POORDERFIN t2 on (t0.FID = t2.FID)
                                 left join T_BD_CURRENCY t20 on (t2.FLOCALCURRID = t20.FCURRENCYID)
                                inner join T_PUR_POORDERENTRY t1 on (t0.FID = t1.FID)
                                 left join T_BD_MATERIAL_L t1M_L on (t1.FMATERIALID = t1m_l.FMATERIALID and t1M_L.FLOCALEID = 2052)
                                where 1=1 ", KSQL_SEQ, tableName);
            DBUtils.ExecuteDynamicObject(this.Context, sql);
        }

        public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
        {
            var result = base.GetSummaryColumnInfo(filter);
            result.Add(new SummaryField("FQty", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));

            return result;
        }

        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            // FID, FEntryId, 编号、状态、物料、数量、单位、单位精度、单价、价税合计
            ReportHeader header = base.GetReportHeaders(filter);
            // 编号
            var dateHeader = header.AddChild("FDate", new LocaleValue("日期"));
            dateHeader.ColIndex = 0;
            var status = header.AddChild("FDocumentStatus", new LocaleValue("状态"));
            status.ColIndex = 7;
            var billNo = header.AddChild("FBillNo", new LocaleValue("单据编号"));
            billNo.ColIndex = 1;
            billNo.IsHyperlink = true;          // 支持超链接
            var material = header.AddChild("FMaterialName", new LocaleValue("物料"));
            material.ColIndex = 2;
            var qty = header.AddChild("FQty", new LocaleValue("数量"), SqlStorageType.SqlDecimal);

            return header;
        }

    }
}
