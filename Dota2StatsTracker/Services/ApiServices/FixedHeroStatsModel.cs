﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Dota2StatsTracker.Services.ApiServices
{
    public class FixedHeroStatsModel
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("localized_name")]
        public string LocalizedName { get; set; }

        [JsonPropertyName("img")]
        public string Image { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("base_health")]
        public int BaseHealth { get; set; }

        [JsonPropertyName("base_health_regen")]
        public double? BaseHealthRegen { get; set; }

        [JsonPropertyName("base_mana")]
        public int BaseMana { get; set; }

        [JsonPropertyName("base_mana_regen")]
        public double BaseManaRegen { get; set; }

        [JsonPropertyName("base_armor")]
        public double BaseArmor { get; set; }

        [JsonPropertyName("base_mr")]
        public int BaseMr { get; set; }

        [JsonPropertyName("base_attack_min")]
        public int BaseAttackMin { get; set; }

        [JsonPropertyName("base_attack_max")]
        public int BaseAttackMax { get; set; }

        [JsonPropertyName("base_str")]
        public int BaseStrength { get; set; }

        [JsonPropertyName("base_agi")]
        public int BaseAgility { get; set; }

        [JsonPropertyName("base_int")]
        public int BaseIntelligence { get; set; }

        [JsonPropertyName("str_gain")]
        public double StrengthGain { get; set; }

        [JsonPropertyName("agi_gain")]
        public double AgilityGain { get; set; }

        [JsonPropertyName("int_gain")]
        public double IntelligenceGain { get; set; }

        [JsonPropertyName("attack_range")]
        public int AttackRange { get; set; }

        [JsonPropertyName("projectile_speed")]
        public int ProjectileSpeed { get; set; }

        [JsonPropertyName("attack_rate")]
        public double AttackRate { get; set; }

        [JsonPropertyName("move_speed")]
        public int MoveSpeed { get; set; }

        [JsonPropertyName("turn_rate")]
        public double? TurnRate { get; set; }

        [JsonPropertyName("cm_enabled")]
        public bool CmEnabled { get; set; }

        [JsonPropertyName("legs")]
        public int Legs { get; set; }

        [JsonPropertyName("pro_ban")]
        public int ProBan { get; set; }

        [JsonPropertyName("hero_id")]
        public int HeroId { get; set; }

        [JsonPropertyName("pro_win")]
        public int ProWin { get; set; }

        [JsonPropertyName("pro_pick")]
        public int ProPick { get; set; }

        [JsonPropertyName("1_pick")]
        public int HeraldPicks { get; set; }

        [JsonPropertyName("1_win")]
        public int HeraldWins { get; set; }

        [JsonPropertyName("2_pick")]
        public int GuardianPicks { get; set; }

        [JsonPropertyName("2_win")]
        public int GuardianWins { get; set; }

        [JsonPropertyName("3_pick")]
        public int CrusaderPicks { get; set; }

        [JsonPropertyName("3_win")]
        public int CrusaderWins { get; set; }

        [JsonPropertyName("4_pick")]
        public int ArchonPicks { get; set; }

        [JsonPropertyName("4_win")]
        public int ArchonWins { get; set; }

        [JsonPropertyName("5_pick")]
        public int LegendPicks { get; set; }

        [JsonPropertyName("5_win")]
        public int LegendWins { get; set; }

        [JsonPropertyName("6_pick")]
        public int AncientPicks { get; set; }

        [JsonPropertyName("6_win")]
        public int AncientWins { get; set; }

        [JsonPropertyName("7_pick")]
        public int DivinePicks { get; set; }

        [JsonPropertyName("7_win")]
        public int DivineWins { get; set; }

        [JsonPropertyName("8_pick")]
        public int ImmortalPicks { get; set; }

        [JsonPropertyName("8_win")]
        public int ImmortalWins { get; set; }

        [JsonPropertyName("turbo_pick")]
        public long TurboPicks { get; set; }

        [JsonPropertyName("turbo_win")]
        public long TurboWins { get; set; }
    }
}
