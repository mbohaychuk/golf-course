<script setup lang="ts">
defineProps<{
  currentStep: number
  steps: string[]
}>()
</script>

<template>
  <div class="flex gap-2" role="navigation" aria-label="Booking progress">
    <span
      v-for="(label, i) in steps"
      :key="i"
      class="px-3 py-1 rounded-full text-xs font-semibold transition-colors"
      :class="i + 1 <= currentStep
        ? 'bg-primary text-white'
        : 'bg-gray-200 text-gray-400'"
      :aria-current="i + 1 === currentStep ? 'step' : undefined"
    >{{ i + 1 }}. {{ label }}</span>
  </div>
</template>
