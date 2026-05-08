<script setup lang="ts">
import type { NuxtError } from '#app'

const props = defineProps<{ error: NuxtError }>()

const isNotFound = computed(() => props.error.statusCode === 404)
const heading = computed(() => isNotFound.value ? 'Off the fairway' : 'Something went sideways')
const subhead = computed(() =>
  isNotFound.value
    ? "We couldn't find the page you were looking for."
    : "An error occurred. Please try again, or call the pro shop if it persists."
)

useSeoMeta({
  title: () => `${props.error.statusCode ?? 'Error'} — Miquelon Hills Golf Course`,
  robots: 'noindex',
})

function handleHome() {
  clearError({ redirect: '/' })
}
</script>

<template>
  <div class="min-h-screen flex flex-col items-center justify-center bg-background text-text px-6 py-16">
    <div class="max-w-lg w-full text-center">
      <p class="font-mono text-xs uppercase tracking-[0.3em] text-accent mb-4">
        Error {{ error.statusCode ?? '' }}
      </p>
      <h1 class="font-display text-4xl md:text-5xl font-bold text-primary mb-4">
        {{ heading }}
      </h1>
      <p class="text-text/70 mb-8 leading-relaxed">
        {{ subhead }}
      </p>

      <div class="flex flex-col sm:flex-row gap-3 justify-center">
        <button
          class="inline-flex items-center justify-center px-6 py-3 bg-accent text-white font-semibold rounded-lg hover:opacity-90 transition-opacity"
          @click="handleHome"
        >
          Back to Home
        </button>
        <a
          href="tel:+17804732511"
          class="inline-flex items-center justify-center px-6 py-3 border border-primary/30 text-primary font-semibold rounded-lg hover:bg-primary/5 transition-colors"
        >
          Call (780) 473-2511
        </a>
      </div>
    </div>
  </div>
</template>
