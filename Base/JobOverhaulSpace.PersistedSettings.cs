using Gamefreak130.Common;
using Sims3.Gameplay.Academics;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Skills;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Gamefreak130.JobOverhaulSpace
{
    [Persistable]
    public class InterviewSettings
    {
        public bool RequiresInterview { get; set; }

        public List<TraitNames> PositiveTraits { get; }

        public List<TraitNames> NegativeTraits { get; }

        public List<SkillNames> RequiredSkills { get; }

        public InterviewSettings()
        {
        }

        public InterviewSettings(bool requiresInterview, List<TraitNames> posTraits, List<TraitNames> negTraits, List<SkillNames> skills)
        {
            RequiresInterview = requiresInterview;
            PositiveTraits = posTraits;
            NegativeTraits = negTraits;
            RequiredSkills = skills;
        }
    }

    [Persistable]
    public class CareerAvailabilitySettings
    {
        public bool IsAvailable { get; set; }

        public bool IsActive { get; set; }

        public List<AcademicDegreeNames> RequiredDegrees { get; set; }

        public CareerAvailabilitySettings()
        {
        }

        public CareerAvailabilitySettings(bool isAvailable, bool isActive, List<AcademicDegreeNames> requiredDegrees)
        {
            IsAvailable = isAvailable;
            RequiredDegrees = requiredDegrees;
            IsActive = isActive;
        }
    }

    [Persistable]
    public class PersistedSettings : IPersistedSettings
    {
        [Tunable, TunableComment("True/False: Whether or not to enable the EA default 'Join Career' rabbithole interactions for occupations that do not require an interview")]
        private static readonly bool kEnableGetJobInRabbitHole = true;

        [Tunable, TunableComment("True/False: Whether or not to enable the EA default 'Join Profession' interactions")]
        private static readonly bool kEnableJoinProfessionInRabbitHoleOrLot = false;

        [Tunable, TunableComment("True/False: Whether or not job offers from computers, newspapers, or smartphones will be presented in one menu; if set to true, kHoloComputerInstantGratification and kHoloPhoneInstantGratification are also assumed to be true")]
        private static readonly bool kInstantGratification = false;

        [Tunable, TunableComment("True/False: Whether or not to have the Holocomputer present all job offers in one menu like the crazy future tech it is, rather than through multiple dialogues over time")]
        private static readonly bool kHoloComputerInstantGratification = true;

        [Tunable, TunableComment("True/False: Whether or not to have the Holophone present all job offers in one menu like the crazy future tech it is, rather than through multiple dialogues over time")]
        private static readonly bool kHoloPhoneInstantGratification = true;

        [Tunable, TunableComment("True/False: Whether or not to allow the 'Register as Self-Employed' interaction on newspapers")]
        private static readonly bool kNewspaperSelfEmployed = false;

        [Tunable, TunableComment("How many bonus job offers to award sims with enough blog followers through the 'Upload Resume' interaction")]
        private static readonly int kNumBonusResumeJobs = 1;

        [Tunable, TunableComment("Range 0-24: The hour at which an interview for a full-time job will be scheduled")]
        private static readonly int kFullTimeInterviewHour = 10;

        [Tunable, TunableComment("Range 0-24: The hour at which an interview for a part-time job will be scheduled")]
        private static readonly int kPartTimeInterviewHour = 17;

        [Tunable, TunableComment("Maximum times a sim can postpone a job interview")]
        private static readonly int kMaxInterviewPostpones = 3;

        [Tunable, TunableComment("Range 0-100: Base chance of a sim getting a full-time job after a job interview")]
        private static readonly float kBaseFullTimeJobChance = 30;

        [Tunable, TunableComment("Range 0-100: Base chance of a sim getting a part-time job after a job interview")]
        private static readonly float kBasePartTimeJobChance = 65;

        [Tunable, TunableComment("Range 0-100: How much each postponement of a job interview decreases the chances of getting a job after that interview")]
        private static readonly float kPostponeInterviewChanceChange = 10;

        [Tunable, TunableComment("Range 0-100: How much the 'Ready For Interview' moodlet affects the chance of getting a job after a job interview")]
        private static readonly float kReadyForInterviewChanceChange = 15;

        [Tunable, TunableComment("Range 0-100: How much traits beneficial to a job affect the chance of getting that job after an interview")]
        private static readonly float kPositiveTraitInterviewChanceChange = 10;

        [Tunable, TunableComment("Range 0-100: How much traits detrimental to a job affect the chance of getting that job after an interview")]
        private static readonly float kNegativeTraitInterviewChanceChange = 15;

        [Tunable, TunableComment("Range 0-100: Change in chance of a sim getting a job after an interview per level in that job's required skills")]
        private static readonly float kRequiredSkillInterviewChanceChange = 3;

        [Tunable, TunableComment("Range 0-100: Chance that a sim gets offered a promotion upon leaving work when the performance bar is full")]
        private static readonly float kPromotionChance = 10;

        [Tunable, TunableComment("The amount of time, in sim minutes, that a sim spends inside a rabbithole for a job interview")]
        private static readonly float kInterviewTime = 60;

        [Tunable, TunableComment("The amount of time, in sim minutes, that a sim spends inside a rabbithole to fill out a job application or sign self-employment paperwork")]
        private static readonly float kApplicationTime = 30;

#pragma warning disable IDE0032 // Use auto property
        private bool mEnableGetJobInRabbitHole = kEnableGetJobInRabbitHole;

        private bool mEnableJoinProfessionInRabbitHoleOrLot = kEnableJoinProfessionInRabbitHoleOrLot;

        private bool mInstantGratification = kInstantGratification;

        private bool mHoloComputerInstantGratification = kHoloComputerInstantGratification;

        private bool mHoloPhoneInstantGratification = kHoloPhoneInstantGratification;

        private bool mNewspaperSelfEmployed = kNewspaperSelfEmployed;

        private int mFullTimeInterviewHour = kFullTimeInterviewHour;

        private int mPartTimeInterviewHour = kPartTimeInterviewHour;

        private int mNumBonusResumeJobs = kNumBonusResumeJobs;

        private int mMaxInterviewPostpones = kMaxInterviewPostpones;

        private float mBaseFullTimeJobChance = kBaseFullTimeJobChance;

        private float mBasePartTimeJobChance = kBasePartTimeJobChance;

        private float mPostponeInterviewChanceChange = kPostponeInterviewChanceChange;

        private float mReadyForInterviewChanceChange = kReadyForInterviewChanceChange;

        private float mPositiveTraitInterviewChanceChange = kPositiveTraitInterviewChanceChange;

        private float mNegativeTraitInterviewChanceChange = kNegativeTraitInterviewChanceChange;

        private float mRequiredSkillInterviewChanceChange = kRequiredSkillInterviewChanceChange;

        private float mPromotionChance = kPromotionChance;

        private float mInterviewTime = kInterviewTime;

        private float mApplicationTime = kApplicationTime;
#pragma warning restore IDE0032 // Use auto property

        public bool EnableGetJobInRabbitHole { get => mEnableGetJobInRabbitHole; set => mEnableGetJobInRabbitHole = value; }

        public bool EnableJoinProfessionInRabbitHoleOrLot { get => mEnableJoinProfessionInRabbitHoleOrLot; set => mEnableJoinProfessionInRabbitHoleOrLot = value; }

        public bool InstantGratification { get => mInstantGratification; set => mInstantGratification = value; }

        public bool HoloComputerInstantGratification { get => mHoloComputerInstantGratification; set => mHoloComputerInstantGratification = value; }

        public bool HoloPhoneInstantGratification { get => mHoloPhoneInstantGratification; set => mHoloPhoneInstantGratification = value; }

        public bool NewspaperSelfEmployed { get => mNewspaperSelfEmployed; set => mNewspaperSelfEmployed = value; }

        public int FullTimeInterviewHour { get => mFullTimeInterviewHour; set => mFullTimeInterviewHour = Math.Min(value, 23); }

        public int PartTimeInterviewHour { get => mPartTimeInterviewHour; set => mPartTimeInterviewHour = Math.Min(value, 23); }

        public int NumBonusResumeJobs { get => mNumBonusResumeJobs; set => mNumBonusResumeJobs = value; }

        public int MaxInterviewPostpones { get => mMaxInterviewPostpones; set => mMaxInterviewPostpones = value; }

        public float BaseFullTimeJobChance { get => mBaseFullTimeJobChance; set => mBaseFullTimeJobChance = Math.Min(value, 100); }

        public float BasePartTimeJobChance { get => mBasePartTimeJobChance; set => mBasePartTimeJobChance = Math.Min(value, 100); }

        public float PostponeInterviewChanceChange { get => mPostponeInterviewChanceChange; set => mPostponeInterviewChanceChange = Math.Min(value, 100); }

        public float ReadyForInterviewChanceChange { get => mReadyForInterviewChanceChange; set => mReadyForInterviewChanceChange = Math.Min(value, 100); }

        public float PositiveTraitInterviewChanceChange { get => mPositiveTraitInterviewChanceChange; set => mPositiveTraitInterviewChanceChange = Math.Min(value, 100); }

        public float NegativeTraitInterviewChanceChange { get => mNegativeTraitInterviewChanceChange; set => mNegativeTraitInterviewChanceChange = Math.Min(value, 100); }

        public float RequiredSkillInterviewChanceChange { get => mRequiredSkillInterviewChanceChange; set => mRequiredSkillInterviewChanceChange = Math.Min(value, 100); }

        public float PromotionChance { get => mPromotionChance; set => mPromotionChance = Math.Min(value, 100); }

        public float InterviewTime { get => mInterviewTime; set => mInterviewTime = value; }

        public float ApplicationTime { get => mApplicationTime; set => mApplicationTime = value; }

        public Dictionary<string, InterviewSettings> InterviewSettings { get; set; } = new Dictionary<string, InterviewSettings>();

        public Dictionary<string, CareerAvailabilitySettings> CareerAvailabilitySettings { get; set; } = new Dictionary<string, CareerAvailabilitySettings>();

        public Dictionary<string, bool> SelfEmployedAvailabilitySettings { get; set; } = new Dictionary<string, bool>();

        public PersistedSettings()
        {
        }

        public string Export()
        {
            StringBuilder text = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<JobOverhaulSettings>\n");
            foreach (var field in typeof(PersistedSettings).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.GetValue(this) is Dictionary<string, InterviewSettings> dict)
                {
                    text.AppendLine("  <mInterviewSettings>");
                    foreach (KeyValuePair<string, InterviewSettings> pair in dict)
                    {
                        InterviewSettings settings = pair.Value;
                        List<string> posTraits = new List<string>();
                        List<string> negTraits = new List<string>();
                        List<string> skills = new List<string>();
                        foreach (TraitNames trait in settings.PositiveTraits)
                        {
                            posTraits.Add(trait.ToString());
                        }
                        foreach (TraitNames trait in settings.NegativeTraits)
                        {
                            negTraits.Add(trait.ToString());
                        }
                        foreach (SkillNames skill in settings.RequiredSkills)
                        {
                            skills.Add(skill.ToString());
                        }
                        text.AppendFormat("    <{0}>\n", pair.Key);
                        text.AppendFormat("      <mRequiresInterview value=\"{0}\"/>\n", settings.RequiresInterview.ToString());
                        text.AppendFormat("      <mPositiveTraits value=\"{0}\"/>\n", string.Join(",", posTraits.ToArray()));
                        text.AppendFormat("      <mNegativeTraits value=\"{0}\"/>\n", string.Join(",", negTraits.ToArray()));
                        text.AppendFormat("      <mRequiredSkills value=\"{0}\"/>\n", string.Join(",", skills.ToArray()));
                        text.AppendFormat("    </{0}>\n", pair.Key);
                    }
                    text.AppendLine("  </mInterviewSettings>");
                }
                else if (field.GetValue(this) is Dictionary<string, CareerAvailabilitySettings> dict2)
                {
                    text.AppendLine("  <mCareerAvailabilitySettings>");
                    foreach (KeyValuePair<string, CareerAvailabilitySettings> pair in dict2)
                    {
                        CareerAvailabilitySettings settings = pair.Value;
                        List<string> list = new List<string>();
                        foreach (AcademicDegreeNames degree in settings.RequiredDegrees)
                        {
                            list.Add(degree.ToString());
                        }
                        text.AppendFormat("    <m{0}>\n", pair.Key);
                        text.AppendFormat("      <mIsAvailable value=\"{0}\"/>\n", settings.IsAvailable.ToString());
                        text.AppendFormat("      <mRequiredDegrees value=\"{0}\"/>\n", string.Join(",", list.ToArray()));
                        text.AppendFormat("    </m{0}>\n", pair.Key);
                    }
                    text.AppendLine("  </mCareerAvailabilitySettings>");
                }
                else if (field.GetValue(this) is Dictionary<string, bool> dict3)
                {
                    text.AppendLine("  <mSelfEmployedAvailabilitySettings>");
                    foreach (KeyValuePair<string, bool> pair in dict3)
                    {
                        text.AppendFormat("    <m{0} value=\"{1}\"/>\n", pair.Key, pair.Value.ToString());
                    }
                    text.AppendLine("  </mSelfEmployedAvailabilitySettings>");
                }
                else
                {
                    text.AppendFormat("  <{0} value=\"{1}\"/>\n", field.Name, field.GetValue(this).ToString());
                }
            }
            text.Append("</JobOverhaulSettings>");
            return text.ToString();
        }
    }
}