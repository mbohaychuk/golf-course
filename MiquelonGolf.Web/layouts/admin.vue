<!-- MiquelonGolf.Web/layouts/admin.vue -->
<script setup lang="ts">
const { logout } = useAuth()

const navLinks = [
  { to: '/admin',          label: 'Dashboard' },
  { to: '/admin/tee-sheet', label: 'Tee Sheet' },
  { to: '/admin/bookings',  label: 'Bookings' },
  { to: '/admin/members',   label: 'Members' },
  { to: '/admin/events',    label: 'Events' },
  { to: '/admin/content',   label: 'Content' },
  { to: '/admin/hours',     label: 'Hours' },
]
</script>

<template>
  <div class="min-h-screen flex bg-background">
    <!-- Sidebar -->
    <aside class="w-56 bg-primary text-white flex flex-col flex-shrink-0">
      <div class="px-4 py-5 border-b border-white/20">
        <p class="font-display text-sm font-bold leading-tight">Miquelon Hills</p>
        <p class="text-xs text-white/50 mt-0.5">Admin Panel</p>
      </div>

      <nav class="flex-1 p-2 flex flex-col gap-0.5">
        <NuxtLink
          v-for="link in navLinks"
          :key="link.to"
          :to="link.to"
          active-class="bg-white/15 text-white"
          class="block px-3 py-2 rounded text-sm text-white/70 hover:bg-white/10 hover:text-white transition-colors"
          :exact="link.to === '/admin'"
        >
          {{ link.label }}
        </NuxtLink>
      </nav>

      <div class="p-3 border-t border-white/20">
        <button
          class="w-full text-left px-3 py-2 rounded text-sm text-white/60 hover:bg-white/10 hover:text-white transition-colors"
          @click="logout"
        >
          Sign Out
        </button>
      </div>
    </aside>

    <!-- Main content -->
    <main class="flex-1 overflow-auto">
      <slot />
    </main>
  </div>
</template>
