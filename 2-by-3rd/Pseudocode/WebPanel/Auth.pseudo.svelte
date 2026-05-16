<script>
    // Komponen autentikasi untuk Petinggi/SDM
    
    import { onMount } from 'svelte'
    import { goto } from '$app/navigation'
    import { supabase, signIn, getUserRole } from './supabaseClient.pseudo.js'
    import { currentUser, userRole } from './store.pseudo.js'
    
    let email = ''
    let password = ''
    let isLoading = false
    let errorMessage = ''
    let showPassword = false
    
    onMount(() => {
        // Cek apakah user sudah login
        checkExistingSession()
    })
    
    // Cek session yang sudah ada
    async function checkExistingSession() {
        const { data: { session } } = await supabase.auth.getSession()
        
        if (session) {
            // User sudah login, redirect ke dashboard
            await handleSuccessfulAuth(session.user)
        }
    }
    
    // Handle form submit
    async function handleLogin(event) {
        event.preventDefault()
        errorMessage = ''
        isLoading = true
        
        // Validasi input
        if (!email || !password) {
            errorMessage = 'Email dan password harus diisi'
            isLoading = false
            return
        }
        
        // Attempt login
        const result = await signIn(email, password)
        
        if (result.success) {
            await handleSuccessfulAuth(result.user)
        } else {
            errorMessage = 'Login gagal: ' + result.error
            isLoading = false
        }
    }
    
    // Handle setelah login berhasil
    async function handleSuccessfulAuth(user) {
        // Dapatkan role user
        const role = await getUserRole(user.id)
        
        // Validasi role - hanya Petinggi dan SDM yang boleh akses
        if (role !== 'petinggi' && role !== 'sdm') {
            errorMessage = 'Anda tidak memiliki akses ke dashboard ini'
            await supabase.auth.signOut()
            isLoading = false
            return
        }
        
        // Set user dan role ke store
        currentUser.set(user)
        userRole.set(role)
        
        console.log(`Login berhasil sebagai ${role}:`, user.email)
        
        // Redirect ke dashboard
        goto('/dashboard')
    }
    
    // Toggle visibility password
    function togglePasswordVisibility() {
        showPassword = !showPassword
    }
</script>

<!-- Template Login -->
<div class="auth-container">
    <div class="auth-card">
        <!-- Logo/Header -->
        <div class="auth-header">
            <div class="logo">🚔</div>
            <h1>Dashboard Absensi HPD</h1>
            <p class="subtitle">Login khusus Petinggi & SDM</p>
        </div>
        
        <!-- Form Login -->
        <form on:submit={handleLogin} class="auth-form">
            <!-- Email Input -->
            <div class="form-group">
                <label for="email">Email</label>
                <input 
                    id="email"
                    type="email" 
                    bind:value={email}
                    placeholder="nama@hpd.gov"
                    disabled={isLoading}
                    required
                />
            </div>
            
            <!-- Password Input -->
            <div class="form-group">
                <label for="password">Password</label>
                <div class="password-input-wrapper">
                    <input 
                        id="password"
                        type={showPassword ? 'text' : 'password'}
                        bind:value={password}
                        placeholder="••••••••"
                        disabled={isLoading}
                        required
                    />
                    <button 
                        type="button"
                        class="toggle-password"
                        on:click={togglePasswordVisibility}
                        disabled={isLoading}
                    >
                        {showPassword ? '👁️' : '👁️‍🗨️'}
                    </button>
                </div>
            </div>
            
            <!-- Error Message -->
            {#if errorMessage}
                <div class="error-message">
                    ⚠️ {errorMessage}
                </div>
            {/if}
            
            <!-- Submit Button -->
            <button 
                type="submit" 
                class="submit-button"
                disabled={isLoading}
            >
                {#if isLoading}
                    <span class="spinner">⏳</span> Memproses...
                {:else}
                    🔐 Login
                {/if}
            </button>
        </form>
        
        <!-- Footer Info -->
        <div class="auth-footer">
            <p class="info-text">
                ℹ️ Hanya akun dengan role <strong>Petinggi</strong> atau <strong>SDM</strong> yang dapat mengakses dashboard ini.
            </p>
        </div>
    </div>
    
    <!-- Background Decoration -->
    <div class="auth-background">
        <div class="bg-shape shape-1"></div>
        <div class="bg-shape shape-2"></div>
        <div class="bg-shape shape-3"></div>
    </div>
</div>

<style>
    /* Styling auth page - placeholder untuk konsep */
    .auth-container {
        min-height: 100vh;
        display: flex;
        align-items: center;
        justify-content: center;
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        position: relative;
        overflow: hidden;
    }
    
    .auth-card {
        background: white;
        border-radius: 16px;
        padding: 3rem;
        box-shadow: 0 20px 60px rgba(0,0,0,0.3);
        max-width: 420px;
        width: 100%;
        z-index: 10;
        position: relative;
    }
    
    .auth-header {
        text-align: center;
        margin-bottom: 2rem;
    }
    
    .logo {
        font-size: 4rem;
        margin-bottom: 1rem;
    }
    
    .form-group {
        margin-bottom: 1.5rem;
    }
    
    .form-group label {
        display: block;
        margin-bottom: 0.5rem;
        font-weight: 600;
        color: #333;
    }
    
    .form-group input {
        width: 100%;
        padding: 0.75rem;
        border: 2px solid #e0e0e0;
        border-radius: 8px;
        font-size: 1rem;
        transition: border-color 0.3s;
    }
    
    .form-group input:focus {
        outline: none;
        border-color: #667eea;
    }
    
    .password-input-wrapper {
        position: relative;
        display: flex;
        align-items: center;
    }
    
    .toggle-password {
        position: absolute;
        right: 0.75rem;
        background: none;
        border: none;
        cursor: pointer;
        font-size: 1.25rem;
    }
    
    .submit-button {
        width: 100%;
        padding: 1rem;
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        color: white;
        border: none;
        border-radius: 8px;
        font-size: 1rem;
        font-weight: 600;
        cursor: pointer;
        transition: transform 0.2s;
    }
    
    .submit-button:hover:not(:disabled) {
        transform: translateY(-2px);
    }
    
    .submit-button:disabled {
        opacity: 0.6;
        cursor: not-allowed;
    }
    
    .error-message {
        background: #f8d7da;
        color: #721c24;
        padding: 0.75rem;
        border-radius: 8px;
        margin-bottom: 1rem;
        font-size: 0.875rem;
    }
    
    .auth-footer {
        margin-top: 2rem;
        padding-top: 1.5rem;
        border-top: 1px solid #e0e0e0;
    }
    
    .info-text {
        font-size: 0.875rem;
        color: #666;
        text-align: center;
        line-height: 1.5;
    }
    
    .auth-background {
        position: absolute;
        width: 100%;
        height: 100%;
        overflow: hidden;
    }
    
    .bg-shape {
        position: absolute;
        border-radius: 50%;
        background: rgba(255,255,255,0.1);
    }
    
    .shape-1 {
        width: 300px;
        height: 300px;
        top: -100px;
        left: -100px;
    }
    
    .shape-2 {
        width: 400px;
        height: 400px;
        bottom: -150px;
        right: -150px;
    }
    
    .shape-3 {
        width: 200px;
        height: 200px;
        top: 50%;
        right: 10%;
    }
</style>
