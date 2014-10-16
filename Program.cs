using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CSharp;

namespace LoLUpdater
{
    internal static class Program
    {
        private const string AirInstaller = "air15_win.exe";
        private const string CgInstaller = "Cg-3.1_April2012_Setup.exe";
        private const string Updater = "LoLUpdater Updater.exe";
        private const string SKernel = "kernel32.dll";
        private static readonly string AdobePath =
            Path.Combine(Environment.Is64BitProcess
                    ? Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86)
                    : Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles), "Adobe AIR",
                "Versions", "1.0");

        private static readonly string Air = Ver("projects", "lol_air_client");
        // DO NOT CHANGE ORDER OF STRINGS IN AirFiles!
        private static readonly string[] AirFiles = { Path.Combine("Resources", "NPSWF32.dll"), "Adobe AIR.dll" };
        private static readonly bool Avx = Dll(2, "GetEnabledXStateFeatures");

        private static readonly ManagementBaseObject[] CpuInfo =
            new ManagementObjectSearcher("Select * from Win32_Processor").Get()
                .Cast<ManagementBaseObject>()
                .AsParallel()
                .ToArray();
        // Lazy semi-futureproof Intel method, no info on AMD CPU names yet.
        private static readonly bool Avx2 =
            CpuInfo.Any(
                item =>
                    item["Name"].ToString()
                        .Contains(
                            new List<string>(new[] { "Haswell", "Broadwell", "Skylake", "Cannonlake" }).AsParallel()
                                .ToString()));

        private static readonly bool Riot = Directory.Exists("RADS");
        private const string CfgFile = "game.cfg";
        private static readonly string[] CfgFilez = Riot
            ? new[]
            {
                CfgFile
            }
            : new[]
            {
                CfgFile, "GamePermanent.cfg", "GamePermanent_zh_MY.cfg",
                "GamePermanent_en_SG.cfg"
            };

        private static readonly string[] CgFiles = { "Cg.dll", "CgGL.dll", "CgD3D9.dll" };
        private static readonly string[] GameFiles = CgFiles.Concat(new[] { "tbb.dll" }).ToArray();

        private static readonly Mutex OnlyInstance = new Mutex(true,
            @"Global\TOTALLYNOTMYMUTEXVERYRANDOMANDRARE#DOGE: 9bba28e3-c2a3-4c71-a4f8-bb72b2f57c3b");

        // Note: Checksums are in SHA512
        private const string Ipf = "IsProcessorFeaturePresent";
        private static readonly string Sln = Ver("solutions", "lol_game_client_sln");
        private static readonly bool Sse = Dll(6, Ipf);
        private static readonly bool Sse2 = Dll(10, Ipf);

        private static readonly string TbbSum = Avx2
            ? "13d78f0fa6b61a13e5b7cf8e4fa4b071fc880ae1356bd518960175fce7c49cba48460d6c43a6e28556be7309327abec7ec83760cf29b043ef1178904e1e98a07"
            : (Avx
                ? "d81edd17a891a2ef464f3e69ff715595f78c229867d8d6e6cc1819b426316a0bf6df5fa09a7341995290e4efe4b884f8d144e0fe8e519c4779f5cf5679db784c"
                : (Sse2
                    ? "61fea5603739cb3ca7a0f13b3f96e7c0c6bcff418d1404272c9fcf7cb5ce6fef7e21a5ee2026fc6af4ebc596d1d912e8201b298f7d092004d8f5256e22b05b64"
                    : Sse
                        ? "fa1cc95eff4ca2638b88fcdb652a7ed19b4a086bab8ce4a7e7f29324e708b5c855574c5053fe3ea84917ca0293dc97bac8830d5be2770a86ca073791696fcbec"
                        : "0c201b344e8bf0451717d6b15326d21fc91cc5981ce36717bf62013ff5624b35054e580a381efa286cc72b6fe0177499a252876d557295bc4e29a3ec92ebfa58"));

        private static string _cgBinPath = Environment.GetEnvironmentVariable("CG_BIN_PATH",
            EnvironmentVariableTarget.User);

        // SAME ORDER AS AirFiles (Flash, Air)
        private static readonly string AirSum = string.Join(string.Empty,
            "e16c024424405ead77a89fabbb4a95a99e5552f33509d872bb7046cba4afb16f5a5bbf496a46b1b1ee9ef8b9e8ba6720bc8faccb654c5317e8142812e56b4930",
            "33f376d3f3a76a2ba122687b18e0306d45a8c65c89d3a51cc956bf4fa6d9bf9677493afa9b7bb5227fa1b162117440a5976484df6413f77a88ff3759ded37e8e"
            );

        private static readonly string Adobe = Riot
            ? Path.Combine("RADS", "projects", "lol_air_client", "releases", Air, "deploy", "Adobe AIR", "Versions",
                "1.0")
            : Path.Combine("Air", "Adobe AIR", "Versions", "1.0");

        private static readonly string Game = Riot
            ? Path.Combine("RADS", "solutions", "lol_game_client_sln", "releases", Sln, "deploy")
            : "Game";

        private static int _userInput;
        private static readonly bool Installing = Convert.ToBoolean(_userInput = 1);

        private static bool _notdone;

        private static readonly bool MultiCore =
            CpuInfo.AsParallel().Sum(item => ToInt(item["NumberOfCores"].ToString())) > 1;

        private static readonly Uri TbbUri =
            new Uri(new Uri("https://github.com/Loggan08/LoLUpdater/raw/master/Tbb/"), Avx2
                ? "Avx2.dll"
                : (Avx
                    ? "Avx.dll"
                    : (Sse2 ? "Sse2.dll" : Sse ? "Sse.dll" : "Default.dll")));

        private static void Main(string[] args)
        {
            if (!OnlyInstance.WaitOne(TimeSpan.Zero, true)) return;
            GC.KeepAlive(OnlyInstance);

            using (
                Stream stream =
                    WebRequest.Create(new Uri("https://github.com/Loggan08/LoLUpdater/raw/master/SHA512.txt"))
                        .GetResponse()
                        .GetResponseStream())
            {
                if (stream == null) return;
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    if (!Sha512("LoLUpdater.exe",
                        streamReader.ReadToEnd()))
                    {
                        using (CSharpCodeProvider cscp = new CSharpCodeProvider())
                        {
                            CompilerParameters parameters = new CompilerParameters
                            {
                                GenerateInMemory = false,
                                GenerateExecutable = true,
                                IncludeDebugInformation = false,
                                CompilerOptions = "/optimize",
                                OutputAssembly = Updater
                            };
                            parameters.ReferencedAssemblies.Add("System.dll");
                            parameters.ReferencedAssemblies.Add("System.Core.dll");

                            using (
                                Stream stream2 =
                                    WebRequest.Create(
                                        new Uri("https://github.com/Loggan08/LoLUpdater/raw/master/Updater.cs"))
                                        .GetResponse()
                                        .GetResponseStream())
                            {


                                if (stream2 == null) return;
                                var list = new List<string>();
                                using (StreamReader streamReader2 = new StreamReader(stream2))
                                {
                                    string line;
                                    while ((line = streamReader2.ReadLine()) != null)
                                    {
                                        list.Add(line);
                                    }
                                    CompilerResults result = cscp.CompileAssemblyFromSource(parameters, string.Join(Environment.NewLine, list.ToArray()));
                                    Normalize(string.Empty, result.PathToAssembly, true);
                                    Unblock(string.Empty, result.PathToAssembly, true);
                                    Process.Start(result.PathToAssembly);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (File.Exists(Updater))
                        {
                            File.Delete(Updater);
                        }
                    }
                }

            }

            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "/?":
                    case "-h":
                    case "--help":
                        Console.WriteLine(
                            string.Join("Command-Line Arguments:",
                                string.Empty,
                                "-install : Installs LoLUpdater",
                                "-uninst : Uninstalls LoLUpdater",
                                "--help /? -h : Shows this menu")
                        );
                        Console.ReadLine();
                        break;

                    case "-install":
                        _userInput = 1;
                        Patch();
                        break;

                    case "-uninst":
                        _userInput = 2;
                        Patch();
                        break;
                }
            }
            else
            {
                _userInput = DisplayMenu();
                Console.Clear();
                Patch();
            }
        }

        private static void Patch()
        {
            do
            {
                Parallel.ForEach(new[]{
            "LoLClient", "LoLLauncher", "LoLPatcher",
            "League of Legends"
        }
                , t =>
                {
                    Parallel.ForEach(
                        Process.GetProcesses()
                            .Where(
                                process =>
                                    string.Equals(process.ProcessName, t, StringComparison.CurrentCultureIgnoreCase))
                            .AsParallel(), process =>
                            {
                                process.Kill();
                                process.WaitForExit();
                            });
                });
            } while (_notdone);
            if (Installing & !Directory.Exists("Backup"))
            {
                Directory.CreateDirectory("Backup");
                Directory.CreateDirectory(Path.Combine("Backup", "Resources"));
            }
            if(Installing & !Directory.Exists(Path.Combine("Backup", "Resources")))
            { Directory.CreateDirectory(Path.Combine("Backup", "Resources")); }

            if (_userInput < 3)
            {
                Console.WriteLine("Configuring...");

                Parallel.ForEach(GameFiles,
    file =>
    {
        Copy(string.Empty, Game, file, string.Empty, Installing);
    });
                Parallel.ForEach(AirFiles,
file =>
{

    Copy(string.Empty, Adobe, file, string.Empty, Installing);


});


                Copy(string.Empty, "Config",  CfgFilez.ToString(), string.Empty, Installing);


                Parallel.ForEach(CfgFilez,
                    file =>
                    {
                        Copy(string.Empty, Path.Combine("Game", "DATA", "CFG", "defaults"), file, string.Empty, Installing);
                    });


                Console.WriteLine(string.Empty);
                if (!Installing) return;
                if (!File.Exists(Path.Combine(AdobePath, "Adobe AIR.dll")))
                {
                    AirInstall();
                }
                else
                {
                    if (new Version(
                        FileVersionInfo.GetVersionInfo(Path.Combine(AdobePath, "Adobe AIR.dll")).FileVersion) <
                        new Version("15.0.0.297"))
                    { AirInstall(); }

                }

                if (string.IsNullOrEmpty(_cgBinPath))

                { CgInstall(); }
                else
                {
                    if (
                        new Version(FileVersionInfo.GetVersionInfo(Path.Combine(_cgBinPath, "cg.dll")).FileVersion) <
                        new Version("3.1.0.13"))
                    { CgInstall(); }

                }
            }
            switch (_userInput)
            {
                case 1:
                    Console.WriteLine("Installing...");
                    Parallel.ForEach(AirFiles,
                            file => { Copy(AdobePath, Adobe, file, string.Empty, null); });
                    Parallel.ForEach(CgFiles, file =>
                    {
                        Copy(_cgBinPath, Game, file, string.Empty, null);

                    });
                    Download("tbb.dll", TbbSum, TbbUri, Game);
                    Cfg(CfgFilez.ToString(), "Config", MultiCore);
                    Parallel.ForEach(CfgFilez,
                        file => { Cfg(file, Path.Combine("Game", "DATA", "CFG", "defaults"), MultiCore); });


                    FinishedPrompt("Done Installing!");
                    break;

                case 2:
                    Parallel.ForEach(GameFiles,
                                    file =>
                                    {
                                        if (!File.Exists(Path.Combine("Backup", file))) return;
                                        File.Delete(Path.Combine("Backup", file));
                                    });
                    Parallel.ForEach(AirFiles,
                        file =>
                        {
                            if (!File.Exists(Path.Combine("Backup", file))) return;
                            File.Delete(Path.Combine("Backup", file));
                        });
                    if (Riot)
                    {
                        if (!File.Exists(Path.Combine("Backup", CfgFilez.ToString()))) return;
                        File.Delete(Path.Combine("Backup", CfgFilez.ToString()));
                    }
                    else
                    {
                        Parallel.ForEach(CfgFilez,
                            file =>
                            {
                                if (!File.Exists(Path.Combine("Backup", file))) return;
                                File.Delete(Path.Combine("Backup", file));
                            });
                    }
                    FinishedPrompt("Done Uninstalling!");
                    break;

                case 3:
                    Environment.Exit(0);
                    break;
            }
        }

        private static void CgInstall()
        {



            using (Stream stream = WebRequest.Create(new Uri(new Uri("http://developer.download.nvidia.com/cg/Cg_3.1/"),
                       CgInstaller))
               .GetResponse()
               .GetResponseStream())
            {

                ByteDl(stream, CgInstaller);
            }

            if (!File.Exists(CgInstaller)) return;
            Normalize(string.Empty, CgInstaller, true);
            Unblock(string.Empty, CgInstaller, true);

            Process cg = new Process
            {
                StartInfo =
                    new ProcessStartInfo
                    {
                        FileName =
                            CgInstaller,
                        Arguments = "/silent /TYPE=compact"
                    }
            };
            cg.Start();
            cg.WaitForExit();

            _cgBinPath = Environment.GetEnvironmentVariable("CG_BIN_PATH",
                EnvironmentVariableTarget.User);
            File.Delete(CgInstaller);
        }

        private static void AirInstall()
        {

            using (Stream stream = WebRequest.Create(new Uri(new Uri("https://labsdownload.adobe.com/pub/labs/flashruntimes/air/"),
                        AirInstaller))
                .GetResponse()
                .GetResponseStream())
            {
                ByteDl(stream, AirInstaller);
            }
            if (!File.Exists(AirInstaller)) return;
            Normalize(string.Empty, AirInstaller, true);
            Unblock(string.Empty, AirInstaller, true);
            Process airwin = new Process
            {
                StartInfo =
                    new ProcessStartInfo
                    {
                        FileName =
                            AirInstaller,
                        Arguments = "-silent"
                    }
            };
            airwin.Start();
            airwin.WaitForExit();
            File.Delete(AirInstaller);
        }

        private static void ByteDl(Stream stream, string path)
        {

            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] buffer = new byte[4096];
                int count;
                do
                {
                    count = stream.Read(buffer, 0, buffer.Length);
                    memoryStream.Write(buffer, 0, count);
                } while (count != 0);
                File.WriteAllBytes(path, memoryStream.ToArray());
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool DllType(byte arg);

        [DllImport(SKernel, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern void DeleteFile(string file);

        private static bool Dll(byte arg, string func)
        {
            bool ok = false;
            IntPtr pDll = LoadLibrary(SKernel);
            if (pDll == IntPtr.Zero) return false;
            IntPtr pFunc = GetProcAddress(pDll, func);
            if (pFunc != IntPtr.Zero)
            {
                DllType bar = (DllType)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(DllType));
                ok = bar(arg);
            }
            FreeLibrary(pDll);
            return ok;
        }



        private static void Copy(string from, string path, string file,string to, bool? mode)
        {
            try
            {
                if (mode.HasValue)
                {
                    string temp;
                    if (mode.Value)
                    {
                        if (path.Equals(Game))
                        {
                            temp = Game;
                            if (!string.IsNullOrEmpty(to) &&
                                File.Exists(Path.Combine(temp, to, file)))
                            {
                                Normalize(temp, Path.Combine(to, file), false);
                                File.Copy(
                                    Path.Combine(temp, to, file)
                                    , Path.Combine("Backup", file),
                                    true);
                                Unblock(temp, Path.Combine(to, file), false);
                            }
                            if (!File.Exists(Path.Combine(temp, file))) return;
                            Normalize(temp, file, false);
                            File.Copy(Path.Combine(
                                temp, file)
                                , Path.Combine("Backup", file),
                                true);
                            Unblock(temp, file, false);
                        }
                        else
                        {
                            temp = Adobe;
                            if (!string.IsNullOrEmpty(to) &&
                               File.Exists(Path.Combine(temp, to, file)))
                            {
                                Normalize(temp, Path.Combine(to, file), false);
                                File.Copy(
                                    Path.Combine(temp, to, file)
                                    , Path.Combine("Backup", file),
                                    true);
                                Unblock(temp, Path.Combine(to, file), false);
                            }
                            if (!File.Exists(Path.Combine("Backup", file))) return;
                            Normalize(temp, file, false);
                            File.Copy(
                               Path.Combine("Backup", file), Path.Combine(
                                temp, file),
                                true);
                            Unblock(temp, file, false);
                        }
                        // To backup if mode = true
                        



                    }
                    else
                    {
                        // from backup if mode = false
                        if (path.Equals(Air))
                        {
                            temp = Air;
                            if (!string.IsNullOrEmpty(to) &&
                                File.Exists(Path.Combine(temp, to, file)))
                            {
                                Normalize(temp, Path.Combine(to, file), false);
                                File.Copy(
                                    
                                   Path.Combine("Backup", file),Path.Combine(temp, to, file),
                                    true);
                                Unblock(temp, Path.Combine(to, file), false);
                            }
                            if (!File.Exists(Path.Combine(temp, file))) return;
                            Normalize(temp, file, false);
                            File.Copy(
                                Path.Combine("Backup", file), Path.Combine(
                                temp, file),
                                true);
                            Unblock(temp, file, false);
                        }
                        else
                        {
                            temp = Adobe;
                            if (!string.IsNullOrEmpty(to) &&
                               File.Exists(Path.Combine(temp, to, file)))
                            {
                                Normalize(temp, Path.Combine(to, file), false);
                                File.Copy(
                                     Path.Combine("Backup", file), Path.Combine(temp, to, file)
                                    ,
                                    true);
                                Unblock(temp, Path.Combine(to, file), false);
                            }
                            if (!File.Exists(Path.Combine("Backup", file))) return;
                            Normalize(temp, file, false);
                            File.Copy(
                               Path.Combine(
                                temp, file), Path.Combine("Backup", file),
                                true);
                            Unblock(temp, file, false);
                        }

                    }


                }

                else
                {
                    if (string.IsNullOrEmpty(to) || !(File.Exists(Path.Combine(@from, file)) & Directory.Exists(to)))
                        return;
                    Normalize(string.Empty, Path.Combine(@from, file), false);
                    File.Copy(Path.Combine(@from, file), Path.Combine(to, file), true);
                    Unblock(string.Empty, Path.Combine(@from, file), false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private static void Cfg(string file, string path, bool mode)
        {
            if (!File.Exists(Path.Combine(path, file))) return;
            Normalize(string.Empty, Path.Combine(path, file), false);
            string text = File.ReadAllText(Path.Combine(path, file));
            text = Regex.Replace(text, "\nEnableParticleOptimization=[01]|$",
                string.Format("{0}{1}", Environment.NewLine, "EnableParticleOptimization=1"));
            if (mode)
            {
                text = Regex.Replace(text, "\nDefaultParticleMultiThreading=[01]|$",
                    string.Format("{0}{1}", Environment.NewLine, "DefaultParticleMultiThreading=1"));
            }
            else
            {
                if (!text.Contains("DefaultParticleMultiThreading=1")) return;
                text = text.Replace("DefaultParticleMultiThreading=1", "DefaultParticleMultiThreading=0");
            }
            File.WriteAllText(Path.Combine(path, file), text);
            Unblock(string.Empty, Path.Combine(path, file), false);
        }

        private static int DisplayMenu()
        {
            int num = 0;
            Console.WriteLine(
                String.Join(Environment.NewLine,
                    string.Empty, "For a list of Command-Line Arguments, start lolupdater with --help", string.Empty,
                    "Select method:",
                    string.Empty,
                    "1. Install",
                    "2. Uninstall",
                    "4. Exit")
            );
            var readLine = Console.ReadLine();
            if (readLine == null) return num;
            string result = readLine.Trim();
            while (!int.TryParse(result, out num) && num < 1 && num > 3)
            {
                Console.WriteLine("{0} is not a valid input. Please try again.", result);
                result = readLine.Trim();
            }
            return num;
        }

        private static void Download(string file, string sha512, Uri uri, string path)
        {
            if (!File.Exists(QuickPath(path, file)))
            {
                DlExt(file, uri, path);
            }
            else
            {
                Normalize(path, file, false);
                if (Sha512(QuickPath(path, file), sha512)) return;
                DlExt(file, uri, path);
            }
            Unblock(path, file, false);
        }

        private static void DlExt(string file, Uri uri, string path)
        {
            using (Stream stream = WebRequest.Create(uri)
                    .GetResponse()
                    .GetResponseStream())
            {
                ByteDl(stream, QuickPath(path, file));
            }
        }

        private static void FinishedPrompt(string message)
        {
            string permanentSum = string.Join(string.Empty,
                "ba3d17fc13894ee301bc11692d57222a21a9d9bbc060fb079741926fb10c9b1f5a4409b59dbf63f6a90a2f7aed245d52ead62ee9c6f8942732b405d4dfc13a22",
                "db7dd6d8b86732744807463081f408356f3031277f551c93d34b3bab3dbbd7f9bca8c03bf9533e94c6282c5fa68fa1f5066d56d9c47810d5ebbe7cee0df64db2",
                "cad3b5bc15349fb7a71205e7da5596a0cb53cd14ae2112e84f9a5bd844714b9e7b06e56b5938d303e5f7ab077cfa79f450f9f293de09563537125882d2094a2b",
                TbbSum);

            Parallel.ForEach(AirFiles, file =>
            {
                Verify(Game, file, AirSum);

            });

            Parallel.ForEach(GameFiles, file =>
            {
                Verify(Game,
                    file, permanentSum);
            });


            Console.WriteLine("{0}", message);
            if (Riot)
            {
                Process.Start("lol.launcher.exe");
            }
            _notdone = false;
            Console.ReadLine();
            Environment.Exit(0);
        }

        private static long ToInt(string value)
        {
            int result;
            int.TryParse(value, out result);
            return result;
        }

        private static void Normalize(string path, string file, bool exe)
        {
            string temp;
            if (path.Equals(Game))
            {
                temp = Game;
                if (!exe)
                {
                    if (!new FileInfo(Path.Combine(temp, file)).Attributes
                        .Equals(FileAttributes.ReadOnly)) return;
                    File.SetAttributes(Path.Combine(temp, file),
                        FileAttributes.Normal);
                }
                else
                {
                    if (!new FileInfo(path).Attributes
                        .Equals(FileAttributes.ReadOnly)) return;
                    File.SetAttributes(temp,
                        FileAttributes.Normal);
                }
            }
            else
            {
                temp = Adobe;
                if (!exe)
                {
                    if (!new FileInfo(Path.Combine(temp, file)).Attributes
                        .Equals(FileAttributes.ReadOnly)) return;
                    File.SetAttributes(Path.Combine(temp, file),
                        FileAttributes.Normal);
                }
                else
                {
                    if (!new FileInfo(path).Attributes
                        .Equals(FileAttributes.ReadOnly)) return;
                    File.SetAttributes(temp,
                        FileAttributes.Normal);
                }
                

            }
            
        }

        private static void Unblock(string path, string file, bool exe)
        {
            string temp;
            if (path.Equals(Game))
            {
                temp = Game;
                DeleteFile(QuickPath(temp, file) + ":Zone.Identifier");
            }
            else
            {
                temp = Adobe;
                DeleteFile(QuickPath(temp, file) + ":Zone.Identifier");
            }
            if (!exe)
            {
                DeleteFile(path + ":Zone.Identifier");   
            }
        }

        private static string QuickPath(string path, string file)
        {
            return Path.Combine(path, file);
        }

        private static bool Sha512(string file, string sha512)
        {
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                StringBuilder sb = new StringBuilder();

                fs.Seek(0, SeekOrigin.Begin);

                Parallel.ForEach(SHA512.Create().ComputeHash(fs), b => { sb.Append(b.ToString("x2")); });

                return
                    Encoding.ASCII.GetBytes(sb.ToString())
                        .Where((t, i) => t == Encoding.ASCII.GetBytes(sha512)[i]).AsParallel()
                        .Any();
            }
        }

        private static string Ver(string path, string path1)
        {
            if (!Directory.Exists(Path.Combine("RADS", path, path1, "releases"))) return string.Empty;
            string dir = Directory.GetDirectories(Path.Combine("RADS", path, path1, "releases")).ToString();
            return dir.Length == 1
                ? dir
                : Path.GetFileName(Directory.GetDirectories(Path.Combine("RADS", path, path1, "releases")).Max());
        }

        private static void Verify(string path, string file, string sha512)
        {
            Console.WriteLine(
               !Sha512(QuickPath(path, file), sha512)
                   ? "{0} Is the old patched file or the original"
                   : "{0} Succesfully patched!",
               Path.GetFileNameWithoutExtension(file));

        }

        [DllImport(SKernel, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport(SKernel, CharSet = CharSet.Ansi, BestFitMapping = false)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string proc);

        [DllImport(SKernel, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(String dllName);
    }
}