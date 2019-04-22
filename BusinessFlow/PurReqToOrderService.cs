using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Witt.Cloud.PlugIn.BusinessFlow
{
    [Description("单据转换插件，如果不支持VMI重置为空")]
    public class PurReqToOrderService : AbstractConvertPlugIn
    {

        public override void AfterConvert(AfterConvertEventArgs e)
        {
            ExtendedDataEntity[] data = e.Result.FindByEntityKey("FBillHead");
            if (data != null && data.Length > 0)
            {
                foreach (ExtendedDataEntity entity in data)
                {
                    DynamicObject billObj = entity.DataEntity;
                    long supplierId = Convert.ToInt64(billObj["SupplierId_Id"]);
                    if (supplierId != 0)
                    {
                        var supplierObj = DealSupplierVMI(supplierId);
                        if (supplierObj == null)
                        {
                            billObj["SupplierId_Id"] = 0;
                            billObj["SupplierId"] = null;
                        }
                    }
                }
            }
        }

        private object DealSupplierVMI(long suggestSupplierId)
        {
            string sql = @"select 1
                        from t_BD_Supplier a 
                        inner join t_BD_SupplierBusiness b
                         on a.FSUPPLIERID = b.FSUPPLIERID
                          where a.FSUPPLIERID= @FSUPPLIERID and b.FVMIBUSINESS = '1'";
            SqlParam param = new SqlParam("FSUPPLIERID", KDDbType.Int64, suggestSupplierId);

            using (var reader = DBUtils.ExecuteReader(this.Context, sql, param))
            {
                if (reader.Read())
                {
                    return suggestSupplierId;
                }
                else
                {
                    return null;
                }
            }
        }

    }
}
