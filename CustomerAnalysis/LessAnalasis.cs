using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
//using Kingdee.BOS.Resource;
//using Kingdee.K3.BD.Contracts;

namespace JCXD.K3.MY.APP.ServicePlugin.MYJD
{
    public class LessMYJDAudit : AbstractOperationServicePlugIn
    {

        //重点嫌疑对象

        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            IMetaDataService serviceBill = Kingdee.BOS.Contracts.ServiceFactory.GetMetaDataService(this.Context);
            FormMetadata metaDataBill = (FormMetadata)serviceBill.Load(this.Context, "BBC_WSSALE");
            foreach (DynamicObject o in e.DataEntitys) //选择多行审核循环，o为单据对象
            {
                long orgId = (long)o["F_BBC_OrgId_ID"];//组织
                DynamicObjectCollection objects = o["FEntity"] as DynamicObjectCollection;

                for (int j = 0; j < objects.Count; j++)
                {
                    DynamicObject rowObj = objects[j];
                    IMetaDataService metaDataService = ServiceFactory.GetMetaDataService(this.Context);//获取元模型服务
                    FormMetadata materialMetaData = metaDataService.Load(this.Context, "BD_MATERIAL") as FormMetadata;//通过元模型服务获取物料元模型
                    DynamicObjectCollection subEntryData = (DynamicObjectCollection)rowObj["productDetail"];//子单据体productDetail
                    subEntryData.Clear();
                    if (!rowObj["FMATERIALID_Id"].IsNullOrEmpty() && Convert.ToInt32(rowObj["FMATERIALID_Id"]) == 0)
                    {
                        //自动生成物料
                        DynamicObject[] obj;

                        obj = this.AutoMaterial(o, rowObj, orgId);

                        OperationService opService = new OperationService();//获取操作服务 
                        OperationResult opResult = new OperationResult();//操作结果 
                        opResult = opService.Validate(this.Context, materialMetaData.BusinessInfo, obj, "Save") as OperationResult;//通过调用物料保存检验服务获取结果


                        if (CheckOpResult(opResult) == false) return;//回滚事务

                        //获取保存服务
                        ISaveService saveService = ServiceFactory.GetSaveService(this.Context);
                        DynamicObject[] objectMaterial = saveService.Save(this.Context, obj);
                        submitAndCheck(objectMaterial, "BD_MATERIAL");

                        //更新子单据体数据 

                        this.updateSubEntity(o, objectMaterial, subEntryData, rowObj, false, true);

                        //opResult = setBom(objectMaterial, orgId, o);//生成BOM（指定物料）

                        //if (CheckOpResult(opResult) == false) return;//回滚事务

                    }
                    else //接的是标门或配件
                    {
                        DynamicObject materialObj = rowObj["FMATERIALID"] as DynamicObject;
                        DynamicObject[] objectMaterial = new DynamicObject[] { materialObj };
                        //DynamicObject[] objectMaterial = this.zyMaterial(materialObj, orgId);
                        //更新子单据体数据                         
                        this.updateSubEntity(o, objectMaterial, subEntryData, rowObj, true, false);
                    }

                    ServiceFactory.GetDBService(this.Context).AutoSetPrimaryKey(this.Context, subEntryData.ToArray(), metaDataBill.BusinessInfo.GetEntity("FSubEntity").DynamicObjectType);

                }


            }
        }

        #region 自动生成物料
        //第一个参数 O 单据头信息 第二个参数 ROWOBJ 单据体信息 第三个参数 系统参数创建组织
        private DynamicObject[] AutoMaterial(DynamicObject o, DynamicObject rowObj, long orgId)
        {
            List<DynamicObject> materObj = new List<DynamicObject>();
            
            return materObj.ToArray();
        }
        #endregion
        #region 设置物料Object
        //rowObj 行信息 orgId 创建组织 billno单据号 number 物料编码 name 物料名称 unit 单位
        private DynamicObject SetMaterial(DynamicObject o, DynamicObject rowObj, long orgID, string billno, string number, string name, string custgroup)
        {
            return null;
        }
        #endregion
        #region 生成物料提交审核
        private void submitAndCheck(DynamicObject[] obj, string formid)
        {
            return;
        }
        #endregion
        #region 更新子单据体
        private void updateSubEntity(DynamicObject o, DynamicObject[] objectMaterial, DynamicObjectCollection subEntryData, DynamicObject rowObj, Boolean isBM, Boolean isAuto = false)
        {
            return;
        }
        #endregion
        
        #region 操作结束后生成销售订单
        

        #endregion
        #region 获取左右物料集合
        private DynamicObject[] zyMaterial(DynamicObject materialObj, long orgId)
        {
            return null;
        }
        #endregion
        #region 获取左右物料个体
        private DynamicObject zyMaterial(DynamicObject materialObj, long orgId, string zy)
        {
            return null;
        }
        #endregion
        #region 获取对象
        private DynamicObject GetDynamicObject(string formid, string strfilter)
        {
            return null;
        }
        #endregion

        #region 操作服务数据准备
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {

        }
        #endregion
        
        
    }
}
