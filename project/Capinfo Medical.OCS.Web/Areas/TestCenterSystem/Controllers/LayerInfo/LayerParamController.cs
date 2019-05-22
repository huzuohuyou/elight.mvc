using Elight.Entity;
using Elight.Infrastructure;
using Elight.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestingCenterSystem.Models.PDP;
using TestingCenterSystem.Service.Layer;
using TestingCenterSystem.Service.Layer.Interface;
using TestingCenterSystem.ViewModels.Layer;

namespace Elight.Web.Areas.TestCenterSystem.Controllers.LayerInfo
{
    public class LayerParamController : BaseController
    {
       
        public static readonly ISdLayerParamService _sdLayerParamService = new SdLayerParamService();
        // GET: TestCenterSystem/LayerParam
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetList(int? layerId)
        {
            var temp = _sdLayerParamService.GetManay(R=>R.SD_LAYER_ID== layerId);
            var data = new List<LAYER_PARAM_VIEWMODEL>();
            temp.ForEach(r => {
                data.Add(new LAYER_PARAM_VIEWMODEL(r));
            });

            var result = new LayPadding<LAYER_PARAM_VIEWMODEL>()
            {
                result = true,
                msg = "success",
                list = data,
                count = data.Count
            };
           
            return Content(result.ToJson());
        }
        
        public ActionResult Add( int layerId, int dataitemid)
        {
            if (_sdLayerParamService.Exists(r => r.SD_LAYER_ID == layerId && r.SD_ITEM_ID == dataitemid))
            {
                return Error("数据项已存在！！！");
            }
            else
            {
                var entity = new SD_LAYER_PARAM() { SD_LAYER_ID = layerId, SD_ITEM_ID = dataitemid, SD_LAYER_PARAM_NAME = _sdLayerParamService.GetParamCode(layerId) };
                _sdLayerParamService.Insert(entity);
                return Success();
            }
        }

        public ActionResult Delete(string primaryKey)
        {
            int SD_LAYER_ID = int.Parse(primaryKey.Split('_')[0]);
            int SD_ITEM_ID = int.Parse(primaryKey.Split('_')[1]);
            int row = _sdLayerParamService.Delete(r => r.SD_LAYER_ID == SD_LAYER_ID&&r.SD_ITEM_ID==SD_ITEM_ID);
            return row > 0 ? Success() : Error();
        }
    }
}