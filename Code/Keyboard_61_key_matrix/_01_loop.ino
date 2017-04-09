void loop() {
  
  scanRow(rownum);

  if(rownum >=7)
  {
    rownum=0;
    midiAftertouch();
    //speedCheck();
    midiAbstractionLayer();
    midiCutoffFrequency();
    midiCapArray();
    midiPitchBend();
    colorizeEnds();
    FastLED.show(); //activate current LEDs
  }
  else
    rownum++;

}
