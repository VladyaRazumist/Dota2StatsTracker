using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord;
using Dota2StatsTracker.Config;
using Dota2StatsTracker.Helpers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Dota2StatsTracker.Services.HelperServices
{
    public class DotaConstantService
    {
        private readonly string _contentPath;
        private readonly string _cdnDotaUrl;

        public List<Hero> Heroes { get; set; }
        public List<Item> Items { get; set; }
        public Dictionary<string, string> RankImagesUrls { get; set; }

        public DotaConstantService(IOptions<Settings> settings)
        {
            _contentPath = settings.Value.JsonDataContentPath;
            _cdnDotaUrl = settings.Value.CdnDotaUrl;

            Load();
        }

        public string GetRankImageLocalUrl(int rankTier)
        {
            var rank = rankTier / 10;
            var stars = rankTier % 10;

            var imageName = $"SeasonalRank{rank}-{stars}.png";

            return RankImagesUrls[imageName];
        }

        public Hero GetHeroByName(string name)
        {
            var exactMatch = Heroes.FirstOrDefault(h => h.LocalizedName == name);

            if (exactMatch != null)
                return exactMatch;

            var matchesByAbbreviation = FindHeroesByNameAbbreviation(name).ToArray();

            if (matchesByAbbreviation.Length == 1)
                return matchesByAbbreviation[0];

            var possibleMatches = FindPossibleHeroMatches(name).ToArray();

            if (possibleMatches.Length > 1 || !possibleMatches.Any())
                return null;

            return possibleMatches[0];
        }

        public IReadOnlyCollection<Hero> FindPossibleHeroesByName(string name)
        {
            var possibleMatches = FuzzyMatcher.ExtractTop(Heroes.Select(h => h.LocalizedName).ToArray(), name, poorMatching: true).ToArray();

            return Heroes.Where(h => possibleMatches.Contains(h.LocalizedName)).ToArray();
        }

        private IReadOnlyCollection<Hero> FindHeroesByNameAbbreviation(string name)
        {
            var matchesByAbbreviation = FuzzyMatcher.FindMatchesByAbbreviation(Heroes.Select(h => h.LocalizedName).ToArray(), name).ToArray();

            return Heroes.Where(h => matchesByAbbreviation.Contains(h.LocalizedName)).ToArray();
        }

        private IReadOnlyCollection<Hero> FindPossibleHeroMatches(string name)
        {
            var possibleMatches = FuzzyMatcher.ExtractTop(Heroes.Select(h => h.LocalizedName).ToArray(), name).ToArray();;

            return Heroes.Where(h => possibleMatches.Contains(h.LocalizedName)).ToArray();
        }

        private void Load()
        {
            Heroes = JsonConvert.DeserializeObject<Dictionary<string, Hero>>(File.ReadAllText(_contentPath + "heroes.json")).Select(h => h.Value).ToList();
            Items = JsonConvert.DeserializeObject<Dictionary<string, Item>>(File.ReadAllText(_contentPath + "items.json")).Select(i => i.Value).ToList();
            RankImagesUrls = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(_contentPath + "rankImages.json"));

            foreach (var hero in Heroes)
            {
                hero.ImgUrl = _cdnDotaUrl + hero.ImgUrl;
                hero.IconUrl = _cdnDotaUrl + hero.IconUrl;
                hero.PrimaryAttributeColor = GetHeroColorByMainAttribute(hero.PrimaryAttr);
            }
        }

        private static Color GetHeroColorByMainAttribute(string attribute)
        {
            var color = attribute switch
            {
                "agi" => Color.Green,
                "str" => Color.Red,
                "int" => Color.Blue,
                _ => throw new ArgumentException("invalid attribute name", attribute)
            };

            return color;
        }
    }

    public class Hero
    {
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("localized_name")]
        public string LocalizedName { get; set; }

        [JsonProperty("primary_attr")]
        public string PrimaryAttr { get; set; }

        [JsonProperty("attack_type")]
        public string AttackType { get; set; }

        public IReadOnlyCollection<string> Roles { get; set; }

        [JsonProperty("img")]
        public string ImgUrl { get; set; }

        [JsonProperty("icon")]
        public string IconUrl { get; set; }

        [JsonProperty("move_speed")]
        public int MoveSpeed { get; set; }

        public Color PrimaryAttributeColor { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }

        public List<string> Hint { get; set; }

        [JsonProperty("img")]
        public string ImgUrl { get; set; }

        [JsonProperty("dname")]
        public string Name { get; set; }

        public string Qual { get; set; }

        public int? Cost { get; set; }

        public string Notes { get; set; }

        public string Lore { get; set; }
    }
}
