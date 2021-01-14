using Gamefreak130.Common;
using Sims3.Gameplay.Academics;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Objects.Miscellaneous;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;

#pragma warning disable IDE0032 // Use auto property
namespace Gamefreak130.JobOverhaulSpace
{
    public class PersistedSettings : Settings
    {
        // I wish these classes weren't nested
        // But moving them will break existing settings, so here they will stay
        [Persistable]
        public class InterviewSettings
        {
            private bool mRequiresInterview = true;

            private readonly List<TraitNames> mPositiveTraits = new() { TraitNames.Ambitious };

            private readonly List<TraitNames> mNegativeTraits = new() { TraitNames.Loser };

            private readonly List<SkillNames> mRequiredSkills = new();

            public bool RequiresInterview { get => mRequiresInterview; set => mRequiresInterview = value; }

            public List<TraitNames> PositiveTraits => mPositiveTraits;

            public List<TraitNames> NegativeTraits => mNegativeTraits;

            public List<SkillNames> RequiredSkills => mRequiredSkills;

            public InterviewSettings()
            {
            }

            public InterviewSettings(bool requiresInterview, List<TraitNames> posTraits, List<TraitNames> negTraits, List<SkillNames> skills)
            {
                mRequiresInterview = requiresInterview;
                mPositiveTraits = posTraits;
                mNegativeTraits = negTraits;
                mRequiredSkills = skills;
            }
        }

        [Persistable]
        public class CareerAvailabilitySettings
        {
            private readonly bool mIsActive;

            private bool mIsAvailable = true;

            private readonly List<AcademicDegreeNames> mRequiredDegrees = new();

            public bool IsActive => mIsActive;

            public bool IsAvailable { get => mIsAvailable; set => mIsAvailable = value; }

            public List<AcademicDegreeNames> RequiredDegrees => mRequiredDegrees;

            private CareerAvailabilitySettings()
            {
            }

            public CareerAvailabilitySettings(bool isActive) => mIsActive = isActive;

            public CareerAvailabilitySettings(bool isAvailable, bool isActive, List<AcademicDegreeNames> requiredDegrees)
            {
                mIsAvailable = isAvailable;
                mRequiredDegrees = requiredDegrees;
                mIsActive = isActive;
            }
        }

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

        [Tunable, TunableComment("The minimum guaranteed number of job offers available through a particular newspaper or the online classifieds; the maximum number is determined by the \"FindJobNumJobOpportunies\" element of computer and newspaper tuning")]
        private static readonly int kMinJobOffers = 0;

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

        private bool mEnableGetJobInRabbitHole = kEnableGetJobInRabbitHole;

        private bool mEnableJoinProfessionInRabbitHoleOrLot = kEnableJoinProfessionInRabbitHoleOrLot;

        private bool mInstantGratification = kInstantGratification;

        private bool mHoloComputerInstantGratification = kHoloComputerInstantGratification;

        private bool mHoloPhoneInstantGratification = kHoloPhoneInstantGratification;

        private bool mNewspaperSelfEmployed = kNewspaperSelfEmployed;

        private int mFullTimeInterviewHour = kFullTimeInterviewHour;

        private int mPartTimeInterviewHour = kPartTimeInterviewHour;

        private int mNumBonusResumeJobs = kNumBonusResumeJobs;

        private int mMinJobOffers = kMinJobOffers;

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

        private readonly Dictionary<string, InterviewSettings> mInterviewSettings = new();

        private readonly Dictionary<string, CareerAvailabilitySettings> mCareerAvailabilitySettings = new();

        private readonly Dictionary<string, bool> mSelfEmployedAvailabilitySettings = new();

        public bool EnableGetJobInRabbitHole { get => mEnableGetJobInRabbitHole; set => mEnableGetJobInRabbitHole = value; }

        public bool EnableJoinProfessionInRabbitHoleOrLot { get => mEnableJoinProfessionInRabbitHoleOrLot; set => mEnableJoinProfessionInRabbitHoleOrLot = value; }

        public bool InstantGratification { get => mInstantGratification; set => mInstantGratification = value; }

        public bool HoloComputerInstantGratification { get => mHoloComputerInstantGratification; set => mHoloComputerInstantGratification = value; }

        public bool HoloPhoneInstantGratification { get => mHoloPhoneInstantGratification; set => mHoloPhoneInstantGratification = value; }

        public bool NewspaperSelfEmployed { get => mNewspaperSelfEmployed; set => mNewspaperSelfEmployed = value; }

        public int FullTimeInterviewHour { get => mFullTimeInterviewHour; set => mFullTimeInterviewHour = Math.Min(value, 23); }

        public int PartTimeInterviewHour { get => mPartTimeInterviewHour; set => mPartTimeInterviewHour = Math.Min(value, 23); }

        public int NumBonusResumeJobs { get => mNumBonusResumeJobs; set => mNumBonusResumeJobs = value; }

        public int MinJobOffers { get => mMinJobOffers; set => mMinJobOffers = Common.Helpers.Min(value, ComputerCheap.kComputerTuning.FindJobNumJobOpportunies, ComputerExpensive.kComputerTuning.FindJobNumJobOpportunies, ComputerLaptop.kComputerTuning.FindJobNumJobOpportunies, 
                                                                                                  ComputerLaptopModern.kComputerTuning.FindJobNumJobOpportunies, ComputerLaptopVenue.kComputerTuning.FindJobNumJobOpportunies, HoloComputer.kComputerTuning.FindJobNumJobOpportunies, 
                                                                                                  Newspaper.kFindJobNumJobsOpportunitiesPerDay, Phone.UploadResume.FindJobNumJobOpportunies); }

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

        public Dictionary<string, InterviewSettings> InterviewMap => mInterviewSettings;

        public Dictionary<string, CareerAvailabilitySettings> CareerAvailabilityMap => mCareerAvailabilitySettings;

        public Dictionary<string, bool> SelfEmployedAvailabilitySettings => mSelfEmployedAvailabilitySettings;
    }
}
#pragma warning restore IDE0032 // Use auto property