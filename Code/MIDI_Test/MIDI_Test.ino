
bool a = 0;

void setup() {
  // put your setup code here, to run once:

  pinMode(10,OUTPUT);
  Serial.begin(38400);

}

void loop() {
  // put your main code here, to run repeatedly:

  a = !a;
  digitalWrite(10,a);
  delay(500);
  bool b = digitalRead(11);
  digitalWrite(13,b);
  Serial.print(a);
  Serial.println(b);

}
