//test current scanning rate
void speedCheck()
{
  loopTime = micros()-loopTime;
  Serial.println(loopTime);
  loopTime = micros();
}

void activateLED(byte led, byte vel)
{

  led = map(led,0,60,START_KEYLEDS,NUM_KEYLEDS+START_KEYLEDS-1);
  
  ledHSV[led].val = vel*2;        //set HSV model value equal to velocity
  ledRGB[led] = ledHSV[led];   //set RGB output equal to HSV
  ledAvgCount++;
  ledAverageRGB = ((ledAverageRGB*(ledAvgCount-1))+ledRGB[led])/ledAvgCount;
}

void deactivateLED(byte led)
{
  
  led = map(led,0,60,START_KEYLEDS,NUM_KEYLEDS+START_KEYLEDS-1);
  ledAvgCount--;
  
  if(ledAvgCount>0)
    ledAverageRGB = ((ledAverageRGB*(ledAvgCount+1))-ledRGB[led])/ledAvgCount; 
  else
    ledAverageRGB = CRGB(0,0,0);
    
  ledRGB[led] = CRGB(0,0,0);
}

void colorizeEnds()
{
    
  if((capkeytoggle>>1)&1)
  {
    fill_rainbow(&ledRGB[0],START_KEYLEDS,0,51);
    fill_rainbow(&ledRGB[NUM_KEYLEDS+START_KEYLEDS],NUM_LEDS-NUM_KEYLEDS-START_KEYLEDS,0,51);
  }
  else
  {
    fill_solid(&ledRGB[0],START_KEYLEDS,ledAverageRGB);
    fill_solid(&ledRGB[NUM_KEYLEDS+START_KEYLEDS],NUM_LEDS-NUM_KEYLEDS-START_KEYLEDS,ledAverageRGB);
  }
}


