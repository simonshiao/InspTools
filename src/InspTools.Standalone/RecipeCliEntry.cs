using System;

namespace InspTools.Standalone
{
    // Standalone console entry for deterministic Recipe operations.
    internal static class RecipeCliEntry
    {
        [STAThread]
        private static int Main(string[] args)
        {
            int exitCode;
            if (DeviceTwinSelfTest.TryRun(args, out exitCode)) return exitCode;
            if (TrainingContracts.TryRun(args, out exitCode)) return exitCode;
            if (GeneralTrainingJob.TryRun(args, out exitCode)) return exitCode;
            if (args != null && args.Length == 1 &&
                string.Equals(args[0], "--delta-runtime-probe", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    bool loaded = DeltaNativeRuntimeBootstrap.InitializeIfPresent();
                    Console.WriteLine(loaded ? "DELTA_NATIVE_RUNTIME=READY" :
                        "DELTA_NATIVE_RUNTIME=NOT_BUNDLED");
                    return loaded ? 0 : 65;
                }
                catch (Exception error)
                {
                    Console.Error.WriteLine("DELTA_NATIVE_RUNTIME=FAIL " +
                        error.GetBaseException().Message);
                    return 66;
                }
            }
            if (R3FacadeProbe.TryRun(args, out exitCode)) return exitCode;
            if (NativeRuntimeGate.TryPreflight(args, out exitCode)) return exitCode;
            if (CvDiagnosticProbe.TryRun(args, out exitCode)) return exitCode;
            if (args != null && args.Length == 6 &&
                string.Equals(args[0], "--recipe-set-flag", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    if (args[5] != "--commit")
                        throw new ArgumentException("寫入需要明確指定 --commit");
                    int station = int.Parse(args[2]);
                    bool enabled;
                    if (string.Equals(args[4], "on", StringComparison.OrdinalIgnoreCase))
                        enabled = true;
                    else if (string.Equals(args[4], "off", StringComparison.OrdinalIgnoreCase))
                        enabled = false;
                    else throw new ArgumentException("狀態只能是 on 或 off");
                    var changes = new System.Collections.Generic.Dictionary<string, bool>
                    {
                        {args[3], enabled}
                    };
                    Console.OutputEncoding = new System.Text.UTF8Encoding(false);
                    string backup = NativeRecipeProbe.WriteEditableFlags(args[1], station, changes);
                    Console.WriteLine("NATIVE_FLAG_SAVE=PASS BACKUP=" + backup);
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("NATIVE_FLAG_SAVE=FAIL " + ex.GetBaseException().Message);
                    return 5;
                }
            }
            if (args != null && args.Length == 2 &&
                string.Equals(args[0], "--recipe-params", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Console.OutputEncoding = new System.Text.UTF8Encoding(false);
                    Console.WriteLine(NativeRecipeProbe.ReadOnlySummary(args[1]));
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("RECIPE_PARAMS_FAIL: " + ex.GetBaseException().Message);
                    return 4;
                }
            }
            if (NativeRecipeProbe.TryRun(args, out exitCode)) return exitCode;
            if (StandaloneRecipeCli.TryRun(args, out exitCode)) return exitCode;
            Console.Error.WriteLine(
                "Usage: InspToolsRecipeCli.exe --recipe-list | --recipe-check NAME | " +
                "--recipe-copy EXISTING NEW | --recipe-import SOURCE_DIRECTORY NEW | " +
                "--recipe-params NAME | --recipe-native-probe R2PROBE-NAME [--roundtrip] | " +
                "--recipe-set-flag NAME STATION FLAG on/off --commit");
            return 2;
        }
    }
}
