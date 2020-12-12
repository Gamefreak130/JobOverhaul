using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.ActiveCareer;
using Sims3.Gameplay.ActiveCareer.ActiveCareers;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Seasons;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;

namespace Gamefreak130.JobOverhaulSpace.Helpers.OccupationStates
{
    [Persistable]
    public abstract class OccupationState
    {
        protected OccupationNames Guid { get; }

        protected ObjectGuid LocationId { get; }

        protected float Xp { get; set; }

        protected int MoneyEarned { get; set; }

        protected int Level { get; }

        private int HighestLevelAchieved { get; }

        private Anniversary Anniversary { get; }

        private int DaysOff { get; }

        private int UnpaidDaysOff { get; }

        private long DateHired { get; }

        private CASAgeGenderFlags AgeWhenJobFirstStarted { get; }

        private long WhenCurLevelStarted { get; }

        private string StageName { get; }

        protected OccupationState()
        {
        }

        public OccupationState(Occupation occupation)
        {
            Guid = occupation.Guid;
            LocationId = occupation.OfficeLocation?.ObjectId ?? default;
            Level = occupation.Level;
            HighestLevelAchieved = occupation.HighestLevelAchieved;
            Anniversary = occupation.WorkAnniversary;
            DaysOff = occupation.PaidDaysOff;
            UnpaidDaysOff = occupation.mUnpaidDaysOff;
            DateHired = occupation.DateHired.Ticks;
            AgeWhenJobFirstStarted = occupation.mAgeWhenJobFirstStarted;
            WhenCurLevelStarted = occupation.WhenCurLevelStarted.Ticks;
            StageName = occupation.StageName;
        }

        public virtual bool AcquireOccupation(CareerManager manager)
        {
            Occupation occupation = manager.Occupation;
            if (manager.mSimDescription.CreatedSim.IsSelectable && SeasonsManager.Enabled)
            {
                occupation.WorkAnniversary = Anniversary;
            }
            occupation.mHighestLevelAchievedVal = HighestLevelAchieved;
            occupation.mDaysOff = DaysOff;
            occupation.mUnpaidDaysOff = UnpaidDaysOff;
            occupation.mDateHired = new(DateHired);
            occupation.mAgeWhenJobFirstStarted = AgeWhenJobFirstStarted;
            occupation.mWhenCurLevelStarted = new(WhenCurLevelStarted);
            occupation.StageName = StageName;
            return true;
        }
    }

    // Persistable dictionaries drop keys with null values
    // So we use a NullState instead to represent no saved occupation
    public class NullState : OccupationState
    {
        public NullState()
        {
        }

        public override bool AcquireOccupation(CareerManager manager)
        {
            Methods.DropOccupation(manager);
            return true;
        }
    }

    public class CareerState : OccupationState
    {
        private string CurBranch { get; }

        private string HighestBranch { get; }

        private float ExtraPay { get; }

        private ulong BossId { get; }

        private ulong FormerBossId { get; }

        protected CareerState()
        {
        }

        public CareerState(Career career) : base(career)
        {
            Xp = career.Performance;
            MoneyEarned = career.mTotalCashEarned;
            ExtraPay = career.mPayPerHourExtra;
            CurBranch = career.CurLevelBranchName;
            HighestBranch = career.mHighestLevelAchievedBranchName;
            BossId = career.Boss?.SimDescriptionId ?? 0;
            FormerBossId = career.FormerBoss?.SimDescriptionId ?? 0;
        }

        public override bool AcquireOccupation(CareerManager manager)
        {
            Methods.DropOccupation(manager);
            if (GameObject.GetObject(LocationId) is RabbitHole rabbitHole && rabbitHole.CareerLocations.TryGetValue((ulong)Guid, out CareerLocation location))
            {
                Career career = CareerManager.GetStaticOccupation(Guid) as Career;
                GreyedOutTooltipCallback callback = null;
                if (career is not null && career.CareerAgeTest(manager.mSimDescription) && career.CanAcceptCareer(manager.mSimDescription?.CreatedSim?.ObjectId ?? default, ref callback))
                {
                    AcquireOccupationParameters parameters = new(Guid, location, false, false)
                    {
                        JumpStartJob = true
                    };
                    career.CareerLevels.TryGetValue(CurBranch, out Dictionary<int, CareerLevel> dictionary);
                    dictionary.TryGetValue(Level, out CareerLevel level);
                    parameters.JumpStartLevel = level;
                    if (manager.AcquireOccupation(parameters))
                    {
                        base.AcquireOccupation(manager);
                        career = manager.OccupationAsCareer;
                        career.mPerformance = Xp;
                        career.mTotalCashEarned = MoneyEarned;
                        career.mPayPerHourExtra = ExtraPay;
                        career.mHighestLevelAchievedBranchName = HighestBranch;
                        career.HighestCareerLevelAchieved = career.SharedData.CareerLevels[career.mHighestLevelAchievedBranchName][career.mHighestLevelAchievedVal];
                        SimDescription curBoss = SimDescription.Find(BossId);
                        SimDescription formerBoss = SimDescription.Find(FormerBossId);
                        if (curBoss is not null && career.Coworkers.Contains(curBoss))
                        {
                            career.SetBoss(curBoss);
                        }
                        if (formerBoss is not null && career.Coworkers.Contains(formerBoss))
                        {
                            career.FormerBoss = formerBoss;
                        }
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class ArtAppraiserState : CareerState
    {
        private int NumArtWorkScanned { get; }

        protected ArtAppraiserState()
        {
        }

        public ArtAppraiserState(ArtAppraiserCareer career) : base(career) => NumArtWorkScanned = career.NumArtWorkScanned;

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                ArtAppraiserCareer career = manager.Occupation as ArtAppraiserCareer;
                career.NumArtWorkScanned = NumArtWorkScanned;
                return true;
            }
            return false;
        }
    }

    public class BusinessState : CareerState
    {
        private int MeetingsHeldToday { get; }

        private int[] MeetingsHeldLastNDays { get; }

        protected BusinessState()
        {
        }

        public BusinessState(Business career) : base(career)
        {
            MeetingsHeldToday = career.MeetingsHeldToday;
            MeetingsHeldLastNDays = career.mMeetingsOnLastNDays;
        }

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                Business career = manager.Occupation as Business;
                career.MeetingsHeldToday = MeetingsHeldToday;
                career.mMeetingsOnLastNDays = MeetingsHeldLastNDays;
                return true;
            }
            return false;
        }
    }

    public class EducationState : CareerState
    {
        private int LecturesHeldToday { get; }

        private int[] LecturesHeldLastNDays { get; }

        protected EducationState()
        {
        }

        public EducationState(Education career) : base(career)
        {
            LecturesHeldToday = career.LecturesGivenToday;
            LecturesHeldLastNDays = career.mLecturesInLastNDays;
        }

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                Education career = manager.Occupation as Education;
                career.LecturesGivenToday = LecturesHeldToday;
                career.mLecturesInLastNDays = LecturesHeldLastNDays;
                return true;
            }
            return false;
        }
    }

    public class FilmState : CareerState
    {
        private int DaysInCostume { get; }

        private int DaysToWearCostume { get; }

        private List<string> AwardsToCollect { get; }

        protected FilmState()
        {
        }

        public FilmState(Film career) : base(career)
        {
            DaysInCostume = career.mDaysInCostume;
            DaysToWearCostume = career.mDaysToWearCostume;
            AwardsToCollect = career.mAwardsToCollect;
        }

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                Film career = manager.Occupation as Film;
                career.mDaysInCostume = DaysInCostume;
                career.mDaysToWearCostume = DaysToWearCostume;
                career.mAwardsToCollect = AwardsToCollect;
                return true;
            }
            return false;
        }
    }

    public class FortuneTellerState : CareerState
    {
        private int PrivateReadingsPerformed { get; }

        protected FortuneTellerState()
        {
        }

        public FortuneTellerState(FortuneTellerCareer career) : base(career) => PrivateReadingsPerformed = career.PrivateReadingsPerfomed;

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                FortuneTellerCareer career = manager.Occupation as FortuneTellerCareer;
                career.mPrivateReadingsPerformed = PrivateReadingsPerformed;
                return true;
            }
            return false;
        }
    }

    public class JournalismState : CareerState
    {
        private int StoriesWrittenToday { get; }

        private int[] StoriesWrittenLastNDays { get; }

        protected JournalismState()
        {
        }

        public JournalismState(Journalism career) : base(career)
        {
            StoriesWrittenToday = career.StoriesWrittenToday;
            StoriesWrittenLastNDays = career.mStoriesWrittenPastNDays;
        }

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                Journalism career = manager.Occupation as Journalism;
                career.StoriesWrittenToday = StoriesWrittenToday;
                career.mStoriesWrittenPastNDays = StoriesWrittenLastNDays;
                return true;
            }
            return false;
        }
    }

    public class LawEnforcementState : CareerState
    {
        private int ReportsWrittenToday { get; }

        private int[] ReportsWrittenLastNDays { get; }

        private ulong PartnerId { get; }

        protected LawEnforcementState()
        {
        }

        public LawEnforcementState(LawEnforcement career) : base(career)
        {
            ReportsWrittenToday = career.ReportsCompltetedToday;
            ReportsWrittenLastNDays = career.mReportsWrittenPastNDays;
            PartnerId = career.Partner.SimDescriptionId;
        }

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                LawEnforcement career = manager.Occupation as LawEnforcement;
                career.ReportsCompltetedToday = ReportsWrittenToday;
                career.mReportsWrittenPastNDays = ReportsWrittenLastNDays;
                if (SimDescription.Find(PartnerId) is SimDescription description)
                {
                    career.Partner = description;
                }
                return true;
            }
            return false;
        }
    }

    public class MedicalState : CareerState
    {
        private int NumMedicalJournalsRead { get; }

        protected MedicalState()
        {
        }

        public MedicalState(Medical career) : base(career) => NumMedicalJournalsRead = career.mNumMedicalJournalsRead;

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                Medical career = manager.Occupation as Medical;
                career.mNumMedicalJournalsRead = NumMedicalJournalsRead;
                return true;
            }
            return false;
        }
    }

    public class MusicState : CareerState
    {
        private int NumConcertsPerformed { get; }

        protected MusicState()
        {
        }

        public MusicState(Music career) : base(career) => NumConcertsPerformed = career.ConcertsPerformed;

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                Music career = manager.Occupation as Music;
                career.ConcertsPerformed = NumConcertsPerformed;
                return true;
            }
            return false;
        }
    }

    public class PoliticalState : CareerState
    {
        private int CampaignMoneyRaised { get; }

        protected PoliticalState()
        {
        }

        public PoliticalState(Political career) : base(career) => CampaignMoneyRaised = career.CampaignMoneyRaised;

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                Political career = manager.Occupation as Political;
                career.CampaignMoneyRaised = CampaignMoneyRaised;
                return true;
            }
            return false;
        }
    }

    public class ProSportsState : CareerState
    {
        private int WinRecord { get; }

        private int LossRecord { get; }

        private int TotalWins { get; }

        private int TotalLosses { get; }

        protected ProSportsState()
        {
        }

        public ProSportsState(ProSports career) : base(career)
        {
            WinRecord = career.mWinRecord;
            LossRecord = career.mLossRecord;
            TotalWins = career.mTotalWins;
            TotalLosses = career.mTotalLoss;
        }

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                ProSports career = manager.Occupation as ProSports;
                career.mWinRecord = WinRecord;
                career.mLossRecord = LossRecord;
                career.mTotalWins = TotalWins;
                career.mTotalLoss = TotalLosses;
                return true;
            }
            return false;
        }
    }

    public class SchoolState : CareerState
    {
        private int ConsecutiveDaysWithA { get; }

        protected SchoolState()
        {
        }

        public SchoolState(School school) : base(school) => ConsecutiveDaysWithA = school.ConsecutiveDaysWithA;

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                School school = manager.Occupation as School;
                school.mConsecutiveDaysWithA = ConsecutiveDaysWithA;
                return true;
            }
            return false;
        }
    }

    public class ScienceState : CareerState
    {
        private string LastPromotionRewardAsSeed { get; }

        protected ScienceState()
        {
        }

        public ScienceState(Science career) : base(career) => LastPromotionRewardAsSeed = career.mLastPromotionRewardAsSeed;

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                Science career = manager.Occupation as Science;
                career.mLastPromotionRewardAsSeed = LastPromotionRewardAsSeed;
                return true;
            }
            return false;
        }
    }

    public class SportsAgentState : CareerState
    {
        private int NumContractsNegotiated { get; }

        private int NumAnalyzeStatistics { get; }

        protected SportsAgentState()
        {
        }

        public SportsAgentState(SportsAgentCareer career) : base(career)
        {
            NumContractsNegotiated = career.NumberOfContractsNegotiated;
            NumAnalyzeStatistics = career.NumberOfAnalyzeStatistics;
        }

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                SportsAgentCareer career = manager.Occupation as SportsAgentCareer;
                career.mNumberOfContractsNegotiated = NumContractsNegotiated;
                career.mNumberOfAnalyzeStatistics = NumAnalyzeStatistics;
                return true;
            }
            return false;
        }
    }

    public abstract class XpBasedCareerState : OccupationState
    {
        private int OverMaxLevel { get; }

        protected XpBasedCareerState()
        {
        }

        public XpBasedCareerState(XpBasedCareer career) : base(career)
        {
            Xp = career.mXp;
            OverMaxLevel = career.OvermaxLevel;
            MoneyEarned = career.TotalCareerMoneyEarned();
        }

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (CareerManager.GetStaticOccupation(Guid) is XpBasedCareer)
            {
                AcquireOccupationParameters parameters = new(Guid, false, false);
                if (GameObject.GetObject(LocationId) is Lot lot)
                {
                    parameters.LotId = lot.LotId;
                }
                if (JobOverhaul.IsCareersInstalled)
                {
                    Career career = Activator.CreateInstance(Type.GetType("NRaas.Gameplay.Careers.OmniCareer, NRaasCareer")) as Career;
                    career.mCareerGuid = Guid;
                    parameters.Location = new() { Career = career };
                }
                if (manager.AcquireOccupation(parameters))
                {
                    base.AcquireOccupation(manager);
                    XpBasedCareer career = manager.Occupation as XpBasedCareer;
                    career.mLevel = Level;
                    career.mOvermaxLevel = OverMaxLevel;
                    career.mXp = Xp;
                    return true;
                }
            }
            return false;
        }
    }

    public class SkillBasedCareerState : XpBasedCareerState
    {
        protected SkillBasedCareerState()
        {
        }

        public SkillBasedCareerState(SkillBasedCareer career) : base(career)
        {
        }

        public override bool AcquireOccupation(CareerManager manager)
        {
            Methods.DropOccupation(manager);
            if (base.AcquireOccupation(manager))
            {
                manager.OccupationAsSkillBasedCareer.mMoneyEarned = MoneyEarned;
                return true;
            }
            return false;
        }
    }

    public class ActiveCareerState : XpBasedCareerState
    {
        protected ActiveCareerState()
        {
        }

        public ActiveCareerState(ActiveCareer career) : base(career) => MoneyEarned = career.mLifetimeEarningsForPensionCalculation;

        public override bool AcquireOccupation(CareerManager manager)
        {
            Methods.DropOccupation(manager);
            ActiveCareer activeCareer = CareerManager.GetStaticOccupation(Guid) as ActiveCareer;
            if (activeCareer is not null && activeCareer.IsActiveCareerAvailable() && (activeCareer.GetOccupationStaticDataForActiveCareer().ValidAges & manager.mSimDescription.Age) is not CASAgeGenderFlags.None && base.AcquireOccupation(manager))
            {
                manager.OccupationAsActiveCareer.mLifetimeEarningsForPensionCalculation = MoneyEarned;
                return true;
            }
            return false;
        }
    }

    public class FireFighterState : ActiveCareerState
    {
        private bool CompletedAJob { get; }

        private bool BeenToFireStation { get; }

        private float UpgradeProgressRemaining { get; }

        protected FireFighterState()
        {
        }

        public FireFighterState(ActiveFireFighter career) : base(career)
        {
            CompletedAJob = career.CompletedAJob;
            BeenToFireStation = career.mBeenToFireStation;
            UpgradeProgressRemaining = career.UpgradeTracker?.ProgressPercentageRemaining ?? -1f;
        }

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                ActiveFireFighter career = manager.Occupation as ActiveFireFighter;
                career.CompletedAJob = CompletedAJob;
                career.mBeenToFireStation = BeenToFireStation;
                if (UpgradeProgressRemaining != -1)
                {
                    career.UpgradeTracker = new() { mProgressPercentageRemaining = UpgradeProgressRemaining };
                }
                return true;
            }
            return false;
        }
    }

    public class DaycareState : ActiveCareerState
    {
        private bool HasHadAfterschoolChildBefore { get; }

        private long LicenseRevokedDate { get; }

        private bool Suspended { get; }

        private int NumSuspensions { get; }

        protected DaycareState()
        {
        }

        public DaycareState(Daycare career) : base(career)
        {
            HasHadAfterschoolChildBefore = career.mHasHadAfterschoolChildBefore;
            LicenseRevokedDate = career.mLicenseRevokedDateAndTime.Ticks;
            Suspended = career.mSuspended;
            NumSuspensions = career.mSuspensions;
        }

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                Daycare career = manager.Occupation as Daycare;
                career.mHasHadAfterschoolChildBefore = HasHadAfterschoolChildBefore;
                career.mLicenseRevokedDateAndTime = new(LicenseRevokedDate);
                career.mSuspended = Suspended;
                career.mSuspensions = NumSuspensions;
                return true;
            }
            return false;
        }
    }

    public class LifeguardState : ActiveCareerState
    {
        private int NumRescues { get; }

        protected LifeguardState()
        {
        }

        public LifeguardState(Lifeguard career) : base(career) => NumRescues = career.mNumRescues;

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                Lifeguard career = manager.Occupation as Lifeguard;
                career.mNumRescues = NumRescues;
                return true;
            }
            return false;
        }
    }

    public class PrivateEyeState : ActiveCareerState
    {
        private int NumCasesCompleted { get; }

        private int NumStakeoutsDone { get; }

        private int ReportsToWrite { get; }

        protected PrivateEyeState()
        {
        }

        public PrivateEyeState(PrivateEye career) : base(career)
        {
            NumCasesCompleted = career.mCasesCompleted;
            NumStakeoutsDone = career.mStakeoutsDone;
            ReportsToWrite = career.ReportsToWrite;
        }

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                PrivateEye career = manager.Occupation as PrivateEye;
                career.mCasesCompleted = NumCasesCompleted;
                career.mStakeoutsDone = NumStakeoutsDone;
                career.ReportsToWrite = ReportsToWrite;
                return true;
            }
            return false;
        }
    }

    public class PerformanceCareerState : ActiveCareerState
    {
        private List<SteadyGig> SteadyGigs { get; }

        private PerformanceCareer.PerformanceTrickSkill[] TrickSkills { get; }

        protected PerformanceCareerState()
        {
        }

        public PerformanceCareerState(PerformanceCareer career) : base(career)
        {
            SteadyGigs = career.mSteadyGigs;
            TrickSkills = career.mTrickSkills;
        }

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                PerformanceCareer career = manager.OccupationAsPerformanceCareer;
                career.mSteadyGigs = SteadyGigs;
                career.mTrickSkills = TrickSkills;
                return true;
            }
            return false;
        }
    }

    public class SingerState : PerformanceCareerState
    {
        private bool HasShownSingagramIntro { get; }

        protected SingerState()
        {
        }

        public SingerState(SingerCareer career) : base(career) => HasShownSingagramIntro = career.mHasShownSingagramIntroTNS;

        public override bool AcquireOccupation(CareerManager manager)
        {
            if (base.AcquireOccupation(manager))
            {
                SingerCareer career = manager.Occupation as SingerCareer;
                career.mHasShownSingagramIntroTNS = HasShownSingagramIntro;
                return true;
            }
            return false;
        }
    }
}
