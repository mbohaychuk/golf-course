<script setup lang="ts">
import type { RoundType } from '~/types/api'

const model = defineModel<RoundType>({ required: true })

const options: { value: RoundType; label: string; sub: string }[] = [
  { value: 'Eighteen', label: '18 Holes', sub: 'Full round' },
  { value: 'FrontNine', label: 'Front 9', sub: 'Holes 1–9' },
  { value: 'BackNine', label: 'Back 9', sub: 'Holes 10–18' },
]
</script>

<template>
  <div>
    <div class="font-bold text-text text-sm mb-2">How many holes?</div>
    <div class="inline-flex gap-2">
      <button
        v-for="opt in options"
        :key="opt.value"
        class="rounded-md px-5 py-2 flex items-center gap-2 text-sm font-bold transition-all"
        :class="model === opt.value
          ? 'bg-primary text-white border-2 border-accent shadow-md'
          : 'bg-white text-text border border-gray-300 hover:border-primary/40'"
        @click="model = opt.value"
      >
        {{ opt.label }}
        <span
          class="text-xs font-normal"
          :class="model === opt.value ? 'text-white/60' : 'text-text/50'"
        >{{ opt.sub }}</span>
      </button>
    </div>
  </div>
</template>
