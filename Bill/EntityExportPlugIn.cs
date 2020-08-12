using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Witt.Cloud.PlugIn.Bill
{
    [Description("引出单据字段测试")]
    public class EntityExportPlugIn: AbstractBillPlugIn
    {
        public override void BeforeEntityExport(BeforeEntityExportArgs e)
        {
            base.BeforeEntityExport(e);



            var headers = e.Headers;
            headers.Remove("FMaterialId");
            

            
        }
    }
}
