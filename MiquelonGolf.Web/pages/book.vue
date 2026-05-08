<!-- MiquelonGolf.Web/pages/book.vue -->
<script setup lang="ts">
import type {
  TeeTimeSlotDto,
  BookingConfirmationDto,
  BookingSettingsDto,
  OperatingHoursDto,
  CourseHolidayDto,
  CreateBookingPayload,
  RoundType,
} from '~/types/api'

useSeoMeta({
  title: 'Book a Tee Time — Miquelon Hills Golf Course',
  description: 'Book your tee time online at Miquelon Hills Golf Course east of Edmonton.',
})

const api = useApi()

// ── Wizard step state ──────────────────────────────────────
const step = ref<1 | 2 | 3>(1)

// ── Step 1: Date & Time state ──────────────────────────────
const roundType = ref<RoundType>('Eighteen')
const selectedDate = ref<string | null>(null)
const selectedSlotId = ref<string | null>(null)

const today = computed(() => new Date().toISOString().split('T')[0])
const minDate = computed(() => today.value)

// Settings — bookingWindowDays drives maxDate
const { data: settings } = api.get<BookingSettingsDto>('/admin/settings')
const maxDate = computed(() => {
  const days = settings.value?.bookingWindowDays ?? 14
  const d = new Date()
  d.setDate(d.getDate() + days)
  return d.toISOString().split('T')[0]
})

// Operating hours — derive closed day-of-week numbers
const { data: operatingHours } = api.get<OperatingHoursDto[]>('/course-hours/schedule')
const closedDays = computed(() =>
  (operatingHours.value ?? []).filter(h => !h.isOpen).map(h => h.dayOfWeek)
)

// Course holidays
const { data: holidaysRaw } = api.get<CourseHolidayDto[]>('/course-hours/holidays')
const holidays = computed(() => (holidaysRaw.value ?? []).map(h => h.date))

// Tee time slots — refetch when date or roundType changes
const startingHole = computed(() => (roundType.value === 'BackNine' ? 10 : 1))
const slotsParams = computed(() => ({
  date: selectedDate.value,
  startingHole: startingHole.value,
}))

const { data: slotsData, pending: slotsLoading, refresh: refreshSlots } = api.get<TeeTimeSlotDto[]>(
  '/tee-time-slots',
  {
    params: slotsParams,
    watch: [slotsParams],
    immediate: false,
  },
)
const slots = computed(() => slotsData.value ?? [])

// When date or roundType changes, clear slot selection and reload
watch([selectedDate, roundType], () => {
  selectedSlotId.value = null
  if (selectedDate.value) refreshSlots()
})

// Selected slot's start time for the summary component
const selectedSlotTime = computed(() => {
  if (!selectedSlotId.value) return null
  const slot = slots.value.find(s => s.id === selectedSlotId.value)
  return slot?.startTime ?? null
})

// ── Step 2: Golfer details ─────────────────────────────────
const form = reactive({
  golferName: '',
  golferEmail: '',
  golferPhone: '',
  numberOfPlayers: 1,
  numberOfCarts: 0,
  referralSource: null as string | null,
})

const formErrors = reactive<Record<string, string>>({})

function validateField(field: string) {
  delete formErrors[field]
  if (field === 'golferName') {
    if (!form.golferName.trim()) formErrors.golferName = 'Name is required.'
    else if (form.golferName.length > 100) formErrors.golferName = 'Max 100 characters.'
  }
  if (field === 'golferEmail') {
    if (!form.golferEmail.trim()) formErrors.golferEmail = 'Email is required.'
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.golferEmail)) formErrors.golferEmail = 'Enter a valid email.'
  }
  if (field === 'golferPhone') {
    const digits = form.golferPhone.replace(/\D/g, '')
    if (!form.golferPhone.trim()) formErrors.golferPhone = 'Phone is required.'
    else if (digits.length < 10) formErrors.golferPhone = 'At least 10 digits required.'
  }
}

function validateAll(): boolean {
  validateField('golferName')
  validateField('golferEmail')
  validateField('golferPhone')
  return Object.keys(formErrors).length === 0
}

const submitting = ref(false)
const submitError = ref<string | null>(null)

// ── Step 3: Confirmation ───────────────────────────────────
const booking = ref<BookingConfirmationDto | null>(null)

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

const roundTypeLabel = computed(() => {
  const labels: Record<RoundType, string> = {
    Eighteen: '18 Holes',
    FrontNine: 'Front 9',
    BackNine: 'Back 9',
  }
  return labels[roundType.value]
})

// ── Booking submission ─────────────────────────────────────
async function submitBooking() {
  if (!validateAll()) return
  if (!selectedSlotId.value) return

  submitting.value = true
  submitError.value = null

  try {
    const payload: CreateBookingPayload = {
      teeTimeSlotId: selectedSlotId.value,
      golferName: form.golferName.trim(),
      golferEmail: form.golferEmail.trim(),
      golferPhone: form.golferPhone.trim(),
      numberOfPlayers: form.numberOfPlayers,
      numberOfCarts: form.numberOfCarts,
      roundType: roundType.value,
      referralSource: form.referralSource,
    }
    booking.value = await api.post<BookingConfirmationDto>('/bookings', payload)
    step.value = 3
    clearSessionStorage()
  } catch (e: any) {
    if (e?.status === 409 || e?.statusCode === 409) {
      submitError.value = 'This tee time was just booked by someone else. Please go back and choose another slot.'
    } else {
      submitError.value =
        e?.data ?? e?.message ?? 'Unable to complete your booking. Please call us at (780) 473-2511.'
    }
  } finally {
    submitting.value = false
  }
}

// ── Reset wizard ───────────────────────────────────────────
function resetWizard() {
  step.value = 1
  roundType.value = 'Eighteen'
  selectedDate.value = null
  selectedSlotId.value = null
  form.golferName = ''
  form.golferEmail = ''
  form.golferPhone = ''
  form.numberOfPlayers = 1
  form.numberOfCarts = 0
  form.referralSource = null
  Object.keys(formErrors).forEach(k => delete formErrors[k])
  submitError.value = null
  booking.value = null
  clearSessionStorage()
}

// ── Session persistence ────────────────────────────────────
const SESSION_KEYS = {
  step: 'booking_step',
  roundType: 'booking_roundType',
  date: 'booking_date',
  slotId: 'booking_slotId',
  form: 'booking_form',
} as const

function saveToSession() {
  if (import.meta.server) return
  try {
    sessionStorage.setItem(SESSION_KEYS.step, String(step.value))
    sessionStorage.setItem(SESSION_KEYS.roundType, roundType.value)
    if (selectedDate.value) sessionStorage.setItem(SESSION_KEYS.date, selectedDate.value)
    if (selectedSlotId.value) sessionStorage.setItem(SESSION_KEYS.slotId, selectedSlotId.value)
    sessionStorage.setItem(SESSION_KEYS.form, JSON.stringify(form))
  } catch { /* sessionStorage may be unavailable */ }
}

function restoreFromSession() {
  if (import.meta.server) return
  try {
    const savedStep = sessionStorage.getItem(SESSION_KEYS.step)
    const savedRoundType = sessionStorage.getItem(SESSION_KEYS.roundType)
    const savedDate = sessionStorage.getItem(SESSION_KEYS.date)
    const savedSlotId = sessionStorage.getItem(SESSION_KEYS.slotId)
    const savedForm = sessionStorage.getItem(SESSION_KEYS.form)

    // Only restore steps 1 or 2 (not confirmation)
    if (savedStep && ['1', '2'].includes(savedStep)) {
      step.value = Number(savedStep) as 1 | 2
    }
    if (savedRoundType && ['Eighteen', 'FrontNine', 'BackNine'].includes(savedRoundType)) {
      roundType.value = savedRoundType as RoundType
    }
    if (savedDate) selectedDate.value = savedDate
    if (savedSlotId) selectedSlotId.value = savedSlotId
    if (savedForm) {
      const parsed = JSON.parse(savedForm)
      Object.assign(form, parsed)
    }
  } catch { /* ignore parse errors */ }
}

function clearSessionStorage() {
  if (import.meta.server) return
  try {
    Object.values(SESSION_KEYS).forEach(k => sessionStorage.removeItem(k))
  } catch { /* ignore */ }
}

// Persist on step change
watch(step, saveToSession)

// Restore on mount
onMounted(() => {
  restoreFromSession()
  // If we restored a date, trigger slot load
  if (selectedDate.value) refreshSlots()
})

// Referral source options
const referralOptions = [
  'Google',
  'Facebook',
  'Word of mouth',
  'Returning golfer',
  'Drive-by',
  'Other',
]
</script>

<template>
  <div class="max-w-5xl mx-auto px-4 py-12">
    <h1 class="font-display text-4xl font-bold text-accent mb-2">Book a Tee Time</h1>
    <p class="text-text/70 mb-8">Online booking for Miquelon Hills Golf Course.</p>

    <!-- Step indicator -->
    <div class="mb-10">
      <BookingStepIndicator :current-step="step" :steps="['Date & Time', 'Your Details', 'Confirmation']" />
    </div>

    <!-- ═══ Step 1: Date & Time ═══ -->
    <div v-if="step === 1">
      <div class="mb-6">
        <BookingRoundSelector v-model="roundType" />
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Left: Calendar + Summary -->
        <div class="flex flex-col gap-4">
          <BookingCalendar
            :model-value="selectedDate"
            :min-date="minDate"
            :max-date="maxDate"
            :closed-days="closedDays"
            :holidays="holidays"
            @update:model-value="selectedDate = $event"
          />
          <BookingSummary
            :date="selectedDate"
            :time="selectedSlotTime"
            :round-type="roundType"
          />
        </div>

        <!-- Right: Slot list -->
        <div>
          <template v-if="selectedDate">
            <BookingSlotList
              :slots="slots"
              :loading="slotsLoading"
              v-model:selected-id="selectedSlotId"
            />
          </template>
          <div v-else class="bg-white rounded-lg p-8 text-center text-text/50 text-sm shadow-sm">
            <p class="font-medium">Select a date to see available tee times.</p>
          </div>
        </div>
      </div>

      <!-- Continue button -->
      <div class="mt-8 flex justify-end">
        <button
          :disabled="!selectedSlotId"
          class="bg-accent hover:bg-accent/90 text-white font-bold py-3 px-6 rounded-lg transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
          @click="step = 2"
        >
          Continue to Details &rarr;
        </button>
      </div>
    </div>

    <!-- ═══ Step 2: Your Details ═══ -->
    <div v-else-if="step === 2">
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <!-- Form -->
        <div class="lg:col-span-2">
          <div class="flex items-center justify-between mb-6">
            <h2 class="font-display text-2xl font-bold text-accent">Your Details</h2>
            <button class="text-sm text-primary hover:underline" @click="step = 1">&larr; Back to Date &amp; Time</button>
          </div>

          <form class="bg-white rounded-lg shadow-sm p-6 flex flex-col gap-5" @submit.prevent="submitBooking">
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <!-- Full Name -->
              <div class="sm:col-span-2">
                <label for="golfer-name" class="block text-sm font-medium text-text mb-1">
                  Full Name <span class="text-red-500">*</span>
                </label>
                <input
                  id="golfer-name"
                  v-model="form.golferName"
                  type="text"
                  required
                  maxlength="100"
                  class="w-full border rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                  :class="formErrors.golferName ? 'border-red-500' : 'border-primary/20'"
                  placeholder="Your full name"
                  @blur="validateField('golferName')"
                />
                <p v-if="formErrors.golferName" class="text-red-500 text-xs mt-1">{{ formErrors.golferName }}</p>
              </div>

              <!-- Email -->
              <div>
                <label for="golfer-email" class="block text-sm font-medium text-text mb-1">
                  Email <span class="text-red-500">*</span>
                </label>
                <input
                  id="golfer-email"
                  v-model="form.golferEmail"
                  type="email"
                  required
                  class="w-full border rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                  :class="formErrors.golferEmail ? 'border-red-500' : 'border-primary/20'"
                  placeholder="your@email.com"
                  @blur="validateField('golferEmail')"
                />
                <p v-if="formErrors.golferEmail" class="text-red-500 text-xs mt-1">{{ formErrors.golferEmail }}</p>
              </div>

              <!-- Phone -->
              <div>
                <label for="golfer-phone" class="block text-sm font-medium text-text mb-1">
                  Phone <span class="text-red-500">*</span>
                </label>
                <input
                  id="golfer-phone"
                  v-model="form.golferPhone"
                  type="tel"
                  required
                  class="w-full border rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                  :class="formErrors.golferPhone ? 'border-red-500' : 'border-primary/20'"
                  placeholder="(780) 123-4567"
                  @blur="validateField('golferPhone')"
                />
                <p v-if="formErrors.golferPhone" class="text-red-500 text-xs mt-1">{{ formErrors.golferPhone }}</p>
              </div>

              <!-- Number of Players -->
              <div>
                <label for="num-players" class="block text-sm font-medium text-text mb-1">
                  Number of Players <span class="text-red-500">*</span>
                </label>
                <select
                  id="num-players"
                  v-model="form.numberOfPlayers"
                  class="w-full border border-primary/20 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 bg-white"
                >
                  <option v-for="n in 4" :key="n" :value="n">{{ n }}</option>
                </select>
              </div>

              <!-- Number of Carts -->
              <div>
                <label for="num-carts" class="block text-sm font-medium text-text mb-1">
                  Number of Carts
                </label>
                <select
                  id="num-carts"
                  v-model="form.numberOfCarts"
                  class="w-full border border-primary/20 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 bg-white"
                >
                  <option :value="0">No cart</option>
                  <option v-for="n in form.numberOfPlayers" :key="n" :value="n">{{ n }} cart{{ n > 1 ? 's' : '' }}</option>
                </select>
              </div>

              <!-- Referral Source -->
              <div class="sm:col-span-2">
                <label for="referral-source" class="block text-sm font-medium text-text mb-1">
                  How did you hear about us?
                </label>
                <select
                  id="referral-source"
                  v-model="form.referralSource"
                  class="w-full border border-primary/20 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 bg-white"
                >
                  <option :value="null">-- Select (optional) --</option>
                  <option v-for="opt in referralOptions" :key="opt" :value="opt">{{ opt }}</option>
                </select>
              </div>
            </div>

            <!-- Submit error -->
            <div v-if="submitError" class="bg-red-50 border border-red-200 rounded p-3 text-red-700 text-sm">
              {{ submitError }}
            </div>

            <!-- Buttons -->
            <div class="flex gap-3 pt-2">
              <button
                type="submit"
                :disabled="submitting"
                class="bg-accent hover:bg-accent/90 text-white font-bold py-3 px-6 rounded-lg transition-colors disabled:opacity-50"
              >
                <template v-if="submitting">Booking...</template>
                <template v-else>Confirm Booking</template>
              </button>
              <button
                type="button"
                class="px-4 py-3 text-sm text-text/60 hover:text-text rounded-lg"
                @click="step = 1"
              >
                Back
              </button>
            </div>
          </form>
        </div>

        <!-- Sidebar: Summary -->
        <div class="lg:col-span-1">
          <BookingSummary
            :date="selectedDate"
            :time="selectedSlotTime"
            :round-type="roundType"
          />
        </div>
      </div>
    </div>

    <!-- ═══ Step 3: Confirmation ═══ -->
    <div v-else-if="step === 3 && booking">
      <div class="bg-white rounded-lg shadow-sm p-8 text-center max-w-2xl mx-auto">
        <!-- Green checkmark -->
        <div class="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
          <svg class="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
          </svg>
        </div>

        <h2 class="font-display text-3xl font-bold text-accent mb-2">You're Booked!</h2>

        <!-- Confirmation code -->
        <p class="text-3xl font-bold text-primary tracking-wide mb-2">{{ booking.confirmationCode }}</p>
        <p class="text-text/50 text-xs mb-6">Confirmation Code</p>

        <p class="text-text/70 text-sm mb-8">
          Save your confirmation code &mdash; please show it at the pro shop on arrival.
          We'll have your tee time ready under <span class="font-semibold">{{ booking.golferName }}</span>.
        </p>

        <!-- Booking details summary -->
        <div class="bg-gray-50 rounded-lg p-6 text-left mb-6 text-sm space-y-2">
          <div class="flex justify-between">
            <span class="text-text/60">Date</span>
            <span class="font-medium text-text">{{ formattedBookingDate }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-text/60">Tee Time</span>
            <span class="font-medium text-text">{{ booking.slotTime }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-text/60">Round</span>
            <span class="font-medium text-text">{{ roundTypeLabel }}</span>
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
        </div>

        <!-- Actions -->
        <div class="flex flex-col sm:flex-row gap-3 justify-center">
          <a
            :href="googleCalendarUrl"
            target="_blank"
            rel="noopener"
            class="inline-flex items-center justify-center gap-2 px-5 py-2.5 border border-primary/30 text-primary font-semibold text-sm rounded-lg hover:bg-primary/5 transition-colors"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            Add to Google Calendar
          </a>
          <button
            class="bg-accent hover:bg-accent/90 text-white font-bold py-2.5 px-5 rounded-lg transition-colors text-sm"
            @click="resetWizard"
          >
            Book Another Tee Time
          </button>
          <NuxtLink
            to="/"
            class="inline-flex items-center justify-center px-5 py-2.5 text-text/60 hover:text-text font-semibold text-sm rounded-lg transition-colors"
          >
            Back to Home
          </NuxtLink>
        </div>
      </div>
    </div>
  </div>
</template>
