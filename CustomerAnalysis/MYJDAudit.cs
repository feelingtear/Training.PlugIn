using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Core.DefaultValueService;
using Kingdee.BOS.App.Core.PlugInProxy;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Enums;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.Operation;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.BD;
using Kingdee.K3.MFG.App;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.BOS.Resource;
//using Kingdee.BOS.Resource;
//using Kingdee.K3.BD.Contracts;

namespace JCXD.K3.MY.APP.ServicePlugin.MYJD
{
    public class MYJDAudit : AbstractOperationServicePlugIn
    {
        #region 执行校对
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            var validator = new UnAuditValidator
            {
                AlwaysValidate = true,
                EntityKey = "FBillHead"

            };

            e.Validators.Add(validator);
        }
        #endregion
        #region 定义效验器
        /// <summary>
        /// 当前操作的校验器
        /// </summary>
        private class UnAuditValidator : AbstractValidator
        {
            public override void Validate(Kingdee.BOS.Core.ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Kingdee.BOS.Context ctx)
            {
                if (!dataEntities.IsNullOrEmpty() && (dataEntities.Length != 0))
                {

                    //DynamicObjectCollection objects = null;
                    MaterialKeyParameter materialKey = new MaterialKeyParameter();

                    foreach (var entity in dataEntities)
                    {
                        DynamicObject o = entity.DataEntity;
                        DynamicObjectCollection objectc = o["FEntity"] as DynamicObjectCollection;
                        long orgId = (long)o["F_BBC_OrgId_ID"];//组织
                        for (int j = 0; j < objectc.Count; j++)
                        {
                            DynamicObject rowObj = objectc[j];//行对象
                            if (!rowObj["FMATERIALID_Id"].IsNullOrEmpty() && Convert.ToInt32(rowObj["FMATERIALID_Id"]) == 0)
                            {//需自动生成物料
                                string groupNumber = rowObj["FGroupNumber"].ToString();//组别代码
                                materialKey.Number = groupNumber;
                                materialKey.CreatorOrgId = orgId;
                                bool existGroup = false;
                                existGroup = this.IsExistsGroupMaterialNumber(this.Context, materialKey);
                                if (!existGroup)
                                {
                                    validateContext.AddError(null, new ValidationErrorInfo("", Convert.ToString(entity["ID"]), entity.DataEntityIndex, 0, "BBC_WSSALE", string.Format(ResManager.LoadKDString("单据编号为{0}的接单，第{1}行，将生成的物料没有对应的上级组{2}!请先添加上级组后再审核！", "005129030001114", SubSystemType.SCM, new object[0]), entity["BillNo"], j + 1, groupNumber), "", ErrorLevel.Error));
                                }
                            }
                        }
                    }

                }

            }
            private bool IsExistsMaterialNumber(Context ctx, MaterialKeyParameter materialKey)
            {

                IDBService service = ServiceHelper.GetService<IDBService>();
                string strSql = string.Empty;
                strSql = string.Format("SELECT FMaterialId,FNumber FROM T_BD_MATERIAL WHERE FNUMBER='{0}' AND FCreateOrgId= {1} ", materialKey.Number, materialKey.CreatorOrgId);
                var ret = service.ExecuteDynamicObject(this.Context, strSql);
                if (ret.Count > 0)
                {
                    return true;
                }
                return false;
            }
            private bool IsExistsGroupMaterialNumber(Context ctx, MaterialKeyParameter materialKey)
            {

                IDBService service = ServiceHelper.GetService<IDBService>();
                string strSql = string.Empty;
                strSql = string.Format("SELECT 1 FROM T_BD_MATERIALGROUP WHERE FNUMBER='{0}' ", materialKey.Number);
                var ret = service.ExecuteDynamicObject(this.Context, strSql);
                if (ret.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }
        #endregion

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

                        this.updateSubEntity(o,objectMaterial, subEntryData, rowObj, false, true);

                        //opResult = setBom(objectMaterial, orgId, o);//生成BOM（指定物料）

                        //if (CheckOpResult(opResult) == false) return;//回滚事务
                    
                    }
                    else //接的是标门或配件
                    {
                        DynamicObject materialObj = rowObj["FMATERIALID"] as DynamicObject;
                        DynamicObject[] objectMaterial = new DynamicObject[] { materialObj };
                        //DynamicObject[] objectMaterial = this.zyMaterial(materialObj, orgId);
                        //更新子单据体数据                         
                        this.updateSubEntity(o,objectMaterial, subEntryData, rowObj, true, false);
                    }

                    ServiceFactory.GetDBService(this.Context).AutoSetPrimaryKey(this.Context, subEntryData.ToArray(), metaDataBill.BusinessInfo.GetEntity("FSubEntity").DynamicObjectType);

                }


            }
        }

        #region 自动生成物料
        //第一个参数 O 单据头信息 第二个参数 ROWOBJ 单据体信息 第三个参数 系统参数创建组织
        private DynamicObject[] AutoMaterial(DynamicObject o, DynamicObject rowObj, long orgId)
        {
            IMetaDataService service = ServiceFactory.GetMetaDataService(this.Context);
            FormMetadata metaData = (FormMetadata)service.Load(this.Context, "BD_MATERIAL");

            List<DynamicObject> materObj = new List<DynamicObject>();

            DynamicObject zMATERIAL = new DynamicObject(metaData.BusinessInfo.GetDynamicObjectType());
            DynamicObject yMATERIAL = new DynamicObject(metaData.BusinessInfo.GetDynamicObjectType());

            //string custgroup = o["FCustText"].ToString();//客户组别
            DynamicObject objcust = o["FCust"] as DynamicObject;
            string custgroup = objcust["Number"].ToString();
            if (custgroup.Length > 1)
            {
                custgroup = custgroup.Substring(0, 4);
            }//客户一级码

            if (Convert.ToDecimal(rowObj["FQtyL"]) > 0)
            {
                zMATERIAL = SetMaterial(o, rowObj, orgId, Convert.ToString(o["BillNo"]), "L", "左", custgroup);
                materObj.Add(zMATERIAL);
            }
            if (Convert.ToDecimal(rowObj["FQtyR"]) > 0)
            {
                yMATERIAL = SetMaterial(o, rowObj, orgId, Convert.ToString(o["BillNo"]), "R", "右", custgroup);
                materObj.Add(yMATERIAL);
            }

            return materObj.ToArray();
        }
        #endregion
        #region 设置物料Object
        //rowObj 行信息 orgId 创建组织 billno单据号 number 物料编码 name 物料名称 unit 单位
        private DynamicObject SetMaterial(DynamicObject o, DynamicObject rowObj, long orgID, string billno, string number, string name, string custgroup)
        {

            IMetaDataService service = ServiceFactory.GetMetaDataService(this.Context);
            FormMetadata metaData = (FormMetadata)service.Load(this.Context, "BD_MATERIAL");

            DynamicFormModelProxy materialModelProxy = new DynamicFormModelProxy();
            FormServiceProvider provider = new FormServiceProvider();
            provider.Add(typeof(IDefaultValueCalculator), new DefaultValueCalculator());
            materialModelProxy.SetContext(Context, metaData.BusinessInfo, provider);
            materialModelProxy.CreateNewData();
            materialModelProxy.BeginIniti();
            materialModelProxy.EndIniti();
            //materialModelProxy.ClearNoDataRow();
            materialModelProxy.SetValue("FCreateOrgId", orgID);
            materialModelProxy.SetValue("FUseOrgId", orgID);
            string fnumber;
            string fname;
            object unit = rowObj["FUnitID_ID"];
            //DynamicObject materialGroup = ((DynamicObject)rowObj["FDL"])["FMATERIALGROUP"] as DynamicObject;

            DynamicObject fobjbilltype = o["FBillTypeID"] as DynamicObject;//订单类型
            string fbilltypename = getName(fobjbilltype);

            if (fbilltypename.IndexOf("钢质") >= 0)
            {
                fnumber = rowObj["FNumber"].ToString() + "." + billno + number;
            }
            else
            {
                fnumber = rowObj["FNumber"].ToString() + "." + billno + Convert.ToString(rowObj["Seq"]).PadLeft(2, '0') + number;//多了行号
            }

            fnumber = fnumber + "-" + custgroup;

            fname = rowObj["FPRONAME"].ToString() + name + ")";

            string fnote = getName(rowObj["FRemarks"]);//备注

            string specification = Convert.ToSingle(rowObj["FSizeHeight"]).ToString() + "*" + Convert.ToSingle(rowObj["FSizeWidth"]).ToString();

            if (specification.Equals("0*0"))
                specification = "";

            materialModelProxy.SetValue("FNumber", fnumber);
            materialModelProxy.SetValue("FName", fname);
            materialModelProxy.SetValue("FSpecification", specification + name + fnote);

            materialModelProxy.SetValue("FCategoryID", Convert.ToInt32(241));//存货类别，默认241
            DynamicObject FCplmObj = rowObj["FCplm"] as DynamicObject;//产品类目
            if (!FCplmObj.IsNullOrEmpty())
            {
                DynamicObject FCategory = FCplmObj["FCHType"] as DynamicObject;//存货类别
                if (!FCategory.IsNullOrEmpty())
                {
                    materialModelProxy.SetValue("FCategoryID", FCategory);//取产品类目属性-存货类别
                }
            }
            materialModelProxy.SetValue("FErpClsID", Convert.ToInt32(2));
            materialModelProxy.SetValue("FIsProduce", true);   //允许生产
            materialModelProxy.SetValue("FIsPurchase", true);   //允许采购
            //materialModelProxy.SetValue("FPlanningStrategy", Convert.ToInt32(2));   //计划策略
            //materialModelProxy.SetValue("FMfgPolicyId", Convert.ToInt32(0));   //制造策略
            materialModelProxy.SetValue("FBaseUnitId", unit);
            materialModelProxy.SetValue("FStoreUnitID", unit);
            materialModelProxy.SetValue("FProduceUnitId", unit);   //生产单位
            materialModelProxy.SetValue("FPurchaseUnitId", unit);
            materialModelProxy.SetValue("FPurchasePriceUnitId", unit);
            materialModelProxy.SetValue("FSubconUnitId", unit);   //委外单位
            materialModelProxy.SetValue("FSubconPriceUnitId", unit);   //委外计价单位
            materialModelProxy.SetValue("FSaleUnitId", unit);
            materialModelProxy.SetValue("FSalePriceUnitId", unit);
            //materialModelProxy.SetValue("FVOLUMEUNITID", this.GetDynamicObject("BD_UNIT", "FNUMBER = 'mm'"));
            materialModelProxy.SetValue("FMinIssueUnitId", unit);//最小发料批量单位
            materialModelProxy.SetValue("FCurrencyId", o["FLocalCurrId"]);
            materialModelProxy.SetValue("FDocumentStatus", Convert.ToString("Z"));
            materialModelProxy.SetValue("FForbidStatus", Convert.ToString("A"));
            materialModelProxy.SetValue("FZY", number);

            materialModelProxy.SetValue("FHTNO", o["FHTNO"]);
            // materialModelProxy.SetValue("FMATERIALGROUP", o["FGroup"]);
            materialModelProxy.SetItemValueByNumber("FMATERIALGROUP", rowObj["FGroupNumber"].ToString(), 0);

            materialModelProxy.SetValue("FIsBatchManage", true);   //启用批号管理
            materialModelProxy.SetValue("FLENGTH", rowObj["FSizeHeight"]); //规格高（长）
            materialModelProxy.SetValue("FWIDTH", rowObj["FSizeWidth"]); //规格宽


            //DynamicObject FSCDeptID = rowObj["FSCDeptID"] as DynamicObject;//生产车间
            //if (!FSCDeptID.IsNullOrEmpty())
            //{
            //    materialModelProxy.SetValue("FWorkShopId", FSCDeptID);
            //}

            DynamicObject FOEMObj = rowObj["FOEM"] as DynamicObject;//OEM
            if (!FOEMObj.IsNullOrEmpty())
            {
                DynamicObject FSCDeptID2 = FOEMObj["FDeptID"] as DynamicObject;//生产车间
                if (!FSCDeptID2.IsNullOrEmpty())
                {
                    long fdeptid = Convert.ToInt32(FSCDeptID2["Id"].ToString());
                    IMetaDataService service3 = ServiceFactory.GetMetaDataService(this.Context);
                    FormMetadata Dept = (FormMetadata)service3.Load(this.Context, "BD_Department");//获取车间元对象
                    IViewService view3 = ServiceFactory.GetViewService(this.Context);
                    DynamicObject FSCDeptID3 = view3.LoadSingle(this.Context, fdeptid, Dept.BusinessInfo.GetDynamicObjectType());
                    materialModelProxy.SetValue("FWorkShopId", FSCDeptID3);
                }
            }
            //库存属性控制 T_BD_INVPROPERTY
            object[] auxPtyIds = new object[] { Convert.ToInt32(10001), Convert.ToInt32(10002), Convert.ToInt32(10003), Convert.ToInt32(10004), Convert.ToInt32(10006) };

            materialModelProxy.BatchCreateNewEntryRow("FEntityInvPty", (auxPtyIds.Length - 1));
            for (int iEntry = 0; iEntry < auxPtyIds.Length; iEntry++)
            {

                materialModelProxy.SetValue("FInvPtyId", auxPtyIds[iEntry], iEntry);
                if (iEntry ==2 || iEntry == 4)
                {
                    materialModelProxy.SetValue("FIsEnable", false, iEntry);

                }
                else
                {
                    materialModelProxy.SetValue("FIsEnable", true, iEntry);
                }
                materialModelProxy.SetValue("FIsAffectPrice", false, iEntry);
                materialModelProxy.SetValue("FIsAffectPlan", false, iEntry);
                materialModelProxy.SetValue("FIsAffectCost", false, iEntry);

            }

            //门业属性
            foreach (Entity entity in metaData.BusinessInfo.Entrys)
            {
                if (entity.Key == "F_BBC_SubHeadEntity")
                {
                    foreach (Field field in entity.Fields)
                    {
                        //if (field.GetType().Name.Equals("BaseDataField") || field.GetType().Name.Equals("TextField"))//基础资料或文本
                        if (!field.GetType().Name.Equals("GropTitleField"))
                        {
                           
                            materialModelProxy.SetValue(field.Key, rowObj[field.Key]);

                        }
                    }
                }
            }
            DynamicObject FMJGObj = rowObj["FDoorjg"] as DynamicObject;//门结构
            string FMJGName = getName(FMJGObj);
            if (FMJGName.Equals("单门"))
            {
                if (name.Equals("左"))
                {
                    materialModelProxy.SetValue("FLashou", rowObj["FLashou"]);//拉手里填入左拉手
                    materialModelProxy.SetValue("FLashouR", 0);
                }
                else
                {
                    materialModelProxy.SetValue("FLashou", rowObj["FLashouR"]);////拉手里填入右拉手
                    materialModelProxy.SetValue("FLashouR", 0);
                }
            }
            //materialModelProxy.SetValue("FDALEI", rowObj["FDL_ID"]);

            return materialModelProxy.DataObject;
        }
        #endregion
        #region 生成物料提交审核
        private void submitAndCheck(DynamicObject[] obj, string formid)
        {
            IMetaDataService metaDataService = ServiceFactory.GetMetaDataService(this.Context);//获取元模型服务
            FormMetadata formMeta = metaDataService.Load(this.Context, formid) as FormMetadata;//通过元模型服务获取物料元模型
            ISubmitService submitBaseService = ServiceFactory.GetSubmitService(this.Context);
            ISetStatusService setBaseService = ServiceFactory.GetSetStatusService(this.Context);
            // 提交
            OperationResult submicResult = submitBaseService.Submit(this.Context, formMeta.BusinessInfo, obj.Select(p => p["Id"]).ToArray(), "Submit", null) as OperationResult;
            if (!submicResult.IsSuccess)
            {
                this.OperationResult.IsSuccess = false;
                this.OperationResult.MergeResult(submicResult);
            }
            else
            {
                // 审核
                List<object> paraAudit = new List<object>();
                //审核通过
                paraAudit.Add("1");
                //审核意见
                paraAudit.Add("");
                List<KeyValuePair<object, object>> pkIds = new List<KeyValuePair<object, object>>();
                foreach (var suc in submicResult.SuccessDataEnity)
                {
                    pkIds.Add(new KeyValuePair<object, object>(suc["Id"], ""));
                }
                setBaseService.SetBillStatus(this.Context, formMeta.BusinessInfo, pkIds, paraAudit, "Audit", null);
            }
        }
        #endregion
        #region 更新子单据体
        private void updateSubEntity(DynamicObject o,DynamicObject[] objectMaterial, DynamicObjectCollection subEntryData, DynamicObject rowObj, bool isBM, bool isAuto = false)
        {
            DynamicObject fobjbilltype = o["FBillTypeID"] as DynamicObject;//订单类型
            string fbilltypename = getName(fobjbilltype);

            
            for (int i = 0; i < objectMaterial.Length; i++)
            {
                DynamicObject itemMX = new DynamicObject(subEntryData.DynamicCollectionItemPropertyType);
                DynamicObjectCollection baseMaterial = objectMaterial[i]["MaterialBase"] as DynamicObjectCollection;
               DynamicObjectCollection saleMaterial = objectMaterial[i]["MaterialSale"] as DynamicObjectCollection;
                itemMX["FMATERIAL_Id"] = objectMaterial[i]["Id"];
                itemMX["FSaleUnitId_Id"] = saleMaterial[0]["SaleUnitId_Id"];
                itemMX["FBaseUnitId_Id"] = baseMaterial[0]["BaseUnitId_Id"];
                itemMX["FISAUTO"] = isAuto;
                if (objectMaterial[i]["FZY"].ToString() == "L")
                {
                    itemMX["FQty"] = rowObj["FQtyL"];
                    itemMX["FBASEUNITQTY"] = rowObj["FQtyL"];
                }
                else if (objectMaterial[i]["FZY"].ToString() == "R")
                {
                    itemMX["FQty"] = rowObj["FQtyR"];
                    itemMX["FBASEUNITQTY"] = rowObj["FQtyR"];
                }
                else
                {
                    itemMX["FQty"] = Convert.ToInt32(rowObj["FQtyL"]) + Convert.ToInt32(rowObj["FQtyR"]);
                    itemMX["FBASEUNITQTY"] = Convert.ToInt32(rowObj["FQtyL"]) + Convert.ToInt32(rowObj["FQtyR"]);
                }

                itemMX["FSALEPRICE"] = rowObj["FPrice"];
                itemMX["FSALEAmount"] = Convert.ToDouble(itemMX["FQty"]) * Convert.ToDouble(itemMX["FSALEPRICE"]);

                //if (fbilltypename.IndexOf("钢质") >= 0)//增加区分码，非标左右门相同
                //{
                    itemMX["FDiffCode"] = Convert.ToString(o["BillNo"]) + Convert.ToString(rowObj["Seq"]).PadLeft(2, '0');
               // }

                if (Convert.ToDouble(itemMX["FQty"]) > 0)
                    subEntryData.Add(itemMX);
            }
        }
        #endregion
        #region 生成BOM
        private OperationResult setBom(DynamicObject[] objectMaterial, long orgID, DynamicObject o)
        {
            IMetaDataService service = ServiceFactory.GetMetaDataService(this.Context);
            FormMetadata bommetaData = (FormMetadata)service.Load(this.Context, "ENG_BOM");

            List<DynamicObject> bomObj = new List<DynamicObject>();

            for (int i = 0; i < objectMaterial.Length; i++)
            {
                //IMetaDataService metaDataService = Kingdee.BOS.Contracts.ServiceFactory.GetMetaDataService(this.Context);//获取元模型服务
                //FormMetadata metaData = metaDataService.Load(this.Context, "ENG_BOM") as FormMetadata;//通过元模型服务获取BOM元模型

                DynamicFormModelProxy bomModelProxy = new DynamicFormModelProxy();
                FormServiceProvider provider = new FormServiceProvider();
                provider.Add(typeof(IDefaultValueCalculator), new DefaultValueCalculator());

                long materialObjId;
                //long childObjId;
                //long childBomId;
                long fgroupid;
                string bomname;

                materialObjId = (long)objectMaterial[i]["Id"];
                //childObjId = 137882;//50.01.01.01 铝合金锭

                fgroupid = 137779;// 293134;//BOM组别-
                bomname = objectMaterial[i]["Number"].ToString();

                bomModelProxy.SetContext(Context, bommetaData.BusinessInfo, provider);
                bomModelProxy.CreateNewData();
                bomModelProxy.BeginIniti();
                bomModelProxy.EndIniti();

                bomModelProxy.SetValue("FCreateOrgId", orgID);
                bomModelProxy.SetValue("FUseOrgId", orgID);
                bomModelProxy.SetValue("FJDBillno", Convert.ToString(o["BillNo"]));//接单编码

                bomModelProxy.SetValue("FMATERIALID", materialObjId);
                bomModelProxy.SetItemValueByNumber("FUNITID", "tang", 0);//FBaseUnitId
                bomModelProxy.SetItemValueByNumber("FBaseUnitId", "tang", 0);
                //bomModelProxy.SetValue("FUNITID", rowobj["FUnitID"]);
                bomModelProxy.SetValue("FGroup", fgroupid);//BOM组别
                bomModelProxy.SetValue("FName", bomname);//BOM简称

                bomModelProxy.SetValue("FRowId", SequentialGuid.NewGuid().ToString(), 0);
                bomModelProxy.SetValue("FReplaceGroup", 1, 0);
                bomModelProxy.SetItemValueByNumber("FMATERIALIDCHILD", "01.999", 0);//bom物料
                //bomModelProxy.SetValue("FMATERIALIDCHILD", childObjId, 0);
                bomModelProxy.SetValue("FNUMERATOR", 1, 0);//分子
                bomModelProxy.SetValue("FDENOMINATOR", 1, 0);//分母
                bomModelProxy.SetItemValueByNumber("FCHILDUNITID", "Pcs", 0);
                bomModelProxy.SetItemValueByNumber("FChildBaseUnitID", "Pcs", 0);
                //bomModelProxy.SetValue("FCHILDUNITID", 10095, 0);//千克
                //bomModelProxy.SetValue("FChildBaseUnitID", 10095, 0);//千克
                bomModelProxy.SetValue("FBaseNumerator", 1, 0);
                bomModelProxy.SetValue("FBaseDenominator", 1, 0);
                bomModelProxy.SetValue("FChildSupplyOrgId", orgID, 0);//供应组织

                // bomModelProxy.SetValue("FBOMID", childBomId, 0);//子项物料版本
                bomModelProxy.SetValue("FISSUETYPE", Convert.ToInt32(7), 0);//发料方式，2直接倒冲,7不发料
                //bomModelProxy.SetValue("FBACKFLUSHTYPE", Convert.ToInt32(3), 0);//倒冲时机，入库倒冲
                bomModelProxy.SetValue("FSUPPLYORG", orgID, 0);//发料组织，
                //bomModelProxy.SetValue("FSTOCKID", stockID, 0);//默认仓库
                bomModelProxy.SetValue("FOWNERID", orgID, 0);//货主，

                bomObj.Add(bomModelProxy.DataObject);
            }
            OperationResult opResult = new OperationResult();//操作结果     
            ISaveService saveService = Kingdee.BOS.Contracts.ServiceFactory.GetSaveService(this.Context);
            opResult = saveService.Save(this.Context, bommetaData.BusinessInfo, bomObj.ToArray()) as OperationResult;
            
            if (opResult.IsSuccess)
            {
                submitAndCheck(opResult.SuccessDataEnity.ToArray(), "ENG_BOM");

            }
            return opResult;
        }

        #endregion
        #region 操作结束后生成销售订单
                public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {

            base.AfterExecuteOperationTransaction(e);
            if (!e.DataEntitys.IsEmpty<DynamicObject>())
            {

                //获取源单与目标单直接的转换规则，如果规则未启用，则返回
                IConvertService convertService = ServiceFactory.GetConvertService(this.Context);
                var rules = convertService.GetConvertRules(this.Context, "BBC_WSSALE", "SAL_SaleOrder");
                if (rules.IsEmpty()) return;
                //获取源单源数据
                IMetaDataService metaDataService = ServiceFactory.GetMetaDataService(this.Context);
                string strFormId = "BBC_WSSALE";

                foreach (DynamicObject data in e.DataEntitys) //选择多行审核循环
                {
                    //获取当前单据上的单据体需下推的行
                    //data = e.DataEntitys.FirstOrDefault();
                    List<ListSelectedRow> rows = new List<ListSelectedRow>();
                    DynamicObjectCollection collections = data["FEntity"] as DynamicObjectCollection; //产品信息
                    ArrayList entryId = new ArrayList();
                    int rowIndex = 0;
                    foreach (var entryRowData in collections)
                    {
                        ListSelectedRow row = new ListSelectedRow(data["Id"].ToString(), entryRowData["Id"].ToString(), rowIndex, strFormId);
                        rows.Add(row);
                        entryId.Add(entryRowData["Id"]);
                    }
                    //调用下推服务，生成下游单据数据包
                    PushArgs pushArgs = new PushArgs(rules.FirstOrDefault(t => t.IsDefault), rows.ToArray());
                    pushArgs.TargetBillTypeId = "eacb50844fc84a10b03d7b841f3a6278";//目标单单据类型id
                    pushArgs.TargetOrgId = Convert.ToInt64(data["F_BBC_OrgId_ID"]);

                    //执行下推操作，并获取下推结果
                    ConvertOperationResult operationResult = convertService.Push(this.Context, pushArgs, OperateOption.Create());
                    // 获取生成的目标单据数据包
                    DynamicObject[] objs = (from p in operationResult.TargetDataEntities select p.DataEntity).ToArray();

                    // 读取目标单据元数据
                    var targetBillMeta = metaDataService.Load(this.Context, "SAL_SaleOrder") as FormMetadata;
                    BusinessInfo targetInfo = targetBillMeta.BusinessInfo;
                    // 提交数据库保存，并获取保存结果
                    ISaveService saveService = ServiceFactory.GetSaveService(this.Context);
                    //for (int i1 = 0; i1 < objs.Length; i1++)
                    //{
                    //    objs[i1]["DocumentStatus"] = Convert.ToString("C");
                    //}
                    OperationResult saveResult = saveService.Save(this.Context, targetInfo, objs) as OperationResult;
                    if (!saveResult.IsSuccess)
                    {
                        this.OperationResult.IsSuccess = false;
                        this.OperationResult.MergeResult(saveResult);
                    }
                    //else
                    //{
                        //submitAndCheck(objs, "SAL_SaleOrder");

                        //this.AfterSaveWriteBackData(entryId);
                    //}
                }

            }
         
        }
        
        #endregion
        #region 获取左右物料集合
        private DynamicObject[] zyMaterial(DynamicObject materialObj, long orgId)
        {
            string materialNumber = materialObj["Number"].ToString();
            IMetaDataService metaDataService = ServiceFactory.GetMetaDataService(this.Context);//获取元模型服务
            FormMetadata materialMetaData = metaDataService.Load(this.Context, "BD_MATERIAL") as FormMetadata;//通过元模型服务获取物料元模型
            int i = 0;
            if (materialObj["FZY"].ToString() == "L")
                i = materialNumber.IndexOf("L");
            else if (materialObj["FZY"].ToString() == "R")
                i = materialNumber.IndexOf("R");
            else
                i = materialNumber.Length;
            IViewService materialViewSvc = ServiceFactory.GetViewService(Context);
            QueryBuilderParemeter p = new QueryBuilderParemeter
            {
                FormId = "BD_MATERIAL",
                SelectItems = SelectorItemInfo.CreateItems("FMATERIALID", "FNumber"),
                FilterClauseWihtKey = string.Format(" FNumber like '{0}%' and FUSEORGID ={1}", materialNumber.Substring(0, i), orgId)
            };
            return materialViewSvc.Load(this.Context, materialMetaData.BusinessInfo.GetDynamicObjectType(), p);
        }
        #endregion
        #region 获取左右物料个体
        private DynamicObject zyMaterial(DynamicObject materialObj, long orgId, string zy)
        {
            string materialNumber = materialObj["Number"].ToString();
            string whereStr = "";
            IMetaDataService metaDataService = ServiceFactory.GetMetaDataService(this.Context);//获取元模型服务
            FormMetadata materialMetaData = metaDataService.Load(this.Context, "BD_MATERIAL") as FormMetadata;//通过元模型服务获取物料元模型
            int i = 0;
            if (materialObj["FZY"].ToString() == "L")
            {
                i = materialNumber.IndexOf("L");
                whereStr = string.Format("AND  FZY ='{0}' ", zy);
            }
            else if (materialObj["FZY"].ToString() == "R")
            {
                i = materialNumber.IndexOf("R");
                whereStr = string.Format("AND  FZY ='{0}' ", zy);
            }
            else
                i = materialNumber.Length;
            IViewService materialViewSvc = ServiceFactory.GetViewService(Context);
            QueryBuilderParemeter p = new QueryBuilderParemeter
            {
                FormId = "BD_MATERIAL",
                SelectItems = SelectorItemInfo.CreateItems("FMATERIALID", "FNumber"),
                FilterClauseWihtKey = string.Format("  FNumber like '{0}%' and FUSEORGID ={1} {2}", materialNumber.Substring(0, i), orgId, whereStr)
            };
            DynamicObject[] materialobjs = materialViewSvc.Load(this.Context, materialMetaData.BusinessInfo.GetDynamicObjectType(), p);
            DynamicObject returnMaterial = materialobjs.SingleOrDefault();
            return returnMaterial;
        }
        #endregion
        #region 获取对象
        private DynamicObject GetDynamicObject(string formid, string strfilter)
        {
            //服务器插件获取元数据模型用ServiceFactory
            IMetaDataService metaService = ServiceFactory.GetMetaDataService(this.Context);
            FormMetadata metaData = (FormMetadata)metaService.Load(this.Context, formid);
            IViewService view = ServiceFactory.GetViewService(this.Context);
            QueryBuilderParemeter queryParameter = new QueryBuilderParemeter();
            queryParameter.FormId = formid;
            queryParameter.FilterClauseWihtKey = strfilter;
            DynamicObject[] objs = view.Load(this.Context, metaData.BusinessInfo.GetDynamicObjectType(), queryParameter);
            return objs.SingleOrDefault();
        }
        #endregion

        #region 操作服务数据准备
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            //单据头F_BBC_OrgId
            e.FieldKeys.Add("F_BBC_OrgId");
            e.FieldKeys.Add("FLocalCurrId");
            e.FieldKeys.Add("FGroup");
            e.FieldKeys.Add("FCustText");
            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("FCust");

            //单据体
            e.FieldKeys.Add("FMATERIALID");
            e.FieldKeys.Add("FGroupNumber");
            e.FieldKeys.Add("FNumber");
            e.FieldKeys.Add("FPRONAME");
            e.FieldKeys.Add("FXMQty");
            //e.FieldKeys.Add("FNote");
            e.FieldKeys.Add("FUnitID");
            e.FieldKeys.Add("FPrice");
            e.FieldKeys.Add("FRemarks");
            e.FieldKeys.Add("FQtyL");
            e.FieldKeys.Add("FQtyR");
            e.FieldKeys.Add("FSCDeptID");
            e.FieldKeys.Add("FDeptID");
            e.FieldKeys.Add("FDiffCode");
            e.FieldKeys.Add("FHTNO");
            IMetaDataService service = ServiceFactory.GetMetaDataService(this.Context);
            FormMetadata metaData = (FormMetadata)service.Load(this.Context, "BD_MATERIAL");
            foreach (Entity entity in metaData.BusinessInfo.Entrys)
            {
                if (entity.Key == "F_BBC_SubHeadEntity")
                { 
                    foreach (Field field in entity.Fields)
                    {
                        e.FieldKeys.Add(field.Key);
                    }
                }
            }


            //子单据体
            e.FieldKeys.Add("FMaterial");
            e.FieldKeys.Add("FSaleUnitID");
            e.FieldKeys.Add("FBaseUnitID");
            e.FieldKeys.Add("FISAUTO");
            e.FieldKeys.Add("FQty");
            e.FieldKeys.Add("FBASEUNITQTY");
            e.FieldKeys.Add("FSALEPRICE");
            e.FieldKeys.Add("FSALEAmount");

            base.OnPreparePropertys(e);

        }
        #endregion
        private string getName(Object obj)
        {
            if (!obj.IsNullOrEmpty())
                return obj.ToString();
            else
                return "";
        }
        private string getName(DynamicObject obj)
        {
            if (!obj.IsNullOrEmpty())
                return obj["Name"].ToString();
            else
                return "";
        }
        private long getID(DynamicObject obj)
        {
            if (!obj.IsNullOrEmpty())
                return Convert.ToInt32(obj["Id"].ToString());
            else
                return 0;
        }

        private bool CheckOpResult(IOperationResult opResult)
        {
            bool isSuccess = false;
            if (opResult.IsSuccess == true)
            {
                // 操作成功
                isSuccess = true;
            }
            else
            {
                if (opResult.InteractionContext != null
                    && opResult.InteractionContext.Option.GetInteractionFlag().Count > 0)
                {// 有交互性提示

                    // 传出交互提示完整信息对象
                    this.OperationResult.InteractionContext = opResult.InteractionContext;
                    // 传出本次交互的标识，
                    // 用户在确认继续后，会重新进入操作；
                    // 将以此标识取本交互是否已经确认过，避免重复交互
                    this.OperationResult.Sponsor = opResult.Sponsor;

                    // 抛出错误，终止本次操作
                    throw new KDBusinessException("", "本次操作需要用户确认是否继续，暂时中断");
                }
                else
                {
                    // 操作失败，拼接失败原因，然后抛出中断
                    opResult.MergeValidateErrors();
                    if (opResult.OperateResult == null)
                    {// 未知原因导致提交失败
                        throw new KDBusinessException("", "未知原因导致审核失败！");
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("审核失败，失败原因：");
                        foreach (var operateResult in opResult.OperateResult)
                        {
                            sb.AppendLine(operateResult.Message);
                        }
                        throw new KDBusinessException("", sb.ToString());
                    }
                }
            }
            return isSuccess;
        }
    }
}
