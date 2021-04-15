using CSCore.CoreAudioAPI;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Timer = System.Timers.Timer;

namespace AdRework {
    static class Program {
        [STAThread]
        static void Main() { AdReworkStart(); }
        public enum SpotifyAdStatus { None, Ad, Unknown }
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
        private static AudioSessionManager2 GetDefaultAudioSessionManager2(DataFlow dataFlow) {
            using (var enumerator = new MMDeviceEnumerator()) {
                using (var device = enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia)) {
                    var sessionManager = AudioSessionManager2.FromMMDevice(device);
                    return sessionManager; }}}

        public static void SetApplicationVolume(int ProcessId, float Volume) {
            using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render)) {
                using (var sessionEnumerator = sessionManager.GetSessionEnumerator()) {
                    foreach (var session in sessionEnumerator) {
                        using (var simpleVolume = session.QueryInterface<SimpleAudioVolume>())
                            using (var sessionControl = session.QueryInterface<AudioSessionControl2>()) {
                                if (sessionControl.ProcessID == ProcessId)
                                    simpleVolume.MasterVolume = Volume; }}}}}
        public static float GetApplicationVolume(int ProcessId) {
            using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render)) {
                using (var sessionEnumerator = sessionManager.GetSessionEnumerator()) {
                    foreach (var session in sessionEnumerator) {
                        using (var simpleVolume = session.QueryInterface<SimpleAudioVolume>())
                            using (var sessionControl = session.QueryInterface<AudioSessionControl2>()) {
                                if (sessionControl.ProcessID == ProcessId)
                                    return simpleVolume.MasterVolume; }}}} return 0; }

        private static bool AdManaged = false;
        private static void SkipAd() {
            if (!SkipAds && !MuteAds && !BypassAds) return;
            if (AdManaged) return;
            AdManaged = true;
            if (Process.GetProcessesByName("Spotify").Length <= 0) { AdManaged = false; return; }

            Process Spotify = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));

            const int VK_MEDIA_NEXT_TRACK = 0xB0;
            const int KEYEVENTF_EXTENDEDKEY = 0x001;
            const int KEYEVENTF_KEYUP = 0x002;
            
            if (SkipAds && GetAdStatus() == SpotifyAdStatus.Ad && ImmediateSkip) {
                keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero);

                for (int i = 0; i < 5; i++) // wait up to ~250ms
                    if (GetAdStatus() == SpotifyAdStatus.Ad) Thread.Sleep(50);
                if (GetAdStatus() == SpotifyAdStatus.None) { Console.WriteLine("Instantly Skipped Ad."); AdManaged = false; return; }}

            float originalVolume = GetApplicationVolume(Spotify.Id);
            if (originalVolume <= 0) originalVolume = FallbackVolume / 100;
            bool unknown = SpotifyAdStatus.Unknown == GetAdStatus();

            if (MuteAds || (unknown && BypassAds)) SetApplicationVolume(Spotify.Id, 0f); 
            for (int i = 0; i < 107; i++) // wait up to ~5,250ms
                if (GetAdStatus() != SpotifyAdStatus.None) Thread.Sleep(50);
            if (GetAdStatus() == SpotifyAdStatus.None) { Console.WriteLine("Ad Already Skipped."); AdManaged = false; return; }

            if (SkipAds && GetAdStatus() == SpotifyAdStatus.Ad) {
                keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero); }

            Console.WriteLine($"Skipped Ad After 5 Seconds");

            while (GetAdStatus() != SpotifyAdStatus.None) {
                for (int i = 0; i < 107; i++) // wait up to 5,250ms
                    if (GetAdStatus() != SpotifyAdStatus.None) Thread.Sleep(50);
                if (GetAdStatus() == SpotifyAdStatus.Ad && SkipAds) {
                    keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                    keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero); }}
            if (MuteAds || (unknown && BypassAds)) SetApplicationVolume(Spotify.Id, originalVolume);
            AdManaged = false; }

        private static SpotifyAdStatus GetAdStatus() {
            string trackInfo = GetSpotifyTrackInfo().ToLower();
            return ((trackInfo.Replace(" ", "").StartsWith("advertisement") || 
                trackInfo.Replace(" ", "") == "advertisement" || 
                trackInfo == "spotify") && !trackInfo.Contains(" - "))?SpotifyAdStatus.Ad:(trackInfo == "paused / unknown")?SpotifyAdStatus.Unknown:SpotifyAdStatus.None; }

        private static string GetSpotifyTrackInfo() {
            // get spotify process
            var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));

            // get track info from window title
            if (proc == null) return "Paused / Unknown";
            if (string.Equals(proc.MainWindowTitle, "Spotify Free", StringComparison.InvariantCultureIgnoreCase)) return "Paused / Unknown";
            return proc.MainWindowTitle; }

        private static void CreateShortcut() { 
            try {
                string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

                using (StreamWriter writer = new StreamWriter($"{Environment.GetFolderPath(Environment.SpecialFolder.Startup)}\\AdRework.url")) {
                    string app = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    writer.WriteLine("[InternetShortcut]");
                    writer.WriteLine("URL=file:///" + app);
                    writer.WriteLine("IconIndex=0");
                    string icon = app.Replace('\\', '/');
                    writer.WriteLine("IconFile=" + icon); }} catch(Exception) {}}

        private static void AutoAntiAd(object sender, EventArgs e) { 
            if (GetAdStatus() != SpotifyAdStatus.None && !AdManaged) SkipAd(); }

        private static string GetBetween(string Source, string Start, string End) {
            int StartI, EndI;
            if (Source.Contains(Start) && Source.Contains(End)) {
                if (Source.Substring(Source.IndexOf(Start)).Contains(End)) {
                    try {
                        StartI = Source.IndexOf(Start, 0) + Start.Length;
                        EndI = Source.IndexOf(End, StartI);
                        return Source.Substring(StartI, EndI - StartI); }
                    catch (ArgumentOutOfRangeException) { return ""; }}
                else return ""; }
            else return ""; }

        // config settings
        private static bool SkipAds = true;     
        private static bool MuteAds = true;     

        // bypass ads is whether spotify should be muted when no song name is returned, its not safe to skip here as it will break pausing songs
        private static bool BypassAds = true;

        // immediate skip is more stable, however if you are annoyed by the 'you can skip this ad in 5 seconds' banner disabling this should prevent it
        private static bool ImmediateSkip = true;

        // if the program should access the registry to try and start at startup
        private static bool RegistryStartup = true;

        // fallback volume is the volume spotify should be set to if an error ever occurs and spotify gets stuck at no volume
        private static float FallbackVolume = 100f;

        // if AdRework should always run all functions even if all related settings are disabled (more of a debug setting)
        private static bool ForceRun = false;

        // ms interval AdRework should check for ads at
        private static int AdInterval = 100;
        // ms interval AdRework should perform an 'integrity check' at
        private static int IntegrityInterval = 450;

        private static void LoadConfiguration() {
            try {
                string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                if (!Directory.Exists($"{AppData}\\dmbk")) Directory.CreateDirectory($"{AppData}\\dmbk");
                if (!Directory.Exists($"{AppData}\\dmbk\\AdRework")) Directory.CreateDirectory($"{AppData}\\dmbk\\AdRework");
                if (!File.Exists($"{AppData}\\dmbk\\AdRework\\config.ini")) { 
                    File.WriteAllText($"{AppData}\\dmbk\\AdRework\\config.ini", "SkipAds='True'\nMuteAds='True'\nBypassAds='True'\nImmediateSkip='True'\nRegistryStartup='True'\nForceRun='False'\nFallbackVolume='50'\nAdInterval='100'\nIntegrityInterval='450'");
                    return; }

                try {
                    string config = File.ReadAllText($"{AppData}\\dmbk\\AdRework\\config.ini");

                    SkipAds = bool.Parse(GetBetween(config, "SkipAds='", "'"));
                    MuteAds = bool.Parse(GetBetween(config, "MuteAds='", "'"));
                    BypassAds = bool.Parse(GetBetween(config, "BypassAds='", "'"));
                    ImmediateSkip = bool.Parse(GetBetween(config, "ImmediateSkip='", "'"));
                    RegistryStartup = bool.Parse(GetBetween(config, "RegistryStartup='", "'"));
                    ForceRun = bool.Parse(GetBetween(config, "ForceRun'", "'"));
                    FallbackVolume = Convert.ToInt32(GetBetween(config, "FallbackVolume='", "'"));
                    AdInterval = Convert.ToInt32(GetBetween(config, "AdInterval='", "'"));
                    IntegrityInterval = Convert.ToInt32(GetBetween(config, "IntegrityInterval='", "'")); }
                catch (Exception) { // if reading config fails, reset it
                    Console.WriteLine("failed to read config!");
                    File.WriteAllText($"{AppData}\\dmbk\\AdRework\\config.ini", "SkipAds='True'\nMuteAds='True'\nBypassAds='True'\nImmediateSkip='True'\nRegistryStartup='True'\nForceRun='False'\nFallbackVolume='50'\nAdInterval='100'\nIntegrityInterval='450'"); }
            } catch (Exception) {}}

        private static void IntegrityCheck(object sender, EventArgs e) { 
            Process Spotify = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
            if (GetAdStatus() == SpotifyAdStatus.None && GetApplicationVolume(Spotify.Id) <= 0) SetApplicationVolume(Spotify.Id, FallbackVolume / 100); }

        private static void AdReworkStart() {
            // load config
            LoadConfiguration();
            if (!SkipAds && !MuteAds && !BypassAds && !ForceRun) Process.GetCurrentProcess().Kill(); // terminate if it has nothing to do

            // start timer checking for ads every 100ms
            Timer timer = new Timer(AdInterval); timer.Elapsed += AutoAntiAd; timer.AutoReset = true; timer.Start();
            // make sure program isnt muted during songs every 235ms
            if (FallbackVolume > 0 || ForceRun) {
                Timer integritycheck = new Timer(IntegrityInterval); integritycheck.Elapsed += IntegrityCheck; integritycheck.AutoReset = true; integritycheck.Start(); }

            // set program to start with windows
            CreateShortcut();
            if (RegistryStartup)
                try { using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)) {
                    key.SetValue("AdRework", System.Reflection.Assembly.GetExecutingAssembly().Location); }} catch (Exception) {}

            // keep thread alive indefinetly
            Thread.Sleep(-1); }}}
