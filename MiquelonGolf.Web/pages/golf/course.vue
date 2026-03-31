<script setup lang="ts">
import type { HoleDto } from '~/types/api'

useSeoMeta({
  title: 'Course Layout — Miquelon Hills Golf Course',
  description: 'Explore all 18 holes of Miquelon Hills Golf Course — par, yardage, handicap, and strategy for every hole.',
})

const api = useApi()
const { data: holes } = await api.get<HoleDto[]>('/holes')

const activeHole = ref(1)

const currentHole = computed(() =>
  holes.value?.find(h => h.holeNumber === activeHole.value) ?? null
)

const frontNine = computed(() => holes.value?.filter(h => h.holeNumber <= 9) ?? [])
const backNine  = computed(() => holes.value?.filter(h => h.holeNumber >= 10) ?? [])

const totalPar = (tee: 'Blue' | 'White' | 'Red') =>
  holes.value?.reduce((s, h) => {
    if (tee === 'Blue')  return s + h.yardageBlue
    if (tee === 'White') return s + h.yardageWhite
    return s + h.yardageRed
  }, 0) ?? 0

const totalParValue = computed(() => holes.value?.reduce((s, h) => s + h.par, 0) ?? 0)
</script>

<template>
  <div class="max-w-6xl mx-auto px-4 py-12">
    <h1 class="font-display text-4xl font-bold text-accent mb-2">Course Layout</h1>
    <p class="text-text/70 mb-8">18 holes of public golf — open front nine, tight forest back nine.</p>

    <!-- Hole tab strip -->
    <div class="mb-6">
      <div class="flex flex-wrap gap-1 mb-1">
        <span class="text-xs font-semibold text-text/40 mr-2 self-end pb-1">Front 9</span>
        <button
          v-for="h in frontNine"
          :key="h.holeNumber"
          :class="[
            'w-9 h-9 rounded text-sm font-semibold transition-colors',
            activeHole === h.holeNumber
              ? 'bg-primary text-white'
              : 'bg-surface text-text hover:bg-primary/10'
          ]"
          @click="activeHole = h.holeNumber"
        >
          {{ h.holeNumber }}
        </button>
      </div>
      <div class="flex flex-wrap gap-1">
        <span class="text-xs font-semibold text-text/40 mr-2 self-end pb-1">Back 9</span>
        <button
          v-for="h in backNine"
          :key="h.holeNumber"
          :class="[
            'w-9 h-9 rounded text-sm font-semibold transition-colors',
            activeHole === h.holeNumber
              ? 'bg-primary text-white'
              : 'bg-surface text-text hover:bg-primary/10'
          ]"
          @click="activeHole = h.holeNumber"
        >
          {{ h.holeNumber }}
        </button>
      </div>
    </div>

    <!-- Active hole panel -->
    <div v-if="currentHole" class="bg-surface rounded-lg shadow-sm p-6 mb-10 grid grid-cols-1 md:grid-cols-2 gap-6">
      <div>
        <h2 class="font-display text-2xl font-bold text-accent mb-4">Hole {{ currentHole.holeNumber }}</h2>
        <div class="grid grid-cols-3 gap-3 mb-4">
          <div class="bg-background rounded p-3 text-center">
            <p class="text-xs text-text/50 uppercase tracking-wide mb-1">Par</p>
            <p class="font-display text-2xl font-bold text-accent">{{ currentHole.par }}</p>
          </div>
          <div class="bg-background rounded p-3 text-center">
            <p class="text-xs text-text/50 uppercase tracking-wide mb-1">Handicap</p>
            <p class="font-display text-2xl font-bold text-accent">{{ currentHole.handicap }}</p>
          </div>
          <div class="bg-background rounded p-3 text-center">
            <p class="text-xs text-text/50 uppercase tracking-wide mb-1">Blue</p>
            <p class="font-display text-2xl font-bold text-accent">{{ currentHole.yardageBlue }}</p>
          </div>
        </div>
        <div class="grid grid-cols-2 gap-3 mb-4">
          <div class="bg-background rounded p-3 text-center">
            <p class="text-xs text-text/50 uppercase tracking-wide mb-1">White</p>
            <p class="font-bold text-lg text-text">{{ currentHole.yardageWhite }}</p>
          </div>
          <div class="bg-background rounded p-3 text-center">
            <p class="text-xs text-text/50 uppercase tracking-wide mb-1">Red</p>
            <p class="font-bold text-lg text-text">{{ currentHole.yardageRed }}</p>
          </div>
        </div>
        <p v-if="currentHole.description" class="text-sm text-text leading-relaxed">
          {{ currentHole.description }}
        </p>
      </div>
      <div v-if="currentHole.diagramUrl || currentHole.imageUrl" class="flex items-center justify-center bg-background rounded">
        <img
          :src="currentHole.diagramUrl ?? currentHole.imageUrl ?? ''"
          :alt="`Hole ${currentHole.holeNumber} diagram`"
          class="max-h-64 object-contain rounded"
        >
      </div>
      <div v-else class="hidden md:flex items-center justify-center bg-background rounded text-text/20 text-sm h-48">
        Hole diagram
      </div>
    </div>

    <!-- Full scorecard -->
    <h2 class="font-display text-2xl font-bold text-accent mb-4">Full Scorecard</h2>
    <div class="bg-surface rounded-lg shadow-sm overflow-x-auto">
      <table class="w-full text-sm min-w-[600px]">
        <thead class="bg-primary text-white">
          <tr>
            <th class="text-left px-3 py-2.5 font-semibold">Hole</th>
            <th class="text-center px-3 py-2.5 font-semibold">Par</th>
            <th class="text-center px-3 py-2.5 font-semibold">Hdcp</th>
            <th class="text-center px-3 py-2.5 font-semibold">Blue</th>
            <th class="text-center px-3 py-2.5 font-semibold">White</th>
            <th class="text-center px-3 py-2.5 font-semibold">Red</th>
          </tr>
        </thead>
        <tbody v-if="holes">
          <template v-for="group in [frontNine, backNine]" :key="group[0]?.holeNumber">
            <tr
              v-for="h in group"
              :key="h.holeNumber"
              :class="['divide-x divide-gray-100 cursor-pointer', activeHole === h.holeNumber ? 'bg-primary/5' : 'hover:bg-background']"
              @click="activeHole = h.holeNumber"
            >
              <td class="px-3 py-2.5 font-medium">{{ h.holeNumber }}</td>
              <td class="px-3 py-2.5 text-center">{{ h.par }}</td>
              <td class="px-3 py-2.5 text-center text-text/60">{{ h.handicap }}</td>
              <td class="px-3 py-2.5 text-center">{{ h.yardageBlue }}</td>
              <td class="px-3 py-2.5 text-center">{{ h.yardageWhite }}</td>
              <td class="px-3 py-2.5 text-center">{{ h.yardageRed }}</td>
            </tr>
            <tr class="bg-primary/5 font-semibold text-sm border-t border-primary/20">
              <td class="px-3 py-2.5">{{ group[0]?.holeNumber <= 9 ? 'Front 9' : 'Back 9' }}</td>
              <td class="px-3 py-2.5 text-center">{{ group.reduce((s, h) => s + h.par, 0) }}</td>
              <td class="px-3 py-2.5"></td>
              <td class="px-3 py-2.5 text-center">{{ group.reduce((s, h) => s + h.yardageBlue, 0) }}</td>
              <td class="px-3 py-2.5 text-center">{{ group.reduce((s, h) => s + h.yardageWhite, 0) }}</td>
              <td class="px-3 py-2.5 text-center">{{ group.reduce((s, h) => s + h.yardageRed, 0) }}</td>
            </tr>
          </template>
          <tr class="bg-primary text-white font-bold text-sm">
            <td class="px-3 py-2.5">Total</td>
            <td class="px-3 py-2.5 text-center">{{ totalParValue }}</td>
            <td class="px-3 py-2.5"></td>
            <td class="px-3 py-2.5 text-center">{{ totalPar('Blue') }}</td>
            <td class="px-3 py-2.5 text-center">{{ totalPar('White') }}</td>
            <td class="px-3 py-2.5 text-center">{{ totalPar('Red') }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
