<script setup lang="ts">
import { ref, computed } from 'vue'

const props = withDefaults(defineProps<{
  modelValue: string | null
  minDate: string
  maxDate: string
  closedDays?: number[]
  holidays?: string[]
}>(), {
  closedDays: () => [],
  holidays: () => []
})

const emit = defineEmits<{ 'update:modelValue': [value: string] }>()

const viewMonth = ref(new Date().getMonth())
const viewYear = ref(new Date().getFullYear())

const dayLabels = ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa']
const monthNames = ['January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December']

const calendarDays = computed(() => {
  const first = new Date(viewYear.value, viewMonth.value, 1)
  const startDay = first.getDay()
  const daysInMonth = new Date(viewYear.value, viewMonth.value + 1, 0).getDate()
  const days: { date: string; day: number; inMonth: boolean; selectable: boolean; closed: boolean }[] = []

  // Previous month overflow
  const prevMonthDays = new Date(viewYear.value, viewMonth.value, 0).getDate()
  for (let i = startDay - 1; i >= 0; i--) {
    const d = prevMonthDays - i
    days.push({ date: '', day: d, inMonth: false, selectable: false, closed: false })
  }

  // Current month
  for (let d = 1; d <= daysInMonth; d++) {
    const dt = new Date(viewYear.value, viewMonth.value, d)
    const dateStr = `${viewYear.value}-${String(viewMonth.value + 1).padStart(2, '0')}-${String(d).padStart(2, '0')}`
    const dow = dt.getDay()
    const isClosed = props.closedDays.includes(dow) || props.holidays.includes(dateStr)
    const inRange = dateStr >= props.minDate && dateStr <= props.maxDate
    days.push({ date: dateStr, day: d, inMonth: true, selectable: inRange && !isClosed, closed: isClosed })
  }

  return days
})

function prevMonth() {
  if (viewMonth.value === 0) { viewMonth.value = 11; viewYear.value-- }
  else viewMonth.value--
}

function nextMonth() {
  if (viewMonth.value === 11) { viewMonth.value = 0; viewYear.value++ }
  else viewMonth.value++
}

function selectDate(day: typeof calendarDays.value[0]) {
  if (day.selectable) emit('update:modelValue', day.date)
}
</script>

<template>
  <div class="bg-white rounded-lg p-4 shadow-sm">
    <div class="flex items-center justify-between mb-3">
      <span class="font-bold text-text text-[15px]">{{ monthNames[viewMonth] }} {{ viewYear }}</span>
      <div class="flex gap-1">
        <button class="w-7 h-7 bg-gray-100 rounded flex items-center justify-center text-sm" @click="prevMonth">&#x2039;</button>
        <button class="w-7 h-7 bg-gray-100 rounded flex items-center justify-center text-sm" @click="nextMonth">&#x203a;</button>
      </div>
    </div>
    <div class="grid grid-cols-7 gap-px text-center text-xs">
      <span v-for="label in dayLabels" :key="label" class="py-1.5 font-bold text-text/50">{{ label }}</span>
      <button
        v-for="(day, i) in calendarDays"
        :key="i"
        :disabled="!day.selectable"
        class="py-1.5 rounded text-xs font-medium transition-colors"
        :class="{
          'text-gray-300': !day.inMonth,
          'text-gray-300 line-through': day.inMonth && day.closed,
          'text-gray-300 cursor-default': day.inMonth && !day.selectable && !day.closed,
          'text-text/80 font-medium cursor-pointer hover:bg-primary/10': day.selectable && day.date !== modelValue,
          'bg-primary text-white font-bold rounded-md': day.date === modelValue,
        }"
        @click="selectDate(day)"
      >{{ day.day }}</button>
    </div>
  </div>
</template>
