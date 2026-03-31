// MiquelonGolf.Web/nuxt.config.ts
export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',
  devtools: { enabled: true },

  modules: [
    '@nuxtjs/tailwindcss',
    '@nuxtjs/google-fonts',
    '@nuxt/test-utils/module',
  ],

  googleFonts: {
    families: {
      'Playfair+Display': [400, 700],
      Inter: [400, 500, 600],
    },
    display: 'swap',
  },

  css: ['~/assets/css/main.css'],

  runtimeConfig: {
    public: {
      apiBase: process.env.API_BASE ?? 'http://localhost:5000',
    },
  },

  ssr: true,
})
