﻿using Core.Application.Interfaces;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using Newtonsoft.Json;
using Nethereum.HdWallet;
using Microsoft.Extensions.Configuration;
using Core.Application.ViewModels.BlockChain;
using Nethereum.Web3.Accounts;
using Core.Utilities.Dtos;
using Nethereum.Util;
using Core.Application.ViewModels.System;
using Core.Data.IRepositories;
using System.Linq;
using Nethereum.Hex.HexTypes;
using Core.Infrastructure.Interfaces;
using Nethereum.Hex.HexConvertors.Extensions;
using System.Collections.Generic;
using System.Numerics;
using Core.Utilities.Constants;
using static Nethereum.Util.UnitConversion;
using Microsoft.AspNetCore.Identity;
using Core.Data.Entities;

namespace Core.Application.Implementation
{
    public class BlockChainService : IBlockChainService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AddressUtil _addressUtil = new AddressUtil();
        private readonly UserManager<AppUser> _userManager;

        public BlockChainService(
            IConfiguration configuration,
            UserManager<AppUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public Account CreateERC20AndBEP20Account()
        {
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
            var account = new Account(privateKey, CommonConstants.ChainId);
            return account;
        }

        public CoinMarKetCapInfoViewModel GetListingLatest(int startIndex,
            int limitItem, string unitConvertTo)
        {
            CoinMarKetCapInfoViewModel coinMarKetCapInfo = null;

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["start"] = startIndex.ToString(); // "1"
            queryString["limit"] = limitItem.ToString();// "2"
            queryString["convert"] = unitConvertTo;// "USD";

            string urlCryptoCurrencyListingLatest = _configuration["BlockChain:CoinmarKetCapApi:CMC_PRO_API_URL:CryptoCurrency:Listings_Latest"];
            var URL = new UriBuilder(urlCryptoCurrencyListingLatest);
            URL.Query = queryString.ToString();

            var client = new WebClient();
            var cmcProApiKey = _configuration["BlockChain:CoinmarKetCapApi:CMC_PRO_API_KEY"];
            client.Headers.Add("X-CMC_PRO_API_KEY", cmcProApiKey);
            client.Headers.Add("Accepts", "application/json");
            var coinInfoString = client.DownloadString(URL.ToString());

            coinMarKetCapInfo = JsonConvert.DeserializeObject<CoinMarKetCapInfoViewModel>(coinInfoString);

            return coinMarKetCapInfo;
        }

        public decimal GetCurrentPrice(string unit, string unitConverto)
        {
            var coinMarKetCapInfo = GetListingLatest(1, 20, unitConverto);
            if (coinMarKetCapInfo == null)
                return 0;

            var dataBNBBep20 = coinMarKetCapInfo.data.Find(x => x.symbol == unit);

            if (dataBNBBep20 == null)
                return 0;

            var priceBNBBep20 = Math.Round(dataBNBBep20.quote.USD.price, 2);

            return priceBNBBep20;
        }

        public async Task<Nethereum.RPC.Eth.DTOs.Transaction> GetTransactionByTransactionID(
            string transactionID, string urlRPC)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
               | SecurityProtocolType.Tls11
               | SecurityProtocolType.Tls
               | SecurityProtocolType.SystemDefault;

            var web3 = new Web3(urlRPC);

            Nethereum.RPC.Eth.DTOs.Transaction transactionInfo = await web3.Eth.Transactions
                .GetTransactionByHash.SendRequestAsync(transactionID).ConfigureAwait(true);

            return transactionInfo;
        }

        public async Task<TransactionReceipt> GetTransactionReceiptByTransactionID(
           string transactionID, string urlRPC)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
               | SecurityProtocolType.Tls11
               | SecurityProtocolType.Tls
               | SecurityProtocolType.SystemDefault;

            var web3 = new Web3(urlRPC);

            var transactionInfo = await web3.Eth.Transactions
                .GetTransactionReceipt.SendRequestAsync(transactionID).ConfigureAwait(true);

            return transactionInfo;
        }

        public async Task<TransactionReceipt> SendERC20Async(string privateKeyERC20, string receiverAddress,
            string contractAddress, decimal tokenAmount, int decimalPlaces, string urlRPC)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
               | SecurityProtocolType.Tls11
               | SecurityProtocolType.Tls
               | SecurityProtocolType.SystemDefault;

            var account = new Account(privateKeyERC20, CommonConstants.ChainId);

            var web3 = new Web3(account, urlRPC);

            web3.TransactionManager.UseLegacyAsDefault = true;

            var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunctionViewModel>();
            var transferFunction = new TransferFunctionViewModel()
            {
                To = receiverAddress,
                FromAddress = account.Address,
                TokenAmount = Web3.Convert.ToWei(tokenAmount, decimalPlaces),
            };

            var transactionReceipt = await transferHandler
                .SendRequestAndWaitForReceiptAsync(contractAddress, transferFunction).ConfigureAwait(true);

            return transactionReceipt;
        }

        public async Task<TransactionReceipt> SendEthAsync(string privateKey,
            string receiverAddress, decimal tokenAmount, string urlRPC)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls
                | SecurityProtocolType.SystemDefault;

            var account = new Account(privateKey, CommonConstants.ChainId);

            var web3 = new Web3(account, urlRPC);

            web3.TransactionManager.UseLegacyAsDefault = true;

            var transaction = await web3.Eth.GetEtherTransferService()
                    .TransferEtherAndWaitForReceiptAsync(receiverAddress, tokenAmount);

            return transaction;
        }

        public async Task<decimal> GetERC20Balance(string owner,
            string smartContractAddress, int decimalPlaces, string urlRPC)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls
                | SecurityProtocolType.SystemDefault;

            var web3 = new Web3(urlRPC);

            var balanceOfMessage = new BalanceOfFunction() { Owner = owner };

            var queryHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();

            var balance = await queryHandler
                .QueryAsync<BigInteger>(smartContractAddress, balanceOfMessage)
                .ConfigureAwait(true);

            decimal balanceUsdt = Web3.Convert.FromWei(balance, decimalPlaces);

            return balanceUsdt;
        }

        public async Task<decimal> GetEtherBalance(string publishKey, string urlRPC)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls
                | SecurityProtocolType.SystemDefault;

            var web3 = new Web3(urlRPC);

            var balance = await web3.Eth.GetBalance.SendRequestAsync(publishKey);

            decimal balanceEther = Web3.Convert.FromWei(balance);

            return balanceEther;
        }
    }
}
