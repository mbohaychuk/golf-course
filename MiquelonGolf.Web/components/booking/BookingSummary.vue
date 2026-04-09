<script setup lang="ts">
import type { RoundType } from '~/types/api'

defineProps<{
  date: string | null
  time: string | null
  roundType: RoundType
}>()

const roundLabels: Record<RoundType, string> = {
  Eighteen: '18 Holes',
  FrontNine: '9 Holes (Front)',
  BackNine: 'Back 9',
}

function formatDate(d: string) {
  const dt = new Date(d + 'T00:00:00')
  return dt.toLocaleDateString('en-CA', { weekday: 'short', month: 'long', day: 'numeric' })
}

function formatTime(t: string) {
  const [h, m] = t.split(':').map(Number)
  const ampm = h >= 12 ? 'PM' : 'AM'
  return `${h % 12 || 12}:${String(m).padStart(2, '0')} ${ampm}`
}
</script>

<template>
  <div v-if="date" class="bg-white rounded-lg p-3.5 shadow-sm border-l-4 border-accent">
    <div class="font-bold text-text text-[15px]">{{ formatDate(date) }}</div>
    <div class="text-xs text-text/60 mt-0.5">
      {{ roundLabels[roundType] }}
      <template v-if="time"> &middot; {{ formatTime(time) }}</template>
    </div>
  </div>
</template>
