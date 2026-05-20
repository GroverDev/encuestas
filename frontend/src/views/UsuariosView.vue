<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import http from '@/utils/http'
import type { CuentaUsuario, Respuesta } from '@/types/api'
import { useToast } from '@/composables/useToast'
import AppCard from '@/components/ui/AppCard.vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppBadge from '@/components/ui/AppBadge.vue'
import AppSpinner from '@/components/ui/AppSpinner.vue'
import AppModal from '@/components/ui/AppModal.vue'
import AppInput from '@/components/ui/AppInput.vue'
import { fmtFecha } from '@/utils/dates'
import { Users, Plus, Edit, PowerOff, Bot, Search } from 'lucide-vue-next'

const { toast } = useToast()
const usuarios  = ref<CuentaUsuario[]>([])
const loading   = ref(true)
const search    = ref('')
const modalOpen = ref(false)
const saving    = ref(false)
const editTarget = ref<CuentaUsuario | null>(null)
const desactivandoId = ref<string | null>(null)

const form = ref({
  correo:           '',
  contrasena:       '',
  esCuentaServicio: false,
})

const filtered = computed(() =>
  usuarios.value.filter(u => u.correo.toLowerCase().includes(search.value.toLowerCase()))
)

async function cargar() {
  loading.value = true
  try {
    const { data } = await http.get<Respuesta<CuentaUsuario[]>>('/cuentausuario')
    if (data.ok) usuarios.value = data.datos
  } catch {
    toast.error('Error al cargar usuarios')
  } finally {
    loading.value = false
  }
}

onMounted(cargar)

function abrirCrear() {
  editTarget.value = null
  form.value = { correo: '', contrasena: '', esCuentaServicio: false }
  modalOpen.value = true
}

function abrirEditar(u: CuentaUsuario) {
  editTarget.value = u
  form.value = { correo: u.correo, contrasena: '', esCuentaServicio: u.esCuentaServicio }
  modalOpen.value = true
}

async function guardar() {
  if (!form.value.correo.trim()) return
  if (!editTarget.value && !form.value.contrasena.trim()) {
    toast.error('La contraseña es requerida para nuevos usuarios')
    return
  }
  saving.value = true
  try {
    const payload: Record<string, unknown> = {
      correo:           form.value.correo,
      esCuentaServicio: form.value.esCuentaServicio,
      ...(form.value.contrasena ? { contrasena: form.value.contrasena } : {}),
      ...(editTarget.value ? { id: editTarget.value.id } : {}),
    }
    if (editTarget.value) {
      await http.put('/cuentausuario', payload)
      toast.success('Usuario actualizado')
    } else {
      await http.post('/cuentausuario', payload)
      toast.success('Usuario creado')
    }
    modalOpen.value = false
    await cargar()
  } catch {
    toast.error('No se pudo guardar')
  } finally {
    saving.value = false
  }
}

async function desactivar(u: CuentaUsuario) {
  if (!confirm(`¿Desactivar la cuenta de "${u.correo}"?`)) return
  desactivandoId.value = u.id
  try {
    await http.delete(`/cuentausuario/${u.id}`)
    toast.success('Cuenta desactivada')
    await cargar()
  } catch {
    toast.error('No se pudo desactivar')
  } finally {
    desactivandoId.value = null
  }
}
</script>

<template>
  <div class="space-y-6">

    <!-- Cabecera -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl font-semibold">Usuarios</h2>
        <p class="text-sm text-muted-foreground">{{ usuarios.length }} cuenta(s) registradas</p>
      </div>
      <AppButton @click="abrirCrear">
        <Plus class="h-4 w-4" />
        Nueva cuenta
      </AppButton>
    </div>

    <!-- Búsqueda -->
    <div class="relative max-w-xs">
      <Search class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
      <input
        v-model="search"
        placeholder="Buscar por correo..."
        class="w-full pl-9 pr-3 py-2 text-sm rounded-md border border-input bg-transparent shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
      />
    </div>

    <div v-if="loading" class="flex justify-center py-12"><AppSpinner size="lg" /></div>

    <AppCard v-else-if="filtered.length === 0" class="p-12 text-center text-muted-foreground">
      <Users class="h-12 w-12 mx-auto mb-3 opacity-30" />
      <p class="font-medium">Sin usuarios</p>
    </AppCard>

    <!-- Tabla -->
    <div v-else class="overflow-x-auto rounded-xl border border-border">
      <table class="w-full text-sm">
        <thead class="bg-muted/50 text-muted-foreground">
          <tr>
            <th class="px-4 py-3 text-left font-medium">Correo</th>
            <th class="px-4 py-3 text-left font-medium">Tipo</th>
            <th class="px-4 py-3 text-left font-medium">Estado</th>
            <th class="px-4 py-3 text-left font-medium">Creado</th>
            <th class="px-4 py-3 text-left font-medium"></th>
          </tr>
        </thead>
        <tbody class="divide-y divide-border">
          <tr v-for="u in filtered" :key="u.id"
            :class="['hover:bg-muted/30 transition-colors', !u.esActivo && 'opacity-50']"
          >
            <td class="px-4 py-3 font-medium">{{ u.correo }}</td>
            <td class="px-4 py-3">
              <div v-if="u.esCuentaServicio" class="flex items-center gap-1.5 text-violet-500">
                <Bot class="h-3.5 w-3.5" />
                <span class="text-xs font-medium">Servicio M2M</span>
              </div>
              <span v-else class="text-xs text-muted-foreground">Usuario</span>
            </td>
            <td class="px-4 py-3">
              <AppBadge :variant="u.esActivo ? 'success' : 'default'">
                {{ u.esActivo ? 'Activo' : 'Inactivo' }}
              </AppBadge>
            </td>
            <td class="px-4 py-3 text-muted-foreground">{{ fmtFecha(u.creadoEn) }}</td>
            <td class="px-4 py-3">
              <div class="flex items-center gap-1 justify-end">
                <AppButton variant="ghost" size="icon" class="h-7 w-7" title="Editar" @click="abrirEditar(u)">
                  <Edit class="h-3.5 w-3.5" />
                </AppButton>
                <AppButton
                  v-if="u.esActivo"
                  variant="ghost" size="icon"
                  class="h-7 w-7 text-destructive hover:text-destructive"
                  title="Desactivar"
                  :loading="desactivandoId === u.id"
                  @click="desactivar(u)"
                >
                  <PowerOff class="h-3.5 w-3.5" />
                </AppButton>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal crear / editar -->
    <AppModal
      :open="modalOpen"
      :title="editTarget ? 'Editar cuenta' : 'Nueva cuenta'"
      @close="modalOpen = false"
    >
      <div class="space-y-4">
        <AppInput
          v-model="form.correo"
          label="Correo electrónico"
          type="email"
          placeholder="usuario@empresa.com"
        />
        <AppInput
          v-model="form.contrasena"
          label="Contraseña"
          type="password"
          :placeholder="editTarget ? 'Dejar vacío para no cambiar' : 'Contraseña segura'"
        />

        <!-- Tipo de cuenta -->
        <div class="rounded-lg border border-border p-4 space-y-3">
          <label class="flex items-start gap-3 cursor-pointer select-none">
            <input
              type="checkbox"
              v-model="form.esCuentaServicio"
              class="mt-0.5 rounded accent-primary"
            />
            <div>
              <div class="flex items-center gap-1.5 text-sm font-medium">
                <Bot class="h-4 w-4 text-violet-500" />
                Cuenta de servicio (M2M)
              </div>
              <p class="text-xs text-muted-foreground mt-0.5">
                Permite autenticarse desde sistemas externos sin intervención humana.
                Usa <code class="bg-muted px-1 rounded">POST /api/auth/token-servicio</code>
                para obtener un token de 1 año.
              </p>
            </div>
          </label>
        </div>
      </div>

      <template #footer>
        <AppButton variant="outline" @click="modalOpen = false">Cancelar</AppButton>
        <AppButton :loading="saving" @click="guardar">
          {{ editTarget ? 'Actualizar' : 'Crear cuenta' }}
        </AppButton>
      </template>
    </AppModal>

  </div>
</template>
