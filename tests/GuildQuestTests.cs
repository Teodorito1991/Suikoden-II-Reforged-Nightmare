using NightmareRebalance;

internal static class GuildQuestTests
{
    internal static void Run()
    {
        int n=0;
        void Check(bool value,string why){n++;if(!value)throw new Exception("Guild quest: "+why);}
        HuntRunProgress Run(string id,string receipt,bool win){var r=new HuntRunProgress{HuntId=id,RunId=receipt};if(win)r.Victory();r.Finish(win?"VictoryRewardsCompleted":"ClosedWithoutVictorySignal",0);return r;}
        var q=new GuildQuest();
        Check(q.State==GuildQuestState.NoContract&&!q.Report(),"No reward/report without contract");
        Check(!q.Accept(" "),"Reject absent contract");
        Check(q.Accept("sun")&&!q.Accept("other"),"No silent active-contract replacement");
        Check(!q.Started("other","foreign"),"Foreign encounter cannot own completion");
        Check(q.Started("sun","escape"),"Owning encounter can start");
        Check(!q.Finished(Run("other","escape",true)),"Wrong hunt cannot complete current receipt");
        Check(!q.Finished(Run("sun","escape",false))&&q.State==GuildQuestState.Accepted,"Escape is retryable");
        Check(!q.Started("sun","escape"),"Closed receipt cannot be reused");
        Check(q.Started("sun","victory"),"Retry starts a new receipt");
        var r=Run("sun","victory",true);
        Check(q.Finished(r)&&q.State==GuildQuestState.AwaitingReport&&q.ContractId=="sun","Victory retains reportable contract");
        Check(!q.Finished(r)&&q.Completions==1,"Repeated callback cannot duplicate completion");
        Check(!q.Withdraw()&&!q.Accept("sun"),"Completed hunt must be reported before a new contract");
        Check(!q.Report(),"Battle closure is not field return");
        var wrong=Run("sun","foreign",true);wrong.FieldResume();
        Check(!q.FieldReturned(wrong),"Foreign field receipt cannot unlock report");
        r.FieldResume();Check(q.FieldReturned(r)&&!q.FieldReturned(r),"Owning field resume observed once");
        Check(q.Report()&&!q.Report()&&q.Reports==1,"Report exactly once");
        Check(q.State==GuildQuestState.Reported&&q.ContractId==null&&q.LastReportedId=="sun","Reported state is available again");
        Check(q.Accept("sun")&&!q.Started("sun","victory"),"Same hunt repeat with fresh receipt required");
        Check(q.Withdraw()&&q.State==GuildQuestState.NoContract,"Withdrawal removes accepted contract");
        for(int i=0;i<100;i++){
            Check(q.Accept("sun"),"Repeat farming has no daily cap");
            string receipt="repeat-"+i;Check(q.Started("sun",receipt),"Repeat receipt");
            var win=Run("sun",receipt,true);Check(q.Finished(win),"Repeat genuine completion receipt");
            win.FieldResume();Check(q.FieldReturned(win)&&q.Report(),"Repeat report and return");
        }
        Check(q.Completions==101&&q.Reports==101,"Bounded diagnostic receipt retention imposes no progression cap");
        var loaded=new GuildQuest();
        Check(loaded.State==GuildQuestState.NoContract&&!loaded.Finished(r),"New session cannot inherit another session's receipt");
        Console.WriteLine($"PASS: Guild quest {n} assertions; report lifecycle, escape, duplicate/foreign receipts, session isolation and repeat farming. Synthetic receipts are not gameplay.");
    }
}
