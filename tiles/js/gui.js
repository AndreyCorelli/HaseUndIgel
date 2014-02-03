function customConfirm(customMessage, title, buttonOkTitle, buttonCancelTitle, processResult) {
    var dfd = new jQuery.Deferred();
    $("#popUp").html(customMessage);
    $("#popUp").dialog({
        title: title,
        resizable: false,
        height: 220,
        width: 340,
        modal: true,
        buttons:
            [ { text: buttonOkTitle, click: function () {
                $(this).dialog("close");
                processResult(0);
                dfd.resolve();
            } },
            { text: buttonCancelTitle, click: function () {
                $(this).dialog("close");
                processResult(1);
                dfd.resolve();
            } }
            ]
    });
    return dfd.promise();
}