<script setup lang="ts">
import { useToast } from '@/composables/useToast'
import { CheckCircle, XCircle, AlertTriangle, Info, X } from 'lucide-vue-next'

const { toasts, remove } = useToast()

const icons = {
  success: CheckCircle,
  error: XCircle,
  warning: AlertTriangle,
  info: Info,
}

const styles = {
  success: 'border-emerald-500/30 bg-emerald-500/10 text-emerald-700 dark:text-emerald-400',
  error: 'border-destructive/30 bg-destructive/10 text-destructive',
  warning: 'border-amber-500/30 bg-amber-500/10 text-amber-700 dark:text-amber-400',
  info: 'border-primary/30 bg-primary/10 text-primary',
}
</script>

<template>
  <Teleport to="body">
    <div class="fixed bottom-4 right-4 z-[100] flex flex-col gap-2 w-80">
      <TransitionGroup name="toast">
        <div
          v-for="t in toasts"
          :key="t.id"
          :class="['flex items-start gap-3 rounded-xl border p-4 shadow-lg backdrop-blur-sm', styles[t.type]]"
        >
          <component :is="icons[t.type]" class="h-5 w-5 shrink-0 mt-0.5" />
          <div class="flex-1 min-w-0">
            <p class="font-medium text-sm">{{ t.title }}</p>
            <p v-if="t.message" class="text-xs opacity-80 mt-0.5">{{ t.message }}</p>
          </div>
          <button class="shrink-0 opacity-60 hover:opacity-100 transition-opacity" @click="remove(t.id)">
            <X class="h-4 w-4" />
          </button>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<style scoped>
.toast-enter-active { transition: all 0.3s ease; }
.toast-leave-active { transition: all 0.2s ease; }
.toast-enter-from { opacity: 0; transform: translateX(1rem); }
.toast-leave-to { opacity: 0; transform: translateX(1rem); }
</style>
