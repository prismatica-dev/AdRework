using CSCore.CoreAudioAPI;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Timer = System.Timers.Timer;

namespace AdRework {
    static class Program {
        [STAThread]
        static void Main() { AdReworkStart(); }
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
        private static AudioSessionManager2 GetDefaultAudioSessionManager2(DataFlow dataFlow) {
            using (var enumerator = new MMDeviceEnumerator()) {
                using (var device = enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia)) {
                    Console.WriteLine("DefaultDevice: " + device.FriendlyName);
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
        private static int AdsSkipped = 0;
        private static void SkipAd() {
            if (AdManaged) return;
            AdManaged = true;
            if (Process.GetProcessesByName("Spotify").Length <= 0) { AdManaged = false; return; }

            Process Spotify = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));

            const int VK_MEDIA_NEXT_TRACK = 0xB0;
            const int KEYEVENTF_EXTENDEDKEY = 0x001;
            const int KEYEVENTF_KEYUP = 0x002;

            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero);

            for (int i = 0; i < 5; i++) // wait up to 250ms
                if (IsAd()) Thread.Sleep(50);
            if (!IsAd()) { Console.WriteLine("Instantly Skipped Ad."); AdManaged = false; return; }

            float originalVolume = GetApplicationVolume(Spotify.Id);

            SetApplicationVolume(Spotify.Id, 0f); 
            for (int i = 0; i < 107; i++) // wait up to 5,250ms
                if (IsAd()) Thread.Sleep(50);
            if (!IsAd()) { Console.WriteLine("Ad Already Skipped."); AdManaged = false; return; }

            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero);

            Console.WriteLine($"Skipped Ad After 5 Seconds");

            while (IsAd()) {
                for (int i = 0; i < 107; i++) // wait up to 5,250ms
                    if (IsAd()) Thread.Sleep(50);
                if (IsAd()) {
                    keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                    keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero); }}
            SetApplicationVolume(Spotify.Id, originalVolume);
            AdManaged = false; }

        private static bool IsAd() {
            string trackInfo = GetSpotifyTrackInfo().ToLower();
            return (trackInfo.Replace(" ", "").StartsWith("advertisement") || 
                trackInfo.Replace(" ", "") == "advertisement" || 
                trackInfo == "spotify") && !trackInfo.Contains(" - "); }

        private static string GetSpotifyTrackInfo() {
            var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));

            if (proc == null) return "Closed";
            if (string.Equals(proc.MainWindowTitle, "Spotify Free", StringComparison.InvariantCultureIgnoreCase)) return "Paused";
            return proc.MainWindowTitle; }

        private static void AutoAntiAd(object sender, EventArgs e) { if (IsAd() && !AdManaged) SkipAd(); }

        private static void AdReworkStart() {
            Process spotify = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
            Console.WriteLine(spotify.MainWindowTitle);
            IntPtr child = spotify.MainWindowHandle;
            Console.WriteLine(child.ToInt32());
            
            Timer timer = new Timer(50); timer.Elapsed += AutoAntiAd; timer.AutoReset = true; timer.Start();

            try { using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)) {
                key.SetValue("AdRework", System.Reflection.Assembly.GetExecutingAssembly().Location); }} catch (Exception) {}
            Thread.Sleep(-1); }}}
