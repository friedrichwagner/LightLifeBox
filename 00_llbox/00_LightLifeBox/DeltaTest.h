#pragma once

#include "Settings.h"
#include "PILight.h"

enum TestMode : int { DTEST_NONE = 0, DTEST_BRIGHTNESS_UP = 1, DTEST_BRIGHTNESS_DOWN = 2, DTEST_CCT_UP = 3, DTEST_CCT_DOWN = 4, DTEST_JUDD_UP = 5, DTEST_JUDD_DOWN = 6 };

class DeltaTest
{
	friend class ControlBox;
protected:
	PILight* light;
	int UserID;

	//Starting Values
	int cct0;
	int brightness0;
	float xy0[2];

	//Actual Values
	int cct;
	int brightness;
	float xy[2];
	float actDuv;
	
	//Deltas
	int dcct;
	int dbrightness;
	float duv;

	//others
	int repeatCnt;
	float frequency;	
	float delayTime;
	TestMode mode;
	int actStep;

	Settings* ini;
	void SetLightState(int);

	bool done;
	unsigned long runTest(void);
	std::thread threadRun;
	void spawnRunThread()
	{
		threadRun = std::thread(&DeltaTest::runTest, this);
	};
	
public:
	DeltaTest();
	~DeltaTest();

	void start(PILight* _light, int Userid, int b0, int cct0, TestMode m);
	void stop();
	string getState();
};
