namespace NightmareRebalance;
// Read the game's actual action maps. Does not inject input or change bindings/focus.
internal static class NativeInputDiagnostics
{
 internal static object Snapshot()
 {
  try{
   if(!GRInputManager.IsInstantiated)return new{Available=false,Reason="Native input manager not instantiated"};
   var manager=GRInputManager.Instance;var rows=new List<object>();
   foreach(var type in new[]{GRInputManager.Type.Decide,GRInputManager.Type.Cancel,GRInputManager.Type.Up,GRInputManager.Type.Down,GRInputManager.Type.Left,GRInputManager.Type.Right,GRInputManager.Type.Menu}){
    if(manager.keys==null||!manager.keys.TryGetValue(type,out var key)||key==null)continue;
    var actions=new List<object>();if(key.action!=null)for(int i=0;i<Math.Min(key.action.Count,32);i++){var action=key.action[i];if(action==null)continue;var bindings=new List<object>();var list=action.bindings;for(int j=0;j<Math.Min(list.Count,32);j++){var b=list[j];bindings.Add(new{b.path,b.effectivePath,b.groups,b.isComposite,b.isPartOfComposite});}actions.Add(new{action.name,action.enabled,Bindings=bindings.ToArray()});}
    rows.Add(new{Type=type.ToString(),key.on,key.press,key.repeat,key.isKeyBoardOn,Actions=actions.ToArray()});
   }
   return new{Available=true,Mode=manager.mode.ToString(),GRInputManager.Pause,LastDevice=GRInputManager.LastInputDeviceType.ToString(),UnityFocused=UnityEngine.Application.isFocused,Keys=rows.ToArray(),ReadOnly=true,InputInjected=false,BindingsChanged=false};
  }catch(Exception e){return new{Available=false,Reason=e.GetType().Name+": "+e.Message};}
 }
}
