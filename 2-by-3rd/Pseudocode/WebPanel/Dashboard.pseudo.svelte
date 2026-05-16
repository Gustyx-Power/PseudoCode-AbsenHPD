<script>
    // Komponen Dashboard utama dengan Realtime subscription
    
    import { onMount, onDestroy } from 'svelte'
    import { supabase, getActiveMembers } from './supabaseClient.pseudo.js'
    import { 
        activeMembers, 
        filteredMembers, 
        statistics, 
        dashboardFilters,
        updateMemberStatus,
        formatTimestamp,
        formatDuration
    } from './store.pseudo.js'
    
    let realtimeChannel
    let searchQuery = ''
    let isLoading = true
    
    // Reactive statement untuk update filter
    $: dashboardFilters.set({ ...$dashboardFilters, searchQuery })
    
    onMount(async () => {
        // Load data awal
        await loadInitialData()
        
        // Subscribe ke Realtime changes
        subscribeToRealtimeUpdates()
        
        // Setup interval untuk update status setiap 30 detik
        const statusInterval = setInterval(updateAllMemberStatuses, 30000)
        
        // Cleanup interval saat component destroy
        return () => clearInterval(statusInterval)
    })
    
    onDestroy(() => {
        // Unsubscribe dari Realtime channel
        if (realtimeChannel) {
            supabase.removeChannel(realtimeChannel)
        }
    })
    
    // Load data awal dari database
    async function loadInitialData() {
        isLoading = true
        const members = await getActiveMembers()
        
        // Update status setiap anggota
        const membersWithStatus = members.map(member => ({
            ...member,
            status: updateMemberStatus(member)
        }))
        
        activeMembers.set(membersWithStatus)
        isLoading = false
    }
    
    // Subscribe ke perubahan realtime
    function subscribeToRealtimeUpdates() {
        realtimeChannel = supabase
            .channel('attendance_changes')
            .on(
                'postgres_changes',
                {
                    event: '*', // INSERT, UPDATE, DELETE
                    schema: 'public',
                    table: 'attendance_logs'
                },
                (payload) => {
                    handleRealtimeEvent(payload)
                }
            )
            .subscribe((status) => {
                if (status === 'SUBSCRIBED') {
                    console.log('Berhasil subscribe ke Realtime updates')
                }
            })
    }
    
    // Handle event dari Realtime
    function handleRealtimeEvent(payload) {
        const { eventType, new: newRecord, old: oldRecord } = payload
        
        if (eventType === 'INSERT') {
            // Anggota baru clock in
            const memberWithStatus = {
                ...newRecord,
                status: updateMemberStatus(newRecord)
            }
            
            activeMembers.update(members => [...members, memberWithStatus])
            console.log('Anggota baru duty:', newRecord.user_identifier)
        }
        
        if (eventType === 'UPDATE') {
            // Update data anggota yang sudah ada
            activeMembers.update(members => 
                members.map(member => 
                    member.id === newRecord.id 
                        ? { ...newRecord, status: updateMemberStatus(newRecord) }
                        : member
                )
            )
            console.log('Data anggota diupdate:', newRecord.user_identifier)
        }
        
        if (eventType === 'DELETE') {
            // Anggota clock out
            activeMembers.update(members => 
                members.filter(member => member.id !== oldRecord.id)
            )
            console.log('Anggota clock out:', oldRecord.user_identifier)
        }
    }
    
    // Update status semua anggota
    function updateAllMemberStatuses() {
        activeMembers.update(members => 
            members.map(member => ({
                ...member,
                status: updateMemberStatus(member)
            }))
        )
    }
    
    // Function untuk export data ke CSV
    function exportToCSV() {
        // Implementasi export CSV
        console.log('Export data ke CSV...')
    }
    
    // Function untuk refresh manual
    async function refreshData() {
        await loadInitialData()
        console.log('Data berhasil direfresh')
    }
</script>

<!-- Template Dashboard -->
<div class="dashboard-container">
    <!-- Header -->
    <header class="dashboard-header">
        <h1>Dashboard Absensi HPD</h1>
        <div class="header-actions">
            <button on:click={refreshData}>🔄 Refresh</button>
            <button on:click={exportToCSV}>📥 Export CSV</button>
        </div>
    </header>
    
    <!-- Statistik Cards -->
    <div class="stats-grid">
        <div class="stat-card">
            <div class="stat-icon">👥</div>
            <div class="stat-value">{$statistics.total}</div>
            <div class="stat-label">Total Anggota Duty</div>
        </div>
        
        <div class="stat-card active">
            <div class="stat-icon">🟢</div>
            <div class="stat-value">{$statistics.active}</div>
            <div class="stat-label">Active</div>
        </div>
        
        <div class="stat-card idle">
            <div class="stat-icon">🟡</div>
            <div class="stat-value">{$statistics.idle}</div>
            <div class="stat-label">Idle/Grace</div>
        </div>
        
        <div class="stat-card offline">
            <div class="stat-icon">🔴</div>
            <div class="stat-value">{$statistics.offline}</div>
            <div class="stat-label">Offline</div>
        </div>
    </div>
    
    <!-- Search Bar -->
    <div class="search-container">
        <input 
            type="text" 
            bind:value={searchQuery}
            placeholder="🔍 Cari anggota berdasarkan identifier..."
        />
    </div>
    
    <!-- Tabel Anggota -->
    {#if isLoading}
        <div class="loading">Memuat data...</div>
    {:else if $filteredMembers.length === 0}
        <div class="empty-state">Tidak ada anggota yang sedang duty</div>
    {:else}
        <div class="table-container">
            <table class="members-table">
                <thead>
                    <tr>
                        <th>Status</th>
                        <th>Identifier</th>
                        <th>Clock In</th>
                        <th>Update Terakhir</th>
                        <th>Durasi</th>
                        <th>Confidence</th>
                        <th>Screenshot</th>
                    </tr>
                </thead>
                <tbody>
                    {#each $filteredMembers as member (member.id)}
                        <tr class="member-row {member.status}">
                            <td>
                                {#if member.status === 'active'}
                                    <span class="status-badge active">🟢 Active</span>
                                {:else if member.status === 'idle'}
                                    <span class="status-badge idle">🟡 Idle</span>
                                {:else}
                                    <span class="status-badge offline">🔴 Offline</span>
                                {/if}
                            </td>
                            <td class="identifier">{member.user_identifier}</td>
                            <td>{formatTimestamp(member.clock_in_time)}</td>
                            <td>{formatTimestamp(member.timestamp)}</td>
                            <td>{formatDuration(member.duration_seconds || 0)}</td>
                            <td>{(member.ocr_confidence * 100).toFixed(0)}%</td>
                            <td>
                                {#if member.screenshot_url}
                                    <a href={member.screenshot_url} target="_blank">
                                        🖼️ Lihat
                                    </a>
                                {:else}
                                    <span class="no-screenshot">-</span>
                                {/if}
                            </td>
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>
    {/if}
</div>

<style>
    /* Styling dashboard - placeholder untuk konsep */
    .dashboard-container {
        padding: 2rem;
        max-width: 1400px;
        margin: 0 auto;
    }
    
    .stats-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
        gap: 1rem;
        margin: 2rem 0;
    }
    
    .stat-card {
        padding: 1.5rem;
        border-radius: 8px;
        background: white;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    
    .members-table {
        width: 100%;
        border-collapse: collapse;
    }
    
    .status-badge {
        padding: 0.25rem 0.75rem;
        border-radius: 12px;
        font-size: 0.875rem;
    }
    
    .status-badge.active {
        background: #d4edda;
        color: #155724;
    }
    
    .status-badge.idle {
        background: #fff3cd;
        color: #856404;
    }
    
    .status-badge.offline {
        background: #f8d7da;
        color: #721c24;
    }
</style>
