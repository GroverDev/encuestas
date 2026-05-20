<script setup lang="ts">
import { computed } from 'vue'
import { useTheme } from '@/composables/useTheme'
import { useAuthStore } from '@/stores/auth'
import { Menu, Sun, Moon } from 'lucide-vue-next'

defineProps<{ title?: string }>()
defineEmits<{ toggleSidebar: [] }>()

const { isDark, toggle } = useTheme()
const auth = useAuthStore()

const initials = computed(() => {
  const e = auth.usuario?.correo ?? ''
  return e.slice(0, 2).toUpperCase()
})
</script>

<template>
  <header class="flex h-16 shrink-0 items-center justify-between border-b border-border bg-card px-4 lg:px-6">
    <div class="flex items-center gap-4">
      <button
        class="rounded-md p-2 text-muted-foreground hover:bg-accent hover:text-foreground transition-colors"
        @click="$emit('toggleSidebar')"
      >
        <Menu class="h-5 w-5" />
      </button>
      <h1 v-if="title" class="text-base font-semibold hidden sm:block">{{ title }}</h1>
    </div>

    <div class="flex items-center gap-2">
      <button
        class="rounded-md p-2 text-muted-foreground hover:bg-accent hover:text-foreground transition-colors"
        @click="toggle"
        :title="isDark ? 'Modo claro' : 'Modo oscuro'"
      >
        <Sun v-if="isDark" class="h-5 w-5" />
        <Moon v-else class="h-5 w-5" />
      </button>

      <div class="h-8 w-8 rounded-full bg-primary flex items-center justify-center text-primary-foreground text-xs font-semibold">
        {{ initials }}
      </div>
    </div>
  </header>
</template>
