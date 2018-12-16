jQuery.fn.forceNumeric = function () {
    return this.each(function () {
        $(this).keydown(function (e) {
            var key = e.which || e.keyCode;

            if (!e.shiftKey && !e.altKey && !e.ctrlKey &&
                // numbers
                key >= 48 && key <= 57 ||
                // Numeric keypad
                key >= 96 && key <= 105 ||
                // comma, period and minus, . on keypad
                // key == 190 || key == 188 || key == 109 || key == 110 ||
                // Backspace and Tab and Enter
                key == 8 || key == 9 || key == 13 ||
                // Home and End
                key == 35 || key == 36 ||
                // left and right arrows
                key == 37 || key == 39 || key == 38 || key == 40 ||
                // Del and Ins
                key == 46 || key == 45)
                return true;
            return false;
        });
    });
}

function byteLength(str) {
    // returns the byte length of an utf8 string
    var s = str.length;
    for (var i = str.length - 1; i >= 0; i--) {
        var code = str.charCodeAt(i);
        if (code > 0x7f && code <= 0x7ff) s++;
        else if (code > 0x7ff && code <= 0xffff) s += 2;
        if (code >= 0xDC00 && code <= 0xDFFF) i--; //trail surrogate
    }
    return s;
}

function addCommas(nStr) {
    nStr += '';
    x = nStr.split('.');
    x1 = x[0];
    x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + ',' + '$2');
    }
    return x1 + x2;
}
function RemoveRougeChar(convertString) {
    if (convertString.substring(0, 1) == ",") {
        return convertString.substring(1, convertString.length)
    }
    return convertString;
}
