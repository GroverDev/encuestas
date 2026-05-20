<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import http from '@/utils/http'
import type { Encuesta, EncuestaRequest } from '@/types/api'
import { useToast } from '@/composables/useToast'
import AppCard from '@/components/ui/AppCard.vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppInput from '@/components/ui/AppInput.vue'
import AppBadge from '@/components/ui/AppBadge.vue'
import AppModal from '@/components/ui/AppModal.vue'
import AppSpinner from '@/components/ui/AppSpinner.vue'
import useVuelidate from '@vuelidate/core'
import { required, minLength } from '@vuelidate/validators'
import { Plus, Search, Edit, Trash2, Eye } from 'lucide-vue-next'
import { fmtFecha } from '@/utils/dates'

const { toast } = useToast()
const encuestas = ref<Encuesta[]>([])
const loading = ref(true)
const search = ref('')
const modalOpen = ref(false)
const editTarget = ref<Encuesta | null>(null)
const saving = ref(false)

const emptyForm = (): EncuestaRequest => ({ titulo: '', esGlobal: false, esPlantilla: false })

const form = ref<EncuestaRequest>(emptyForm())

function toDateInput(iso?: string) {
  return iso ? iso.substring(0, 10) : ''
}
const rules = computed(() => ({
  titulo: { required, minLength: minLength(3) },
}))
const v$ = useVuelidate(rules, form)

const filtered = computed(() =>
  encuestas.value.filter((e) =>
    e.titulo.toLowerCase().includes(search.value.toLowerCase()),
  ),
)

const estadoBadge: Record<string, 'default' | 'success' | 'warning'> = {
  BORRADOR: 'warning',
  PUBLICADA: 'success',
  CERRADA: 'default',
}

async function load() {
  loading.value = true
  try {
    const { data } = await http.get<{ ok: boolean; datos: Encuesta[] }>('/encuesta')
    if (data.ok) encuestas.value = data.datos
  } finally {
    loading.value = false
  }
}

function openCreate() {
  editTarget.value = null
  form.value = emptyForm()
  v$.value.$reset()
  modalOpen.value = true
}

function openEdit(e: Encuesta) {
  editTarget.value = e
  form.value = {
    id:          e.id,
    titulo:      e.titulo,
    descripcion: e.descripcion,
    esGlobal:    e.esGlobal,
    esPlantilla: e.esPlantilla,
    fechaInicio: toDateInput(e.fechaInicio),
    fechaFin:    toDateInput(e.fechaFin),
  }
  v$.value.$reset()
  modalOpen.value = true
}

function formPayload() {
  return {
    ...form.value,
    fechaInicio: form.value.fechaInicio || null,
    fechaFin:    form.value.fechaFin    || null,
  }
}

async function save() {
  const valid = await v$.value.$validate()
  if (!valid) return
  saving.value = true
  try {
    if (editTarget.value) {
      await http.put('/encuesta', formPayload())
      toast.success('Encuesta actualizada')
    } else {
      await http.post('/encuesta', formPayload())
      toast.success('Encuesta creada')
    }
    modalOpen.value = false
    await load()
  } catch {
    toast.error('No se pudo guardar la encuesta')
  } finally {
    saving.value = false
  }
}

async function remove(e: Encuesta) {
  if (!confirm(`¿Eliminar "${e.titulo}"?`)) return
  try {
    await http.delete(`/encuesta/${e.id}`)
    toast.success('Encuesta eliminada')
    await load()
  } catch {
    toast.error('No se pudo eliminar la encuesta')
  }
}

onMounted(load)
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl font-semibold">Encuestas</h2>
        <p class="text-sm text-muted-foreground">{{ encuestas.length }} encuesta(s) registradas</p>
      </div>
      <AppButton @click="openCreate">
        <Plus class="h-4 w-4" />
        Nueva encuesta
      </AppButton>
    </div>

    <!-- Search -->
    <AppInput v-model="search" placeholder="Buscar encuestas..." class="max-w-xs">
      <template #prefix><Search class="h-4 w-4" /></template>
    </AppInput>

    <!-- Loading -->
    <div v-if="loading" class="flex justify-center py-12">
      <AppSpinner size="lg" />
    </div>

    <!-- Empty -->
    <div v-else-if="filtered.length === 0" class="text-center py-16 text-muted-foreground">
      <ClipboardList class="h-12 w-12 mx-auto mb-3 opacity-30" />
      <p class="font-medium">Sin encuestas</p>
      <p class="text-sm">Crea tu primera encuesta para comenzar</p>
    </div>

    <!-- Grid -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <AppCard v-for="e in filtered" :key="e.id" hover class="p-5 space-y-3">
        <div class="flex items-start justify-between gap-2">
          <div class="flex-1 min-w-0">
            <p class="font-semibold truncate">{{ e.titulo }}</p>
            <p v-if="e.descripcion" class="text-xs text-muted-foreground mt-0.5 line-clamp-2">{{ e.descripcion }}</p>
          </div>
          <AppBadge :variant="estadoBadge[e.estado]">{{ e.estado }}</AppBadge>
        </div>

        <div class="flex gap-1.5 flex-wrap">
          <AppBadge v-if="e.esPlantilla" variant="secondary">Plantilla</AppBadge>
          <AppBadge v-if="e.esGlobal" variant="secondary">Global</AppBadge>
        </div>

        <p class="text-xs text-muted-foreground">
          <template v-if="e.fechaInicio || e.fechaFin">
            Vigencia:
            <span class="text-foreground">{{ fmtFecha(e.fechaInicio) }}</span>
            →
            <span class="text-foreground">{{ e.fechaFin ? fmtFecha(e.fechaFin) : 'Indefinido' }}</span>
          </template>
          <template v-else>Sin fechas definidas</template>
        </p>

        <div class="flex items-center gap-1 pt-1 border-t border-border">
          <router-link :to="`/encuestas/${e.id}`">
            <AppButton variant="ghost" size="icon" title="Ver detalle">
              <Eye class="h-4 w-4" />
            </AppButton>
          </router-link>
          <AppButton variant="ghost" size="icon" title="Editar" @click="openEdit(e)">
            <Edit class="h-4 w-4" />
          </AppButton>
          <AppButton variant="ghost" size="icon" title="Eliminar" class="text-destructive hover:text-destructive" @click="remove(e)">
            <Trash2 class="h-4 w-4" />
          </AppButton>
        </div>
      </AppCard>
    </div>

    <!-- Modal -->
    <AppModal :open="modalOpen" :title="editTarget ? 'Editar encuesta' : 'Nueva encuesta'" @close="modalOpen = false">
      <form class="space-y-4" @submit.prevent="save">
        <AppInput
          v-model="form.titulo"
          label="Título"
          placeholder="Título de la encuesta"
          :error="v$.titulo.$error ? 'El título es requerido (mínimo 3 caracteres)' : undefined"
          @blur="v$.titulo.$touch"
        />
        <AppInput v-model="form.descripcion" label="Descripción" placeholder="Descripción opcional" />

        <!-- Vigencia -->
        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-1.5">
            <label class="text-sm font-medium">Fecha inicio <span class="text-muted-foreground font-normal">(opcional)</span></label>
            <input
              type="date"
              v-model="form.fechaInicio"
              class="flex w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            />
          </div>
          <div class="space-y-1.5">
            <label class="text-sm font-medium">Fecha fin <span class="text-muted-foreground font-normal">(opcional)</span></label>
            <input
              type="date"
              v-model="form.fechaFin"
              :min="form.fechaInicio || undefined"
              class="flex w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            />
          </div>
        </div>
        <p class="text-xs text-muted-foreground -mt-2">Deja vacío para vigencia indefinida.</p>

        <div class="flex gap-6">
          <label class="flex items-center gap-2 text-sm cursor-pointer">
            <input type="checkbox" v-model="form.esGlobal" class="rounded" />
            Global
          </label>
          <label class="flex items-center gap-2 text-sm cursor-pointer">
            <input type="checkbox" v-model="form.esPlantilla" class="rounded" />
            Es plantilla
          </label>
        </div>
      </form>

      <template #footer>
        <AppButton variant="outline" @click="modalOpen = false">Cancelar</AppButton>
        <AppButton :loading="saving" @click="save">
          {{ editTarget ? 'Actualizar' : 'Crear' }}
        </AppButton>
      </template>
    </AppModal>
  </div>
</template>
