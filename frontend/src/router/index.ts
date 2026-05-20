import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { public: true },
    },
    {
      path: '/responder/:token',
      name: 'responder',
      component: () => import('@/views/ResponderView.vue'),
      meta: { public: true },
    },
    {
      path: '/',
      component: () => import('@/components/layout/AppLayout.vue'),
      children: [
        {
          path: '',
          name: 'dashboard',
          component: () => import('@/views/DashboardView.vue'),
        },
        {
          path: 'encuestas',
          name: 'encuestas',
          component: () => import('@/views/EncuestasView.vue'),
        },
        {
          path: 'encuestas/:id',
          name: 'encuesta-detalle',
          component: () => import('@/views/EncuestaDetalleView.vue'),
        },
        {
          path: 'encuestas/:id/resultados',
          name: 'encuesta-resultados',
          component: () => import('@/views/ResultadosView.vue'),
        },
        {
          path: 'encuestas/:id/invitaciones',
          name: 'encuesta-invitaciones',
          component: () => import('@/views/InvitacionesView.vue'),
        },
        {
          path: 'entidades',
          name: 'entidades',
          component: () => import('@/views/EntidadesView.vue'),
        },
        {
          path: 'usuarios',
          name: 'usuarios',
          component: () => import('@/views/UsuariosView.vue'),
        },
      ],
    },
    { path: '/:pathMatch(.*)*', redirect: '/' },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (!to.meta.public && !auth.isAuthenticated) return '/login'
  if (to.meta.public && to.name === 'login' && auth.isAuthenticated) return '/'
})

export default router
