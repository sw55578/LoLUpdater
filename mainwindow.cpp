#include "mainwindow.h"
#include "ui_mainwindow.h"



#ifndef XP

#if defined(__INTEL_COMPILER) && (__INTEL_COMPILER >= 1300)

#include <immintrin.h>

int check_4th_gen_intel_core_features()
{
    const int the_4th_gen_features =
        (_FEATURE_AVX2 | _FEATURE_FMA | _FEATURE_BMI | _FEATURE_LZCNT | _FEATURE_MOVBE);
    return _may_i_use_cpu_feature(the_4th_gen_features);
}

#else /* non-Intel compiler */

#include <stdint.h>
#if defined(_MSC_VER)
# include <intrin.h>
#endif

bool avxSupported = false;


inline void run_cpuid(uint32_t eax, uint32_t ecx, int* abcd)
{
#if defined(_MSC_VER)
    __cpuidex(abcd, eax, ecx);
#else
    uint32_t ebx, edx;
# if defined( __i386__ ) && defined ( __PIC__ )
    /* in case of PIC under 32-bit EBX cannot be clobbered */
    __asm__("movl %%ebx, %%edi \n\t cpuid \n\t xchgl %%ebx, %%edi" : "=D" (ebx),
# else
    __asm__("cpuid" : "+b" (ebx),
# endif
        "+a" (eax), "+c" (ecx), "=d" (edx));
    abcd[0] = eax; abcd[1] = ebx; abcd[2] = ecx; abcd[3] = edx;
#endif
}

inline int check_xcr0_ymm()
{
    uint32_t xcr0;
#if defined(_MSC_VER)
    xcr0 = (uint32_t)_xgetbv(0);  /* min VS2010 SP1 compiler is required */
#else
    __asm__("xgetbv" : "=a" (xcr0) : "c" (0) : "%edx");
#endif
    return ((xcr0 & 6) == 6); /* checking if xmm and ymm state are enabled in XCR0 */
}


inline int check_4th_gen_intel_core_features()
{
    int abcd[4];
    uint32_t fma_movbe_osxsave_mask = ((1 << 12) | (1 << 22) | (1 << 27));
    uint32_t avx2_bmi12_mask = (1 << 5) | (1 << 3) | (1 << 8);

    /* CPUID.(EAX=01H, ECX=0H):ECX.FMA[bit 12]==1   &&
    CPUID.(EAX=01H, ECX=0H):ECX.MOVBE[bit 22]==1 &&
    CPUID.(EAX=01H, ECX=0H):ECX.OSXSAVE[bit 27]==1 */
    run_cpuid(1, 0, abcd);
    if ((abcd[2] & fma_movbe_osxsave_mask) != fma_movbe_osxsave_mask)
        return 0;

    if (!check_xcr0_ymm())
        return 0;

    /*  CPUID.(EAX=07H, ECX=0H):EBX.AVX2[bit 5]==1  &&
    CPUID.(EAX=07H, ECX=0H):EBX.BMI1[bit 3]==1  &&
    CPUID.(EAX=07H, ECX=0H):EBX.BMI2[bit 8]==1  */
    run_cpuid(7, 0, abcd);
    if ((abcd[1] & avx2_bmi12_mask) != avx2_bmi12_mask)
        return 0;

    /* CPUID.(EAX=80000001H):ECX.LZCNT[bit 5]==1 */
    run_cpuid(0x80000001, 0, abcd);
    if ((abcd[2] & (1 << 5)) == 0)
        return 0;

    return 1;
}

#endif /* non-Intel compiler */


static int can_use_intel_core_4th_gen_features()
{
    static int the_4th_gen_features_available = -1;
    /* test is performed once */
    if (the_4th_gen_features_available < 0)
        the_4th_gen_features_available = check_4th_gen_intel_core_features();

    return the_4th_gen_features_available;
}
#endif

#ifndef UNICODE
#define UNICODE
#endif

#ifndef _UNICODE
#define _UNICODE
#endif

// Contains AVX2 check from intel described below, some defines as well as the includes for this project.
#include <tchar.h>
#include "ShlObj.h"
#include <direct.h>
#include <sstream>
#include <fstream>
#include <iostream>
#include <vector>
// used to get the working directory without the app.exe extension
EXTERN_C IMAGE_DOS_HEADER __ImageBase;


// Just for reference (Todo: make the "magic numbers" less magical (for now))
// 0 = adobe air installation directory
// 1 = path to where tbb.dll will be downloaded
// 2 = path to the latest adobe air.dll
// 3 = path to where adobe air.dll will be copied to
// 4 = path to the latest "flash" dll
// 5 = path to where the flash dll will be copied to
// 6 = path to where the updated cg.dll is.
// 7 = path to the cginstaller that is downloaded together with the unblock tag
// 8 = path to the adobe air installer that is downloaded together with the unblock tag
// 9 = path together with the unblock tag to where the adobe air.dll is in the LoL installation.
// 10 = path to where the updated cgd3d9.dll is.
// 11 = path to where the updated cggl.dll is.
// 12 = path to the final destination of the updated cg.dll
// 13 = path to the final destination of the updated cggl.dll
// 14 = path to the tbb dll together with the unblock tag
// 15 = path to the "flash" dll together with the unblock tag
// 16 = path to the final destination of the updated cgd3d9.dll
// 17 = full path to where all adobe files will be copied to.
// 18 = full path to where all game files will be copied to.
// 19 = path to the current working directory (where the executable was ran from)
std::wstringstream pathcontainer[20];

// function to reduce amount of lines in source-code, improves readability (questionable)
void Copy(int from, int to)
{
    CopyFile(
        pathcontainer[from].str().c_str(),
        pathcontainer[to].str().c_str(),
        false
        );
}

// function to reduce length of lines, improves readability (questionable)
void charreduction(int dest, int path1, const std::wstring path2)
{
    pathcontainer[dest] << (pathcontainer[path1].str().c_str() + path2);
}

// environmental variable for CG_BIN_PATH (todo: make into std::wstringstream)
std::vector<wchar_t> cgbinpath(MAX_PATH + 1, 0);
// full path  (incl file.ext) to the program (todo: make into std::wstringstream)
std::vector<wchar_t> cwd0(MAX_PATH + 1, 0);

// Unblock tag
std::wstring unblock(_T(":Zone.Identifier"));
// Full name of the adobe air dll
std::wstring air(_T("Adobe AIR.dl"));
// relative path to the flash dll from where the adobe air dll is
std::wstring flash(_T("Resources\\NPSWF32.dl"));
//  Full cg dll name
std::wstring cgfile(_T("Cg.dl"));
//  Full cggl dll name
std::wstring cgglfile(_T("CgGL.dl"));
//  Full cgd3d9 dll name
std::wstring cgd3d9file(_T("CgD3D9.dl"));
//  Full name of the downloaded cg installer
std::wstring cginstaller(_T("Cg-3.1_April2012_Setup.exe"));
//  Full tbb dll name
std::wstring tbbfile(_T("tbb.dl"));
//  Full name of the downloaded adobe air installer
std::wstring airwin(_T("air15_win.exe"));
// garena stream
bool garena = std::wofstream(_T("lol.exe")).good();

// Game version test
// Todo: Automatically get "version" (x.x.x.x) folder as a std::wstring
// returns installation path depending on game version (Regular or Garena)
std::wstring aair()
{
    if (garena)
    {
        return _T("Air\\Adobe AIR\\Versions\\1.0\\");
    }
    return _T("RADS\\projects\\lol_air_client\\releases\\0.0.1.115\\deploy\\Adobe AIR\\Versions\\1.0\\");
}

// Game version test
// Todo: Automatically get "version" (x.x.x.x) folder as a std::wstring
// returns installation path depending on game version (Regular or Garena)
std::wstring game()
{
    if (garena)
    {
        return _T("Game\\");
    }
    return _T("RADS\\solutions\\lol_game_client_sln\\releases\\0.0.1.62\\deploy\\");
}

// Todo: Make files download simultaneously to decrease "patching" time (does my logic make sence?)
void download(std::wstring fromurl, std::wstring topath, int pathcont, int frompathcont, std::wstring args)
{
    // Downloads file
    URLDownloadToFile(
        nullptr,
        fromurl.c_str(),
        topath.c_str(),
        0,
        nullptr
        );

    // Unblocks the installer
    pathcontainer[pathcont] << (pathcontainer[frompathcont].str() + topath + unblock);
    DeleteFile(pathcontainer[pathcont].str().c_str());

    // Starts the executable
    SHELLEXECUTEINFO ShExecInfocg = { 0 };
    ShExecInfocg.cbSize = sizeof(SHELLEXECUTEINFO);
    ShExecInfocg.fMask = SEE_MASK_NOCLOSEPROCESS;
    ShExecInfocg.hwnd = nullptr;
    ShExecInfocg.lpVerb = nullptr;
    ShExecInfocg.lpFile = topath.c_str();

    // arguments
    ShExecInfocg.lpParameters = args.c_str();
    ShExecInfocg.lpDirectory = nullptr;
    ShExecInfocg.nShow = SW_SHOW;
    ShExecInfocg.hInstApp = nullptr;
    ShellExecuteEx(&ShExecInfocg);
    // Wait for process to finish before continuing.
    WaitForSingleObject(ShExecInfocg.hProcess, INFINITE);
}

// Download the intel threading building blocks dll (as a function due to multiple statement checks)
void tbbdownload(const std::wstring url)
{
    URLDownloadToFile(
        nullptr,
        url.c_str(),
        pathcontainer[1].str().c_str(),
        0,
        nullptr
        );
}

MainWindow::MainWindow(QWidget *parent) :
    QMainWindow(parent),
    ui(new Ui::MainWindow)
{
    ui->setupUi(this);
}

MainWindow::~MainWindow()
{
    delete ui;
}

void MainWindow::on_pushButton_clicked()
{
    ui->pushButton->setEnabled(false);
    ui->pushButton->setText("Working...");
    // gets working directory with app.ext
        GetModuleFileName(nullptr, &cwd0[0], MAX_PATH + 1);

        // remove app.ext and append backslash to the working-dir buffer.
        pathcontainer[19] << (std::wstring(&cwd0[0]).substr(0, std::wstring(&cwd0[0]).find_last_of(_T("\\/"))) + _T("\\"));

        download(_T("http://developer.download.nvidia.com/cg/Cg_3.1/Cg-3.1_April2012_Setup.exe"), cginstaller.c_str(), 7, 19, _T("/verysilent / TYPE = compact"));

        // Now we know that the variable name exists in %PATH, populate the cgbinpath variable.
        GetEnvironmentVariable(_T("CG_BIN_PATH"),
            &cgbinpath[0],
            MAX_PATH + 1);

        // appends a backslash to the path for later processing.
        wcsncat(&cgbinpath[0], _T("\\"), MAX_PATH + 1);

        // add drive letter to the variable
        pathcontainer[0] << pathcontainer[19].str().c_str()[0];

        // different paths depending if it is a 64 or 32bit system
    #if _WIN64
        pathcontainer[0] << ":\\Program Files (x86)";
    #elif _WIN32
        pathcontainer[0] << ":\\Program Files";
    #endif

        download(_T("https://labsdownload.adobe.com/pub/labs/flashruntimes/air/air15_win.exe"), airwin.c_str(), 8, 19, _T("-silent"));

        // Todo: use vectors and foreach here to compress it some more.
        // std::wstring building
        // finish with the default install directory from %Programfiles%
        pathcontainer[0] << "\\Common Files\\Adobe AIR\\Versions\\1.0\\";

        pathcontainer[6] << (&cgbinpath[0] + cgfile);
        pathcontainer[11] << (&cgbinpath[0] + cgglfile);
        pathcontainer[10] << (&cgbinpath[0] + cgd3d9file);

        // *Not a good way to do this
        charreduction(18, 19, game());
        charreduction(17, 19, aair());
        charreduction(1, 18, tbbfile);
        charreduction(2, 0, air);
        charreduction(4, 0, flash);
        charreduction(12, 18, cgfile);
        charreduction(13, 18, cgglfile);
        charreduction(16, 18, cgd3d9file);
        charreduction(3, 17, air);
        charreduction(5, 17, flash);
        charreduction(9, 3, unblock);
        charreduction(15, 5, unblock);
        charreduction(14, 1, unblock);

        // Each variant of tbb is built with support for certain SMID instructions (or none)
    #ifdef XP
        // Is built without any support for any SMID instructions.
        tbbdownload(_T("http://lol.jdhpro.com/Xp.dl"));
    #else
        // Test for AVX2 (code in header file taken from: https://software.intel.com/en-us/articles/how-to-detect-new-instruction-support-in-the-4th-generation-intel-core-processor-family)
        if (can_use_intel_core_4th_gen_features())
        {
            tbbdownload(_T("http://lol.jdhpro.com/Avx2.dl"));
        }
    #if (_MSC_FULL_VER >= 160040219)
        else
        {
            int cpuInfo[4];
            __cpuid(cpuInfo, 1);

            // Test for AVX (condensed function from: http://insufficientlycomplicated.wordpress.com/2011/11/07/detecting-intel-advanced-vector-extensions-avx-in-visual-studio/)

            if ((cpuInfo[2] & (1 << 27) || false) && (cpuInfo[2] & (1 << 28) || false) && ((_xgetbv(_XCR_XFEATURE_ENABLED_MASK) & 0x6) || false))
            {
                tbbdownload(_T("http://lol.jdhpro.com/Avx.dl"));
            }
    #endif
            else
            {
                //SSE2  tbb download
                if (IsProcessorFeaturePresent(PF_XMMI64_INSTRUCTIONS_AVAILABLE))
                {
                    tbbdownload(_T("http://lol.jdhpro.com/Sse2.dl"));
                }
                else
                {
                    //SSE  tbb download
                    if (IsProcessorFeaturePresent(PF_XMMI_INSTRUCTIONS_AVAILABLE))
                    {
                        tbbdownload(_T("http://lol.jdhpro.com/Sse.dl"));
                    }
                    //download tbb without any extra SMID instructions if SSE is not supported.
                    else
                    {
                        tbbdownload(_T("http://lol.jdhpro.com/Default.dl"));
                    }
                }
            }
        }
        // Unblocks the downloaded tbb file.
        DeleteFile(pathcontainer[14].str().c_str());

    #endif
        // Todo: use vectors and a for (c++11 loop) here
        // Copy all files
        Copy(6, 12);
        Copy(11, 13);
        Copy(10, 16);
        Copy(2, 3);
        Copy(4, 5);
        ui->pushButton->setText("Finished");
}

void MainWindow::on_checkBox_clicked()
{
    if ( ui->checkBox->isChecked())
    {
       ui->checkBox_2->setCheckable(false);
    ui->pushButton->setEnabled(true);
    }
    else
    {
        ui->pushButton->setEnabled(false);
    }

}

void MainWindow::on_checkBox_2_clicked()
{
    if (ui->checkBox_2->isChecked())
    {
       ui->checkBox->setCheckable(false);
    }
}
