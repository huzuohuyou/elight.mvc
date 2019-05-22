using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestingCenter.Models;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Models.Unit;
using TestingCenterSystem.Service.DataItem;
using TestingCenterSystem.Service.DataItem.Interface;
using TestingCenterSystem.Service.Dict;
using TestingCenterSystem.Service.Dict.Interface;
using TestingCenterSystem.Service.Project;
using TestingCenterSystem.Service.Project.Interface;
using TestingCenterSystem.ViewModels.CheckDic;
using TestingCenterSystem.ViewModels.Project;

namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    [LoginChecked]
    public class CheckDicController : BaseController
    {
        private static readonly ITermCatConfService _termCatConfService = new TermCatConfService();
        private static readonly IOthTermMapService _othThermMapService = new OthTermMapService();

        // GET: TestCenterSystem/CheckDicsw
        public ActionResult Index()
        {
            ViewBag.checkType = _termCatConfService.GetSearchList(s => s.SD_ID == ProjectProvider.Instance.Current.SD_ID && s.TERM_TYPE == 2).Select(s => s.TERM_CAT_NAME).Distinct().ToList();
            return View();
        }

        [HttpGet]
        public ActionResult Form()
        {
            ViewBag.ItemType = _othThermMapService.GetTyptName();
            return View();
        }
        [HttpPost]
        public ActionResult Form(CHECKDIC_VIEWMODEL model)
        {
            //新增
            if (string.IsNullOrEmpty(model.OTH_TERM_MAP_ID.ToString()) || model.OTH_TERM_MAP_ID == 0)
            {
                var confId = _termCatConfService.Get(d => d.TERM_CAT_NAME == model.TERM_CAT_NAME && d.SD_ID == ProjectProvider.Instance.Current.SD_ID).TERM_CAT_CONF_ID;
                if (_othThermMapService.Exists(b => b.OTH_TERM_NAME == model.OTH_TERM_NAME && b.OTH_TERM_CODE == model.OTH_TERM_CODE && b.TERM_CAT_CONF_ID == confId && b.SD_ID == ProjectProvider.Instance.Current.SD_ID))
                {
                    return Error("该条信息已经存在");
                }
                else
                {
                    #region MyRegion
                    //var sd_mode = new SD_OTH_TERM_MAP
                    //{
                    //    OTH_TERM_CODE = model.OTH_TERM_CODE,
                    //    OTH_TERM_NAME = model.OTH_TERM_NAME,
                    //    TERM_CAT_CONF_ID = _termCatConfService.Get(d => d.TERM_CAT_NAME == model.TERM_CAT_NAME).TERM_CAT_CONF_ID,
                    //    SD_ID = ProjectProvider.Instance.Current.SD_ID
                    //}; 
                    #endregion
                    var result = _othThermMapService.Inserts(model);
                    return result.Item1 ? Success() : Error(result.Item2);
                }
            }
            //编辑
            else
            {
                var result = _othThermMapService.EditTermMapInfo(model);
                return result.Item1 ? Success() : Error(result.Item2);
                #region qiaolei
                //if (_othThermMapService.Exists(b => (b.OTH_TERM_CODE == model.OTH_TERM_CODE || b.OTH_TERM_NAME == model.OTH_TERM_NAME) && b.SD_ID == ProjectProvider.Instance.Current.SD_ID && b.OTH_TERM_MAP_ID != model.OTH_TERM_MAP_ID))
                //{
                //    return Error("编码或名称不能重复！");
                //}
                //else
                //{
                //    #region 编码为空随机给

                //    Random ro = new Random();
                //    int iResult;
                //    int iUp = 9999;
                //    int iDown = 1000;
                //    iResult = ro.Next(iDown, iUp);
                //    string catName = "";
                //    // var codeMax = GetManay(b => b.TERM_CAT_CONF_ID == confId).Select(b => b.OTH_TERM_CODE).Max();
                //    if (model.TERM_CAT_NAME.Contains("心电图"))
                //    {
                //        catName = "XDT_" + iResult;
                //    }
                //    else if (model.TERM_CAT_NAME.Contains("经颅多普勒"))
                //    {
                //        catName = "JLD_" + iResult;
                //    }
                //    else if (model.TERM_CAT_NAME.Contains("颅腔CT"))
                //    {
                //        catName = "LCCT_" + iResult;
                //    }
                //    else
                //    {
                //        catName = "DEF_" + iResult;
                //    }
                //    #endregion
                //    if (model.OTH_TERM_CODE != null)
                //    {
                //        UN_SD_OTH_TERM_MAP termcatconf = _othThermMapService.GetWithTrace(d => d.OTH_TERM_MAP_ID == model.OTH_TERM_MAP_ID);
                //        termcatconf.OTH_TERM_CODE = model.OTH_TERM_CODE;
                //        termcatconf.OTH_TERM_NAME = model.OTH_TERM_NAME;
                //        termcatconf.TERM_CAT_CONF_ID = _termCatConfService.GetWithTrace(d => d.TERM_CAT_NAME == model.TERM_CAT_NAME && d.SD_ID == ProjectProvider.Instance.Current.SD_ID).TERM_CAT_CONF_ID;
                //        var result = _othThermMapService.Update(termcatconf);
                //        return result > 0 ? Success() : Error();
                //    }
                //    else
                //    {
                //        UN_SD_OTH_TERM_MAP termcatconf = _othThermMapService.GetWithTrace(d => d.OTH_TERM_MAP_ID == model.OTH_TERM_MAP_ID);
                //        termcatconf.OTH_TERM_CODE = catName;
                //        termcatconf.OTH_TERM_NAME = model.OTH_TERM_NAME;
                //        termcatconf.TERM_CAT_CONF_ID = _termCatConfService.GetWithTrace(d => d.TERM_CAT_NAME == model.TERM_CAT_NAME && d.SD_ID == ProjectProvider.Instance.Current.SD_ID).TERM_CAT_CONF_ID;
                //        var result = _othThermMapService.Update(termcatconf);
                //        return result > 0 ? Success() : Error();

                //    }

                //}
                #endregion
            }
        }
        /// <summary>
        /// Form 修改页面获取
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetForm(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var entity = _othThermMapService.Get(item => item.OTH_TERM_MAP_ID == id);
            return Content(new CHECKDIC_VIEWMODEL
            {
                OTH_TERM_MAP_ID = id,
                TERM_CAT_NAME = _termCatConfService.Get(b => b.TERM_CAT_CONF_ID == (int)entity.TERM_CAT_CONF_ID).TERM_CAT_NAME,
                OTH_TERM_CODE = entity.OTH_TERM_CODE,
                OTH_TERM_NAME = entity.OTH_TERM_NAME
            }.ToJson());
        }
        [HttpGet]
        public ActionResult GetList(int page, int limit, string field, string type, string keyWord = "")
        {
            var pageData = _othThermMapService.GetPage(page, limit, field, type, keyWord);
            var result = new Dictionary<string, object>() {
                { "code", 0},
                { "msg","success"},
                { "data", pageData.Items},
                { "count", pageData.TotalItems}
            };
            return Content(result.ToJson());
        }
        [HttpPost]
        public ActionResult GetTypeNameList(string typeId)
        {
            var ItemTermName = _othThermMapService.GetTyptName(typeId);
            return Content(ItemTermName.ToJson());
        }

        public ActionResult Delete(string primaryKey)
        {
            var id = int.Parse(primaryKey.Trim());
            var row = _othThermMapService.ExDelete(r => r.OTH_TERM_MAP_ID == id);
            return row > 0 ? Success() : Error();
        }

        #region 类别维护
        /*-----------------------------类别维护-------------------------------------------*/
        /// <summary>
        /// 类别
        /// </summary>
        [HttpGet]
        public ActionResult CategoryIndex()
        {
            //ViewBag.ItemType = _othThermMapService.GetTyptName();
            return View();
        }
        /// <summary>
        /// 类别显示
        /// </summary>
        [HttpGet]
        public ActionResult CategoryGetList(int page, int limit, string field, string type, string keyWord = "")
        {
            var pageData = _othThermMapService.CategoryGetPage(page, limit, field, type, keyWord);
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
        public ActionResult CategoryFrom(CHECKDIC_VIEWMODEL model)
        {
            //新增
            if (string.IsNullOrEmpty(model.TERM_CAT_CONF_ID.ToString()) || model.TERM_CAT_CONF_ID == 0)
            {
                if (_termCatConfService.Exists(d => d.TERM_CAT_NAME == model.TERM_CAT_NAME && d.TERM_TYPE == 2 && d.SD_ID == ProjectProvider.Instance.Current.SD_ID))
                {
                    return Error("该条检查类别已存在!");
                }
                else
                {
                    var catConf = _termCatConfService.GetManay(d => d.TERM_TYPE == 2 && d.SD_ID == ProjectProvider.Instance.Current.SD_ID);
                    var catConfig = _termCatConfService.Insert(new UN_SD_TERM_CAT_CONF
                    {
                        TERM_TYPE = 2,
                        TERM_CAT_CODE = catConf.Count > 0 ? catConf.Max(r => int.Parse(r.TERM_CAT_CODE ?? "0") + 1).ToString() : "1",//(_termCatConfService.GetManay(d => d.TERM_TYPE == 2 && d.SD_ID == ProjectProvider.Instance.Current.SD_ID).Max(r => int.Parse(r.TERM_CAT_CODE??"0")) + 1).ToString(),
                        TERM_CAT_NAME = model.TERM_CAT_NAME,
                        SD_ID = ProjectProvider.Instance.Current.SD_ID
                    });
                    return catConfig != null ? Success() : Error();
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
                    #region 旧版
                    //SD_OTH_TERM_MAP termcatconf = _othThermMapService.GetWithTrace(d => d.OTH_TERM_MAP_ID == model.OTH_TERM_MAP_ID);
                    //termcatconf.OTH_TERM_CODE = model.OTH_TERM_CODE;
                    //termcatconf.OTH_TERM_NAME = model.OTH_TERM_NAME;
                    //termcatconf.TERM_CAT_CONF_ID = _termCatConfService.GetWithTrace(d => d.TERM_CAT_NAME == model.TERM_CAT_NAME && d.SD_ID == ProjectProvider.Instance.Current.SD_ID).TERM_CAT_CONF_ID; 
                    #endregion

                    var confn = _termCatConfService.GetWithTrace(r => r.TERM_CAT_CONF_ID == model.TERM_CAT_CONF_ID);
                    confn.TERM_CAT_NAME = model.TERM_CAT_NAME;
                    var Catconfig = _termCatConfService.Update(confn);
                    return Catconfig > 0 ? Success() : Error();
                }
            }
        }
        /// <summary>
        /// 修改页面内容
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
       // [HttpPost]
        public ActionResult CategoryGetForm(string primaryKey)
        {
            var id = int.Parse(primaryKey);
            var entity = _termCatConfService.Get(item => item.TERM_CAT_CONF_ID == id);
            return Content(new CHECKDIC_VIEWMODEL
            {
                TERM_CAT_CONF_ID = id,
                TERM_CAT_NAME = entity.TERM_CAT_NAME,
                TERM_CAT_CODE = entity.TERM_CAT_CODE
            }.ToJson());
        }

        public ActionResult CategoryDelete(string primaryKey)
        {
            if (_othThermMapService.Exists(b => b.TERM_CAT_CONF_ID.ToString() == primaryKey))
            {
                return Error("该类别已使用，不能删除!");
            }
            var id = int.Parse(primaryKey.Trim());
            var row = _termCatConfService.ExDelete(r => r.TERM_CAT_CONF_ID == id);
            return row > 0 ? Success() : Error();
        }
        #endregion
    }
}