var ExchangeController = function () {
    this.initialize = function () {
        loadWalletBlance();
        registerEvents();
        registerControl();
    }
    function registerControl() {
        $(".numberFormat").each(function () {
            var numberValue = parseFloat($(this).html().replace(/,/g, ''));
            if (!numberValue) {
                $(this).html(be.formatCurrency(0));
            }
            else {
                $(this).html(be.formatCurrency(numberValue));
            }
        });

        $(".numberFormat").each(function () {
            var numberValue = parseFloat($(this).val().replace(/,/g, ''));
            if (!numberValue) {
                $(this).val(be.formatCurrency(0));
            }
            else {
                $(this).val(be.formatCurrency(numberValue));
            }
        });
    }

    var registerEvents = function () {

        $('.numberFormat').on("keypress", function (e) {
            var keyCode = e.which ? e.which : e.keyCode;
            var ret = ((keyCode >= 48 && keyCode <= 57) || keyCode == 46);
            if (ret)
                return true;
            else
                return false;
        });

        $(".numberFormat").focusout(function () {
            var numberValue = parseFloat($(this).val().replace(/,/g, ''));
            if (!numberValue) {
                $(this).val(be.formatCurrency(0));
            }
            else {
                $(this).val(be.formatCurrency(numberValue));
            }
        });

        $("#OrderBNB").focusout(function () {
            var OrderBNB = parseFloat($(this).val());
            if (isNaN(OrderBNB)) {
                OrderBNB = 0;
            }
            $('#OrderBNB').val(OrderBNB);
            CaculationByBNB(OrderBNB);
        });

        $("#btnMax").on('click', function (e) {

            var OrderBNB = $('#BalanceBNB').val();
            $('#OrderBNB').val(OrderBNB);

            CaculationByBNB(OrderBNB);
        });

        $("#walletType").on('change', function (e) {
            loadWalletBlanceByType($(this).val());
        });

        $("#btnBuy").on('click', function (e) {

            var byVM = {
                Type: $('#walletType').val(),
                Amount: parseFloat($('#OrderBNB').val()),
            };

            var isValid = true;

            if (!byVM.Amount) {
                isValid = be.notify('Amount is required', 'error');
            }
            else {
                if (byVM.Amount < 0.1) {
                    isValid = be.notify('Minimum buy 0.1 BNB', 'error');
                }
                else {
                    if (byVM.Amount > 6) {
                        isValid = be.notify('Maximum buy 6 BNB', 'error');
                    }
                }
            }

            if (!byVM.Type) {
                isValid = be.notify('Wallet type is required', 'error');
            }

            if (isValid) {

                $.ajax({
                    type: "POST",
                    url: '/Admin/Exchange/By',
                    dataType: "json",
                    data: { modelJson: JSON.stringify(byVM) },
                    beforeSend: function () {
                        be.startLoading();
                    },
                    success: function (response) {
                        be.stopLoading();

                        if (response.Success) {
                            be.success('Buy ', response.Message, function () {
                                window.location.reload();
                            });
                        }
                        else {
                            be.error('Buy ', response.Message);
                        }
                    },
                    error: function (message) {
                        be.notify(`${message.responseText}`, 'error');
                        be.stopLoading();
                    }
                });
            }
        });
    }

    function CaculationByBNB(OrderBNB) {

        var data = {
            OrderBNB: OrderBNB
        };

        if (data.OrderBNB > 0) {

            $.ajax({
                type: "GET",
                url: "/Admin/Exchange/CaculationByBNB",
                dataType: "json",
                data: { modelJson: JSON.stringify(data) },
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    $('#Amount').val(be.formatCurrency(response.Amount));
                    be.stopLoading();
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });

        } else {
            $('#Amount').val(be.formatCurrency(data.OrderBNB));
        }
    }

    function loadWalletBlanceByType(type) {
        $.ajax({
            type: 'GET',
            url: '/admin/Exchange/GetWalletBlanceByType',
            dataType: 'json',
            data: { type: type },
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                $('#BalanceBNB').val(response);
                $('#OrderBNB').val(0);
                $('#Amount').val(be.formatCurrency(0));
                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function loadWalletBlance() {
        $.ajax({
            type: 'GET',
            url: '/admin/Exchange/GetWalletBlance',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                $('#BalanceBNB').val(response.MainBalance);
                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}