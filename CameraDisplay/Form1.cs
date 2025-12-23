using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Net.Http;
using System.Collections.Concurrent;

// Thư viện bên thứ ba
using OpenCvSharp;
using OpenCvSharp.Extensions;
using NAudio.Wave;

namespace CameraDisplay
{
    public partial class up : Form
    {
        #region 1. Fields & Constants

        // --- HTTP CLIENT (Cho Zoom) ---
        private static readonly HttpClient httpClient = new HttpClient();

        // --- TCP/IP SERVER ---
        private TcpListener tcpServer;
        private Thread serverThread;
        private volatile bool isServerRunning = false;
        private const int SERVER_PORT = 8888;

        // --- Client Management ---
        private List<TcpClient> connectedClients = new List<TcpClient>();
        private object clientLock = new object();

        // --- Camera & Vision ---
        private VideoCapture capture;
        private VideoWriter videoWriter;
        private Mat currentFrame;

        // --- Threading & FPS Control ---
        private Task cameraLoopTask;
        private bool isCameraRunning;
        private double actualVideoFps;
        private double currentTargetFps;

        // Biến dùng để giới hạn tốc độ vẽ lên UI (tránh lag giao diện)
        private long lastDrawTime = 0;
        private const long MIN_DRAW_INTERVAL_MS = 33; // ~30 FPS cho hiển thị

        // --- Recording (ĐÃ CẢI TIẾN) ---
        private bool isRecording = false;
        private Stopwatch stopwatch;
        private System.Windows.Forms.Timer recordingTimerUi;
        private long recordedFrameCount;

        // Queue xử lý ghi đĩa để không làm lag luồng hình ảnh
        private ConcurrentQueue<Mat> recordingQueue;
        private Task diskWritingTask;
        private volatile bool isWritingActive = false;

        // --- Audio ---
        private WaveInEvent waveSource;
        private WaveFileWriter waveFile;
        private string tempAudioFilePath;
        private string tempVideoFilePath;
        private string finalVideoPath;

        // --- Keyboard ---
        private bool isUpPressed = false;
        private bool isDownPressed = false;
        private bool isLeftPressed = false;
        private bool isRightPressed = false;

        // --- BIẾN MỚI: XỬ LÝ ZOOM TỪ TAY CẦM ---
        private int lastZoomValue = -1; // Lưu giá trị cũ để so sánh
        private long lastZoomTime = 0;  // Lưu thời gian để giới hạn tốc độ gửi lệnh HTTP

        #endregion

        // -------------------------------------------------------------------------

        #region 2. Constructor & Lifecycle

        public up()
        {
            InitializeComponent();
            this.KeyPreview = true;
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            ShowLocalIPAddress();
            LoadWebcamDevices();
            LoadAudioDevices();

            numPanOffset.Maximum = 1000000;
            numTiltOffset.Maximum = 1000000;
            numPanOffset.Value = 38400;
            numTiltOffset.Value = 9600;

            SetMotorControlButtonsEnabled(false);
            btnCapture.Enabled = false;
            btnRecord.Enabled = false;
            btnConnect.Text = "Start Server";

            if (rbIp != null) rbIp.Checked = true;

            // Tự động điền link IP Camera
            txtIpAddress.Text = "http://192.168.137.35:8080/video";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 1. Ngắt cờ hoạt động
            isCameraRunning = false;
            isServerRunning = false;
            isRecording = false;
            isWritingActive = false;

            // 2. Dừng Server & Camera
            StopServer();
            try
            {
                if (waveSource != null) { waveSource.StopRecording(); waveSource.Dispose(); }
                waveFile?.Dispose();
                capture?.Release(); capture?.Dispose();
            }
            catch { }

            // 3. Kill process để tắt sạch sẽ
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        #endregion

        // -------------------------------------------------------------------------

        #region 3. TCP/IP SERVER LOGIC (UPDATE ZOOM)

        void ShowLocalIPAddress()
        {
            cboPorts.Items.Clear();
            string localIP = "Not Found";
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        cboPorts.Items.Add(localIP);
                    }
                }
            }
            catch { }

            if (cboPorts.Items.Count > 0) cboPorts.SelectedIndex = 0;
            lblStatus.Text = "IP của bạn: " + localIP;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!isServerRunning)
            {
                try
                {
                    isServerRunning = true;
                    tcpServer = new TcpListener(IPAddress.Any, SERVER_PORT);
                    tcpServer.Start();

                    serverThread = new Thread(ServerListeningLoop);
                    serverThread.IsBackground = true;
                    serverThread.Start();

                    btnConnect.Text = "Stop Server";
                    btnConnect.ForeColor = Color.Red;
                    lblStatus.Text = "Server đang chạy...";
                    lblStatus.ForeColor = Color.Blue;
                    cboPorts.Enabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi Server: " + ex.Message);
                    isServerRunning = false;
                }
            }
            else
            {
                StopServer();
            }
        }

        private void StopServer()
        {
            isServerRunning = false;
            try
            {
                if (tcpServer != null) tcpServer.Stop();
            }
            catch { }

            lock (clientLock)
            {
                foreach (var client in connectedClients) client.Close();
                connectedClients.Clear();
            }

            Action updateUI = () =>
            {
                btnConnect.Text = "Start Server";
                btnConnect.ForeColor = Color.Black;
                lblStatus.Text = "Server Stopped";
                cboPorts.Enabled = true;
                SetMotorControlButtonsEnabled(false);
            };

            if (btnConnect.InvokeRequired) btnConnect.Invoke(updateUI);
            else updateUI();
        }

        private void ServerListeningLoop()
        {
            while (isServerRunning)
            {
                try
                {
                    TcpClient newClient = tcpServer.AcceptTcpClient();
                    lock (clientLock) { connectedClients.Add(newClient); }

                    Thread clientThread = new Thread(() => HandleClient(newClient));
                    clientThread.IsBackground = true;
                    clientThread.Start();

                    this.BeginInvoke(new Action(() => {
                        lblStatus.Text = $"Clients: {connectedClients.Count}";
                        SetMotorControlButtonsEnabled(true);
                    }));
                }
                catch { }
            }
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = null;
            byte[] buffer = new byte[1024];

            try
            {
                stream = client.GetStream();
                while (isServerRunning && client.Connected)
                {
                    if (stream.DataAvailable)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;

                        string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        string[] lines = data.Split('\n');

                        foreach (var line in lines)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                string cleanLine = line.Trim();

                                // === CẬP NHẬT: XỬ LÝ LỆNH TỪ TAY CẦM ===
                                if (cleanLine.StartsWith("J:"))
                                {
                                    // 1. Vẫn gửi lệnh cho module điều khiển động cơ (ESP32 nhận lại lệnh này nếu cần, hoặc bỏ qua)
                                    // Lưu ý: Nếu tay cầm nối thẳng ESP32 điều khiển motor thì ESP32 đã xử lý rồi.
                                    // Nhưng nếu ông dùng mô hình: Tay cầm -> PC -> Cụm PTU (ESP32 khác) thì dòng dưới là cần thiết.
                                    BroadcastCommand(cleanLine, client);

                                    // 2. Xử lý Zoom ngay trên PC (Gửi lệnh HTTP tới Camera IP)
                                    // Dùng BeginInvoke để chạy trên luồng UI (an toàn cho biến toàn cục và giao diện)
                                    this.BeginInvoke(new Action(() => ProcessRemoteZoom(cleanLine)));
                                }
                                else
                                {
                                    this.BeginInvoke(new Action(() => ProcessReceivedData(cleanLine)));
                                }
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            catch { }
            finally
            {
                lock (clientLock) { connectedClients.Remove(client); }
                this.BeginInvoke(new Action(() => {
                    lblStatus.Text = $"Clients: {connectedClients.Count}";
                    if (connectedClients.Count == 0) SetMotorControlButtonsEnabled(false);
                }));
            }
        }

        // --- HÀM MỚI: XỬ LÝ ZOOM TỪ DỮ LIỆU TAY CẦM ---
        private async void ProcessRemoteZoom(string data)
        {
            // Format mong đợi: J:pan,tilt,btn,speed,zoom
            // Ví dụ: J:2048,2048,1,4095,1500
            try
            {
                string content = data.Substring(2); // Bỏ chữ "J:"
                string[] parts = content.Split(',');

                // Phải có đủ 5 phần tử thì mới có Zoom
                if (parts.Length < 5) return;

                // Lấy giá trị Zoom (phần tử thứ 4 - tính từ 0)
                if (int.TryParse(parts[4], out int rawZoom))
                {
                    // rawZoom từ ADC ESP32 là 0 -> 4095
                    // Quy đổi sang thang đo của thanh trượt tbZoom (lấy Max/Min thực tế từ giao diện)
                    int maxZoomUI = tbZoom.Maximum;
                    int minZoomUI = tbZoom.Minimum;

                    // Công thức map giá trị:
                    int newZoomLevel = (int)((rawZoom / 4095.0) * (maxZoomUI - minZoomUI)) + minZoomUI;

                    // --- CHỐNG SPAM LỆNH HTTP (QUAN TRỌNG) ---
                    // Chỉ gửi lệnh nếu:
                    // 1. Giá trị thay đổi đủ lớn (> 2 đơn vị) 
                    // 2. HOẶC đã quá 200ms từ lần gửi trước (để cập nhật trạng thái tĩnh)
                    long now = DateTime.Now.Ticks / 10000; // ms

                    if (Math.Abs(newZoomLevel - lastZoomValue) > 2 && (now - lastZoomTime > 200))
                    {
                        lastZoomValue = newZoomLevel;
                        lastZoomTime = now;

                        // Cập nhật thanh trượt trên giao diện
                        tbZoom.Value = newZoomLevel;

                        // Gửi lệnh HTTP tới Camera
                        string rawUrl = txtIpAddress.Text.Trim();
                        if (!string.IsNullOrEmpty(rawUrl))
                        {
                            string baseUrl = rawUrl;
                            // Xử lý link để lấy base URL (bỏ đuôi /video)
                            if (baseUrl.EndsWith("/video")) baseUrl = baseUrl.Replace("/video", "");
                            if (baseUrl.EndsWith("/video/")) baseUrl = baseUrl.Replace("/video/", "");

                            // Tạo link lệnh zoom
                            string commandUrl = $"{baseUrl}/ptz?zoom={newZoomLevel}";

                            // Gửi bất đồng bộ (Async) để không đơ ứng dụng
                            try
                            {
                                await httpClient.GetAsync(commandUrl);
                            }
                            catch { } // Bỏ qua lỗi kết nối để app không crash
                        }
                    }
                }
            }
            catch { }
        }

        private void BroadcastCommand(string command, TcpClient senderToIgnore = null)
        {
            if (!command.EndsWith("\n")) command += "\n";
            byte[] dataBytes = Encoding.ASCII.GetBytes(command);

            lock (clientLock)
            {
                foreach (var client in connectedClients)
                {
                    if (client != senderToIgnore && client.Connected)
                    {
                        try
                        {
                            NetworkStream stream = client.GetStream();
                            if (stream.CanWrite) stream.Write(dataBytes, 0, dataBytes.Length);
                        }
                        catch { }
                    }
                }
            }
        }

        private void SendCommand(string command) => BroadcastCommand(command, null);

        private void ProcessReceivedData(string data)
        {
            try
            {
                if (data.Contains("HOME_DONE"))
                {
                    lblStatus.Text = "HOMING COMPLETE!";
                    lblStatus.ForeColor = Color.Green;
                }
                else if (data.StartsWith("P:") || data.Contains("T:"))
                {
                    string[] parts = data.Split(',');
                    foreach (var part in parts)
                    {
                        if (part.StartsWith("P:")) lblPanPos.Text = "Pan: " + part.Substring(2);
                        else if (part.StartsWith("T:")) lblTiltPos.Text = "Tilt: " + part.Substring(2);
                    }
                }
            }
            catch { }
        }

        private void SetMotorControlButtonsEnabled(bool isEnabled)
        {
            right.Enabled = isEnabled;
            left.Enabled = isEnabled;
            down.Enabled = isEnabled;
            upp.Enabled = isEnabled;
            Home.Enabled = isEnabled;
        }

        private void Refresh2_Click(object sender, EventArgs e) => ShowLocalIPAddress();

        #endregion

        // -------------------------------------------------------------------------

        #region 4. Motor Control Logic

        private void Home_Click(object sender, EventArgs e)
        {
            long panOffsetValue = (long)numPanOffset.Value;
            long tiltOffsetValue = (long)numTiltOffset.Value;
            string command = $"H:{panOffsetValue},{tiltOffsetValue}\n";
            SendCommand(command);
            lblStatus.Text = "Đang về Home...";
            lblStatus.ForeColor = Color.Blue;
        }

        private void right_MouseDown(object sender, MouseEventArgs e) { SendCommand("L\n"); }
        private void right_MouseUp(object sender, MouseEventArgs e) { SendCommand("P\n"); }
        private void left_MouseDown(object sender, MouseEventArgs e) { SendCommand("R\n"); }
        private void left_MouseUp(object sender, MouseEventArgs e) { SendCommand("P\n"); }
        private void down_MouseDown(object sender, MouseEventArgs e) { SendCommand("D\n"); }
        private void down_MouseUp(object sender, MouseEventArgs e) { SendCommand("T\n"); }
        private void upp_MouseDown(object sender, MouseEventArgs e) { SendCommand("U\n"); }
        private void upp_MouseUp(object sender, MouseEventArgs e) { SendCommand("T\n"); }

        private void up_KeyDown(object sender, KeyEventArgs e)
        {
            if (connectedClients.Count == 0) return;
            switch (e.KeyCode)
            {
                case Keys.Right: if (!isRightPressed) { isRightPressed = true; SendCommand("L\n"); } e.Handled = true; break;
                case Keys.Left: if (!isLeftPressed) { isLeftPressed = true; SendCommand("R\n"); } e.Handled = true; break;
                case Keys.Up: if (!isUpPressed) { isUpPressed = true; SendCommand("U\n"); } e.Handled = true; break;
                case Keys.Down: if (!isDownPressed) { isDownPressed = true; SendCommand("D\n"); } e.Handled = true; break;
            }
        }

        private void up_KeyUp(object sender, KeyEventArgs e)
        {
            if (connectedClients.Count == 0) return;
            switch (e.KeyCode)
            {
                case Keys.Right: isRightPressed = false; SendCommand("P\n"); break;
                case Keys.Left: isLeftPressed = false; SendCommand("P\n"); break;
                case Keys.Up: isUpPressed = false; SendCommand("T\n"); break;
                case Keys.Down: isDownPressed = false; SendCommand("T\n"); break;
            }
        }

        private void btnGoHome_Click_1(object sender, EventArgs e) => SendCommand("M:0,0\n");

        #endregion

        // -------------------------------------------------------------------------

        #region 5. Camera & Recording & ZOOM Logic (TỐI ƯU TỐC ĐỘ LƯU)

        // --- CÁC BIẾN CHO FFMPEG PIPING ---
        private Process ffmpegProcess;
        private Stream ffmpegInputStream;

        void LoadWebcamDevices()
        {
            cboDevices.Items.Clear();
            for (int i = 0; i < 5; i++)
            {
                VideoCapture tempCapture = new VideoCapture(i);
                if (tempCapture.IsOpened()) { cboDevices.Items.Add($"Camera {i}"); tempCapture.Release(); }
                tempCapture.Dispose();
            }
            if (cboDevices.Items.Count > 0) cboDevices.SelectedIndex = 0;
            else MessageBox.Show("Không tìm thấy webcam!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        void LoadAudioDevices()
        {
            if (cboAudioDevices == null) return;
            cboAudioDevices.Items.Clear();
            if (WaveIn.DeviceCount == 0) { cboAudioDevices.Items.Add("Không tìm thấy micro"); cboAudioDevices.Enabled = false; }
            else
            {
                for (int i = 0; i < WaveIn.DeviceCount; i++)
                {
                    var caps = WaveIn.GetCapabilities(i);
                    cboAudioDevices.Items.Add(caps.ProductName);
                }
                cboAudioDevices.SelectedIndex = 0;
                cboAudioDevices.Enabled = true;
            }
        }

        private void Start_Click(object sender, EventArgs e)
        {
            if (capture != null && capture.IsOpened()) Stop_Click(sender, e);

            try
            {
                if (rbIp.Checked)
                {
                    string url = txtIpAddress.Text.Trim();
                    if (string.IsNullOrEmpty(url)) { MessageBox.Show("Chưa nhập link IP Camera"); return; }
                    if (!url.EndsWith("/video") && !url.EndsWith("/video/") && !url.Contains(".")) url += "/video";
                    Environment.SetEnvironmentVariable("OPENCV_FFMPEG_CAPTURE_OPTIONS", "fflags;nobuffer|allow_lag;1|minimize_latency;1");
                    capture = new VideoCapture(url, VideoCaptureAPIs.FFMPEG);
                    capture.Set(VideoCaptureProperties.BufferSize, 0);
                }
                else
                {
                    if (cboDevices.SelectedItem == null) { MessageBox.Show("Vui lòng chọn webcam."); return; }
                    capture = new VideoCapture(cboDevices.SelectedIndex, VideoCaptureAPIs.DSHOW);
                    if (!capture.IsOpened()) capture = new VideoCapture(cboDevices.SelectedIndex);
                }

                if (capture == null || !capture.IsOpened()) { MessageBox.Show("Không kết nối được Camera!"); return; }

                double webcamFps = capture.Get(VideoCaptureProperties.Fps);
                this.currentTargetFps = (webcamFps < 10) ? 30.0 : webcamFps;

                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                currentFrame = new Mat();
                isCameraRunning = true;
                cameraLoopTask = Task.Run(() => CameraLoop());

                btnCapture.Enabled = true;
                btnRecord.Enabled = true;
                if (!isRecording) btnRecord.Text = "Record";
                Stop.Enabled = true;
                Start.Enabled = false;
                cboDevices.Enabled = false;
                txtIpAddress.Enabled = false;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi khởi động: " + ex.Message); }
        }

        private void CameraLoop()
        {
            Stopwatch frameTimer = new Stopwatch();
            double frameTimeMs = 1000.0 / this.currentTargetFps;

            while (isCameraRunning)
            {
                try
                {
                    if (capture == null || !capture.IsOpened()) break;

                    frameTimer.Restart();
                    capture.Read(currentFrame);

                    if (currentFrame.Empty()) { Thread.Sleep(5); continue; }

                    // --- ĐẨY ẢNH VÀO QUEUE ---
                    if (isRecording && isWritingActive)
                    {
                        // Giới hạn Queue để tránh tràn RAM nếu máy quá chậm
                        if (recordingQueue.Count < 300)
                        {
                            recordingQueue.Enqueue(currentFrame.Clone());
                        }
                    }

                    // --- VẼ LÊN UI ---
                    long currentTime = DateTime.Now.Ticks / 10000;
                    if (currentTime - lastDrawTime > MIN_DRAW_INTERVAL_MS)
                    {
                        lastDrawTime = currentTime;
                        using (Bitmap bmp = BitmapConverter.ToBitmap(currentFrame))
                        {
                            Image imageForUi = (Image)bmp.Clone();
                            if (this.IsDisposed || pictureBox1.IsDisposed) return;
                            pictureBox1.BeginInvoke(new Action(() => {
                                try { var old = pictureBox1.Image; pictureBox1.Image = imageForUi; old?.Dispose(); }
                                catch { imageForUi?.Dispose(); }
                            }));
                        }
                    }

                    frameTimer.Stop();
                    if (rbIp.Checked) Thread.Sleep(1);
                    else
                    {
                        int delay = (int)(frameTimeMs - frameTimer.ElapsedMilliseconds);
                        if (delay > 0) Thread.Sleep(delay);
                    }
                }
                catch { }
            }
        }

        // --- GHI THẲNG VÀO FFMPEG (PIPING) ---
        private void WriteFramesToDiskLoop()
        {
            byte[] frameBuffer = null;

            while (isWritingActive || !recordingQueue.IsEmpty)
            {
                if (recordingQueue.TryDequeue(out Mat frameToWrite))
                {
                    try
                    {
                        if (ffmpegInputStream != null && ffmpegInputStream.CanWrite)
                        {
                            // Resize nếu cần (để đảm bảo size chẵn cho codec h264)
                            // Ở đây ta giả sử kích thước camera ok. Nếu lỗi "width not divisible by 2" thì phải resize.

                            // Convert Mat sang mảng byte RAW (BGR)
                            // Cách nhanh nhất là copy memory, không nén JPEG
                            int dataSize = (int)(frameToWrite.Total() * frameToWrite.ElemSize());
                            if (frameBuffer == null || frameBuffer.Length != dataSize)
                            {
                                frameBuffer = new byte[dataSize];
                            }

                            // Copy dữ liệu pixel thô
                            System.Runtime.InteropServices.Marshal.Copy(frameToWrite.Data, frameBuffer, 0, dataSize);

                            // Ghi thẳng vào đường ống của FFMPEG
                            ffmpegInputStream.Write(frameBuffer, 0, dataSize);

                            recordedFrameCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Pipe Error: " + ex.Message);
                    }
                    finally
                    {
                        frameToWrite.Dispose();
                    }
                }
                else
                {
                    Thread.Sleep(1); // Nghỉ nếu queue rỗng
                }
            }
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            if (isRecording) btnRecord_Click(sender, e);
            isCameraRunning = false;
            cameraLoopTask?.Wait(1000);
            capture?.Release(); capture = null;
            currentFrame?.Dispose(); currentFrame = null;
            btnCapture.Enabled = false; btnRecord.Enabled = false;
            Stop.Enabled = false; Start.Enabled = true; cboDevices.Enabled = true; txtIpAddress.Enabled = true;
        }

        // --- ZOOM LOGIC ---
        private async void tbZoom_Scroll(object sender, EventArgs e)
        {
            int zoomLevel = tbZoom.Value;
            string rawUrl = txtIpAddress.Text.Trim();
            if (string.IsNullOrEmpty(rawUrl)) return;
            string baseUrl = rawUrl.Replace("/video", "").Replace("/video/", "");
            string commandUrl = $"{baseUrl}/ptz?zoom={zoomLevel}";
            try { await httpClient.GetAsync(commandUrl); } catch { }
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSavePath.Text)) { MessageBox.Show("Chọn thư mục lưu trước!"); return; }
            if (currentFrame == null || currentFrame.Empty()) return;
            try
            {
                string fullPath = Path.Combine(txtSavePath.Text, $"capture_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
                currentFrame.SaveImage(fullPath);
                MessageBox.Show("Lưu ảnh thành công!");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        // --- RECORDING LOGIC (ĐÃ SỬA: DIRECT FFMPEG PIPING) ---
        private void btnRecord_Click(object sender, EventArgs e)
        {
            if (!isRecording)
            {
                // START
                if (!ValidateRecordingPreconditions()) return;
                try
                {
                    // Tên file Video cuối cùng (không tiếng, ta sẽ merge tiếng sau hoặc pipe tiếng luôn nếu phức tạp)
                    // Ở đây để đơn giản và nhanh: Ta ghi Video ra file .mp4 (không tiếng) trước bằng Pipe.
                    // Ghi Audio ra file .wav riêng.
                    // Lúc dừng thì Merge lại (nhanh hơn nhiều so với convert AVI).

                    string baseFileName = $"record_{DateTime.Now:yyyyMMdd_HHmmss}";
                    finalVideoPath = Path.Combine(txtSavePath.Text, baseFileName + ".mp4");
                    tempVideoFilePath = Path.Combine(Path.GetTempPath(), baseFileName + "_video_only.mp4");
                    tempAudioFilePath = Path.Combine(Path.GetTempPath(), baseFileName + "_audio.wav");

                    // 1. Khởi tạo FFMPEG Process để nhận luồng Video
                    StartFfmpegProcess(tempVideoFilePath, capture.FrameWidth, capture.FrameHeight, currentTargetFps);

                    // 2. Khởi tạo Audio Recorder
                    waveSource = new WaveInEvent { DeviceNumber = cboAudioDevices.SelectedIndex, WaveFormat = new WaveFormat(44100, 1) };
                    waveFile = new WaveFileWriter(tempAudioFilePath, waveSource.WaveFormat);
                    waveSource.DataAvailable += (s, a) => { waveFile?.Write(a.Buffer, 0, a.BytesRecorded); };
                    waveSource.StartRecording();

                    // 3. Chạy luồng ghi hình
                    recordingQueue = new ConcurrentQueue<Mat>();
                    recordedFrameCount = 0;
                    isWritingActive = true;
                    diskWritingTask = Task.Run(() => WriteFramesToDiskLoop());

                    stopwatch = new Stopwatch(); stopwatch.Restart();
                    recordingTimerUi = new System.Windows.Forms.Timer { Interval = 1000 };
                    recordingTimerUi.Tick += RecordingTimerUi_Tick;
                    recordingTimerUi.Start();

                    isRecording = true;
                    btnRecord.Text = "Stop Record";
                    time.Text = "00:00:00";
                    ToggleUiControlsForRecording(false);
                }
                catch (Exception ex) { MessageBox.Show("Lỗi Start Record: " + ex.Message); CleanupRecordingResources(); }
            }
            else
            {
                // STOP
                FinalizeRecording();
            }
        }

        private void StartFfmpegProcess(string outputPath, int width, int height, double fps)
        {
            string ffmpegPath = Path.Combine(Application.StartupPath, "ffmpeg.exe");

            // Lệnh quan trọng: Đọc raw video từ stdin -> nén h264 ultrafast -> ghi ra mp4
            // -f rawvideo: Định dạng đầu vào là thô
            // -pix_fmt bgr24: OpenCV dùng BGR
            // -s {width}x{height}: Kích thước ảnh
            // -r {fps}: Tốc độ khung hình
            // -i -: Đọc từ Pipe (stdin)
            // -c:v libx264 -preset ultrafast: Nén tốc độ cao nhất (file lớn chút nhưng cực nhanh)

            string args = $"-y -f rawvideo -vcodec rawvideo -pix_fmt bgr24 -s {width}x{height} -r {fps} -i - -c:v libx264 -preset ultrafast -pix_fmt yuv420p \"{outputPath}\"";

            ffmpegProcess = new Process();
            ffmpegProcess.StartInfo.FileName = ffmpegPath;
            ffmpegProcess.StartInfo.Arguments = args;
            ffmpegProcess.StartInfo.UseShellExecute = false;
            ffmpegProcess.StartInfo.RedirectStandardInput = true; // Cho phép bơm dữ liệu vào
            ffmpegProcess.StartInfo.CreateNoWindow = true;

            ffmpegProcess.Start();
            ffmpegInputStream = ffmpegProcess.StandardInput.BaseStream;
        }

        private void FinalizeRecording()
        {
            this.Cursor = Cursors.WaitCursor;
            time.Text = "Stopping...";
            Application.DoEvents(); // Cập nhật UI

            isRecording = false;
            isWritingActive = false;

            stopwatch?.Stop();
            recordingTimerUi?.Stop();

            // 1. Dừng nhận input vào FFMPEG Video
            diskWritingTask?.Wait(2000); // Đợi nốt các frame trong queue

            try
            {
                ffmpegInputStream?.Flush();
                ffmpegInputStream?.Close(); // Đóng ống nước -> FFMPEG tự hiểu là hết phim và finalize file mp4
            }
            catch { }

            ffmpegProcess?.WaitForExit(5000); // Đợi FFMPEG đóng gói xong file video (thường chỉ mất 1-2s)
            if (ffmpegProcess != null && !ffmpegProcess.HasExited) ffmpegProcess.Kill();

            // 2. Dừng Audio
            waveSource?.StopRecording();
            waveSource?.Dispose(); waveSource = null;
            waveFile?.Dispose(); waveFile = null;

            // 3. Merge Audio và Video (Bước này cực nhanh vì video đã nén rồi, chỉ muxing thôi)
            time.Text = "Merging...";
            Application.DoEvents();

            try
            {
                // Tính toán FPS thực tế (để khớp tiếng nếu cần, nhưng ở đây ta tin vào FPS cài đặt vì dùng Pipe chuẩn)
                MergeAudioVideo(tempVideoFilePath, tempAudioFilePath, finalVideoPath);

                // Dọn dẹp file rác
                if (File.Exists(tempVideoFilePath)) File.Delete(tempVideoFilePath);
                if (File.Exists(tempAudioFilePath)) File.Delete(tempAudioFilePath);

                MessageBox.Show($"Đã lưu: {Path.GetFileName(finalVideoPath)}");
                time.Text = "Done";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi Merge: " + ex.Message);
            }
            finally
            {
                CleanupRecordingResources();
                this.Cursor = Cursors.Default;
                btnRecord.Text = "Record";
                ToggleUiControlsForRecording(true);
            }
        }

        private void MergeAudioVideo(string videoPath, string audioPath, string outputPath)
        {
            string ffmpegPath = Path.Combine(Application.StartupPath, "ffmpeg.exe");
            // Lệnh copy stream (không encode lại) -> SIÊU NHANH
            // -c:v copy -c:a aac: Copy video, nén audio sang aac
            string args = $"-y -i \"{videoPath}\" -i \"{audioPath}\" -c:v copy -c:a aac -shortest \"{outputPath}\"";

            using (Process process = Process.Start(new ProcessStartInfo { FileName = ffmpegPath, Arguments = args, UseShellExecute = false, CreateNoWindow = true }))
            {
                process.WaitForExit();
            }
        }

        private bool ValidateRecordingPreconditions()
        {
            if (string.IsNullOrWhiteSpace(txtSavePath.Text)) { MessageBox.Show("Chọn thư mục lưu."); return false; }
            if (capture == null || !capture.IsOpened()) { MessageBox.Show("Chưa bật Camera."); return false; }
            if (cboAudioDevices.SelectedIndex < 0) { MessageBox.Show("Chưa chọn Mic."); return false; }
            return true;
        }

        private void ToggleUiControlsForRecording(bool isEnabled)
        {
            Start.Enabled = isEnabled && !Stop.Enabled;
            Stop.Enabled = !isEnabled;
            if (!isEnabled) { Start.Enabled = false; Stop.Enabled = false; cboDevices.Enabled = false; cboAudioDevices.Enabled = false; btnChooseFolder.Enabled = false; }
            else { if (capture != null && capture.IsOpened()) Stop.Enabled = true; Start.Enabled = !Stop.Enabled; cboDevices.Enabled = !Stop.Enabled; cboAudioDevices.Enabled = true; btnChooseFolder.Enabled = true; }
        }

        private void CleanupRecordingResources()
        {
            recordingTimerUi?.Dispose(); recordingTimerUi = null;
            ffmpegProcess?.Dispose(); ffmpegProcess = null;
            ffmpegInputStream = null;
            // Xả queue nếu còn
            while (recordingQueue != null && !recordingQueue.IsEmpty) { if (recordingQueue.TryDequeue(out Mat f)) f.Dispose(); }
        }

        private void RecordingTimerUi_Tick(object sender, EventArgs e) { if (stopwatch != null) time.Text = $"{stopwatch.Elapsed:hh\\:mm\\:ss}"; }
        private void btnChooseFolder_Click(object sender, EventArgs e) { using (var d = new FolderBrowserDialog()) if (d.ShowDialog() == DialogResult.OK) txtSavePath.Text = d.SelectedPath; }
        private void Refresh_Click(object sender, EventArgs e) { LoadWebcamDevices(); LoadAudioDevices(); ShowLocalIPAddress(); }

        #endregion


        // 6. Empty Events
        private void cboDevices_SelectedIndexChanged(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void right_Click(object sender, EventArgs e) { }
        private void left_Click(object sender, EventArgs e) { }
        private void down_Click(object sender, EventArgs e) { }
        private void upp_Click(object sender, EventArgs e) { }
        private void lblStatus_Click(object sender, EventArgs e) { }
        private void time_TextChanged(object sender, EventArgs e) { }
        private void cboPorts_SelectedIndexChanged(object sender, EventArgs e) { }
        private void lblPanPos_Click(object sender, EventArgs e) { }
        private void lblTiltPos_Click(object sender, EventArgs e) { }
        private void numPanOffset_ValueChanged(object sender, EventArgs e) { }
        private void numTiltOffset_ValueChanged(object sender, EventArgs e) { }
        private void rbUsb_CheckedChanged(object sender, EventArgs e) { }
        private void rbIp_CheckedChanged(object sender, EventArgs e) { }
        private void txtIpAddress_TextChanged(object sender, EventArgs e) { }

    }
}