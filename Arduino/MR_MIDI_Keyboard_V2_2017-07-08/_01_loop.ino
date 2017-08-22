void loop() {
  
  scanRow(rownum);  //scan row on every cycle

  if(rownum >=7)  //perform other operations once per matrix scan
  {
    rownum=0;
    midiAftertouch();
    midiAbstractionLayer();
    midiCutoffFrequency();
    midiResonance();
    #if(CAP_ARRAY_ACTIVE==1)
      midiCapArray();
    #endif
    midiButtonArray();
    midiPitchBend();
    midiEncoder();
    colorizeEnds();
    FastLED.show(); //activate current LEDs
    //speedCheck();
  }
  else
    rownum++;

}
