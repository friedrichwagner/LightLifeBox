#include "PlatformWin32.h"
//#include <Windows.h>
#include <sstream>
#include <algorithm>
#include <conio.h>  //_getch();
#include "ControlBox.h"

namespace lumitech
{

	std::string gExeName;
	DWORD SequenceThreadID;
	HANDLE hSequenceThread;

	//to be able to stop the Sequence in the Ctr-C Handler
	ControlBox* thisBox = 0;

	int PlatformInit(char* argv0)
	{
		WSADATA w;
		if (WSAStartup(0x0101, &w) != 0)
			return -1;

		gExeName = argv0;

		SequenceThreadID = 0;
		hSequenceThread = 0;

		//Install Ctrl-C Handler
		SetConsoleCtrlHandler((PHANDLER_ROUTINE)CtrlHandler, TRUE);

		return 0;
	}
	void PlatformClose()
	{
		WSACleanup();
	}


	std::string getComputerNamePlatform()
	{
		std::string retstr("MyMachine");
		std::vector<char> MachineName(MAX_PATH);
		DWORD size;
		BOOL ret = false;
		ret = ::GetComputerNameA(&retstr[0], &size);

		if (ret)
		{
			retstr = std::string(retstr.begin(), retstr.begin() + size);
		}

		return retstr;
	}

	std::string getFileNamePlatform()
	{
		//std::vector<char> executablePath(gExeName.begin(), gExeName.end());
		std::vector<char> executablePath(MAX_PATH);
		std::string retstr;

		retstr = "thislog.log";

		DWORD result = ::GetModuleFileNameA(nullptr, &executablePath[0], static_cast<DWORD>(executablePath.size()));

		if (result > 0)
		{
			retstr = std::string(executablePath.begin(), executablePath.begin() + result);
			retstr.replace(retstr.find(".exe"), sizeof(".log") - 1, ".log");
		}
		return retstr;
	}

	std::string getFilePathPlatform()
	{
		std::vector<char> executablePath(MAX_PATH);
		std::string retstr;
		retstr = "thisIni.ini";

		DWORD result = ::GetModuleFileNameA(nullptr, &executablePath[0], static_cast<DWORD>(executablePath.size()));

		if (result > 0)
		{
			retstr = std::string(executablePath.begin(), executablePath.begin() + result);
			std::string::size_type pos = retstr.find_last_of("\\/");
			retstr = retstr.substr(0, pos + 1);
		}
		return retstr;
	}

	int stoi(std::string s)
	{
		//int ret = fromString<int>(s);
		//return ret;

		return std::stoi(s);
	}

	long stol(std::string s)
	{
		return std::stol(s);
	}

	float stof(std::string s)
	{
		return std::stof(s);
	}

	double stod(std::string s)
	{
		return std::stod(s);
	}

	bool stob(std::string s)
	{
		bool ret = false;

		std::transform(s.begin(), s.end(), s.begin(), ::toupper);
		if (s.compare("TRUE") == 0 || s.compare("1") == 0) ret = true;

		return ret;
	}


	template <typename T>
	std::string toString(T Number)
	{
		std::ostringstream ss;
		ss << Number;
		return ss.str();
	}

	std::string itos(int i)
	{
		return toString<int>(i);
	}

	std::string dtos(double d)
	{
		return toString<double>(d);
	}

	int waitOnKeyPress()
	{
		int c = _getch();

		return c;
	}

	void sleep(unsigned long miliseceonds)
	{
		Sleep(miliseceonds);
	}


	int gettimeofdayLT(struct timeval* tp, void* tzp)
	{
		DWORD t;
		t = timeGetTime();
		tp->tv_sec = t / 1000;
		tp->tv_usec = t % 1000;

		return 0;
	}

	// Get current date/time, format is YYYY-MM-DD.HH:mm:ss
	const std::string currentDateTime()
	{
		time_t     now = time(0);
		struct tm  tstruct;
		char       buf[80];
		localtime_s(&tstruct, &now);
		// Visit http://en.cppreference.com/w/cpp/chrono/c/strftime
		// for more information about date/time format
		strftime(buf, sizeof(buf), "%Y.%m.%d %X", &tstruct);

		return buf;
	}

	std::vector<unsigned char> intToBytes(UINT16 paramInt)
	{
		int size = sizeof(UINT16);

		std::vector<unsigned char> arrayOfByte(size);
		for (int i = 0; i < size; i++)
			arrayOfByte[size - i] = (paramInt >> (i * 8));
		return arrayOfByte;
	}

	BOOL CtrlHandler(DWORD fdwCtrlType)
	{
		switch (fdwCtrlType)
		{
			// Handle the CTRL-C signal. 
		case CTRL_C_EVENT:
			cout << "Ctrl-C event" << endl;

			if (thisBox != 0) 	thisBox->stopEventLoop();

			return true;

			// CTRL-CLOSE: confirm that the user wants to exit. 
		case CTRL_CLOSE_EVENT:
			cout << "Ctrl-Close event" << endl;
			if (thisBox != 0) 	thisBox->stopEventLoop();
			//exit(1);
			return true;

			// Pass other signals to the next handler. 
		case CTRL_BREAK_EVENT:
			cout << "Ctrl-Break event" << endl;
			if (thisBox != 0) 	thisBox->stopEventLoop();
			//exit(1);
			return false;

		case CTRL_LOGOFF_EVENT:
			cout << "Ctrl-Logoff event" << endl;
			if (thisBox != 0) 	thisBox->stopEventLoop();
			//exit(1);
			return FALSE;

		case CTRL_SHUTDOWN_EVENT:
			cout << "Ctrl-Shutdown event" << endl;
			if (thisBox != 0) 	thisBox->stopEventLoop();
			//exit(1);
			return false;

		default:
			return false;
		}
	}

	void startSequenceThread(LPTHREAD_START_ROUTINE funcPointer, char* boxName)
	{
		hSequenceThread = CreateThread(NULL, 0, funcPointer, (void*)boxName, 0, &SequenceThreadID);
	}

	void waitSequenceThread()
	{
		WaitForSingleObject(hSequenceThread, INFINITE);

		CloseHandle(hSequenceThread);
	}

	void setSequencePointer(void* p)
	{
		if (p != 0) thisBox = (ControlBox*)p;
	}

}