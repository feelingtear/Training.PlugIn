using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Model.List;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Witt.Cloud.PlugIn.BillList
{
    [Description("自动刷新列表")]
    public class AutoRefreshListPlugIn : AbstractListPlugIn
    {
        private const string TestKey = "TestKey";
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            KeepAlive();
        }

        

        public override void PrepareFilterParameter(FilterArgs e)
        {
            base.PrepareFilterParameter(e);

        }

        HashSet<string> entityIdSet = new HashSet<string>();
        public override void FormatCellValue(FormatCellValueArgs args)
        {
            base.FormatCellValue(args);

           var entityId =  args.DataRow["FEntityId"].ToString();
            if(entityIdSet.Contains(entityId))
            {
                args.FormateValue = string.Empty;
            }
            else
            {
                entityIdSet.Add(entityId);
            }
        }


        public void KeepAlive()
        {
            JSONObject para = new JSONObject
            {
                ["key"] = TestKey,
                ["eventName"] = "CustomEvents"
            };
            var data = new JSONObject();
            para["data"] = data;
            data["refreshData"] = DateTime.Now;
            para["delay"] = "60000"; //每100s刷新一次
            this.View.AddAction(JSAction.FireCustomRequest, para);
        }

        public override void BeforeClosed(BeforeClosedEventArgs e)
        {
            base.BeforeClosed(e);

        }


        public override void CustomEvents(CustomEventsArgs e)
        {
            base.CustomEvents(e);
            if (e.Key.EqualsIgnoreCase(TestKey))
            {
                var data = JSONObject.Parse(e.EventArgs);

                if (data != null && data.ContainsKey("refreshData"))
                {
                    this.View.Refresh();
                }
            }

        }
    }
}
