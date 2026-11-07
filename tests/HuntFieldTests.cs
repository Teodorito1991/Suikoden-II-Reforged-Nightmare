using NightmareRebalance;
internal static class HuntFieldTests
{
 internal static void Run(){int n=0;void Check(bool x){n++;if(!x)throw new Exception("Hunt field "+n);}
  var w=new NativeFieldWindow();Check(!w.IsCurrent(0));Check(!w.IsCurrent(100));w.Record(100);Check(w.IsCurrent(100));Check(w.IsCurrent(101));Check(w.IsCurrent(102));Check(!w.IsCurrent(103));Check(!w.IsCurrent(99));w.Clear();Check(!w.IsCurrent(100));w.Record(-1);Check(!w.IsCurrent(0));w.Record(int.MaxValue);Check(w.IsCurrent(int.MaxValue));Check(!w.IsCurrent(0));
  Console.WriteLine($"PASS: Hunt field {n} assertions; absent/current/stale/destroyed field context, backward frames and overflow-safe window.");
 }
}
