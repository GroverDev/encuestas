<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import http from '@/utils/http'
import type { Entidad, TipoEntidad, Respuesta } from '@/types/api'
import { useToast } from '@/composables/useToast'
import AppCard from '@/components/ui/AppCard.vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppBadge from '@/components/ui/AppBadge.vue'
import AppSpinner from '@/components/ui/AppSpinner.vue'
import AppModal from '@/components/ui/AppModal.vue'
import AppInput from '@/components/ui/AppInput.vue'
import {
  Building2, MapPin, Layers, User, Users, Truck,
  Package, Wrench, Monitor, FolderKanban, Calendar,
  Plus, Edit, PowerOff, ChevronRight, ChevronDown, Search,
} from 'lucide-vue-next'

// ── tipos locales ─────────────────────────────────────────
interface EntidadNodo extends Entidad {
  children: EntidadNodo[]
}

interface EntidadFlat extends Entidad {
  nivel: number
  tipoNombre: string
  tipoIcono: unknown
  expanded: boolean
  tieneHijos: boolean
}

// ── iconos por código de tipo ─────────────────────────────
const ICONOS: Record<string, unknown> = {
  EMPRESA: Building2, REGIONAL: MapPin, UNIDAD: Layers,
  EMPLEADO: User, CLIENTE: Users, PROVEEDOR: Truck,
  PRODUCTO: Package, SERVICIO: Wrench, APLICACION: Monitor,
  PROYECTO: FolderKanban, EVENTO: Calendar,
}

const COLORES: Record<string, string> = {
  EMPRESA: 'text-blue-500 bg-blue-500/10',
  REGIONAL: 'text-violet-500 bg-violet-500/10',
  UNIDAD: 'text-emerald-500 bg-emerald-500/10',
  EMPLEADO: 'text-amber-500 bg-amber-500/10',
  CLIENTE: 'text-pink-500 bg-pink-500/10',
  PROVEEDOR: 'text-orange-500 bg-orange-500/10',
}

// ── estado ────────────────────────────────────────────────
const { toast } = useToast()
const entidades  = ref<Entidad[]>([])
const tipos      = ref<TipoEntidad[]>([])
const loading    = ref(true)
const search     = ref('')
const expanded   = ref<Set<string>>(new Set())

const modalOpen  = ref(false)
const editTarget = ref<Entidad | null>(null)
const parentCtx  = ref<Entidad | null>(null)   // padre preseleccionado al crear hijo
const saving     = ref(false)
const desactivandoId = ref<string | null>(null)

const form = ref({
  tipoEntidadId: '',
  nombreVisible: '',
  entidadPadreId: '' as string | null,
  idExterno: '',
})

// ── árbol ─────────────────────────────────────────────────
function buildTree(lista: Entidad[]): EntidadNodo[] {
  const map = new Map<string, EntidadNodo>()
  lista.forEach(e => map.set(e.id, { ...e, children: [] }))
  const roots: EntidadNodo[] = []
  lista.forEach(e => {
    const node = map.get(e.id)!
    if (e.entidadPadreId && map.has(e.entidadPadreId))
      map.get(e.entidadPadreId)!.children.push(node)
    else
      roots.push(node)
  })
  return roots
}

function flattenTree(nodes: EntidadNodo[], nivel = 0): EntidadFlat[] {
  const result: EntidadFlat[] = []
  for (const n of nodes) {
    const tipo = tipos.value.find(t => t.id === n.tipoEntidadId)
    const isExpanded = expanded.value.has(n.id)
    result.push({
      ...n,
      nivel,
      tipoNombre: tipo?.nombre ?? '—',
      tipoIcono:  ICONOS[tipo?.codigo ?? ''] ?? Building2,
      expanded:   isExpanded,
      tieneHijos: n.children.length > 0,
    })
    if (isExpanded) result.push(...flattenTree(n.children, nivel + 1))
  }
  return result
}

const tree = computed(() => buildTree(entidades.value))

const flatList = computed(() => {
  const all = flattenTree(tree.value)
  if (!search.value) return all
  const q = search.value.toLowerCase()
  return all.filter(e => e.nombreVisible.toLowerCase().includes(q))
})

function toggleExpand(id: string) {
  if (expanded.value.has(id)) expanded.value.delete(id)
  else expanded.value.add(id)
  expanded.value = new Set(expanded.value)
}

function expandAll() {
  entidades.value.forEach(e => expanded.value.add(e.id))
  expanded.value = new Set(expanded.value)
}

function collapseAll() {
  expanded.value = new Set()
}

// ── carga ─────────────────────────────────────────────────
async function cargar() {
  loading.value = true
  try {
    const [entRes, tipRes] = await Promise.all([
      http.get<Respuesta<Entidad[]>>('/entidad'),
      http.get<Respuesta<TipoEntidad[]>>('/tipoentidad'),
    ])
    if (entRes.data.ok) entidades.value = entRes.data.datos
    if (tipRes.data.ok) tipos.value     = tipRes.data.datos
    // Auto-expandir el primer nivel
    entidades.value.filter(e => !e.entidadPadreId).forEach(e => expanded.value.add(e.id))
    expanded.value = new Set(expanded.value)
  } catch {
    toast.error('Error al cargar entidades')
  } finally {
    loading.value = false
  }
}

onMounted(cargar)

// ── modal ─────────────────────────────────────────────────
function abrirCrear(padre?: Entidad) {
  editTarget.value = null
  parentCtx.value  = padre ?? null
  form.value = {
    tipoEntidadId:  tipos.value[0]?.id ?? '',
    nombreVisible:  '',
    entidadPadreId: padre?.id ?? null,
    idExterno:      '',
  }
  modalOpen.value = true
}

function abrirEditar(e: Entidad) {
  editTarget.value = e
  parentCtx.value  = null
  form.value = {
    tipoEntidadId:  e.tipoEntidadId,
    nombreVisible:  e.nombreVisible,
    entidadPadreId: e.entidadPadreId ?? null,
    idExterno:      e.idExterno ?? '',
  }
  modalOpen.value = true
}

async function guardar() {
  if (!form.value.nombreVisible.trim() || !form.value.tipoEntidadId) return
  saving.value = true
  try {
    const payload = {
      ...form.value,
      entidadPadreId: form.value.entidadPadreId || null,
      idExterno:      form.value.idExterno || null,
      ...(editTarget.value ? { id: editTarget.value.id } : {}),
    }
    if (editTarget.value) {
      await http.put('/entidad', payload)
      toast.success('Entidad actualizada')
    } else {
      await http.post('/entidad', payload)
      toast.success('Entidad creada')
    }
    modalOpen.value = false
    await cargar()
  } catch {
    toast.error('No se pudo guardar')
  } finally {
    saving.value = false
  }
}

async function desactivar(e: Entidad) {
  if (!confirm(`¿Desactivar "${e.nombreVisible}"? Sus hijos también quedarán inaccesibles.`)) return
  desactivandoId.value = e.id
  try {
    await http.delete(`/entidad/${e.id}`)
    toast.success('Entidad desactivada')
    await cargar()
  } catch {
    toast.error('No se pudo desactivar')
  } finally {
    desactivandoId.value = null
  }
}

// ── helpers ───────────────────────────────────────────────
const tiposOrdenados = computed(() => [...tipos.value].sort((a, b) => a.nombre.localeCompare(b.nombre)))

const entidadesActivas = computed(() => entidades.value.filter(e => e.esActivo))

function colorClase(tipoId: string) {
  const codigo = tipos.value.find(t => t.id === tipoId)?.codigo ?? ''
  return COLORES[codigo] ?? 'text-muted-foreground bg-muted'
}
</script>

<template>
  <div class="space-y-6">

    <!-- Cabecera -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl font-semibold">Entidades</h2>
        <p class="text-sm text-muted-foreground">
          {{ entidadesActivas.length }} entidad(es) activa(s)
        </p>
      </div>
      <AppButton @click="abrirCrear()">
        <Plus class="h-4 w-4" />
        Nueva entidad raíz
      </AppButton>
    </div>

    <!-- Barra de herramientas -->
    <div class="flex items-center gap-3 flex-wrap">
      <div class="relative flex-1 min-w-48 max-w-xs">
        <Search class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <input
          v-model="search"
          placeholder="Buscar entidades..."
          class="w-full pl-9 pr-3 py-2 text-sm rounded-md border border-input bg-transparent shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
        />
      </div>
      <AppButton variant="ghost" size="sm" @click="expandAll">Expandir todo</AppButton>
      <AppButton variant="ghost" size="sm" @click="collapseAll">Colapsar todo</AppButton>
    </div>

    <div v-if="loading" class="flex justify-center py-16"><AppSpinner size="lg" /></div>

    <!-- Árbol vacío -->
    <AppCard v-else-if="flatList.length === 0" class="p-12 text-center text-muted-foreground">
      <Building2 class="h-12 w-12 mx-auto mb-3 opacity-30" />
      <p class="font-medium">Sin entidades</p>
      <p class="text-sm mt-1">Crea la empresa raíz para comenzar la jerarquía.</p>
    </AppCard>

    <!-- Árbol de entidades -->
    <div v-else class="space-y-1">
      <div
        v-for="e in flatList"
        :key="e.id"
        :style="{ paddingLeft: `${e.nivel * 1.75 + 0.5}rem` }"
        :class="['flex items-center gap-2 rounded-lg border border-border bg-card px-3 py-2.5 transition-colors',
                 !e.esActivo && 'opacity-50']"
      >
        <!-- Expand toggle -->
        <button
          v-if="e.tieneHijos"
          class="shrink-0 text-muted-foreground hover:text-foreground transition-colors"
          @click="toggleExpand(e.id)"
        >
          <component :is="e.expanded ? ChevronDown : ChevronRight" class="h-4 w-4" />
        </button>
        <span v-else class="w-4 shrink-0" />

        <!-- Icono tipo -->
        <div :class="['shrink-0 rounded-md p-1.5', colorClase(e.tipoEntidadId)]">
          <component :is="e.tipoIcono" class="h-3.5 w-3.5" />
        </div>

        <!-- Nombre + tipo -->
        <div class="flex-1 min-w-0 flex items-center gap-2 flex-wrap">
          <span class="font-medium text-sm">{{ e.nombreVisible }}</span>
          <span class="text-xs text-muted-foreground">{{ e.tipoNombre }}</span>
          <span v-if="e.idExterno" class="text-xs text-muted-foreground font-mono">· {{ e.idExterno }}</span>
          <AppBadge v-if="!e.esActivo" variant="default" class="text-[10px] py-0">Inactivo</AppBadge>
        </div>

        <!-- Acciones -->
        <div class="flex items-center gap-1 shrink-0">
          <AppButton
            variant="ghost" size="icon"
            title="Agregar hijo"
            class="h-7 w-7"
            @click="abrirCrear(e)"
          >
            <Plus class="h-3.5 w-3.5" />
          </AppButton>
          <AppButton
            variant="ghost" size="icon"
            title="Editar"
            class="h-7 w-7"
            @click="abrirEditar(e)"
          >
            <Edit class="h-3.5 w-3.5" />
          </AppButton>
          <AppButton
            v-if="e.esActivo"
            variant="ghost" size="icon"
            title="Desactivar"
            class="h-7 w-7 text-destructive hover:text-destructive"
            :loading="desactivandoId === e.id"
            @click="desactivar(e)"
          >
            <PowerOff class="h-3.5 w-3.5" />
          </AppButton>
        </div>
      </div>
    </div>

    <!-- ── Modal crear / editar ───────────────────────────── -->
    <AppModal
      :open="modalOpen"
      :title="editTarget ? 'Editar entidad' : parentCtx ? `Nueva entidad en ${parentCtx.nombreVisible}` : 'Nueva entidad raíz'"
      @close="modalOpen = false"
    >
      <div class="space-y-4">

        <!-- Tipo -->
        <div class="space-y-1.5">
          <label class="text-sm font-medium">Tipo de entidad</label>
          <select
            v-model="form.tipoEntidadId"
            class="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
          >
            <option v-for="t in tiposOrdenados" :key="t.id" :value="t.id">{{ t.nombre }}</option>
          </select>
        </div>

        <!-- Nombre -->
        <AppInput
          v-model="form.nombreVisible"
          label="Nombre visible"
          placeholder="Ej: Regional Norte, Departamento de RRHH..."
        />

        <!-- Entidad padre -->
        <div class="space-y-1.5">
          <label class="text-sm font-medium">
            Entidad padre <span class="text-muted-foreground font-normal">(opcional)</span>
          </label>
          <select
            v-model="form.entidadPadreId"
            class="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
          >
            <option :value="null">— Sin padre (nodo raíz) —</option>
            <option
              v-for="e in entidadesActivas.filter(e => e.id !== editTarget?.id)"
              :key="e.id"
              :value="e.id"
            >{{ e.nombreVisible }}</option>
          </select>
        </div>

        <!-- ID externo -->
        <AppInput
          v-model="form.idExterno"
          label="ID externo"
          placeholder="Código en sistema ERP/RRHH (opcional)"
        />
      </div>

      <template #footer>
        <AppButton variant="outline" @click="modalOpen = false">Cancelar</AppButton>
        <AppButton :loading="saving" @click="guardar">
          {{ editTarget ? 'Actualizar' : 'Crear' }}
        </AppButton>
      </template>
    </AppModal>

  </div>
</template>
