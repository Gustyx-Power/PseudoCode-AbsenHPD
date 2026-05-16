// Class untuk sinkronisasi data ke Supabase

public class CloudSync
{
    private string apiUrl;
    private string apiKey;
    private HttpClient httpClient;
    
    public CloudSync(string url, string key)
    {
        this.apiUrl = url;
        this.apiKey = key;
        this.httpClient = new HttpClient();
        this.httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
        this.httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }
    
    // Generate SHA-256 hash dari gambar
    public string GenerateImageHash(Bitmap image)
    {
        using (var ms = new MemoryStream())
        {
            image.Save(ms, ImageFormat.Png);
            byte[] imageBytes = ms.ToArray();
            
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(imageBytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                
                Log($"Hash gambar berhasil digenerate: {hash.Substring(0, 16)}...");
                return hash;
            }
        }
    }
    
    // Push payload JSON ke Supabase
    public async Task<bool> PushAttendanceLog(AttendancePayload payload)
    {
        try
        {
            string endpoint = $"{apiUrl}/rest/v1/{Config.TableName}";
            string jsonPayload = JsonSerializer.Serialize(payload);
            
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(endpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                Log("Data berhasil dikirim ke Supabase");
                return true;
            }
            else
            {
                Log($"Gagal mengirim data: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Log($"Error saat push data: {ex.Message}");
            return false;
        }
    }
    
    // Upload gambar ke Supabase Storage
    public async Task<string> UploadScreenshot(Bitmap image, string fileName)
    {
        try
        {
            string storageEndpoint = $"{apiUrl}/storage/v1/object/screenshots/{fileName}";
            
            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                byte[] imageBytes = ms.ToArray();
                
                var content = new ByteArrayContent(imageBytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                
                var response = await httpClient.PostAsync(storageEndpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    Log($"Screenshot berhasil diupload: {fileName}");
                    return $"{apiUrl}/storage/v1/object/public/screenshots/{fileName}";
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Log($"Error saat upload screenshot: {ex.Message}");
            return null;
        }
    }
    
    private void Log(string message)
    {
        if (Config.EnableDebugMode)
        {
            Console.WriteLine($"[CloudSync] {message}");
        }
    }
}

// Model untuk payload attendance
public class AttendancePayload
{
    public string user_identifier { get; set; }
    public DateTime timestamp { get; set; }
    public bool is_on_duty { get; set; }
    public string screenshot_hash { get; set; }
    public string screenshot_url { get; set; }
    public float ocr_confidence { get; set; }
    public int process_id { get; set; }
    public string detected_text { get; set; }
}
