using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Witt.Cloud.PlugIn.BillList
{
    public class PurchaseFomatPlugIn : AbstractListPlugIn
    {

        
        HashSet<string> entityIdSet = new HashSet<string>();

        public override void FormatCellValue(FormatCellValueArgs args)
        {
            var datarow = args.DataRow;
            if(args.Header.Key.EqualsIgnoreCase("FQTY") && datarow.ColumnContains("t3_FENTRYID"))
            {
                var curEntityId = datarow["t3_FENTRYID"].ToString();
                if(entityIdSet.Contains(curEntityId))
                {
                    args.FormateValue = string.Empty;
                }
                else
                {
                    entityIdSet.Add(curEntityId);
                }
            }            
        }

        Dictionary<string, HashSet<string>> dic = new Dictionary<string, HashSet<string>>();

        public override void AfterGetData()
        {
            base.AfterGetData();
            entityIdSet.Clear();
        }

    }
}
