<!-- MiquelonGolf.Web/pages/admin/login.vue -->
<script setup lang="ts">
definePageMeta({ layout: false, ssr: false })

useSeoMeta({ title: 'Admin Login — Miquelon Hills' })

const { login, isAdmin } = useAuth()

// Redirect if already logged in
if (isAdmin.value) navigateTo('/admin')

const form = reactive({ email: '', password: '' })
const error = ref<string | null>(null)
const loading = ref(false)

async function handleLogin() {
  loading.value = true
  error.value = null
  try {
    const result = await login(form.email, form.password)
    if (result.role !== 'Admin') {
      error.value = 'This account does not have admin access.'
      return
    }
    await navigateTo('/admin')
  } catch {
    error.value = 'Invalid email or password.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-background px-4">
    <div class="w-full max-w-sm">
      <div class="text-center mb-8">
        <h1 class="font-display text-3xl font-bold text-accent">Miquelon Hills</h1>
        <p class="text-sm text-text/60 mt-1">Admin Sign In</p>
      </div>

      <form class="bg-surface rounded-lg shadow-sm p-6 flex flex-col gap-4" @submit.prevent="handleLogin">
        <div>
          <label for="login-email" class="block text-sm font-medium text-text mb-1">Email</label>
          <input
            id="login-email"
            v-model="form.email"
            type="email"
            required
            autofocus
            class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
            placeholder="admin@example.com"
          >
        </div>

        <div>
          <label for="login-password" class="block text-sm font-medium text-text mb-1">Password</label>
          <input
            id="login-password"
            v-model="form.password"
            type="password"
            required
            class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
          >
        </div>

        <div v-if="error" class="bg-red-50 border border-red-200 rounded p-3 text-red-700 text-sm">
          {{ error }}
        </div>

        <button
          type="submit"
          :disabled="loading"
          class="w-full py-2.5 bg-primary text-white font-semibold text-sm rounded hover:opacity-90 transition-opacity disabled:opacity-50"
        >
          <template v-if="loading">Signing in…</template>
          <template v-else>Sign In</template>
        </button>
      </form>

      <p class="text-center mt-4">
        <NuxtLink to="/" class="text-xs text-text/40 hover:text-text/70">← Back to website</NuxtLink>
      </p>
    </div>
  </div>
</template>
