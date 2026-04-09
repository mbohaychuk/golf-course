<!-- MiquelonGolf.Web/pages/book.vue -->
<script setup lang="ts">
import type { TeeTimeSlotDto, BookingDto } from '~/types/api'

useSeoMeta({
  title: 'Book a Tee Time — Miquelon Hills Golf Course',
  description: 'Book your tee time online at Miquelon Hills Golf Course east of Edmonton.',
})

const api = useApi()

// ── Step state ──────────────────────────────────────────────
const step = ref<1 | 2 | 3 | 4>(1)

// ── Step 1: Date picker ──────────────────────────────────────
const today = ref('')
const minDate = ref('')
const selectedDate = ref('')

onMounted(() => {
  const todayStr = new Date().toISOString().split('T')[0]
  today.value = todayStr
  minDate.value = todayStr
  selectedDate.value = todayStr
})

function goToStep2() {
  selectedSlot.value = null
  slots.value = []
  slotsError.value = null
  step.value = 2
  loadSlots()
}

// ── Step 2: Slot grid ────────────────────────────────────────
const slots = ref<TeeTimeSlotDto[]>([])
const slotsLoading = ref(false)
const slotsError = ref<string | null>(null)
const selectedSlot = ref<TeeTimeSlotDto | null>(null)

async function loadSlots() {
  slotsLoading.value = true
  slotsError.value = null
  try {
    slots.value = await $fetch<TeeTimeSlotDto[]>(
      api.url(`/tee-time-slots?date=${selectedDate.value}`)
    )
  } catch {
    slotsError.value = 'Unable to load tee times. Please try again or call us at (780) 473-2511.'
  } finally {
    slotsLoading.value = false
  }
}

// ── Step 3: Golfer details ───────────────────────────────────
const form = reactive({
  golferName: '',
  golferEmail: '',
  golferPhone: '',
  numberOfPlayers: 1,
  numberOfCarts: 0,
})

const submitting = ref(false)
const submitError = ref<string | null>(null)

async function submitBooking() {
  if (!selectedSlot.value) return
  submitting.value = true
  submitError.value = null
  try {
    booking.value = await $fetch<BookingDto>(api.url('/bookings'), {
      method: 'POST',
      body: {
        teeTimeSlotId: selectedSlot.value.id,
        golferName: form.golferName,
        golferEmail: form.golferEmail,
        golferPhone: form.golferPhone,
        numberOfPlayers: form.numberOfPlayers,
        numberOfCarts: form.numberOfCarts,
      },
    })
    step.value = 4
  } catch (e: any) {
    submitError.value =
      e?.data ?? e?.message ?? 'Unable to complete your booking. Please call us at (780) 473-2511.'
  } finally {
    submitting.value = false
  }
}

// ── Step 4: Confirmation ─────────────────────────────────────
const booking = ref<BookingDto | null>(null)

const formattedBookingDate = computed(() => {
  if (!booking.value) return ''
  const d = new Date(booking.value.slotDate + 'T00:00:00')
  return d.toLocaleDateString('en-CA', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
})

const googleCalendarUrl = computed(() => {
  if (!booking.value) return ''
  const date = booking.value.slotDate
  const [h, m] = booking.value.slotTime.split(':').map(Number)
  const start = new Date(`${date}T${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:00`)
  const end = new Date(start.getTime() + 4 * 60 * 60 * 1000)
  const fmt = (d: Date) =>
    d.getFullYear().toString() +
    String(d.getMonth() + 1).padStart(2, '0') +
    String(d.getDate()).padStart(2, '0') +
    'T' +
    String(d.getHours()).padStart(2, '0') +
    String(d.getMinutes()).padStart(2, '0') +
    '00'
  return (
    'https://calendar.google.com/calendar/render?action=TEMPLATE' +
    `&text=${encodeURIComponent('Golf at Miquelon Hills')}` +
    `&dates=${fmt(start)}/${fmt(end)}` +
    `&location=${encodeURIComponent('Miquelon Hills Golf Course, Camrose County, Alberta')}`
  )
})
</script>

<template>
  <div class="max-w-3xl mx-auto px-4 py-12">
    <h1 class="font-display text-4xl font-bold text-accent mb-2">Book a Tee Time</h1>
    <p class="text-text/70 mb-8">Online booking for Miquelon Hills Golf Course.</p>

    <!-- Step indicator -->
    <div class="flex items-center gap-2 mb-10 text-sm font-medium">
      <template v-for="(label, i) in ['Date', 'Time', 'Details', 'Confirmed']" :key="i">
        <div
          :class="[
            'w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold',
            step > i + 1 ? 'bg-primary text-white' : step === i + 1 ? 'bg-accent text-white' : 'bg-gray-200 text-gray-500'
          ]"
        >
          <template v-if="step > i + 1">✓</template>
          <template v-else>{{ i + 1 }}</template>
        </div>
        <span :class="step === i + 1 ? 'text-accent font-semibold' : 'text-text/40'">{{ label }}</span>
        <div v-if="i < 3" class="flex-1 h-px bg-gray-200" />
      </template>
    </div>

    <!-- ── Step 1: Date picker ── -->
    <div v-if="step === 1">
      <h2 class="font-display text-2xl font-bold text-accent mb-4">Pick a Date</h2>
      <div class="bg-surface rounded-lg shadow-sm p-6 flex flex-col gap-4">
        <div>
          <label for="booking-date" class="block text-sm font-medium text-text mb-2">Select your preferred date</label>
          <input
            id="booking-date"
            v-model="selectedDate"
            type="date"
            :min="minDate"
            class="border border-primary/20 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 w-full sm:w-auto"
          >
        </div>
        <div>
          <button
            class="px-6 py-2.5 bg-primary text-white font-semibold text-sm rounded hover:opacity-90 transition-opacity"
            @click="goToStep2"
          >
            See Available Times →
          </button>
        </div>
      </div>
    </div>

    <!-- ── Step 2: Slot grid ── -->
    <div v-else-if="step === 2">
      <div class="flex items-center justify-between mb-4">
        <h2 class="font-display text-2xl font-bold text-accent">
          Available Times
          <span class="text-base font-normal text-text/60 ml-2">{{ selectedDate }}</span>
        </h2>
        <button class="text-sm text-primary hover:underline" @click="step = 1">← Change date</button>
      </div>

      <div v-if="slotsLoading" class="py-12 text-center text-text/40">Loading tee times…</div>

      <div v-else-if="slotsError" class="bg-red-50 border border-red-200 rounded p-4 text-red-700 text-sm mb-4">
        {{ slotsError }}
      </div>

      <template v-else-if="slots.length > 0">
        <SlotGrid
          :slots="slots"
          :selected-slot-id="selectedSlot?.id ?? null"
          @select="slot => { selectedSlot = slot }"
        />

        <div v-if="selectedSlot" class="mt-6 flex items-center justify-between bg-primary/5 border border-primary/20 rounded p-4">
          <div>
            <p class="text-sm font-semibold text-text">Selected: {{ selectedSlot.startTime }} on {{ selectedDate }}</p>
            <p class="text-xs text-text/60 mt-0.5">{{ selectedSlot.maxPlayers - selectedSlot.bookingCount }} spot(s) remaining</p>
          </div>
          <button
            class="px-5 py-2 bg-primary text-white font-semibold text-sm rounded hover:opacity-90 transition-opacity"
            @click="step = 3"
          >
            Continue →
          </button>
        </div>
      </template>

      <div v-else class="bg-background rounded-lg p-8 text-center text-text/50 text-sm">
        <p class="font-medium mb-1">No tee times available for this date.</p>
        <p>Try a different date, or call us at <a href="tel:+17804732511" class="text-primary hover:underline">(780) 473-2511</a>.</p>
        <button class="mt-4 text-primary text-sm hover:underline" @click="step = 1">← Pick another date</button>
      </div>
    </div>

    <!-- ── Step 3: Golfer details ── -->
    <div v-else-if="step === 3">
      <div class="flex items-center justify-between mb-4">
        <h2 class="font-display text-2xl font-bold text-accent">Your Details</h2>
        <button class="text-sm text-primary hover:underline" @click="step = 2">← Change time</button>
      </div>

      <!-- Booking summary -->
      <div class="bg-primary/5 border border-primary/20 rounded p-4 mb-6 text-sm">
        <p class="font-semibold text-text">{{ selectedDate }} at {{ selectedSlot?.startTime }}</p>
        <p class="text-text/60 text-xs mt-0.5">Miquelon Hills Golf Course</p>
      </div>

      <form class="bg-surface rounded-lg shadow-sm p-6 flex flex-col gap-4" @submit.prevent="submitBooking">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div class="sm:col-span-2">
            <label for="golfer-name" class="block text-sm font-medium text-text mb-1">Full Name <span class="text-red-500">*</span></label>
            <input
              id="golfer-name"
              v-model="form.golferName"
              type="text"
              required
              class="w-full border border-primary/20 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
              placeholder="Your full name"
            >
          </div>

          <div>
            <label for="golfer-email" class="block text-sm font-medium text-text mb-1">Email <span class="text-red-500">*</span></label>
            <input
              id="golfer-email"
              v-model="form.golferEmail"
              type="email"
              required
              class="w-full border border-primary/20 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
              placeholder="your@email.com"
            >
          </div>

          <div>
            <label for="golfer-phone" class="block text-sm font-medium text-text mb-1">Phone <span class="text-red-500">*</span></label>
            <input
              id="golfer-phone"
              v-model="form.golferPhone"
              type="tel"
              required
              class="w-full border border-primary/20 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
              placeholder="(780) 555-0100"
            >
          </div>

          <div>
            <label for="num-players" class="block text-sm font-medium text-text mb-1">Number of Players <span class="text-red-500">*</span></label>
            <select
              id="num-players"
              v-model="form.numberOfPlayers"
              class="w-full border border-primary/20 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 bg-white"
            >
              <option v-for="n in Math.min(4, selectedSlot ? selectedSlot.maxPlayers - selectedSlot.bookingCount : 4)" :key="n" :value="n">{{ n }}</option>
            </select>
          </div>

          <div>
            <label for="num-carts" class="block text-sm font-medium text-text mb-1">Cart Rental</label>
            <select
              id="num-carts"
              v-model="form.numberOfCarts"
              class="w-full border border-primary/20 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 bg-white"
            >
              <option :value="0">No cart</option>
              <option v-for="n in form.numberOfPlayers" :key="n" :value="n">{{ n }} cart{{ n > 1 ? 's' : '' }}</option>
            </select>
          </div>
        </div>

        <div v-if="submitError" class="bg-red-50 border border-red-200 rounded p-3 text-red-700 text-sm">
          {{ submitError }}
        </div>

        <div class="flex gap-3 pt-2">
          <button
            type="submit"
            :disabled="submitting"
            class="px-6 py-2.5 bg-primary text-white font-semibold text-sm rounded hover:opacity-90 transition-opacity disabled:opacity-50"
          >
            <template v-if="submitting">Booking…</template>
            <template v-else>Confirm Booking</template>
          </button>
          <button type="button" class="px-4 py-2.5 text-sm text-text/60 hover:text-text" @click="step = 2">Cancel</button>
        </div>
      </form>
    </div>

    <!-- ── Step 4: Confirmation ── -->
    <div v-else-if="step === 4 && booking">
      <div class="bg-surface rounded-lg shadow-sm p-8 text-center">
        <div class="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-4">
          <svg class="w-8 h-8 text-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
          </svg>
        </div>
        <h2 class="font-display text-3xl font-bold text-accent mb-2">You're Booked!</h2>
        <p class="text-text/70 text-sm mb-8">A confirmation has been sent to {{ booking.golferEmail }}</p>

        <div class="bg-background rounded-lg p-6 text-left mb-6 text-sm space-y-2">
          <div class="flex justify-between">
            <span class="text-text/60">Date</span>
            <span class="font-medium text-text">{{ formattedBookingDate }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-text/60">Tee Time</span>
            <span class="font-medium text-text">{{ booking.slotTime }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-text/60">Name</span>
            <span class="font-medium text-text">{{ booking.golferName }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-text/60">Players</span>
            <span class="font-medium text-text">{{ booking.numberOfPlayers }}</span>
          </div>
          <div v-if="booking.numberOfCarts > 0" class="flex justify-between">
            <span class="text-text/60">Carts</span>
            <span class="font-medium text-text">{{ booking.numberOfCarts }}</span>
          </div>
          <div class="flex justify-between pt-2 border-t border-primary/10">
            <span class="text-text/60">Booking #</span>
            <span class="font-medium text-text/50 text-xs">{{ booking.id }}</span>
          </div>
        </div>

        <div class="flex flex-col sm:flex-row gap-3 justify-center">
          <a
            :href="googleCalendarUrl"
            target="_blank"
            rel="noopener"
            class="inline-flex items-center justify-center gap-2 px-5 py-2.5 border border-primary/30 text-primary font-semibold text-sm rounded hover:bg-primary/5 transition-colors"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
            </svg>
            Add to Google Calendar
          </a>
          <NuxtLink
            to="/"
            class="inline-flex items-center justify-center px-5 py-2.5 bg-primary text-white font-semibold text-sm rounded hover:opacity-90 transition-opacity"
          >
            Back to Home
          </NuxtLink>
        </div>
      </div>
    </div>
  </div>
</template>
