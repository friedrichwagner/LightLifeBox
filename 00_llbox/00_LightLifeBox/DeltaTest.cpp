#include "DeltaTest.h"
#include "helpers.h"
#include "Photometric.h"

DeltaTest::DeltaTest(int _boxid)
{
	ini = Settings::getInstance();
	boxid = _boxid;

	//Starting Values
	cct0 = DEFAULT_CCT; brightness0 = DEFAULT_BRIGHTNESS;
	fCieCoords_t cie = CCT2xy(cct0);
	xy0[0] = cie.x; xy0[1] = cie.y;

	//Actual Values
	cct = cct0; brightness = brightness0;
	xy[0] = xy0[0]; xy[1] = xy0[1];
	actDuv = 0.0f;

	//Deltas
	dcct = ini->Read<int>("DeltaTest", "dCCT",25);
	dbrightness = ini->Read<int>("DeltaTest", "dBrightness", 2);
	duv = ini->Read<float>("DeltaTest", "duv", 0.0005f);

	//others
	repeatCnt = ini->Read<int>("DeltaTest", "repeatCnt", 3);
	frequency = ini->Read<float>("DeltaTest", "frequency", 1);
	delayTime = (float) (1.0f / frequency / 2.0f)*1000;
	mode = DTEST_NONE;
	actStep = 0;
	done = false;
}

DeltaTest::~DeltaTest()
{
	stop();
}

void DeltaTest::SetLightState(int deltaCnt)
{
	switch (mode)
	{
	case DTEST_BRIGHTNESS_UP:
		brightness = brightness0 + deltaCnt*dbrightness;
		light->setBrightness(brightness);

		if ((brightness <= 0) || (brightness >= 255))
			done = true;
		break;

	case DTEST_BRIGHTNESS_DOWN:
		brightness = brightness0 - deltaCnt*dbrightness;
		light->setBrightness(brightness);

		if ((brightness <= 0) || (brightness >= 255))
			done = true;
		break;

	case DTEST_CCT_UP:
		cct = cct0 + deltaCnt*dcct;
		light->setCCT(cct);
		light->getXY(xy);

		if (cct > MAX_CCT) done = true;
		break;

	case DTEST_CCT_DOWN:
		cct = cct0 - deltaCnt*dcct;
		light->setCCT(cct);
		light->getXY(xy);

		if (cct < MIN_CCT) done = true;
		break;

	case DTEST_JUDD_UP:
		actDuv = deltaCnt*duv;
		light->setCCTDuv(cct0, actDuv);
		light->getXY(xy);

		if (duv > MAX_DUV) done = true;
		break;

	case DTEST_JUDD_DOWN:
		actDuv = deltaCnt*duv * -1;
		light->setCCTDuv(cct0, actDuv);
		light->getXY(xy);

		if (duv < MIN_DUV) done = true;
		break;
	}
}

void DeltaTest::start(PILight* _light, int _userid, int b0, int _cct0, TestMode m)
{
	if ((m != DTEST_NONE) && (_light != NULL))
	{
		light = _light; UserID = _userid;
		actStep = 0;

		brightness0 = b0; cct0 = _cct0; mode = m;
		brightness = b0; cct = _cct0; actDuv = 0.0f;

		light->setFadeTime(0);
		
		//Ausgangssituation herstellen
		light->setCCT(cct0);
		light->getXY(xy0);
		xy[0] = xy0[0]; xy[1] = xy0[1];

		lumitech::sleep(DEFAULT_NEOLINK_FADETIME + 30);
		light->setBrightness(brightness0);

		spawnRunThread();
	}
}

unsigned long DeltaTest::runTest()
{

	while (!done)
	{
		actStep++;
		for (int i = 0; i < repeatCnt; i++)
		{
			SetLightState(0);
			lumitech::sleep((unsigned long) delayTime);
			SetLightState(actStep);
			lumitech::sleep((unsigned long) delayTime);
		}
	}

	return 0;
}


void DeltaTest::stop()
{
	done = true;
	if (threadRun.joinable())  threadRun.join();
}

string DeltaTest::getState()
{
	ostringstream ss;
	
	//damit sicher der actStep Status gesetzt ist, und nicht der Grundzustand
	SetLightState(actStep);

	ss << "userid=" << UserID << ";boxid=" << boxid << ";brightness0=" << brightness0 << ";cct0=" << cct0 << ";x0=" << xy0[0] << ";y0=" << xy0[1] << ";mode=" << mode
		<< ";actStep=" << actStep << ";brightness=" << brightness << ";cct=" << cct << ";x=" << xy[0] << ";y=" << xy[1] << ";actduv=" << actDuv
		<< ";dbrightness=" << dbrightness << ";dcct=" << dcct << ";duv=" << duv << ";frequency=" << frequency << ";dummy=";



	return ss.str();
}
