// Konfigurasi untuk aplikasi companion attendance

public static class Config
{
    // Konfigurasi Supabase API
    public static string SupabaseUrl = "https://your-project.supabase.co";
    public static string SupabaseKey = "your-anon-key-here";
    public static string TableName = "attendance_logs";
    
    // Konfigurasi interval capture (dalam milidetik)
    public static int MinCaptureInterval = 300000; // 5 menit
    public static int MaxCaptureInterval = 600000; // 10 menit
    
    // Konfigurasi proses monitoring
    public static string TargetProcessName = "FiveM";
    
    // Konfigurasi screen capture
    public static int CaptureWidth = 400; // Lebar area capture (pojok kanan atas)
    public static int CaptureHeight = 200; // Tinggi area capture
    
    // Konfigurasi OCR
    public static string[] DutyKeywords = { "HPD", "DUTY", "ON DUTY" };
    public static float OcrConfidenceThreshold = 0.7f; // Minimum confidence untuk deteksi
    
    // Konfigurasi logging
    public static string LogFilePath = "attendance_app.log";
    public static bool EnableDebugMode = true;
}
