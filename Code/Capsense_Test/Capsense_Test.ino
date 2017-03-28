#include <CapacitiveSensor.h>

CapacitiveSensor cSenseArray[] = {CapacitiveSensor(7,8),CapacitiveSensor(9,10),CapacitiveSensor(11,12)};

long cSenseValue[3] = {0};
int cSenseMax[3] = {306,306,346};
int cSenseMin[3] = {50,50,90};
byte ledArray[3] = {3,5,6};

unsigned long previousTime = 0;


void setup() {

  for(byte i = 0; i< 3; i++)
  {
    pinMode(ledArray[i], OUTPUT); 
    //cSenseArray[i].set_CS_AutocaL_Millis(0xFFFFFFFF);
  }

  Serial.begin(38400);
}

void loop() {

  for(byte i = 0; i< 3; i++)
  {
    cSenseValue[i] = cSenseArray[i].capacitiveSensor(10);
    byte b = constrain(map(cSenseValue[i],cSenseMin[i],cSenseMax[i],0,255),0,255);
    analogWrite(ledArray[i],b); 
  }

  if (millis()-previousTime>200)
  {
    for(byte i = 0; i< 3; i++)
    {    
      Serial.print(i);
      Serial.print(": ");
      Serial.print(cSenseValue[i]);
      Serial.print(",   ");
    }
    
    Serial.println();

    previousTime = millis();
  }

}

