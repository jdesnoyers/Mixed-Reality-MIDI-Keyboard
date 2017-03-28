//test current scanning rate
void speedCheck()
{
  loopTime = micros()-loopTime;
  Serial.println(loopTime);
  loopTime = micros();
}

void activateLED(byte led, byte vel)
{

  ledHSV[led].val = vel*2;        //set HSV model value equal to velocity
  ledRGB[led] = ledHSV[led];   //set RGB output equal to HSV
  
}

void deactivateLED(byte led)
{
  ledRGB[led] = CRGB(0,0,0);
}

