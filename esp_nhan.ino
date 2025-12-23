#include <Arduino.h>
#line 1 "C:\\Users\\Admin\\Documents\\Đồ Án Tốt Nghiệp\\ĐỒ ÁN TỐT NGHIỆP__ ĐẠT K14\\3D design\\V3\\code\\esp_nhan\\esp_nhan.ino"
#include <AccelStepper.h>
#include <WiFi.h>
#include <WiFiManager.h>
#include <ESPmDNS.h>

// ================= CẤU HÌNH IP MÁY TÍNH =================
const char* PC_SERVER_IP = "192.168.137.1"; 
const int PC_SERVER_PORT = 8888;

// ================= CẤU HÌNH PHẦN CỨNG (GIỮ NGUYÊN) =================
#define WIFI_RESET_BUTTON 0  
const unsigned long WIFI_RESET_HOLD_TIME = 3000; 
#define DIR_PIN_PAN   17
#define STEP_PIN_PAN  16
#define DIR_PIN_TILT  19
#define STEP_PIN_TILT 18
#define EN_PIN        5
#define PAN_HOME_PIN  33
#define TILT_HOME_PIN 32

// Thông số động cơ
#define MOTOR_STEPS_PER_REVP 25600
#define MOTOR_STEPS_PER_REVT 12800
#define PAN_GEAR_RATIO       3.0
#define TILT_GEAR_RATIO      3.0
#define PAN_STEPS_PER_DEGREE  (MOTOR_STEPS_PER_REVP * PAN_GEAR_RATIO / 360.0)
#define TILT_STEPS_PER_DEGREE (MOTOR_STEPS_PER_REVT * TILT_GEAR_RATIO / 360.0)

const float PAN_HARDWARE_MAX_SPEED = 8000.0; 
const float PAN_ACCEL_RUN      = 4000.0; 
const float PAN_ACCEL_STOP     = 15000.0; 
const long  PAN_MIN_LIMIT      = (long)(-90.0 * PAN_STEPS_PER_DEGREE);
const long  PAN_MAX_LIMIT      = (long)(90.0 * PAN_STEPS_PER_DEGREE);
const float HOMING_SPEED_PAN   = 6000.0;

const float TILT_HARDWARE_MAX_SPEED = 5000.0; 
const float TILT_ACCEL_RUN     = 3000.0;
const float TILT_ACCEL_STOP    = 10000.0;
const long  TILT_MIN_LIMIT     = (long)(-10.0 * TILT_STEPS_PER_DEGREE);
const long  TILT_MAX_LIMIT     = (long)(40.0 * TILT_STEPS_PER_DEGREE);
const float HOMING_SPEED_TILT  = 3000.0; 

const int STOP_LOCK_TIME = 300; 

AccelStepper panMotor(AccelStepper::DRIVER, STEP_PIN_PAN, DIR_PIN_PAN);
AccelStepper tiltMotor(AccelStepper::DRIVER, STEP_PIN_TILT, DIR_PIN_TILT);

WiFiClient tcpClient;          
bool isConnectedToPC = false;

// BIẾN TRẠNG THÁI
volatile int currentMode = 0; 
#define MODE_IDLE 0
#define MODE_JOYSTICK 1 // Điều khiển qua TCP Joystick
#define MODE_HOMING 2
#define MODE_PC_AUTO 3

long userPanOffset = 0; long userTiltOffset = 0;
volatile int homingState = 0; unsigned long homingWaitStart = 0;
volatile bool panHomingDone = false; volatile bool tiltHomingDone = false;

// Các biến giả lập struct cũ để tái sử dụng logic joystick
int rawPanVal = 2048; int rawTiltVal = 2048; int rawSpeedPot = 0;
volatile unsigned long lastJoystickTime = 0;
volatile int joyPanDir = 0; volatile int joyTiltDir = 0;
volatile float currentPanTargetSpeed = 1000.0; 
volatile float currentTiltTargetSpeed = 1000.0;

bool isPanStopping = false; bool isTiltStopping = false;
unsigned long panStopTimestamp = 0; unsigned long tiltStopTimestamp = 0;
volatile bool remoteGoHomeState = false;    
bool lastHomeButtonState = false;    
unsigned long lastHomeClickTime = 0; 
int homeClickCount = 0;              
const int DOUBLE_CLICK_DELAY = 500; 

TaskHandle_t TaskNetworkHandle;

// ================= LOGIC XỬ LÝ MỚI =================

void startHomingProcess() {
    currentMode = MODE_HOMING; homingState = 1; 
    panHomingDone = false; tiltHomingDone = false;
    panMotor.setMaxSpeed(HOMING_SPEED_PAN); tiltMotor.setMaxSpeed(HOMING_SPEED_TILT);
    panMotor.setSpeed(HOMING_SPEED_PAN); tiltMotor.setSpeed(HOMING_SPEED_TILT);
}

void processCmd(String cmd) {
    if (cmd.length() == 0) return;
    
    // --- [MỚI] XỬ LÝ LỆNH JOYSTICK TỪ PC CHUYỂN TIẾP VỀ ---
    // Dạng lệnh: "J:2048,2048,1,4095" (Pan, Tilt, BtnState, Speed)
    if (cmd.startsWith("J:")) {
        String params = cmd.substring(2); // Bỏ "J:"
        int firstComma = params.indexOf(',');
        int secondComma = params.indexOf(',', firstComma + 1);
        int thirdComma = params.indexOf(',', secondComma + 1);

        if (firstComma > 0 && secondComma > 0 && thirdComma > 0) {
             rawPanVal = params.substring(0, firstComma).toInt();
             rawTiltVal = params.substring(firstComma + 1, secondComma).toInt();
             int btnState = params.substring(secondComma + 1, thirdComma).toInt();
             rawSpeedPot = params.substring(thirdComma + 1).toInt();

             // Cập nhật trạng thái
             remoteGoHomeState = (btnState == 0); // 0 là nhấn
             lastJoystickTime = millis();

             // Tính toán tốc độ & Hướng (Logic cũ mapping lại)
             currentPanTargetSpeed = map(rawSpeedPot, 0, 4095, 500, (int)PAN_HARDWARE_MAX_SPEED);
             currentTiltTargetSpeed = map(rawSpeedPot, 0, 4095, 500, (int)TILT_HARDWARE_MAX_SPEED);

             if (rawPanVal > 2500) joyPanDir = 1; else if (rawPanVal < 1300) joyPanDir = -1; else joyPanDir = 0;
             if (rawTiltVal > 2500) joyTiltDir = 1; else if (rawTiltVal < 1300) joyTiltDir = -1; else joyTiltDir = 0;

             if ((joyPanDir != 0 || joyTiltDir != 0) && currentMode != MODE_HOMING) {
                 currentMode = MODE_JOYSTICK;
             }
        }
        return;
    }

    // Các lệnh cũ giữ nguyên
    if (currentMode == MODE_HOMING && cmd != "STOP") return;
    if (currentMode == MODE_JOYSTICK && !cmd.startsWith("J:")) { panMotor.stop(); tiltMotor.stop(); }

    if (cmd == "RESET_WIFI") {
       WiFiManager wm; wm.resetSettings(); ESP.restart();
    }
    else if (cmd.startsWith("H:")) { 
      String params = cmd.substring(2); int c = params.indexOf(',');
      userPanOffset = atol(params.substring(0, c).c_str()); 
      userTiltOffset = atol(params.substring(c+1).c_str());
      startHomingProcess();
    }
    else if (cmd.startsWith("M:")) { 
       String params = cmd.substring(2); int c = params.indexOf(',');
       currentMode = MODE_PC_AUTO;
       panMotor.setAcceleration(PAN_ACCEL_RUN); tiltMotor.setAcceleration(TILT_ACCEL_RUN);
       panMotor.setMaxSpeed(PAN_HARDWARE_MAX_SPEED); panMotor.moveTo(params.substring(0, c).toInt());
       tiltMotor.setMaxSpeed(TILT_HARDWARE_MAX_SPEED); tiltMotor.moveTo(params.substring(c+1).toInt());
    }
    else if (cmd == "R") { currentMode = MODE_PC_AUTO; panMotor.setAcceleration(PAN_ACCEL_RUN); panMotor.setMaxSpeed(PAN_HARDWARE_MAX_SPEED); panMotor.moveTo(PAN_MAX_LIMIT); }
    else if (cmd == "L") { currentMode = MODE_PC_AUTO; panMotor.setAcceleration(PAN_ACCEL_RUN); panMotor.setMaxSpeed(PAN_HARDWARE_MAX_SPEED); panMotor.moveTo(PAN_MIN_LIMIT); }
    else if (cmd == "U") { currentMode = MODE_PC_AUTO; tiltMotor.setAcceleration(TILT_ACCEL_RUN); tiltMotor.setMaxSpeed(TILT_HARDWARE_MAX_SPEED); tiltMotor.moveTo(TILT_MAX_LIMIT); }
    else if (cmd == "D") { currentMode = MODE_PC_AUTO; tiltMotor.setAcceleration(TILT_ACCEL_RUN); tiltMotor.setMaxSpeed(TILT_HARDWARE_MAX_SPEED); tiltMotor.moveTo(TILT_MIN_LIMIT); }
    else if (cmd == "STOP" || cmd == "P" || cmd == "T") { 
       currentMode = MODE_IDLE; panMotor.stop(); tiltMotor.stop(); 
    }
}

// Logic Click nút Home
void handleHomeButtonLogic() {
  unsigned long now = millis();
  if (remoteGoHomeState == true && lastHomeButtonState == false) { 
      homeClickCount++; lastHomeClickTime = now; 
  }
  lastHomeButtonState = remoteGoHomeState;

  if (homeClickCount >= 2) { 
      userPanOffset = 34800; userTiltOffset = 9600; startHomingProcess(); homeClickCount = 0; 
  }
  else if (homeClickCount == 1 && (now - lastHomeClickTime > DOUBLE_CLICK_DELAY)) { 
      currentMode = MODE_PC_AUTO;
      panMotor.setAcceleration(PAN_ACCEL_RUN); tiltMotor.setAcceleration(TILT_ACCEL_RUN);
      panMotor.setMaxSpeed(PAN_HARDWARE_MAX_SPEED); tiltMotor.setMaxSpeed(TILT_HARDWARE_MAX_SPEED);
      panMotor.moveTo(0); tiltMotor.moveTo(0); homeClickCount = 0; 
  }
}

// Logic Joystick chống trôi (Giữ nguyên)
void handleJoystickLogicCore1() {
  unsigned long now = millis();
  if (now - lastJoystickTime > 300) { // Nếu mất kết nối Joystick quá 300ms -> Dừng
      if (panMotor.isRunning()) panMotor.stop(); 
      if (tiltMotor.isRunning()) tiltMotor.stop(); 
      return; 
  }
  
  // Logic Pan
  if (joyPanDir == 0) { 
      if (!isPanStopping && panMotor.isRunning()) {
           panMotor.setAcceleration(PAN_ACCEL_STOP); panMotor.stop(); 
           isPanStopping = true; panStopTimestamp = now;
      }
      if (panMotor.distanceToGo() == 0) isPanStopping = false;
  } 
  else { 
      if (!isPanStopping || (now - panStopTimestamp > STOP_LOCK_TIME)) { 
          isPanStopping = false;
          panMotor.setAcceleration(PAN_ACCEL_RUN);
          panMotor.setMaxSpeed(currentPanTargetSpeed);
          if (joyPanDir == 1) { if (panMotor.currentPosition() < PAN_MAX_LIMIT) panMotor.moveTo(PAN_MAX_LIMIT); else panMotor.stop(); }
          else { if (panMotor.currentPosition() > PAN_MIN_LIMIT) panMotor.moveTo(PAN_MIN_LIMIT); else panMotor.stop(); }
      }
  }

  // Logic Tilt
  if (joyTiltDir == 0) { 
      if (!isTiltStopping && tiltMotor.isRunning()) {
           tiltMotor.setAcceleration(TILT_ACCEL_STOP); tiltMotor.stop(); 
           isTiltStopping = true; tiltStopTimestamp = now;
      }
      if (tiltMotor.distanceToGo() == 0) isTiltStopping = false;
  } 
  else { 
      if (!isTiltStopping || (now - tiltStopTimestamp > STOP_LOCK_TIME)) {
          isTiltStopping = false;
          tiltMotor.setAcceleration(TILT_ACCEL_RUN);
          tiltMotor.setMaxSpeed(currentTiltTargetSpeed);
          if (joyTiltDir == 1) { if (tiltMotor.currentPosition() < TILT_MAX_LIMIT) tiltMotor.moveTo(TILT_MAX_LIMIT); else tiltMotor.stop(); }
          else { if (tiltMotor.currentPosition() > TILT_MIN_LIMIT) tiltMotor.moveTo(TILT_MIN_LIMIT); else tiltMotor.stop(); }
      }
  }
}

void NetworkTask(void * pvParameters) {
  unsigned long lastSendTime = 0;
  unsigned long lastConnectAttempt = 0;
  static unsigned long btnPressStart = 0;

  for(;;) { 
      // Reset logic (Giữ nguyên)
      if (digitalRead(WIFI_RESET_BUTTON) == LOW) {
          if (btnPressStart == 0) btnPressStart = millis();
          if (millis() - btnPressStart > WIFI_RESET_HOLD_TIME) {
              panMotor.stop(); tiltMotor.stop();
              WiFiManager wm; wm.resetSettings(); delay(100); ESP.restart();
          }
      } else { btnPressStart = 0; }

      handleHomeButtonLogic();

      if (!tcpClient.connected()) {
          isConnectedToPC = false;
          if (millis() - lastConnectAttempt > 2000) {
              lastConnectAttempt = millis();
              if (tcpClient.connect(PC_SERVER_IP, PC_SERVER_PORT)) {
                  isConnectedToPC = true;
                  tcpClient.setNoDelay(true); 
              }
          }
      } else {
          isConnectedToPC = true;
          while (tcpClient.available()) {
              String cmd = tcpClient.readStringUntil('\n'); cmd.trim(); processCmd(cmd);
          }
      }

      if (isConnectedToPC && (millis() - lastSendTime >= 200)) { 
          lastSendTime = millis(); 
          char buffer[50]; 
          sprintf(buffer, "P:%ld,T:%ld\n", panMotor.currentPosition(), tiltMotor.currentPosition());
          tcpClient.print(buffer); 
      }
      vTaskDelay(10 / portTICK_PERIOD_MS); 
  }
}

void runHoming() {
  if (homingState == 1) {
    if (!panHomingDone) { 
        if (digitalRead(PAN_HOME_PIN) == HIGH) { panMotor.setSpeed(0); panHomingDone = true; } else { panMotor.runSpeed(); } 
    }
    if (!tiltHomingDone) { 
        if (digitalRead(TILT_HOME_PIN) == HIGH) { tiltMotor.setSpeed(0); tiltHomingDone = true; } else { tiltMotor.runSpeed(); } 
    }
    if (panHomingDone && tiltHomingDone) { 
        panMotor.stop(); tiltMotor.stop(); panMotor.setCurrentPosition(0); tiltMotor.setCurrentPosition(0);
        homingWaitStart = millis(); homingState = 2; 
    }
  }
  else if (homingState == 2) {
    if (millis() - homingWaitStart >= 500) { 
       panMotor.setAcceleration(PAN_ACCEL_RUN); tiltMotor.setAcceleration(TILT_ACCEL_RUN);
       float dp = abs(userPanOffset), dt = abs(userTiltOffset);
       if (dp > 0 || dt > 0) {
           panMotor.setMaxSpeed(PAN_HARDWARE_MAX_SPEED); tiltMotor.setMaxSpeed(TILT_HARDWARE_MAX_SPEED);
           panMotor.moveTo(-userPanOffset); tiltMotor.moveTo(-userTiltOffset); 
           homingState = 3; 
       } else { 
           currentMode = MODE_IDLE; 
           if (isConnectedToPC) tcpClient.print("HOME_DONE\n");
       }
    }
  }
  else if (homingState == 3) {
    if (!panMotor.isRunning() && !tiltMotor.isRunning()) {
      panMotor.setCurrentPosition(0); tiltMotor.setCurrentPosition(0); 
      currentMode = MODE_IDLE; 
      if (isConnectedToPC) tcpClient.print("HOME_DONE\n"); 
    } else { panMotor.run(); tiltMotor.run(); }
  }
}

void setup() {
  Serial.begin(115200);
  pinMode(WIFI_RESET_BUTTON, INPUT_PULLUP);
  pinMode(EN_PIN, OUTPUT); digitalWrite(EN_PIN, LOW); 
  pinMode(PAN_HOME_PIN, INPUT_PULLUP); pinMode(TILT_HOME_PIN, INPUT_PULLUP);
  
  panMotor.setMaxSpeed(PAN_HARDWARE_MAX_SPEED); panMotor.setAcceleration(PAN_ACCEL_RUN);
  tiltMotor.setMaxSpeed(TILT_HARDWARE_MAX_SPEED); tiltMotor.setAcceleration(TILT_ACCEL_RUN);
  
  WiFiManager wm;
  bool res = wm.autoConnect("PTU_CAM_SETUP", "12345678"); 
  if(!res) { ESP.restart(); } 
  
  WiFi.mode(WIFI_AP_STA); 
  
  xTaskCreatePinnedToCore(NetworkTask, "NetTask", 10000, NULL, 1, &TaskNetworkHandle, 0);
}

void loop() {
  if (currentMode == MODE_HOMING) { runHoming(); }
  else if (currentMode == MODE_JOYSTICK) { handleJoystickLogicCore1(); panMotor.run(); tiltMotor.run(); }
  else { panMotor.run(); tiltMotor.run(); }
}
