#include <stdio.h>
#include <wiringPi.h>
#include "rotaryencoder.h"

//----- Brightness
#define PORT_BRIGHTNESS_UP 		21      // Define wiringPIPin21/Physical29 as A2
#define PORT_BRIGHTNESS_DOWN 	25      // Define wiringPIPin25/Physical37 as B2
#define PORT_BRIGHTNESS_PRESSED 22   	// Define wiringPIPin22/Physical31 as TAST2
#define LED_BRIGHTNESS 			23 		// Define wiringPiPin23/Physical33 as LED1
//----- 

//----- CCT
#define PORT_CCT_UP 		12      // Define wiringPIPin12Physical19 as A3
#define PORT_CCT_DOWN 		14      // Define wiringPIPin14/Physical23 as B3
#define PORT_CCT_PRESSED 	13   	// Define wiringPIPin13/Physical21 as TAST3
#define LED_CCT 			24 		// Define wiringPiPin24/Physical35 as LED1
//----- 

//----- Judd
#define PORT_JUDD_UP 		27       // Define wiringPIPin27/Physical36  as A1
#define PORT_JUDD_DOWN 		29       // Define wiringPIPin29/Physical40  as B1
#define PORT_JUDD_PRESSED 	28    	// Define wiringPIPin28/Physical38  as TAST1
#define LED_JUDD 			6 	// Define wiringPiPin6/Physical22 as LED1
//----- 

//----- Lock Taster
#define PORT_LOCK1 1 	// Define wiringPIPin1/Physical12 as EINGABETAST
#define PORT_LOCK2 4 	// Define wiringPIPin4/Physical26 as EINGABETAST2

#define SUMMER 26     // Define wiringPiPin26/Physical32 as Summer-Piezo

#define DEFAULT_VALUE 		100
#define BLINK_DELAY_TIME 	200
#define BLINK_COUNT 		3

struct encoder *encBrigthness;
struct encoder *encCCT;
struct encoder *encJudd;

//------------------ Test LEDs
void BlinkLED(int LEDNUM, int time, int cnt)
{
	int i=0;
	
	for ( i=0; i<cnt; i++)
	{
		digitalWrite(LEDNUM, HIGH);
		delay(time);
		digitalWrite(LEDNUM, LOW);
		delay(time);
	}
	
}

void BlinkAllLED(int time, int cnt)
{
	int i=0;
	
	for ( i=0; i<cnt; i++)
	{
		digitalWrite(LED_BRIGHTNESS, HIGH); digitalWrite(LED_CCT, HIGH); digitalWrite(LED_JUDD, HIGH);
		delay(time);
		digitalWrite(LED_BRIGHTNESS, LOW); digitalWrite(LED_CCT, LOW); digitalWrite(LED_JUDD, LOW);
		delay(time);
	}
	
}

//------------------ INIT PINS and FUNCTIONS
void init(void)
{
	wiringPiSetup();
	//Brigthness
	pinMode(LED_BRIGHTNESS, OUTPUT);
	BlinkLED(LED_BRIGHTNESS, BLINK_DELAY_TIME, BLINK_COUNT);
	
	//CCT
	pinMode(LED_CCT, OUTPUT);
	BlinkLED(LED_CCT, BLINK_DELAY_TIME, BLINK_COUNT);
	
	//JUDD
	pinMode(LED_JUDD, OUTPUT);
	BlinkLED(LED_JUDD, BLINK_DELAY_TIME, BLINK_COUNT);

	//LOCK
	pinMode(PORT_LOCK1, INPUT);
	pinMode(PORT_LOCK2, INPUT);
}

//------------------ ISRs - Interrupt Service Routines
void isr_Brigthness_Pressed(void)
{
	setValue(encBrigthness,DEFAULT_VALUE) ;
	BlinkLED(LED_BRIGHTNESS, 500,1); 
}

void isr_Cct_Pressed(void)
{
	setValue(encCCT,DEFAULT_VALUE) ;
	BlinkLED(LED_CCT, 500,1);
}

void isr_Judd_Pressed(void)
{
	setValue(encJudd,DEFAULT_VALUE);
	BlinkLED(LED_JUDD, 500,1);
}

void isr_Lock1_Pressed(void)
{
	setValue(encBrigthness,DEFAULT_VALUE) ; setValue(encCCT,DEFAULT_VALUE) ; setValue(encJudd,DEFAULT_VALUE) ;
	BlinkAllLED(500,1);
}

void isr_Lock2_Pressed(void)
{
	isr_Lock1_Pressed();
}


//------------------ INIT ISRs
void initISRs(void)
{
	//ISRs Brightness
	wiringPiISR(PORT_BRIGHTNESS_PRESSED, INT_EDGE_FALLING, &isr_Brigthness_Pressed);
	
	// ISRs Color
	wiringPiISR(PORT_CCT_PRESSED, INT_EDGE_FALLING, &isr_Cct_Pressed);

	// ISRs Plankdistance
	wiringPiISR(PORT_JUDD_PRESSED, INT_EDGE_FALLING, &isr_Judd_Pressed);

	//ISRs EINGABESTASTER
	wiringPiISR(PORT_LOCK1, INT_EDGE_FALLING, &isr_Lock1_Pressed);
	wiringPiISR(PORT_LOCK2, INT_EDGE_FALLING, &isr_Lock2_Pressed);
}

//---------------------------------------------------------------
// Kommando zum compilieren am PI:
//			gcc -lwiringPi -Wall -o testbox wiringpitest2.c rotaryencoder.c
//
// Use: sudo ./testbox
//---------------------------------------------------------------
int main(void)
{

	init();
	initISRs();
	
	encBrigthness 	= setupencoder(PORT_BRIGHTNESS_UP, 	PORT_BRIGHTNESS_DOWN, 	DEFAULT_VALUE);
	encCCT 			= setupencoder(PORT_CCT_UP, 		PORT_CCT_DOWN, 			DEFAULT_VALUE);
	encJudd 		= setupencoder(PORT_JUDD_UP, 		PORT_JUDD_DOWN, 		DEFAULT_VALUE);


	while (1)
	{
		printf(" Brightness: %3ld \t CCT: %3ld \t Judd: %3ld \r", encBrigthness->value, encCCT->value, encJudd->value) ;
		delay(10);
	}

	return 0;

}

