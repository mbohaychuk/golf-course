<script setup lang="ts">
import type { EventDto } from '~/types/api'

const props = defineProps<{
  event: EventDto
}>()

const formattedDate = computed(() => {
  const d = new Date(props.event.eventDate + 'T00:00:00')
  return d.toLocaleDateString('en-CA', { year: 'numeric', month: 'long', day: 'numeric' })
})

const categoryLabel: Record<EventDto['category'], string> = {
  Tournament:  'Tournament',
  SocialNight: 'Social Night',
  LadiesNight: "Ladies' Night",
  MensNight:   "Men's Night",
  Other:       'Event',
}
</script>

<template>
  <article class="bg-surface rounded-lg shadow-sm overflow-hidden flex flex-col">
    <img
      v-if="event.imageUrl"
      :src="event.imageUrl"
      :alt="event.title"
      class="w-full h-40 object-cover"
    >
    <div class="p-4 flex flex-col gap-2 flex-1">
      <div class="flex items-center gap-2 flex-wrap">
        <span class="text-xs font-semibold uppercase tracking-wide text-accent bg-background px-2 py-0.5 rounded">
          {{ categoryLabel[event.category] }}
        </span>
        <span
          v-if="!event.isPublic"
          data-testid="members-badge"
          class="text-xs font-semibold uppercase tracking-wide text-primary bg-green-50 border border-primary/30 px-2 py-0.5 rounded"
        >
          Members Only
        </span>
      </div>
      <h3 class="font-display text-lg font-bold text-accent leading-snug">{{ event.title }}</h3>
      <p class="text-sm text-text/70">{{ formattedDate }}<template v-if="event.startTime"> · {{ event.startTime.slice(0,5) }}</template></p>
      <p class="text-sm text-text leading-relaxed flex-1">{{ event.description }}</p>
    </div>
  </article>
</template>
