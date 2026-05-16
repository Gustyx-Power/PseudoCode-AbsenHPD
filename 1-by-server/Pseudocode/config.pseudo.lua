-- Konfigurasi sistem absensi Zero Trust untuk FiveM

Config = {}

-- Konfigurasi database
Config.Database = {
    host = "localhost",
    port = 3306,
    username = "db_user",
    password = "db_password",
    database = "attendance_db",
    table = "duty_logs"
}

-- Konfigurasi Discord Webhook
Config.Webhook = {
    url = "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL",
    enabled = true,
    color = 3447003
}

-- Nama faction yang diizinkan
Config.AllowedJob = "hpd"

-- Pengaturan command
Config.Commands = {
    duty = "duty"
}

-- Pesan notifikasi
Config.Messages = {
    notHPD = "Anda harus menjadi anggota HPD untuk menggunakan command ini.",
    dutyOn = "Anda sekarang sedang bertugas.",
    dutyOff = "Anda telah selesai bertugas."
}
