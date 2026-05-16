// Class untuk monitoring proses FiveM

public class ProcessMonitor
{
    private string processName;
    
    public ProcessMonitor(string targetProcess)
    {
        this.processName = targetProcess;
    }
    
    // Cek apakah FiveM sedang berjalan
    public bool IsFiveMRunning()
    {
        var processes = Process.GetProcessesByName(processName);
        
        if (processes.Length > 0)
        {
            Log("FiveM terdeteksi sedang berjalan");
            return true;
        }
        
        Log("FiveM tidak terdeteksi");
        return false;
    }
    
    // Dapatkan ID proses FiveM
    public int GetProcessId()
    {
        var processes = Process.GetProcessesByName(processName);
        
        if (processes.Length > 0)
        {
            return processes[0].Id;
        }
        
        return -1;
    }
    
    // Cek apakah proses crash atau tertutup
    public bool HasProcessCrashed(int lastKnownPid)
    {
        try
        {
            var process = Process.GetProcessById(lastKnownPid);
            return false; // Masih berjalan
        }
        catch
        {
            Log("FiveM terdeteksi crash atau tertutup");
            return true;
        }
    }
    
    private void Log(string message)
    {
        if (Config.EnableDebugMode)
        {
            Console.WriteLine($"[ProcessMonitor] {message}");
        }
    }
}
