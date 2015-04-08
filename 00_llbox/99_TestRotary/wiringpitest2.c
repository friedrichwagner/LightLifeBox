#include <stdio.h>
#include <wiringPi.h>
#include "rotaryencoder.h"

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

#define DEFAULT_VALUE 100

struct encoder *encBrigthness;
struct encoder *encCCT;
struct encoder *encJudd;

//VARIABLE
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*volatile int plankdistance = DEFAULT_VALUE ;
volatile int plankchange = 0;

volatile int brightness = DEFAULT_VALUE ;
volatile int brightnesschange = 0;

volatile int color = DEFAULT_VALUE ;
volatile int colorchange = 0;*/
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//INIT PINS and FUNCTIONS

void init(void)
{
	wiringPiSetup();

	pinMode(EINGABETAST, INPUT);
	pinMode(EINGABETAST2, INPUT);

	pinMode(LED1, OUTPUT);
	pinMode(LED2, OUTPUT);
	pinMode(LED3, OUTPUT);
}


//ISRs_PLANK DISTANCE
///////////////////////////////////////////////////////////////////////////////////////////////////////////

void isr_TAST1(void)
{
	setValue(encJudd,DEFAULT_VALUE) ;
}

// ISRs BRIGHTNESS
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void isr_TAST2(void)
{
	setValue(encBrigthness,DEFAULT_VALUE) ;
}
// ISRs COLOR
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////

void isr_TAST3(void)
{
	setValue(encCCT,DEFAULT_VALUE) ;
}
// ISRs EINGABETASTE
/////////////////////////////////////////////////////////////////////////////////////////////

void isr_EINGABETAST(void)
{
 // LOGGING TO SERVER from "HAUPTTEIL"
	setValue(encBrigthness,DEFAULT_VALUE) ; setValue(encCCT,DEFAULT_VALUE) ; setValue(encJudd,DEFAULT_VALUE) ;
}

void isr_EINGABETAST2(void)
{
	// LOGGING TO SERVER from "NEBENTEIL
	isr_EINGABETAST();
}


// INIT ISRs
void initISRs(void)
{
	//ISRs Brightness
	wiringPiISR(TAST2, INT_EDGE_FALLING, &isr_TAST2);
	
	// ISRs Color
	wiringPiISR(TAST3, INT_EDGE_FALLING, &isr_TAST3);

	// ISRs Plankdistance
	wiringPiISR(TAST1, INT_EDGE_FALLING, &isr_TAST1);

	//ISRs EINGABESTASTER
	wiringPiISR(EINGABETAST, INT_EDGE_FALLING, &isr_EINGABETAST);
	wiringPiISR(EINGABETAST2, INT_EDGE_FALLING, &isr_EINGABETAST2);
}


//gcc -lwiringPi -Wall -o testfw wiringpitest2.c rotaryencoder.c
int main(void)
{

	init();
	initISRs();
	
	encBrigthness = setupencoder(A2, B2, DEFAULT_VALUE);
	encCCT = setupencoder(A3, B3, DEFAULT_VALUE);
	encJudd = setupencoder(A1, B1, DEFAULT_VALUE);


	while (1)
	{
		printf(" Brightness: %3ld \t CCT: %3ld \t Judd: %3ld \r", encBrigthness->value, encCCT->value, encJudd->value) ;
		delay(10);

	}

	return 0;

}

