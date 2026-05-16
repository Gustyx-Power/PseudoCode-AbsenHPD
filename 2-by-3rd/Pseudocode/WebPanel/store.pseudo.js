// State management reaktif untuk data attendance

import { writable, derived } from 'svelte/store'

// Store untuk daftar anggota yang sedang duty
export const activeMembers = writable([])

// Store untuk semua logs attendance
export const attendanceLogs = writable([])

// Store untuk user yang sedang login
export const currentUser = writable(null)

// Store untuk role user (petinggi/sdm)
export const userRole = writable(null)

// Store untuk filter dashboard
export const dashboardFilters = writable({
    startDate: null,
    endDate: null,
    searchQuery: ''
})

// Derived store untuk menghitung statistik
export const statistics = derived(activeMembers, ($activeMembers) => {
    const total = $activeMembers.length
    
    // Hitung berdasarkan status
    const active = $activeMembers.filter(m => m.status === 'active').length
    const idle = $activeMembers.filter(m => m.status === 'idle').length
    const offline = $activeMembers.filter(m => m.status === 'offline').length
    
    return {
        total,
        active,
        idle,
        offline
    }
})

// Derived store untuk filter anggota berdasarkan search query
export const filteredMembers = derived(
    [activeMembers, dashboardFilters],
    ([$activeMembers, $filters]) => {
        if (!$filters.searchQuery) {
            return $activeMembers
        }
        
        const query = $filters.searchQuery.toLowerCase()
        return $activeMembers.filter(member => 
            member.user_identifier.toLowerCase().includes(query)
        )
    }
)

// Function untuk update status anggota berdasarkan timestamp terakhir
export function updateMemberStatus(member) {
    const now = new Date()
    const lastUpdate = new Date(member.timestamp)
    const diffMinutes = (now - lastUpdate) / 1000 / 60
    
    // Logika status:
    // 🟢 Active: Update dalam 10 menit terakhir
    // 🟡 Idle: Update 10-30 menit yang lalu
    // 🔴 Offline: Lebih dari 30 menit tidak ada update
    
    if (diffMinutes <= 10) {
        return 'active'
    } else if (diffMinutes <= 30) {
        return 'idle'
    } else {
        return 'offline'
    }
}

// Function untuk format durasi
export function formatDuration(seconds) {
    const hours = Math.floor(seconds / 3600)
    const minutes = Math.floor((seconds % 3600) / 60)
    
    if (hours > 0) {
        return `${hours} jam ${minutes} menit`
    }
    return `${minutes} menit`
}

// Function untuk format timestamp
export function formatTimestamp(timestamp) {
    const date = new Date(timestamp)
    return date.toLocaleString('id-ID', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    })
}
