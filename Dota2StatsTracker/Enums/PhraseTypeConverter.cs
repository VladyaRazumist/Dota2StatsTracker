using System;
using System.Collections.Generic;
using System.Text;
using Dota2StatsTracker.Config;

namespace Dota2StatsTracker.Enums
{
    public enum PhraseType
    {
        Spawn,
        Die,
        DoNotWannaLeave,
        LookWhoIsBack,
        WhatDidIMiss,
        Win,
        Lose,
        Joke,
        DidYouCallButcher,
        MyNameIsButcher,
        GetDownToBusiness,
        UncoverKnives,
        BreathDeeplyGuys,
        Thanks,
        FreshMeat,
        WhoComesForDinner,
        LetsGetCloser,
        PrettyMincedMeat,
        Pain1,
    }

    public static class PhraseTypeConverter
    {
        public static PhraseType[] GreetingPhraseTypes =
        {
            PhraseType.LookWhoIsBack,
            PhraseType.WhoComesForDinner,
            PhraseType.LetsGetCloser,
            PhraseType.PrettyMincedMeat,
        };

        public static string AudioPathFromPhraseType(PhraseType type)
        {
            var filePath = type switch
            {
                PhraseType.Spawn => "pudge_spawn.mp3",
                PhraseType.Die => "pudge_farsh_does_not_go_back.mp3",
                PhraseType.DoNotWannaLeave => "pudge_dont_wanna_leave.mp3",
                PhraseType.LookWhoIsBack => "pudge_look_who_is_back.mp3",
                PhraseType.WhatDidIMiss => "pudge_what_did_i_miss.mp3",
                PhraseType.Win => "pudge_win.mp3",
                PhraseType.Lose => "pudge_lose.mp3",
                PhraseType.Joke => "pudge_sad_joke.mp3",
                PhraseType.DidYouCallButcher => "pudge_butcher_was_called.mp3",
                PhraseType.MyNameIsButcher => "pudge_my_name_is_butcher.mp3",
                PhraseType.GetDownToBusiness => "pudge_get_to_business.mp3",
                PhraseType.UncoverKnives => "pudge_uncover_knives.mp3",
                PhraseType.BreathDeeplyGuys => "pudge_breath_deeply.mp3",
                PhraseType.Pain1 => "pudge_pain_1.mp3",
                PhraseType.Thanks => "pudge_thanks.mp3",
                PhraseType.FreshMeat => "pudge_thanks.mp3",
                PhraseType.WhoComesForDinner => "pudge_who_comes_for_dinner.mp3",
                PhraseType.LetsGetCloser => "pudge_lets_get_closer.mp3",
                PhraseType.PrettyMincedMeat => "pudge_pretty_minced_meat.mp3",
                _ => throw new ArgumentException("invalid enum value", nameof(type))
            };

            return Settings.Current.AudioContentPath + filePath;
        }

        public static string TextByPhraseType(PhraseType type)
        {
            var phrase = type switch
            {
                PhraseType.Spawn => "Мясник!",
                PhraseType.Die => "Фарш не провернуть назад, и мясо из котлет не восстановишь",
                PhraseType.DoNotWannaLeave => "Солнце уходит на запад, а я остаюсь!",
                PhraseType.LookWhoIsBack => "Вы посмотрите, кто вернулся!",
                PhraseType.WhatDidIMiss => "Что я пропустил?",
                PhraseType.Win => "Победа!",
                PhraseType.Lose => "Поражение...",
                PhraseType.Joke => "столько костей... А МАМА ГДЕ!? \n мамы нет...( \n ЕЛКИ ПАЛКИ",
                PhraseType.DidYouCallButcher => "Мясника вызывали?",
                PhraseType.MyNameIsButcher => "Меня зовут Мясником.",
                PhraseType.GetDownToBusiness => "Давай к делу!",
                PhraseType.UncoverKnives => "Р-р-расчехляю ножи!",
                PhraseType.BreathDeeplyGuys => "Дышите глубже, ребята!",
                PhraseType.Pain1 => "Э-э-а-у.",
                PhraseType.Thanks => "Спасибо, дружище. Ты тоже аппетитно выглядишь.",
                PhraseType.FreshMeat => "Хы-хы-хы-хы, свежее мясо!",
                PhraseType.WhoComesForDinner => "Это кто зашел на ужин?",
                PhraseType.LetsGetCloser => "Познакомимся поближе.",
                PhraseType.PrettyMincedMeat => "Какой симпатичный фарш!",

                _ => throw new ArgumentException("invalid enum value", nameof(type))
            };

            return phrase;
        }

        public static PhraseType GetRandomGreetingPhraseType()
        {
            var random = new Random();

            return GreetingPhraseTypes[random.Next(GreetingPhraseTypes.Length)];
        }

        public static PhraseType GetRandomPhraseType()
        {
            var enumValues = Enum.GetValues(typeof(PhraseType));
            var random = new Random();

            return (PhraseType)enumValues.GetValue(random.Next(enumValues.Length));
        }
    }
}
