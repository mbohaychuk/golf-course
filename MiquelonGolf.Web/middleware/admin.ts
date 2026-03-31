// MiquelonGolf.Web/middleware/admin.ts
export default defineNuxtRouteMiddleware((to) => {
  if (to.path === '/admin/login') return
  const { isAdmin } = useAuth()
  if (!isAdmin.value) return navigateTo('/admin/login')
})
