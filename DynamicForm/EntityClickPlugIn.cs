using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Util;

namespace Witt.Cloud.PlugIn.DynamicForm
{
    [HotUpdate]
    [Description("单据体连动数据插件示例")]
    public class EntityClickPlugIn : AbstractDynamicFormPlugIn
    {
        const string HeadEntityKey = "F_PAEZ_Entity";
        const string DetailEntityKey = "F_PAEZ_Details";

        #region FieldKey collection

        const string BillNoKey = "F_PAEZ_BILLNO";
        const string SupplierKey = "F_PAEZ_SUPNAME";
        const string HeadFidKey = "F_PAEZ_FID";

        //details
        const string MaterialKey = "F_PAEZ_MatName";
        const string QtyKey = "F_PAEZ_COUNT";
        const string AmountKey = "F_PAEZ_AMOUNT";

        #endregion

        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            //绑定第一个单据体的数据源

            //可以使用SQL方式获取数据
            //string strSql = "select * from t_pur_poorder where 1=1";
            const string formId = "PUR_PurchaseOrder"; //使用采购订单示例
            List<SelectorItemInfo> selectorItems = new List<SelectorItemInfo>
            {
                new SelectorItemInfo("FID"),
                new SelectorItemInfo("FBillNo"),
                new SelectorItemInfo("FSupplierId")
            };
            OQLFilter filter = OQLFilter.CreateHeadEntityFilter("FCREATEDATE > '2014-08-25' AND FCREATEDATE < '2014-08-26' ");
            var objs = BusinessDataServiceHelper.Load(this.View.Context, formId, selectorItems, filter);
            if (objs.Length == 0) return;

            for (int i = 0; i < objs.Length; i++)
            {
                this.Model.CreateNewEntryRow(HeadEntityKey);
                this.Model.SetValue(HeadFidKey, objs[i]["ID"], i);
                this.Model.SetValue(BillNoKey, objs[i]["BillNo"], i);

                var suplierData = objs[i]["SupplierId"] as DynamicObject;
                if (suplierData != null)
                {
                    this.Model.SetValue(SupplierKey, suplierData["Name"] == null ? string.Empty : suplierData["Name"].ToString(), i);
                }

            }
            this.View.UpdateView(HeadEntityKey);

        }

        public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
        {
            base.EntityRowDoubleClick(e);
            if (!e.Key.Equals(HeadEntityKey, StringComparison.CurrentCultureIgnoreCase)) return;

            //通过row获取主键

            var headEntry = this.View.GetControl<EntryGrid>(HeadEntityKey);

            var headEntity = this.View.BusinessInfo.GetEntity(HeadEntityKey);
            var selectObj = this.Model.GetEntityDataObject(headEntity, e.Row);
            if (selectObj == null || !selectObj.DynamicObjectType.Properties.Contains(HeadFidKey)) return;

            this.Model.DeleteEntryData(DetailEntityKey);
            var headId = selectObj[HeadFidKey].ToString();
            //使用DataTable获取单据体信息
            string strSql = string.Format(@"
                        SELECT 
                         T2.FNAME AS MATERIAL_NAME,
                         T3.FAMOUNT,
                         T0.FQTY
                         FROM  T_PUR_POORDERENTRY T0
                         INNER JOIN T_BD_MATERIAL T1 ON T0.FMATERIALID = T1.FMATERIALID
                         INNER JOIN T_BD_MATERIAL_L T2 ON T1.FMATERIALID = T2.FMATERIALID AND T2.FLOCALEID=2052
                         INNER JOIN T_PUR_POORDERENTRY_F T3 ON T0.FENTRYID = T3.FENTRYID
                         WHERE T0.FID ={0}", headId);
            var entryData = DBServiceHelper.ExecuteDataSet(this.View.Context, strSql);
            if (entryData == null || entryData.Tables[0].Rows.Count == 0) return;

            for (int i = 0; i < entryData.Tables[0].Rows.Count; i++)
            {
                this.Model.CreateNewEntryRow(DetailEntityKey);
                var dr = entryData.Tables[0].Rows[i];

                var materialName = dr["MATERIAL_NAME"] ?? string.Empty;
                this.Model.SetValue(MaterialKey, materialName, i);

                var amount = dr["FAMOUNT"] ?? 0;
                this.Model.SetValue(AmountKey, amount, i);

                var qty = dr["FQTY"] ?? 0;
                this.Model.SetValue(QtyKey, qty, i);

            }

            this.View.UpdateView(DetailEntityKey);
        }

    }
}
