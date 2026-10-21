using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InspTools.Standalone
{
    // Ports stay private to the one formal InspTools product. These are not a
    // replacement SDK or a second machine Host.
    internal enum FrameReplayMode { Fixed, Sequence }

    internal sealed class CameraFrame
    {
        internal int Station;
        internal string CameraId;
        internal string SourcePath;
        internal long FrameId;
        internal byte[] EncodedImage;
        internal DateTime CapturedUtc;
    }

    internal interface ICameraFrameSource : IDisposable
    {
        void Open();
        CameraFrame Capture(int station);
        void Close();
    }

    internal interface IPlcInspectionPort
    {
        InspectionHandshakeSnapshot ReadSnapshot();
        // Notification arriving FROM PLC. Not a command to trigger a machine.
        void ReceivePlcTrigger(int station, long cycle);
        void ReportAoiStarted(int station, long cycle);
        void ReportAoiResult(int station, long cycle, string bin);
    }

    internal sealed class InspectionHandshakeSnapshot
    {
        internal int Station;
        internal long Cycle;
        internal bool PlcTrigger;
        internal int AoiStatus;
        internal string AoiResult;
        internal bool AoiStatus2;
    }

    /// <summary>
    /// Local, file-only replay; never opens sockets, cameras or vendor drivers.
    /// Logical stations may map to a shared physical camera ID (Shunt S3/S4).
    /// Files must remain inside an explicitly supplied, per-product fixture root.
    /// </summary>
    internal sealed class TwinCameraReplay : ICameraFrameSource
    {
        private sealed class Slot
        {
            internal string CameraId;
            internal string[] Files;
            internal int Index;
        }
        private readonly string root;
        private readonly Dictionary<int, Slot> slots = new Dictionary<int, Slot>();
        private FrameReplayMode mode;
        private bool open;
        private long frameId;

        internal TwinCameraReplay(string fixtureRoot, FrameReplayMode mode)
        {
            if (string.IsNullOrWhiteSpace(fixtureRoot))
                throw new ArgumentException("Fixture root is required");
            root = Path.GetFullPath(fixtureRoot).TrimEnd(Path.DirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            this.mode = mode;
        }

        internal void Bind(int station, string physicalCameraId, params string[] relativeImages)
        {
            if (open) throw new InvalidOperationException("Cannot rebind while open");
            if (station < 1 || station > 8) throw new ArgumentOutOfRangeException("station");
            if (string.IsNullOrWhiteSpace(physicalCameraId) ||
                relativeImages == null || relativeImages.Length == 0)
                throw new ArgumentException("Camera and image paths are required");
            var files = new List<string>();
            foreach (string relative in relativeImages)
            {
                if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative))
                    throw new ArgumentException("Only fixture-relative image files allowed");
                string full = Path.GetFullPath(Path.Combine(root, relative));
                if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Twin image path escapes fixture root");
                string extension = Path.GetExtension(full).ToLowerInvariant();
                if (extension != ".png" && extension != ".jpg" &&
                    extension != ".jpeg" && extension != ".bmp")
                    throw new InvalidDataException("Unsupported image format: " + extension);
                files.Add(full);
            }
            slots[station] = new Slot { CameraId = physicalCameraId, Files = files.ToArray() };
        }

        internal void SetReplayMode(FrameReplayMode next) { mode = next; }

        internal void SelectImage(int station, int zeroBasedIndex)
        {
            Slot slot = RequireStation(station);
            if (zeroBasedIndex < 0 || zeroBasedIndex >= slot.Files.Length)
                throw new ArgumentOutOfRangeException("zeroBasedIndex");
            slot.Index = zeroBasedIndex;
        }

        public void Open()
        {
            if (slots.Count == 0) throw new InvalidOperationException("No cameras bound");
            foreach (Slot slot in slots.Values)
                foreach (string image in slot.Files)
                    if (!File.Exists(image))
                        throw new FileNotFoundException("Twin image missing", image);
            open = true;
        }

        public CameraFrame Capture(int station)
        {
            if (!open) throw new InvalidOperationException("Twin camera not opened");
            Slot slot = RequireStation(station);
            string file = slot.Files[slot.Index];
            var info = new FileInfo(file);
            if (info.Length < 8 || info.Length > 64L * 1024 * 1024)
                throw new InvalidDataException("Twin image has invalid size");
            byte[] data = File.ReadAllBytes(file);
            bool png = data.Length >= 8 && data[0] == 0x89 && data[1] == 0x50 &&
                       data[2] == 0x4E && data[3] == 0x47;
            bool jpeg = data.Length >= 3 && data[0] == 0xFF &&
                        data[1] == 0xD8 && data[2] == 0xFF;
            bool bmp = data.Length >= 2 && data[0] == 0x42 && data[1] == 0x4D;
            if (!png && !jpeg && !bmp)
                throw new InvalidDataException("Invalid replay image header");
            if (mode == FrameReplayMode.Sequence)
                slot.Index = (slot.Index + 1) % slot.Files.Length;
            return new CameraFrame {
                Station = station, CameraId = slot.CameraId,
                SourcePath = file, FrameId = ++frameId, EncodedImage = data,
                CapturedUtc = DateTime.UtcNow
            };
        }

        private Slot RequireStation(int station)
        {
            Slot value;
            if (!slots.TryGetValue(station, out value))
                throw new InvalidOperationException("Station " + station + " not bound");
            return value;
        }

        public void Close() { open = false; }
        public void Dispose() { Close(); }
    }

    /// <summary>
    /// In-process PLC semantics only; no sockets/addresses/write commands.
    /// The PLC owns its trigger and its acknowledgement/counting scan.
    /// Exact Shunt trigger-clear/count timing requires field evidence.
    /// </summary>
    internal sealed class TwinPlcHandshake : IPlcInspectionPort
    {
        private InspectionHandshakeSnapshot snapshot =
            new InspectionHandshakeSnapshot { AoiResult = "" };

        public InspectionHandshakeSnapshot ReadSnapshot()
        {
            return new InspectionHandshakeSnapshot {
                Station = snapshot.Station, Cycle = snapshot.Cycle,
                PlcTrigger = snapshot.PlcTrigger, AoiStatus = snapshot.AoiStatus,
                AoiResult = snapshot.AoiResult, AoiStatus2 = snapshot.AoiStatus2
            };
        }

        public void ReceivePlcTrigger(int station, long cycle)
        {
            if (station < 1 || station > 8 || cycle <= snapshot.Cycle)
                throw new InvalidOperationException("Invalid station or stale PLC cycle");
            if (snapshot.PlcTrigger)
                throw new InvalidOperationException("PLC must acknowledge preceding cycle");
            snapshot = new InspectionHandshakeSnapshot {
                Station = station, Cycle = cycle, PlcTrigger = true,
                AoiStatus = 0, AoiResult = "", AoiStatus2 = false
            };
        }

        public void ReportAoiStarted(int station, long cycle)
        {
            VerifyCurrent(station, cycle);
            if (snapshot.AoiStatus != 0)
                throw new InvalidOperationException("Duplicate AOI start");
            snapshot.AoiStatus = 1;
        }

        public void ReportAoiResult(int station, long cycle, string bin)
        {
            VerifyCurrent(station, cycle);
            if (snapshot.AoiStatus != 1 || snapshot.AoiStatus2 ||
                (bin != "Ok" && bin != "Ng" && bin != "Ng2" && bin != "Retest"))
                throw new InvalidOperationException("Invalid AOI result transition");
            snapshot.AoiResult = bin;
            snapshot.AoiStatus2 = true;
        }

        // Simulator-only representation of PLC-owned trigger clear.
        internal void SimulatePlcAcknowledgement()
        {
            if (!snapshot.PlcTrigger || !snapshot.AoiStatus2)
                throw new InvalidOperationException("PLC cannot acknowledge incomplete AOI");
            snapshot.PlcTrigger = false;
        }

        private void VerifyCurrent(int station, long cycle)
        {
            if (!snapshot.PlcTrigger || snapshot.Station != station ||
                snapshot.Cycle != cycle)
                throw new InvalidOperationException("Wrong or untriggered AOI cycle");
        }
    }

    internal static class DeviceTwinSelfTest
    {
        internal static bool TryRun(string[] args, out int exitCode)
        {
            exitCode = 0;
            if (args == null || args.Length != 1 ||
                args[0] != "--device-twin-selftest") return false;
            string root = Path.Combine(Path.GetTempPath(),
                "InspToolsDeviceTwin-" + Guid.NewGuid().ToString("N"));
            int checks = 0;
            try
            {
                Directory.CreateDirectory(root);
                // Two tiny valid PNG fixtures. No production images, no hardware.
                byte[] png = Convert.FromBase64String(
                    "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9Y9h6FoAAAAASUVORK5CYII=");
                File.WriteAllBytes(Path.Combine(root, "first.png"), png);
                File.WriteAllBytes(Path.Combine(root, "second.png"), png);
                using (var camera = new TwinCameraReplay(root, FrameReplayMode.Fixed))
                {
                    camera.Bind(1, "DIA-CAM-S1", "first.png", "second.png");
                    camera.Bind(3, "SHUNT-CAM-S3-S4", "first.png");
                    camera.Bind(4, "SHUNT-CAM-S3-S4", "second.png");
                    camera.Open();
                    var a = camera.Capture(1);
                    var b = camera.Capture(1);
                    Expect(a.FrameId != b.FrameId &&
                        a.SourcePath == b.SourcePath, "fixed replay", ref checks);
                    camera.SetReplayMode(FrameReplayMode.Sequence);
                    var c = camera.Capture(1);
                    var d = camera.Capture(1);
                    Expect(c.SourcePath != d.SourcePath, "sequence replay", ref checks);
                    Expect(camera.Capture(3).CameraId == camera.Capture(4).CameraId,
                        "shared camera id", ref checks);
                    Expect(camera.Capture(3).Station == 3 &&
                        camera.Capture(4).Station == 4,
                        "logical stations separate", ref checks);
                    bool escaped = false;
                    camera.Close();
                    try { camera.Bind(8, "NONE", "..\\outside.png"); }
                    catch (InvalidOperationException) { escaped = true; }
                    Expect(escaped, "fixture path guard", ref checks);
                }
                var plc = new TwinPlcHandshake();
                plc.ReceivePlcTrigger(1, 1);
                Expect(plc.ReadSnapshot().AoiStatus == 0 &&
                    plc.ReadSnapshot().AoiResult == "", "cycle reset", ref checks);
                bool early = false;
                try { plc.ReportAoiResult(1, 1, "Ok"); }
                catch (InvalidOperationException) { early = true; }
                Expect(early, "reject result before start", ref checks);
                plc.ReportAoiStarted(1, 1);
                Expect(plc.ReadSnapshot().AoiStatus == 1, "status1", ref checks);
                plc.ReportAoiResult(1, 1, "Ng2");
                Expect(plc.ReadSnapshot().AoiResult == "Ng2" &&
                    plc.ReadSnapshot().AoiStatus2, "result then status2", ref checks);
                bool duplicate = false;
                try { plc.ReceivePlcTrigger(1, 2); }
                catch (InvalidOperationException) { duplicate = true; }
                Expect(duplicate, "PLC owns acknowledgement", ref checks);
                plc.SimulatePlcAcknowledgement();
                plc.ReceivePlcTrigger(2, 2);
                Expect(plc.ReadSnapshot().Station == 2 &&
                    plc.ReadSnapshot().AoiStatus == 0 &&
                    plc.ReadSnapshot().AoiResult == "", "fresh cycle", ref checks);
                bool invalid = false;
                try { plc.ReportAoiStarted(1, 2); }
                catch (InvalidOperationException) { invalid = true; }
                Expect(invalid, "reject wrong station", ref checks);
                Console.WriteLine("DEVICE_TWIN=PASS CHECKS=" + checks +
                    " EXTERNAL_CONNECTIONS=0");
            }
            catch (Exception error)
            {
                exitCode = 70;
                Console.Error.WriteLine("DEVICE_TWIN=FAIL " + error.GetBaseException().Message);
            }
            finally
            {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
            return true;
        }
        private static void Expect(bool ok, string name, ref int checks)
        {
            if (!ok) throw new InvalidOperationException(name);
            checks++;
        }
    }
}
