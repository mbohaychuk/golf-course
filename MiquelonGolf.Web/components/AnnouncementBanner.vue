<script setup lang="ts">
import type { AnnouncementDto } from '~/types/api'

const props = defineProps<{
  announcements: AnnouncementDto[]
}>()

const colorClass = (type: AnnouncementDto['type']) => ({
  CourseConditions: 'bg-primary/10 border-primary/40 text-primary',
  Closure:          'bg-red-50 border-red-300 text-red-700',
  General:          'bg-accent/10 border-accent/40 text-text',
})[type]
</script>

<template>
  <div v-if="announcements.length > 0" aria-live="polite">
    <div
      v-for="a in announcements"
      :key="a.id"
      data-testid="banner"
      :class="['border-l-4 px-4 py-3 text-sm font-medium', colorClass(a.type)]"
    >
      {{ a.message }}
    </div>
  </div>
</template>
