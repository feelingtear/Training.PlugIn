using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;


namespace Witt.Cloud.PlugIn.WebAPI
{
    public class InvokeHelper
    {
        private static string CloudUrl = "http://localhost/k3cloud/";//K/3 Cloud 业务站点地址

        /// <summary>
        /// 登陆
        /// </summary>
        public static string Login()
        {
            HttpClient httpClient = new HttpClient();      

            httpClient.Url = string.Concat(CloudUrl, "Kingdee.BOS.WebApi.ServicesStub.AuthService.ValidateUser.common.kdsvc");

            List<object> Parameters = new List<object>();
            Parameters.Add("59ad312b4d5531");//帐套Id
            Parameters.Add("Administrator");//用户名
            Parameters.Add("888888");//密码

            Parameters.Add(2052);
            httpClient.Content = JsonConvert.SerializeObject(Parameters);
            return httpClient.SysncRequest();
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string Save(string formId, string content)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.Url = string.Concat(CloudUrl, "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save.common.kdsvc");

            List<object> Parameters = new List<object>();
            //业务对象Id 
            Parameters.Add(formId);
            //Json字串
            Parameters.Add(content);
            httpClient.Content = JsonConvert.SerializeObject(Parameters);
            return httpClient.SysncRequest();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string Delete(string formId, string content)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.Url = string.Concat(CloudUrl, "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Delete.common.kdsvc");

            List<object> Parameters = new List<object>();
            //业务对象Id 
            Parameters.Add(formId);
            //Json字串
            Parameters.Add(content);
            httpClient.Content = JsonConvert.SerializeObject(Parameters);
            return httpClient.SysncRequest();
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string Audit(string formId, string content)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.Url = string.Concat(CloudUrl, "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Audit.common.kdsvc");

            List<object> Parameters = new List<object>();
            //业务对象Id 
            Parameters.Add(formId);
            //Json字串
            Parameters.Add(content);
            httpClient.Content = JsonConvert.SerializeObject(Parameters);
            return httpClient.SysncRequest();
        }

        /// <summary>
        /// 自定义
        /// </summary>
        /// <param name="key">自定义方法标识</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public static string AbstractWebApiBusinessService(string key, List<object> args)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.Url = string.Concat(CloudUrl, key, ".common.kdsvc");

            httpClient.Content = JsonConvert.SerializeObject(args);
            return httpClient.SysncRequest();
        }
    }
}

