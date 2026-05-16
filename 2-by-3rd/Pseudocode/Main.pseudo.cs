// Orchestrator utama untuk aplikasi companion attendance

public class MainProgram
{
    private ProcessMonitor processMonitor;
    private ScreenEngine screenEngine;
    private CloudSync cloudSync;
    private Random random;
    private int lastKnownProcessId = -1;
    
    public MainProgram()
    {
        // Inisialisasi semua komponen
        this.processMonitor = new ProcessMonitor(Config.TargetProcessName);
        this.screenEngine = new ScreenEngine(Config.CaptureWidth, Config.CaptureHeight);
        this.cloudSync = new CloudSync(Config.SupabaseUrl, Config.SupabaseKey);
        this.random = new Random();
        
        Log("Aplikasi companion attendance dimulai");
    }
    
    // Main loop orchestrator
    public async Task Run()
    {
        while (true)
        {
            try
            {
                // Step 1: Cek apakah FiveM berjalan
                if (!processMonitor.IsFiveMRunning())
                {
                    Log("FiveM tidak berjalan, menunggu...");
                    await Task.Delay(5000); // Tunggu 5 detik sebelum cek lagi
                    continue;
                }
                
                // Step 2: Dapatkan process ID
                int currentPid = processMonitor.GetProcessId();
                
                // Step 3: Cek apakah proses crash (jika sebelumnya ada PID)
                if (lastKnownProcessId != -1 && processMonitor.HasProcessCrashed(lastKnownProcessId))
                {
                    await HandleProcessCrash();
                }
                
                lastKnownProcessId = currentPid;
                
                // Step 4: Capture screenshot pojok kanan atas
                var screenshot = await screenEngine.CaptureTopRightCorner();
                
                // Step 5: Deteksi status duty menggunakan OCR
                var ocrResult = await screenEngine.DetectDutyStatus(screenshot);
                
                // Step 6: Generate hash dari screenshot
                string imageHash = cloudSync.GenerateImageHash(screenshot);
                
                // Step 7: Upload screenshot (opsional)
                string fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{imageHash.Substring(0, 8)}.png";
                string screenshotUrl = await cloudSync.UploadScreenshot(screenshot, fileName);
                
                // Step 8: Buat payload dan push ke Supabase
                var payload = new AttendancePayload
                {
                    user_identifier = GetUserIdentifier(),
                    timestamp = DateTime.UtcNow,
                    is_on_duty = ocrResult.IsOnDuty,
                    screenshot_hash = imageHash,
                    screenshot_url = screenshotUrl,
                    ocr_confidence = ocrResult.Confidence,
                    process_id = currentPid,
                    detected_text = ocrResult.DetectedText
                };
                
                bool success = await cloudSync.PushAttendanceLog(payload);
                
                if (success)
                {
                    Log($"Log attendance berhasil dikirim - Duty: {ocrResult.IsOnDuty}");
                }
                
                // Step 9: Tunggu interval acak sebelum capture berikutnya
                int nextInterval = GetRandomInterval();
                Log($"Menunggu {nextInterval / 1000} detik untuk capture berikutnya");
                await Task.Delay(nextInterval);
            }
            catch (Exception ex)
            {
                Log($"Error dalam main loop: {ex.Message}");
                await Task.Delay(5000); // Tunggu sebelum retry
            }
        }
    }
    
    // Handle ketika FiveM crash
    private async Task HandleProcessCrash()
    {
        Log("Mendeteksi FiveM crash, mengirim log crash...");
        
        var crashPayload = new AttendancePayload
        {
            user_identifier = GetUserIdentifier(),
            timestamp = DateTime.UtcNow,
            is_on_duty = false,
            screenshot_hash = "CRASH_EVENT",
            screenshot_url = null,
            ocr_confidence = 0.0f,
            process_id = lastKnownProcessId,
            detected_text = "PROCESS_CRASHED"
        };
        
        await cloudSync.PushAttendanceLog(crashPayload);
    }
    
    // Generate interval acak antara min dan max
    private int GetRandomInterval()
    {
        return random.Next(Config.MinCaptureInterval, Config.MaxCaptureInterval);
    }
    
    // Dapatkan identifier user (bisa dari Steam ID, Discord ID, dll)
    private string GetUserIdentifier()
    {
        // Placeholder - implementasi nyata bisa ambil dari registry, file config, dll
        return Environment.MachineName + "_" + Environment.UserName;
    }
    
    private void Log(string message)
    {
        string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        Console.WriteLine(logMessage);
        
        // Tulis ke file log
        File.AppendAllText(Config.LogFilePath, logMessage + Environment.NewLine);
    }
    
    // Entry point
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== HPD Attendance Companion App ===");
        Console.WriteLine("Memulai monitoring...\n");
        
        var app = new MainProgram();
        await app.Run();
    }
}
