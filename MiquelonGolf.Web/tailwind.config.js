// MiquelonGolf.Web/tailwind.config.js
/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './components/**/*.{vue,js,ts}',
    './layouts/**/*.vue',
    './pages/**/*.vue',
    './composables/**/*.{js,ts}',
    './app.vue',
  ],
  theme: {
    extend: {
      colors: {
        primary:    'var(--color-primary)',
        accent:     'var(--color-accent)',
        background: 'var(--color-background)',
        surface:    'var(--color-surface)',
        text:       'var(--color-text)',
        forest:     'var(--color-forest)',
        canopy:     'var(--color-canopy)',
      },
      fontFamily: {
        display: ['"Playfair Display"', 'serif'],
        body:    ['Inter', 'sans-serif'],
      },
    },
  },
}
