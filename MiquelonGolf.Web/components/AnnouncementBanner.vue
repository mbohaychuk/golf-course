<script setup lang="ts">
import type { AnnouncementDto } from '~/types/api'

const props = defineProps<{
  announcements: AnnouncementDto[]
}>()

const colorClass = (type: AnnouncementDto['type']) => ({
  CourseConditions: 'bg-green-100 border-green-400 text-green-800',
  Closure:          'bg-red-100 border-red-400 text-red-800',
  General:          'bg-amber-50 border-amber-400 text-amber-800',
})[type]
</script>

<template>
  <div v-if="announcements.length > 0">
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
