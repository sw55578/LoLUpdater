// LoLUpdater.cpp : Defines the entry point for the console application.
//
#include <stdio.h>
#include "ShlObj.h"
#include <VersionHelpers.h>
#include "stdafx.h"
#include "Urlmon.h"
#include "Windows.h"
#include "stdlib.h"
#include <string.h>

int _tmain(int argc, _TCHAR* argv[])
{

#if _WIN32 || _WIN64
#if _WIN64
#define ENVIRONMENT64
#else
#define ENVIRONMENT32
#endif
#endif
	char* cgbinpath
	if (cgbinpath = NULL)
	{
		URLDownloadToFile(
			NULL,
			L"http://developer.download.nvidia.com/cg/Cg_3.1/Cg-3.1_April2012_Setup.exe",
			L"Cg-3.1_April2012_Setup.exe",
			0,
			NULL
			);
		DeleteFile(L"Cg - 3.1_April2012_Setup.exe:Zone.Identifier");
		SHELLEXECUTEINFO shExecInfo;
		shExecInfo.cbSize = sizeof(SHELLEXECUTEINFO);
		shExecInfo.fMask = NULL;
		shExecInfo.hwnd = NULL;
		shExecInfo.lpFile = L"Cg-3.1_April2012_Setup.exe";
		shExecInfo.lpParameters = L"/verysilent";
		shExecInfo.lpDirectory = NULL;
		shExecInfo.nShow = SW_NORMAL;
		shExecInfo.hInstApp = NULL;
		WaitForSingleObject(shExecInfo.shExecInfo, INFINITE);
		char* cgbinpath = getenv("CG_BIN_PATH");
		CopyFile(
			strcat(cgbinpath, "\cg.dll"),
			L"RADS/solutions/lol_game_client_sln/releases/0.0.1.62/deploy/Cg.dll",
			false
			);
		CopyFile(
			strcat(cgbinpath, "\cgGL.dll"),
			L"RADS/solutions/lol_game_client_sln/releases/0.0.1.62/deploy/CgGL.dll",
			false
			);
		CopyFile(
			strcat(cgbinpath, "\cgGL.dll"),
			L"RADS/solutions/lol_game_client_sln/releases/0.0.1.62/deploy/CgD3D9.dll",
			false
			);
		DeleteFile(L"RADS/solutions/lol_game_client_sln/releases/0.0.1.62/deploy/CgD3D9.dll:Zone.Identifier");
		DeleteFile(L"RADS/solutions/lol_game_client_sln/releases/0.0.1.62/deploy/Cg.dll:Zone.Identifier");
		DeleteFile(L"RADS/solutions/lol_game_client_sln/releases/0.0.1.62/deploy/CgGL.dll:Zone.Identifier");
	};
	URLDownloadToFile(
		NULL,
		L"https://labsdownload.adobe.com/pub/labs/flashruntimes/air/air15_win.exe",
		L"air15_win.exe",
		0,
		NULL
		);
	DeleteFile(L"air15_win.exe:Zone.Identifier");
	SHELLEXECUTEINFO shExecInfo;
	shExecInfo.cbSize = sizeof(SHELLEXECUTEINFO);
	shExecInfo.fMask = NULL;
	shExecInfo.hwnd = NULL;
	shExecInfo.lpFile = L"air15_win.exe";
	shExecInfo.lpParameters = L"-silent";
	shExecInfo.lpDirectory = NULL;
	shExecInfo.nShow = SW_NORMAL;
	shExecInfo.hInstApp = NULL;
	WaitForSingleObject(ShExecInfo.shExecInfo, INFINITE);

	TCHAR szPath[MAX_PATH];
	wchar_t *
	if (ENIRONMENT32)
	{
		CopyFile(

			SHGetFolderPath(NULL,
			CSIDL_PROGRAM_FILESX86,
			NULL,
			0,
			szPath) + "\Common Files\Adobe AIR\Versions\1.0\Adobe AIR.dll",
			L"RADS\projects\lol_air_client\releases\0.0.1.114\deploy\Adobe AIR\versions\1.0\Adobe AIR.dll",
			false
			);
		TCHAR szPath[MAX_PATH];
		char*
		CopyFile(

			SHGetFolderPath(NULL,
			CSIDL_PROGRAM_FILESX86,
			NULL,
			0,
			szPath) + "\Common Files\Adobe AIR\Versions\1.0\Resources\NPSWF32.dll",
			L"RADS\projects\lol_air_client\releases\0.0.1.114\deploy\Adobe AIR\versions\1.0\Resources\NPSWF32.dll",
			false
			);

		;
	}
	else
	{

		CopyFile(
			SHGetFolderPath(NULL,
			CSIDL_PROGRAM_FILESX86,
			NULL,
			0,
			szPath) + "\Common Files\Adobe AIR\Versions\1.0\Adobe AIR.dll",
			L"RADS\projects\lol_air_client\releases\0.0.1.114\deploy\Adobe AIR\versions\1.0\Adobe AIR.dll",
			false
			);
		CopyFile(
			SHGetFolderPath(NULL,
			CSIDL_PROGRAM_FILESX86,
			NULL,
			0,
			szPath) + "\Common Files\Adobe AIR\Versions\1.0\Resources\NPSWF32.dll",
			L"RADS\projects\lol_air_client\releases\0.0.1.114\deploy\Adobe AIR\versions\1.0\Resources\NPSWF32.dll",
			false
			);
		
	}
	OSVERSIONINFO	vi;

	memset(&vi, 0, sizeof vi);
	vi.dwOSVersionInfoSize = sizeof vi;
	GetVersionEx(&vi);
	if (vi.dwPlatformId == VER_PLATFORM_WIN32_NT  &&  vi.dwMajorVersion = < )
	{
		URLDownloadToFile(
			NULL,
			L"https://github.com/Loggan08/LoLUpdater/raw/master/Tbb/XP.dll",
			L"RADS/solutions/lol_game_client_sln/releases/0.0.1.62/deploy/tbb.dll",
			0,
			NULL
			);
	}


	if (can_use_intel_core_4th_gen_features())
	{

		URLDownloadToFile(
			NULL,
			L"https://github.com/Loggan08/LoLUpdater/raw/master/Tbb/AVX2.dll",
			L"RADS/solutions/lol_game_client_sln/releases/0.0.1.62/deploy/tbb.dll",
			0,
			NULL
			);
	}
	if (isAvxSupported())
	{
		URLDownloadToFile(
			NULL,
			L"https://github.com/Loggan08/LoLUpdater/raw/master/Tbb/AVX.dll",
			L"RADS/solutions/lol_game_client_sln/releases/0.0.1.62/deploy/tbb.dll",
			0,
			NULL
			);
	}
	if (::IsProcessorFeaturePresent(PF_XMMI64_INSTRUCTIONS_AVAILABLE))
	{
		URLDownloadToFile(
			NULL,
			L"https://github.com/Loggan08/LoLUpdater/raw/master/Tbb/SSE2.dll",
			L"RADS/solutions/lol_game_client_sln/releases/0.0.1.62/deploy/tbb.dll",
			0,
			NULL
			);
	}
	if (::IsProcessorFeaturePresent(PF_XMMI_INSTRUCTIONS_AVAILABLE))
	{
		URLDownloadToFile(
			NULL,
			L"https://github.com/Loggan08/LoLUpdater/raw/master/Tbb/SSE.dll",
			L"RADS/solutions/lol_game_client_sln/releases/0.0.1.62/deploy/tbb.dll",
			0,
			NULL
			);
	}
	else
	{
		URLDownloadToFile(
			NULL,
			L"https://github.com/Loggan08/LoLUpdater/raw/master/Tbb/Default.dll",
			L"RADS/solutions/lol_game_client_sln/releases/0.0.1.62/deploy/tbb.dll",
			0,
			NULL
			);
	}
	DeleteFile(L"RADS/solutions/lol_game_client_sln/releases/0.0.1.62/deploy/tbb.dll:Zone.Identifier");

	return 0;
}

