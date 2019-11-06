using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Witt.Cloud.PlugIn.Bill
{
    public class LoadSavePlugIn : AbstractBillPlugIn
    {

        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);
            LoadSample();
        }


        private void LoadSample()
        {
            //物料的formId
            const string formId = "BD_MATERIAL";
            //通过formId取到元数据
            var metadata = MetaDataServiceHelper.Load(this.Context, formId) as FormMetadata; ;

            if (metadata == null) return;
            var businInfo = metadata.BusinessInfo;
            //假设物料主键是100001
            var dataObj = BusinessDataServiceHelper.LoadSingle(this.Context, 100201, businInfo.GetDynamicObjectType());

            dataObj["Number"] = "TestNum";
            this.Model.SetValue("FNumber", "TestNum");            

            //保存数据
            BusinessDataServiceHelper.Save(this.Context, dataObj);
        }
    }
}
