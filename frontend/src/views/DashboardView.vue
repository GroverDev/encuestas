<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import http from '@/utils/http'
import type { DashboardStats } from '@/types/api'
import AppCard from '@/components/ui/AppCard.vue'
import AppSpinner from '@/components/ui/AppSpinner.vue'
import { ClipboardList, CheckCircle2, MessageSquare, Mail } from 'lucide-vue-next'

const stats = ref<DashboardStats | null>(null)
const loading = ref(true)

onMounted(async () => {
  try {
    const { data } = await http.get<{ ok: boolean; datos: DashboardStats }>('/dashboard/stats')
    if (data.ok) stats.value = data.datos
  } finally {
    loading.value = false
  }
})

const statCards = computed(() => [
  { label: 'Encuestas totales', value: stats.value?.totalEncuestas ?? 0, icon: ClipboardList, color: 'text-primary' },
  { label: 'Encuestas activas', value: stats.value?.encuestasActivas ?? 0, icon: CheckCircle2, color: 'text-emerald-500' },
  { label: 'Respuestas recibidas', value: stats.value?.totalRespuestas ?? 0, icon: MessageSquare, color: 'text-violet-500' },
  { label: 'Invitaciones enviadas', value: stats.value?.totalInvitaciones ?? 0, icon: Mail, color: 'text-amber-500' },
])

</script>

<template>
  <div class="space-y-6">
    <div>
      <h2 class="text-xl font-semibold">Dashboard</h2>
      <p class="text-sm text-muted-foreground">Resumen general de la plataforma</p>
    </div>

    <div v-if="loading" class="flex justify-center py-12">
      <AppSpinner size="lg" />
    </div>

    <div v-else class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
      <AppCard v-for="s in statCards" :key="s.label" hover class="p-5">
        <div class="flex items-center justify-between">
          <div class="space-y-1">
            <p class="text-sm text-muted-foreground">{{ s.label }}</p>
            <p class="text-3xl font-bold">{{ s.value.toLocaleString() }}</p>
          </div>
          <div :class="['rounded-xl p-3 bg-current/10', s.color]">
            <component :is="s.icon" :class="['h-6 w-6', s.color]" />
          </div>
        </div>
      </AppCard>
    </div>
  </div>
</template>
