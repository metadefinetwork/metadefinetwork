var CustomerController = function () {
    this.initialize = function () {
        loadData();
        registerEvents();
    }

    function registerEvents() {

        $('#txt-search-keyword').keypress(function (e) {
            if (e.which === 13) {
                e.preventDefault();
                loadData(true);
            }
        });

        $("#ddl-show-page").on('change', function () {
            be.configs.pageSize = $(this).val();
            be.configs.pageIndex = 1;
            loadData(true);
        });

        $('body').on('click', '.btn-delete', function (e) {
            deleteCustomer(e, this);
        });

        $('body').on('click', '.btn-edit', function (e) {
            showSettingModal(e, this);
        });

        $('body').on('change', '.setting-2fa-check', function (e) {
            if (!$('.setting-2fa-check').is(":checked")) {
                let userId = $('#hidId').val();
                updateSettingUser(userId)
                this.disabled = true;
                $('.setting-2fa-status').text('Disable')
            } 
        });
    };

    function showSettingModal(e, element) {

        let userId = $(element).attr('data-id');

        $('#hidId').val(userId);

        loadSettingUser(userId);

        $('#modal-user-setting').modal('show');
    }

    function loadSettingUser(id) {
        $.ajax({
            type: "GET",
            url: `/admin/user/GetCustomerSetting?id=${id}`,
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                $('.setting-email').text(response.Email)
                $('.setting-2fa-check').attr('checked',response.TwoFactorEnabled)

                let status = response.TwoFactorEnabled ? 'Enable' : 'Disable'
                $('.setting-2fa-status').text(status)

                if (response.TwoFactorEnabled) {
                    $('.setting-2fa-check').removeAttr('disabled');
                }

                be.stopLoading();

                if (response.RowCount) {
                    be.wrapPaging(response.RowCount, function () {
                        loadData();
                    }, isPageChanged);
                }
            },
            error: function (message) {
                be.notify(`jqXHR.responseText: ${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function updateSettingUser(id) {

        var data = {
            Id: id,
            Enable2FA: false
        }

        $.ajax({
            type: "POST",
            url: `/admin/user/UpdateCustomerSetting`,
            data: JSON.stringify(data),
            contentType: "application/json",
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                be.stopLoading();

                be.notify('Update success!', 'success');
            },
            error: function (message) {
                be.notify(`jqXHR.responseText: ${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function deleteCustomer(e, element) {
        e.preventDefault();
        be.confirm('Delete member', 'You want to delete this member?', function () {
            $.ajax({
                type: "POST",
                url: "/Admin/User/DeleteCustomer",
                data: { id: $(element).data('id') },
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {

                    be.stopLoading();

                    if (response.Success) {
                        be.notify(response.Message, 'success');

                        loadData(true);
                    }
                    else {
                        be.notify(response.Message, 'error');
                    }
                },
                error: function (message) {
                    be.notify(`jqXHR.responseText: ${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        });
    }

    function loadData(isPageChanged) {
        $.ajax({
            type: "GET",
            url: "/admin/user/GetAllCustomerPaging",
            data: {
                keyword: $('#txt-search-keyword').val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            dataType: "json",
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                var template = $('#table-template').html();
                var render = "";

                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        Id: item.Id,
                        Email: item.Email,
                        Sponsor: item.Sponsor,
                        MainPublishKey: item.MainPublishKey,
                        BNBBEP20PublishKey: item.BNBBEP20PublishKey,
                        MainBalance: item.MainBalance,
                        BNBAffiliateBalance: item.BNBAffiliateBalance,
                        LockBalance: be.formatCurrency(item.LockBalance),
                        InvestBalance: be.formatCurrency(item.InvestBalance),
                        TokenAffiliateBalance: be.formatCurrency(item.TokenAffiliateBalance),
                        TokenCommissionBalance: be.formatCurrency(item.TokenCommissionBalance),
                        StakingBalance: be.formatCurrency(item.StakingBalance),
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                        EmailConfirmed: be.getEmailConfirmed(item.EmailConfirmed)
                    });
                });

                $("#lbl-total-records").text(response.RowCount);

                $('#tbl-content').html(render);

                be.stopLoading();

                if (response.RowCount) {
                    be.wrapPaging(response.RowCount, function () {
                        loadData();
                    }, isPageChanged);
                }
            },
            error: function (message) {
                be.notify(`jqXHR.responseText: ${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}