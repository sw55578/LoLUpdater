﻿using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
namespace LoLUpdater
{
    public partial class MainWindow : Window
    {
        string folderLocation = "";
        string programFiles = "";
        string releasePath = "";
        string fullReleasePath = "";
        string solutionPath = "";
        string fullSolutionPath = "";
        string airClientPath = "";
        string fullAirClientPath = "";
        string cgBinPath = "";
        string cgPath = "";
        string cgGLPath = "";
        string CgD3D9Path = "";
        string backupCG = "";
        string backupCgGL = "";
        string backupCg3D9 = "";
        string backupTbb = "";
        string backupNPSWF32 = "";
        string backupAdobeAir = "";
        string NPSWF32Install = "";
        string adobeAirInstall = "";
        string finalCG = "";
        string finalCgGL = "";
        string finalCg3D9 = "";
        string finalTbb = "";
        string finalNPSWF32 = "";
        string finalAdobeAir = "";

        public MainWindow()
        {
            Loaded += MainWindow_Loaded;
                System.Windows.Threading.DispatcherTimer pingTimer = new System.Windows.Threading.DispatcherTimer();
                pingTimer.Tick += new EventHandler(pingTimer_Tick);
                pingTimer.Interval = new TimeSpan(0, 0, 5);
                pingTimer.Start();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            if (getPing("64.7.194.1") > getPing("190.93.245.13"))
            {
                EUW.IsSelected = true;
            }

            handlePing();
        }

        private void pingTimer_Tick(object sender, EventArgs e)
        {
            handlePing();
        }

        private void AdobeAIR_Checked(object sender, RoutedEventArgs e)
        {
            adobeAlert();
        }

        private void Flash_Checked(object sender, RoutedEventArgs e)
        {
            adobeAlert();
        }

        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            populateVariableLocations();
            taskProgress.IsIndeterminate = true;

            if (Clean.IsChecked == true)
            {
                taskProgress.Tag = "Opening Disk Cleanup...";
                runCleanManager();
            }


            taskProgress.Tag = "Performing Backup...";
            if (!Directory.Exists("Backup"))
            {
                taskProgress.Tag = "Performing Backup...";
                handleBackup();
            }

            if (Patch.IsChecked == true)
            {
                taskProgress.Tag = "Patching...";
                handleParticleMultithreading();
                handlePandoUninstall();
                handleCGInstall();
                handleAdobeAndTBB();
            }
            else if (Remove.IsChecked == true)
            {
                taskProgress.Tag = "Removing Patch...";
                handleUninstall();
            }
            taskProgress.IsIndeterminate = false;
            taskProgress.Value = 100;
            taskProgress.Tag = "All Processes Completed Successfully!";
        }

        /// <summary>
        /// This method checks the game config, and sets the particle multithreading to 1.
        /// </summary>
        private void handleParticleMultithreading()
        {
            if (File.Exists(Path.Combine("Config", "game.cfg")))
            {
                if (!File.ReadAllText(Path.Combine("Config", "game.cfg")).Contains("DefaultParticleMultithreading=1"))
                {
                    File.AppendAllText(Path.Combine("Config", "game.cfg"), Environment.NewLine + "DefaultParticleMultithreading=1");
                }
            }
            else if (File.Exists(Path.Combine("Game", "DATA", "CFG", "defaults", "game.cfg")))
            {
                if (!File.ReadAllText(Path.Combine("Game", "DATA", "CFG", "defaults", "game.cfg")).Contains("DefaultParticleMultithreading=1"))
                {
                    File.AppendAllText(Path.Combine("Game", "DATA", "CFG", "defaults", "game.cfg"), Environment.NewLine + "DefaultParticleMultithreading=1");
                }
                if (File.Exists(Path.Combine("Game", "DATA", "CFG", "defaults", "game.cfg")))
                {
                    if (!File.ReadAllText(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent.cfg")).Contains("DefaultParticleMultithreading=1"))
                    {
                        File.AppendAllText(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent.cfg"), Environment.NewLine + "DefaultParticleMultithreading=1");
                    }
                }
                if (File.Exists(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_zh_MY.cfg")))
                {
                    if (!File.ReadAllText(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_zh_MY.cfg")).Contains("DefaultParticleMultithreading=1"))
                    {
                        File.AppendAllText(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_zh_MY.cfg"), Environment.NewLine + "DefaultParticleMultithreading=1");
                    }
                }
                if (File.Exists(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_en_SG.cfg")))
                {
                    if (!File.ReadAllText(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_en_SG.cfg")).Contains("DefaultParticleMultithreading=1"))
                    {
                        File.AppendAllText(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_en_SG.cfg"), Environment.NewLine + "DefaultParticleMultithreading=1");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a backup of all vital files.
        /// </summary>
        private void handleBackup()
        {
            Directory.CreateDirectory("Backup");
            if (Directory.Exists("RADS"))
            {
                if (File.Exists(Path.Combine("Config", "game.cfg")))
                {
                    File.Copy(Path.Combine("Config", "game.cfg"), Path.Combine("Backup", "game.cfg"), true);
                }
            }
            else if (Directory.Exists("Game"))
            {
                if (File.Exists(Path.Combine("Game", "DATA", "CFG", "defaults", "game.cfg")))
                {
                    File.Copy(Path.Combine("Game", "DATA", "CFG", "defaults", "game.cfg"), Path.Combine("Backup", "game.cfg"), true);
                    File.Copy(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent.cfg"), Path.Combine("Backup", "GamePermanent.cfg"), true);
                    if (File.Exists(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_zh_MY.cfg")))
                    {
                        File.Copy(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_zh_MY.cfg"), Path.Combine("Backup", "GamePermanent_zh_MY.cfg"), true);
                    }
                    if (File.Exists(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_en_SG.cfg")))
                    {
                        File.Copy(Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_én_SG.cfg"), Path.Combine("Backup", "GamePermanent_en_SG.cfg"), true);
                    }
                }
            }

            File.Copy(fullSolutionPath + finalCG, backupCG, true);
            File.Copy(fullSolutionPath + finalCg3D9, backupCg3D9, true);
            File.Copy(fullSolutionPath + finalCgGL, backupCgGL, true);
            File.Copy(fullSolutionPath + finalTbb, backupTbb, true);
            File.Copy(fullAirClientPath + finalNPSWF32, backupNPSWF32, true);
            File.Copy(fullAirClientPath + finalAdobeAir, backupAdobeAir, true);
        }

        /// <summary>
        /// This method handles the installation of adobe and tbb.
        /// </summary>
        private void handleAdobeAndTBB()
        {
            if (tbb.IsChecked == true)
            {
                File.WriteAllBytes(fullSolutionPath + finalTbb, Properties.Resources.tbb);
            }

            if (AdobeAIR.IsChecked == true)
            {
                File.Copy(adobeAirInstall, fullAirClientPath + finalAdobeAir, true);
            }

            if (Flash.IsChecked == true)
            {
                File.Copy(NPSWF32Install, fullAirClientPath + finalNPSWF32, true);
            }
        }

        /// <summary>
        /// This method checks if the user wishes to install the CG toolkit, if so
        /// it installs each option the user selects.
        /// </summary>
        private void handleCGInstall() // The error checking needs to be revised. Perhaps a variable that denotes the error if it is unresolvable, and relays it to the user.
        {
            try
            {
                if (Cg.IsChecked == true || CgGL.IsChecked == true || CgD3D9.IsChecked == true)
                {
                    if (CGCheck())
                    {
                        if (Cg.IsChecked == true)
                        {
                            File.Copy(cgPath, fullSolutionPath + finalCG, true);
                        }
                        if (Cg1.IsChecked == true)
                        {
                            File.Copy(cgPath, fullReleasePath + finalCG, true);
                        }

                        if (CgGL.IsChecked == true)
                        {
                            File.Copy(cgGLPath, fullSolutionPath + finalCgGL, true);
                        }
                        if (CgGL1.IsChecked == true)
                        {
                            File.Copy(cgGLPath, fullReleasePath + finalCgGL, true);
                        }

                        if (CgD3D9.IsChecked == true)
                        {
                            File.Copy(CgD3D9Path, fullSolutionPath + finalCg3D9, true);
                        }
                        if (CgD3D1.IsChecked == true)
                        {
                            File.Copy(CgD3D9Path, fullReleasePath + finalCg3D9, true);
                        }
                    }
                }
            }
            catch (System.IO.IOException)
            {
                var output = MessageBox.Show("Error: League of Legends is currently open. Would you like to automatically close it?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (output == MessageBoxResult.Yes)
                {
                    try
                    {
                        Process[] proc = Process.GetProcessesByName("LoLClient");
                        proc[0].Kill();
                        proc[0].WaitForExit();
                        handleCGInstall();
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        /// <summary>
        /// This method removes Pando Media Booster from the system.
        /// </summary>
        private void handlePandoUninstall()
        {
            if (File.Exists(Path.Combine(programFiles, "Pando Networks", "Media Booster", "uninst.exe")))
            {
                var PMB = new ProcessStartInfo();
                var process = new Process();
                PMB.FileName = Path.Combine(programFiles, "Pando Networks", "Media Booster", "uninst.exe");
                PMB.Arguments = "/silent";
                process.StartInfo = PMB;
                process.Start();

            }
        }

        /// <summary>
        /// This method handles the uninstall of the latest game patches.
        /// </summary>
        private void handleUninstall()
        {
            if (Directory.Exists("RADS"))
            {
                if (File.Exists(Path.Combine("Backup", "game.cfg")))
                {
                    File.Copy(Path.Combine("Backup", "game.cfg"), Path.Combine("Config", "game.cfg"), true);
                }
            }
            else if (Directory.Exists("Games"))
            {
                File.Copy(Path.Combine("Backup", "game.cfg"), Path.Combine("Game", "DATA", "CFG", "defaults", "game.cfg"), true);
                File.Copy(Path.Combine("Backup", "GamePermanent.cfg"), Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent.cfg"), true);

                if (File.Exists(Path.Combine("Backup", "GamePermanent_zh_MY.cfg")))
                {
                    File.Copy(Path.Combine("Backup", "GamePermanent_zh_MY.cfg"), Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_zh_MY.cfg"), true);
                }
                if (File.Exists(Path.Combine("Backup", "GamePermanent_en_SG.cfg")))
                {
                    File.Copy(Path.Combine("Backup", "GamePermanent_en_SG.cfg"), Path.Combine("Game", "DATA", "CFG", "defaults", "GamePermanent_en_SG.cfg"), true);
                }
            }

            File.Copy(backupCG, fullReleasePath + finalCG, true);
            File.Copy(backupCG, fullSolutionPath + finalCG, true);
            File.Copy(backupCgGL, fullReleasePath + finalCgGL, true);
            File.Copy(backupCgGL, fullSolutionPath + finalCgGL, true);
            File.Copy(backupCg3D9, fullReleasePath + finalCg3D9, true);
            File.Copy(backupCg3D9, fullSolutionPath + finalCg3D9, true);
            File.Copy(backupTbb, fullSolutionPath + finalTbb, true);
            File.Copy(backupNPSWF32, fullAirClientPath + finalNPSWF32, true);
            File.Copy(backupAdobeAir, fullAirClientPath + finalAdobeAir, true);

            Directory.Delete("Backup", true);
        }

        /// <summary>
        /// This method determines what version of the application is running, and populates
        /// all the necessary variables for later use.
        /// </summary>
        private void populateVariableLocations()
        {
            if (Environment.Is64BitProcess == true)
            {
                programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            }
            else
            {
                programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }

            if (Directory.Exists("RADS"))
            {
                folderLocation = "deploy";

                finalNPSWF32 = Path.Combine("deploy", "Adobe Air", "Versions", "1.0", "Resources", "NPSWF32.dll");
                finalAdobeAir = Path.Combine("deploy", "Adobe Air", "Versions", "1.0", "Adobe AIR.dll");

                releasePath = Path.Combine("RADS", "projects", "lol_launcher", "releases");
                fullReleasePath = releasePath + @"\" + new DirectoryInfo(releasePath).GetDirectories().OrderByDescending(d => d.CreationTime).FirstOrDefault() + @"\";
                solutionPath = Path.Combine("RADS", "solutions", "lol_game_client_sln", "releases");
                fullSolutionPath = solutionPath + @"\" + new DirectoryInfo(solutionPath).GetDirectories().OrderByDescending(d => d.CreationTime).FirstOrDefault() + @"\";
                airClientPath = Path.Combine("RADS", "projects", "lol_air_client", "releases");
                fullAirClientPath = airClientPath + @"\" + new DirectoryInfo(airClientPath).GetDirectories().OrderByDescending(d => d.CreationTime).FirstOrDefault() + @"\";
            }
            else if (Directory.Exists("Games"))
            {
                folderLocation = "Games";

                finalNPSWF32 = Path.Combine("AIR", "Adobe Air", "Versions", "1.0", "Resources", "NPSWF32.dll");
                finalAdobeAir = Path.Combine("AIR", "Adobe Air", "Versions", "1.0", "Adobe AIR.dll");
            }

            NPSWF32Install = Path.Combine(programFiles, "Common Files", "Adobe AIR", "Versions", "1.0", "Resources", "NPSWF32.dll");
            adobeAirInstall = Path.Combine(programFiles, "Common Files", "Adobe AIR", "Versions", "1.0", "Adobe AIR.dll");


            cgBinPath = Environment.GetEnvironmentVariable("CG_BIN_PATH", EnvironmentVariableTarget.User);
            if (cgBinPath != null)
            {
                cgPath = Path.Combine(cgBinPath, "cg.dll");
                cgGLPath = Path.Combine(cgBinPath, "cgGL.dll");
                CgD3D9Path = Path.Combine(cgBinPath, "cgD3D9.dll");
            }

            backupCG = Path.Combine("Backup", "cg.dll");
            backupCgGL = Path.Combine("Backup", "cgGL.dll");
            backupCg3D9 = Path.Combine("Backup", "cgD3D9.dll");
            backupTbb = Path.Combine("Backup", "tbb.dll");
            backupNPSWF32 = Path.Combine("Backup", "NPSWF32.dll");
            backupAdobeAir = Path.Combine("Backup", "Adobe AIR.dll");

            finalCG = Path.Combine(folderLocation, "cg.dll");
            finalCgGL = Path.Combine(folderLocation, "cgGL.dll");
            finalCg3D9 = Path.Combine(folderLocation, "cgD3D9.dll");
            finalTbb = Path.Combine(folderLocation, "tbb.dll");
        }

        /// <summary>
        /// This method restarts the application with elevated privaleges.
        /// </summary>
        private void restartAsAdmin()  // Closes the application and restarts as an administrator.
        {
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.UseShellExecute = true;
            proc.WorkingDirectory = Environment.CurrentDirectory;
            proc.FileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            proc.Verb = "runas";

            try
            {
                Process.Start(proc);
            }
            catch
            {
                return;
            }
            Application.Current.Shutdown();
        }

        /// <summary>
        /// This method checks for the presence of the CG toolkit, if it is not found, it is installed.
        /// </summary>
        /// <returns>True if present or installation completed successfully.</returns>
        private Boolean CGCheck()
        {
            string fileLocation = Path.Combine(programFiles, "NVIDIA Corporation", "Cg", "Bin", "cg.dll");

            if (File.Exists(fileLocation))
            {
                return true;                                                                                            // Return if found, we don't
            }                                                                                                           // need to go any further.

            try
            {
                File.WriteAllBytes("Cg-3.1 April2012 Setup.exe", Properties.Resources.Cg_3_1_April2012_Setup);              // Extract and write the file
                Process cg = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "Cg-3.1 April2012 Setup.exe";
                startInfo.Arguments = "/silent";
                cg.StartInfo = startInfo;
                cg.Start();
                cg.WaitForExit();
                File.Delete("Cg-3.1 April2012 Setup.exe");
                populateVariableLocations();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Prompts user to install adobe product.
        /// </summary>
        // TODO: Add error checking to see if adobe product is already installed.
        private void adobeAlert()
        {
            MessageBoxResult alertMessage = MessageBox.Show("We are unable to include any Adobe products, " +
                "HOWEVER, you are fully capable of installing it yourself. Click yes to download and run the" +
                " installer then apply the patch.", "LoLUpdater", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (alertMessage == MessageBoxResult.Yes)
            {
                Process.Start("http://labsdownload.adobe.com/pub/labs/flashruntimes/air/air14_win.exe");
            }
        }


        private void runCleanManager()
        {
            var cm = new ProcessStartInfo();
            var process = new Process();
            cm.FileName = "cleanmgr.exe";
            cm.Arguments = "sagerun:1";
            process.StartInfo = cm;
            process.Start();
        }

        private void handleMouseHz()
        {
            RegistryKey mousehz;

            if (Environment.Is64BitProcess)
            {
                mousehz = Registry.LocalMachine.CreateSubKey(Path.Combine("SOFTWARE", "Microsoft", "Windows NT",
                    "CurrentVersion", "AppCompatFlags", "Layers"));
            }
            else
            {
                mousehz = Registry.LocalMachine.CreateSubKey(Path.Combine("SOFTWARE", "WoW64Node", "Microsoft",
                    "Windows NT", "CurrentVersion", "AppCompatFlags", "Layers"));
            }

            mousehz.SetValue("NoDTToDITMouseBatch", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "explorer.exe"), RegistryValueKind.String);
            var cmd = new ProcessStartInfo();
            var process = new Process();
            cmd.FileName = "cmd.exe";
            cmd.Verb = "runas";
            cmd.Arguments = "/C Rundll32 apphelp.dll,ShimFlushCache";
            process.StartInfo = cmd;
            process.Start();
        }


        private void handleRiotLogs()
        {
            if (Directory.Exists("RADS"))
            {
                string[] files = Directory.GetFiles("Logs");

                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.LastAccessTime < DateTime.Now.AddDays(-7))
                        fi.Delete();
                }
            }
        }

        private void handlePing()
        {
            if (OCE.IsSelected)
            {
                Label.Content = getPing("oce.leagueoflegends.com");
            }
            if (LAN.IsSelected)
            {
                Label.Content = getPing("lan.leagueoflegends.com");
            }
            if (RU.IsSelected)
            {
                Label.Content = getPing("ru.leagueoflegends.com");
            }
            if (TR.IsSelected)
            {
                Label.Content = getPing("tr.leagueoflegends.com");
            }
            if (EUNE.IsSelected)
            {
                Label.Content = getPing("eune.leagueoflegends.com");
            }
            if (BR.IsSelected)
            {
                Label.Content = getPing("br.leagueoflegends.com");
            }
            if (GarenaPH.IsSelected)
            {
                Label.Content = getPing("garena.ph");
            }
            if (GarenaSingapore.IsSelected)
            { Label.Content = getPing("lol.garena.com"); }

            if (NA.IsSelected)
            {
                Label.Content = getPing("64.7.194.1");
            }
            else if (EUW.IsSelected)
            {
                Label.Content = getPing("190.93.245.13");
            }
        }
        private long getPing(string ip)
        {
            Ping ping = new Ping();
            PingReply reply = ping.Send(ip);

            return reply.RoundtripTime;
        }
        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            handlePing();
        }

        private void chkOption_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var chkBox = (CheckBox)sender;
            switch (chkBox.Name)
            {
                case "AdobeAIR":
                    lblDescription.Text = "Provides you a link to the Adobe AIR redistributable so you can install it before patching. This upgrades the PvP.NET client";
                    break;
                case "Flash":
                    lblDescription.Text = "Provides you a link to the Adobe AIR redistributable so you can install it before patching. This upgrades the built in Flash player in the Air Client";
                    break;
                case "Cg":
                    lblDescription.Text = "Installs one of the DLLs from the Nvidia CG toolkit, yes you need it even if you are on ATI/Intel. This modifies the shader.";
                    break;
                case "CgD3D9":
                    lblDescription.Text = "Installs one of the DLLs from the Nvidia CG toolkit, yes you need it even if you are on ATI/Intel. This modifies the shader.";
                    break;
                case "CgGL":
                    lblDescription.Text = "Installs one of the DLLs from the Nvidia CG toolkit, yes you need it even if you are on ATI/Intel. This modifies the shader.";
                    break;
                case "tbb":
                    lblDescription.Text = "Installs a custom lightweight tbb.dll file that increases the fps of the game, This makes multiprocessing available for LoL";
                    break;
                case "Clean":
                    lblDescription.Text = "Do a quick clean of the harddrive using the Windows automatic disk cleanup manager";
                    break;
                case "Cg1":
                    lblDescription.Text = "Installs one of the DLLs from the Nvidia CG toolkit, yes you need it even if you are on ATI/Intel. This modifies the shader.";
                    break;
                case "CgD3D1":
                    lblDescription.Text = "Installs one of the DLLs from the Nvidia CG toolkit, yes you need it even if you are on ATI/Intel. This modifies the shader.";
                    break;
                case "CgGL1":
                    lblDescription.Text = "Installs one of the DLLs from the Nvidia CG toolkit, yes you need it even if you are on ATI/Intel. This modifies the shader.";
                    break;
            }
        }

        private void chkOption_MouseExit(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lblDescription.Text = "";
        }



    }
}