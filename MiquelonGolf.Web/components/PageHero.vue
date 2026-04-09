<script setup lang="ts">
defineProps<{
  title: string
  subtitle?: string
}>()
</script>

<template>
  <section class="bg-primary relative overflow-hidden">
    <div class="absolute inset-0 bg-gradient-to-br from-forest/40 to-transparent" />
    <div class="max-w-6xl mx-auto px-4 py-14 md:py-20 relative z-10">
      <h1 class="font-display text-4xl md:text-5xl font-bold text-white leading-tight">{{ title }}</h1>
      <p v-if="subtitle" class="mt-3 text-white/70 text-lg max-w-2xl">{{ subtitle }}</p>
    </div>
  </section>
</template>
