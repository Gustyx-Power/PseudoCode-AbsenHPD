// Class untuk capture layar dan OCR detection

public class ScreenEngine
{
    private int captureWidth;
    private int captureHeight;
    
    public ScreenEngine(int width, int height)
    {
        this.captureWidth = width;
        this.captureHeight = height;
    }
    
    // Capture screenshot pojok kanan atas secara silent
    public async Task<Bitmap> CaptureTopRightCorner()
    {
        var screenBounds = Screen.PrimaryScreen.Bounds;
        
        // Hitung koordinat pojok kanan atas
        int x = screenBounds.Width - captureWidth;
        int y = 0;
        
        var bitmap = new Bitmap(captureWidth, captureHeight);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(x, y, 0, 0, new Size(captureWidth, captureHeight));
        }
        
        Log("Screenshot pojok kanan atas berhasil diambil");
        return bitmap;
    }
    
    // Simulasi OCR untuk deteksi teks HPD/Duty
    public async Task<OcrResult> DetectDutyStatus(Bitmap image)
    {
        // Simulasi proses OCR (gunakan library seperti Tesseract di implementasi nyata)
        var detectedText = await PerformOCR(image);
        
        // Cek apakah teks mengandung keyword duty
        bool isDutyDetected = false;
        foreach (var keyword in Config.DutyKeywords)
        {
            if (detectedText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                isDutyDetected = true;
                Log($"Keyword '{keyword}' terdeteksi dalam screenshot");
                break;
            }
        }
        
        return new OcrResult
        {
            IsOnDuty = isDutyDetected,
            DetectedText = detectedText,
            Confidence = CalculateConfidence(detectedText)
        };
    }
    
    // Simulasi proses OCR
    private async Task<string> PerformOCR(Bitmap image)
    {
        // Placeholder untuk OCR engine (Tesseract, Azure OCR, dll)
        await Task.Delay(100); // Simulasi processing time
        return "SIMULATED_OCR_TEXT";
    }
    
    // Hitung confidence score
    private float CalculateConfidence(string text)
    {
        // Simulasi perhitungan confidence
        return text.Length > 0 ? 0.85f : 0.0f;
    }
    
    private void Log(string message)
    {
        if (Config.EnableDebugMode)
        {
            Console.WriteLine($"[ScreenEngine] {message}");
        }
    }
}

// Model untuk hasil OCR
public class OcrResult
{
    public bool IsOnDuty { get; set; }
    public string DetectedText { get; set; }
    public float Confidence { get; set; }
}
