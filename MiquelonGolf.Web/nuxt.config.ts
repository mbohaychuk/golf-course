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
      link: [
        { rel: 'icon', type: 'image/svg+xml', href: '/icon.svg' },
        { rel: 'icon', type: 'image/png', sizes: '32x32', href: '/icon-32.png' },
        { rel: 'icon', type: 'image/png', sizes: '192x192', href: '/icon-192.png' },
        { rel: 'apple-touch-icon', sizes: '180x180', href: '/apple-touch-icon.png' },
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
