<!-- MiquelonGolf.Web/pages/admin/bookings.vue -->
<script setup lang="ts">
import type { BookingDto, BookingSearchResultDto, TeeTimeSlotDto, RoundType, CreateBookingPayload } from '~/types/api'

definePageMeta({ middleware: 'admin', layout: 'admin', ssr: false })
useSeoMeta({ title: 'Bookings — Admin' })

const api = useApi()
const { authHeaders } = useAuth()
const toast = useToast()
const router = useRouter()

// ── Date-based view ─────────────────────────────────────
const today = new Date().toISOString().split('T')[0]
const selectedDate = ref(today)

const bookings = ref<BookingDto[]>([])
const loading = ref(false)

async function loadBookings() {
  loading.value = true
  try {
    bookings.value = await $fetch<BookingDto[]>(
      api.url(`/bookings?date=${selectedDate.value}`),
      { headers: authHeaders.value },
    )
  } catch {
    toast.error('Could not load bookings.')
  } finally {
    loading.value = false
  }
}

watch(selectedDate, loadBookings, { immediate: true })

const confirmedBookings = computed(() => bookings.value.filter(b => b.status === 'Confirmed'))

// ── Search ──────────────────────────────────────────────
const searchQuery = ref('')
const searchResults = ref<BookingSearchResultDto[]>([])
const searching = ref(false)
const searchPerformed = ref(false)

async function runSearch() {
  const q = searchQuery.value.trim()
  if (!q) return
  searching.value = true
  searchPerformed.value = true
  try {
    searchResults.value = await $fetch<BookingSearchResultDto[]>(
      api.url(`/bookings/search?q=${encodeURIComponent(q)}`),
      { headers: authHeaders.value },
    )
  } catch {
    toast.error('Search failed. Please try again.')
    searchResults.value = []
  } finally {
    searching.value = false
  }
}

function clearSearch() {
  searchQuery.value = ''
  searchResults.value = []
  searchPerformed.value = false
}

function goToDateSheet(date: string) {
  router.push(`/admin/tee-sheet?date=${date}`)
}

// ── Helpers ─────────────────────────────────────────────
function formatTime(time: string): string {
  const [h, m] = time.split(':').map(Number)
  const suffix = h >= 12 ? 'PM' : 'AM'
  const hour12 = h % 12 || 12
  return `${hour12}:${String(m).padStart(2, '0')} ${suffix}`
}

function roundTypeBadge(roundType: string): { label: string; classes: string } {
  switch (roundType) {
    case 'Eighteen': return { label: '18', classes: 'bg-green-100 text-green-700' }
    case 'FrontNine': return { label: 'F9', classes: 'bg-blue-100 text-blue-700' }
    case 'BackNine': return { label: 'B9', classes: 'bg-purple-100 text-purple-700' }
    default: return { label: roundType, classes: 'bg-gray-100 text-gray-600' }
  }
}

function statusBadgeClasses(status: string): string {
  switch (status) {
    case 'Confirmed': return 'bg-green-100 text-green-700'
    case 'Cancelled': return 'bg-gray-100 text-gray-500'
    case 'NoShow': return 'bg-amber-100 text-amber-700'
    default: return 'bg-gray-100 text-gray-600'
  }
}

function isBookingInPast(booking: BookingDto): boolean {
  const [h, m] = booking.slotTime.split(':').map(Number)
  const slotDate = new Date(booking.slotDate + 'T00:00:00')
  slotDate.setHours(h, m, 0, 0)
  return slotDate < new Date()
}

// ── Inline edit ─────────────────────────────────────────
const editBookingId = ref<string | null>(null)
const editForm = reactive({
  golferName: '',
  golferEmail: '',
  golferPhone: '',
  numberOfPlayers: 1,
  numberOfCarts: 0,
})
const editSubmitting = ref(false)

function openEdit(booking: BookingDto) {
  editBookingId.value = booking.id
  editForm.golferName = booking.golferName
  editForm.golferEmail = booking.golferEmail
  editForm.golferPhone = booking.golferPhone
  editForm.numberOfPlayers = booking.numberOfPlayers
  editForm.numberOfCarts = booking.numberOfCarts
}

function cancelEdit() {
  editBookingId.value = null
}

async function submitEdit() {
  if (!editBookingId.value) return
  editSubmitting.value = true
  try {
    await $fetch(api.url(`/bookings/${editBookingId.value}`), {
      method: 'PATCH',
      headers: authHeaders.value,
      body: {
        golferName: editForm.golferName.trim(),
        golferEmail: editForm.golferEmail.trim(),
        golferPhone: editForm.golferPhone.trim(),
        numberOfPlayers: editForm.numberOfPlayers,
        numberOfCarts: editForm.numberOfCarts,
      },
    })
    toast.success('Booking updated.')
    editBookingId.value = null
    await loadBookings()
  } catch (e: any) {
    toast.error(e?.data?.detail ?? e?.data ?? 'Failed to update booking.')
  } finally {
    editSubmitting.value = false
  }
}

// ── Cancel booking ──────────────────────────────────────
const cancelBookingId = ref<string | null>(null)
const cancelBookingName = ref('')

function openCancel(booking: BookingDto) {
  cancelBookingId.value = booking.id
  cancelBookingName.value = booking.golferName
}

function dismissCancel() {
  cancelBookingId.value = null
}

async function confirmCancel() {
  if (!cancelBookingId.value) return
  try {
    await api.del(`/bookings/${cancelBookingId.value}`, authHeaders.value)
    toast.success('Booking cancelled.')
    cancelBookingId.value = null
    await loadBookings()
  } catch {
    toast.error('Failed to cancel booking.')
  }
}

// ── No-show ─────────────────────────────────────────────
const noShowBookingId = ref<string | null>(null)
const noShowBookingName = ref('')

function openNoShow(booking: BookingDto) {
  noShowBookingId.value = booking.id
  noShowBookingName.value = booking.golferName
}

function dismissNoShow() {
  noShowBookingId.value = null
}

async function confirmNoShow() {
  if (!noShowBookingId.value) return
  try {
    await $fetch(api.url(`/bookings/${noShowBookingId.value}/no-show`), {
      method: 'PATCH',
      headers: authHeaders.value,
    })
    toast.success('Marked as no-show.')
    noShowBookingId.value = null
    await loadBookings()
  } catch (e: any) {
    toast.error(e?.data?.detail ?? e?.data ?? 'Failed to mark no-show.')
  }
}

// ── Walk-in form ────────────────────────────────────────
const showWalkInForm = ref(false)
const slots = ref<TeeTimeSlotDto[]>([])
const slotsLoading = ref(false)
const slotsError = ref<string | null>(null)

async function loadSlots() {
  slotsLoading.value = true
  slotsError.value = null
  try {
    slots.value = await $fetch<TeeTimeSlotDto[]>(api.url(`/tee-time-slots?date=${selectedDate.value}`))
  } catch {
    slotsError.value = 'Could not load available slots.'
    slots.value = []
  } finally {
    slotsLoading.value = false
  }
}

const walkInForm = reactive({
  teeTimeSlotId: '',
  golferName: '',
  golferEmail: '',
  golferPhone: '',
  numberOfPlayers: 1,
  numberOfCarts: 0,
  roundType: 'Eighteen' as RoundType,
})
const walkInSubmitting = ref(false)
const walkInError = ref<string | null>(null)

function resetWalkInForm() {
  walkInForm.teeTimeSlotId = ''
  walkInForm.golferName = ''
  walkInForm.golferEmail = ''
  walkInForm.golferPhone = ''
  walkInForm.numberOfPlayers = 1
  walkInForm.numberOfCarts = 0
  walkInForm.roundType = 'Eighteen'
  walkInError.value = null
  slotsError.value = null
}

watch(showWalkInForm, (open) => {
  if (open) loadSlots()
  else resetWalkInForm()
})

async function submitWalkIn() {
  walkInSubmitting.value = true
  walkInError.value = null
  try {
    const payload: CreateBookingPayload = {
      teeTimeSlotId: walkInForm.teeTimeSlotId,
      golferName: walkInForm.golferName.trim(),
      golferEmail: walkInForm.golferEmail.trim(),
      golferPhone: walkInForm.golferPhone.trim(),
      numberOfPlayers: walkInForm.numberOfPlayers,
      numberOfCarts: walkInForm.numberOfCarts,
      roundType: walkInForm.roundType,
      referralSource: 'Walk-in',
    }
    await api.post('/bookings', payload, authHeaders.value)
    toast.success('Walk-in booking created.')
    showWalkInForm.value = false
    await loadBookings()
  } catch (e: any) {
    walkInError.value = e?.data?.title ?? e?.data?.detail ?? 'Could not create booking.'
  } finally {
    walkInSubmitting.value = false
  }
}
</script>

<template>
  <div class="p-6 max-w-6xl">
    <div class="flex items-center justify-between mb-6">
      <h1 class="font-display text-3xl font-bold text-accent">Bookings</h1>
      <button
        class="px-4 py-2 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 transition-opacity"
        @click="showWalkInForm = !showWalkInForm"
      >
        {{ showWalkInForm ? 'Cancel' : '+ Add Walk-In' }}
      </button>
    </div>

    <!-- Search bar -->
    <div class="bg-surface rounded-lg shadow-sm p-4 mb-6 border border-gray-100">
      <label class="block text-xs font-medium text-text/60 mb-2" for="booking-search">Search Bookings</label>
      <div class="flex gap-2">
        <input
          id="booking-search"
          v-model="searchQuery"
          type="text"
          placeholder="Name, email, phone, or confirmation code..."
          class="flex-1 border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
          @keydown.enter="runSearch"
        >
        <button
          :disabled="!searchQuery.trim() || searching"
          class="px-4 py-2 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 disabled:opacity-50 transition-opacity"
          @click="runSearch"
        >
          {{ searching ? 'Searching...' : 'Search' }}
        </button>
        <button
          v-if="searchPerformed"
          class="px-3 py-2 text-sm text-text/60 hover:text-text border border-gray-200 rounded hover:bg-gray-50 transition-colors"
          @click="clearSearch"
        >
          Clear
        </button>
      </div>

      <!-- Search results -->
      <div v-if="searchPerformed" class="mt-4">
        <div v-if="searching" class="text-sm text-text/40 py-4 text-center">Searching...</div>
        <div v-else-if="searchResults.length === 0" class="text-sm text-text/40 py-4 text-center">
          No results found for "{{ searchQuery }}".
        </div>
        <div v-else class="overflow-x-auto">
          <p class="text-xs text-text/50 mb-2">{{ searchResults.length }} result{{ searchResults.length !== 1 ? 's' : '' }} found</p>
          <table class="w-full text-sm">
            <thead class="bg-gray-50 border-b border-gray-100">
              <tr>
                <th class="text-left px-3 py-2 font-semibold text-text/60 text-xs uppercase tracking-wide">Date</th>
                <th class="text-left px-3 py-2 font-semibold text-text/60 text-xs uppercase tracking-wide">Time</th>
                <th class="text-left px-3 py-2 font-semibold text-text/60 text-xs uppercase tracking-wide">Golfer</th>
                <th class="text-center px-3 py-2 font-semibold text-text/60 text-xs uppercase tracking-wide">Status</th>
                <th class="text-center px-3 py-2 font-semibold text-text/60 text-xs uppercase tracking-wide">Round</th>
                <th class="text-left px-3 py-2 font-semibold text-text/60 text-xs uppercase tracking-wide">Code</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-50">
              <tr
                v-for="r in searchResults"
                :key="r.id"
                class="hover:bg-gray-50 cursor-pointer transition-colors"
                :class="r.status === 'Cancelled' ? 'opacity-50' : ''"
                @click="goToDateSheet(r.slotDate)"
              >
                <td class="px-3 py-2 text-text">{{ r.slotDate }}</td>
                <td class="px-3 py-2 text-text font-medium">{{ formatTime(r.slotTime) }}</td>
                <td class="px-3 py-2 text-text">{{ r.golferName }}</td>
                <td class="px-3 py-2 text-center">
                  <span :class="['text-xs font-semibold px-2 py-0.5 rounded', statusBadgeClasses(r.status)]">
                    {{ r.status }}
                  </span>
                </td>
                <td class="px-3 py-2 text-center">
                  <span :class="['text-xs font-semibold px-1.5 py-0.5 rounded', roundTypeBadge(r.roundType).classes]">
                    {{ roundTypeBadge(r.roundType).label }}
                  </span>
                </td>
                <td class="px-3 py-2 text-text/60 font-mono text-xs">{{ r.confirmationCode }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
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
      <span class="text-sm text-text/50">
        {{ confirmedBookings.length }} confirmed booking{{ confirmedBookings.length !== 1 ? 's' : '' }}
      </span>
    </div>

    <!-- Walk-in form -->
    <div v-if="showWalkInForm" class="bg-surface rounded-lg shadow-sm p-5 mb-6 border border-primary/20">
      <h2 class="font-semibold text-text mb-4">Add Walk-In Booking for {{ selectedDate }}</h2>
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-4">
        <div class="sm:col-span-2">
          <label class="block text-xs font-medium text-text/60 mb-1" for="wi-slot">Tee Time</label>
          <div v-if="slotsLoading" class="text-sm text-text/40 py-2">Loading slots...</div>
          <div v-else-if="slotsError" class="text-sm text-red-600 py-2">{{ slotsError }}</div>
          <select
            v-else
            id="wi-slot"
            v-model="walkInForm.teeTimeSlotId"
            required
            class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 bg-white"
          >
            <option value="">Select a time slot...</option>
            <option
              v-for="slot in slots.filter(s => !s.isBlocked && s.bookingCount < s.maxPlayers)"
              :key="slot.id"
              :value="slot.id"
            >
              {{ slot.startTime }} — Hole {{ slot.startingHole }} ({{ slot.maxPlayers - slot.bookingCount }} spots left)
            </option>
          </select>
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="wi-name">Golfer Name</label>
          <input id="wi-name" v-model="walkInForm.golferName" type="text" required placeholder="Full name" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="wi-phone">Phone</label>
          <input id="wi-phone" v-model="walkInForm.golferPhone" type="tel" required placeholder="(780) 123-4567" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="wi-email">Email</label>
          <input id="wi-email" v-model="walkInForm.golferEmail" type="email" required placeholder="name@example.ca" class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="wi-round-type">Round Type</label>
          <select
            id="wi-round-type"
            v-model="walkInForm.roundType"
            class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 bg-white"
          >
            <option value="Eighteen">18 Holes</option>
            <option value="FrontNine">Front 9</option>
            <option value="BackNine">Back 9</option>
          </select>
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
        {{ walkInSubmitting ? 'Adding...' : 'Add Booking' }}
      </button>
    </div>

    <!-- Bookings table -->
    <div class="bg-surface rounded-lg shadow-sm overflow-hidden">
      <div v-if="loading" class="p-8 text-center text-text/40 text-sm">Loading...</div>
      <div v-else-if="bookings.length === 0" class="p-8 text-center text-text/40 text-sm">No bookings for this date.</div>
      <table v-else class="w-full text-sm">
        <thead class="bg-gray-50 border-b border-gray-100">
          <tr>
            <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Time</th>
            <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Golfer</th>
            <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide hidden md:table-cell">Contact</th>
            <th class="text-center px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Players</th>
            <th class="text-center px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Round</th>
            <th class="text-center px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Hole</th>
            <th class="text-center px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Status</th>
            <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide hidden lg:table-cell">Code</th>
            <th class="text-right px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Actions</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-50">
          <template v-for="b in bookings" :key="b.id">
            <!-- Booking row -->
            <tr :class="b.status === 'Cancelled' || b.status === 'NoShow' ? 'opacity-50' : ''">
              <td class="px-4 py-3 font-medium text-text">{{ formatTime(b.slotTime) }}</td>
              <td class="px-4 py-3 text-text">{{ b.golferName }}</td>
              <td class="px-4 py-3 text-text/60 hidden md:table-cell">
                <div>{{ b.golferEmail }}</div>
                <div class="text-xs">{{ b.golferPhone }}</div>
              </td>
              <td class="px-4 py-3 text-center text-text/70">{{ b.numberOfPlayers }}</td>
              <td class="px-4 py-3 text-center">
                <span :class="['text-xs font-semibold px-1.5 py-0.5 rounded', roundTypeBadge(b.roundType).classes]">
                  {{ roundTypeBadge(b.roundType).label }}
                </span>
              </td>
              <td class="px-4 py-3 text-center text-text/70">{{ b.startingHole }}</td>
              <td class="px-4 py-3 text-center">
                <span :class="['text-xs font-semibold px-2 py-0.5 rounded', statusBadgeClasses(b.status)]">
                  {{ b.status }}
                </span>
              </td>
              <td class="px-4 py-3 text-text/50 font-mono text-xs hidden lg:table-cell">{{ b.confirmationCode }}</td>
              <td class="px-4 py-3 text-right">
                <div v-if="b.status === 'Confirmed'" class="flex items-center justify-end gap-2">
                  <button
                    class="text-xs text-primary hover:underline"
                    @click="openEdit(b)"
                  >
                    Edit
                  </button>
                  <button
                    v-if="isBookingInPast(b)"
                    class="text-xs text-amber-600 hover:underline"
                    @click="openNoShow(b)"
                  >
                    No-Show
                  </button>
                  <button
                    class="text-xs text-red-600 hover:underline"
                    @click="openCancel(b)"
                  >
                    Cancel
                  </button>
                </div>
              </td>
            </tr>

            <!-- Inline edit row -->
            <tr v-if="editBookingId === b.id" class="bg-primary/5">
              <td colspan="9" class="px-4 py-4">
                <div class="grid grid-cols-1 sm:grid-cols-3 gap-3 mb-3">
                  <div>
                    <label class="block text-xs font-medium text-text/60 mb-1">Name</label>
                    <input
                      v-model="editForm.golferName"
                      type="text"
                      class="w-full border border-gray-200 rounded px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    >
                  </div>
                  <div>
                    <label class="block text-xs font-medium text-text/60 mb-1">Email</label>
                    <input
                      v-model="editForm.golferEmail"
                      type="email"
                      class="w-full border border-gray-200 rounded px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    >
                  </div>
                  <div>
                    <label class="block text-xs font-medium text-text/60 mb-1">Phone</label>
                    <input
                      v-model="editForm.golferPhone"
                      type="tel"
                      class="w-full border border-gray-200 rounded px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    >
                  </div>
                  <div>
                    <label class="block text-xs font-medium text-text/60 mb-1">Players</label>
                    <select
                      v-model.number="editForm.numberOfPlayers"
                      class="w-full border border-gray-200 rounded px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 bg-white"
                    >
                      <option v-for="n in 4" :key="n" :value="n">{{ n }}</option>
                    </select>
                  </div>
                  <div>
                    <label class="block text-xs font-medium text-text/60 mb-1">Carts</label>
                    <select
                      v-model.number="editForm.numberOfCarts"
                      class="w-full border border-gray-200 rounded px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 bg-white"
                    >
                      <option :value="0">No cart</option>
                      <option v-for="n in editForm.numberOfPlayers" :key="n" :value="n">{{ n }}</option>
                    </select>
                  </div>
                </div>
                <div class="flex gap-2">
                  <button
                    :disabled="editSubmitting"
                    class="px-4 py-1.5 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 disabled:opacity-50 transition-opacity"
                    @click="submitEdit"
                  >
                    {{ editSubmitting ? 'Saving...' : 'Save' }}
                  </button>
                  <button
                    :disabled="editSubmitting"
                    class="px-4 py-1.5 text-sm text-text/60 hover:text-text border border-gray-200 rounded hover:bg-gray-50 transition-colors"
                    @click="cancelEdit"
                  >
                    Cancel
                  </button>
                </div>
              </td>
            </tr>
          </template>
        </tbody>
      </table>
    </div>

    <!-- Cancel confirm modal -->
    <UiConfirmModal
      :open="!!cancelBookingId"
      title="Cancel Booking"
      :message="`Are you sure you want to cancel the booking for ${cancelBookingName}? This cannot be undone.`"
      confirm-text="Cancel Booking"
      variant="danger"
      @confirm="confirmCancel"
      @cancel="dismissCancel"
    />

    <!-- No-show confirm modal -->
    <UiConfirmModal
      :open="!!noShowBookingId"
      title="Mark No-Show"
      :message="`Mark ${noShowBookingName} as a no-show? This is for record-keeping purposes.`"
      confirm-text="Mark No-Show"
      variant="danger"
      @confirm="confirmNoShow"
      @cancel="dismissNoShow"
    />
  </div>
</template>
