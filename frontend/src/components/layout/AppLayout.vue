<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute } from 'vue-router'
import AppSidebar from './AppSidebar.vue'
import AppHeader from './AppHeader.vue'

const sidebarCollapsed = ref(false)
const mobileSidebarOpen = ref(false)
const route = useRoute()

const titles: Record<string, string> = {
  '/': 'Dashboard',
  '/encuestas': 'Encuestas',
  '/entidades': 'Entidades',
  '/usuarios': 'Usuarios',
}

const pageTitle = computed(() => titles[route.path] ?? 'Encuestas')
</script>

<template>
  <div class="flex h-screen overflow-hidden bg-background">
    <!-- Mobile overlay -->
    <Transition name="fade">
      <div
        v-if="mobileSidebarOpen"
        class="fixed inset-0 z-20 bg-black/50 lg:hidden"
        @click="mobileSidebarOpen = false"
      />
    </Transition>

    <!-- Sidebar desktop -->
    <div class="hidden lg:flex h-full">
      <AppSidebar :collapsed="sidebarCollapsed" @toggle="sidebarCollapsed = !sidebarCollapsed" />
    </div>

    <!-- Sidebar mobile -->
    <Transition name="slide-sidebar">
      <div v-if="mobileSidebarOpen" class="fixed inset-y-0 left-0 z-30 flex lg:hidden">
        <AppSidebar @toggle="mobileSidebarOpen = false" />
      </div>
    </Transition>

    <!-- Main content -->
    <div class="flex flex-1 flex-col overflow-hidden">
      <AppHeader :title="pageTitle" @toggle-sidebar="mobileSidebarOpen = !mobileSidebarOpen" />

      <main class="flex-1 overflow-y-auto p-4 lg:p-6">
        <router-view />
      </main>
    </div>
  </div>
</template>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
.slide-sidebar-enter-active, .slide-sidebar-leave-active { transition: transform 0.25s ease; }
.slide-sidebar-enter-from, .slide-sidebar-leave-to { transform: translateX(-100%); }
</style>
