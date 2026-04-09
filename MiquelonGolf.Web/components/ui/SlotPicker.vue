<script setup lang="ts">
import type { TeeTimeSlotDto } from '~/types/api'

const props = defineProps<{
  open: boolean
  slots: TeeTimeSlotDto[]
  currentSlotId: string
}>()

const emit = defineEmits<{
  select: [slotId: string]
  cancel: []
}>()

function formatTime(time: string): string {
  const [h, m] = time.split(':').map(Number)
  const suffix = h >= 12 ? 'PM' : 'AM'
  const hour12 = h % 12 || 12
  return `${hour12}:${String(m).padStart(2, '0')} ${suffix}`
}

function isAvailable(slot: TeeTimeSlotDto): boolean {
  return !slot.isBlocked && slot.bookingCount < slot.maxPlayers && slot.id !== props.currentSlotId
}

function spotsLeft(slot: TeeTimeSlotDto): number {
  return slot.maxPlayers - slot.bookingCount
}
</script>

<template>
  <Teleport to="body">
    <div v-if="open" class="fixed inset-0 z-50 flex items-center justify-center">
      <div class="absolute inset-0 bg-black/40" @click="emit('cancel')" />
      <div class="relative bg-white rounded-lg shadow-xl max-w-md w-full mx-4 flex flex-col max-h-[80vh]">
        <div class="px-6 pt-6 pb-3 border-b border-gray-100">
          <h3 class="text-lg font-bold text-text">Move Booking</h3>
          <p class="text-sm text-text/60 mt-1">Select a new tee time slot.</p>
        </div>

        <div class="flex-1 overflow-y-auto px-6 py-3">
          <div v-if="slots.length === 0" class="py-8 text-center text-text/40 text-sm">
            No slots available.
          </div>
          <ul v-else class="space-y-1.5">
            <li v-for="slot in slots" :key="slot.id">
              <button
                :disabled="!isAvailable(slot)"
                class="w-full text-left px-4 py-3 rounded-lg text-sm transition-colors flex items-center justify-between gap-3"
                :class="[
                  !isAvailable(slot) ? 'bg-gray-50 text-gray-400 cursor-not-allowed' : 'hover:bg-primary/5 hover:text-primary cursor-pointer',
                  slot.isBlocked ? 'line-through' : '',
                ]"
                @click="isAvailable(slot) && emit('select', slot.id)"
              >
                <span class="font-medium">{{ formatTime(slot.startTime) }}</span>
                <span v-if="slot.id === currentSlotId" class="text-xs text-gray-400 italic">Current</span>
                <span v-else-if="slot.isBlocked" class="text-xs text-red-400">Blocked</span>
                <span v-else-if="slot.bookingCount >= slot.maxPlayers" class="text-xs text-amber-500">Full</span>
                <span v-else class="text-xs text-green-600">{{ spotsLeft(slot) }} open</span>
              </button>
            </li>
          </ul>
        </div>

        <div class="px-6 py-4 border-t border-gray-100 flex justify-end">
          <button
            class="px-4 py-2 text-sm font-medium text-text/70 hover:text-text rounded-lg hover:bg-gray-100"
            @click="emit('cancel')"
          >Cancel</button>
        </div>
      </div>
    </div>
  </Teleport>
</template>
