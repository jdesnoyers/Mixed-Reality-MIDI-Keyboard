void setup() {

  for(byte i=0; i<8; i++)
  {

    pinMode(matrixKD[i], OUTPUT);     //Activate KD columns as outputs

    digitalWrite(matrixKD[i], 0);     //set all KD low (inactive)
        
    //Activate BR, MK as inputs via registers (10k pull-down resistors)
    /* BR 10,9,8,7,6,5,4,3 = 47,48,44,40,36,32,28,24 = L2,L1,L5,G1,C1,C5,A6,A2
     * MK 10,9,8,7,6,5,4,3 = 49,46,42,38,34,30,26,25 = L0,L3,L7,D7,C3,C7,A4,A3
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

  #if(CAP_ARRAY_ACTIVE==1)
    capArray.begin(0x5A);   //Activate capacitive touch array
  #endif
  
  FastLED.addLeds<NEOPIXEL,LED_DATA_PIN>(ledRGB, NUM_LEDS).setCorrection(TypicalLEDStrip);       //Activate LED strip with color correction
  FastLED.setTemperature(HighNoonSun);  //set LED temperature adjustment
  FastLED.setBrightness(255);           //set maximum brightness
  FastLED.clear();                      //clear the LED array
  FastLED.show();                       //Initialize pixels off
  
  fill_rainbow(&ledHSV[START_KEYLEDS],NUM_KEYLEDS,0,5); //fill HSV array with spectrum along keyboard

  TCCR1B = TCCR1B & B11111000 | 0x01; //increase PWM frequency for pins 11 & 12 to 31250
  TCCR2B = TCCR2B & B11111000 | 0x01; //increase PWM frequency for pins 9 & 10 to 31250

  //set IR LED pins to outputs
  pinMode(IR_LED_PIN_A,OUTPUT); 
  pinMode(IR_LED_PIN_B,OUTPUT);
  #if (LEFT_LED_ACTIVE == 1) 
    pinMode(IR_LED_PIN_C,OUTPUT);
  #endif

  //Activate input pullup resistors on encoder and buttons
  pinMode(ENCODER_CHAN_A,INPUT_PULLUP);
  pinMode(ENCODER_CHAN_B,INPUT_PULLUP);
  pinMode(ENCODER_BUTTON,INPUT_PULLUP);
  pinMode(ARCADE_BUTTON_01,INPUT_PULLUP);
  pinMode(ARCADE_BUTTON_02,INPUT_PULLUP);
  pinMode(ARCADE_BUTTON_03,INPUT_PULLUP);

  attachInterrupt(digitalPinToInterrupt(2),encoderPinA,RISING);  //set interrupt to watch for changes on encoder PinA
  attachInterrupt(digitalPinToInterrupt(3),encoderPinB,RISING);  //set interrupt to watch for changes on encoder PinB
  
}
