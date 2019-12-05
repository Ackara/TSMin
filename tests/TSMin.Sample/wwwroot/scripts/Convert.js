var TSMin;
(function (TSMin) {
    var Convert = /** @class */ (function () {
        function Convert() {
        }
        Convert.toMoney = function (value) {
            return "$" + value;
        };
        return Convert;
    }());
    TSMin.Convert = Convert;
})(TSMin || (TSMin = {}));
