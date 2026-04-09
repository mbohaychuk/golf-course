<script setup lang="ts">
const props = withDefaults(defineProps<{
  open: boolean
  title: string
  message: string
  confirmText?: string
  cancelText?: string
  variant?: 'danger' | 'default'
}>(), {
  confirmText: 'Confirm',
  cancelText: 'Cancel',
  variant: 'default'
})

const emit = defineEmits<{
  confirm: []
  cancel: []
}>()
</script>

<template>
  <Teleport to="body">
    <div v-if="open" class="fixed inset-0 z-50 flex items-center justify-center">
      <div class="absolute inset-0 bg-black/40" @click="emit('cancel')" />
      <div class="relative bg-white rounded-lg shadow-xl max-w-md w-full mx-4 p-6" @keydown.escape="emit('cancel')">
        <h3 class="text-lg font-bold text-text mb-2">{{ title }}</h3>
        <p class="text-sm text-text/70 mb-6">{{ message }}</p>
        <div class="flex justify-end gap-3">
          <button
            class="px-4 py-2 text-sm font-medium text-text/70 hover:text-text rounded-lg hover:bg-gray-100"
            @click="emit('cancel')"
          >{{ cancelText }}</button>
          <button
            class="px-4 py-2 text-sm font-medium text-white rounded-lg"
            :class="variant === 'danger' ? 'bg-red-600 hover:bg-red-700' : 'bg-primary hover:bg-primary/90'"
            @click="emit('confirm')"
          >{{ confirmText }}</button>
        </div>
      </div>
    </div>
  </Teleport>
</template>
