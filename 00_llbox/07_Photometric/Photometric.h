#pragma once


typedef struct
{
	float	x;
	float	y;
}fCieCoords_t;

fCieCoords_t CCT2xy(int cct);
fCieCoords_t Mired2xy(unsigned short mired);
int xy2uv1960(fCieCoords_t, fCieCoords_t*);
int uv19602xy(fCieCoords_t uv, fCieCoords_t* xy);
int CCTDuv2xy(int cct, float Duv, fCieCoords_t* xy);


