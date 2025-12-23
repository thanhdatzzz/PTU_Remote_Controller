#include <WiFi.h>

// ================= CẤU HÌNH WIFI =================
const char* ssid = "iphoencp"; 
const char* password = "11111111"; 
const char* serverIP = "192.168.137.1"; 
const int serverPort = 8888;

#define VRX_PIN 32  
#define VRY_PIN 33  
#define SW_PIN  25  
#define POT_PIN 34  
#define ZOOM_PIN 35 // <--- THÊM CHÂN NÀY (Biến trở Zoom)

WiFiClient client;

void setup() {
  Serial.begin(115200);
  pinMode(SW_PIN, INPUT_PULLUP);
  pinMode(ZOOM_PIN, INPUT); // <--- Khai báo Input

  WiFi.begin(ssid, password);
  Serial.print("Connecting to WiFi");
  while (WiFi.status() != WL_CONNECTED) {
    delay(500); Serial.print(".");
  }
  Serial.println("\nWiFi Connected!");
}

void loop() {
  if (!client.connected()) {
    Serial.println("Connecting to Server...");
    if (client.connect(serverIP, serverPort)) {
      Serial.println("Connected to PC!");
      client.setNoDelay(true); 
    } else {
      delay(1000); return;
    }
  }

  // Đọc giá trị
  int pan = analogRead(VRX_PIN);
  int tilt = analogRead(VRY_PIN);
  int btn = digitalRead(SW_PIN);
  int speed = analogRead(POT_PIN);
  int zoom = analogRead(ZOOM_PIN); 

  // Tạo chuỗi lệnh: "J:pan,tilt,btn,speed,zoom"
  // Thêm zoom vào cuối chuỗi
  String cmd = "J:" + String(pan) + "," + String(tilt) + "," + String(btn) + "," + String(speed) + "," + String(zoom) + "\n";
  
  client.print(cmd);
  
  delay(30); 
}