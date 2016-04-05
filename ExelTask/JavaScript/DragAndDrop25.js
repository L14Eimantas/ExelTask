window.templateCount = $("#customInput").attr("value");

$(document).ready(function () {
    $.ajax({
        type: "GET",
        url: "/ActionWithColumn/PageLoad",
        dataType: "json",
        success: function (data) {
            if (data.check == true) { // check if required mapping cofiguration table have data
                for (var i = 0; i < data.num; i++) {  //data.num  is cofigurations table (mapping) rows count 
                    $.ajax({
                        type: "post",
                        dataType: "json",
                        url: "/ActionWithColumn/GetData",  // GetData gets columns and actions string (this strings saves / * - and 1 23 6 45
                        data: {
                            num: i,
                        },
                        success: function (configdata) {
                            var columns = configdata.columns.split(" ");  // gets cell table ID from configurations table columns string
                            var signs = configdata.signs.split(" ");  // get actions signs + / * -
                            for (var j = 0; j < columns.length - 1; j++) {  // -1 because last data is ""
                                $.ajax({
                                    async: false,  // very important
                                    type: "post",
                                    dataType: "json",
                                    url: "/ActionWithColumn/GetName",
                                    data: {
                                        num: columns[j],
                                    },
                                    success: function (column) {
                                        var $li = $('<li class="btn btn-danger2"></li>');
                                        $li.html(column.name);
                                        var position = $('div#' + configdata.Where).find('li').length;  // gets sign position in string
                                        $li.attr('id', position); // id element position in Columns string
                                        $li.attr('value', configdata.Where); // value is Where copy this element
                                        $li.appendTo('#' + configdata.Where + '.dropme');
                                        var $model = $('<select class="btn"> <option value="+">+</option> <option value="-">-</option> <option value="*">*</option> <option value="/">/</option> </select>');
                                        $model.val(signs[j]);
                                        var name = $('div#' + configdata.Where).find('select').length;  // gets sign position in string
                                        $model.attr('name', configdata.Where); // name is where
                                        $model.attr('id', name); // id is position
                                        if (j != columns.length - 2) {  // because last sign we dont want see
                                            $model.appendTo('#' + configdata.Where + '.dropme');
                                        }
                                    }
                                });
                            }
                        },
                    });
                }
            }
        },
    });
});

$('button#allow').button().draggable({
    cancel: false,
    cursor: 'pointer',
    connectWith: '.dropme',
    helper: 'clone',
    opacity: 0.5,
    zIndex: 10
});

$(".dropme").droppable({
    accept: '#allow', //allow drop just items who have id allow
    hoverClass: 'highlight',
    drop: function (event, ui) {
        var WhatCopy = ui.draggable.attr("value");
        var WhereCopy = this.id;
        var $li = $('<li class="btn btn-danger2"></li>').html(ui.draggable.html());
        var position = $(this).find('li').length;
        $li.attr('id', position);
        $li.attr('value', WhereCopy); // say where copy element
        var $model = $('<select class="btn"> <option value="+">+</option> <option value="-">-</option> <option value="*">*</option> <option value="/">/</option> </select>');
        $model.attr('name', WhereCopy); //add  anme to select
        var name = $(this).find('select').length; // how many select tag is in one div
        var check = (position == 0 || name == position);  //true or false (chech if at least one button is in div)
        $model.attr('id', name);
        $.ajax({
            type: "post",
            dataType: "json",
            url: "/ActionWithColumn/CheckAndFill",
            data: {
                ColumnWhat: WhatCopy,
                ColumnWhere: WhereCopy,
            },
            success: function () {
                if (check)  // if it first button in div
                    $li.appendTo('#' + WhereCopy + '.dropme');
                else {
                    $model.appendTo('#' + WhereCopy + '.dropme');
                    $li.appendTo('#' + WhereCopy + '.dropme');
                }

            },
        });

    }

});

// this function starts then user select sign
$("body").on("change", 'select', function () {
    var value = $(this).val(); //  find slected sign value(+,-,/,*) from dropdownlist
    var name = this.id;// name is correct position where sign has changed 
    var ID = this.name; // ID says where copy data (by select name)
    $.ajax({
        type: "post",
        dataType: "json",
        url: "/ActionWithColumn/ChangeActionSign",
        data: {
            columnWhere: ID,
            sign: value,
            where: name,
        },
        success: function () {
        },
    });
});


function deleteGraficalElement(position, where) {
    var tet = $('#' + where + ".dropme").find('li');
    var tet1 = $('#' + where + ".dropme").find('select');
    var length = $('#' + where).find('select').length;
    for (var i = 0; i < tet.length; i++) {
        if (tet[position].id == tet[i].id) {
            tet[i].remove();
        }
    }
    if (position < length) {
        for (var i = 0; i < tet1.length; i++) {
            if (tet1[position].id == tet1[i].id)
                tet1[i].remove();
        }
    }

}

function updateGraficalElementPosition(where) {  // graficalElement is action dropdownlist and drop columns
    var tet = $('#' + where + ".dropme").find('li');
    var tet1 = $('#' + where + ".dropme").find('select');
    var seip = $('#' + where).find('li').length;
    var seip1 = $('#' + where).find('select').length;
    var ii = 0;
    for (var i = 0; i < tet.length; i++) {
        if (i < seip) {
            tet[i].id = i;
        }
        else {
            tet[i].id = ii;
            ii++;
        }
        if (ii == (seip))
            ii = 0;
    }
    ii = 0;
    for (var i = 0; i < tet1.length; i++) {
        if (i < seip1) {
            tet1[i].id = i;
        }
        else {
            tet1[i].id = ii;
            ii++;
        }
        if (ii == seip1)
            ii = 0;
    }
}


function  deleteAndUpdateOne(where,This,position)
{
    This.remove();
            var data = $('#' + where + '.dropme').find('select');
            data[position].remove();
            var NEW = $('div#' + where).find('li');
            var NEW2 = $('#' + where + '.dropme').find('select');
            for (var i = 0; i < NEW.length; i++) {   //for loop need to update element position (li and select) 
                NEW[i].id = i;
                NEW2[i].id = i;
            }
}



$("div.dropme").on("click", "li", function () {   //method that delete one element
    var position = this.id;
    var where = this.value;
    var This = this;
    var t = ($('#' + where).find('li').length == 1);
    $.ajax({
        type: "post",
        dataType: "json",
        url: "/ActionWithColumn/DeleteItem",
        data: {
            where: where,
            position: position,
            t: t,
        },
        success: function () {
            if (templateCount != 1) {
                deleteGraficalElement(position, where);
                updateGraficalElementPosition(where);
            }
            else {
                deleteAndUpdateOne(where, This, position);
            }
        },
     });
});


$('#runclick').click(function () {  //Run actionlick click if (any inputs dont drop into template then button cant click
    var t;
    $.ajax({
        async: false,
        type: "GET",
        url: "/ActionWithColumn/PageLoad",
        dataType: "json",
        success: function (data) {
            t = data.check;
        },
    });
    if (t == false) {
        alert("You did not Drag and Drop any inputs into the output templates!");
    }
    return t;
});