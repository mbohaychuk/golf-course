<!-- components/AppFooter.vue -->
<template>
  <footer class="bg-primary text-white mt-16">
    <div class="h-1 bg-accent/60" />
    <div class="max-w-6xl mx-auto px-4 py-10 grid grid-cols-1 md:grid-cols-3 gap-8 text-sm">
      <!-- Address & Contact -->
      <div>
        <h3 class="font-display text-lg font-bold text-white mb-1">Miquelon Hills Golf Course</h3>
        <div class="w-8 h-0.5 bg-accent/60 mb-3" />
        <p class="text-white/80 leading-relaxed">
          Camrose County, Alberta<br>
          Bordering Miquelon Lake Provincial Park
        </p>
        <p class="mt-2 text-white/80">
          <a href="tel:+17804732511" class="hover:text-white transition-colors">(780) 473-2511</a>
        </p>
        <p class="text-white/80">
          <a href="mailto:info@miquelonhillsgolf.com" class="hover:text-white transition-colors">
            info@miquelonhillsgolf.com
          </a>
        </p>
      </div>

      <!-- Quick Links -->
      <div>
        <h3 class="font-display text-lg font-bold text-white mb-1">Quick Links</h3>
        <div class="w-8 h-0.5 bg-accent/60 mb-3" />
        <ul class="space-y-1 text-white/80">
          <li><NuxtLink to="/book" class="hover:text-white transition-colors">Book a Tee Time</NuxtLink></li>
          <li><NuxtLink to="/golf/fees" class="hover:text-white transition-colors">Fees &amp; Memberships</NuxtLink></li>
          <li><NuxtLink to="/golf/course" class="hover:text-white transition-colors">Course Layout</NuxtLink></li>
          <li><NuxtLink to="/events" class="hover:text-white transition-colors">Events</NuxtLink></li>
          <li><NuxtLink to="/rv" class="hover:text-white transition-colors">Stay &amp; Play</NuxtLink></li>
          <li><NuxtLink to="/contact" class="hover:text-white transition-colors">Contact</NuxtLink></li>
        </ul>
      </div>

      <!-- Hours & Social -->
      <div>
        <h3 class="font-display text-lg font-bold text-white mb-1">Season Hours</h3>
        <div class="w-8 h-0.5 bg-accent/60 mb-3" />
        <p class="text-white/80 text-sm leading-relaxed">
          Open May – October<br>
          Daily: Dawn to Dusk
        </p>
        <div class="flex gap-3 mt-4">
          <a href="https://www.facebook.com/miquelonhillsgolf" target="_blank" rel="noopener" aria-label="Facebook" class="text-white/80 hover:text-white transition-colors">
            <span class="w-8 h-8 rounded-full flex items-center justify-center hover:bg-white/15 transition-colors">
              <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                <path d="M18 2h-3a5 5 0 00-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 011-1h3z"/>
              </svg>
            </span>
          </a>
          <a href="https://www.instagram.com/miquelonhillsgolf" target="_blank" rel="noopener" aria-label="Instagram" class="text-white/80 hover:text-white transition-colors">
            <span class="w-8 h-8 rounded-full flex items-center justify-center hover:bg-white/15 transition-colors">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <rect x="2" y="2" width="20" height="20" rx="5" ry="5" stroke-width="2"/>
                <circle cx="12" cy="12" r="4" stroke-width="2"/>
                <circle cx="17.5" cy="6.5" r="1" fill="currentColor"/>
              </svg>
            </span>
          </a>
        </div>
      </div>
    </div>
    <div class="border-t border-white/20 text-center text-white/50 text-xs py-4">
      &copy; {{ new Date().getFullYear() }} Miquelon Hills Golf Course. All rights reserved.
    </div>
  </footer>
</template>
