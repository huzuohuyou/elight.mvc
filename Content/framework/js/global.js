/// <reference path="jquery/jquery.min.js" />
//依赖项：jquery、layer、select2。

/**
 * 创建模态窗。
 * @param {Object} options
 */
$.layerOpen = function (options) {
    var defaults = {
        id: "default",
        title: '系统窗口',
        type: 2,
        //skin: 'layui-layer-molv',
        width: "auto",
        height: "auto",
        maxmin: false,
        content: '',
        shade: 0.3,
        btn: ['确认', '取消'],
        btnclass: ['btn btn-primary', 'btn btn-danger'],
        yes: null
    };
    var options = $.extend(defaults, options);
    top.layer.open({
        id: options.id,
        type: options.type,
        scrollbar: false,
        //skin: options.skin,
        shade: options.shade,
        shadeClose: false,
        maxmin: options.maxmin,
        title: options.title,
        fix: false,
        area: [options.width, options.height],
        content: options.content,
        btn: options.btn,
        btnclass: options.btnclass,
        yes: function (index, layero) {
            if (options.yes && $.isFunction(options.yes)) {
                var iframebody = top.layer.getChildFrame('body', index);
                var iframeWin = layero.find('iframe')[0].contentWindow;
                options.yes(iframebody, iframeWin, index);
            }
        },
        cancel: function () {
            return true;
        }
    });

}

/**
 * 关闭模态窗。
 */
$.layerClose = function () {
    var index = top.layer.getFrameIndex(window.name);
    top.layer.close(index);
}

/**
 * 创建询问框。
 * @param {Object} options
 */
$.layerConfirm = function (options) {
    var defaults = {
        title: '提示',
        //skin: 'layui-layer-molv',
        content: "",
        icon: 3,
        shade: 0.3,
        anim: 4,
        btn: ['确认', '取消'],
        btnclass: ['btn btn-primary', 'btn btn-danger'],
        callback: null
    };
    var options = $.extend(defaults, options);
    top.layer.confirm(options.content, {
        title: options.title,
        icon: options.icon,
        btn: options.btn,
        btnclass: options.btnclass,
        //skin: options.skin,
        anim: options.anim
    }, function () {
        if (options.callback && $.isFunction(options.callback)) {
            options.callback();

        }
    }, function () {
        return true;
    });
}
/**
 * 创建一个信息弹窗。
 * @param {String} content 
 * @param {String} type 
 */
$.layerMsg = function (content, type, callback) {
    debugger;
    if (type != undefined) {
        var icon = "";
        if (type == 'warning' || type == 0) {
            icon = 0;
        }
        if (type == 'success' || type == 1) {
            icon = 1;
        }
        if (type == 'error' || type == 2) {
            icon = 2;
        }
        if (type == 'info' || type == 6) {
            icon = 6;
        }
        top.layer.msg(content, { icon: icon, time: 2000 }, function () {
            if (callback && $.isFunction(callback)) {
                callback();
            }
        });
    } else {
        top.layer.msg(content, function () {
            if (callback && $.isFunction(callback)) {
                callback();
            }
        });
    }
}

/**
 * 绑定Select选项。
 * @param {Object} options
 */
$.fn.bindSelect = function (options) {
    var defaults = {
        id: "id",
        text: "text",
        search: true,
        //multiple: false,
        title: "请选择",
        url: "",
        param: [],
        change: null
    };
    var options = $.extend(defaults, options);
    var $element = $(this);
    if (options.url != "") {
        $.ajax({
            url: options.url,
            data: options.param,
            type: "post",
            dataType: "json",
            async: false,
            success: function (data) {
                //$element.append($("<option></option>").val("").html(options.title)) //zlt
                $.each(data, function (i) {
                    $element.append($("<option></option>").val(data[i][options.id]).html(data[i][options.text]));
                });
                $element.select2({
                    placeholder: options.title,
                    //multiple: options.multiple,
                    minimumResultsForSearch: options.search == true ? 0 : -1,
                });
                $element.on("change", function (e) {
                    if (options.change != null) {
                        options.change(data[$(this).find("option:selected").index()]);
                    }
                    $("#select2-" + $element.attr('id') + "-container").html($(this).find("option:selected").text().replace(/　　/g, ''));
                });
            }
        });
    } else {
        $element.select2({
            minimumResultsForSearch: -1
        });
    }
}

/**
 * 绑定Enter提交事件。
 * @param {Object} $event 需要触发的元素或事件。
 */
$.fn.bindEnterEvent = function ($event) {
    var $selector = $(this);
    $.each($selector, function () {
        $(this).unbind("keydown").bind("keydown", function (event) {
            if (event.keyCode == 13) {
                if ($.isFunction($event)) {
                    $event();
                } else {
                    $event.click();
                }
            }
        });
    });
}

/**
 * 绑定Change提交事件。
 * @param {Object} $event 需要触发的元素或事件。
 * 
 */
$.fn.bindChangeEvent = function ($event) {
    var $selector = $(this);
    $.each($selector, function () {
        $(this).unbind("change").bind("change", function (event) {
            if ($.isFunction($event)) {
                $event();
            } else {
                $event.click();
            }
        })
    });
},

/**
 * 控制授权按钮显示隐藏。
 */
$.fn.authorizeButton = function () {
    var url = top.$("iframe:visible").attr("src");
    //alert(url);
    var modules = top.client.permission;
    //alert(JSON.stringify(modules))
    var module = {};
    var childModules = [];
    for (var i = 0; i < modules.length; i++) {
        if (modules[i].Url != "") {
            if (url == modules[i].Url) {
                module = modules[i];
            }
        }
    }
    for (var i = 0; i < modules.length; i++) {
        if (modules[i].Url != "") {
            if (modules[i].ParentId == module.Id) {
                childModules.push(modules[i]);
            }
        }
    }
    if (childModules.length > 0) {
        var $toolbar = $(this);
        var _buttons = '';
        $.each(childModules, function (index, item) {
            _buttons += "<button id='" + item.EnCode + "' onclick='" + item.JsEvent + "' class=\"layui-btn layui-btn-primary layui-btn-small\">";
            _buttons += "   <i class='" + item.Icon + "' aria-hidden='true'></i> " + item.Name + "";
            _buttons += "</button>";
        });
        $toolbar.find('.layui-btn-group:last').html(_buttons);
    } else {
        //$toolbar.css('height', '50px');
    }
}

/**
 * 获取数据表格选中行主键值。
 */
$.fn.gridSelectedRowValue = function () {
    var $selectedRows = $(this).children('tbody').find("input[type=checkbox]:checked");
    var result = [];
    if ($selectedRows.length > 0) {
        for (var i = 0; i < $selectedRows.length; i++) {
            result.push($selectedRows[i].value);
        }
    }
    return result;
}

$.ExportSqlFile = function (fileName, content) {
    //var uri = 'data:sql/plain;charset=unicode,' + content;//encodeURIComponent(content);//  encodeURIComponent(excelFile);
    //var componet = encodeURIComponent(content);
    var link = document.createElement("a");
    var blob = new Blob([content], { type: "txt/plain;charset=gb2312" });
    link.download = fileName;
    link.href = URL.createObjectURL(blob);//"data:text/plain;charset=ANSI," + content;//uri;//
    link.charset = "gb2312";
    link.style = "visibility:hidden;";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

$.JsonFormat = function format(txt, compress/*是否为压缩模式*/) {/* 格式化JSON源码(对象转换为JSON文本) */
    var indentChar = '    ';
    if (/^\s*$/.test(txt)) {
        alert('数据为空,无法格式化! ');
        return;
    }
    try { var data = eval('(' + txt + ')'); }
    catch (e) {
        alert('数据源语法错误,格式化失败! 错误信息: ' + e.description, 'err');
        return;
    };
    var draw = [], last = false, This = this, line = compress ? '' : '\n', nodeCount = 0, maxDepth = 0;

    var notify = function (name, value, isLast, indent/*缩进*/, formObj) {
        nodeCount++;/*节点计数*/
        for (var i = 0, tab = ''; i < indent; i++) tab += indentChar;/* 缩进HTML */
        tab = compress ? '' : tab;/*压缩模式忽略缩进*/
        maxDepth = ++indent;/*缩进递增并记录*/
        if (value && value.constructor == Array) {/*处理数组*/
            draw.push(tab + (formObj ? ('"' + name + '":') : '') + '[' + line);/*缩进'[' 然后换行*/
            for (var i = 0; i < value.length; i++)
                notify(i, value[i], i == value.length - 1, indent, false);
            draw.push(tab + ']' + (isLast ? line : (',' + line)));/*缩进']'换行,若非尾元素则添加逗号*/
        } else if (value && typeof value == 'object') {/*处理对象*/
            draw.push(tab + (formObj ? ('"' + name + '":') : '') + '{' + line);/*缩进'{' 然后换行*/
            var len = 0, i = 0;
            for (var key in value) len++;
            for (var key in value) notify(key, value[key], ++i == len, indent, true);
            draw.push(tab + '}' + (isLast ? line : (',' + line)));/*缩进'}'换行,若非尾元素则添加逗号*/
        } else {
            if (typeof value == 'string') value = '"' + value + '"';
            draw.push(tab + (formObj ? ('"' + name + '":') : '') + value + (isLast ? '' : ',') + line);
        };
    };
    var isLast = true, indent = 0;
    notify('', data, isLast, indent, false);
    return draw.join('');
}

//大文件导出
$.JsonToCSV = function exportCsv(obj, fileName) {
    var title = obj.title;
    var titleForKey = obj.titleForKey;
    var data = obj.data;
    var str = [];
    str.push(title.join(",") + "\n");
    for (var i = 0; i < data.length; i++) {
        var temp = [];
        for (var j = 0; j < titleForKey.length; j++) {
            temp.push(data[i][titleForKey[j]]);
        }
        str.push(temp.join(",") + "\n");
    }
    var link = document.createElement("a");
    var blob = new Blob(["\ufeff" + str], { type: 'text/csv' }); //解决大文件下载失败
    link.setAttribute("href", URL.createObjectURL(blob));
    link.setAttribute("download", fileName);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

/**
 * 生成Excel文件。
 * @param {JSON} JSONData
 * @param {String} FileName
 * @param {JSON} ShowLabel
 */
$.JSONToExcelConvertor = function (JSONData, FileName, ShowLabel, ShowType) {
    //先转化json  
    var arrData = typeof JSONData != 'object' ? JSON.parse(JSONData) : JSONData;
    var excel = '<table>';
    var row = "";
    if (ShowType != undefined) {
        for (var i = 0, l = ShowType.length; i < l; i++) {
            for (var index in ShowType[i]) {
                row += "<td>" + ShowType[i][index] + '</td>';
                row += "<td></td>";
            }
        }
    }
    //设置表头  
    row += "<tr>";
    for (var i = 0, l = ShowLabel.length; i < l; i++) {
        for (var index in ShowLabel[i]) {
            row += "<td>" + ShowLabel[i][index] + '</td>';
        }
    }
    //换行  
    excel += row + "</tr>";
    //设置数据  
    for (var i = 0; i < arrData.length; i++) {
        var row = "<tr>";
        for (var index in arrData[i]) {
            var value = arrData[i][index] === "." ? "" : arrData[i][index];
            row += '<td style="mso-number-format:' + "'\\@';" + '">' + value + '</td>';
        }
        excel += row + "</tr>";
    }
    excel += "</table>";
    var excelFile = "<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:x='urn:schemas-microsoft-com:office:excel' xmlns='http://www.w3.org/TR/REC-html40'>";
    excelFile += '<meta http-equiv="content-type" content="application/vnd.ms-excel; charset=UTF-8">';
    excelFile += '<meta http-equiv="content-type" content="application/vnd.ms-excel';
    excelFile += '; charset=UTF-8">';
    excelFile += "<head>";
    excelFile += "<!--[if gte mso 9]>";
    excelFile += "<xml>";
    excelFile += "<x:ExcelWorkbook>";
    excelFile += "<x:ExcelWorksheets>";
    excelFile += "<x:ExcelWorksheet>";
    excelFile += "<x:Name>";
    excelFile += "sheet1";
    excelFile += "</x:Name>";
    excelFile += "<x:WorksheetOptions>";
    excelFile += "<x:DisplayGridlines/>";
    excelFile += "</x:WorksheetOptions>";
    excelFile += "</x:ExcelWorksheet>";
    excelFile += "</x:ExcelWorksheets>";
    excelFile += "</x:ExcelWorkbook>";
    excelFile += "</xml>";
    excelFile += "<![endif]-->";
    excelFile += "</head>";
    excelFile += "<body>";
    excelFile += excel;
    excelFile += "</body>";
    excelFile += "</html>";
    var link = document.createElement("a");
    var blob = new Blob(["\ufeff" + excelFile], { data: 'application/vnd.ms-excel' }); //解决大文件下载失败
    link.href = URL.createObjectURL(blob);
    link.download = FileName + ".xls";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

/**
 * 获取URL指定参数值。
 * @param {String} name
 */
$.getQueryString = function (name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(r[2]); return null;
}

/**
 * 序列化和反序列化表单字段。
 * @param {Object} formdate
 * @param {Function} callback 
 */
$.fn.formSerialize = function (formdate, callback) {
    var $form = $(this);
    if (!!formdate) {
        for (var key in formdate) {
            var $field = $form.find("[name=" + key + "]");
            if ($field.length == 0) {
                continue;
            }
            var value = $.trim(formdate[key]);
            var type = $field.attr('type');
            if ($field.hasClass('select2')) {
                type = "select2";
            }
            switch (type) {
                case "checkbox":
                    value == "true" ? $field.attr("checked", 'checked') : $field.removeAttr("checked");
                    break;
                case "select2":
                    if (!$field[0].multiple) {
                        $field.select2().val(value).trigger("change");
                    } else {
                        var values = value.split(',');
                        $field.select2().val(values).trigger("change");
                    }
                    break;
                case "radio":
                    $field.each(function (index, $item) {
                        if ($item.value == value) {
                            $item.checked = true;
                        }
                    });
                    break;
                default:
                    $field.val(value);
                    break;
            }

        };
        // 特殊的表单字段可以在回调函数中手动赋值。
        if (callback && $.isFunction(callback)) {
            callback(formdate);
        }
        return false;
    }
    var postdata = {};
    $form.find('input,select,textarea').each(function (r) {
        var $this = $(this);
        var id = $this.attr('id');
        var type = $this.attr('type');
        switch (type) {
            case "checkbox":
                postdata[id] = $this.is(":checked");
                break;
            default:
                var value = $this.val() == "" ? "&nbsp;" : $this.val();
                if (!$.getQueryString("id")) {
                    value = value.replace(/&nbsp;/g, '');
                }
                postdata[id] = value;
                break;
        }
    });
    //if ($('[name=__RequestVerificationToken]').length > 0) {
    //    postdata["__RequestVerificationToken"] = $('[name=__RequestVerificationToken]').val();
    //}
    return postdata;
}

/**
 * 提交表单。
 * @param {Object} options
 */
$.formSubmit = function (options) {
    var defaults = {
        url: "",
        data: {},
        type: "post",
        async: false,
        success: null,
        close: true,
        showMsg: true,
    };
    var options = $.extend(defaults, options);
    $.ajax({
        url: options.url,
        data: options.data,
        type: options.type,
        async: options.async,
        dataType: "json",
        success: function (data) {
            if (options.success && $.isFunction(options.success)) {
                options.success(data);
            }
            if (options.close) {
                $.layerClose();
            }
            if (options.showMsg) {
                $.layerMsg(data.message, data.state);
            }
        },
        error: function (xhr, status, error) {
            $.layerMsg(error, "error");
        },
        beforeSend: function () {

        },
        complete: function () {

        }
    });
}




