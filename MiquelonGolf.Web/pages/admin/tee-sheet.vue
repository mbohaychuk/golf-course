<!-- MiquelonGolf.Web/pages/admin/tee-sheet.vue -->
<script setup lang="ts">
import type { TeeTimeSlotDto } from '~/types/api'

definePageMeta({ middleware: 'admin', layout: 'admin', ssr: false })

useSeoMeta({ title: 'Tee Sheet — Admin' })

const api = useApi()
const { authHeaders } = useAuth()

const today = new Date().toISOString().split('T')[0]
const selectedDate = ref(today)

// Slots
const slots = ref<TeeTimeSlotDto[]>([])
const slotsLoading = ref(false)
const slotsError = ref<string | null>(null)

async function loadSlots() {
  slotsLoading.value = true
  slotsError.value = null
  try {
    slots.value = await $fetch<TeeTimeSlotDto[]>(api.url(`/tee-time-slots?date=${selectedDate.value}`))
  } catch {
    slotsError.value = 'Could not load slots.'
  } finally {
    slotsLoading.value = false
  }
}

watch(selectedDate, loadSlots, { immediate: true })

// Generate slots
const generateForm = reactive({
  intervalMinutes: 10,
  openTime: '07:00',
  closeTime: '19:00',
  maxPlayers: 4,
})
const generating = ref(false)
const generateError = ref<string | null>(null)
const showGenerateForm = ref(false)

async function generateSlots() {
  generating.value = true
  generateError.value = null
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
    await loadSlots()
  } catch (e: any) {
    generateError.value = e?.data?.title ?? e?.data?.detail ?? 'Failed to generate slots.'
  } finally {
    generating.value = false
  }
}

// Block / Unblock
const blockingId = ref<string | null>(null)
const blockError = ref<string | null>(null)
const blockReasonInput = ref('')
const showBlockModal = ref(false)
const blockTargetId = ref<string | null>(null)

function openBlockModal(id: string) {
  blockTargetId.value = id
  blockReasonInput.value = ''
  blockError.value = null
  showBlockModal.value = true
}

async function blockSlot() {
  if (!blockTargetId.value) return
  blockingId.value = blockTargetId.value
  try {
    await $fetch(api.url(`/tee-time-slots/${blockTargetId.value}/block`), {
      method: 'PATCH',
      headers: authHeaders.value,
      body: { reason: blockReasonInput.value || null },
    })
    showBlockModal.value = false
    await loadSlots()
  } catch {
    blockError.value = 'Could not block slot. Please try again.'
  } finally {
    blockingId.value = null
  }
}

async function unblockSlot(id: string) {
  blockingId.value = id
  try {
    await $fetch(api.url(`/tee-time-slots/${id}/unblock`), {
      method: 'PATCH',
      headers: authHeaders.value,
    })
    await loadSlots()
  } catch {
    slotsError.value = 'Could not unblock slot. Please reload.'
  } finally {
    blockingId.value = null
  }
}
</script>

<template>
  <div class="p-6 max-w-4xl">
    <div class="flex items-center justify-between mb-6">
      <h1 class="font-display text-3xl font-bold text-accent">Tee Sheet</h1>
      <button
        class="px-4 py-2 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 transition-opacity"
        @click="showGenerateForm = !showGenerateForm"
      >
        {{ showGenerateForm ? 'Cancel' : 'Generate Slots' }}
      </button>
    </div>

    <!-- Date picker -->
    <div class="flex items-center gap-3 mb-6">
      <label class="text-sm font-medium text-text" for="tee-date">Date</label>
      <input
        id="tee-date"
        v-model="selectedDate"
        type="date"
        class="border border-gray-200 rounded px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
      >
    </div>

    <!-- Generate form -->
    <div v-if="showGenerateForm" class="bg-surface rounded-lg shadow-sm p-5 mb-6 border border-primary/20">
      <h2 class="font-semibold text-text mb-4">Generate Slots for {{ selectedDate }}</h2>
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-4">
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="gen-open">Open Time</label>
          <input id="gen-open" v-model="generateForm.openTime" type="time" class="w-full border border-gray-200 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="gen-close">Close Time</label>
          <input id="gen-close" v-model="generateForm.closeTime" type="time" class="w-full border border-gray-200 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="gen-interval">Interval (min)</label>
          <input id="gen-interval" v-model.number="generateForm.intervalMinutes" type="number" min="5" max="60" class="w-full border border-gray-200 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
        <div>
          <label class="block text-xs font-medium text-text/60 mb-1" for="gen-max">Max Players</label>
          <input id="gen-max" v-model.number="generateForm.maxPlayers" type="number" min="1" max="8" class="w-full border border-gray-200 rounded px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40">
        </div>
      </div>
      <div v-if="generateError" class="text-red-600 text-sm mb-3">{{ generateError }}</div>
      <button
        :disabled="generating"
        class="px-5 py-2 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 disabled:opacity-50 transition-opacity"
        @click="generateSlots"
      >
        {{ generating ? 'Generating…' : 'Generate' }}
      </button>
      <p class="text-xs text-text/50 mt-2">Existing empty slots will be replaced. Booked slots are preserved.</p>
    </div>

    <!-- Slots table -->
    <div class="bg-surface rounded-lg shadow-sm overflow-hidden">
      <div v-if="slotsLoading" class="p-8 text-center text-text/40 text-sm">Loading…</div>
      <div v-else-if="slotsError" class="p-4 text-red-600 text-sm">{{ slotsError }}</div>
      <div v-else-if="slots.length === 0" class="p-8 text-center text-text/40 text-sm">
        No slots for this date. Use "Generate Slots" to create them.
      </div>
      <table v-else class="w-full text-sm">
        <thead class="bg-gray-50 border-b border-gray-100">
          <tr>
            <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Time</th>
            <th class="text-center px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Booked</th>
            <th class="text-center px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Max</th>
            <th class="text-left px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Status</th>
            <th class="text-right px-4 py-3 font-semibold text-text/60 text-xs uppercase tracking-wide">Actions</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-50">
          <tr v-for="slot in slots" :key="slot.id" :class="slot.isBlocked ? 'bg-red-50/50' : ''">
            <td class="px-4 py-3 font-medium text-text">{{ slot.startTime }}</td>
            <td class="px-4 py-3 text-center text-text/70">{{ slot.bookingCount }}</td>
            <td class="px-4 py-3 text-center text-text/70">{{ slot.maxPlayers }}</td>
            <td class="px-4 py-3">
              <span v-if="slot.isBlocked" class="text-xs font-semibold text-red-600 bg-red-100 px-2 py-0.5 rounded">
                Blocked{{ slot.blockReason ? ` — ${slot.blockReason}` : '' }}
              </span>
              <span v-else-if="slot.bookingCount >= slot.maxPlayers" class="text-xs font-semibold text-amber-600 bg-amber-100 px-2 py-0.5 rounded">Full</span>
              <span v-else class="text-xs font-semibold text-green-600 bg-green-100 px-2 py-0.5 rounded">Open</span>
            </td>
            <td class="px-4 py-3 text-right">
              <button
                v-if="slot.isBlocked"
                :disabled="blockingId === slot.id"
                class="text-xs text-primary hover:underline disabled:opacity-50"
                @click="unblockSlot(slot.id)"
              >
                Unblock
              </button>
              <button
                v-else
                :disabled="blockingId === slot.id"
                class="text-xs text-red-600 hover:underline disabled:opacity-50"
                @click="openBlockModal(slot.id)"
              >
                Block
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Block reason modal -->
    <div v-if="showBlockModal" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
      <div class="bg-surface rounded-lg shadow-xl p-6 w-full max-w-sm">
        <h3 class="font-semibold text-text mb-4">Block Slot</h3>
        <div class="mb-4">
          <label class="block text-sm font-medium text-text mb-1" for="block-reason">Reason (optional)</label>
          <input
            id="block-reason"
            v-model="blockReasonInput"
            type="text"
            placeholder="e.g. Course maintenance"
            class="w-full border border-gray-200 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
          >
        </div>
        <div v-if="blockError" class="text-red-600 text-sm mb-3">{{ blockError }}</div>
        <div class="flex gap-3">
          <button class="flex-1 py-2 bg-red-600 text-white text-sm font-semibold rounded hover:opacity-90" @click="blockSlot">Block</button>
          <button class="flex-1 py-2 bg-gray-100 text-text text-sm font-semibold rounded hover:bg-gray-200" @click="showBlockModal = false">Cancel</button>
        </div>
      </div>
    </div>
  </div>
</template>
