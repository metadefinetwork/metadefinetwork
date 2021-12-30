var FishingController = function () {
    this.initialize = function () {
        loadWalletBlance();
        loadData();
        loadItemStaking();
        registerEvents();
        registerControl();
        loadItemGame();
        loadHistoryStaking();
    }

    function registerControl() {

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

    function registerEvents() {
        $('#txt-search-keyword').keypress(function (e) {
            if (e.which === 13) {
                e.preventDefault();
                loadData(true);
            }
        });

        $('body').on('change', "#ddl-show-page", function () {
            be.configs.pageSize = $(this).val();
            be.configs.pageIndex = 1;
            loadData(true);
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
        $('body').on('click', '.item-list-buy', function (e) {
            showitemstak(e, this);
        });
        $('body').on('click', '#buy-staking', function (e) {
            buyStaking(e, this);
        });
        $('body').on('click', '#buy-item', function (e) {
            buyItem(e, this);
        });
        $("#walletType-staking").on('change', function (e) {
            loadWalletBlanceByType($(this).val(),"staking");
        });
        $("#walletType-buy").on('change', function (e) {
            loadWalletBlanceByType($(this).val(), "buy");
        });
        $('body').on('click', '[data-bs-toggle="tab"]', function (e) {
            this.querySelector('[type="radio"]').checked = true;
        });
        $('body').on('click', '#search-item', function (e) {
            loadData(true);
        });
    }
    function showitemstak(e, element) {
        e.preventDefault();

        var group = $(element).attr('data-item-group');
        if (group == 1) {
            $(".item-buy-name").html($(element).attr('data-item-name'));
            $("#item-image").removeClass();
            $("#item-image").addClass($(element).attr('data-item-class'));
            $("#first-plan").prop("checked", true);
            $("#AmountPaymentStaking").val($(element).attr('data-item-price'));
            $("#ItemId-Staking").val($(element).attr('data-item-id'));
            $("#kt_modal_buy_staking").modal("show");
        } else if (group == 2) {
            $(".item-buy-name").html($(element).attr('data-item-name'));
            $("#item-image").removeClass();
            $("#item-image").addClass($(element).attr('data-item-class'));
            $("#first-plan").prop("checked", true);
            $("#AmountPaymentBuy").val($(element).attr('data-item-price'));
            console.log($(element).attr('data-item-id'))
            $("#ItemId-Buy").val($(element).attr('data-item-id'));
            $("#kt_modal_buy_item").modal("show");
        }
    }

    function loadData(isPageChanged) {

        $.ajax({
            type: 'GET',
            data: {
                keyword: $('#txt-search-keyword').val(),
                type: $('#type-item').val(),
                group: $('#group-item').val(),
                price: $('#price-item').val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/admin/fishing/GetItemAllPaging',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                var template = $('#item-template').html();
                var render = "";
                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        ClassName: item.ClassName,
                        Function: item.Type == 5 ? '<div class="' + item.ClassName + '"><img class="Character_' + item.ClassName +'" src="/images/games/fish/' + item.ClassName+'.png" alt="Character" /></div>' : '<div class="mb-5 ' + item.ClassName +'"></div>',
                        Id: item.Id,
                        Name: item.Name,
                        Price: item.Price,
                        TypeName: item.TypeName,
                        StatusName: item.StatusName,
                        GroupItem: item.GroupItem,
                    });
                });

                $('#lbl-total-records').text(response.RowCount);

                $('#item-content').html(render);

                be.stopLoading();

                if (response.RowCount)
                    be.wrapPaging(response.RowCount, function () {
                        loadData();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function loadWalletBlanceByType(type,name) {

        $.ajax({
            type: 'GET',
            url: '/admin/fishing/GetWalletBlanceByType',
            dataType: 'json',
            data: { type: type },
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                if (name == "buy") {
                    $("#WalletBalanceBuy").val(response);
                } else {
                    $("#WalletBalanceStaking").val(response);
                }
                be.stopLoading();
                //CaculationStakingByType();
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
            url: '/admin/fishing/GetWalletBlance',
            dataType: 'json',
            success: function (response) {
                $("#WalletBalanceBuy").val(response.MARBalance);
                $("#WalletBalanceStaking").val(response.MARBalance);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
            }
        });
    }

    function buyStaking() {

        var data = {
            PeriodId: $('input[name="plan"]:checked').val(),
            ItemId: $('#ItemId-Staking').val(),
            WalletType: parseInt($('#walletType-staking').val())
        };
        var isValid = true;
        if (!data.WalletType) {
            isValid = be.notify('Wallet type is required', 'error');
        }
        if (!data.PeriodId) {
            isValid = be.notify('Time Line is required', 'error');
        }
        if (!data.ItemId) {
            isValid = be.notify('Package is required', 'error');
        }

        if (isValid) {

            $.ajax({
                type: "POST",
                url: '/admin/fishing/BuyStaking',
                dataType: "json",
                data: { modelJson: JSON.stringify(data) },
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    be.stopLoading();

                    if (response.Success) {

                        be.success('Buy Item Game', response.Message, function () {
                            window.location.reload();
                        });
                    }
                    else {
                        be.error('Buy Item Game', response.Message);
                    }
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        }
    }

    function buyItem() {

        var data = {
            ItemId: $('#ItemId-Buy').val(),
            WalletType: parseInt($('#walletType-buy').val())
        };

        var isValid = true;

        if (!data.WalletType) {
            isValid = be.notify('Wallet type is required', 'error');
        }
        if (!data.ItemId) {
            isValid = be.notify('Package is required', 'error');
        }

        if (isValid) {

            $.ajax({
                type: "POST",
                url: '/admin/fishing/BuyItemGame',
                dataType: "json",
                data: { modelJson: JSON.stringify(data) },
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    be.stopLoading();
                    if (response.Success) {

                        be.success('Buy', response.Message, function () {
                            window.location.reload();
                        });
                    }
                    else {
                        be.error('Buy', response.Message);
                    }
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        }
    }

    function loadItemStaking(isPageChanged) {
        $.ajax({
            type: 'GET',
            data: {
                group: 1,
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/admin/fishing/GetMyItemAllPaging',
            dataType: 'json',
            beforeSend: function () {
                //be.startLoading();
            },
            success: function (response) {
                var template = $('#item-template-staking').html();
                var render = "";
                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        ClassName: item.ItemStaking.ClassName,
                        Function: item.ItemStaking.Type == 5 ? '<div class="' + item.ItemStaking.ClassName + '"><img class="Character_' + item.ItemStaking.ClassName + '" src="/images/games/fish/' + item.ItemStaking.ClassName + '.png" alt="Character" /></div>' : '<div class="mb-5 ' + item.ItemStaking.ClassName + '"></div>',
                        Id: item.ItemStaking.Id,
                        Name: item.ItemStaking.Name,
                        Price: item.ItemStaking.Price,
                        TypeName: item.ItemStaking.TypeName,
                        StatusName: item.StatusName,
                        GroupItem: item.ItemStaking.GroupItem,
                        DateCreated: be.dateFormatJson(item.DateCreated),
                    });
                });

                $('#lbl-total-records').text(response.RowCount);

                $('#item-staking').html(render);

                be.stopLoading();

                if (response.RowCount)
                    be.wrapPaging(response.RowCount, function () {
                        loadData();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function loadItemGame(isPageChanged) {
        $.ajax({
            type: 'GET',
            data: {
                group: 2,
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/admin/fishing/GetMyItemAllPaging',
            dataType: 'json',
            beforeSend: function () {
                //be.startLoading();
            },
            success: function (response) {
                var template = $('#item-template-game').html();
                var render = "";
                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        ClassName: item.ItemStaking.ClassName,
                        Function: item.ItemStaking.Type == 5 ? '<div class="' + item.ItemStaking.ClassName + '"><img class="Character_' + item.ItemStaking.ClassName + '" src="/images/games/fish/' + item.ItemStaking.ClassName + '.png" alt="Character" /></div>' : '<div class="mb-5 ' + item.ItemStaking.ClassName + '"></div>',
                        Id: item.ItemStaking.Id,
                        Name: item.ItemStaking.Name,
                        Price: item.ItemStaking.Price,
                        TypeName: item.ItemStaking.TypeName,
                        StatusName: item.StatusName,
                        GroupItem: item.ItemStaking.GroupItem,
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                    });
                });
                $('#lbl-total-records').text(response.RowCount);
                $('#item-game').html(render);
                be.stopLoading();

                if (response.RowCount)
                    be.wrapPaging(response.RowCount, function () {
                        loadData();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function loadHistoryStaking(isPageChanged) {
        $.ajax({
            type: 'GET',
            data: {
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/admin/fishing/GetHistoryStacking',
            dataType: 'json',
            beforeSend: function () {
                //be.startLoading();
            },
            success: function (response) {
                var template = $('#table-template-history-staking').html();
                var render = "";
                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        ItemName: item.ItemInfo.Name,
                        Amount: item.Amount,
                        InterestRate: item.InterestRate,
                        ReceivedAmount: item.ReceivedAmount,
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                    });
                });
                $('#lbl-total-records').text(response.RowCount);
                $('#tbl-content-staking').html(render);
                be.stopLoading();

                if (response.RowCount)
                    be.wrapPaging(response.RowCount, function () {
                        loadData();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}