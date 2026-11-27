namespace NightmareRebalance;
// A context lease is a prerequisite, never permission to launch or a completion receipt.
internal sealed record WarFieldLease(long Epoch,long Game,long System,long Party,long Data,long Map,long Event,long Asset,long Chapter,int Area,int Town,int Variant,int X,int Y,string Flags);
internal static class WarBoundaryRules
{
 internal static string Compare(WarFieldLease? before,WarFieldLease? now)
 {
  if(before==null||now==null)return "Missing verified field context";
  if(before.Epoch!=now.Epoch)return "Load/session epoch changed";
  if(before.Game!=now.Game||before.System!=now.System||before.Party!=now.Party||before.Data!=now.Data)return "Campaign owner changed";
  if(before.Map!=now.Map||before.Event!=now.Event||before.Asset!=now.Asset)return "Original field/resource owner changed";
  if(before.Area!=now.Area||before.Town!=now.Town||before.Variant!=now.Variant||before.X!=now.X||before.Y!=now.Y)return "Field location changed";
  if(before.Flags!=now.Flags)return "Campaign flags changed";
  return "Prerequisite context matches; resource cleanup/control restoration unproven";
 }
 internal static bool CanLaunch { get; set; }
}
