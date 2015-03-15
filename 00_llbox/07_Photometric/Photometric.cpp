#include "Photometric.h"
#include <math.h>

#define ACCURACY	0.000001f

void CCT2xy(int cct, float xy[])
{
	xy[0] = 0.3333f;
	xy[1] = 0.3333f;
}

int xy2uv1960(float xy[], float uv[])
{
	float nen = (-2 * xy[0] + 12 * xy[1] + 3);

	if (abs(nen) > ACCURACY)
	{
		uv[0] = 4 * xy[0] / nen;
		uv[1] = 6 * xy[1] / nen;

		return 0;
	}

	return -1;
}

int uv19602xy(float uv[], float xy[])
{
	float nen = (2*uv[0] - 8*uv[1] + 4);

	if (abs(nen) > ACCURACY)
	{
		xy[0] = 3 * uv[0] / nen;
		xy[1] = 2 * uv[1] / nen;

		return 0;
	}

	return -1;
}

//------------------------------------------------------------------------------------------------------------
//http://www.cormusa.org/uploads/CORM_2011_Calculation_of_CCT_and_Duv_and_Practical_Conversion_Formulae.PDF
//------------------------------------------------------------------------------------------------------------

int CCTDuv2xy(int cct, float Duv, float xy[])
{
	int ret;
	float u0v0[2]; 	float u1v1[2];
	float x0y0[2]; 	float x1y1[2];
	float duv[2]; float uv[2];
	xy[0] = 0.0; xy[1] = 0.0;


	//1. Calculate (u0,v0) of Planck at T(K)
	CCT2xy(cct, x0y0);
	ret=xy2uv1960(x0y0, u0v0);
	if (ret < 0) return -1;


	//2. Calculate (u1,v1) of Planck at T+dT(K), dT=1K (shall be 0,01K!!!)
	CCT2xy(cct+1, x1y1);
	ret=xy2uv1960(x1y1, u1v1);
	if (ret < 0) return -1;

	//3. Calculate
	duv[0] = u1v1[0] - u0v0[0];
	duv[1] = u1v1[1] - u0v0[1];

	float nen = sqrtf(duv[0] * duv[0] + duv[1] * duv[1]);
	if (abs(nen) > ACCURACY)
	{
		uv[0] = u0v0[0] + Duv * duv[0] / nen;
		uv[1] = u0v0[0] + Duv * duv[1] / nen;

		uv19602xy(uv, xy);

		return 0;
	}

	return -1;
}