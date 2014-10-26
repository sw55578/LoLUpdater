
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
#include <fstream>
#include "Windows.h"
#include <direct.h>

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
	xcr0 = (uint32_t)_xgetbv(0); /* min VS2010 SP1 compiler is required */
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


#if _WIN32 || _WIN64
#if _WIN64
#define ENVIRONMENT64
#else
#define ENVIRONMENT32
#endif
#else
#if __x86_64__ || __ppc64__
#define ENVIRONMENT64
#else
#define ENVIRONMENT32
#endif
#endif

inline bool is_file_exist(const char *fileName)
{
	std::ifstream infile(fileName);
	return infile.good();
}

#undef CONTEXT_XSTATE

#if defined(_M_X64)
#define CONTEXT_XSTATE                      (0x00100040)
#else
#define CONTEXT_XSTATE                      (0x00010040)
#endif


#define XSTATE_MASK_AVX                     (XSTATE_MASK_GSSE)

typedef DWORD64(WINAPI *PGETENABLEDXSTATEFEATURES)();
HMODULE hm = GetModuleHandleA("kernel32.dll");

PGETENABLEDXSTATEFEATURES pfnGetEnabledXStateFeatures = (PGETENABLEDXSTATEFEATURES)GetProcAddress(hm, "GetEnabledXStateFeatures");
DWORD64 FeatureMask = pfnGetEnabledXStateFeatures();

char* unblock = ":Zone.Identifier";
char* workingdirbuffer = nullptr;
char* workingdir = _getcwd(
	workingdirbuffer,
	260
	);

// Buffers
char tbb[MAX_PATH];
char airfile[MAX_PATH];
char airdir[MAX_PATH];
char flashfile[MAX_PATH];
char flashdir[MAX_PATH];
char cgbin[MAX_PATH];
char cginstunblock[MAX_PATH];
char strair[MAX_PATH];
char airunblock[MAX_PATH];
char cgd3d9bin[MAX_PATH];
char cgglbin[MAX_PATH];
char cgpath[MAX_PATH];
char cgglpath[MAX_PATH];
char tbbunblock[MAX_PATH];
char flashunblock[MAX_PATH];
char cgd3d9path[MAX_PATH];
char cgglunblock[MAX_PATH];
char cgunblock[MAX_PATH];
char cgd3d9unblock[MAX_PATH];

// Variables
char* flash = "Resources\\NPSWF32.dll";
char* cgfile = "\\Cg.dll";
char* cgglfile = "\\CgGL.dll";
char* cgd3d9file = "\\CgD3D9.dll";
char* cginstaller = "Cg-3.1_April2012_Setup.exe";
char* cgbinpath = getenv("CG_BIN_PATH");
char* airwin = "air15_win.exe";
char* slnpath = "\\RADS\\solutions\\lol_game_client_sln\\releases\\0.0.1.62\\deploy";
char* game = "\\Game";
char* tbbfile = "\\tbb.dll";
char* adobepath = "\\Common Files\\Adobe AIR\\Versions\\1.0\\";
char* gair = "\\Air\\Adobe AIR\\Versions\\1.0\\";
char* airpath = "\\RADS\\projects\\lol_air_client\\releases\\0.0.1.115\\deploy\\Adobe AIR\\Versions\\1.0\\";
char* air = "Adobe AIR.dll";
char* unblockfiles[] = { cgunblock, cgglunblock, cgd3d9unblock, tbbunblock, airunblock, flashunblock };
