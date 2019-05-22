using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Models.Unit;
using TestingCenterSystem.Service.Dict;
using TestingCenterSystem.Service.Dict.Interface;
using TestingCenterSystem.Service.Dictionary;
using TestingCenterSystem.Service.Project;
using TestingCenterSystem.Service.Project.Interface;
using TestingCenterSystem.ViewModels.Dictionary;
using TestingCenterSystem.ViewModels.Project;

namespace Elight.Web.Areas.TestCenterSystem.Controllers.Dictionary
{
    [LoginChecked]
    public class TestDicController : BaseController
    {
        private static readonly ITestDicService _testDicService = new TestDicService();
        private static readonly ITermCatConfService _termCatConfService = new TermCatConfService();
        private static readonly IOthTermMapService _othThermMapService = new OthTermMapService();
        // GET: TestCenterSystem/TestDic
        public ActionResult Index()
        {
            ViewBag.testType = _termCatConfService.GetSearchList(b => b.SD_ID == ProjectProvider.Instance.Current.SD_ID && b.TERM_TYPE == 3).Select(b => b.TERM_CAT_NAME).Distinct().ToList();
            return View();
        }
        //得到数据
        [HttpGet]
        public ActionResult GetList(int page, int limit, string field, string type, string keyWord = "")
        {
            var pageData = _testDicService.GetPage(page, limit, field, type, keyWord);
            var result = new Dictionary<string, object>()
            {
                {"code",0 },
                {"msg","success" },
                {"data",pageData.Items },
                {"count",pageData.TotalItems }
            };
            return Content(result.ToJson());
        }
        public ActionResult Form()
        {
            ViewBag.ItemType = _testDicService.GetTypeName();
            return View();
        }
        [HttpPost, AuthorizeChecked]
        public ActionResult Form(TESTDIC_VIEWMODEL model)
        {
            //新增
            if (string.IsNullOrEmpty(model.OTH_TERM_MAP_ID.ToString()) || model.OTH_TERM_MAP_ID == 0)
            {
                var result = _testDicService.InsertTermMapInfo(model);
                return result.Item1 ? Success() : Error(result.Item2);
                #region qiaolei
                //int confId = 0;
                //if (model.TERM_CAT1_NAME != null)
                //{
                //    confId = _termCatConfService.Get(b => (b.TERM_CAT_NAME == model.TERM_CAT_NAME && b.TERM_CAT1_NAME == model.TERM_CAT1_NAME) && b.TERM_TYPE == 3 && b.SD_ID == ProjectProvider.Instance.Current.SD_ID).TERM_CAT_CONF_ID;

                //}
                //else
                //{
                //    confId = _termCatConfService.Get(b => b.TERM_CAT_NAME == model.TERM_CAT_NAME && b.TERM_TYPE == 3 && b.SD_ID == ProjectProvider.Instance.Current.SD_ID).TERM_CAT_CONF_ID;
                //}

                //if (_testDicService.Exists(b => b.OTH_TERM_NAME == model.OTH_TERM_NAME && b.OTH_TERM_CODE == model.OTH_TERM_CODE && b.TERM_CAT_CONF_ID == confId && b.SD_ID == ProjectProvider.Instance.Current.SD_ID))
                //{
                //    return Error("该条信息已经存在");
                //}
                //else if (_testDicService.Exists(b => (b.OTH_TERM_NAME == model.OTH_TERM_NAME || b.OTH_TERM_CODE == model.OTH_TERM_CODE) && b.SD_ID == ProjectProvider.Instance.Current.SD_ID))
                //{
                //    return Error("编码或名称不能重复！");
                //}
                //else
                //{
                //    var isSuccess = _testDicService.Inserts(model);
                //    return isSuccess ? Success() : Error("编码或名称不能重复！");
                //}
                #endregion
            }
            //修改
            else
            {
                var result = _testDicService.EditTermMapInfo(model);
                return result.Item1 ? Success() : Error(result.Item2);
                #region 乔磊
                // var confId = _termCatConfService.Get(b => b.TERM_CAT_NAME == model.TERM_CAT_NAME && b.TERM_TYPE == 3 && b.TERM_CAT1_NAME == model.TERM_CAT1_NAME && b.SD_ID == ProjectProvider.Instance.Current.SD_ID).TERM_CAT_CONF_ID;

                //var confIdList = _termCatConfService.GetManay(b => b.SD_ID == ProjectProvider.Instance.Current.SD_ID && b.TERM_TYPE == 2).Select(b => b.TERM_CAT_CONF_ID).ToList();
                //if (_othThermMapService.Exists(b => (b.OTH_TERM_CODE == model.OTH_TERM_CODE || b.OTH_TERM_NAME == model.OTH_TERM_NAME) && b.SD_ID == ProjectProvider.Instance.Current.SD_ID && b.OTH_TERM_MAP_ID != model.OTH_TERM_MAP_ID))
                //{
                //    return Error("编码或名称不能重复！");
                //}
                //else
                //{
                //    if (model.OTH_TERM_CODE != null)
                //    {
                //        UN_SD_OTH_TERM_MAP termcatconf = _testDicService.GetWithTrace(b => b.OTH_TERM_MAP_ID == model.OTH_TERM_MAP_ID);
                //        termcatconf.OTH_TERM_CODE = model.OTH_TERM_CODE;
                //        termcatconf.OTH_TERM_NAME = model.OTH_TERM_NAME;
                //        termcatconf.TERM_CAT_CONF_ID = _termCatConfService.GetWithTrace(b => b.TERM_CAT_NAME == model.TERM_CAT_NAME && b.TERM_CAT1_NAME == model.TERM_CAT1_NAME && b.SD_ID == ProjectProvider.Instance.Current.SD_ID).TERM_CAT_CONF_ID;

                //        var result = _testDicService.Update(termcatconf);
                //        return result > 0 ? Success() : Error();
                //    }
                //    else
                //    {
                //        #region 随机编码 
                //        Random ro = new Random();
                //        int iResult;
                //        int iUp = 9999;
                //        int iDown = 1000;
                //        iResult = ro.Next(iDown, iUp);
                //        string catName = "";
                //        // var codeMax = GetManay(b => b.TERM_CAT_CONF_ID == confId).Select(b => b.OTH_TERM_CODE).Max();
                //        if (model.TERM_CAT_NAME.Contains("血脂"))
                //        {
                //            catName = "XZ_" + iResult;
                //        }
                //        else if (model.TERM_CAT_NAME.Contains("血糖"))
                //        {
                //            catName = "XT_" + iResult;
                //        }
                //        else
                //        {
                //            catName = "DEF_" + iResult;
                //        }
                //        #endregion
                //        UN_SD_OTH_TERM_MAP termcatconf = _testDicService.GetWithTrace(b => b.OTH_TERM_MAP_ID == model.OTH_TERM_MAP_ID);
                //        termcatconf.OTH_TERM_CODE = catName;
                //        termcatconf.OTH_TERM_NAME = model.OTH_TERM_NAME;
                //        termcatconf.TERM_CAT_CONF_ID = _termCatConfService.GetWithTrace(b => b.TERM_CAT_NAME == model.TERM_CAT_NAME && b.TERM_CAT1_NAME == model.TERM_CAT1_NAME && b.SD_ID == ProjectProvider.Instance.Current.SD_ID).TERM_CAT_CONF_ID;

                //        var result = _testDicService.Update(termcatconf);
                //        return result > 0 ? Success() : Error();
                //    }

                //}
                #endregion
            }
        }
        [HttpPost]
        public ActionResult GetForm(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var entity = _testDicService.Get(b => b.OTH_TERM_MAP_ID == id);
            return Content(new TESTDIC_VIEWMODEL
            {
                OTH_TERM_MAP_ID = id,
                TERM_CAT_NAME = _termCatConfService.Get(b => b.TERM_CAT_CONF_ID == (int)entity.TERM_CAT_CONF_ID).TERM_CAT_NAME,
                TERM_CAT1_NAME = _termCatConfService.Get(b => b.TERM_CAT_CONF_ID == (int)entity.TERM_CAT_CONF_ID).TERM_CAT1_NAME,
                OTH_TERM_CODE = entity.OTH_TERM_CODE,
                OTH_TERM_NAME = entity.OTH_TERM_NAME
            }.ToJson());
        }
        /// <summary>
        ///  全部类别名称
        /// </summary> 
        [HttpPost]
        public ActionResult GetTypeNameList(string typeId = "")
        {
            var ItemTermName = _testDicService.GetTypeName(typeId);
            return Content(ItemTermName.ToJson());
        }
        /// <summary>
        /// 删除
        /// </summary> 
        public ActionResult Delete(string primaryKey)
        {
            var id = int.Parse(primaryKey.Trim());
            var row = _testDicService.ExDelete(r => r.OTH_TERM_MAP_ID == id);
            return row > 0 ? Success() : Error();
        }

        /*-----------------------------类别维护-------------------------------------------*/
        public ActionResult SubCategoryIndex()
        {
            return View();
        }

        /// <summary>
        /// 类别显示
        /// </summary>
        [HttpGet]
        public ActionResult CategoryGetList(int page, int limit, string field, string type, string keyWord = "")
        {
            var pageData = _testDicService.CategoryGetPage(page, limit, field, type, keyWord);
            var result = new Dictionary<string, object>() {
                { "code", 0},
                { "msg","success"},
                { "data", pageData.Items},
                { "count", pageData.TotalItems}
            };
            return Content(result.ToJson());
        }
        [HttpGet]
        public ActionResult CategoryFrom()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CategoryFrom(UN_SD_TERM_CAT_CONF model)
        {
            //新增
            if (string.IsNullOrEmpty(model.TERM_CAT_CODE))
            {
                //  var count = _termCatConfService.GetManay(d => d.TERM_CAT_NAME == model.TERM_CAT_NAME && d.TERM_TYPE == 3 && d.SD_ID == ProjectProvider.Instance.Current.SD_ID).Count;
                if (_termCatConfService.Exists(d => d.TERM_CAT_NAME == model.TERM_CAT_NAME && d.TERM_TYPE == 3 && d.SD_ID == ProjectProvider.Instance.Current.SD_ID))
                {
                    return Error("该条检查类别已存在!");
                }
                else
                {
                    var catConf = _termCatConfService.GetManay(d => d.TERM_TYPE == 3 && d.SD_ID == ProjectProvider.Instance.Current.SD_ID);
                    var Catconfig = _termCatConfService.Insert(new UN_SD_TERM_CAT_CONF
                    {
                        TERM_TYPE = 3,
                        TERM_CAT_CODE = catConf.Count > 0 ? catConf.Max(r => int.Parse(r.TERM_CAT_CODE) + 1).ToString() : "1",//(_termCatConfService.GetManay(d => d.TERM_TYPE == 3 && d.SD_ID == ProjectProvider.Instance.Current.SD_ID).Max(r => int.Parse(r.TERM_CAT_CODE)) + 1).ToString(),
                        TERM_CAT_NAME = model.TERM_CAT_NAME,
                        SD_ID = ProjectProvider.Instance.Current.SD_ID
                    });
                    return Catconfig != null ? Success() : Error();
                }
            }
            //编辑
            else
            {
                //var confId = _termCatConfService.Get(d => d.TERM_CAT_NAME == model.TERM_CAT_NAME && d.TERM_TYPE == 2 && d.SD_ID == ProjectProvider.Instance.Current.SD_ID).TERM_CAT_CONF_ID;
                if (_termCatConfService.Exists(d => d.TERM_CAT_NAME == model.TERM_CAT_NAME && d.SD_ID == ProjectProvider.Instance.Current.SD_ID && d.TERM_CAT_CONF_ID != model.TERM_CAT_CONF_ID))
                {
                    return Error("该类别名称重复!");
                }
                else
                {
                    if (_termCatConfService.Exists(b => b.TERM_CAT_NAME == model.TERM_CAT_NAME && b.TERM_TYPE == 3 && b.SD_ID == ProjectProvider.Instance.Current.SD_ID && b.TERM_CAT_CODE != model.TERM_CAT_CODE))
                    {
                        return Error("此类别名称已经存在!");
                    }
                    var confnList = _termCatConfService.GetManayWithTrace(r => r.TERM_CAT_CODE == model.TERM_CAT_CODE && r.TERM_TYPE == 3 && r.SD_ID == ProjectProvider.Instance.Current.SD_ID);

                    foreach (var confn in confnList)
                    {
                        confn.TERM_CAT_NAME = model.TERM_CAT_NAME;
                        if (_termCatConfService.Update(confn) < 0)
                            return Error("数据更新失败！");
                    }
                    return Success();
                }
            }
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CategoryGetForm(string primaryKey)
        {
            var entity = _termCatConfService.GetManay(item => item.TERM_CAT_CODE == primaryKey && item.TERM_TYPE == 3 && item.SD_ID == ProjectProvider.Instance.Current.SD_ID).FirstOrDefault();
            return Content(new TESTDIC_VIEWMODEL
            {
                TERM_CAT_NAME = entity.TERM_CAT_NAME,
                TERM_CAT_CODE = entity.TERM_CAT_CODE
            }.ToJson());
        }

        /// <summary>
        /// 检验删除
        /// </summary> 
        public ActionResult CategoryDelete(string primaryKey)
        {
            // var code= _termCatConfService.GetManay(b => b.TERM_CAT_CODE == primaryKey && b.TERM_TYPE == 3 && b.SD_ID == ProjectProvider.Instance.Current.SD_ID).Select(
            ///   b => b.TERM_CAT1_CODE).FirstOrDefault();
            var code1 = _termCatConfService.GetManay(b => b.TERM_CAT_CODE == primaryKey && b.TERM_TYPE == 3 && b.SD_ID == ProjectProvider.Instance.Current.SD_ID).Select(
               b => b.TERM_CAT1_CODE).Count();

            if (code1 > 0)
            {
                return Error("该类别已使用，不能删除");
            }
            var id = primaryKey.Trim();
            var row = _termCatConfService.ExDelete(r => r.TERM_CAT_CODE == id);
            return row > 0 ? Success() : Error();
        }

        /*--------------------------细分类别----------------------------*/
        // Subdivide ==Sub
        [HttpGet]
        public ActionResult GetListSub(int page, int limit, string field, string type, string keyWord = "")
        {
            var pageData = _testDicService.CategoryGetPageSub(page, limit, field, type, keyWord);
            var result = new Dictionary<string, object>() {
                { "code", 0},
                { "msg","success"},
                { "data", pageData.Items},
                { "count", pageData.TotalItems}
            };
            return Content(result.ToJson());
        }
        [HttpGet]
        public ActionResult SubCategoryForm()
        {
            ViewBag.testType = _termCatConfService.GetSearchList(b => b.SD_ID == ProjectProvider.Instance.Current.SD_ID && b.TERM_TYPE == 3).Select(b => b.TERM_CAT_NAME).Distinct().ToList();
            return View();
        }
        [HttpPost]
        public ActionResult SubCategoryForm(UN_SD_TERM_CAT_CONF model)
        {
            //新增
            if (model.TERM_CAT_CONF_ID == 0)
            {
                //  var count = _termCatConfService.GetManay(d => d.TERM_CAT_NAME == model.TERM_CAT_NAME && d.TERM_TYPE == 3 && d.SD_ID == ProjectProvider.Instance.Current.SD_ID).Count;
                if (_termCatConfService.Exists(d => d.TERM_CAT_NAME == model.TERM_CAT_NAME && d.TERM_CAT1_NAME == model.TERM_CAT1_NAME && d.TERM_TYPE == 3 && d.SD_ID == ProjectProvider.Instance.Current.SD_ID))
                {
                    return Error("该条检查类别已存在");
                }
                else
                {
                    //var catConf = _termCatConfService.GetManay(b => b.TERM_CAT_NAME == model.TERM_CAT_NAME && b.TERM_TYPE == 3 && b.SD_ID == ProjectProvider.Instance.Current.SD_ID).Select(b => b.TERM_CAT_CODE).FirstOrDefault();
                    var catConf = _termCatConfService.GetManayWithTrace(b => b.TERM_CAT_NAME == model.TERM_CAT_NAME && b.TERM_TYPE == 3 && b.SD_ID == ProjectProvider.Instance.Current.SD_ID);
                    var catConf1 = catConf.Where(r => r.TERM_CAT1_NAME.IsNullOrEmpty()).ToList();
                    if (catConf1.Count > 0)
                    {
                        var catConfEntity = catConf1.FirstOrDefault();
                        catConfEntity.TERM_CAT1_CODE = (_termCatConfService.GetManayWithTrace(d => d.TERM_TYPE == 3 && d.SD_ID == ProjectProvider.Instance.Current.SD_ID && d.TERM_CAT_NAME == model.TERM_CAT_NAME).Max(r => int.Parse(r.TERM_CAT1_CODE ?? "0")) + 1).ToString();
                        catConfEntity.TERM_CAT1_NAME = model.TERM_CAT1_NAME;
                        var count = _termCatConfService.Update(catConfEntity);
                        return count > 0 ? Success() : Error();
                    }
                    else
                    {
                        var Catconfig = _termCatConfService.Insert(new UN_SD_TERM_CAT_CONF
                        {
                            //如何得到查询后code的内容
                            TERM_TYPE = 3,
                            TERM_CAT_CODE = catConf.Select(r => r.TERM_CAT_CODE).FirstOrDefault(),//(_termCatConfService.GetManay(b => b.TERM_CAT_NAME == model.TERM_CAT_NAME && b.TERM_TYPE == 3 && b.SD_ID == ProjectProvider.Instance.Current.SD_ID)).ToString(),
                            TERM_CAT_NAME = model.TERM_CAT_NAME,
                            TERM_CAT1_CODE = (_termCatConfService.GetManay(d => d.TERM_TYPE == 3 && d.SD_ID == ProjectProvider.Instance.Current.SD_ID && d.TERM_CAT_NAME == model.TERM_CAT_NAME).Max(r => int.Parse(r.TERM_CAT1_CODE ?? "0")) + 1).ToString(),
                            TERM_CAT1_NAME = model.TERM_CAT1_NAME,
                            SD_ID = ProjectProvider.Instance.Current.SD_ID
                        });
                        return Catconfig != null ? Success() : Error();
                    }
                }
            }
            //编辑
            else
            {
                if (_termCatConfService.Exists(d => d.TERM_CAT1_NAME == model.TERM_CAT1_NAME && d.SD_ID == ProjectProvider.Instance.Current.SD_ID && d.TERM_CAT_NAME == model.TERM_CAT_NAME && d.TERM_CAT_CONF_ID != model.TERM_CAT_CONF_ID))
                {
                    return Error("该类别名称重复");
                }
                else
                {
                    if (_termCatConfService.Exists(b => b.TERM_CAT1_NAME == model.TERM_CAT1_NAME && b.TERM_TYPE == 3 && b.SD_ID == ProjectProvider.Instance.Current.SD_ID && b.TERM_CAT_NAME == model.TERM_CAT_NAME && b.TERM_CAT_CONF_ID != model.TERM_CAT_CONF_ID))
                    {
                        return Error("此类别名称已经存在");
                    }
                    // var confnList = _termCatConfService.GetManay(r => r.TERM_TYPE == 3 && r.SD_ID == ProjectProvider.Instance.Current.SD_ID&&r.TERM_CAT_CONF_ID==model.TERM_CAT_CONF_ID);
                    var confn = _termCatConfService.GetWithTrace(r => r.TERM_CAT_CONF_ID == model.TERM_CAT_CONF_ID);
                    confn.TERM_CAT1_NAME = model.TERM_CAT1_NAME;
                    confn.TERM_CAT_NAME = model.TERM_CAT_NAME;
                    var Catconfig = _termCatConfService.Update(confn);

                    return Catconfig != 0 ? Success() : Error();
                    //foreach (var confn in confnList)
                    //{
                    //    confn.TERM_CAT1_NAME = model.TERM_CAT1_NAME;
                    //    if (_termCatConfService.Update(confn) < 0)
                    //        return Error("数据更新失败！");
                    //}

                    //return Success();
                }
            }
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CategoryGetFormSub(string primaryKey)
        {
            var entity = _termCatConfService.GetManay(item => item.TERM_CAT_CONF_ID.ToString() == primaryKey && item.TERM_TYPE == 3 && item.SD_ID == ProjectProvider.Instance.Current.SD_ID).FirstOrDefault();

            return Content(new TESTDIC_VIEWMODEL
            {
                TERM_CAT_CONF_ID = entity.TERM_CAT_CONF_ID,
                TERM_CAT_NAME = entity.TERM_CAT_NAME,
                TERM_CAT_CODE = entity.TERM_CAT_CODE,
                TERM_CAT1_CODE = entity.TERM_CAT1_CODE,
                TERM_CAT1_NAME = entity.TERM_CAT1_NAME
            }.ToJson());
        }

        /// <summary>
        /// 检验删除
        /// </summary> 

        public ActionResult DeleteSub(string primaryKey)
        {
            //var code = _termCatConfService.GetManay(b => b.TERM_CAT_CODE == primaryKey && b.TERM_TYPE == 3 && b.SD_ID == ProjectProvider.Instance.Current.SD_ID).Select(b => b.TERM_CAT_CONF_ID).FirstOrDefault();

            if (_othThermMapService.Exists(b => b.TERM_CAT_CONF_ID.ToString() == primaryKey))
            {
                return Error("该类别已使用，不能删除!");
            }
            var id = primaryKey.Trim();
            var row = _termCatConfService.ExDelete(r => r.TERM_CAT_CONF_ID.ToString() == id);
            return row > 0 ? Success() : Error();
        }

    }
}