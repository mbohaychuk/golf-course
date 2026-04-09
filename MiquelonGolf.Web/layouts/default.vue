<!-- layouts/default.vue -->
<template>
  <div class="min-h-screen flex flex-col bg-background">
    <a
      href="#main-content"
      class="sr-only focus:not-sr-only focus:absolute focus:top-2 focus:left-2 focus:z-[100] focus:px-4 focus:py-2 focus:bg-primary focus:text-white focus:rounded focus:outline-none"
    >
      Skip to main content
    </a>
    <AppNav />
    <main id="main-content" class="flex-1">
      <slot />
    </main>
    <AppFooter />
    <UiToast />
  </div>
</template>
