-- Logika client-side untuk sistem absensi

-- Variabel lokal
local isDutyActive = false
local playerIdentifier = nil

-- Inisialisasi client
FUNCTION InitializeClient()
    playerIdentifier = GetPlayerIdentifier()
    RegisterCommand(Config.Commands.duty, OnDutyCommand)
END FUNCTION

-- Handler untuk command /duty
FUNCTION OnDutyCommand()
    -- Cek apakah player adalah anggota HPD
    IF NOT IsPlayerHPD() THEN
        ShowNotification(Config.Messages.notHPD)
        RETURN
    END IF
    
    -- Toggle status duty
    IF isDutyActive THEN
        ClockOut()
    ELSE
        ClockIn()
    END IF
END FUNCTION

-- Fungsi untuk clock in
FUNCTION ClockIn()
    isDutyActive = true
    
    -- Kirim data clock-in ke server
    TriggerServerEvent("attendance:clockIn", {
        identifier = playerIdentifier,
        timestamp = GetCurrentTimestamp()
    })
    
    ShowNotification(Config.Messages.dutyOn)
END FUNCTION

-- Fungsi untuk clock out
FUNCTION ClockOut()
    isDutyActive = false
    
    -- Kirim data clock-out ke server
    TriggerServerEvent("attendance:clockOut", {
        identifier = playerIdentifier,
        timestamp = GetCurrentTimestamp(),
        reason = "manual"
    })
    
    ShowNotification(Config.Messages.dutyOff)
END FUNCTION

-- Fungsi helper untuk cek apakah player adalah HPD
FUNCTION IsPlayerHPD()
    local playerJob = GetPlayerJob()
    RETURN playerJob == Config.AllowedJob
END FUNCTION

-- Fungsi helper untuk mendapatkan timestamp
FUNCTION GetCurrentTimestamp()
    RETURN os.time()
END FUNCTION

-- Fungsi helper untuk menampilkan notifikasi
FUNCTION ShowNotification(message)
    DrawNotification(message)
END FUNCTION

-- Inisialisasi saat resource dimulai
AddEventHandler("onClientResourceStart", FUNCTION(resourceName)
    IF GetCurrentResourceName() == resourceName THEN
        InitializeClient()
    END IF
END FUNCTION)
