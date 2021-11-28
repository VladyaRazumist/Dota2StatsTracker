using System;
using System.Collections.Generic;
using System.Text;

namespace Dota2StatsTracker.Config
{
    public class Settings
    {
        public static Settings Current;

        public Settings()
        {
            Current = this;
        }

        public string Prefix { get; set; }
        public string DiscordToken { get; set; }
        public string VkApiToken { get; set; }
        public string CdnDotaUrl { get; set; }
        public string AudioContentPath { get; set; }
        public string ImagesContentPath { get; set; }
        public string JsonDataContentPath { get; set; }
        public string PositiveEmojis { get; set; }
    }
}
