// MiquelonGolf.Web/nuxt.config.ts
export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',
  devtools: { enabled: true },

  modules: [
    '@nuxtjs/tailwindcss',
    '@nuxtjs/google-fonts',
    '@nuxtjs/sitemap',
    '@nuxt/test-utils/module',
  ],

  site: {
    url: 'https://miquelonhillsgolf.com',
  },

  sitemap: {
    exclude: ['/admin/**'],
  },

  app: {
    head: {
      htmlAttrs: { lang: 'en-CA' },
      meta: [
        { name: 'theme-color', content: '#1C4209' },
      ],
    },
  },

  googleFonts: {
    families: {
      'Playfair+Display': [400, 700, 800, 900],
      Inter: [300, 400, 500, 600],
    },
    display: 'swap',
  },

  css: ['~/assets/css/main.css'],

  runtimeConfig: {
    public: {
      apiBase: process.env.API_BASE ?? 'http://localhost:5151',
    },
  },

  ssr: true,
})
