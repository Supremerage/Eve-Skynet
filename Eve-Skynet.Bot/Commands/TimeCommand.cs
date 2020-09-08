using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Eve_Skynet.Bot.Interfaces;
using TimeZoneConverter;

namespace Eve_Skynet.Bot.Commands
{
    public class TimeCommand : ICommand
    {
        public string Name => "Time";
        public IEnumerable<string> Aliases => new List<string>();
        public string Help => "Converts EVE time. Type !time hhmm or !time hh:mm for a specific eve time.";
        public bool Public => true;

        public async Task Invoke(SocketUserMessage msg)
        {
            var content = msg.Content;

            var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var timezoneList = new List<TimeZoneInfo>()
            {
                TZConvert.GetTimeZoneInfo("Pacific Standard Time"),
                TZConvert.GetTimeZoneInfo("Mountain Standard Time"),
                TZConvert.GetTimeZoneInfo("Central Standard Time"),
                TZConvert.GetTimeZoneInfo("Eastern Standard Time"),
                TZConvert.GetTimeZoneInfo("GMT Standard Time"),
                TZConvert.GetTimeZoneInfo("Central Europe Standard Time"),
                TZConvert.GetTimeZoneInfo("Russian Standard Time"),
                TZConvert.GetTimeZoneInfo("China Standard Time"),
                TZConvert.GetTimeZoneInfo("AUS Eastern Standard Time"),
                TZConvert.GetTimeZoneInfo("New Zealand Standard Time"),
            };

            try
            {
                var utc = (words.Length > 1)
                    ? DatetimeFromShortString(words[1].Trim())
                    : DateTime.UtcNow;
                var builder = new EmbedBuilder();
                builder.WithTitle($"EVE Time: {utc:t}");

                foreach (var tz in timezoneList)
                {
                    var output = ConvertToTimezone(utc, tz);
                    builder.AddField(output.Item1, output.Item2);
                }

                builder.WithColor(Color.Blue);

                await msg.Channel.SendMessageAsync(null, false, builder.Build());
            }
            catch
            {
                await msg.Channel.SendMessageAsync("Are you running on Drakyll time?");
            }
        }

        private readonly Dictionary<TimeZoneInfo, string> _daylightNamesDictionary = new Dictionary<TimeZoneInfo, string>()
        {
            {TZConvert.GetTimeZoneInfo("Pacific Standard Time"), "PDT"},
            {TZConvert.GetTimeZoneInfo("Mountain Standard Time"), "MDT"},
            {TZConvert.GetTimeZoneInfo("Central Standard Time"), "CDT"},
            {TZConvert.GetTimeZoneInfo("Eastern Standard Time"), "EDT"},
            {TZConvert.GetTimeZoneInfo("GMT Standard Time"), "BST"},
            {TZConvert.GetTimeZoneInfo("Central Europe Standard Time"), "CEST"},
            {TZConvert.GetTimeZoneInfo("Russian Standard Time"), "MSK"},
            {TZConvert.GetTimeZoneInfo("China Standard Time"), "CST"},
            {TZConvert.GetTimeZoneInfo("AUS Eastern Standard Time"), "AEDT"},
            {TZConvert.GetTimeZoneInfo("New Zealand Standard Time"), "NZDT"},
        };

        private readonly Dictionary<TimeZoneInfo, string> _namesDictionary = new Dictionary<TimeZoneInfo, string>()
        {
            {TZConvert.GetTimeZoneInfo("Pacific Standard Time"), "PST"},
            {TZConvert.GetTimeZoneInfo("Mountain Standard Time"), "MST"},
            {TZConvert.GetTimeZoneInfo("Central Standard Time"), "CST"},
            {TZConvert.GetTimeZoneInfo("Eastern Standard Time"), "EST"},
            {TZConvert.GetTimeZoneInfo("GMT Standard Time"), "GMT"},
            {TZConvert.GetTimeZoneInfo("Central Europe Standard Time"), "CET"},
            {TZConvert.GetTimeZoneInfo("Russian Standard Time"), "MSK"},
            {TZConvert.GetTimeZoneInfo("China Standard Time"), "CST"},
            {TZConvert.GetTimeZoneInfo("AUS Eastern Standard Time"), "AEST"},
            {TZConvert.GetTimeZoneInfo("New Zealand Standard Time"), "NZST"},
        };

        private DateTime DatetimeFromShortString(string input)
        {
            var timeRegex = new Regex(@"^(?<h>\d{1,2}):?(?<m>\d{2})$");

            if (!timeRegex.IsMatch(input)) throw new Exception("Bad input");

            var match = timeRegex.Match(input);

            var h = int.Parse(match.Groups["h"].Value);
            var m = int.Parse(match.Groups["m"].Value);

            var now = DateTime.UtcNow;

            var date = new DateTime(now.Year, now.Month, now.Day, h, m, now.Second);
            return date;
        }

        private (string, string) ConvertToTimezone(DateTime utcTime, TimeZoneInfo timeZone)
        {

            var abbreviation = (timeZone.IsDaylightSavingTime(utcTime))
                ? _daylightNamesDictionary[timeZone]
                : _namesDictionary[timeZone];

            var name = (timeZone.IsDaylightSavingTime(utcTime))
                ? timeZone.DaylightName
                : timeZone.StandardName;

            var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZone);
            var timeString = localTime.ToString("t");

            var title = $"**{abbreviation}/{name}**";
            var value = $"**{timeString}**";
            return (title, value);
        }
    }
}
