<script setup lang="ts">
import type { TeeTimeSlotDto, BookingDto, FlowThroughEntryDto, RoundType, CreateBookingPayload } from '~/types/api'

definePageMeta({ middleware: 'admin', layout: 'admin', ssr: false })
useSeoMeta({ title: 'Tee Sheet — Admin' })

const api = useApi()
const { authHeaders } = useAuth()
const toast = useToast()

// ── Date navigation ──────────────────────────────────────
const today = new Date().toISOString().split('T')[0]
const selectedDate = ref(today)

function goToday() { selectedDate.value = today }
function goPrev() {
  const d = new Date(selectedDate.value + 'T00:00:00')
  d.setDate(d.getDate() - 1)
  selectedDate.value = d.toISOString().split('T')[0]
}
function goNext() {
  const d = new Date(selectedDate.value + 'T00:00:00')
  d.setDate(d.getDate() + 1)
  selectedDate.value = d.toISOString().split('T')[0]
}

// ── Data fetching ────────────────────────────────────────
const hole1Slots = ref<TeeTimeSlotDto[]>([])
const hole10Slots = ref<TeeTimeSlotDto[]>([])
const bookings = ref<BookingDto[]>([])
const flowThrough = ref<FlowThroughEntryDto[]>([])
const loading = ref(false)

async function loadData() {
  loading.value = true
  try {
    const [h1, h10, bk, ft] = await Promise.all([
      $fetch<TeeTimeSlotDto[]>(api.url(`/tee-time-slots?date=${selectedDate.value}&startingHole=1`)),
      $fetch<TeeTimeSlotDto[]>(api.url(`/tee-time-slots?date=${selectedDate.value}&startingHole=10`)),
      $fetch<BookingDto[]>(api.url(`/bookings?date=${selectedDate.value}`), { headers: authHeaders.value }),
      $fetch<FlowThroughEntryDto[]>(api.url(`/tee-time-slots/flow-through?date=${selectedDate.value}`), { headers: authHeaders.value }),
    ])
    hole1Slots.value = h1
    hole10Slots.value = h10
    bookings.value = bk
    flowThrough.value = ft
  } catch {
    toast.error('Failed to load tee sheet data.')
  } finally {
    loading.value = false
  }
}

watch(selectedDate, loadData, { immediate: true })

// ── Helpers ──────────────────────────────────────────────
function formatTime(time: string): string {
  const [h, m] = time.split(':').map(Number)
  const suffix = h >= 12 ? 'PM' : 'AM'
  const hour12 = h % 12 || 12
  return `${hour12}:${String(m).padStart(2, '0')} ${suffix}`
}

/** All unique times across both columns, sorted */
const allTimes = computed(() => {
  const timeSet = new Set<string>()
  hole1Slots.value.forEach(s => timeSet.add(s.startTime))
  hole10Slots.value.forEach(s => timeSet.add(s.startTime))
  return [...timeSet].sort()
})

/** Map slotId -> bookings for that slot */
const bookingsBySlot = computed(() => {
  const map = new Map<string, BookingDto[]>()
  for (const b of bookings.value) {
    if (b.status !== 'Confirmed') continue
    const list = map.get(b.teeTimeSlotId) ?? []
    list.push(b)
    map.set(b.teeTimeSlotId, list)
  }
  return map
})

/** Map time -> Hole 1 slot */
const hole1ByTime = computed(() => {
  const map = new Map<string, TeeTimeSlotDto>()
  hole1Slots.value.forEach(s => map.set(s.startTime, s))
  return map
})

/** Map time -> Hole 10 slot */
const hole10ByTime = computed(() => {
  const map = new Map<string, TeeTimeSlotDto>()
  hole10Slots.value.forEach(s => map.set(s.startTime, s))
  return map
})

/** Map time -> flow-through entries that arrive at that time */
const flowByTime = computed(() => {
  const map = new Map<string, FlowThroughEntryDto[]>()
  for (const ft of flowThrough.value) {
    const list = map.get(ft.estimatedArrival) ?? []
    list.push(ft)
    map.set(ft.estimatedArrival, list)
  }
  return map
})

function roundTypeBadge(roundType: string): { label: string; classes: string } {
  switch (roundType) {
    case 'Eighteen': return { label: '18', classes: 'bg-green-100 text-green-700' }
    case 'FrontNine': return { label: 'F9', classes: 'bg-blue-100 text-blue-700' }
    case 'BackNine': return { label: 'B9', classes: 'bg-purple-100 text-purple-700' }
    default: return { label: roundType, classes: 'bg-gray-100 text-gray-600' }
  }
}

function isSlotInPast(slot: TeeTimeSlotDto): boolean {
  const [h, m] = slot.startTime.split(':').map(Number)
  const slotDate = new Date(slot.date + 'T00:00:00')
  slotDate.setHours(h, m, 0, 0)
  return slotDate < new Date()
}

// ── Action menu state ────────────────────────────────────
const activeBookingId = ref<string | null>(null)
function toggleActions(bookingId: string) {
  activeBookingId.value = activeBookingId.value === bookingId ? null : bookingId
}
function closeActions() {
  activeBookingId.value = null
}

// ── Walk-in booking ──────────────────────────────────────
const walkinSlotId = ref<string | null>(null)
const walkinForm = reactive({
  golferName: '',
  golferEmail: '',
  golferPhone: '',
  numberOfPlayers: 1,
  numberOfCarts: 0,
  roundType: 'Eighteen' as RoundType,
})
const walkinSubmitting = ref(false)

function openWalkin(slot: TeeTimeSlotDto) {
  walkinSlotId.value = slot.id
  walkinForm.golferName = ''
  walkinForm.golferEmail = ''
  walkinForm.golferPhone = ''
  walkinForm.numberOfPlayers = 1
  walkinForm.numberOfCarts = 0
  walkinForm.roundType = slot.startingHole === 10 ? 'BackNine' : 'Eighteen'
  closeActions()
}

function cancelWalkin() {
  walkinSlotId.value = null
}

async function submitWalkin() {
  if (!walkinSlotId.value) return
  walkinSubmitting.value = true
  try {
    const payload: CreateBookingPayload = {
      teeTimeSlotId: walkinSlotId.value,
      golferName: walkinForm.golferName.trim(),
      golferEmail: walkinForm.golferEmail.trim(),
      golferPhone: walkinForm.golferPhone.trim(),
      numberOfPlayers: walkinForm.numberOfPlayers,
      numberOfCarts: walkinForm.numberOfCarts,
      roundType: walkinForm.roundType,
      referralSource: 'Walk-in',
    }
    await api.post('/bookings', payload, authHeaders.value)
    toast.success('Walk-in booking created.')
    walkinSlotId.value = null
    await loadData()
  } catch (e: any) {
    toast.error(e?.data?.detail ?? e?.data ?? 'Failed to create booking.')
  } finally {
    walkinSubmitting.value = false
  }
}

// ── Block / Unblock ──────────────────────────────────────
const blockTargetId = ref<string | null>(null)
const blockReason = ref('')

function openBlockModal(slotId: string) {
  blockTargetId.value = slotId
  blockReason.value = ''
  closeActions()
}
function cancelBlock() { blockTargetId.value = null }

async function submitBlock() {
  if (!blockTargetId.value) return
  try {
    await $fetch(api.url(`/tee-time-slots/${blockTargetId.value}/block`), {
      method: 'PATCH',
      headers: authHeaders.value,
      body: { reason: blockReason.value || null },
    })
    toast.success('Slot blocked.')
    blockTargetId.value = null
    await loadData()
  } catch {
    toast.error('Could not block slot.')
  }
}

async function unblockSlot(slotId: string) {
  try {
    await $fetch(api.url(`/tee-time-slots/${slotId}/unblock`), {
      method: 'PATCH',
      headers: authHeaders.value,
    })
    toast.success('Slot unblocked.')
    await loadData()
  } catch {
    toast.error('Could not unblock slot.')
  }
}

// ── Edit booking ─────────────────────────────────────────
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
  closeActions()
}
function cancelEdit() { editBookingId.value = null }

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
    await loadData()
  } catch (e: any) {
    toast.error(e?.data?.detail ?? e?.data ?? 'Failed to update booking.')
  } finally {
    editSubmitting.value = false
  }
}

// ── Move booking ─────────────────────────────────────────
const moveBookingId = ref<string | null>(null)
const moveBookingSlotId = ref<string | null>(null)
const moveBookingStartingHole = ref<number>(1)

function openMove(booking: BookingDto) {
  moveBookingId.value = booking.id
  moveBookingSlotId.value = booking.teeTimeSlotId
  moveBookingStartingHole.value = booking.startingHole
  closeActions()
}
function cancelMove() { moveBookingId.value = null }

const moveSlotsForPicker = computed(() => {
  return moveBookingStartingHole.value === 1 ? hole1Slots.value : hole10Slots.value
})

async function handleMoveSelect(targetSlotId: string) {
  if (!moveBookingId.value) return
  try {
    await api.post(`/bookings/${moveBookingId.value}/move`, { targetTeeTimeSlotId: targetSlotId }, authHeaders.value)
    toast.success('Booking moved.')
    moveBookingId.value = null
    await loadData()
  } catch (e: any) {
    toast.error(e?.data?.detail ?? e?.data ?? 'Failed to move booking.')
  }
}

// ── Cancel booking ───────────────────────────────────────
const cancelBookingId = ref<string | null>(null)
const cancelBookingName = ref('')

function openCancel(booking: BookingDto) {
  cancelBookingId.value = booking.id
  cancelBookingName.value = booking.golferName
  closeActions()
}
function dismissCancel() { cancelBookingId.value = null }

async function confirmCancel() {
  if (!cancelBookingId.value) return
  try {
    await api.del(`/bookings/${cancelBookingId.value}`, authHeaders.value)
    toast.success('Booking cancelled.')
    cancelBookingId.value = null
    await loadData()
  } catch {
    toast.error('Failed to cancel booking.')
  }
}

// ── No-show ──────────────────────────────────────────────
const noShowBookingId = ref<string | null>(null)
const noShowBookingName = ref('')

function openNoShow(booking: BookingDto) {
  noShowBookingId.value = booking.id
  noShowBookingName.value = booking.golferName
  closeActions()
}
function dismissNoShow() { noShowBookingId.value = null }

async function confirmNoShow() {
  if (!noShowBookingId.value) return
  try {
    await $fetch(api.url(`/bookings/${noShowBookingId.value}/no-show`), {
      method: 'PATCH',
      headers: authHeaders.value,
    })
    toast.success('Marked as no-show.')
    noShowBookingId.value = null
    await loadData()
  } catch (e: any) {
    toast.error(e?.data?.detail ?? e?.data ?? 'Failed to mark no-show.')
  }
}

// ── Generate slots (carried over) ────────────────────────
const showGenerateForm = ref(false)
const generateForm = reactive({
  intervalMinutes: 10,
  openTime: '07:00',
  closeTime: '19:00',
  maxPlayers: 4,
})
const generating = ref(false)

async function generateSlots() {
  generating.value = true
  try {
    await $fetch(api.url('/tee-time-slots/generate'), {
      method: 'POST',
      headers: authHeaders.value,
      body: {
        date: selectedDate.value,
        intervalMinutes: generateForm.intervalMinutes,
        openTime: generateForm.openTime,
        closeTime: generateForm.closeTime,
        maxPlayers: generateForm.maxPlayers,
      },
    })
    showGenerateForm.value = false
    toast.success('Slots generated.')
    await loadData()
  } catch (e: any) {
    toast.error(e?.data?.title ?? e?.data?.detail ?? 'Failed to generate slots.')
  } finally {
    generating.value = false
  }
}
</script>

<template>
  <div class="p-4 lg:p-6 max-w-7xl overflow-y-auto">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6">
      <h1 class="font-display text-3xl font-bold text-accent">Tee Sheet</h1>
      <button
        class="px-4 py-2 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 transition-opacity self-start"
        @click="showGenerateForm = !showGenerateForm"
      >
        {{ showGenerateForm ? 'Cancel' : 'Generate Slots' }}
      </button>
    </div>

    <!-- Date navigation -->
    <div class="flex items-center gap-2 mb-5">
      <button
        class="p-2 rounded hover:bg-gray-100 text-text/60 hover:text-text transition-colors"
        title="Previous day"
        @click="goPrev"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
        </svg>
      </button>
      <input
        v-model="selectedDate"
        type="date"
        class="border border-gray-200 rounded px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
      >
      <button
        class="p-2 rounded hover:bg-gray-100 text-text/60 hover:text-text transition-colors"
        title="Next day"
        @click="goNext"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
        </svg>
      </button>
      <button
        class="ml-1 px-3 py-1.5 text-xs font-semibold rounded border border-gray-200 hover:bg-gray-50 transition-colors"
        @click="goToday"
      >
        Today
      </button>
    </div>

    <!-- Generate form -->
    <div v-if="showGenerateForm" class="bg-surface rounded-lg shadow-sm p-5 mb-6 border border-primary/20">
      <h2 class="font-semibold text-text mb-4">Generate Slots for {{ selectedDate }}</h2>
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-4">
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1">Open Time</label>
          <input v-model="generateForm.openTime" type="time" class="w-full border border-gray-200 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1">Close Time</label>
          <input v-model="generateForm.closeTime" type="time" class="w-full border border-gray-200 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1">Interval (min)</label>
          <input v-model.number="generateForm.intervalMinutes" type="number" min="5" max="60" class="w-full border border-gray-200 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1">Max Players</label>
          <input v-model.number="generateForm.maxPlayers" type="number" min="1" max="8" class="w-full border border-gray-200 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
      </div>
      <button
        :disabled="generating"
        class="px-5 py-2 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 disabled:opacity-50 transition-opacity"
        @click="generateSlots"
      >
        {{ generating ? 'Generating...' : 'Generate' }}
      </button>
      <p class="text-xs text-text/50 mt-2">Existing empty slots will be replaced. Booked slots are preserved.</p>
    </div>

    <!-- Loading state -->
    <div v-if="loading" class="bg-surface rounded-lg shadow-sm p-12 text-center text-text/40 text-sm">
      Loading tee sheet...
    </div>

    <!-- Empty state -->
    <div v-else-if="allTimes.length === 0" class="bg-surface rounded-lg shadow-sm p-12 text-center text-text/40 text-sm">
      No slots for this date. Use "Generate Slots" to create them.
    </div>

    <!-- Tee sheet grid -->
    <div v-else class="bg-surface rounded-lg shadow-sm overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-sm min-w-[700px]">
          <thead class="bg-gray-50 border-b border-gray-100">
            <tr>
              <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide w-20">Time</th>
              <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Hole 1</th>
              <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Hole 10</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-50">
            <tr v-for="time in allTimes" :key="time" class="align-top">
              <!-- Time label -->
              <td class="px-4 py-3 font-medium text-text/80 whitespace-nowrap">
                {{ formatTime(time) }}
              </td>

              <!-- Hole 1 cell -->
              <td class="px-3 py-2">
                <template v-if="hole1ByTime.get(time)">
                  <div
                    v-if="hole1ByTime.get(time)!.isBlocked"
                    class="bg-gray-100 text-gray-500 rounded-lg px-3 py-2 text-xs"
                  >
                    <div class="flex items-center justify-between">
                      <span>Blocked{{ hole1ByTime.get(time)!.blockReason ? ` — ${hole1ByTime.get(time)!.blockReason}` : '' }}</span>
                      <button
                        class="text-primary hover:underline font-medium ml-2"
                        @click="unblockSlot(hole1ByTime.get(time)!.id)"
                      >Unblock</button>
                    </div>
                  </div>
                  <template v-else>
                    <!-- Bookings in this slot -->
                    <div
                      v-for="booking in bookingsBySlot.get(hole1ByTime.get(time)!.id) ?? []"
                      :key="booking.id"
                      class="relative bg-white border border-gray-200 rounded-lg px-3 py-2 mb-1.5 shadow-sm"
                    >
                      <!-- Edit form (inline) -->
                      <template v-if="editBookingId === booking.id">
                        <form class="flex flex-col gap-2" @submit.prevent="submitEdit">
                          <input v-model="editForm.golferName" type="text" placeholder="Name" required class="border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-primary/40" />
                          <input v-model="editForm.golferEmail" type="email" placeholder="Email" required class="border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-primary/40" />
                          <input v-model="editForm.golferPhone" type="tel" placeholder="Phone" required class="border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-primary/40" />
                          <div class="flex gap-2">
                            <select v-model="editForm.numberOfPlayers" class="flex-1 border border-gray-200 rounded px-2 py-1 text-xs bg-white">
                              <option v-for="n in 4" :key="n" :value="n">{{ n }} player{{ n > 1 ? 's' : '' }}</option>
                            </select>
                            <select v-model="editForm.numberOfCarts" class="flex-1 border border-gray-200 rounded px-2 py-1 text-xs bg-white">
                              <option :value="0">0 carts</option>
                              <option v-for="n in editForm.numberOfPlayers" :key="n" :value="n">{{ n }} cart{{ n > 1 ? 's' : '' }}</option>
                            </select>
                          </div>
                          <div class="flex gap-2">
                            <button type="submit" :disabled="editSubmitting" class="flex-1 bg-primary text-white text-xs font-semibold py-1.5 rounded hover:opacity-90 disabled:opacity-50">Save</button>
                            <button type="button" class="flex-1 bg-gray-100 text-text text-xs font-semibold py-1.5 rounded hover:bg-gray-200" @click="cancelEdit">Cancel</button>
                          </div>
                        </form>
                      </template>
                      <template v-else>
                        <div class="flex items-center gap-2 cursor-pointer" @click="toggleActions(booking.id)">
                          <span class="font-semibold text-text text-sm truncate">{{ booking.golferName }}</span>
                          <span class="inline-flex items-center justify-center px-1.5 py-0.5 rounded text-xs font-bold bg-gray-100 text-text/70">
                            &times;{{ booking.numberOfPlayers }}
                          </span>
                          <span
                            class="inline-flex items-center px-1.5 py-0.5 rounded text-xs font-semibold"
                            :class="roundTypeBadge(booking.roundType).classes"
                          >{{ roundTypeBadge(booking.roundType).label }}</span>
                        </div>
                        <!-- Action menu -->
                        <div v-if="activeBookingId === booking.id" class="mt-2 flex flex-wrap gap-1.5">
                          <button class="text-xs px-2 py-1 rounded bg-blue-50 text-blue-700 hover:bg-blue-100 font-medium" @click="openEdit(booking)">Edit</button>
                          <button class="text-xs px-2 py-1 rounded bg-indigo-50 text-indigo-700 hover:bg-indigo-100 font-medium" @click="openMove(booking)">Move</button>
                          <button class="text-xs px-2 py-1 rounded bg-red-50 text-red-700 hover:bg-red-100 font-medium" @click="openCancel(booking)">Cancel</button>
                          <button
                            v-if="hole1ByTime.get(time) && isSlotInPast(hole1ByTime.get(time)!)"
                            class="text-xs px-2 py-1 rounded bg-amber-50 text-amber-700 hover:bg-amber-100 font-medium"
                            @click="openNoShow(booking)"
                          >No-Show</button>
                        </div>
                      </template>
                    </div>
                    <!-- Empty slot: walk-in or block -->
                    <div v-if="!(bookingsBySlot.get(hole1ByTime.get(time)!.id)?.length)">
                      <!-- Walk-in form (inline) -->
                      <template v-if="walkinSlotId === hole1ByTime.get(time)!.id">
                        <form class="bg-green-50/50 border border-green-200 rounded-lg px-3 py-2 flex flex-col gap-2" @submit.prevent="submitWalkin">
                          <div class="text-xs font-semibold text-green-700 mb-0.5">Walk-in Booking</div>
                          <input v-model="walkinForm.golferName" type="text" placeholder="Name *" required class="border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-primary/40" />
                          <input v-model="walkinForm.golferEmail" type="email" placeholder="Email *" required class="border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-primary/40" />
                          <input v-model="walkinForm.golferPhone" type="tel" placeholder="Phone *" required class="border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-primary/40" />
                          <div class="flex gap-2">
                            <select v-model="walkinForm.numberOfPlayers" class="flex-1 border border-gray-200 rounded px-2 py-1 text-xs bg-white">
                              <option v-for="n in 4" :key="n" :value="n">{{ n }} player{{ n > 1 ? 's' : '' }}</option>
                            </select>
                            <select v-model="walkinForm.numberOfCarts" class="flex-1 border border-gray-200 rounded px-2 py-1 text-xs bg-white">
                              <option :value="0">0 carts</option>
                              <option v-for="n in walkinForm.numberOfPlayers" :key="n" :value="n">{{ n }} cart{{ n > 1 ? 's' : '' }}</option>
                            </select>
                          </div>
                          <select v-model="walkinForm.roundType" class="border border-gray-200 rounded px-2 py-1 text-xs bg-white">
                            <option value="Eighteen">18 Holes</option>
                            <option value="FrontNine">Front 9</option>
                          </select>
                          <div class="flex gap-2">
                            <button type="submit" :disabled="walkinSubmitting" class="flex-1 bg-primary text-white text-xs font-semibold py-1.5 rounded hover:opacity-90 disabled:opacity-50">Book</button>
                            <button type="button" class="flex-1 bg-gray-100 text-text text-xs font-semibold py-1.5 rounded hover:bg-gray-200" @click="cancelWalkin">Cancel</button>
                          </div>
                        </form>
                      </template>
                      <template v-else>
                        <div
                          class="text-text/30 text-xs rounded-lg px-3 py-2 hover:bg-primary/5 hover:text-primary/60 cursor-pointer transition-colors flex items-center justify-between group"
                          @click="openWalkin(hole1ByTime.get(time)!)"
                        >
                          <span>{{ hole1ByTime.get(time)!.maxPlayers - hole1ByTime.get(time)!.bookingCount }} open</span>
                          <span class="opacity-0 group-hover:opacity-100 text-xs transition-opacity">+ Walk-in</span>
                        </div>
                      </template>
                    </div>
                    <!-- Partially filled: allow walk-in too -->
                    <div v-else-if="hole1ByTime.get(time)!.bookingCount < hole1ByTime.get(time)!.maxPlayers">
                      <template v-if="walkinSlotId === hole1ByTime.get(time)!.id">
                        <form class="bg-green-50/50 border border-green-200 rounded-lg px-3 py-2 flex flex-col gap-2 mt-1" @submit.prevent="submitWalkin">
                          <div class="text-xs font-semibold text-green-700 mb-0.5">Walk-in Booking</div>
                          <input v-model="walkinForm.golferName" type="text" placeholder="Name *" required class="border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-primary/40" />
                          <input v-model="walkinForm.golferEmail" type="email" placeholder="Email *" required class="border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-primary/40" />
                          <input v-model="walkinForm.golferPhone" type="tel" placeholder="Phone *" required class="border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-primary/40" />
                          <div class="flex gap-2">
                            <select v-model="walkinForm.numberOfPlayers" class="flex-1 border border-gray-200 rounded px-2 py-1 text-xs bg-white">
                              <option v-for="n in 4" :key="n" :value="n">{{ n }} player{{ n > 1 ? 's' : '' }}</option>
                            </select>
                            <select v-model="walkinForm.numberOfCarts" class="flex-1 border border-gray-200 rounded px-2 py-1 text-xs bg-white">
                              <option :value="0">0 carts</option>
                              <option v-for="n in walkinForm.numberOfPlayers" :key="n" :value="n">{{ n }} cart{{ n > 1 ? 's' : '' }}</option>
                            </select>
                          </div>
                          <select v-model="walkinForm.roundType" class="border border-gray-200 rounded px-2 py-1 text-xs bg-white">
                            <option value="Eighteen">18 Holes</option>
                            <option value="FrontNine">Front 9</option>
                          </select>
                          <div class="flex gap-2">
                            <button type="submit" :disabled="walkinSubmitting" class="flex-1 bg-primary text-white text-xs font-semibold py-1.5 rounded hover:opacity-90 disabled:opacity-50">Book</button>
                            <button type="button" class="flex-1 bg-gray-100 text-text text-xs font-semibold py-1.5 rounded hover:bg-gray-200" @click="cancelWalkin">Cancel</button>
                          </div>
                        </form>
                      </template>
                      <template v-else>
                        <div
                          class="text-text/30 text-xs rounded-lg px-3 py-1 hover:bg-primary/5 hover:text-primary/60 cursor-pointer transition-colors flex items-center justify-between group mt-1"
                          @click="openWalkin(hole1ByTime.get(time)!)"
                        >
                          <span>{{ hole1ByTime.get(time)!.maxPlayers - hole1ByTime.get(time)!.bookingCount }} more open</span>
                          <span class="opacity-0 group-hover:opacity-100 text-xs transition-opacity">+ Walk-in</span>
                        </div>
                      </template>
                    </div>
                    <!-- Block button for empty/available slots -->
                    <button
                      v-if="!(bookingsBySlot.get(hole1ByTime.get(time)!.id)?.length) && walkinSlotId !== hole1ByTime.get(time)!.id"
                      class="text-xs text-text/25 hover:text-red-500 transition-colors mt-0.5"
                      @click="openBlockModal(hole1ByTime.get(time)!.id)"
                    >Block</button>
                  </template>
                </template>
                <template v-else>
                  <div class="text-text/20 text-xs px-3 py-2">--</div>
                </template>
              </td>

              <!-- Hole 10 cell -->
              <td class="px-3 py-2">
                <template v-if="hole10ByTime.get(time)">
                  <div
                    v-if="hole10ByTime.get(time)!.isBlocked"
                    class="bg-gray-100 text-gray-500 rounded-lg px-3 py-2 text-xs"
                  >
                    <div class="flex items-center justify-between">
                      <span>Blocked{{ hole10ByTime.get(time)!.blockReason ? ` — ${hole10ByTime.get(time)!.blockReason}` : '' }}</span>
                      <button
                        class="text-primary hover:underline font-medium ml-2"
                        @click="unblockSlot(hole10ByTime.get(time)!.id)"
                      >Unblock</button>
                    </div>
                  </div>
                  <template v-else>
                    <!-- Bookings in this slot -->
                    <div
                      v-for="booking in bookingsBySlot.get(hole10ByTime.get(time)!.id) ?? []"
                      :key="booking.id"
                      class="relative bg-white border border-gray-200 rounded-lg px-3 py-2 mb-1.5 shadow-sm"
                    >
                      <!-- Edit form (inline) -->
                      <template v-if="editBookingId === booking.id">
                        <form class="flex flex-col gap-2" @submit.prevent="submitEdit">
                          <input v-model="editForm.golferName" type="text" placeholder="Name" required class="border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-primary/40" />
                          <input v-model="editForm.golferEmail" type="email" placeholder="Email" required class="border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-primary/40" />
                          <input v-model="editForm.golferPhone" type="tel" placeholder="Phone" required class="border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-primary/40" />
                          <div class="flex gap-2">
                            <select v-model="editForm.numberOfPlayers" class="flex-1 border border-gray-200 rounded px-2 py-1 text-xs bg-white">
                              <option v-for="n in 4" :key="n" :value="n">{{ n }} player{{ n > 1 ? 's' : '' }}</option>
                            </select>
                            <select v-model="editForm.numberOfCarts" class="flex-1 border border-gray-200 rounded px-2 py-1 text-xs bg-white">
                              <option :value="0">0 carts</option>
                              <option v-for="n in editForm.numberOfPlayers" :key="n" :value="n">{{ n }} cart{{ n > 1 ? 's' : '' }}</option>
                            </select>
                          </div>
                          <div class="flex gap-2">
                            <button type="submit" :disabled="editSubmitting" class="flex-1 bg-primary text-white text-xs font-semibold py-1.5 rounded hover:opacity-90 disabled:opacity-50">Save</button>
                            <button type="button" class="flex-1 bg-gray-100 text-text text-xs font-semibold py-1.5 rounded hover:bg-gray-200" @click="cancelEdit">Cancel</button>
                          </div>
                        </form>
                      </template>
                      <template v-else>
                        <div class="flex items-center gap-2 cursor-pointer" @click="toggleActions(booking.id)">
                          <span class="font-semibold text-text text-sm truncate">{{ booking.golferName }}</span>
                          <span class="inline-flex items-center justify-center px-1.5 py-0.5 rounded text-xs font-bold bg-gray-100 text-text/70">
                            &times;{{ booking.numberOfPlayers }}
                          </span>
                          <span
                            class="inline-flex items-center px-1.5 py-0.5 rounded text-xs font-semibold"
                            :class="roundTypeBadge(booking.roundType).classes"
                          >{{ roundTypeBadge(booking.roundType).label }}</span>
                        </div>
                        <!-- Action menu -->
                        <div v-if="activeBookingId === booking.id" class="mt-2 flex flex-wrap gap-1.5">
                          <button class="text-xs px-2 py-1 rounded bg-blue-50 text-blue-700 hover:bg-blue-100 font-medium" @click="openEdit(booking)">Edit</button>
                          <button class="text-xs px-2 py-1 rounded bg-indigo-50 text-indigo-700 hover:bg-indigo-100 font-medium" @click="openMove(booking)">Move</button>
                          <button class="text-xs px-2 py-1 rounded bg-red-50 text-red-700 hover:bg-red-100 font-medium" @click="openCancel(booking)">Cancel</button>
                          <button
                            v-if="hole10ByTime.get(time) && isSlotInPast(hole10ByTime.get(time)!)"
                            class="text-xs px-2 py-1 rounded bg-amber-50 text-amber-700 hover:bg-amber-100 font-medium"
                            @click="openNoShow(booking)"
                          >No-Show</button>
                        </div>
                      </template>
                    </div>
                    <!-- Flow-through ghosts -->
                    <div
                      v-for="ft in flowByTime.get(time) ?? []"
                      :key="'ft-' + ft.bookingId"
                      class="border border-dashed border-gray-300 text-gray-400 text-sm italic rounded-lg px-3 py-2 mb-1.5"
                    >
                      <span>{{ ft.golferName }}</span>
                      <span class="text-xs ml-1">&times;{{ ft.numberOfPlayers }}</span>
                      <span class="block text-xs text-gray-300">from {{ formatTime(ft.originSlotTime) }}</span>
                    </div>
                    <!-- Empty slot: walk-in or block -->
                    <div v-if="!(bookingsBySlot.get(hole10ByTime.get(time)!.id)?.length) && !(flowByTime.get(time)?.length)">
                      <template v-if="walkinSlotId === hole10ByTime.get(time)!.id">
                        <form class="bg-green-50/50 border border-green-200 rounded-lg px-3 py-2 flex flex-col gap-2" @submit.prevent="submitWalkin">
                          <div class="text-xs font-semibold text-green-700 mb-0.5">Walk-in Booking</div>
                          <input v-model="walkinForm.golferName" type="text" placeholder="Name *" required class="border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-primary/40" />
                          <input v-model="walkinForm.golferEmail" type="email" placeholder="Email *" required class="border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-primary/40" />
                          <input v-model="walkinForm.golferPhone" type="tel" placeholder="Phone *" required class="border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-primary/40" />
                          <div class="flex gap-2">
                            <select v-model="walkinForm.numberOfPlayers" class="flex-1 border border-gray-200 rounded px-2 py-1 text-xs bg-white">
                              <option v-for="n in 4" :key="n" :value="n">{{ n }} player{{ n > 1 ? 's' : '' }}</option>
                            </select>
                            <select v-model="walkinForm.numberOfCarts" class="flex-1 border border-gray-200 rounded px-2 py-1 text-xs bg-white">
                              <option :value="0">0 carts</option>
                              <option v-for="n in walkinForm.numberOfPlayers" :key="n" :value="n">{{ n }} cart{{ n > 1 ? 's' : '' }}</option>
                            </select>
                          </div>
                          <select v-model="walkinForm.roundType" class="border border-gray-200 rounded px-2 py-1 text-xs bg-white">
                            <option value="BackNine">Back 9</option>
                          </select>
                          <div class="flex gap-2">
                            <button type="submit" :disabled="walkinSubmitting" class="flex-1 bg-primary text-white text-xs font-semibold py-1.5 rounded hover:opacity-90 disabled:opacity-50">Book</button>
                            <button type="button" class="flex-1 bg-gray-100 text-text text-xs font-semibold py-1.5 rounded hover:bg-gray-200" @click="cancelWalkin">Cancel</button>
                          </div>
                        </form>
                      </template>
                      <template v-else>
                        <div
                          class="text-text/30 text-xs rounded-lg px-3 py-2 hover:bg-primary/5 hover:text-primary/60 cursor-pointer transition-colors flex items-center justify-between group"
                          @click="openWalkin(hole10ByTime.get(time)!)"
                        >
                          <span>{{ hole10ByTime.get(time)!.maxPlayers - hole10ByTime.get(time)!.bookingCount }} open</span>
                          <span class="opacity-0 group-hover:opacity-100 text-xs transition-opacity">+ Walk-in</span>
                        </div>
                      </template>
                    </div>
                    <!-- Partially filled: allow walk-in too -->
                    <div v-else-if="hole10ByTime.get(time)!.bookingCount < hole10ByTime.get(time)!.maxPlayers && !(walkinSlotId === hole10ByTime.get(time)!.id)">
                      <div
                        class="text-text/30 text-xs rounded-lg px-3 py-1 hover:bg-primary/5 hover:text-primary/60 cursor-pointer transition-colors flex items-center justify-between group mt-1"
                        @click="openWalkin(hole10ByTime.get(time)!)"
                      >
                        <span>{{ hole10ByTime.get(time)!.maxPlayers - hole10ByTime.get(time)!.bookingCount }} more open</span>
                        <span class="opacity-0 group-hover:opacity-100 text-xs transition-opacity">+ Walk-in</span>
                      </div>
                    </div>
                    <!-- Block button for empty/available slots -->
                    <button
                      v-if="!(bookingsBySlot.get(hole10ByTime.get(time)!.id)?.length) && !(flowByTime.get(time)?.length) && walkinSlotId !== hole10ByTime.get(time)!.id"
                      class="text-xs text-text/25 hover:text-red-500 transition-colors mt-0.5"
                      @click="openBlockModal(hole10ByTime.get(time)!.id)"
                    >Block</button>
                  </template>
                </template>
                <template v-else>
                  <div class="text-text/20 text-xs px-3 py-2">--</div>
                </template>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Block reason modal -->
    <Teleport to="body">
      <div v-if="blockTargetId" class="fixed inset-0 z-50 flex items-center justify-center">
        <div class="absolute inset-0 bg-black/40" @click="cancelBlock" />
        <div class="relative bg-white rounded-lg shadow-xl p-6 w-full max-w-sm mx-4">
          <h3 class="font-semibold text-text mb-4">Block Slot</h3>
          <div class="mb-4">
            <label class="block text-sm font-medium text-text mb-1">Reason (optional)</label>
            <input
              v-model="blockReason"
              type="text"
              placeholder="e.g. Course maintenance"
              class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
              @keydown.enter.prevent="submitBlock"
            >
          </div>
          <div class="flex gap-3">
            <button class="flex-1 py-2 bg-red-600 text-white text-sm font-semibold rounded hover:opacity-90" @click="submitBlock">Block</button>
            <button class="flex-1 py-2 bg-gray-100 text-text text-sm font-semibold rounded hover:bg-gray-200" @click="cancelBlock">Cancel</button>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Move booking modal (SlotPicker) -->
    <UiSlotPicker
      :open="!!moveBookingId"
      :slots="moveSlotsForPicker"
      :current-slot-id="moveBookingSlotId ?? ''"
      @select="handleMoveSelect"
      @cancel="cancelMove"
    />

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
