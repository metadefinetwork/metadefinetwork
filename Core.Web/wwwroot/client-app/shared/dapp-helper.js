var DAppHelper = {

    Web3Instant: function fn() {
        return new Web3(Web3.givenProvider)
    }(),

    //========Mainnet=========
    ChainId: 56,
    Chain: '0x38',
    ChainName: 'BNB',
    CurrencyName: 'BNB',
    RpcUrls: 'https://quiet-weathered-voice.bsc.quiknode.pro/d3e2ded180c61cba24bcc1f22baad17a0ba707bd/',//https://bsc-dataseed1.binance.org:443 Mainnet //https://data-seed-prebsc-1-s1.binance.org:8545 Testnet // https://bsc-dataseed.binance.org/ new Mainnet

    TokenSymbol: 'MAR',
    TokenAddress: '0xC7993a3E3b83DE0F61b39647284B10c7d91ce5e4',

    //========Testnet=========
    //ChainId: 97,
    //Chain: '0x61',
    //ChainName: 'BNB',
    //CurrencyName: 'BNB',
    //RpcUrls: 'https://data-seed-prebsc-1-s1.binance.org:8545',//https://bsc-dataseed1.binance.org:443 Mainnet //https://data-seed-prebsc-1-s1.binance.org:8545 Testnet // https://bsc-dataseed.binance.org/ new Mainnet

    //TokenSymbol: 'MAR',
    //TokenAddress: '0xf97a5d94a925f49bb27685732d744e89c6a67344',

    ReferralLink: window.location.origin + '/claim?referralLink=',

    initialize: function () {
        if (!window.Web3Instant) {
            window.Web3Instant = new Web3(Web3.givenProvider);
        }

        if (!(ethereum && ethereum.isMetaMask)) {
            //show popup with link to metamask extension install
            window.location.replace('https://metamask.io/');
        }

        if (!(ethereum && ethereum.isTrust)) {
            //window.location.replace('https://metamask.io/');
        }
    },

    CheckNetWork: async function fn() {
        try {
            let currentChain = await DAppHelper.Web3Instant.eth.getChainId();

            if (currentChain === DAppHelper.ChainId) {
                return true;
            }

            be.error('DAPP Notification', 'Wrong network, please chain your network to BSC. Current Chain: ' + currentChain)
            return false
        } catch (e) {
            be.error('DAPP Notification', e.message)
            return false
        }
    },

    AnyConnectedAccounts: async function fn() {
        try {
            var accounts = await DAppHelper.Web3Instant.eth.getAccounts();

            return accounts && accounts.length > 0;
        } catch (e) {
            be.error('DAPP Notification', e.message);

            return false;
        }
    },

    SwitchChain: async function fn() {
        if (ethereum.isTrust)
            return true

        try {
            await ethereum.request({
                method: 'wallet_switchEthereumChain',
                params: [{ chainId: DAppHelper.Chain }],
            });

            return true;
        } catch (switchError) {
            // This error code indicates that the chain has not been added to DAPP.
            if (switchError.code === 4902) {
                try {
                    await ethereum.request({
                        method: 'wallet_addEthereumChain',
                        params: [{
                            chainId: DAppHelper.Chain,
                            chainName: DAppHelper.ChainName,
                            rpcUrls: [DAppHelper.RpcUrls],
                            nativeCurrency: {
                                name: DAppHelper.CurrencyName,
                                symbol: DAppHelper.CurrencyName,
                                decimals: 18
                            },
                        }],
                    });
                } catch (addError) {
                    // handle "add" error
                    be.error('DAPP Notification', addError.message)
                    return false
                }
            }

            be.error('DAPP Notification', switchError.message)
            return false
        }
    },

    RegisterEthereumEvents: function fn(accountChangeHandler) {

        ethereum.on('accountsChanged', accountChangeHandler);
    },

    AddICDAsset: async function fn() {
        const tokenDecimals = 18;
        const tokenImage = 'https://localhost:44396/client-side/metadefi/main-html/images/favicon.png';

        try {
            // wasAdded is a boolean. Like any RPC method, an error may be thrown.
            const wasAdded = await ethereum.request({
                method: 'wallet_watchAsset',
                params: {
                    type: 'ERC20', // Initially only supports ERC20, but eventually more!
                    options: {
                        address: DAppHelper.TokenAddress, // The address that the token is at.
                        symbol: DAppHelper.TokenSymbol, // A ticker symbol or shorthand, up to 5 chars.
                        decimals: tokenDecimals, // The number of decimals in the token
                        image: "", // A string url of the token logo
                    },
                },
            });

            //if (wasAdded) {
            //    be.success('DAPP Notification', 'Added asset successful!')
            //} else {
            //    be.error('DAPP Notification', 'Failed to add asset')
            //}
        } catch (error) {
            /*be.error('DAPP Notification', error.message)*/
        }
    },

    HasInstalledMetaMask: function fn() {
        try {
            return window.ethereum && window.ethereum.isMetaMask
        } catch (e) {
            return false;
        }
    },

    HasInstalledTrust: function fn() {
        try {
            return window.ethereum && window.ethereum.isTrust
        } catch (e) {
            return false;
        }
    },

    CurrentAddress: '',

    DApp: function () {
        this.currentAccount = {}

        this.init = async function fn() {

            registerEvents()

            if (!window.ethereum) {
                ShowConnectButton()
                //$('#connect-wallet-modal').modal('show');
                $('.dapp_add-asset').hide()
                return
            }

            if (window.ethereum.isTrust)
                $('.dapp_add-asset').hide()

            DAppHelper.RegisterEthereumEvents(handleAccountsChanged)

            var hasAccounts = await DAppHelper.AnyConnectedAccounts()
            if (hasAccounts) {
                await DAppHelper.CheckNetWork();
                await DAppHelper.SwitchChain();
                await handleRequestAccounts()
                ShowSendButton()
            } else {
                ShowConnectButton()
                HandleDisconnected()
            }
        }

        function registerEvents() {

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

            $('.dapp_add-asset').on('click', () => {
            });

            $('.dapp_amount').on('focusout', async function () {
                be.startLoading()

                let value = $('.dapp_amount').val();

                var response = await DAppHelper.GetAsync('/DAPP/GetAmountICDPerBNB/' + value)
                    .catch(error => {
                        be.error('Invalid amount BNB')
                        be.stopLoading()
                    })

                if (response) {

                    $('.dapp_token-amount').val(be.formatCurrency(response.Data))
                }

                be.stopLoading()
            })

            $('#wallet-connect-metamask').on('click', async function (e) {
                e.preventDefault()
                await ConnectMetaMask();

                $('#connect-wallet-modal').modal('hide');
            });

            $('#wallet-connect-trust').on('click', async function (e) {
                e.preventDefault()
                await ConnectTrustWallet();

                $('#connect-wallet-modal').modal('hide');
            });
            var els = document.getElementsByClassName("txtaddressCurrent");
            Array.prototype.forEach.call(els, function (el) {
                el.addEventListener("click", function () {
                    copyToClipboard(el);
                });
            });
        }

        function copyToClipboard(elem) {
            // create hidden text element, if it doesn't already exist
            var targetId = "_hiddenCopyText_";
            var isInput = elem.tagName === "INPUT" || elem.tagName === "TEXTAREA";
            var origSelectionStart, origSelectionEnd;
            if (isInput) {
                // can just use the original source element for the selection and copy
                target = elem;
                origSelectionStart = elem.selectionStart;
                origSelectionEnd = elem.selectionEnd;
            } else {
                // must use a temporary form element for the selection and copy
                target = document.getElementById(targetId);
                if (!target) {
                    var target = document.createElement("textarea");
                    target.style.position = "absolute";
                    target.style.left = "-9999px";
                    target.style.top = "0";
                    target.id = targetId;
                    document.body.appendChild(target);
                }
                target.textContent = elem.textContent;
            }
            // select the content
            var currentFocus = document.activeElement;
            target.focus();
            target.setSelectionRange(0, target.value.length);

            // copy the selection
            var succeed;
            try {
                succeed = document.execCommand("copy");
            } catch (e) {
                succeed = false;
            }
            // restore original focus
            if (currentFocus && typeof currentFocus.focus === "function") {
                currentFocus.focus();
            }

            if (isInput) {
                // restore prior selection
                elem.setSelectionRange(origSelectionStart, origSelectionEnd);
            } else {
                // clear temporary content
                target.textContent = "";
            }

            be.notify('Copy to clipboard is successful', 'success');

            return succeed;
        }

        async function confirmProcessingTransaction(action) {

            //switch net
            var isSwitchSucess = await DAppHelper.SwitchChain()

            if (!isSwitchSucess) {
                return;
            }

            //process buy icd
            be.startLoading();

            InitializePresaleProgress(action).then(async res => {

                if (!res.Success) {
                    be.stopLoading()

                    if (!res.Message) {
                        be.error('DAPP Notification', 'Can not process transaction.')
                        return
                    }

                    be.error(res.Message)
                    return
                }

                var amountConverted = Web3.utils.toWei(res.Data.Value.toString());
                var hexAmount = Web3.utils.numberToHex(amountConverted, 'ether');

                let data = {
                    from: res.Data.From,
                    to: res.Data.To,
                    value: hexAmount,
                    gasPrice: res.Data.GasPrice,
                    gas: res.Data.Gas,
                    data: res.Data.TransactionHex
                }

                let transactionHash = await sendTransaction(data)
                    .then(txh => txh)
                    .catch(error => {
                        if (window.ethereum.isMetaMask) {
                            UpdateErrorMetaMask(data.data, error.code);

                            if (error.code === 4001)
                                be.error('DAPP Notification', 'Transaction was Rejected')
                            else {
                                be.error('DAPP Notification', 'Something went wrong! Please contact administrator for support. Code: ' + error.code)
                            }
                        }

                        if (window.ethereum.isTrust) {
                            UpdateErrorMetaMask(data.data, 4001);
                            be.error('DAPP Notification', 'Transaction was Rejected')
                        }

                        be.stopLoading();
                    })

                if (!transactionHash) {
                    return;
                }

                be.stopLoading();
                be.startLoading('<b>We are processing your transaction.</b><b> Kindly wait for a moment ultil the process completed...</b>');

                VerifyMetaMaskRequest(transactionHash).then(res => {
                    be.stopLoading();

                    if (!res.Success) {

                        be.error('DAPP Notification', res.Message)
                        return
                    }
                    be.success('DAPP Notification', res.Message, function () {
                        window.location.reload();
                    })

                    $('.dapp_amount').val(0)
                    $('.dapp_icd-amount').val(0)

                });
            })
                .catch(error => {
                    be.error('DAPP Notification', 'Something went wrong! please, contact administrator.')
                    be.stopLoading();
                })
        }

        function ShowConnectButton() {
            $('.dapp_button').off('click')

            $('.dapp_button').on('click', async function () {
                $('#connect-wallet-modal').modal('show');
            });
        }

        function ShowSendButton() {
            $('.dapp_button').off('click')

            $('.dapp_button').on('click', async function () {

                var isValidNet = await DAppHelper.CheckNetWork();
                let action = $(this).attr('data-action')

                if (isValidNet) {
                    await confirmProcessingTransaction(action)
                }
            });
        }

        async function ConnectTrustWallet() {
            if (DAppHelper.HasInstalledTrust()) {
                await handleRequestAccounts();
            } else {
                if (be.isDevice()) {
                    window.open('https://link.trustwallet.com/open_url?coin_id=60&url=https://metadefi.network', '_blank').focus();
                    return;
                }
            }
        }

        async function ConnectMetaMask() {
            if (!DAppHelper.HasInstalledMetaMask()) {
                if (be.isDevice()) {
                    window.location.replace('https://metamask.app.link/dapp/metadefi.network');
                    return;
                }
                be.confirm('DAPP notification', 'You have not installed DAPP, please install DAPP', function () {
                    window.location.replace('https://metamask.io/');
                });

                return;
            }
            await handleRequestAccounts();
        }


        //request to metamask if not connected, will show a prompt from metamask
        async function handleRequestAccounts() {
            try {
                let accounts = await DAppHelper.Web3Instant.eth.requestAccounts()

                if (accounts && accounts.length > 0) {

                    $('.dapp_address').val(accounts[0])
                    $('.dapp_address').val(accounts[0])
                    $('.txtaddressCurrent').val(accounts[0])
                    $('.dapp_referral-link').val(DAppHelper.ReferralLink + accounts[0])
                    HandleConnected()
                    return accounts
                }
            } catch (e) {
                ShowConnectButton()
                HandleDisconnected()
            }
        }

        async function sendTransaction(data) {
            return await ethereum.request({
                method: 'eth_sendTransaction',
                params: [
                    data,
                ]
            });
        }

        async function handleAccountsChanged(accounts) {
            if (accounts.length === 0) {
                // DAPP is locked or the user has not connected any accounts
                ShowConnectButton()
                HandleDisconnected()

            } else if (accounts[0]) {
                // Do any other work!
                $('.dapp_address').val(accounts[0])
                $('.dapp_referral-link').val(DAppHelper.ReferralLink + accounts[0])
                ShowSendButton()
                HandleConnected()
                //await DAppHelper.DAppConnect(accounts[0]);
            }
        }

        function HandleConnected() {
            $('.dapp_form').show();
            $('.dapp_transactions').show();
            $('.dapp_no-transactions').hide()
            $('.dapp_button_confirm').hide()

            $('.dapp_button .dapp_button_connected').show()
            $('.dapp_button .dapp_button_disconnected').hide()

            $('.dapp_form').removeClass('d-none')
            $('.dapp_transactions').removeClass('d-none')
            $('.dapp_button_connected').removeClass('d-none')
        }

        function HandleDisconnected() {
            $('.dapp_form').hide()
            //$('.dapp_transactions').hide()
            $('.dapp_no-transactions').show()

            $('.dapp_address').val("");

            $('.dapp_button .dapp_button_connected').hide()
            $('.dapp_button .dapp_button_disconnected').show()
        }

        function InitializePresaleProgress(action) {

            let data = {
                Address: $('.dapp_address').val(),
                Email: $('.dapp_email').val()
            }

            data.IsDevice = DAppHelper.isDevice

            if (ethereum.isMetaMask) {
                data.WalletType = "Metamask"
            }

            if (ethereum.isTrust) {
                data.WalletType = "Trust"
            }

            if (action === 'presale') {
                data.BNBAmount = $('.dapp_amount').val()
            } else {
                data.BNBAmount = 0
            }

            return DAppHelper.PostAsync(`/DAPP/InitializeTransactionProgress/${action}`, data);
        }

        function VerifyMetaMaskRequest(transactionHash) {
            return DAppHelper.PostAsync('/DAPP/VerifyTransactionRequest', transactionHash)
        }

        function UpdateErrorMetaMask(transactionHex, errorCode) {
            let data = {
                TransactionHex: transactionHex,
                ErrorCode: errorCode.toString()
            }
            return DAppHelper.PostLegacyAsync('/DAPP/UpdateErrorMetaMask', data);
        }
    },
    PostLegacyAsync: async function fn(url = '', data = {}) {
        $.ajax({
            type: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            data: JSON.stringify(data),
            url: url,
            dataType: 'JSON',
            beforeSend: function () {

            },
            success: function (response) {

                if (response.status === 401) {
                    be.error('DAPP Notification', 'Please, Disconnect and connect your wallet again!')
                }
                return response.json()


            },
            error: function (message) {
                be.error(`${message.responseText}`, 'error');

            },
        });
    },
    PostAsync: async function fn(url = '', data = {}) {

        var accounts = await DAppHelper.Web3Instant.eth.getAccounts();
        var address = accounts[0];

        return await fetch(url, {
            method: 'POST',
            dataType: 'json',
            mode: 'cors',
            headers: {
                'Content-Type': 'application/json',
                'ConnectedAddress': address,
                //"XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            body: JSON.stringify(data),
        })
            .then(response => {
                if (response.status === 401) {
                    be.error('DAPP Notification', 'Please, Disconnect and connect your wallet again!')
                }
                return response.json()
            })
    },

    GetAsync: async function fn(url = '') {
        let accounts = await DAppHelper.Web3Instant.eth.getAccounts();

        let address = accounts[0];

        return await fetch(url, {
            method: 'GET',
            dataType: 'json',
            headers: {
                'Content-Type': 'application/json',
                'ConnectedAddress': address,
                //"XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
        }).then(response => {
            if (response.status === 401) {
                be.error('DAPP Notification', 'Please, Disconnect and connect your wallet again!')
            }
            return response.json()
        })
    },

    //DAppConnect: async function DAppConnect(address) {
    //    let referralHidden = $('.dapp_referral-address-hidden').val();
    //    let result = await DAppHelper.GetAsync(`/DApp/DAppConnect?address=${address}&referral=${referralHidden}`)
    //    if (result.Data.Referral && result.Data.Referral.length > 0) {
    //        $('.dapp_referral-address').val(result.Data.Referral)
    //    } else {
    //        if (referralHidden && referralHidden.length > 0) {
    //            $('.dapp_referral-address').val(referralHidden)
    //        }
    //    }
    //}
}
