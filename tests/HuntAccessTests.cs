using NightmareRebalance;
internal static class HuntAccessTests
{
 internal static void Run(){int n=0;void Check(bool x){n++;if(!x)throw new Exception("Hunt access "+n);}
  var r=new HuntRequest();Check(!r.Armed);Check(!r.Consume(true));Check(!r.Toggle(false));Check(r.Toggle(true));Check(r.Armed);Check(r.Consume(true));Check(!r.Armed);Check(!r.Consume(true));Check(r.Toggle(true));Check(!r.Toggle(true));Check(r.Toggle(true));Check(!r.Consume(false));Check(!r.Armed);Check(r.Toggle(true));r.Cancel();Check(!r.Armed);Check(!r.Toggle(false));
  Console.WriteLine($"PASS: Hunt access {n} assertions; explicit arm/cancel, failed guard consumes request, one-shot selection and disabled availability.");
 }
}
