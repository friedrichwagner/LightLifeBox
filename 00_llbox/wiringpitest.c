#include <stdlib.h>
#include <stdio.h>
#include <wiringPi.h>
#include <softTone.h>

//////////////////////////////////////////////////////////////////////////////////
//DEFINES:
/////////////////////////////////////////////////////////////////////////////////

#define A1 27       // Define wiringPIPin27/Physical36  as A1
#define B1 29       // Define wiringPIPin29/Physical40  as B1
#define TAST1 28    // Define wiringPIPin28/Physical38  as TAST1

#define A2 21      // Define wiringPIPin21/Physical29 as A2
#define B2 25      // Define wiringPIPin25/Physical37 as B2
#define TAST2 22   // Define wiringPIPin22/Physical31 as TAST2

#define A3 12      // Define wiringPIPin12Physical19 as A3
#define B3 14      // Define wiringPIPin14/Physical23 as B3
#define TAST3 13   // Define wiringPIPin13/Physical21 as TAST3

#define EINGABETAST 1 // Define wiringPIPin1/Physical12 as EINGABETAST
#define EINGABETAST2 4 // Define wiringPIPin4/Physical26 as EINGABETAST2

#define SUMMER 26     // Define wiringPiPin26/Physical32 as Summer-Piezo

#define LED1 6 // Define wiringPiPin6/Physical22 as LED1
#define LED2 23 // Define wiringPiPin23/Physical33 as LED1
#define LED3 24 // Define wiringPiPin24/Physical35 as LED1

//VARIABLE
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
volatile int plankdistance = 127;
volatile bool plankchange = 0;

volatile int brightness = 127;
volatile bool brightnesschange = 0;

volatile int color = 127;
volatile bool colorchange = 0;
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//INIT PINS and FUNCTIONS

void init(void)
{
	wiringPiSetup();

	pinMode(A1, INPUT);
	pinMode(B1, INPUT);
	pinMode(TAST1, INPUT);

	pinMode(A2, INPUT);
	pinMode(B2, INPUT);
	pinMode(TAST2, INPUT);

	pinMode(A3, INPUT);
	pinMode(B3, INPUT);
	pinMode(TAST3, INPUT);

	pinMode(EINGABETAST, INPUT);
	pinMode(EINGABETAST2, INPUT);

	pinMode(LED1, OUTPUT);
	pinMode(LED2, OUTPUT);
	pinMode(LED3, OUTPUT);

	pinMode(SUMMER, OUTPUT);

	// int softToneCreate(int pin);
}


//ISRs_PLANK DISTANCE
///////////////////////////////////////////////////////////////////////////////////////////////////////////

void isr_A1(void)
{

	if (!plankchange)
	{
		plankdistance++;
		plankchange = 1;
	}
	else if (plankchange)
	{
		plankchange = 0;
	}
}

void isr_B1(void)
{

	if (!plankchange)
	{
		plankdistance--;
		plankchange=1;
	}
	else if (plankchange)
	{
		plankchange=0;
	}
}

void isr_TAST1(void)
{
	plankdistance = 127;
}

// ISRs BRIGHTNESS
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void isr_A2(void)
{

	if (!brightnesschange)
	{
		brightness++;
		brightnesschange = 1;
	}
	else if (brightnesschange)
	{
		brightnesschange = 0;
	}
}

void isr_B2(void)
{

	if (!brightnesschange)
	{
		brightness--;
		brightnesschange = 1;
	}
	else if (brightnesschange)
	{
		brightnesschange = 0;
	}
}

void isr_TAST2(void)
{
	plankdistance = 127;
}
// ISRs COLOR
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void isr_A3(void)
{

	if (!colorchange)
	{
		color++;
		colorchange = 1;
	}
	else if (colorchange)
	{
		colorchange = 0;
	}
}

void isr_B3(void)
{

	if (!colorchange)
	{
		color--;
		colorchange = 1;
	}
	else if (colorchange)
	{
		colorchange = 0;
	}
}

void isr_TAST3(void)
{
	color = 127;
}
// ISRs EINGABETASTE
/////////////////////////////////////////////////////////////////////////////////////////////

void isr_EINGABETAST(void)
{
 // LOGGING TO SERVER from "HAUPTTEIL"
}

void isr_EINGABETAST2(void)
{
	// LOGGING TO SERVER from "NEBENTEIL
}


// INIT ISRs
void initISRs(void)
{
	// ISRs Plankdistance
	wiringPiISR(A1, INT_EDGE_FALLING, &isr_A1);
	wiringPiISR(B1, INT_EDGE_FALLING, &isr_B1);
	wiringPiISR(TAST1, INT_EDGE_FALLING, &isr_TAST1);
	//ISRs Brightness
	wiringPiISR(A2, INT_EDGE_FALLING, &isr_A2);
	wiringPiISR(B2, INT_EDGE_FALLING, &isr_B2);
	wiringPiISR(TAST2, INT_EDGE_FALLING, &isr_TAST2);
	// ISRs Color
	wiringPiISR(A3, INT_EDGE_FALLING, &isr_A3);
	wiringPiISR(B3, INT_EDGE_FALLING, &isr_B3);
	wiringPiISR(TAST3, INT_EDGE_FALLING, &isr_TAST3);

	//ISRs EINGABESTASTER
	wiringPiISR(EINGABETAST, INT_EDGE_FALLING, &isr_EINGABETAST);
	wiringPiISR(EINGABETAST2, INT_EDGE_FALLING, &isr_EINGABETAST2);
}


//g++ -Wall -o testfw wiringpitest.c -lwiringPi
int main(void)
{

	init();
	initISRs();

	while (1)
	{
		printf("Judd: %3d \t CCT: %3d \t Brightness: %3d \r", plankdistance, brightness, color) ;
		delay(100);
	}

	return 0;

}

