<!-- MiquelonGolf.Web/pages/admin/bookings.vue -->
<script setup lang="ts">
import type { BookingDto, TeeTimeSlotDto } from '~/types/api'

definePageMeta({ middleware: 'admin', layout: 'admin', ssr: false })

useSeoMeta({ title: 'Bookings — Admin' })

const api = useApi()
const { authHeaders } = useAuth()

const today = new Date().toISOString().split('T')[0]
const selectedDate = ref(today)

const bookings = ref<BookingDto[]>([])
const loading = ref(false)
const loadError = ref<string | null>(null)

async function loadBookings() {
  loading.value = true
  loadError.value = null
  try {
    bookings.value = await $fetch<BookingDto[]>(
      api.url(`/bookings?date=${selectedDate.value}`),
      { headers: authHeaders.value }
    )
  } catch {
    loadError.value = 'Could not load bookings.'
  } finally {
    loading.value = false
  }
}

watch(selectedDate, loadBookings, { immediate: true })

// Cancel
const cancellingId = ref<string | null>(null)

async function cancelBooking(id: string) {
  if (!confirm('Cancel this booking?')) return
  cancellingId.value = id
  try {
    await $fetch(api.url(`/bookings/${id}`), {
      method: 'DELETE',
      headers: authHeaders.value,
    })
    await loadBookings()
  } catch {
    alert('Could not cancel booking. Please try again.')
  } finally {
    cancellingId.value = null
  }
}

// Add walk-in booking
const showWalkInForm = ref(false)
const slots = ref<TeeTimeSlotDto[]>([])

async function loadSlots() {
  try {
    slots.value = await $fetch<TeeTimeSlotDto[]>(api.url(`/tee-time-slots?date=${selectedDate.value}`))
  } catch {
    slots.value = []
  }
}

watch(showWalkInForm, (open) => { if (open) loadSlots() })

const walkInForm = reactive({
  teeTimeSlotId: '',
  golferName: '',
  golferEmail: '',
  golferPhone: '',
  numberOfPlayers: 1,
  numberOfCarts: 0,
})
const walkInSubmitting = ref(false)
const walkInError = ref<string | null>(null)

async function submitWalkIn() {
  walkInSubmitting.value = true
  walkInError.value = null
  try {
    await $fetch<BookingDto>(api.url('/bookings'), {
      method: 'POST',
      body: { ...walkInForm },
    })
    showWalkInForm.value = false
    walkInForm.teeTimeSlotId = ''
    walkInForm.golferName = ''
    walkInForm.golferEmail = ''
    walkInForm.golferPhone = ''
    walkInForm.numberOfPlayers = 1
    walkInForm.numberOfCarts = 0
    await loadBookings()
  } catch (e: any) {
    walkInError.value = e?.data?.title ?? e?.data?.detail ?? 'Could not create booking.'
  } finally {
    walkInSubmitting.value = false
  }
}

const confirmedBookings = computed(() => bookings.value.filter(b => b.status === 'Confirmed'))
</script>

<template>
  <div class="p-6 max-w-5xl">
    <div class="flex items-center justify-between mb-6">
      <h1 class="font-display text-3xl font-bold text-accent">Bookings</h1>
      <button
        class="px-4 py-2 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 transition-opacity"
        @click="showWalkInForm = !showWalkInForm"
      >
        {{ showWalkInForm ? 'Cancel' : '+ Add Walk-In' }}
      </button>
    </div>

    <!-- Date picker -->
    <div class="flex items-center gap-3 mb-6">
      <label class="text-sm font-medium text-text" for="booking-date">Date</label>
      <input
        id="booking-date"
        v-model="selectedDate"
        type="date"
        class="border border-gray-200 rounded px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
      >
      <span class="text-sm text-text/50">{{ confirmedBookings.length }} confirmed booking{{ confirmedBookings.length !== 1 ? 's' : '' }}</span>
    </div>

    <!-- Walk-in form -->
    <div v-if="showWalkInForm" class="bg-surface rounded-lg shadow-sm p-5 mb-6 border border-primary/20">
      <h2 class="font-semibold text-text mb-4">Add Walk-In Booking for {{ selectedDate }}</h2>
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-4">
        <div class="sm:col-span-2">
          <label class="block text-xs font-medium text-text/60 mb-1" for="wi-slot">Tee Time</label>
          <select
            id="wi-slot"
            v-model="walkInForm.teeTimeSlotId"
            required
            class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 bg-white"
          >
            <option value="">Select a time slot…</option>
            <option
              v-for="slot in slots.filter(s => !s.isBlocked && s.bookingCount < s.maxPlayers)"
              :key="slot.id"
              :value="slot.id"
            >
              {{ slot.startTime }} ({{ slot.maxPlayers - slot.bookingCount }} spots left)
            </option>
          </select>
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="wi-name">Golfer Name</label>
          <input id="wi-name" v-model="walkInForm.golferName" type="text" required placeholder="Full name" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="wi-phone">Phone</label>
          <input id="wi-phone" v-model="walkInForm.golferPhone" type="tel" required placeholder="(780) 555-0100" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="wi-email">Email</label>
          <input id="wi-email" v-model="walkInForm.golferEmail" type="email" required placeholder="golfer@email.com" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="wi-players">Players</label>
          <select id="wi-players" v-model.number="walkInForm.numberOfPlayers" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 bg-white">
            <option v-for="n in 4" :key="n" :value="n">{{ n }}</option>
          </select>
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="wi-carts">Carts</label>
          <select id="wi-carts" v-model.number="walkInForm.numberOfCarts" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 bg-white">
            <option :value="0">No cart</option>
            <option v-for="n in walkInForm.numberOfPlayers" :key="n" :value="n">{{ n }}</option>
          </select>
        </div>
      </div>
      <div v-if="walkInError" class="text-red-600 text-sm mb-3">{{ walkInError }}</div>
      <button
        :disabled="walkInSubmitting || !walkInForm.teeTimeSlotId"
        class="px-5 py-2 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 disabled:opacity-50 transition-opacity"
        @click="submitWalkIn"
      >
        {{ walkInSubmitting ? 'Adding…' : 'Add Booking' }}
      </button>
    </div>

    <!-- Bookings table -->
    <div class="bg-surface rounded-lg shadow-sm overflow-hidden">
      <div v-if="loading" class="p-8 text-center text-text/40 text-sm">Loading…</div>
      <div v-else-if="loadError" class="p-4 text-red-600 text-sm">{{ loadError }}</div>
      <div v-else-if="bookings.length === 0" class="p-8 text-center text-text/40 text-sm">No bookings for this date.</div>
      <table v-else class="w-full text-sm">
        <thead class="bg-gray-50 border-b border-gray-100">
          <tr>
            <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Time</th>
            <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Golfer</th>
            <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide hidden md:table-cell">Contact</th>
            <th class="text-center px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Players</th>
            <th class="text-center px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Status</th>
            <th class="text-right px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Actions</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-50">
          <tr v-for="b in bookings" :key="b.id" :class="b.status === 'Cancelled' ? 'opacity-50' : ''">
            <td class="px-4 py-3 font-medium text-text">{{ b.slotTime }}</td>
            <td class="px-4 py-3 text-text">{{ b.golferName }}</td>
            <td class="px-4 py-3 text-text/60 hidden md:table-cell">
              <div>{{ b.golferEmail }}</div>
              <div class="text-xs">{{ b.golferPhone }}</div>
            </td>
            <td class="px-4 py-3 text-center text-text/70">{{ b.numberOfPlayers }}</td>
            <td class="px-4 py-3 text-center">
              <span
                :class="[
                  'text-xs font-semibold px-2 py-0.5 rounded',
                  b.status === 'Confirmed' ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'
                ]"
              >{{ b.status }}</span>
            </td>
            <td class="px-4 py-3 text-right">
              <button
                v-if="b.status === 'Confirmed'"
                :disabled="cancellingId === b.id"
                class="text-xs text-red-600 hover:underline disabled:opacity-50"
                @click="cancelBooking(b.id)"
              >
                Cancel
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
