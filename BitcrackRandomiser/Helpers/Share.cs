﻿using BitcrackRandomiser.Enums;
using BitcrackRandomiser.Models;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace BitcrackRandomiser.Helpers
{
    internal class Share
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="settings"></param>
        /// <param name="data"></param>
        public static void Send(ResultType type, Settings settings, string? data = "")
        {
            /// Telegram Share
            if (settings.TelegramShare)
                SendTelegram(type, settings, data);

            /// API Share
            if (settings.IsApiShare)
                SendApiShare(type, settings, data);
        }

        /// <summary>
        /// Telegram share
        /// </summary>
        /// <param name="type"></param>
        /// <param name="settings"></param>
        /// <param name="data"></param>
        static async void SendTelegram(ResultType type, Settings settings, string? data = "")
        {
            string message = "";
            switch (type)
            {
                case ResultType.workerStarted:
                    message = string.Format("[{0}].[{2}] started job for (Puzzle{1})", Helper.StringParser(settings.ParsedWalletAddress), settings.TargetPuzzle, settings.ParsedWorkerName);
                    break;
                case ResultType.reachedOfKeySpace:
                    message = string.Format("[{0}].[{1}] reached of keyspace", Helper.StringParser(settings.ParsedWalletAddress), settings.ParsedWorkerName);
                    break;
                case ResultType.workerExited:
                    message = string.Format("[{0}].[{1}] goes offline.", Helper.StringParser(settings.ParsedWalletAddress), settings.ParsedWorkerName);
                    break;
                case ResultType.keyFound:
                    message = string.Format("[Key Found] Congratulations. Found by worker [{0}].[{2}] {1}", Helper.StringParser(settings.ParsedWalletAddress), data, settings.ParsedWorkerName);
                    break;
                case ResultType.rewardFound:
                    message = string.Format("[Reward Found] A reward found by worker [{0}].[{2}] {1}", Helper.StringParser(settings.ParsedWalletAddress), data, settings.ParsedWorkerName);
                    break;
                case ResultType.rangeScanned:
                    if (!settings.TelegramShareEachKey) message = "";
                    else message = string.Format("[{0}] scanned by [{1}].[{2}]", data, Helper.StringParser(settings.ParsedWalletAddress), settings.ParsedWorkerName);
                    break;
                default:
                    break;
            }

            if (message.Length > 0)
            {
                bool isSent = false;
                int sendTries = 0, maxTries = 1;
                if (type == ResultType.keyFound) maxTries = int.MaxValue;
                while (!isSent && sendTries < maxTries)
                {
                    try
                    {
                        var botClient = new TelegramBotClient(settings.TelegramAccessToken);
                        Message result = await botClient.SendTextMessageAsync(
                        chatId: settings.TelegramChatId,
                        text: message);
                        isSent = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"Telegram share failed. ChatId:{settings.TelegramChatId}, Token:{settings.TelegramAccessToken}");
                    }
                    if (type == ResultType.keyFound) Thread.Sleep(10000);
                    sendTries++;
                }
            }
        }

        /// <summary>
        /// Api share
        /// </summary>
        /// <param name="type"></param>
        /// <param name="settings"></param>
        /// <param name="data"></param>
        static void SendApiShare(ResultType type, Settings settings, string? data = "")
        {
            bool isSent = false;
            int sendTries = 0, maxTries = 1;
            if (type == ResultType.keyFound) maxTries = int.MaxValue;
            while (!isSent && sendTries < maxTries)
            {
                switch (type)
                {
                    case ResultType.keyFound:
                        isSent = Requests.SendApiShare(new ApiShare { Status = type, PrivateKey = data, HEX = data }, settings).Result;
                        Thread.Sleep(10000);
                        break;
                    case ResultType.rewardFound:
                        isSent = Requests.SendApiShare(new ApiShare { Status = type, PrivateKey = data, HEX = data }, settings).Result;
                        break;
                    case ResultType.rangeScanned:
                        isSent = Requests.SendApiShare(new ApiShare { Status = type, HEX = data }, settings).Result;
                        break;
                    default:
                        isSent = Requests.SendApiShare(new ApiShare { Status = type }, settings).Result;
                        break;
                }
                sendTries++;
            }
        }
    }
}
