<script setup lang="ts">
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import {
  LayoutDashboard, ClipboardList, Users, Building2, LogOut, ChevronRight,
} from 'lucide-vue-next'

defineProps<{ collapsed?: boolean }>()
defineEmits<{ toggle: [] }>()

const route = useRoute()
const auth = useAuthStore()

const navItems = [
  { label: 'Dashboard', icon: LayoutDashboard, to: '/' },
  { label: 'Encuestas', icon: ClipboardList, to: '/encuestas' },
  { label: 'Entidades', icon: Building2, to: '/entidades' },
  { label: 'Usuarios', icon: Users, to: '/usuarios' },
]

const isActive = (path: string) =>
  path === '/' ? route.path === '/' : route.path.startsWith(path)
</script>

<template>
  <aside
    :class="[
      'flex flex-col h-full bg-sidebar text-sidebar-foreground border-r border-sidebar-border transition-all duration-300',
      collapsed ? 'w-16' : 'w-64',
    ]"
  >
    <!-- Logo -->
    <div class="flex h-16 items-center border-b border-sidebar-border px-4 shrink-0">
      <div class="flex items-center gap-3 overflow-hidden">
        <div class="h-8 w-8 shrink-0 rounded-lg bg-primary flex items-center justify-center text-white font-bold text-sm">
          E
        </div>
        <Transition name="fade">
          <span v-if="!collapsed" class="font-semibold text-sm truncate">Encuestas</span>
        </Transition>
      </div>
    </div>

    <!-- Nav -->
    <nav class="flex-1 px-2 py-4 space-y-1 overflow-y-auto">
      <router-link
        v-for="item in navItems"
        :key="item.to"
        :to="item.to"
        :class="[
          'flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors',
          isActive(item.to)
            ? 'bg-sidebar-accent-foreground/10 text-sidebar-accent-foreground'
            : 'text-sidebar-foreground/60 hover:bg-sidebar-accent hover:text-sidebar-foreground',
        ]"
        :title="collapsed ? item.label : undefined"
      >
        <component :is="item.icon" class="h-5 w-5 shrink-0" />
        <Transition name="fade">
          <span v-if="!collapsed" class="truncate">{{ item.label }}</span>
        </Transition>
        <ChevronRight v-if="!collapsed && isActive(item.to)" class="ml-auto h-4 w-4 opacity-50" />
      </router-link>
    </nav>

    <!-- Footer -->
    <div class="border-t border-sidebar-border px-2 py-3 shrink-0">
      <button
        :class="[
          'flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm text-sidebar-foreground/60',
          'hover:bg-sidebar-accent hover:text-destructive transition-colors',
        ]"
        :title="collapsed ? 'Cerrar sesión' : undefined"
        @click="auth.logout(); $router.push('/login')"
      >
        <LogOut class="h-5 w-5 shrink-0" />
        <Transition name="fade">
          <span v-if="!collapsed">Cerrar sesión</span>
        </Transition>
      </button>
    </div>
  </aside>
</template>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: opacity 0.15s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>
