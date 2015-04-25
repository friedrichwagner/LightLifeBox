#pragma once


enum TestMode : int { DTEST_NONE = 0, DTEST_BRIGHTNESS = 1, DTEST_CCT_UP = 2, DTEST_CCT_DOWN = 3, DTEST_JUDD_UP = 4, DTEST_JUDD_DOWN = 5 };

class DeltaTest
{
protected:
	//Fields
	int cct0;
	int brightness0;
	float xy0[2];
	
	int dcct;
	int dbrightness;
	float duv;

	int repeatCnt;
	int frequency;
	TestMode mode;
	
public:
	DeltaTest();
	~DeltaTest();
	void start();
	void stop();
	void lock();
};
