﻿using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LoLUpdater
{
    internal static class Program
    {
        private static readonly string Sln = Version("solutions", "lol_game_client_sln");
        private static readonly string Air = Version("projects", "lol_air_client");
        private static readonly string[] LoLProccessStrings = { "LoLClient", "LoLLauncher", "LoLPatcher", "League of Legends" };

        private static Uri _tbbVersionUri;

        private static readonly Uri FinaltbbVersionUri = Uri.TryCreate(new Uri("https://github.com/Loggan08/LoLUpdater/raw/master/Resources/"),
                HighestSupportedInstruction(), out _tbbVersionUri) ? _tbbVersionUri : new Uri(string.Empty);

        private static readonly Uri FlashUri =
            new Uri("https://github.com/Loggan08/LoLUpdater/raw/master/Resources/NPSWF32.dll");

        private static readonly Uri AirUri =
    new Uri("https://github.com/Loggan08/LoLUpdater/raw/master/Resources/Adobe AIR.dll");

        private static readonly Version CgLatestVersion = new Version("3.1.13");

        private static readonly string PmbUninstall = Path.Combine(Environment.Is64BitProcess
            ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "Pando Networks", "Media Booster", "uninst.exe");

        private static readonly bool IsMultiCore = new ManagementObjectSearcher("Select * from Win32_Processor").Get()
            .Cast<ManagementBaseObject>()
            .Sum(item => ToInt(item["NumberOfCores"].ToString())) > 1;

        private static readonly bool IsAvx2 = new ManagementObjectSearcher("Select * from Win32_Processor").Get()
            .Cast<ManagementBaseObject>()
            .Any(item => item["Name"].ToString().Contains("Haswell")) || new ManagementObjectSearcher("Select * from Win32_Processor").Get()
            .Cast<ManagementBaseObject>()
            .Any(item => item["Name"].ToString().Contains("Broadwell")) || new ManagementObjectSearcher("Select * from Win32_Processor").Get()
            .Cast<ManagementBaseObject>()
            .Any(item => item["Name"].ToString().Contains("Skylake")) || new ManagementObjectSearcher("Select * from Win32_Processor").Get()
            .Cast<ManagementBaseObject>()
            .Any(item => item["Name"].ToString().Contains("Cannonlake"));

        private static string _cgBinPath = Environment.GetEnvironmentVariable("CG_BIN_PATH",
            EnvironmentVariableTarget.User);

        private static void Main()
        {
            if (File.Exists("LoLUpdater.txt"))
            {
                RemoveReadOnly("LoLUpdater.txt", string.Empty, string.Empty, string.Empty);
            }
            if (File.Exists("LoLUpdater Updater.exe"))
            { Unblock("LoLUpdater Updater.exe", string.Empty, string.Empty, string.Empty); }
            if (File.Exists("LoLUpdater Uninstall.exe"))
            { Unblock("LoLUpdater Uninstall.exe", string.Empty, string.Empty, string.Empty); }
            if (File.Exists(PmbUninstall))
            {
                Kill("PMB"); Process.Start(new ProcessStartInfo { FileName = PmbUninstall, Arguments = "/silent" });
            }
            Console.WriteLine("Patching...");
            Console.WriteLine("");
            Kill(LoLProccessStrings);
            using (WebClient webClient = new WebClient())
            {
                CgCheck(webClient);
                if (!Directory.Exists("Backup"))
                {
                    Directory.CreateDirectory("Backup");
                    if (Directory.Exists("RADS"))
                    {
                        CopyToBak("solutions", "lol_game_client_sln", "cg.dll", Sln);
                        CopyToBak("solutions", "lol_game_client_sln", "cgD3D9.dll", Sln);
                        CopyToBak("solutions", "lol_game_client_sln", "cgGL.dll", Sln);
                        CopyToBak("solutions", "lol_game_client_sln", "tbb.dll", Sln);
                        CopyToBak("projects", "lol_air_client",
                            Path.Combine("Adobe Air", "Versions", "1.0", "Adobe AIR.dll"), Air);
                        CopyToBak("projects", "lol_air_client",
                            Path.Combine("Adobe Air", "Versions", "1.0", "Resources", "NPSWF32.dll"), Air);
                        if (!File.Exists(Path.Combine("Config", "game.cfg"))) return;
                        Copy("game.cfg", "Config", "Backup");
                    }
                    else
                    {
                        Copy("cg.dll", "Game", "Backup");
                        Copy("cgGL.dll", "Game", "Backup");
                        Copy("cgD3D9.dll", "Game", "Backup");
                        Copy("tbb.dll", "Game", "Backup");
                        Copy("Adobe AIR.dll", Path.Combine("Air", "Adobe AIR", "Versions", "1.0"), "Backup");
                        Copy("NPSWF32.dll", Path.Combine("Air", "Adobe AIR", "Versions", "1.0", "Resources"),
                            "Backup");
                        Copy(Path.Combine("Game", "DATA", "CFG", "defaults", "game.cfg"), "Backup", "game.cfg");
                        Copy(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent.cfg"), "Backup", "GamePermanent.cfg");
                        Copy(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_zh_MY.cfg"), "Backup", "GamePermanent_zh_MY.cfg");
                        Copy(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_en_SG.cfg"), "Backup", "GamePermanent_en_SG.cfg");
                    }
                }

                if (Directory.Exists("RADS"))
                {
                    RemoveReadOnly(Path.Combine("Config", "game.cfg"), string.Empty, string.Empty, string.Empty);
                    RemoveReadOnly("solutions", "lol_game_client_sln", "tbb.dll", Sln);
                    RemoveReadOnly("solutions", "lol_game_client_sln", "cg.dll", Sln);
                    RemoveReadOnly("solutions", "lol_game_client_sln", "cgGl.dll", Sln);
                    RemoveReadOnly("solutions", "lol_game_client_sln", "cgD3D9.dll", Sln);
                    RemoveReadOnly("projects", "lol_air_client",
                        Path.Combine("Air", "Adobe AIR", "Versions", "1.0", "Resources", "NPSWF32.dll"), Air);
                    RemoveReadOnly("projects", "lol_air_client",
                        Path.Combine("Air", "Adobe AIR", "Versions", "1.0", "Adobe AIR.dll"), Air);

                    if (IsMultiCore)
                    {
                        if (File.Exists(Path.Combine("Config", "game.cfg")))
                        {
                            Cfg("game.cfg", "Config", true);
                        }
                    }
                    if (!IsMultiCore)
                    {
                        if (File.Exists(Path.Combine("Config", "game.cfg")))
                        {
                            Cfg("game.cfg", "Config", false);
                        }
                    }
                    webClient.DownloadFile(
                        FinaltbbVersionUri,
                        DirPath("solutions", "lol_game_client_sln", Sln, "tbb.dll"));

                    webClient.DownloadFile(
                        FlashUri,
                        DirPath("projects", "lol_air_client", Air, Path.Combine("Adobe Air", "Versions", "1.0", "Resources", "NPSWF32.dll")));

                    webClient.DownloadFile(
                        AirUri,
                        DirPath("projects", "lol_air_client", Air,
                            Path.Combine("Adobe Air", "Versions", "1.0", "Adobe AIR.dll")));

                    CopyCg(
                        "cg.dll", "solutions", "lol_game_client_sln", Sln);
                    CopyCg(
                        "cgD3D9.dll", "solutions", "lol_game_client_sln", Sln);
                    Unblock("solutions", "lol_game_client_sln", "tbb.dll", Sln);
                    Unblock("solutions", "lol_game_client_sln", "cg.dll", Sln);
                    Unblock("solutions", "lol_game_client_sln", "cgGL.dll", Sln);
                    Unblock("solutions", "lol_game_client_sln", "cgD3D9.dll", Sln);
                    Unblock("projects", "lol_air_client",
                        Path.Combine("Air", "Adobe AIR", "Versions", "1.0", "Adobe AIR.dll"), Air);
                    Unblock("projects", "lol_air_client",
                        Path.Combine("Air", "Adobe AIR", "Versions", "1.0", "Resources", "NPSWF32.dll"), Air);
                    Unblock(Path.Combine("Config", "game.cfg"), string.Empty, string.Empty, string.Empty);
                    using (var md5 = MD5.Create())
                    {
                        File.WriteAllText("LoLUpdater.txt", string.Format("// MD5 Hashes used to find updates (DO NOT MODIFY) {2} tbb {0}: {1} {2} air: {3} {2} flash: {4}", HighestSupportedInstruction(), md5.ComputeHash(File.OpenRead(DirPath("solutions", "lol_game_client_sln", Sln, "tbb.dll"))), Environment.NewLine, md5.ComputeHash(File.OpenRead(DirPath("projects", "lol_air_client", Air, Path.Combine("Air", "Adobe AIR", "Versions", "1.0", "Adobe AIR.dll")))), md5.ComputeHash(File.OpenRead(DirPath("projects", "lol_air_client", Air, Path.Combine("Air", "Adobe AIR", "Versions", "1.0", "Resources", "NPSWF32.dll"))))));
                        File.SetAttributes("LoLUpdater.txt",
                        FileAttributes.ReadOnly);
                    }
                }
                else
                {
                    RemoveReadOnly(Path.Combine("Game", "DATA", "CFG", "defaults", "game.cfg"), string.Empty, string.Empty, string.Empty);
                    RemoveReadOnly(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent.cfg"), string.Empty, string.Empty, string.Empty);
                    RemoveReadOnly(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_zh_MY.cfg"), string.Empty, string.Empty, string.Empty);
                    RemoveReadOnly(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_en_SG.cfg"), string.Empty, string.Empty, string.Empty);
                    RemoveReadOnly(Path.Combine("Game", "tbb.dll"), string.Empty, string.Empty, string.Empty);
                    RemoveReadOnly(Path.Combine("Game", "cg.dll"), string.Empty, string.Empty, string.Empty);
                    RemoveReadOnly(Path.Combine("Game", "cgGL.dll"), string.Empty, string.Empty, string.Empty);
                    RemoveReadOnly(Path.Combine("Game", "cgD3D9.dll"), string.Empty, string.Empty, string.Empty);
                    RemoveReadOnly(Path.Combine("Air", "Adobe Air", "Versions", "1.0", "Resources", "NPSWF32.dll"), string.Empty, string.Empty, string.Empty);
                    RemoveReadOnly(Path.Combine("Air", "Adobe Air", "Versions", "1.0", "Adobe AIR.dll"), string.Empty, string.Empty, string.Empty);

                    if (IsMultiCore)
                    {
                        if (File.Exists(Path.Combine("Game", "DATA", "CFG", "defaults", "game.cfg")))
                        {
                            Cfg("game.cfg", Path.Combine("Game", "DATA", "CFG", "defaults"), true);
                        }
                        if (File.Exists(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent.cfg")))
                        {
                            Cfg("GamePermanent.cfg", Path.Combine("Game", "DATA", "CFG", "defaults"), true);
                        }
                        if (File.Exists(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_zh_MY.cfg")))
                        {
                            Cfg("GamePermanent_zh_MY.cfg",
                                Path.Combine("Game", "DATA", "CFG", "defaults"), true);
                        }
                        if (File.Exists(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_en_SG.cfg")))
                        { Cfg("GamePermanent_en_SG.cfg", Path.Combine("Game", "DATA", "CFG", "defaults"), true); }
                    }
                    if (!IsMultiCore)
                    {
                        if (File.Exists(Path.Combine("Game", "DATA", "CFG", "defaults", "game.cfg")))
                        {
                            Cfg("game.cfg", Path.Combine("Game", "DATA", "CFG", "defaults"), false);
                        }
                        if (File.Exists(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent.cfg")))
                        {
                            Cfg("GamePermanent.cfg", Path.Combine("Game", "DATA", "CFG", "defaults"), false);
                        }
                        if (File.Exists(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_zh_MY.cfg")))
                        {
                            Cfg("GamePermanent_zh_MY.cfg",
                                Path.Combine("Game", "DATA", "CFG", "defaults"), false);
                        }
                        if (File.Exists(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_en_SG.cfg")))
                        { Cfg("GamePermanent_en_SG.cfg", Path.Combine("Game", "DATA", "CFG", "defaults"), false); }
                    }
                    Copy("cg.dll",
                        _cgBinPath,
                        "Game");
                    Copy("cgGL.dll",
                        _cgBinPath,
                        "Game");
                    Copy("cgD3D9.dll", _cgBinPath, "Game");

                    webClient.DownloadFile(
                        FlashUri,
                        Path.Combine("Air", "Adobe Air", "Versions", "1.0", "Resources", "NPSWF32.dll"));

                    webClient.DownloadFile(
                        AirUri,
                        Path.Combine("Air", "Adobe Air", "Versions", "1.0", "Adobe AIR.dll"));

                    webClient.DownloadFile(
                        FinaltbbVersionUri,
                        Path.Combine("Game", "tbb.dll"));

                    Unblock(Path.Combine("Game", "DATA", "CFG", "defaults", "game.cfg"), string.Empty, string.Empty, string.Empty);
                    Unblock(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent.cfg"), string.Empty, string.Empty, string.Empty);
                    Unblock(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_zh_MY.cfg"), string.Empty, string.Empty, string.Empty);
                    Unblock(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_en_SG.cfg"), string.Empty, string.Empty, string.Empty);
                    Unblock(Path.Combine("Game", "tbb.dll"), string.Empty, string.Empty, string.Empty);
                    Unblock(Path.Combine("Game", "cg.dll"), string.Empty, string.Empty, string.Empty);
                    Unblock(Path.Combine("Game", "cgGl.dll"), string.Empty, string.Empty, string.Empty);
                    Unblock(Path.Combine("Game", "cgD3D9.dll"), string.Empty, string.Empty, string.Empty);
                    Unblock(Path.Combine("Air", "Adobe Air", "Versions", "1.0", "Resources", "NPSWF32.dll"), string.Empty, string.Empty, string.Empty);
                    Unblock(Path.Combine("Air", "Adobe Air", "Versions", "1.0", "Adobe AIR.dll"), string.Empty, string.Empty, string.Empty);
                }
                Console.WriteLine("Done!");
                Console.ReadLine();
                if (File.Exists("lol_launcher.exe"))
                {
                    Process.Start("lol_launcher.exe");
                }
                Environment.Exit(0);
            }
        }

        private static void Unblock(string file, string path1, string path2, string ver)
        {
            if (File.Exists(DirPath(path1, path2, ver, file)))
            {
                DeleteFile(DirPath(path1, path2, ver, file) + ":Zone.Identifier");
            }
            if (File.Exists(file))
            { DeleteFile(file + ":Zone.Identifier"); }
        }

        private static void RemoveReadOnly(string file, string path1, string path2, string ver)
        {
            if (File.Exists(DirPath(path1, path2,
                ver, file)) &&
                new FileInfo(DirPath(path1, path2,
                    file, ver)).Attributes
                    .Equals(FileAttributes.ReadOnly))
            {
                File.SetAttributes(DirPath(path1, path2,
                    ver, file),
                    FileAttributes.Normal);
            }
            if (File.Exists(file) &&
    new FileInfo(file).Attributes
        .Equals(FileAttributes.ReadOnly))
            {
                File.SetAttributes(file,
                    FileAttributes.Normal);
            }
        }

        private static void CgCheck(WebClient webClient)
        {
            if (!string.IsNullOrEmpty(_cgBinPath) && new Version(
                FileVersionInfo.GetVersionInfo(Path.Combine(_cgBinPath, "cg.dll")).FileVersion) >= CgLatestVersion)
                return;
            webClient.DownloadFile(new Uri("https://github.com/Loggan08/LoLUpdater/raw/master/Resources/Cg-3.1_April2012_Setup.exe"), "Cg-3.1_April2012_Setup.exe");
            Unblock("Cg-3.1_April2012_Setup.exe", string.Empty, string.Empty, string.Empty);

            Process cg = new Process
            {
                StartInfo =
                    new ProcessStartInfo
                    {
                        FileName =
                            "Cg-3.1_April2012_Setup.exe",
                        Arguments = "/silent /TYPE=compact"
                    }
            };
            cg.Start();
            cg.WaitForExit();
            File.Delete("Cg-3.1_April2012_Setup.exe");
            _cgBinPath = Environment.GetEnvironmentVariable("CG_BIN_PATH",
                EnvironmentVariableTarget.User);
        }

        private static void Kill(IEnumerable process)
        {
            if (IsMultiCore)
            {
                Parallel.ForEach(Process.GetProcessesByName(process.ToString()), proc =>
                {
                    proc.Kill();
                    proc.WaitForExit();
                });
            }
            else
            {
                foreach (Process proc in Process.GetProcessesByName(process.ToString()))
                {
                    proc.Kill();
                    proc.WaitForExit();
                }
            }
        }

        private static void CopyToBak(string folder, string folder1, string file, string version)
        {
            if (File.Exists(Path.Combine("RADS", folder, folder1, "releases", version, "deploy", file)))
            {
                File.Copy(
                    Path.Combine("RADS", folder, folder1, "releases", version, "deploy", file)
                    , Path.Combine("Backup", file),
                    true);
            }
        }

        private static string DirPath(string folder, string folder1, string version, string file)
        {
            return Path.Combine("RADS", folder, folder1, "releases", version, "deploy", file);
        }

        private static void CopyCg(string file, string folder, string folder1, string version)
        {
            if (File.Exists(Path.Combine(
                  _cgBinPath, file)))
            {
                File.Copy(
                 Path.Combine(
                     _cgBinPath, file),
                 Path.Combine("RADS", folder, folder1, "releases", version, "deploy", file), true);
            }
        }

        private static void Copy(string file, string from, string to)
        {
            if (File.Exists(Path.Combine(from, file)) && Directory.Exists(to))
            {
                File.Copy(Path.Combine(from, file),
                    Path.Combine(to, file), true);
            }
        }

        private static void Cfg(string file, string path, bool mode)
        {
            if (!File.Exists(Path.Combine(path, file))) return;
            if (mode)
            {
                if (File.ReadAllText(Path.Combine(path, file))
                    .Contains("DefaultParticleMultiThreading=1")) return;
                File.AppendAllText(Path.Combine(path, file),
                    string.Format("{0}{1}", Environment.NewLine, "DefaultParticleMultiThreading=1"));
            }
            else
            {
                var oldLines = File.ReadAllLines(Path.Combine(path, file));
                if (!oldLines.Contains("DefaultParticleMultiThreading=1")) return;
                var newLines = oldLines.Select(line => new { Line = line, Words = line.Split(' ') }).Where(lineInfo => !lineInfo.Words.Contains("DefaultParticleMultiThreading=1")).Select(lineInfo => lineInfo.Line);
                File.WriteAllLines(Path.Combine(path, file), newLines);
            }
        }

        private static void Md5(string hash)
        {
            var oldLines = File.ReadAllLines("LoLUpdater.txt");
            if (oldLines.Contains(hash)) return;
            var newLines = oldLines.Select(line => new { Line = line, Words = line.Split(' ') }).Where(lineInfo => !lineInfo.Words.Contains("DefaultParticleMultiThreading=1")).Select(lineInfo => lineInfo.Line);
            File.WriteAllLines("LoLUpdater.txt", newLines);
        }

        private static string Version(string folder, string folder1)
        {
            return Directory.Exists("RADS") ? Path.GetFileName(Directory.GetDirectories(Path.Combine("RADS", folder, folder1, "releases")).Max()) : string.Empty;
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsProcessorFeaturePresent(uint processorFeature);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern void DeleteFile(string fileName);

        private static string HighestSupportedInstruction()
        {
            if (Environment.Is64BitProcess && Environment.OSVersion.Version.Major >= 6 && IsAvx2)
            { return "AVX2.dll"; }
            if (IsMultiCore)
            {
                if (Environment.Is64BitProcess && Environment.OSVersion.Version.Major >= 6 && IsProcessorFeaturePresent(17))
                {
                    return "AVX.dll";
                }
                if (IsProcessorFeaturePresent(10))
                {
                    return "SSE2.dll";
                }
                return IsProcessorFeaturePresent(6) ? "SSE.dll" : "tbb.dll";
            }
            if (IsProcessorFeaturePresent(10))
            {
                return "SSE2ST.dll";
            }

            return IsProcessorFeaturePresent(6) ? "SSEST.dll" : "tbbST.dll";
        }

        private static int ToInt(string value)
        {
            int result;
            int.TryParse(value, out result);
            return result;
        }
    }
}