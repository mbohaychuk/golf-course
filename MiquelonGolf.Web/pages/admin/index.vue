<!-- MiquelonGolf.Web/pages/admin/index.vue -->
<script setup lang="ts">
import type { BookingDto, AnnouncementDto } from '~/types/api'

definePageMeta({ middleware: 'admin', layout: 'admin', ssr: false })

useSeoMeta({ title: 'Dashboard — Admin' })

const api = useApi()
const { authHeaders } = useAuth()

const today = new Date().toISOString().split('T')[0]
const todayLabel = new Date().toLocaleDateString('en-CA', { weekday: 'long', month: 'long', day: 'numeric' })

const bookingsLoading = ref(true)
const bookingsError = ref<string | null>(null)
const bookings = ref<BookingDto[]>([])

const announcementsLoading = ref(true)
const announcementsError = ref<string | null>(null)
const announcements = ref<AnnouncementDto[]>([])

async function loadDashboard() {
  await Promise.all([
    $fetch<BookingDto[]>(api.url(`/bookings?date=${today}`), { headers: authHeaders.value })
      .then(data => { bookings.value = data })
      .catch(() => { bookingsError.value = 'Could not load bookings.' }),
    $fetch<AnnouncementDto[]>(api.url('/announcements/active'))
      .then(data => { announcements.value = data })
      .catch(() => { announcementsError.value = 'Could not load alerts.' }),
  ])
  bookingsLoading.value = false
  announcementsLoading.value = false
}

onMounted(loadDashboard)
</script>

<template>
  <div class="p-6 max-w-5xl">
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="font-display text-3xl font-bold text-accent">Dashboard</h1>
        <p class="text-sm text-text/60 mt-0.5">{{ todayLabel }}</p>
      </div>
      <div class="flex gap-3">
        <NuxtLink
          to="/admin/tee-sheet"
          class="px-4 py-2 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 transition-opacity"
        >
          Manage Tee Sheet
        </NuxtLink>
      </div>
    </div>

    <!-- Stats row -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
      <div class="bg-surface rounded-lg shadow-sm p-4">
        <p class="text-xs text-text/50 uppercase tracking-wide mb-1">Today's Bookings</p>
        <p class="font-display text-3xl font-bold text-accent">
          {{ bookingsLoading ? '…' : bookings.filter(b => b.status === 'Confirmed').length }}
        </p>
      </div>
      <div class="bg-surface rounded-lg shadow-sm p-4">
        <p class="text-xs text-text/50 uppercase tracking-wide mb-1">Golfers Today</p>
        <p class="font-display text-3xl font-bold text-accent">
          {{ bookingsLoading ? '…' : bookings.filter(b => b.status === 'Confirmed').reduce((s, b) => s + b.numberOfPlayers, 0) }}
        </p>
      </div>
      <div class="bg-surface rounded-lg shadow-sm p-4">
        <p class="text-xs text-text/50 uppercase tracking-wide mb-1">Active Alerts</p>
        <p class="font-display text-3xl font-bold text-accent">{{ announcementsLoading ? '…' : announcements.length }}</p>
      </div>
      <div class="bg-surface rounded-lg shadow-sm p-4">
        <p class="text-xs text-text/50 uppercase tracking-wide mb-1">Date</p>
        <p class="font-bold text-lg text-text">{{ today }}</p>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <!-- Today's bookings -->
      <div class="lg:col-span-2 bg-surface rounded-lg shadow-sm">
        <div class="px-5 py-4 border-b border-gray-100 flex items-center justify-between">
          <h2 class="font-semibold text-text">Today's Bookings</h2>
          <NuxtLink to="/admin/bookings" class="text-xs text-primary hover:underline">View all →</NuxtLink>
        </div>

        <div v-if="bookingsLoading" class="p-6 text-center text-text/40 text-sm">Loading…</div>
        <div v-else-if="bookingsError" class="p-4 text-red-600 text-sm">{{ bookingsError }}</div>
        <div v-else-if="bookings.filter(b => b.status === 'Confirmed').length === 0" class="p-6 text-center text-text/40 text-sm">No bookings today.</div>
        <div v-else class="divide-y divide-gray-50">
          <div
            v-for="b in bookings.filter(b => b.status === 'Confirmed')"
            :key="b.id"
            class="px-5 py-3 flex items-center justify-between text-sm"
          >
            <div>
              <span class="font-medium text-text">{{ b.slotTime }}</span>
              <span class="text-text/60 ml-3">{{ b.golferName }}</span>
            </div>
            <span class="text-text/50">{{ b.numberOfPlayers }} player{{ b.numberOfPlayers > 1 ? 's' : '' }}</span>
          </div>
        </div>
      </div>

      <!-- Active announcements -->
      <div class="bg-surface rounded-lg shadow-sm">
        <div class="px-5 py-4 border-b border-gray-100 flex items-center justify-between">
          <h2 class="font-semibold text-text">Active Alerts</h2>
          <NuxtLink to="/admin/content" class="text-xs text-primary hover:underline">Manage →</NuxtLink>
        </div>

        <div v-if="announcementsLoading" class="p-6 text-center text-text/40 text-sm">Loading…</div>
        <div v-else-if="announcementsError" class="p-4 text-red-600 text-sm">{{ announcementsError }}</div>
        <div v-else-if="announcements.length === 0" class="p-6 text-center text-text/40 text-sm">No active alerts.</div>
        <div v-else class="divide-y divide-gray-50">
          <div v-for="a in announcements" :key="a.id" class="px-5 py-3 text-sm">
            <span
              :class="[
                'inline-block text-xs font-semibold px-1.5 py-0.5 rounded mr-2',
                a.type === 'Closure' ? 'bg-red-100 text-red-700' : a.type === 'CourseConditions' ? 'bg-green-100 text-green-700' : 'bg-amber-100 text-amber-700'
              ]"
            >{{ a.type }}</span>
            <span class="text-text/70">{{ a.message }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
