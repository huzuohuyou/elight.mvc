using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Models.Unit;
using TestingCenterSystem.Service.Dict;
using TestingCenterSystem.Service.Dict.Interface;
using TestingCenterSystem.ViewModels.Dictionary;
using TestingCenterSystem.ViewModels.Project;

namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    [LoginChecked]
    public class MedicineController : BaseController
    {
        private static readonly IMedicineService _medicineService = new MedicineService();
        private static readonly ITermCatConfService _termcatconfService = new TermCatConfService();
        private static readonly IMedicineKWService _medicineKWService = new MedicineKWService();

        // GET: TestCenterSystem/Medicine
        public ActionResult Index()
        {
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            ViewBag.ItemType = _termcatconfService.GetSearchList(d => d.SD_ID == sdId && d.TERM_TYPE == 1)
                .Select(d => d.TERM_CAT_NAME).Distinct().ToList();
            return View();
        }

        // GET: TestCenterSystem/Medicine
        public ActionResult IndexCate()
        {
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            ViewBag.ItemType = _termcatconfService.GetSearchList(d => d.SD_ID == sdId && d.TERM_TYPE == 1).Distinct()
                .ToList();
            return View();
        }

        [HttpGet]
        public ActionResult GetList(int page, int limit, string field, string type, string keyWord = "")
        {
            var pageData = _medicineService.GetPage(page, limit, field, type, keyWord);
            var result = new Dictionary<string, object>
            {
                {"code", 0},
                {"msg", "success"},
                {"data", pageData.Items},
                {"count", pageData.TotalItems}
            };
            return Content(result.ToJson());
        }

        [HttpGet]
        public ActionResult GetListKW(int page, int limit, string field, string type, string keyWord = "")
        {
            var pageData = _medicineKWService.GetPage(page, limit, field, type, keyWord);
            var result = new Dictionary<string, object>
            {
                {"code", 0},
                {"msg", "success"},
                {"data", pageData.Items},
                {"count", pageData.TotalItems}
            };
            return Content(result.ToJson());
        }

        [HttpGet]
        public ActionResult GetListCateMain(int page, int limit, string field, string type, int termtype,
            string keyWord = "")
        {
            var pageData = _termcatconfService.GetPage(page, limit, field, type, termtype, keyWord);
            var result = new Dictionary<string, object>
            {
                {"code", 0},
                {"msg", "success"},
                {"data", pageData.Items},
                {"count", pageData.TotalItems}
            };
            return Content(result.ToJson());
        }

        [HttpGet]
        public ActionResult Form(string primaryKey, int termtype)
        {
            ViewBag.ItemType = _termcatconfService.GetTypeName("", termtype, 0, "", "");
            return View();
        }

        [HttpGet]
        public ActionResult FormKW(string primaryKeyKW, int termtype)
        {
            ViewBag.ItemType = _termcatconfService.GetTypeName("", termtype, 0, "", "");
            return View();
        }

        [HttpGet]
        public ActionResult FormCate(string primaryKeyCate, int termtype)
        {
            ViewBag.ItemType = _termcatconfService.GetTypeName("", termtype, 0, "", "");
            return View();
        }

        [HttpPost]
        public ActionResult Form(MEDICINE_VIEWMODEL model)
        {
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            var termcatconf = _termcatconfService.GetManay(d =>
                d.TERM_CAT_CODE == model.TERM_CAT_CODE && d.TERM_CAT1_CODE == model.TERM_CAT1_CODE && d.SD_ID == sdId &&
                d.TERM_TYPE == 1);
            if (model.TERM_CAT2_CODE != null && !model.TERM_CAT2_CODE.Equals("null"))
                termcatconf = termcatconf.Where(d => d.TERM_CAT2_CODE == model.TERM_CAT2_CODE).ToList();
            if (model.TERM_CAT3_CODE != null && !model.TERM_CAT3_CODE.Equals("null"))
                termcatconf = termcatconf.Where(d => d.TERM_CAT3_CODE == model.TERM_CAT3_CODE).ToList();
            if (_medicineService.Exists(d => d.DRUG_MAP_ID == model.DRUG_MAP_ID))
            {
                var entity = _medicineService.GetWithTrace(r => r.DRUG_MAP_ID == model.DRUG_MAP_ID);
                entity.DRUG_CODE = model.DRUG_CODE;
                entity.DRUG_NAME = model.DRUG_NAME;
                entity.DRUG_BRAND_NAME = model.DRUG_BRAND_NAME;
                entity.DRUG_GEN_NAME = model.DRUG_GEN_NAME;
                entity.DRUG_OTH_NAME = model.DRUG_OTH_NAME;
                entity.TERM_CAT_CONF_ID = termcatconf.FirstOrDefault().TERM_CAT_CONF_ID;
                var row = _medicineService.Update(entity);
                return row > 0 ? Success() : Error();
            }
            else
            {
                var drugcode = "";
                if (string.IsNullOrEmpty(model.DRUG_CODE))
                {
                    var lsDrugCode = _medicineService
                        .GetManay(d => d.SD_ID == sdId)
                        .Select(d => new { d.DRUG_CODE }).Distinct().ToList()
                        .FindAll(d => Regex.IsMatch(d.DRUG_CODE, @"^\d+$"));
                    var lsDrugCodeInt = lsDrugCode.Select(d => new { drugcode = Convert.ToInt32(d.DRUG_CODE) })
                        .OrderByDescending(d => d.drugcode);
                    var drugCodeInt = lsDrugCodeInt.FirstOrDefault()?.drugcode;
                    if (drugCodeInt > 0)
                        drugCodeInt++;
                    drugcode = drugCodeInt.ToString();
                }
                else
                {
                    if (_medicineService.Count(d => d.DRUG_CODE == model.DRUG_CODE && d.SD_ID == sdId) <= 0)
                        drugcode = model.DRUG_CODE;
                    else
                        return Error("此药品编码已在数据库中存在,请重新输入!");
                }
                var value = new UN_SD_DRUG_MAP
                {
                    TERM_CAT_CONF_ID = termcatconf.FirstOrDefault().TERM_CAT_CONF_ID,
                    DRUG_CODE = drugcode,
                    DRUG_NAME = model.DRUG_NAME,
                    DRUG_GEN_NAME = model.DRUG_GEN_NAME,
                    DRUG_BRAND_NAME = model.DRUG_BRAND_NAME,
                    DRUG_OTH_NAME = model.DRUG_OTH_NAME,
                    DRG_ATC_CODE = "",
                    MEMO = "",
                    UPD_USER_ID = "",
                    UPD_DATE = DateTime.Now,
                    SD_ID = sdId
                };

                var entity = _medicineService.Insert(value);
                return entity != null ? Success() : Error();
            }
        }

        [HttpPost]
        public ActionResult FormKW(MEDICINE_VIEWMODEL model)
        {
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            var termcatconf = _termcatconfService.GetManay(d =>
                d.TERM_CAT_CODE == model.TERM_CAT_CODE && d.TERM_CAT1_CODE == model.TERM_CAT1_CODE && d.SD_ID == sdId &&
                d.TERM_TYPE == 1);
            if (model.TERM_CAT2_CODE != null && !model.TERM_CAT2_CODE.Equals("null"))
                termcatconf = termcatconf.Where(d => d.TERM_CAT2_CODE == model.TERM_CAT2_CODE).ToList();
            if (model.TERM_CAT3_CODE != null && !model.TERM_CAT3_CODE.Equals("null"))
                termcatconf = termcatconf.Where(d => d.TERM_CAT3_CODE == model.TERM_CAT3_CODE).ToList();
            if (_medicineKWService.Exists(d => d.TERM_KW_MAP_ID == model.TERM_KW_MAP_ID))
            {
                var entity = _medicineKWService.GetWithTrace(r => r.TERM_KW_MAP_ID == model.TERM_KW_MAP_ID);
                entity.TERM_KW_NAME = model.TERM_KW_NAME;
                entity.TERM_CAT_CONF_ID = termcatconf.FirstOrDefault().TERM_CAT_CONF_ID;
                var row = _medicineKWService.Update(entity);
                return row > 0 ? Success() : Error();
            }
            else
            {
                var value = new UN_SD_TERM_KW_MAP
                {
                    TERM_CAT_CONF_ID = termcatconf.FirstOrDefault().TERM_CAT_CONF_ID,
                    TERM_KW_NAME = model.TERM_KW_NAME,
                    ATTR2_NAME = "",
                    ATTR1_NAME = "",
                    MEMO = "",
                    UPD_USER_ID = "",
                    UPD_DATE = DateTime.Now,
                    SD_ID = sdId
                };
                var m =
                    _medicineKWService.GetWithTrace(
                        d => d.TERM_CAT_CONF_ID == value.TERM_CAT_CONF_ID && d.SD_ID == sdId &&
                             d.TERM_KW_NAME.Trim() == value.TERM_KW_NAME.Trim());
                if (m != null)
                    return Error("该关键字已存在，无法重复添加！");
                var entity = _medicineKWService.Insert(value);
                return entity != null ? Success() : Error();
            }
        }

        [HttpPost]
        public ActionResult FormCate(UN_SD_TERM_CAT_CONF model)
        {
            #region 原逻辑代码
            //var sdId = ProjectProvider.Instance.Current.SD_ID;
            //var lsTermConf = _termcatconfService.Get(d =>
            //    d.TERM_CAT_CODE == model.TERM_CAT_CODE && d.SD_ID == sdId && d.TERM_TYPE == 1);
            //var termname = "";
            //var termcatcode = "";

            //#region 判断TERM_CAT_CODE是否存在
            ////存在(说明是选择已有的药品类别--需要验证化学名是否重复)--找到TERM_CAT_NAME并赋值
            //if (lsTermConf != null)
            //{
            //    termname = lsTermConf.TERM_CAT_NAME;
            //    termcatcode = lsTermConf.TERM_CAT_CODE;
            //    var lsTermCatConf = _termcatconfService.GetSearchList(d =>
            //        d.TERM_CAT_CODE == termcatcode && d.TERM_CAT1_NAME.Trim() == model.TERM_CAT1_NAME.Trim() &&
            //        d.SD_ID == sdId && d.TERM_TYPE == 1);
            //    int index = lsTermCatConf?.Count() ?? 0;
            //    //同一termcatcode下termcat1name不能重复
            //    if (index > 0)
            //    {
            //        //编辑化学名
            //        if (model.TERM_CAT_CONF_ID > 0)
            //        {
            //            if (lsTermCatConf.FindAll(d => d.TERM_CAT_CONF_ID == model.TERM_CAT_CONF_ID).Count <= 0)
            //            {
            //                return Error("此药品类别下已有此化学名,请重新输入!");
            //            }
            //        }
            //        //添加化学名
            //        else
            //        {
            //            return Error("此药品类别下已有此化学名,请重新输入!");
            //        }
            //    }
            //}
            ////不存在(说明是新增药品类别)--不用验证化学名是否重复
            //else
            //{
            //    //判断TERM_CAT_CODE是否是int型
            //    var termcatcodeint = 0;
            //    try
            //    {
            //        Convert.ToInt32(model.TERM_CAT_CODE);
            //        termcatcode = model.TERM_CAT_CODE;
            //    }
            //    //不是的话,找到数据库中的TERM_CAT_CODE的最大值并加1赋给termcatcode,TERM_CAT_CODE的值赋给termname
            //    catch (Exception e)
            //    {
            //        //先判断TERM_CAT_CODE是否与数据库中的TERM_CAT_NAME一致
            //        if (_termcatconfService.Count(d =>
            //                d.SD_ID == sdId && d.TERM_TYPE == 1 && d.TERM_CAT_NAME == model.TERM_CAT_CODE) <= 0)
            //        {
            //            var termcatconf = _termcatconfService.GetSearchList(d => d.SD_ID == sdId && d.TERM_TYPE == 1)
            //                .Select(d => new { termcatcodetemp = Convert.ToInt32(d.TERM_CAT_CODE) }).Distinct()
            //                .OrderByDescending(d => d.termcatcodetemp).FirstOrDefault();
            //            if (termcatconf != null)
            //                termcatcode = (termcatconf.termcatcodetemp + 1).ToString();
            //            else
            //                termcatcode = "1";
            //            termname = model.TERM_CAT_CODE;
            //        }
            //        else
            //        {
            //            return Error("此药品类别已存在,请重新输入!");
            //        }
            //    }
            //}
            //#endregion
            //#region 更新
            //if (_termcatconfService.Exists(d => d.TERM_CAT_CONF_ID == model.TERM_CAT_CONF_ID))
            //{
            //    var entity = _termcatconfService.GetWithTrace(r => r.TERM_CAT_CONF_ID == model.TERM_CAT_CONF_ID);
            //    entity.TERM_CAT1_NAME = model.TERM_CAT1_NAME;
            //    entity.TERM_CAT_NAME = termname;
            //    entity.TERM_CAT1_CODE = model.TERM_CAT1_CODE;
            //    entity.TERM_CAT_CODE = termcatcode;
            //    entity.TERM_CAT2_CODE = model.TERM_CAT2_CODE;
            //    entity.TERM_CAT2_NAME = model.TERM_CAT2_NAME;
            //    entity.TERM_CAT3_CODE = model.TERM_CAT3_CODE;
            //    entity.TERM_CAT3_NAME = model.TERM_CAT3_NAME;
            //    var row = _termcatconfService.Update(entity);
            //    return row > 0 ? Success() : Error();
            //}
            //#endregion
            //#region 插入
            //else
            //{
            //    var lsTermConf2 = _termcatconfService.GetSearchList(d => d.SD_ID == sdId && d.TERM_TYPE == 1)
            //        .Select(d => new { termcat1code = Convert.ToInt32(d.TERM_CAT1_CODE) }).Distinct()
            //        .OrderByDescending(d => d.termcat1code).FirstOrDefault();
            //    var termcat1code = "";
            //    if (lsTermConf2 != null)
            //        termcat1code = (lsTermConf2.termcat1code + 1).ToString();
            //    else
            //        termcat1code = "1";
            //    var value = new UN_SD_TERM_CAT_CONF
            //    {
            //        TERM_TYPE = 1,
            //        TERM_CAT1_NAME = model.TERM_CAT1_NAME,
            //        TERM_CAT_NAME = termname,
            //        TERM_CAT1_CODE = termcat1code,
            //        TERM_CAT_CODE = termcatcode,
            //        TERM_CAT2_NAME = "",
            //        TERM_CAT3_NAME = "",
            //        TERM_CAT2_CODE = "",
            //        TERM_CAT3_CODE = "",
            //        MEMO = "",
            //        UPD_USER_ID = "",
            //        UPD_DATE = DateTime.Now,
            //        SD_ID = sdId
            //    };

            //    var entity = _termcatconfService.Insert(value);
            //    return entity != null ? Success() : Error();
            //}
            //#endregion
            #endregion
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            var lstCatConf = _termcatconfService.GetManay(d => d.TERM_CAT_CODE == model.TERM_CAT_CODE && d.SD_ID == sdId && d.TERM_TYPE == 1);
            //药物类别存在
            if (lstCatConf.Count > 0)
            {
                var lsCat1 = lstCatConf.Where(d =>
                    d.TERM_CAT1_CODE == model.TERM_CAT1_CODE && d.TERM_CAT1_NAME.Trim() == model.TERM_CAT1_NAME.Trim()).ToList();
                //一级类别存在
                if (lsCat1.Count() > 0)
                {
                    //二级类别更新
                    if (model.TERM_CAT2_CODE != null && model.TERM_CAT2_NAME != null)
                    {
                        var lsCat2 =
                            lsCat1.Where(
                                d =>
                                    d.TERM_CAT2_CODE == model.TERM_CAT2_CODE &&
                                    d.TERM_CAT2_NAME.Trim() == model.TERM_CAT2_NAME.Trim()).ToList();
                        //二级类别存在
                        if (lsCat2.Count() > 0)
                        {
                            //三级类别更新
                            //if (model.TERM_CAT3_NAME != null)
                            return CatNew3(sdId, model);
                        }
                        //二级类别不存在
                        else
                            return CatNew2(sdId, model);
                    }
                    else
                        return Error("数据库中存在相同数据，无法更新！");
                }
                //一级类别不存在
                else
                {
                    var catCode1 = GetCatCode(sdId, 1, model.TERM_CAT_CODE, "", "");
                    var entity = _termcatconfService.GetWithTrace(r => r.TERM_CAT_CONF_ID == model.TERM_CAT_CONF_ID);
                    if (entity == null)
                    {
                        var entityModel = CatInsert(sdId, model, 1, catCode1);
                        return entityModel != null ? Success() : Error();
                    }
                    var row = CatUpdata(entity, model, 1, catCode1);
                    return row > 0 ? Success() : Error();
                }
            }
            else
            {
                var catCode = GetCatCode(sdId, 0, "", "", "");
                var entity = CatInsert(sdId, model, 0, catCode);
                return entity != null ? Success() : Error();
            }
            return Success();
        }

        //二级类别不存在时新增
        private ActionResult CatNew2(int sdId, UN_SD_TERM_CAT_CONF model)
        {
            var catCode2 = GetCatCode(sdId, 2, model.TERM_CAT_CODE, model.TERM_CAT1_CODE, "");
            var entity = _termcatconfService.GetWithTrace(r => r.TERM_CAT_CONF_ID == model.TERM_CAT_CONF_ID);
            if (entity != null)
            {
                var row = CatUpdata(entity, model, 2, entity.TERM_CAT2_CODE ?? catCode2);
                return row > 0 ? Success() : Error();
            }
            else
            {
                var list = _termcatconfService.GetManay(d => d.TERM_CAT_CODE == model.TERM_CAT_CODE && d.SD_ID == sdId && d.TERM_TYPE == 1
                 && d.TERM_CAT1_CODE == model.TERM_CAT1_CODE && d.TERM_CAT1_NAME.Trim() == model.TERM_CAT1_NAME.Trim()).ToList();
                if (list.Count > 1 || list.Count == 0 || (list.Count == 1 && !string.IsNullOrWhiteSpace(list.FirstOrDefault().TERM_CAT2_CODE)))
                {
                    entity = CatInsert(sdId, model, 2, catCode2);
                    return entity != null ? Success() : Error();
                }
                var m = _termcatconfService.GetWithTrace(d => d.TERM_CAT_CODE == model.TERM_CAT_CODE && d.SD_ID == sdId &&
                             d.TERM_TYPE == 1 && d.TERM_CAT1_CODE == model.TERM_CAT1_CODE && d.TERM_CAT1_NAME.Trim() == model.TERM_CAT1_NAME.Trim());
                var row = CatUpdata(m, model, 2, m.TERM_CAT2_CODE ?? catCode2);
                return row > 0 ? Success() : Error();
            }
        }
        //三级类别更新
        private ActionResult CatNew3(int sdId, UN_SD_TERM_CAT_CONF model)
        {
            var catCode3 = GetCatCode(sdId, 3, model.TERM_CAT_CODE, model.TERM_CAT1_CODE, model.TERM_CAT2_CODE);
            var entity = _termcatconfService.GetWithTrace(r => r.TERM_CAT_CONF_ID == model.TERM_CAT_CONF_ID);
            if (entity == null)
            {
                var list = _termcatconfService.GetManay(d =>
                    d.TERM_CAT_CODE == model.TERM_CAT_CODE && d.SD_ID == sdId &&
                    d.TERM_TYPE == 1 && d.TERM_CAT1_CODE == model.TERM_CAT1_CODE &&
                    d.TERM_CAT1_NAME.Trim() == model.TERM_CAT1_NAME.Trim() && d.TERM_CAT2_CODE == model.TERM_CAT2_CODE &&
                    d.TERM_CAT2_NAME.Trim() == model.TERM_CAT2_NAME.Trim()).ToList();
                if (list.Count > 1 || list.Count == 0 || (list.Count == 1 && !string.IsNullOrWhiteSpace(list.FirstOrDefault().TERM_CAT3_CODE)))
                {
                    entity = CatInsert(sdId, model, 3, catCode3);
                    return entity != null ? Success() : Error();
                }
                var m =
                    _termcatconfService.GetWithTrace(
                        d => d.TERM_CAT_CODE == model.TERM_CAT_CODE && d.SD_ID == sdId &&
                             d.TERM_TYPE == 1 && d.TERM_CAT1_CODE == model.TERM_CAT1_CODE &&
                             d.TERM_CAT1_NAME.Trim() == model.TERM_CAT1_NAME.Trim() &&
                             d.TERM_CAT2_CODE == model.TERM_CAT2_CODE &&
                             d.TERM_CAT2_NAME.Trim() == model.TERM_CAT2_NAME.Trim());
                if (string.IsNullOrWhiteSpace(model.TERM_CAT3_CODE) && string.IsNullOrWhiteSpace(model.TERM_CAT3_NAME))
                    return Error("数据库中存在相同数据，无法添加！");
                var row = CatUpdata(m, model, 3, (m.TERM_CAT3_CODE == null || m.TERM_CAT3_CODE == "") ? catCode3 : m.TERM_CAT3_CODE);
                return row > 0 ? Success() : Error();
            }
            else
            {
                var m =
                    _termcatconfService.GetWithTrace(
                        d => d.TERM_CAT_CODE == model.TERM_CAT_CODE && d.SD_ID == sdId &&
                             d.TERM_TYPE == 1 && d.TERM_CAT1_CODE == model.TERM_CAT1_CODE &&
                             d.TERM_CAT1_NAME.Trim() == model.TERM_CAT1_NAME.Trim() &&
                             d.TERM_CAT2_CODE == model.TERM_CAT2_CODE &&
                             d.TERM_CAT2_NAME.Trim() == model.TERM_CAT2_NAME.Trim());
                if (m != null && m.TERM_CAT_CONF_ID != entity.TERM_CAT_CONF_ID)
                    return Error("数据库中存在相同数据，无法修改！");
                var row = CatUpdata(entity, model, 3, (entity.TERM_CAT3_CODE == null || entity.TERM_CAT3_CODE == "") ? catCode3 : entity.TERM_CAT3_CODE);
                return row > 0 ? Success() : Error();
            }
        }
        /// <summary>
        /// 类别新增
        /// </summary>
        /// <param name="sdId"></param>
        /// <param name="model"></param>
        /// <param name="type">类型（0：药物类别 1：一级类别 2：二级类别 3：三级类别）</param>
        /// <param name="code"></param>
        /// <returns></returns>
        private UN_SD_TERM_CAT_CONF CatInsert(int sdId, UN_SD_TERM_CAT_CONF model, int type, string code)
        {
            var entity = new UN_SD_TERM_CAT_CONF()
            {
                SD_ID = sdId,
                TERM_TYPE = 1,
                TERM_CAT_CODE = type == 0 ? code : model.TERM_CAT_CODE,
                TERM_CAT_NAME = model.TERM_CAT_NAME ?? "",
                TERM_CAT1_CODE = type == 1 ? code : (model.TERM_CAT1_CODE == "aaa" ? "1" : model.TERM_CAT1_CODE),
                TERM_CAT1_NAME = model.TERM_CAT1_NAME ?? "",
                TERM_CAT2_CODE = type == 2 ? code : (model.TERM_CAT2_CODE == "aaa" ? "1" : model.TERM_CAT2_CODE),
                TERM_CAT2_NAME = model.TERM_CAT2_NAME ?? "",
                TERM_CAT3_CODE = type == 3 ? code : (model.TERM_CAT3_CODE == "aaa" ? "1" : model.TERM_CAT3_CODE),
                TERM_CAT3_NAME = model.TERM_CAT3_NAME ?? ""
            };
            var m = _termcatconfService.Insert(entity);
            return m;
        }
        /// <summary>
        /// 类别更新
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="model"></param>
        /// <param name="type">类型（0：药物类别 1：一级类别 2：二级类别 3：三级类别）</param>
        /// <param name="code"></param>
        /// <returns></returns>
        private int CatUpdata(UN_SD_TERM_CAT_CONF entity, UN_SD_TERM_CAT_CONF model, int type, string code)
        {
            entity.TERM_CAT_CODE = model.TERM_CAT_NAME == null ? "" : (type == 0 ? code : model.TERM_CAT_CODE);
            entity.TERM_CAT_NAME = model.TERM_CAT_NAME ?? "";
            entity.TERM_CAT1_CODE = model.TERM_CAT1_NAME == null ? "" : (type == 1 ? code : (model.TERM_CAT1_CODE == "aaa" ? "1" : model.TERM_CAT1_CODE));
            entity.TERM_CAT1_NAME = model.TERM_CAT1_NAME ?? "";
            entity.TERM_CAT2_CODE = model.TERM_CAT2_NAME == null ? "" : (type == 2 ? code : (model.TERM_CAT2_CODE == "aaa" ? "1" : model.TERM_CAT2_CODE));
            entity.TERM_CAT2_NAME = model.TERM_CAT2_NAME ?? "";
            entity.TERM_CAT3_CODE = model.TERM_CAT3_NAME == null ? "" : (type == 3 ? code : (model.TERM_CAT3_CODE == "aaa" ? "1" : model.TERM_CAT3_CODE));
            entity.TERM_CAT3_NAME = model.TERM_CAT3_NAME ?? "";
            var row = _termcatconfService.Update(entity);
            return row;
        }
        /// <summary>
        /// 获取类别代码
        /// </summary>
        /// <param name="sdId">病种代码</param>
        /// <param name="type">类别（0：药物类别  1：一级类别 2：二级类别  3：三级类别）</param>
        /// <param name="cat">药物类别代码</param>
        /// <param name="cat1">一级类别代码</param>
        /// <param name="cat2">二级类别代码</param>
        /// <returns></returns>
        private string GetCatCode(int sdId, int type, string cat, string cat1, string cat2)
        {
            try
            {
                var catConf = new { temp = 0 };
                switch (type)
                {
                    case 0:
                        catConf = _termcatconfService.GetManay(d => d.SD_ID == sdId && d.TERM_TYPE == 1)
                            .Select(d => new { temp = Convert.ToInt32(d.TERM_CAT_CODE == "" ? "0" : d.TERM_CAT_CODE) }).Distinct()
                            .OrderByDescending(d => d.temp).FirstOrDefault() ?? new { temp = 0 };
                        break;
                    case 1:
                        catConf = _termcatconfService.GetManay(d => d.SD_ID == sdId && d.TERM_TYPE == 1 && d.TERM_CAT_CODE == cat)
                            .Select(d => new { temp = Convert.ToInt32(d.TERM_CAT1_CODE == "" ? "0" : d.TERM_CAT1_CODE) }).Distinct()
                            .OrderByDescending(d => d.temp).FirstOrDefault() ?? new { temp = 0 };
                        break;
                    case 2:
                        catConf = _termcatconfService.GetManay(
                                d => d.SD_ID == sdId && d.TERM_TYPE == 1 && d.TERM_CAT_CODE == cat && d.TERM_CAT1_CODE == cat1)
                                .Select(d => new { temp = Convert.ToInt32(d.TERM_CAT2_CODE == "" ? "0" : d.TERM_CAT2_CODE) }).Distinct()
                                .OrderByDescending(d => d.temp).FirstOrDefault() ?? new { temp = 0 };
                        break;
                    case 3:
                        catConf = _termcatconfService.GetManay(
                                d => d.SD_ID == sdId && d.TERM_TYPE == 1 && d.TERM_CAT_CODE == cat && d.TERM_CAT1_CODE == cat1 && d.TERM_CAT2_CODE == cat2)
                                .Select(d => new { temp = Convert.ToInt32(d.TERM_CAT3_CODE == "" ? "0" : d.TERM_CAT3_CODE) }).Distinct()
                                .OrderByDescending(d => d.temp).FirstOrDefault() ?? new { temp = 0 };
                        break;
                }
                var catCode = (catConf.temp + 1).ToString();
                return catCode;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult GetTypeNameList(string typeId, int termtype, int cattype, string catcode1, string catcode2)
        {
            var ItemTermName = _termcatconfService.GetTypeName(typeId, termtype, cattype, catcode1, catcode2);
            return Content(ItemTermName.ToJson());
        }

        public ActionResult Delete(string primaryKey)
        {
            var id = int.Parse(primaryKey.Trim());
            var row = _medicineService.ExDelete(r => r.DRUG_MAP_ID == id);
            return row > 0 ? Success() : Error();
        }

        public ActionResult DeleteKW(string primaryKeyKW)
        {
            var id = int.Parse(primaryKeyKW.Trim());
            var row = _medicineKWService.ExDelete(r => r.TERM_KW_MAP_ID == id);
            return row > 0 ? Success() : Error();
        }

        public ActionResult DeleteTermType(string primaryKeyTermType)
        {
            var sdId = ProjectProvider.Instance.Current.SD_ID;
            var dics = new Dictionary<string, string>();
            var ifTheLast = "";
            var id = int.Parse(primaryKeyTermType.Trim());
            var strTermCatCode
                = _termcatconfService.Get(d => d.TERM_CAT_CONF_ID == id).TERM_CAT_CODE;
            var countTermCatCode = _termcatconfService.Count(d =>
                d.TERM_CAT_CODE == strTermCatCode && d.SD_ID == sdId && d.TERM_TYPE == 1);
            ifTheLast = countTermCatCode > 1 ? "0" : "1";

            dics.Add("ifTheLast", ifTheLast);

            return Json(dics, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteTermTypeConn(string primaryKeyTermTypeConn)
        {
            var dics = new Dictionary<string, string>();
            var ifDelSuccess = "";
            var ifExistsDrug = "";
            var ifExistsTermKW = "";
            var id = int.Parse(primaryKeyTermTypeConn.Trim());
            var countDrugMap = _medicineService.Count(d => d.TERM_CAT_CONF_ID == id);
            var countTermKWMap = _medicineKWService.Count(d => d.TERM_CAT_CONF_ID == id);
            ifExistsDrug = countDrugMap > 0 ? "1" : "0";
            ifExistsTermKW = countTermKWMap > 0 ? "1" : "0";
            if (ifExistsDrug == "0" && ifExistsTermKW == "0")
                ifDelSuccess = "1";
            else
                ifDelSuccess = "0";
            dics.Add("ifDelSuccess", ifDelSuccess);
            return Json(dics, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteCate(string primaryKeyCate)
        {
            var id = int.Parse(primaryKeyCate.Trim());
            var row = _termcatconfService.ExDelete(r => r.TERM_CAT_CONF_ID == id);
            return row > 0 ? Success() : Error();
        }

        public ActionResult GetForm(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var entity = _medicineService.Get(r => r.DRUG_MAP_ID == id);
            var entity2 = _termcatconfService.Get(r => r.TERM_CAT_CONF_ID == entity.TERM_CAT_CONF_ID);
            return Content(new MEDICINE_VIEWMODEL
            {
                DRUG_MAP_ID = entity.DRUG_MAP_ID,
                DRUG_BRAND_NAME = entity.DRUG_BRAND_NAME,
                DRUG_CODE = entity.DRUG_CODE,
                DRUG_GEN_NAME = entity.DRUG_GEN_NAME,
                DRUG_NAME = entity.DRUG_NAME,
                DRUG_OTH_NAME = entity.DRUG_OTH_NAME,
                TERM_CAT1_NAME = entity2?.TERM_CAT1_NAME,
                TERM_CAT1_CODE = entity2?.TERM_CAT1_CODE,
                TERM_CAT_NAME = entity2?.TERM_CAT_NAME,
                TERM_CAT_CODE = entity2?.TERM_CAT_CODE,
                TERM_CAT2_NAME = entity2?.TERM_CAT2_NAME,
                TERM_CAT2_CODE = entity2?.TERM_CAT2_CODE,
                TERM_CAT3_NAME = entity2?.TERM_CAT3_NAME,
                TERM_CAT3_CODE = entity2?.TERM_CAT3_CODE
            }.ToJson());
        }

        public ActionResult GetFormKW(string primaryKeyKW)
        {
            var id = int.Parse(primaryKeyKW);
            var entity = _medicineKWService.Get(r => r.TERM_KW_MAP_ID == id);
            var entity2 = _termcatconfService.Get(r => r.TERM_CAT_CONF_ID == entity.TERM_CAT_CONF_ID);
            return Content(new MEDICINE_VIEWMODEL
            {
                TERM_KW_MAP_ID = entity.TERM_KW_MAP_ID,
                TERM_KW_NAME = entity.TERM_KW_NAME,
                TERM_CAT1_NAME = entity2?.TERM_CAT1_NAME,
                TERM_CAT1_CODE = entity2?.TERM_CAT1_CODE,
                TERM_CAT_NAME = entity2?.TERM_CAT_NAME,
                TERM_CAT_CODE = entity2?.TERM_CAT_CODE,
                TERM_CAT2_NAME = entity2?.TERM_CAT2_NAME,
                TERM_CAT2_CODE = entity2?.TERM_CAT2_CODE,
                TERM_CAT3_NAME = entity2?.TERM_CAT3_NAME,
                TERM_CAT3_CODE = entity2?.TERM_CAT3_CODE
            }.ToJson());
        }

        public ActionResult GetFormCate(string primaryKeyCate)
        {
            var id = int.Parse(primaryKeyCate);
            var entity = _termcatconfService.Get(r => r.TERM_CAT_CONF_ID == id);
            return Content(new UN_SD_TERM_CAT_CONF
            {
                TERM_CAT_CONF_ID = entity.TERM_CAT_CONF_ID,
                TERM_CAT1_NAME = entity.TERM_CAT1_NAME,
                TERM_CAT1_CODE = entity.TERM_CAT1_CODE,
                TERM_CAT_NAME = entity.TERM_CAT_NAME,
                TERM_CAT_CODE = entity.TERM_CAT_CODE,
                TERM_CAT2_NAME = entity.TERM_CAT2_NAME,
                TERM_CAT2_CODE = entity.TERM_CAT2_CODE,
                TERM_CAT3_NAME = entity.TERM_CAT3_NAME,
                TERM_CAT3_CODE = entity.TERM_CAT3_CODE
            }.ToJson());
        }
    }
}