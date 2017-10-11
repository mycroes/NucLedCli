using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;

namespace NucLedCli
{
    class Program
    {
        private const string Scope = @"\root\WMI";
        private const string Query = "SELECT * FROM CISD_WMI";
        private const string Method = "SetState";

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Usage(Console.Error, "No arguments supplied.");
                return;
            }

            if (args[0] == "help")
            {
                Usage(Console.Out, "\t" + string.Join("\n\t", args.Skip(1).Select(a => $"{a}: {Explain(a)}")));
                return;
            }

            try
            {
                if (args.Length < 4)
                {
                    Usage(Console.Error, "Insufficient arguments supplied.");
                    return;
                }

                byte[] data =
                    {Parse<Led>(args[0]), Parse<Color>(args[1]), Parse<Mode>(args[2]), ParseBrightness(args[3])};

                var wmi = new ManagementObjectSearcher(Scope, Query,
                    new EnumerationOptions {ReturnImmediately = false});

                foreach (var o in wmi.Get())
                {
                    var mo = (ManagementObject) o;
                    mo.InvokeMethod(Method, new object[] {BitConverter.ToInt32(data, 0)});
                    mo.Dispose();
                }
            }
            catch (ArgumentException e)
            {
                Usage(Console.Error, e.Message);
            }
            catch (ManagementException e)
            {
                Console.Error.WriteLine($"WMI error: {e.Message} (not running on NUC?)");
            }
        }

        private static string Explain(string arg)
        {
            var name = char.ToUpper(arg[0]) + arg.Substring(1).ToLower();
            switch (name)
            {
                case "Brightness": return $"0-{100}";
                case "Led":
                case "Mode":
                case "Color": return string.Join(", ", Enum.GetValues(Type.GetType($"{typeof(Color).Namespace}.{name}", true)).Cast<object>());
                default: return "Unknown argument";
            }
        }

        private static void Usage(TextWriter writer, string message = null)
        {
            var exe = Path.GetFileNameWithoutExtension(Assembly.GetCallingAssembly().Location);
            writer.WriteLine("Usage:");
            writer.WriteLine($"\t{exe} led color mode brightness");
            writer.WriteLine($"\t{exe} help [led, color, mode, brightness]");

            if (message == null) return;

            writer.WriteLine();
            writer.WriteLine(message);
        }

        private static byte ParseBrightness(string arg)
        {
            if (!byte.TryParse(arg, out var parsed)) throw new ArgumentException($"Failed to parse brightness value from {arg}.");
            if (parsed > 100) throw new ArgumentException($"Brightness can't be larger than {100}.");

            return parsed;
        }

        private static byte Parse<T>(string arg) where T : struct, IComparable
        {
            if (!Enum.TryParse(arg, true, out T res) || !Enum.IsDefined(typeof(T), res))
                throw new ArgumentException($"Failed to parse {typeof(T).Name} from {arg}.");

            return (byte) (object) res;
        }
    }
}
