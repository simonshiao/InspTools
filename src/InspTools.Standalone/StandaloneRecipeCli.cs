using System;
using System.IO;
using System.Text;

namespace InspTools.Standalone
{
    /// <summary>Safe, machine-free Recipe management for scripts and AI agents.</summary>
    internal static class StandaloneRecipeCli
    {
        internal static bool TryRun(string[] args, out int code)
        {
            code = 0;
            if (args == null || args.Length == 0)
                return false;
            string requested = args[0].ToLowerInvariant();
            if (requested != "--recipe-list" && requested != "--recipe-check" &&
                requested != "--recipe-copy" && requested != "--recipe-import")
                return false;
            try
            {
                Console.OutputEncoding = new UTF8Encoding(false);
                string command = args[0].ToLowerInvariant();
                switch (command)
                {
                    case "--recipe-list":
                        foreach (string name in StandaloneRecipeStore.List()) Console.WriteLine(name);
                        break;
                    case "--recipe-check":
                        Require(args, 2);
                        Console.WriteLine("RECIPE_VALID=" + StandaloneRecipeStore.Validate(args[1]));
                        break;
                    case "--recipe-copy":
                        Require(args, 3);
                        Console.WriteLine("RECIPE_CREATED=" + StandaloneRecipeStore.Duplicate(args[1], args[2]));
                        break;
                    case "--recipe-import":
                        Require(args, 3);
                        Console.WriteLine("RECIPE_IMPORTED=" + StandaloneRecipeStore.Import(args[1], args[2]));
                        break;
                    default:
                        throw new ArgumentException("Unknown Recipe command: " + command);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("RECIPE_FAIL: " + e.GetBaseException().Message);
                code = 2;
                return true;
            }
        }

        private static void Require(string[] args, int length)
        {
            if (args.Length != length)
                throw new ArgumentException("Recipe command arguments invalid");
        }
    }
}
