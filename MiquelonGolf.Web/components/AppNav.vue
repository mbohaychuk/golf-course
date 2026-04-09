<!-- components/AppNav.vue -->
<script setup lang="ts">
const mobileOpen = ref(false)
const golfOpen = ref(false)
const golfDropdownRef = ref<HTMLElement | null>(null)

function handleClickOutside(event: MouseEvent) {
  if (golfDropdownRef.value && !golfDropdownRef.value.contains(event.target as Node)) {
    golfOpen.value = false
  }
}

function handleKeydown(e: KeyboardEvent) {
  if (e.key === 'Escape') {
    golfOpen.value = false
    mobileOpen.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
  document.addEventListener('keydown', handleKeydown)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside)
  document.removeEventListener('keydown', handleKeydown)
})
</script>

<template>
  <nav class="sticky top-0 z-50 bg-surface shadow-sm border-b" style="border-color: color-mix(in srgb, var(--color-primary) 15%, transparent);">
    <div class="max-w-6xl mx-auto px-4 flex items-center justify-between h-16">
      <!-- Logo / Site Name -->
      <NuxtLink to="/" class="font-display text-xl font-bold text-accent">
        Miquelon Hills Golf
      </NuxtLink>

      <!-- Desktop links -->
      <ul class="hidden md:flex items-center gap-6 text-sm font-medium text-text">
        <li ref="golfDropdownRef" class="relative group">
          <button
            class="flex items-center gap-1 hover:text-accent transition-colors"
            :aria-expanded="golfOpen"
            aria-haspopup="true"
            @click="golfOpen = !golfOpen"
            @keydown.escape="golfOpen = false"
          >
            Golf
            <svg class="w-3 h-3" viewBox="0 0 12 12" fill="currentColor" aria-hidden="true">
              <path d="M2 4l4 4 4-4" stroke="currentColor" stroke-width="1.5" fill="none" stroke-linecap="round"/>
            </svg>
          </button>
          <Transition
            enter-active-class="transition ease-out duration-150"
            enter-from-class="opacity-0 -translate-y-1"
            enter-to-class="opacity-100 translate-y-0"
            leave-active-class="transition ease-in duration-100"
            leave-from-class="opacity-100 translate-y-0"
            leave-to-class="opacity-0 -translate-y-1"
          >
            <ul
              v-show="golfOpen"
              id="golf-submenu"
              role="menu"
              class="absolute top-full left-0 mt-1 w-44 bg-surface rounded shadow-md border border-primary/15 py-1 text-sm"
            >
              <li role="none"><NuxtLink role="menuitem" to="/golf/fees" class="block px-4 py-2 hover:bg-background" @click="golfOpen=false">Fees & Memberships</NuxtLink></li>
              <li role="none"><NuxtLink role="menuitem" to="/golf/course" class="block px-4 py-2 hover:bg-background" @click="golfOpen=false">Course Layout</NuxtLink></li>
              <li role="none"><NuxtLink role="menuitem" to="/golf/hours" class="block px-4 py-2 hover:bg-background" @click="golfOpen=false">Hours</NuxtLink></li>
            </ul>
          </Transition>
        </li>
        <li><NuxtLink to="/events" class="hover:text-accent transition-colors">Events</NuxtLink></li>
        <li><NuxtLink to="/rv" class="hover:text-accent transition-colors">Stay &amp; Play</NuxtLink></li>
        <li><NuxtLink to="/about" class="hover:text-accent transition-colors">About</NuxtLink></li>
        <li><NuxtLink to="/contact" class="hover:text-accent transition-colors">Contact</NuxtLink></li>
      </ul>

      <!-- Book CTA -->
      <NuxtLink
        to="/book"
        class="hidden md:inline-flex items-center px-4 py-2 bg-primary text-white text-sm font-semibold rounded hover:opacity-90 transition-opacity"
      >
        Book a Tee Time
      </NuxtLink>

      <!-- Mobile hamburger -->
      <button
        class="md:hidden p-2 text-text"
        aria-label="Toggle menu"
        :aria-expanded="mobileOpen"
        aria-controls="mobile-menu"
        @click="mobileOpen = !mobileOpen"
      >
        <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
          <path v-if="!mobileOpen" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"/>
          <path v-else stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
        </svg>
      </button>
    </div>

    <!-- Mobile menu -->
    <Transition
      enter-active-class="transition ease-out duration-200"
      enter-from-class="opacity-0 -translate-y-1"
      enter-to-class="opacity-100 translate-y-0"
      leave-active-class="transition ease-in duration-150"
      leave-from-class="opacity-100 translate-y-0"
      leave-to-class="opacity-0 -translate-y-1"
    >
      <div v-show="mobileOpen" id="mobile-menu" class="md:hidden bg-surface border-t border-primary/10 px-4 pb-4">
        <ul class="flex flex-col gap-3 pt-3 text-sm font-medium text-text">
          <li><NuxtLink to="/golf/fees" @click="mobileOpen=false">Fees &amp; Memberships</NuxtLink></li>
          <li><NuxtLink to="/golf/course" @click="mobileOpen=false">Course Layout</NuxtLink></li>
          <li><NuxtLink to="/golf/hours" @click="mobileOpen=false">Hours</NuxtLink></li>
          <li><NuxtLink to="/events" @click="mobileOpen=false">Events</NuxtLink></li>
          <li><NuxtLink to="/rv" @click="mobileOpen=false">Stay &amp; Play</NuxtLink></li>
          <li><NuxtLink to="/about" @click="mobileOpen=false">About</NuxtLink></li>
          <li><NuxtLink to="/contact" @click="mobileOpen=false">Contact</NuxtLink></li>
          <li>
            <NuxtLink
              to="/book"
              class="inline-flex items-center px-4 py-2 bg-primary text-white text-sm font-semibold rounded"
              @click="mobileOpen=false"
            >
              Book a Tee Time
            </NuxtLink>
          </li>
        </ul>
      </div>
    </Transition>
  </nav>
</template>
