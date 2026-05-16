-- Logika server-side untuk sistem absensi

-- Tracking sesi duty aktif (in-memory)
local activeSessions = {}

-- Inisialisasi server
FUNCTION InitializeServer()
    -- Koneksi ke database
    ConnectToDatabase(Config.Database)
    
    -- Register event dari client
    RegisterServerEvent("attendance:clockIn", OnClockIn)
    RegisterServerEvent("attendance:clockOut", OnClockOut)
    
    -- Register handler untuk disconnect
    AddEventHandler("playerDropped", OnPlayerDisconnect)
END FUNCTION

-- Event handler untuk clock in
FUNCTION OnClockIn(data)
    local source = GetEventSource()
    local identifier = data.identifier
    
    -- Validasi identitas player
    IF NOT ValidatePlayer(source, identifier) THEN
        RETURN
    END IF
    
    -- Buat record sesi
    activeSessions[identifier] = {
        source = source,
        clockInTime = data.timestamp
    }
    
    -- Insert ke database
    local query = "INSERT INTO " .. Config.Database.table .. 
                  " (identifier, clock_in, status) VALUES (?, ?, ?)"
    
    ExecuteQuery(query, {
        identifier,
        data.timestamp,
        "active"
    })
    
    -- Kirim notifikasi ke Discord
    IF Config.Webhook.enabled THEN
        SendDiscordWebhook({
            title = "Anggota HPD Mulai Bertugas",
            description = "Anggota " .. identifier .. " telah memulai tugas",
            timestamp = data.timestamp,
            color = Config.Webhook.color
        })
    END IF
    
    LogToConsole("[ATTENDANCE] " .. identifier .. " clock in pada " .. data.timestamp)
END FUNCTION

-- Event handler untuk clock out
FUNCTION OnClockOut(data)
    local source = GetEventSource()
    local identifier = data.identifier
    
    -- Validasi identitas player
    IF NOT ValidatePlayer(source, identifier) THEN
        RETURN
    END IF
    
    -- Cek apakah sesi ada
    IF NOT activeSessions[identifier] THEN
        RETURN
    END IF
    
    local session = activeSessions[identifier]
    local duration = data.timestamp - session.clockInTime
    
    -- Update record di database
    local query = "UPDATE " .. Config.Database.table .. 
                  " SET clock_out = ?, duration = ?, status = ?, reason = ? " ..
                  "WHERE identifier = ? AND status = 'active'"
    
    ExecuteQuery(query, {
        data.timestamp,
        duration,
        "completed",
        data.reason,
        identifier
    })
    
    -- Kirim notifikasi ke Discord
    IF Config.Webhook.enabled THEN
        SendDiscordWebhook({
            title = "Anggota HPD Selesai Bertugas",
            description = "Anggota " .. identifier .. " telah selesai bertugas (" .. data.reason .. ")",
            duration = FormatDuration(duration),
            timestamp = data.timestamp,
            color = Config.Webhook.color
        })
    END IF
    
    -- Hapus dari sesi aktif
    activeSessions[identifier] = nil
    
    LogToConsole("[ATTENDANCE] " .. identifier .. " clock out. Alasan: " .. data.reason)
END FUNCTION

-- Event handler untuk player disconnect
FUNCTION OnPlayerDisconnect(reason)
    local source = GetEventSource()
    local identifier = GetPlayerIdentifierFromSource(source)
    
    -- Cek apakah player memiliki sesi aktif
    IF activeSessions[identifier] THEN
        local timestamp = GetCurrentTimestamp()
        local session = activeSessions[identifier]
        local duration = timestamp - session.clockInTime
        
        -- Update database - tandai sebagai disconnected
        local query = "UPDATE " .. Config.Database.table .. 
                      " SET clock_out = ?, duration = ?, status = ?, reason = ?, disconnect_reason = ? " ..
                      "WHERE identifier = ? AND status = 'active'"
        
        ExecuteQuery(query, {
            timestamp,
            duration,
            "disconnected",
            "crash_or_quit",
            reason,
            identifier
        })
        
        -- Kirim notifikasi ke Discord
        IF Config.Webhook.enabled THEN
            SendDiscordWebhook({
                title = "Anggota HPD Terputus",
                description = "Anggota " .. identifier .. " terputus saat bertugas",
                reason = reason,
                duration = FormatDuration(duration),
                timestamp = timestamp,
                color = 15158332
            })
        END IF
        
        -- Hapus dari sesi aktif
        activeSessions[identifier] = nil
        
        LogToConsole("[ATTENDANCE] " .. identifier .. " terputus. Alasan: " .. reason)
    END IF
END FUNCTION

-- Fungsi helper untuk validasi player
FUNCTION ValidatePlayer(source, identifier)
    local serverIdentifier = GetPlayerIdentifierFromSource(source)
    RETURN serverIdentifier == identifier
END FUNCTION

-- Fungsi helper untuk format durasi
FUNCTION FormatDuration(seconds)
    local hours = floor(seconds / 3600)
    local minutes = floor((seconds % 3600) / 60)
    RETURN hours .. " jam " .. minutes .. " menit"
END FUNCTION

-- Fungsi helper untuk kirim Discord webhook
FUNCTION SendDiscordWebhook(data)
    local embed = {
        title = data.title,
        description = data.description,
        color = data.color,
        timestamp = FormatTimestamp(data.timestamp),
        fields = {}
    }
    
    IF data.duration THEN
        table.insert(embed.fields, {
            name = "Durasi",
            value = data.duration,
            inline = true
        })
    END IF
    
    IF data.reason THEN
        table.insert(embed.fields, {
            name = "Alasan",
            value = data.reason,
            inline = true
        })
    END IF
    
    PerformHttpRequest(Config.Webhook.url, FUNCTION(err, text, headers)
        -- Handle response webhook
    END FUNCTION, "POST", json.encode({embeds = {embed}}), {["Content-Type"] = "application/json"})
END FUNCTION

-- Fungsi helper untuk mendapatkan timestamp
FUNCTION GetCurrentTimestamp()
    RETURN os.time()
END FUNCTION

-- Fungsi helper untuk log ke console
FUNCTION LogToConsole(message)
    print(message)
END FUNCTION

-- Inisialisasi saat resource dimulai
AddEventHandler("onResourceStart", FUNCTION(resourceName)
    IF GetCurrentResourceName() == resourceName THEN
        InitializeServer()
    END IF
END FUNCTION)
