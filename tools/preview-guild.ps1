param(
    [Parameter(Mandatory)][string]$CopiedGamePath,
    [int]$Width=1920,
    [int]$Height=1080
)
$ErrorActionPreference='Stop'
$root=(Resolve-Path -LiteralPath $CopiedGamePath).Path
if (!(Test-Path -LiteralPath (Join-Path $root 'PRIVATE_DO_NOT_UPLOAD.txt') -PathType Leaf)) {
    throw 'Use the marked disposable game copy with its startup-only save guard.'
}
$plugins=@(Get-ChildItem -LiteralPath (Join-Path $root 'BepInEx/plugins') -Filter '*.dll' -Recurse)
if ($plugins.Count -ne 2 -or !($plugins.Name -contains 'PrivateStartupGuard.dll') -or
    !($plugins.Name -contains 'ReforgedNightmare.dll')) {
    throw 'The preview copy must contain exactly one mod DLL and one startup-only save guard.'
}
if (Get-Process | Where-Object ProcessName -eq 'Suikoden I and II HD Remaster') {
    throw 'Close the running game before previewing.'
}
$runId=[Guid]::NewGuid().ToString('N')
$capture=Join-Path $root "guild-previews/$runId"
$started=[DateTime]::UtcNow
Add-Type -TypeDefinition @'
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
public static class CopiedGameDesktop {
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    private struct StartupInfo {
        public int size;
        public string reserved, desktop, title;
        public int x,y,xSize,ySize,xChars,yChars,fill,flags;
        public short showWindow,reserved2Size;
        public IntPtr reserved2,standardInput,standardOutput,standardError;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInfo {
        public IntPtr process,thread;
        public uint pid,tid;
    }
    [DllImport("user32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
    private static extern IntPtr CreateDesktop(string name, IntPtr device, IntPtr devMode,
        uint flags, uint access, IntPtr security);
    [DllImport("user32.dll")] private static extern bool CloseDesktop(IntPtr desktop);
    [DllImport("kernel32.dll")] private static extern bool CloseHandle(IntPtr handle);
    [DllImport("kernel32.dll")] private static extern uint WaitForSingleObject(IntPtr handle,uint ms);
    [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
    private static extern bool CreateProcess(string path,StringBuilder args,IntPtr procSecurity,
        IntPtr threadSecurity,bool inheritHandles,uint flags,IntPtr environment,string directory,
        ref StartupInfo startup,out ProcessInfo process);
    public static void Run(string path,string args,string directory) {
        string desktopName="ReforgedPreview-"+Guid.NewGuid().ToString("N");
        IntPtr desktop=CreateDesktop(desktopName,IntPtr.Zero,IntPtr.Zero,0,0x000f01ff,IntPtr.Zero);
        if(desktop==IntPtr.Zero) throw new Win32Exception();
        try {
            var startup=new StartupInfo {size=Marshal.SizeOf<StartupInfo>(),desktop=desktopName};
            if(!CreateProcess(path,new StringBuilder("\""+path+"\" "+args),IntPtr.Zero,
                IntPtr.Zero,false,0,IntPtr.Zero,directory,ref startup,out var process))
                throw new Win32Exception();
            try {
                Console.WriteLine("Copied game PID "+process.pid+" on separate desktop.");
                if(WaitForSingleObject(process.process,50000)!=0)
                    throw new TimeoutException("Copied game did not exit in the preview period.");
            } finally {CloseHandle(process.thread);CloseHandle(process.process);}
        } finally {CloseDesktop(desktop);}
    }
}
'@
$previousAppId=$env:SteamAppId
$previousGameId=$env:SteamGameId
try {
    $env:SteamAppId='1932640'
    $env:SteamGameId='1932640'
    $exe=Join-Path $root 'Suikoden I and II HD Remaster.exe'
    $argsLine="--reforged-private-startup-smoke --reforged-guild-render-smoke --reforged-guild-capture=$runId -screen-fullscreen 0 -screen-width $Width -screen-height $Height -logFile guild-preview-$runId.log"
    [CopiedGameDesktop]::Run($exe,$argsLine,$root)
} finally {
    $env:SteamAppId=$previousAppId
    $env:SteamGameId=$previousGameId
}
$guardResult=Get-Item -LiteralPath (Join-Path $root 'private-startup-result.json')
$guard=Get-Content -LiteralPath $guardResult.FullName -Raw | ConvertFrom-Json
$log=Join-Path $root 'BepInEx/LogOutput.log'
$hash=(Get-FileHash -LiteralPath ($plugins | Where-Object Name -eq 'ReforgedNightmare.dll').FullName).Hash
if ($guardResult.LastWriteTimeUtc -lt $started -or $guard.Failure -or
    $guard.Observed.ExactPackagedDllSha256 -ne $hash -or $guard.Observed.ActiveScene -ne 'Main') {
    throw 'The startup guard did not verify this copied-game run.'
}
if (!(Select-String -LiteralPath $log -SimpleMatch 'Guild render smoke PASS') -or
    !(Select-String -LiteralPath $log -SimpleMatch 'original asset loads=1, releases=1')) {
    throw 'The game did not complete its preview and asset cleanup. Inspect the copied-game log.'
}
$images=@(Get-ChildItem -LiteralPath $capture -Filter '*.png')
if ($images.Count -ne 18 -or ($images | Where-Object Length -le 8)) {
    throw 'Expected eighteen nonempty screenshots from this preview run.'
}
[pscustomobject]@{CaptureDirectory=$capture; DllSha256=$hash; Images=$images.Count}
