<!-- MiquelonGolf.Web/components/booking/SlotGrid.vue -->
<script setup lang="ts">
import type { TeeTimeSlotDto } from '~/types/api'

const props = defineProps<{
  slots: TeeTimeSlotDto[]
  selectedSlotId: string | null
}>()

const emit = defineEmits<{
  select: [slot: TeeTimeSlotDto]
}>()

const slotStatus = (slot: TeeTimeSlotDto): 'available' | 'full' | 'blocked' => {
  if (slot.isBlocked) return 'blocked'
  if (slot.bookingCount >= slot.maxPlayers) return 'full'
  return 'available'
}

const spotsLeft = (slot: TeeTimeSlotDto): number =>
  Math.max(0, slot.maxPlayers - slot.bookingCount)
</script>

<template>
  <div class="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-6 gap-2">
    <button
      v-for="slot in slots"
      :key="slot.id"
      :disabled="slotStatus(slot) !== 'available'"
      :data-testid="`slot-${slotStatus(slot)}`"
      :class="[
        'flex flex-col items-center px-3 py-2 rounded text-sm font-medium transition-colors border',
        slotStatus(slot) === 'available' && selectedSlotId === slot.id
          ? 'bg-primary text-white border-primary'
          : slotStatus(slot) === 'available'
          ? 'bg-surface border-primary/20 text-text hover:bg-primary hover:text-white hover:border-primary'
          : 'bg-gray-100 text-gray-400 cursor-not-allowed border-gray-200',
      ]"
      @click="slotStatus(slot) === 'available' && emit('select', slot)"
    >
      <span>{{ slot.startTime }}</span>
      <span class="text-xs mt-0.5 opacity-70">
        <template v-if="slotStatus(slot) === 'blocked'">Unavailable</template>
        <template v-else-if="slotStatus(slot) === 'full'">Full</template>
        <template v-else>{{ spotsLeft(slot) }} left</template>
      </span>
    </button>
  </div>
</template>
