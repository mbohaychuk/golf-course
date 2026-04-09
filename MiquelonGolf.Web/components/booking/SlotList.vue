<script setup lang="ts">
import { ref, computed } from 'vue'
import type { TeeTimeSlotDto } from '~/types/api'

const props = defineProps<{
  slots: TeeTimeSlotDto[]
  loading: boolean
}>()

const selectedId = defineModel<string | null>('selectedId', { default: null })

const filter = ref<'all' | 'morning' | 'afternoon' | 'twilight'>('all')

const filters = [
  { key: 'all' as const, label: 'All Times' },
  { key: 'morning' as const, label: 'Morning' },
  { key: 'afternoon' as const, label: 'Afternoon' },
  { key: 'twilight' as const, label: 'Twilight' },
]

const filteredSlots = computed(() => {
  return props.slots.filter(s => {
    const hour = parseInt(s.startTime.split(':')[0])
    if (filter.value === 'morning') return hour < 12
    if (filter.value === 'afternoon') return hour >= 12 && hour < 16
    if (filter.value === 'twilight') return hour >= 16
    return true
  })
})

function formatTime(t: string) {
  const [h, m] = t.split(':').map(Number)
  const ampm = h >= 12 ? 'PM' : 'AM'
  const hour = h % 12 || 12
  return `${hour}:${String(m).padStart(2, '0')} ${ampm}`
}

function isBooked(slot: TeeTimeSlotDto) {
  return slot.bookingCount >= slot.maxPlayers || slot.isBlocked
}
</script>

<template>
  <div>
    <div class="font-bold text-accent text-sm mb-3 font-display">Available Tee Times</div>

    <!-- Filter tabs -->
    <div class="flex gap-1 mb-4">
      <button
        v-for="f in filters"
        :key="f.key"
        class="px-4 py-1.5 rounded-full text-xs font-semibold transition-colors"
        :class="filter === f.key
          ? 'bg-primary text-white'
          : 'bg-white border border-gray-300 text-text/60 hover:border-primary/40'"
        @click="filter = f.key"
      >{{ f.label }}</button>
    </div>

    <!-- Loading skeleton -->
    <div v-if="loading" class="flex flex-col gap-2">
      <div v-for="i in 6" :key="i" class="h-12 bg-gray-100 rounded-lg animate-pulse" />
    </div>

    <!-- Slot list -->
    <div v-else class="flex flex-col gap-2 max-h-[380px] overflow-y-auto pr-1">
      <div v-if="filteredSlots.length === 0" class="text-center text-text/50 py-8 text-sm">
        No tee times available for this filter.
      </div>

      <button
        v-for="slot in filteredSlots"
        :key="slot.id"
        :disabled="isBooked(slot)"
        class="rounded-lg px-4 py-3 flex items-center justify-between transition-all text-left"
        :class="{
          'bg-primary border-2 border-accent shadow-md': selectedId === slot.id,
          'bg-white border border-gray-200 shadow-sm hover:border-primary/40 cursor-pointer': !isBooked(slot) && selectedId !== slot.id,
          'bg-gray-50 border border-gray-200 opacity-50 cursor-default': isBooked(slot),
        }"
        :aria-disabled="isBooked(slot)"
        @click="!isBooked(slot) && (selectedId = slot.id)"
      >
        <span
          class="font-semibold text-[15px]"
          :class="selectedId === slot.id ? 'text-white' : isBooked(slot) ? 'text-gray-400' : 'text-text'"
        >{{ formatTime(slot.startTime) }}</span>

        <span v-if="selectedId === slot.id" class="text-white text-lg">&#x2713;</span>
        <span
          v-else-if="isBooked(slot)"
          class="text-xs font-semibold text-gray-400 bg-gray-100 px-2 py-0.5 rounded"
        >Booked</span>
        <span
          v-else
          class="text-xs font-semibold text-primary bg-primary/10 px-2 py-0.5 rounded"
        >Available</span>
      </button>
    </div>
  </div>
</template>
