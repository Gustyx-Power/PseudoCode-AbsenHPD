// Inisiasi koneksi Supabase dengan Realtime

import { createClient } from '@supabase/supabase-js'

// Konfigurasi Supabase
const supabaseUrl = 'https://your-project.supabase.co'
const supabaseAnonKey = 'your-anon-key-here'

// Buat client Supabase dengan Realtime enabled
export const supabase = createClient(supabaseUrl, supabaseAnonKey, {
    realtime: {
        params: {
            eventsPerSecond: 10
        }
    }
})

// Helper function untuk query attendance logs
export async function getActiveMembers() {
    const { data, error } = await supabase
        .from('attendance_logs')
        .select('*')
        .eq('is_on_duty', true)
        .order('timestamp', { ascending: false })
    
    if (error) {
        console.error('Error mengambil data anggota aktif:', error)
        return []
    }
    
    return data
}

// Helper function untuk query semua logs dengan filter
export async function getAttendanceLogs(filters = {}) {
    let query = supabase
        .from('attendance_logs')
        .select('*')
        .order('timestamp', { ascending: false })
    
    if (filters.startDate) {
        query = query.gte('timestamp', filters.startDate)
    }
    
    if (filters.endDate) {
        query = query.lte('timestamp', filters.endDate)
    }
    
    if (filters.userIdentifier) {
        query = query.eq('user_identifier', filters.userIdentifier)
    }
    
    const { data, error } = await query
    
    if (error) {
        console.error('Error mengambil logs:', error)
        return []
    }
    
    return data
}

// Helper function untuk autentikasi
export async function signIn(email, password) {
    const { data, error } = await supabase.auth.signInWithPassword({
        email,
        password
    })
    
    if (error) {
        console.error('Error login:', error.message)
        return { success: false, error: error.message }
    }
    
    return { success: true, user: data.user }
}

// Helper function untuk sign out
export async function signOut() {
    const { error } = await supabase.auth.signOut()
    
    if (error) {
        console.error('Error logout:', error.message)
    }
}

// Helper function untuk cek role user
export async function getUserRole(userId) {
    const { data, error } = await supabase
        .from('user_roles')
        .select('role')
        .eq('user_id', userId)
        .single()
    
    if (error) {
        console.error('Error mengambil role user:', error)
        return null
    }
    
    return data.role // 'petinggi' atau 'sdm'
}
