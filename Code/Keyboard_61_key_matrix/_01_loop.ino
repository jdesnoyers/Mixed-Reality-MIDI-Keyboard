void loop() {
  
  scanRow(rownum);

  if(rownum >=7)
  {
    rownum=0;
    midiAftertouch();
    //speedCheck();
    midiAbstractionLayer();
    midiCapArray();
    //midiPitchBend();
    FastLED.show(); //activate current LEDs
  }
  else
    rownum++;

}
