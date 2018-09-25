var drugOrderItems = new Array();

function buildDrugItemCode(drug) {
    return String.format('DrugID{0}Unit{1}', drug.ID, drug.SelectedUnit);
}

function buildDrugItemFullCode(drug) {
    return String.format('DrugID{0}Unit{1}_{2}', drug.ID, drug.Unit, drug.BatchUnit);
}

function updateTotalPrice(drug) {
    var price = drug.CurrentPrice;
    drug.TotalPrice = price * drug.Quantity;
}

function getDrug(barcode) {   
    return window.drugs[barcode];
}

function getDrugUnitName(drug) {
    return drug.SelectedUnit == drug.Unit ? drug.UnitName : drug.BatchUnitName;
}

function getDrugPrice(drug) {
    return (drug.SelectedUnit == drug.Unit || drug.Unit == drug.BatchUnit) ? drug.Price : drug.BatchPrice;
}
//type: 0 - barcode, 1 - mã thuốc
function handleInputBarcode(sData,type) {
    if (type === 0) {
        $.post($('input[name=baseUrl]').val() + "PhieuXuats/GetThuocsByBarCode?sDrugBarCode=" + sData).done(function (data) {
            if (data) {
                createDrugItem(data);
            }
            else {
                alert("Không tồn tại thuốc bạn nhập trong danh sách thuốc.");
            }
        });
    }
    else {
        $.post($('input[name=baseUrl]').val() + "PhieuXuats/GetThuocsByCode?sDrugCode=" + sData).done(function (data) {
            if (data) {
                createDrugItem(data);
            }
            else {
                alert("Không tồn tại thuốc bạn nhập trong danh sách thuốc.");
            }
        });
    }
    //var drug = getDrug(barcode);
    //if (drug == null) {
    //    alert("Không tồn tại thuốc có mã vạch '" + barcode + "' trong danh sách thuốc.");
    //} else {
    //    createDrugItem(drug);
    //}
} 

function createDrugItemRow(drug, editMode, itemCode) {
    var trString =
        String.format('<tr data-drugitemcode="{0}" id ="row{1}">', drug.ItemCode, drug.ItemCode)
            + '<td style="padding: 10px 0">'
                + '<a class="edit-drug display-mode" style="cursor: pointer" id="0" title="Sửa"><i class="glyphicon glyphicon-pencil"></i></a>'
                + '<a class="save-drug edit-mode text-success" style="cursor: pointer" id="0" title="Lưu"><i class="glyphicon glyphicon-ok"></i></a>&nbsp'
                + '<a class="delete-drug edit-mode text-danger" style="cursor: pointer" id="0" title="Xóa"><i class="glyphicon glyphicon-trash"></i></a>&nbsp'
            + '</td>'

            + '<td>'
                //+ '<span id="spOrdinalNumber"></span>'
                + '<a href="javascript:void(0)" action="/Thuocs/DialogDetail?id=' + drug.ID + '" id="spOrdinalNumber" class="a_view"></a>'
            + '</td>'

            + '<td>'
                + String.format('<span id="spDrugCode">{0}</span>', drug.Code)
            + '</td>'

            + '<td>'
                + String.format('<span id="spDrugName">{0}</span>', drug.Name)
            + '</td>'

            + '<td>'
                + String.format('<span class="display-mode" id="spUnit{0}">{1}</span>', drug.ItemCode, getDrugUnitName(drug))
                + String.format('<select onchange="onUnitChanged(this, this.value)" class="form-control edit-mode valid" data-val="true" data-val-number="The field Đơn Vị Tính must be a number." data-val-required="The Đơn Vị Tính field is required." id="ddlUnits{0}" style="display: none;">', drug.ItemCode)
                + ((drug.UnitName && drug.UnitName.length != 0)? String.format('<option value="{0}">{1}</option>', drug.Unit, drug.UnitName):'')
                + ((drug.BatchUnitName && drug.BatchUnitName.length != 0 && drug.BatchUnit != drug.Unit) ? String.format('<option value="{0}">{1}</option>', drug.BatchUnit, drug.BatchUnitName) : '')
                + '</select>'
            + '</td>'

            + '<td>'
                + String.format('<span class="display-mode" id="spQuantity{0}">{1}</span>', drug.ItemCode ,drug.Quantity)
                + String.format('<input onkeyup="handleQuantityChanged(this, event);" class="form-control edit-mode text-box single-line" data-val="true" data-val-number="The field Số Lượng must be a number." data-val-required="The Số Lượng field is required." id="tbxQuantity{0}" type="text" value="1" style="display: none;">', drug.ItemCode)
            + '</td>'

            + '<td>'
                + String.format('<span  data-inputmask="\'alias\': \'decimal\', \'groupSeparator\': \',\', \'autoGroup\': true" class="display-mode pNumber" style="padding-right: 40px;" format="number" id="spPrice{0}">{1}</span>', drug.ItemCode, $.number(drug.CurrentPrice, 0, '.', ','))
                + String.format('<input  data-inputmask="\'alias\': \'decimal\', \'groupSeparator\': \',\', \'autoGroup\': true" onkeyup="handleQuantityChanged(this, event);" class="form-control edit-mode text-box single-line" data-val="true" data-val-number="The field Số Lượng must be a number." data-val-required="Chưa nhập đơn giá." id="tbxPrice{0}" type="text" value="{1}" style="display: none;">', drug.ItemCode, drug.CurrentPrice)
            + '</td>'

            + '<td>'
                + String.format('<b  data-inputmask="\'alias\': \'decimal\', \'groupSeparator\': \',\', \'autoGroup\': true" class="pNumber" format="number" id="itemTotalPrice{0}">{1}</b>', drug.ItemCode, $.number(drug.TotalPrice, 0, '.', ','))
            + '</td>'
        + '</tr>';
    if (editMode) {
        var rowId = "#row" + itemCode;
        var tr = $(rowId);
        if (tr != null) {
            var rowIndex = tr[0].rowIndex;
            $('#barcode-table-id > tbody > tr').eq(rowIndex - 1).after(trString);              
        }       
    }
    else {
        $("table#drugItems tbody").append(trString);
        var newAddedTr = $("table#drugItems tr:last");
        newAddedTr.find('.edit-mode').hide();
    }
}

function handleQuantityChanged(sender, e) {
    var charCode;
    //Get key code (support for all browsers)
    if (e && e.which) {
        charCode = e.which;
    } else if (window.event) {
        e = window.event;
        charCode = e.keyCode;
    }

    if (charCode == 13) {
        if (sender.parentElement && sender.parentElement.parentElement) {
            var tr = $(sender.parentElement.parentElement);
            saveDrugItemRow(tr);
            $("#barcode").focus();
        }
    }
}

function onUnitChanged(sender, val) {
    var selUnitVal = val;
    var tr = sender.parentNode.parentNode;
    var drugItemCode = tr.getAttribute('data-drugitemcode');
    var drugs = findDrugItem(drugItemCode);
    if (drugs.length == 1) {
        var drug = drugs[0];
        drug.SelectedUnit = selUnitVal;
        drug.CurrentPrice = getDrugPrice(drug);
        updateTotalPrice(drug);
        $("#spUnit" + drug.ItemCode).text(getDrugUnitName(drug));
        var price = $.number(drug.CurrentPrice, 0, '.', ',');
        $("#spPrice" + drug.ItemCode).text(price);
        $("#tbxPrice" + drug.ItemCode).text(price);
        $("#tbxPrice" + drug.ItemCode).val(price);
        $("#itemTotalPrice" + drug.ItemCode).text($.number(drug.TotalPrice, 0, '.', ','));
    }      
}

function findDrugItem(drugItemCode) {
    var res = new Array();
    for (var i = 0; i < drugOrderItems.length; i++) {
        if (drugOrderItems[i].ItemCode == drugItemCode) {
            res.push(drugOrderItems[i]);
        }
    }
    return res;
}

function createDrugItem(drug) {
    drug.OldItemCode = drug.ItemCode;
    drug.ItemCode = buildDrugItemCode(drug);
    var resDrugItems = findDrugItem(drug.ItemCode);
    switch(resDrugItems.length) {
        case 0:
            addNewDrugItem(drug);
            break;
        case 1:
            editExistingDrugItem(resDrugItems[0], drug);
            break;
        case 2:
            mergeTwoDrugItems(resDrugItems[0], resDrugItems[1]);
            break;
        default:
    }
    updateTotalPrices();
}

function editExistingDrugItem(drugItem, drug) {
    if (drugItem == drug) { // Edit existing drug item.
        drugItem.Quantity = drug.Quantity;
    } else {  // Combine drugs
        drugItem.Quantity += drug.Quantity;
    }
    updateTotalPrice(drugItem);
    if (drugItem.OldItemCode == drugItem.ItemCode) {
        updateDrugItemRow(drugItem);
    } else {
        //removeDrugItemRow(drugItem.OldItemCode);
        addNewDrugItemRow(drugItem, true, drugItem.OldItemCode);
        drugItem.OldItemCode = drugItem.ItemCode;
    }
}

function mergeTwoDrugItems(drugItem1, drugItem2) {
    var removeItem = drugItem1;
    var maintainItem = drugItem2;
    if (drugItem2.OldItemCode != drugItem2.ItemCode) {
        removeItem = drugItem2;
        maintainItem = drugItem1;
    }

    maintainItem.Quantity += removeItem.Quantity;
    updateTotalPrice(maintainItem);
    updateDrugItemRow(maintainItem);
    removeDrugItem(removeItem);
    removeDrugItemRow(removeItem.OldItemCode);
    rebuildOrdinalNumbers();
}

function addNewDrugItem(drug) {
    var drug2Add = jQuery.extend(true, {}, drug); // Deep clone drug object.
    drug2Add.OldItemCode = drug2Add.ItemCode;
    updateTotalPrice(drug2Add);
    drugOrderItems.push(drug2Add);
    createDrugItemRow(drug2Add, false);
    rebuildOrdinalNumbers();
}

function addNewDrugItemRow(drug, editMode, itemCode) {
    createDrugItemRow(drug, editMode, itemCode);
    rebuildOrdinalNumbers();
}

function removeDrugItem(drugItem) {
    var index = drugOrderItems.indexOf(drugItem);
    if (index > -1) {
        drugOrderItems.splice(index, 1);
    }
}

function removeDrugItemRow(drugItemCode) {
    deleteRow("#row" + drugItemCode);
}

function updateDrugItemRow(drugItem) {
    $("#ddlUnit" + drugItem.ItemCode).value = drugItem.SelectedUnit;
    $("#ddlUnit" + drugItem.ItemCode).text(getDrugUnitName(drugItem));
    $("#itemTotalPrice" + drugItem.ItemCode).text($.number(drugItem.TotalPrice, 0, '.', ','));
    $("#tbxQuantity" + drugItem.ItemCode).text(drugItem.Quantity);
    $("#tbxQuantity" + drugItem.ItemCode).val(drugItem.Quantity);
    $("#spQuantity" + drugItem.ItemCode).text(drugItem.Quantity);
    var price = $.number(drugItem.CurrentPrice, 0, '.', ',');
    $("#tbxPrice" + drugItem.ItemCode).text(price);
    $("#tbxPrice" + drugItem.ItemCode).val(price);
    $("#spPrice" + drugItem.ItemCode).text(price);
}

function rebuildOrdinalNumbers() {
    var table = $('#drugItems')[0],
    rows = table.getElementsByTagName('tr'),
    text = 'textContent' in document ? 'textContent' : 'innerText';
    if (rows.length > 1) {
        for (var i = 1, len = rows.length; i < len; i++) {
            //rows[i].children[1][text] = i;
            $(rows[i]).find("a[id='spOrdinalNumber']").text(i);
        }
    }
    InitBtnViewDetailThuoc();
}

function calTotalPrices() {
    var total = 0.0;
    for (var i = 0; i < drugOrderItems.length; i++) {
        total += drugOrderItems[i].TotalPrice;
    }

    return total;
}

function updateTotalPrices() {
    var total = calTotalPrices();
    $("#spanSumAll").text(numberWithCommas(total));
}

function deleteRow(rowid) {
    var tr = $(rowid);
    if (tr != null) {
        tr.remove();
    }
}

function saveDrugItemRow(tr) {
    var drugItemCode = tr.data('drugitemcode');
    var drugs = findDrugItem(drugItemCode);
    if (drugs.length == 1) {
        var drug = drugs[0];
        drug.Quantity = parseFloat($("#tbxQuantity" + drug.ItemCode).val());
        drug.SelectedUnit = parseInt($("#ddlUnits" + drug.ItemCode).val());
        var price = 0.0;
        price = parseFloat($("#tbxPrice" + drug.ItemCode).val().replace(/,/g, ""));
        drug.CurrentPrice = price;
        updateTotalPrice(drug);
        createDrugItem(drug);
        tr.find('.edit-mode, .display-mode').toggle();
    }
    $("#barcode").focus();
}

function initAndBindEvents() {
    //Edit row
    $('body').off('click', '.edit-drug');
    $('body').on('click', '.edit-drug', function () {
        var tr = $(this).parents('tr:first');
        var drugItemCode = tr.data('drugitemcode');
        var drugs = findDrugItem(drugItemCode);
        if (drugs.length == 1) {
            tr.find('.edit-mode, .display-mode').toggle();
            var drug = drugs[0];
            updateDrugItemRow(drug);
            tr.find("#tbxQuantity" + drugItemCode).focus();
        }
    });
    
    //Save the row
    $('body').off('click', '.save-drug');
    //$('body').off('keyup', '.lastInput');
    //$('body').on('keyup', '#drugItems tbody tr:eq(5) input', function (event) {
    //    if (event.keyCode == 13) {
    //        var tr = $(this).parents('tr:first');
    //        saveDrugItemRow(tr);
    //    }
    //});

    $('body').on('click', '.save-drug', function () {
        var tr = $(this).parents('tr:first');
        saveDrugItemRow(tr);
    });
    
    //Delete the row
    $('body').off('click', '.delete-drug');
    $('body').on('click', '.delete-drug', function () {
        var tr = $(this).parents('tr:first');
        var drugItemCode = tr.data('drugitemcode');
        var drugs = findDrugItem(drugItemCode);
        if (drugs.length == 1) {
            var drug = drugs[0];
            removeDrugItem(drug);
            $(tr).remove();
            rebuildOrdinalNumbers();
            updateTotalPrices();
            //tr.find('.edit-mode, .display-mode').toggle();
        }
    });
    $("span.pNumber").each(function () {
        $(this).html(numberWithCommas($(this).html()));
    });
    $("b.pNumber").each(function () {
        $(this).html(numberWithCommas($(this).html()));
    });
   
}
var isClick;
function InitBtnViewDetailThuoc() {
    $('.a_view').unbind().on('click', function () {
        $.get($(this).attr("action")).done(function (data) {
            if (data) {
                $(data).hide().appendTo(document.body).on('hide.bs.modal', function () { $('.modal-backdrop').remove(); }).modal();
            }
        });
    });
}

function BindNgayTaoPosition() {
    $(document.body).on('click', '.daterange-picker', function (e) {
        var reportDatePicker = $(e.currentTarget).find('input');
        var currDate = moment($("#NgayXuat").val(), DEFAULT_MOMENT_DATE_FORMAT).toDate();
        reportDatePicker.datepicker("update", currDate);
        reportDatePicker.datepicker('show');
    });

    $('.daterange-picker input').datepicker({
        format: DEFAULT_DATE_PICKER_FORMAT,
        changeMonth: true,
        changeYear: true,
        endDate: 0,
        maxViewMode: 2,
        defaultDate: new Date(),
        minDate: MIN_PRODUCTION_DATA_DATE,
        language: 'vi',
        autoclose: true
    }).on('changeDate', function (dateEvent) {
        var selectedDate = moment(dateEvent.date).format(DEFAULT_MOMENT_DATE_FORMAT);
        $("#inputDate").text(selectedDate);
        $("#NgayXuat").val(selectedDate);
    });
}

function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",").replace('.00', '');
}


//end region