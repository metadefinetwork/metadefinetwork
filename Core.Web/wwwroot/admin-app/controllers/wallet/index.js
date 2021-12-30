var WalletController = function () {
    this.initialize = function () {
        loadWalletBlance();
        loadTicketTransactionData();
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
            var numberValue = parseFloat($(this).html().replace(/,/g, ''));
            if (!numberValue) {
                $(this).val(be.formatCurrency(0));
            }
            else {
                $(this).val(be.formatCurrency(numberValue));
            }
        });

        jQuery('#qrcodeBNBPublishKey').qrcode({
            text: $("#txtBNBPublishKey").val()
        });

        jQuery('#qrcodeMVRPublishKey').qrcode({
            text: $("#txtMVRPublishKey").val()
        });

        jQuery('#qrcodeMARPublishKey').qrcode({
            text: $("#txtMARPublishKey").val()
        });
    }

    var registerEvents = function () {

        $('body').on('change', "#ddl-show-page", function () {
            be.configs.pageSize = $(this).val();
            be.configs.pageIndex = 1;
            loadTicketTransactionData(true);
        });

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

        $('body').on('click', '#btnWithdrawMAR', function (e) {
            if (checkEnabled2FA()) {
                showModalWithdrawMAR()
            }
        });
        $('body').on('click', '#btnConfirmWithdrawMAR', function (e) {

            var isValid = validateWithdrawMAR();

            if (!isValid) return;

            hideModalWithdrawMAR();

            be.verifyCodeAndPassword(confirmWithdrawMAR);
        });

        $("#txtMARAmount").focusout(function () {

            var mar = parseFloat($('.MARBalance').val().replace(/,/g, ''));

            var amount = parseFloat($(this).val().replace(/,/g, ''));

            var marFeeAmount = amount * 0.02;
            var marReceiveAmount = amount - marFeeAmount;

            if (amount > mar) {
                $(".lblMARErrorInsufficient").html("Insufficient account balance");
            }
            else {
                $(".lblMARErrorInsufficient").html("");
            }

            $('#txtMARFeeAmount').val(marFeeAmount);
            $('#txtMARReceiveAmount').val(marReceiveAmount);
        });
    }

    function validateWithdrawMAR() {
        var WithdrawMARVM = {
            MARBalance: parseFloat($('.MARBalance').val().replace(/,/g, '')),
            Amount: parseFloat($('#txtMARAmount').val().replace(/,/g, '')),
            AddressTo: $('#txtMARAddressTo').val()
        };

        var isValid = true;

        if (WithdrawMARVM.Amount <= 0) {
            isValid = be.notify('Amount is required', 'error');
        }
        else {
            if (WithdrawMARVM.Amount < 10000) {
                isValid = be.notify('Minimum withdraw 10,000 MAR', 'error');
            }
        }

        if (WithdrawMARVM.Amount > WithdrawMARVM.MARBalance) {
            isValid = be.notify('Insufficient account balance', 'error');
        }

        if (!WithdrawMARVM.AddressTo) {
            isValid = be.notify('Please, update the wallet address in your profile', 'error');
        }

        return isValid;
    }

    function confirmWithdrawMAR() {
        var WithdrawMARVM = {
            Amount: parseFloat($('#txtMARAmount').val().replace(/,/g, '')),
            AddressTo: $('#txtMARAddressTo').val(),
            Password: $('#be-hidden-password').val()
        };

        var code = $('#be-hidden-2faCode').val();

        var url = '/Admin/Wallet/WithdrawWalletMAR?authenticatorCode=' + code;

        $.ajax({
            type: "POST",
            headers: {
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            url: url,
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify(WithdrawMARVM),
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                be.stopLoading();

                if (response.Success) {
                    be.success('Withdraw MAR', response.Message, function () {
                        window.location.reload();
                    });
                }
                else {
                    be.error('Withdraw MAR', response.Message);
                }
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function showModalWithdrawMAR() {
        $("#ic_modal_withdraw_mar").modal("show");
    }
    function hideModalWithdrawMAR() {
        $("#ic_modal_withdraw_mar").modal("hide");
    }

    function checkEnabled2FA() {
        var isEnabled2FA = $("#Enabled2FA").val();
        if (isEnabled2FA) {
            return true;
        }
        else {
            be.error("Two-factor authentication (2FA)", "Your account has not enabled 2FA, please go to the profile page to enable.");
            return false;
        }
    }

    function loadTicketTransactionData(isPageChanged) {

        $.ajax({
            type: 'GET',
            data: {
                keyword: $('#txt-search-keyword').val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/admin/wallet/GetAllTicketTransactionPaging',
            dataType: 'json',
            beforeSend: function () {
            },
            success: function (response) {
                var template = $('#table-template').html();
                var render = "";
                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        UserName: item.AppUserName,
                        TypeName: item.TypeName,
                        UnitName: item.UnitName,
                        StatusName: be.getTicketStatus(item.Status),
                        Sponsor: item.Sponsor,
                        AddressFrom: item.AddressFrom,
                        AddressTo: item.AddressTo,
                        Amount: item.Amount,
                        AmountReceive: item.AmountReceive,
                        Fee: (item.Fee * 100),
                        FeeAmount: item.FeeAmount,
                        DateUpdated: be.dateTimeFormatJson(item.DateUpdated),
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                    });
                });

                $('#lbl-total-records').text(response.RowCount);

                $('#tbl-content').html(render);


                if (response.RowCount)
                    be.wrapPaging(response.RowCount, function () {
                        loadTicketTransactionData();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
            }
        });
    }

    function loadWalletBlance() {
        $.ajax({
            type: 'GET',
            url: '/admin/Wallet/GetWalletBlance',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                be.stopLoading();

                if (response.Success) {
                    $('#MARBalance').html(be.formatCurrency(response.Data.MARBalance));
                    $('.MARBalance').val(be.formatCurrency(response.Data.MARBalance));

                    $('#MVRBalance').html(be.formatCurrency(response.Data.MVRBalance));
                    $('.MVRBalance').val(be.formatCurrency(response.Data.MVRBalance));

                    $('#BNBBalance').html(be.formatCurrency(response.Data.BNBBalance));
                    $('.BNBBalance').val(be.formatCurrency(response.Data.BNBBalance));
                }
                else {
                    be.notify(response.Message, 'error');
                }
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}