/*
* 服务需要引用组件如下
* Kingdee.BOS.dll、Kingdee.BOS.ServiceFacade.KDServiceFx.dll、Kingdee.BOS.WebApi.ServicesStub.dll
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.BOS.JSON;

namespace Witt.Cloud.PlugIn.WebAPI
{
    public class CustomBusinessService : AbstractWebApiBusinessService
    {
        public CustomBusinessService(KDServiceContext context): base(context)
        {

        }

        public JSONArray ExecuteService(string parameter)
        {
            JSONArray jsonArray = new JSONArray();
            jsonArray.Add("CustomBusinessService:"+ parameter);
            return jsonArray;
        }
    }
}
