using Elight.Entity;
using Elight.Infrastructure;
using Elight.Web.Controllers;
using Elight.Web.Filters;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Service.ComDataItem;
using TestingCenterSystem.Service.comm.Interface;
using TestingCenterSystem.Service.DataItem;
using TestingCenterSystem.Service.DataItem.Interface;
using TestingCenterSystem.Service.InGroup;
using TestingCenterSystem.Service.Project;
using TestingCenterSystem.Service.Project.Interface;
using TestingCenterSystem.ViewModels.ComDataItem;

namespace Elight.Web.Areas.TestCenterSystem.Controllers
{
    public class CommonItemController : BaseController
    {
        public IAddCommonItemService _itemService = new AddCommonItemService();
        public ISDService _sdService = new SDService();
        public ICatConfService _catconfService = new CatConfService();
        public IProcStateService _procStateService = new ProcStateService();
        public IProjectService _projectService = new ProjectService();
        public IItemOptionService _itemOptionService = new ItemOptionService();


        //[HttpGet, AuthorizeChecked]
        //public ActionResult Index()
        //{

        //    return View();
        //}
        //自定义数据项管理-公共添加按钮响应
        [HttpGet, AuthorizeChecked]
        public ActionResult AddCommonItem()
        {
            ViewBag.SD_NAME = _projectService.GetCurrentSD().SD_NAME;
            ViewBag.TC_PROJECT_NAME = _projectService.GetCurrentSD().TC_PROJECT_NAME;
            return View();
        }


        public ActionResult GetCommonItemList(int pageIndex, int pageSize, string keyWord)
        {
            var pageData = _itemService.GetPage(pageIndex, pageSize, keyWord);
            var result = new LayPadding<AddCommonItemViewModel>()
            {
                result = true,
                msg = "success",
                list = pageData.Items,
                count = pageData.TotalItems
            };
            return Content(result.ToJson());
        }

        /// <summary>
        /// 提交表单
        /// </summary>
        /// <returns></returns>
        public ActionResult SubmitItem(List<int> ids)
        {
            //var result = _itemService.InsertItem(ids);
            //return result > 0 ? Success() : Error();
            return null;
        }

        /// <summary>
        /// 自定义数据项管理-修改修改数据项-公共部分
        /// </summary>
        /// <param name="strEnum"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet, AuthorizeChecked]
        public ActionResult UpdateCommonItem(int primaryKey)
        {
            ViewBag.ItemCount = primaryKey;
            //var sd_info = _sdService.Get(s => s.SD_ID = SD_ID);
            ViewBag.ItemType = _catconfService.GetSearchList(s => s.CAT_ID != null);
            return View();
        }

        /// <summary>
        /// 自定义数据项管理-修改修改数据项-公共部分
        /// </summary>
        /// <param name="strEnum"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult UpdateCommonItem(string strEnum, SD_ITEM_INFO model)
        {
            if (!string.IsNullOrWhiteSpace(strEnum))
            {
                var enumList = strEnum.Split(',');
                if (_itemOptionService.Exists(r => r.SD_ITEM_ID == model.SD_ITEM_ID))
                    _itemOptionService.Delete(r => r.SD_ITEM_ID == model.SD_ITEM_ID);
                for (var i = 0; i < enumList.Length; i++)
                {
                    var enumModel = new SD_ITEM_OPTION()
                    {
                        SD_ITEM_ID = model.SD_ITEM_ID,
                        ITEM_OPTION_NAME = enumList[i],
                        ORDER_NO = i + 1
                    };
                    var optionEntity = _itemOptionService.Insert(enumModel);
                }
            }
            //保存或修改并更新到数据库
            if (_itemService.Exists(r => r.SD_ITEM_ID == model.SD_ITEM_ID))
            {
                var row = _itemService.Update(model);
                return row > 0 ? Success() : Error();
            }
            else
            {
                model.SD_ITEM_SRC = 2;
                var entity = _itemService.Insert(model);
                return entity != null ? Success() : Error();
            }
        }


        //公共数据项管理
        public ActionResult CommonManage()
        {
            return View();
        }

        

        // GET: TestCenterSystem/ItemDepend
        public ActionResult ItemDetail(SD_ITEM_INFO itemInfo)
        {
            return View();
        }

        public ActionResult MappingRefrence()
        {
            return View();
        }
        ////添加映射
        //[HttpPost]
        public ActionResult AddCommonDepend(String list)
        {
            //将列表结果更新数据库
            //return Success();
            return View();
        }

        //添加公共数据项依赖
        //public ActionResult AddCommonDepend(int ItemCount)
        //{
        //    ViewBag.ItemCount = ItemCount;
        //    return View();
        //}


       
        [HttpPost]
        public ActionResult TestDllEvent()
        {
            return Content("");
        }
        [HttpPost]
        public ActionResult AddCommonItem(String list)
        {
            //将列表结果更新数据库
            return Success();
        }

        

        ////修改公共数据项
        //public ActionResult UpdateCommonItem(int ItemCount)
        //{
        //    ViewBag.ItemCount = ItemCount;
        //    return View();
        //}

        ////添加、修改、查看数据
        //public ActionResult UpdateCommonItem(int ItemCount = 1)
        //{
        //    ViewBag.ItemCount = ItemCount;
        //    //var sd_info = _sdService.Get(s => s.SD_ID = SD_ID);
        //    ViewBag.ItemType = _catconfService.GetSearchList(s => s.CAT_ID != null);
        //    return View();
        //}



    }
}