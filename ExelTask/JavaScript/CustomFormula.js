window.glogal = "";
window.names = $('textarea#hid').val().split(',');

$('button#allow, button#allow2').button().draggable({
    cancel: false,
    cursor: 'pointer',
    connectWith: '.dropme',
    helper: 'clone', 
    opacity: 0.5,
    zIndex: 10
});

$(".dropme").droppable({ 
    accept: '#allow, #allow2', //allow drop just items who have id allow
    drop: function (event, ui) {
        var indificator = ui.draggable.attr("id");
        var li = $('<text></text>').html(ui.draggable.html()).text(); 
        var valueGlogol = ui.draggable.attr("value");
        writeTextarea(indificator, valueGlogol, li);
    }
});

$("button#allow, button#allow2").on("click", function () {  // user just can click required button
    var indificator = this.id;
    var valueGlogol = $(this).attr("value");
    var text = $(this).text();
    writeTextarea(indificator, valueGlogol, text);
});


function writeTextarea(indificator, valueGlogol, text) { //write on textarea
    if (indificator == "allow2") {
        var li = text + "(";
    }
    else {
        var li = text;
        glogal += (valueGlogol + " ");
    }
    var newtext = $('textarea.dropme').val() + li;
    $('textarea.dropme').val(newtext);
}




$('textarea.dropme').on('keypress', function () {  //then user press backspace delete char or column name
}).on('keyup', function (e) {
    if (e.keyCode == 8) {
        var alltext = $('textarea.dropme').val();
        var text = $('textarea.dropme').val();
        text = text.replace(/(\r\n|\n|\r)/gm, "");
        var titles = getTitles(text);
        $.ajax({
            type: "post",
            dataType: "json",
            url: "/CustomFormula/CheckCellId",
            data: {
                cellId: glogal,
                titles: titles,
                alltext:alltext,
            },
            success: function (data) {
                glogal = data.answer; // get right cellId string
                $('textarea.dropme').val(data.newtext);
            },
        });
    }
});


function getTitles(text) {  // this function from user write data, take just dropped culumn name and add to textarea (html)
    var titles = "";
    var interim = "";
    for (var i = 0; i < text.length; i++) {
        if (text[i] == "(" || text[i] == ")" || text[i] == "," || text[i] == ":" || text[i] == "+" || text[i] == "-" || text[i] == "/" || text[i] == "*" || text[i] == "^") {
            titles += ";";
            if (text[i] != "(")
                interim = "";
        }
        else {
            titles += text[i];
            interim += text[i];
        }
        if (text[i]=="(")
        {
            titles=titles.replace(interim,"");
            interim = "";
        }
    }
    return titles.replace(/(\r\n|\n|\r)/gm, "");
}


$('button#test').click(function () {  // this is use check if formula is correct
    var text = $('textarea.dropme').val();
    text = text.replace(/(\r\n|\n|\r)/gm, "");
    var titles = getTitles(text);
    var mId = $('textarea.dropme').attr("name");
    $.ajax({
        type: "post",
        dataType: "json",
        url: "/CustomFormula/FormulaTest",
        data: {
            text: text,
            titles: titles,
            cellId: glogal,
            mId:mId,
        },
        success: function (data) {
            if (data.TorF == true)
            {
                $('button#done').removeAttr("disabled");// then formula is correct
                $('div#answer').empty();
                $('div#answer').append("Test was successful");
            }
            else
            {
                $('button#done').attr("disabled", true);
                $('div#answer').empty();
                $('div#answer').append("Test was not successful! Check the formula or data");
            }
        },
    });

 
});

function GetTitlesWithoutsemicolon() { 
    var text = $('textarea.dropme').val();
    text=text.replace(/(\r\n|\n|\r)/gm,"");
    var titles = getTitles(text);
    $.ajax({
        async: false,
        type: "post",
        dataType: "json",
        url: "/CustomFormula/GetTitles",
        data: {
            titles:titles,
        },
        success: function (data) {
            titles = data.titles;
         
        }
    });
    return titles.replace(/(\r\n|\n|\r)/gm, "");
}


$('button#done').click(function () {

    var mId = $('textarea.dropme').attr("name"); //Get mappingId
    var whereCopy = $('textarea.dropme').attr("Id");// get wherecopy information in database
    var whatCopy = $('textarea.dropme').val();
    whatCopy = whatCopy.replace(/(\r\n|\n|\r)/gm, "");
    var titles = GetTitlesWithoutsemicolon();
    whatCopy = whatCopy.replace(/\s/g, ''); //replace all spaces
    $.ajax({
        type: "post",
        async: false,
        dataType: "json",
        url: "/ActionWithColumn/CheckAndFill",
        data: {
            ColumnWhat: whatCopy,
            ColumnWhere: whereCopy,
        },
        success: function () {
            $.ajax({
                type: "post",
                dataType: "json",
                url: "/ActionWithColumn/CustomaAddData",
                data: {
                    culumns: glogal,
                    names: titles,
                    where:whereCopy,
                },
                success: function () {
                }
            });
            location.href = "/ActionWithColumn/MainPageColumn?Id=" + mId; //goes to MainPageColumn actionmethod (controller ActionWithColumn)
            }
    });
   
});

$('textarea.dropme').highlightTextarea({  // this library method add background color to worksheet columns names

    words: names,
    color: '#C4CFF5'
});