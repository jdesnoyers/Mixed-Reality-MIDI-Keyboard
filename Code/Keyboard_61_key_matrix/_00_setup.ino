void setup() {

  for(byte i=0; i<8; i++)
  {

    pinMode(matrixKD[i], OUTPUT);     //Activate KD columns as outputs

    digitalWrite(matrixKD[i], 0);     //set all KD low (inactive)
        
    /*Activate BR, MK as inputs (10k pull-down resistors)
    *BR 10,9,8,7,6,5,4,3 = 47,48,44,40,36,32,28,24 = L2,L1,L5,G1,C1,C5,A6,A2
    *MK 10,9,8,7,6,5,4,3 = 49,46,42,38,34,30,26,25 = L0,L3,L7,D7,C3,C7,A4,A3
    */
    DDRL = DDRL & B01010000;
    DDRG = DDRG & B11111101;
    DDRC = DDRC & B01010101;
    DDRA = DDRA & B10100011;
    //pinMode(matrixBR[i], INPUT);
    //pinMode(matrixMK[i], INPUT);
      
  }
  
  Serial.begin(38400);    //Activate USB serial
  Serial1.begin(31250);   //Activate MIDI serial

  capArray.begin(0x5A);   //Activate capacitive touch array

  FastLED.addLeds<NEOPIXEL,LED_DATA_PIN>(ledRGB, NUM_LEDS).setCorrection(TypicalLEDStrip);       //Activate LED strip
  FastLED.setTemperature(HighNoonSun);
  FastLED.setBrightness(127);
  FastLED.clear();  
  FastLED.show();        //Initialize pixels off
  
  fill_rainbow(&ledHSV[START_KEYLEDS],NUM_KEYLEDS,0,5);

  TCCR1B = TCCR1B & B11111000 | 0x01; //increase PWM frequency for pins 11 & 12 to 31250

  pinMode(IR_LED_PIN_A,OUTPUT);
  pinMode(IR_LED_PIN_B,OUTPUT);
  
}
