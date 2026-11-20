using System.Runtime.CompilerServices;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace NightmareRebalance;

// Unloaded, explicitly requested native gate fixture. No battle or save is run.
internal static class HallUnlockProbe
{
    static bool ran;
    internal static void Run()
    {
        if(ran||!Application.isBatchMode||!Environment.GetCommandLineArgs().Contains("--reforged-private-hall-audit"))return;
        ran=true;
        var previous=Framework.Singleton<GAME_WORK>.instance;
        var o=Plugin.Options;
        var policy=o.HallUnlockPolicy.Value;
        bool hall=o.EnableHallOfEchoes.Value;
        var trialOptions=new[]{o.EnableHallPestRat,o.EnableHallHarpy,o.EnableHallGoldenWolf,
            o.EnableHallStoneGolem,o.EnableHallAbomination}.Concat(o.AdditionalHallTrials.Values).ToArray();
        var trialValues=trialOptions.Select(x=>x.Value).ToArray();
        static void Check(bool ok){if(!ok)throw new InvalidOperationException("Native Hall availability/reset guard failed.");}
        try
        {
            RuntimeHelpers.RunClassConstructor(typeof(GAME_WORK).TypeHandle);
            var scratch=new GAME_WORK(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<GAME_WORK>.NativeClassPtr));
            scratch.event_flag=new Il2CppStructArray<byte>(256);
            Framework.Singleton<GAME_WORK>.instance=scratch;
            o.HallUnlockPolicy.Value=HallUnlockPolicy.StoryProgression;
            o.EnableHallOfEchoes.Value=true;
            foreach(var option in trialOptions)option.Value=true;
            var witnessed=HallTrialCatalogue.Entries.Select((d,i)=>(d,i))
                .Where(x=>HallUnlockRules.HasWitness(x.d.Id)).Select(x=>x.i).ToArray();
            bool All(bool expected)=>witnessed.All(i=>HallAvailability.CanSelect(i)==expected);
            Check(All(false));
            // Old v1.44 map / nonordinary battle witnesses must not unlock trials.
            scratch.event_flag[0x15]=2;scratch.event_flag[0xDD]=2;scratch.event_flag[0xDF]=0x40;
            Check(All(false));
            scratch.event_flag[0xB6]=2;scratch.event_flag[0x21]=0x10;scratch.event_flag[0x16]=2;
            scratch.event_flag[0xC9]=1;scratch.event_flag[0x56]=0x20;
            scratch.event_flag[0x3D]=0x10;
            scratch.event_flag[0x41]=0x40;scratch.event_flag[0x56]|=0x02;
            scratch.event_flag[7]=64;scratch.event_flag[0xBA]=8;scratch.event_flag[0xBB]=16;scratch.event_flag[0x14]=2;
            Check(!HallAvailability.Unlocked(7)); // Worm's encounter-start flag is not victory.
            Check(All(true));
            o.EnableHallGoldenWolf.Value=false;Check(!HallAvailability.CanSelect(2));o.EnableHallGoldenWolf.Value=true;
            o.EnableHallOfEchoes.Value=false;Check(All(false));o.EnableHallOfEchoes.Value=true;
            scratch.event_flag=new Il2CppStructArray<byte>(256);
            Check(All(false)); // Current state is reread; no previous selection can authorize handoff.
            Check(!HallAvailability.CanSelect(-1)&&!HallAvailability.CanSelect(HallTrialCatalogue.Entries.Length));
            ProductionAssetAudit.Rows.Add(new {Kind="HallNativeUnlockGuard",EarlyLocked=true,
                CorrectWitnessesUnlocked=true,OldWitnessesRejected=true,ResetRevokedEligibility=true,
                IndependentConfiguration=true,NativeFlagReads=true,OwnedScratchState=true,
                OriginalStoryPlayed=false,LoadingCancellationExecuted=false});
        }
        finally
        {
            Framework.Singleton<GAME_WORK>.instance=previous;
            o.HallUnlockPolicy.Value=policy;o.EnableHallOfEchoes.Value=hall;
            for(int i=0;i<trialOptions.Length;i++)trialOptions[i].Value=trialValues[i];
        }
    }
}
