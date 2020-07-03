using Kingdee.BOS.App.Core.BillCodeRule;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Witt.Cloud.PlugIn.Bill
{
    [Description("编码规则更新插件")]
    public class UpdateMaxNoPlugIn : AbstractBillPlugIn
    {

        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);
            if(e.BarItemKey.EqualsIgnoreCase("tbUpdateMaxCodes"))
            {
                UpdateMaxNo();
            }
        }

        private void UpdateMaxNo()
        {
            //获取当前编码规则id，然后更新
            if (this.View.OpenParameter.Status == OperationStatus.ADDNEW) return;


            var objType = this.Model.GetValue("FBillFormID") as DynamicObject;
            if (objType == null) return;

            string ruleId = this.Model.DataObject["Id"].ToString();
            var formId = objType["Id"].ToString();
            if(!formId.EqualsIgnoreCase("FA_CARD"))
            {
                this.View.ShowMessage("仅限更新资产卡片业务对象");
                return;
            }

            var formMetadata = MetaDataServiceHelper.Load(this.Context, formId) as FormMetadata;

            List<string> codeRuleFieldList = BusinessDataServiceHelper.GetCodeRuleFieldList(this.Context,
                       formMetadata.BusinessInfo, ruleId);
            //至少有一个单据编号字段
            List<SelectorItemInfo> selectedInfos = SelectorItemInfo.CreateItems(codeRuleFieldList.ToArray());

            DynamicObject[] objs = BusinessDataServiceHelper.Load(this.Context, formMetadata.BusinessInfo,
                selectedInfos, null);

            string strSql = string.Format("SELECT fcodeid, fbyvalue,fnummax FROM T_BAS_BILLCODES WHERE FRULEID = '{0}'", ruleId);
            var dbBillcodes = DBUtils.ExecuteDynamicObject(this.Context, strSql);

            var billNoField = GetBillNoField(ruleId, formMetadata);


            Dictionary<int, Tuple<string,string,decimal>> dicUpdateMax = new Dictionary<int, Tuple<string, string, decimal>>();
            foreach (var dbCodes in dbBillcodes)
            {
                int codeId = int.Parse(dbCodes["fcodeid"].ToString());
                string byValue = dbCodes["fbyvalue"].ToString();
                decimal numMax = decimal.Parse(dbCodes["fnummax"].ToString());

                var regexPattern = this.BuildRepairFlowNoExp(4, '0');
                var replacePattern = byValue.Replace("{{{{{0}}}", regexPattern);
                Regex regexCodes = new Regex(replacePattern);

                foreach (var dataObj in objs)
                {
                    var billNoList = this.GetBillNoList(dataObj, billNoField);
                    foreach (var billCode in billNoList)
                    {
                        //需要替换掉配置的-号
                        var repairMatch = regexCodes.Match(billCode.Replace("-",""));

                        if (!repairMatch.Success || repairMatch.Groups.Count <= 1)
                        {
                            continue;
                        }
                        //分组2是流水号数据
                        var flowNoValue = repairMatch.Groups[1].Value;
                        if (flowNoValue.IsNullOrEmptyOrWhiteSpace()) continue;                        
                        
                        decimal newMaxNo;
                        if (!decimal.TryParse(flowNoValue, out newMaxNo)) continue;

                        
                        if (dicUpdateMax.ContainsKey(codeId))
                        {
                            decimal oldMaxNo = dicUpdateMax[codeId].Item3;
                            if (newMaxNo > oldMaxNo)
                            {
                                dicUpdateMax[codeId] = new Tuple<string, string, decimal>(ruleId, byValue, newMaxNo); ;
                            }
                        }
                        else
                        {
                            dicUpdateMax.Add(codeId, new Tuple<string, string, decimal>(ruleId, byValue, newMaxNo));
                        }
                    }
                }
            }

            UpdateBillcodes(dicUpdateMax);
        }


        /// <summary>
        /// 更新最大流水号
        /// </summary>
        /// <param name="dicUpdateMax"></param>
        private void UpdateBillcodes(Dictionary<int, Tuple<string, string, decimal>> dicUpdateMax)
        {            
            foreach(var item in dicUpdateMax)
            {
                BillCodeRuleHelper.UpdateMaxValue(this.Context, item.Value.Item1, item.Value.Item2, item.Value.Item3);
            }

            this.View.ShowMessage("历史数据更新最大流水号成功！");
        }

        private Field GetBillNoField(string ruleId,FormMetadata metadata)
        {
            string strSql = string.Format(@"SELECT  FSPECIFICKEY FROM T_BAS_BILLCODERULE WHERE FSPECIFICKEY != '' AND FRULEID = '{0}' ", ruleId);

            var specificKey = DBUtils.ExecuteScalar<string>(this.Context, strSql, string.Empty);
            if (string.IsNullOrWhiteSpace(specificKey))
            {
                return metadata.BusinessInfo.GetBillNoField();
            }

            return metadata.BusinessInfo.GetField(specificKey);
        }

        private List<string> GetBillNoList(DynamicObject obj, Field billNoField)
        {
            var billNoList = new List<string>();
            if (obj.DynamicObjectType.Properties.Contains(billNoField.PropertyName))
            {
                var billNo = obj[billNoField.PropertyName];
                if (!billNo.IsNullOrEmptyOrWhiteSpace())
                {
                    billNoList.Add(billNo.ToString());
                }
            }
            else if (billNoField.Entity is EntryEntity)
            {
                var entityData = obj[billNoField.Entity.EntryName] as DynamicObjectCollection;
                if (entityData != null && entityData.Any(f => f.DynamicObjectType.Properties.ContainsKey(billNoField.PropertyName)))
                {
                    foreach (var data in entityData)
                    {
                        if (data[billNoField.PropertyName] != null && !string.IsNullOrWhiteSpace(data[billNoField.PropertyName].ToString()))
                        {
                            billNoList.Add(data[billNoField.PropertyName].ToString());
                        }
                    }
                }
            }

            return billNoList;
        }

        private string BuildRepairFlowNoExp(int len,char addChar)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            for (short i = 1; i <= len - 1; i++)
            {
                //构造类似如下 (0{1}\d{3}|0{2}\d{2}|0{3}\d{1}|
                sb.AppendFormat(@"{0}{{{1}}}\d{{{2}}}|", addChar, i, len - i);
            }
            //补号规则，流水号也是定长的，因此不需要{0,3}这种形式
            sb.AppendFormat(@"[1-9][0-9]{{{0}}}|\d{{{0}}}[0]", len - 1);
            sb.Append(")");
            return sb.ToString();
        }
    }
}
