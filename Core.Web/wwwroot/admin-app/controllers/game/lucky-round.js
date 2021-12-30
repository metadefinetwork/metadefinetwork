var LuckyRoundController = function () {
    this.initialize = function () {
        getTicket();
        loadWalletBlanceByType(1);
        loadLuckyRoundHistoryData();
        registerEvents();
        registerControl();
    }

    function registerControl() {
    }

    var registerEvents = function () {
        $('body').on('change', "#ddl-show-page", function () {
            be.configs.pageSize = $(this).val();
            be.configs.pageIndex = 1;
            loadLuckyRoundHistoryData(true);
        });

        $("#spin_button").on('click', function (e) {
            startSpin(1);
        });
        $("#spin_10_button").on('click', function (e) {
            startSpin(10);
        });
        $("#spin_all_button").on('click', function (e) {
            startSpin(0);
        });

        $("#reset_button").on('click', function (e) {
            resetWheel();
        });

        $('body').on('click', '#btnBuyTicket', function (e) {
            showModalBuyTicket()
        });

        $("#walletType").on('change', function (e) {
            loadWalletBlanceByType($(this).val());
        });

        $("#TicketOrder").focusout(function () {
            CaculationTicketByType();
        });

        $("#btnConfirmBuyTicket").on('click', function (e) {
            var data = {
                TicketOrder: parseInt($('#TicketOrder').val()),
                Type: parseInt($('#walletType').val())
            };
            debugger;
            var isValid = true;

            if (!data.TicketOrder) {
                isValid = be.notify('Ticket Order is required', 'error');
            }
            else {
                if (data.TicketOrder < 5) {
                    isValid = be.notify('Minimum buy 5 TICKET', 'error');
                }
            }

            if (!data.Type) {
                isValid = be.notify('Wallet type is required', 'error');
            }

            if (isValid) {

                $.ajax({
                    type: "POST",
                    url: '/Admin/Game/BuyTicket',
                    dataType: "json",
                    data: { modelJson: JSON.stringify(data) },
                    beforeSend: function () {
                        be.startLoading();
                    },
                    success: function (response) {
                        be.stopLoading();

                        if (response.Success) {

                            hideModalBuyTicket();

                            be.success('Buy Ticket', response.Message, function () {
                                window.location.reload();
                            });
                        }
                        else {
                            be.error('Buy Ticket', response.Message);
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

    function CaculationTicketByType() {

        var ticketOrder = parseInt($('#TicketOrder').val());
        var type = $('#walletType').val();

        if (isNaN(ticketOrder)) {
            ticketOrder = 0;
        }

        $('#TicketOrder').val(ticketOrder);

        var data = {
            TicketOrder: ticketOrder,
            Type: type
        };

        if (data.TicketOrder > 0) {

            $.ajax({
                type: "GET",
                url: "/Admin/game/CaculationTicketByType",
                dataType: "json",
                data: { modelJson: JSON.stringify(data) },
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    $('#AmountPayment').val(response.AmountPayment);
                    be.stopLoading();
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });

        } else {
            $('#AmountPayment').val(data.TicketOrder);
        }
    }

    function showModalBuyTicket() {
        $("#ic_modal_buy_ticket").modal("show");
    }
    function hideModalBuyTicket() {
        $("#ic_modal_buy_ticket").modal("hide");
    }

    function loadWalletBlanceByType(type) {
        $.ajax({
            type: 'GET',
            url: '/admin/game/GetWalletBlanceByType',
            dataType: 'json',
            data: { type: type },
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                $('#WalletBalance').val(response);
                //$('#TicketOrder').val(0);
                //$('#AmountPayment').val(be.formatCurrency(0));
                be.stopLoading();

                CaculationTicketByType();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    var theWheel = new Winwheel({
        'outerRadius': 156,
        'innerRadius': 58,
        'textFontSize': 14,
        'textFontWeight': 800,
        'responsive': true,
        'textMargin': 10,
        'textDirection': 'reversed',
        'textAlignment': 'inner',
        'numSegments': 10,
        'segments':
            [
                { 'fillStyle': '#151521', 'text': 'Good Luck', 'textFillStyle': '#ffffff' },
                { 'fillStyle': '#3cb878', 'text': '50 ' },
                { 'fillStyle': '#f6989d', 'text': '500 ' },
                { 'fillStyle': '#00aef0', 'text': '1000 ' },
                { 'fillStyle': '#f26522', 'text': '3000 ' },
                { 'fillStyle': '#4f9004', 'text': '5000 ', 'textFillStyle': '#ffffff' },
                { 'fillStyle': '#6fba18', 'text': '10000 ', 'textFillStyle': '#ffffff' },
                { 'fillStyle': '#2196f3', 'text': 'TICKET', 'textFillStyle': '#ffffff' },
                { 'fillStyle': '#ffc107', 'textFontSize': 16, 'text': '0.5 BNB' },
                { 'fillStyle': '#ffeb3b', 'textFontSize': 18, 'text': '1 BNB' }
            ],
        'animation': {
            'type': 'spinToStop',
            'duration': 10,
            'spins': 10,
            'callbackFinished': alertPrize,
            'callbackSound': playSound,
            'soundTrigger': 'pin'
        },
        'pins': {
            'number': 10,
            'fillStyle': 'hsl(54deg 100% 50%)',
            'outerRadius': 6,
            'responsive': true,
        }
    });

    var playAudio = new Audio('/Winwheel/examples/wheel_of_fortune/tick.mp3');
    var winAudio = new Audio('/Winwheel/examples/wheel_of_fortune/win.wav');
    var loseAudio = new Audio('/Winwheel/examples/wheel_of_fortune/lose.wav');

    function playSound() {
        playAudio.pause();
        playAudio.currentTime = 0;
        playAudio.play();
    }
    function winSound() {
        winAudio.pause();
        winAudio.currentTime = 0;
        winAudio.play();
    }
    function loseSound() {
        loseAudio.pause();
        loseAudio.currentTime = 0;
        loseAudio.play();
    }

    var wheelSpinning = false;
    var isReseting = false;
    var serverResponse;

    function startSpin(totalSpin) {

        if (wheelSpinning == false) {
            $.ajax({
                type: "POST",
                url: '/Admin/Game/LuckyRoundResult',
                dataType: "json",
                data: { totalSpin: totalSpin },
                beforeSend: function () {
                },
                success: function (response) {
                    if (response.Success) {

                        serverResponse = response.Data;

                        getTicket();

                        document.getElementById('spin_button').disabled = true;
                        document.getElementById('spin_10_button').disabled = true;
                        document.getElementById('spin_all_button').disabled = true;
                        document.getElementById('reset_button').disabled = true;

                        var numSegment = response.Data[0].Value;

                        var stopAt = theWheel.getRandomForSegment(numSegment);

                        theWheel.animation.stopAngle = stopAt;

                        theWheel.startAnimation();

                        wheelSpinning = true;
                        isReseting = true;
                    }
                    else {
                        be.error('Lucky Round', response.Message);
                    }
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                }
            });
        }
    }

    function resetWheel() {

        if (isReseting == false) {

            theWheel.stopAnimation(false);

            theWheel.rotationAngle = 0;

            theWheel.draw();

            wheelSpinning = false;

            document.getElementById('spin_button').disabled = false;
            document.getElementById('spin_10_button').disabled = false;
            document.getElementById('spin_all_button').disabled = false;
        }
    }

    function alertPrize(indicatedSegment) {

        loadLuckyRoundHistoryData(true);

        document.getElementById('reset_button').disabled = false;

        debugger;
        if (serverResponse.length == 1 && serverResponse[0].Value == 1) {
            loseSound();
            $(".times-lose").html(serverResponse[0].Times);
            $("#ic_modal_spin_lose_result").modal("show");
        }
        else {
            winSound();

            var template = $('#table-template-spin').html();
            var render = "";
            $.each(serverResponse, function (i, item) {
                render += Mustache.render(template, {
                    Message: item.Message,
                    Times: item.Times
                });
            });
            $('#tbl-content-spin').html(render);

            $("#ic_modal_spin_result").modal("show");
        }

        isReseting = false;
    }

    function loadLuckyRoundHistoryData(isPageChanged) {

        $.ajax({
            type: 'GET',
            data: {
                keyword: $('#txt-search-keyword').val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/admin/game/GetAllLuckyRoundHistoryPaging',
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
                        Sponsor: item.Sponsor,
                        AddressFrom: item.AddressFrom,
                        AddressTo: item.AddressTo,
                        Amount: item.Amount,
                        DateUpdated: be.dateTimeFormatJson(item.DateUpdated),
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                    });
                });

                $('#lbl-total-records').text(response.RowCount);

                $('#tbl-content').html(render);


                if (response.RowCount)
                    be.wrapPaging(response.RowCount, function () {
                        loadLuckyRoundHistoryData();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
            }
        });
    }
    function getTicket() {

        $.ajax({
            type: 'GET',
            url: '/admin/game/getTicket',
            dataType: 'json',
            beforeSend: function () {
            },
            success: function (response) {
                $(".TicketBalance").html(response);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
            }
        });
    }
}