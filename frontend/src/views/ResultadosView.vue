<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import http from '@/utils/http'
import type {
  Respuesta, Pregunta, EstadisticasEncuesta, EstadisticasPregunta,
  DimensionPregunta, ResumenEntidad,
} from '@/types/api'
import AppCard from '@/components/ui/AppCard.vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppBadge from '@/components/ui/AppBadge.vue'
import AppSpinner from '@/components/ui/AppSpinner.vue'
import { ArrowLeft, ChevronDown, ChevronUp, BarChart2, List, Layers, Building2, X } from 'lucide-vue-next'
import { fmtFechaHora } from '@/utils/dates'

interface RespuestaRow {
  id: string; encuestaId: string; versionEncuesta: number
  canal?: string; completadoEn?: string; iniciadoEn?: string
}
interface DetalleRow {
  id: string; respuestaId: string; preguntaId: string
  valorTexto?: string; valorNumero?: number; valorBooleano?: boolean
  valorFecha?: string; valorJson?: string
}

const route  = useRoute()
const router = useRouter()
const encuestaId = route.params.id as string

// ── estado ────────────────────────────────────────────────
const tab            = ref<'respuestas' | 'estadisticas' | 'entidades'>('respuestas')
const encuestaTitulo = ref('...')
const preguntas      = ref<Pregunta[]>([])
const dimensiones    = ref<DimensionPregunta[]>([])
const respuestas     = ref<RespuestaRow[]>([])
const estadisticas   = ref<EstadisticasEncuesta | null>(null)
const resumenEntidades = ref<ResumenEntidad[]>([])
const loading        = ref(true)
const loadingStats   = ref(false)
const loadingEntidades = ref(false)
const expandedId     = ref<string | null>(null)
const detallesCache  = ref<Record<string, DetalleRow[]>>({})
const loadingDetalle = ref(false)

// filtro de entidad en pestaña estadísticas
const entidadFiltro  = ref<ResumenEntidad | null>(null)

const preguntaMap = computed(() =>
  Object.fromEntries(preguntas.value.map(p => [p.id, p]))
)

// ── carga inicial ─────────────────────────────────────────
onMounted(async () => {
  try {
    const [encRes, pregRes, respRes, dimRes] = await Promise.all([
      http.get<Respuesta<{ titulo: string }>>(`/encuesta/${encuestaId}`),
      http.get<Respuesta<Pregunta[]>>(`/pregunta/${encuestaId}`),
      http.get<Respuesta<RespuestaRow[]>>(`/respuesta/${encuestaId}`),
      http.get<Respuesta<DimensionPregunta[]>>(`/dimensionpregunta/${encuestaId}`),
    ])
    if (encRes.data.ok)  encuestaTitulo.value = encRes.data.datos.titulo
    if (pregRes.data.ok) preguntas.value      = pregRes.data.datos
    if (respRes.data.ok) respuestas.value     = respRes.data.datos
    if (dimRes.data.ok)  dimensiones.value    = dimRes.data.datos.sort((a, b) => a.orden - b.orden)
  } finally {
    loading.value = false
  }
})

// ── estadísticas (con filtro opcional de entidad) ─────────
async function cargarEstadisticas(entidad?: ResumenEntidad | null) {
  loadingStats.value = true
  estadisticas.value = null
  try {
    const url = entidad
      ? `/estadisticas/${encuestaId}?entidadId=${entidad.entidadId}`
      : `/estadisticas/${encuestaId}`
    const { data } = await http.get<Respuesta<EstadisticasEncuesta>>(url)
    if (data.ok) estadisticas.value = data.datos
  } finally {
    loadingStats.value = false
  }
}

async function cargarResumenEntidades() {
  if (resumenEntidades.value.length > 0) return
  loadingEntidades.value = true
  try {
    const { data } = await http.get<Respuesta<ResumenEntidad[]>>(
      `/estadisticas/${encuestaId}/entidades`
    )
    if (data.ok) resumenEntidades.value = data.datos
  } finally {
    loadingEntidades.value = false
  }
}

function switchTab(t: 'respuestas' | 'estadisticas' | 'entidades') {
  tab.value = t
  if (t === 'estadisticas') {
    if (!estadisticas.value) cargarEstadisticas()
    cargarResumenEntidades()
  }
  if (t === 'entidades') cargarResumenEntidades()
}

function aplicarFiltroEntidad(ent: ResumenEntidad | null) {
  entidadFiltro.value = ent
  cargarEstadisticas(ent)
}

// ── respuestas individuales ───────────────────────────────
async function toggleDetalle(id: string) {
  if (expandedId.value === id) { expandedId.value = null; return }
  expandedId.value = id
  if (detallesCache.value[id]) return
  loadingDetalle.value = true
  try {
    const { data } = await http.get<Respuesta<DetalleRow[]>>(`/detallerespuesta/${id}`)
    if (data.ok) detallesCache.value[id] = data.datos
  } finally {
    loadingDetalle.value = false
  }
}

function fmtValor(d: DetalleRow): string {
  if (d.valorBooleano !== null && d.valorBooleano !== undefined) return d.valorBooleano ? 'Sí' : 'No'
  if (d.valorNumero   !== null && d.valorNumero   !== undefined) return String(d.valorNumero)
  if (d.valorFecha    !== null && d.valorFecha    !== undefined)
    return new Date(d.valorFecha).toLocaleDateString('es-PE', { day: '2-digit', month: 'short', year: 'numeric' })
  if (d.valorJson) {
    try {
      const p = JSON.parse(d.valorJson)
      if (Array.isArray(p)) return p.join(' → ')
      if (typeof p === 'object') return Object.entries(p).map(([k, v]) => `${k}: ${v}`).join(' | ')
    } catch { return d.valorJson }
  }
  return d.valorTexto ?? '—'
}

const canalBadge: Record<string, 'default' | 'secondary' | 'outline'> = {
  ENLACE_PUBLICO: 'secondary', EMAIL: 'outline', QR: 'outline',
}

// ── helpers estadísticas ──────────────────────────────────
function npsColor(score: number) {
  if (score >= 50) return 'text-emerald-500'
  if (score >= 0)  return 'text-amber-500'
  return 'text-destructive'
}

function barColor(tipo: string, valor: string) {
  if (tipo === 'NPS') {
    if (valor === 'promotores')  return 'bg-emerald-500'
    if (valor === 'neutrales')   return 'bg-amber-400'
    return 'bg-red-400'
  }
  if (tipo === 'BOOLEANO') return valor === 'true' ? 'bg-emerald-500' : 'bg-red-400'
  return 'bg-primary'
}

function maxConteo(p: EstadisticasPregunta) {
  return Math.max(...p.conteos.map(c => c.cantidad), 1)
}

// ── agrupación por dimensión ──────────────────────────────
interface GrupoDimension {
  dimension: DimensionPregunta | null
  preguntas: EstadisticasPregunta[]
  promedioGlobal: number | null
}

const gruposDimension = computed((): GrupoDimension[] => {
  if (!estadisticas.value) return []
  const statMap = Object.fromEntries(estadisticas.value.preguntas.map(p => [p.preguntaId, p]))
  const grupos: GrupoDimension[] = dimensiones.value.map(dim => {
    const psDim = preguntas.value
      .filter(p => p.dimensionId === dim.id)
      .map(p => statMap[p.id])
      .filter(Boolean) as EstadisticasPregunta[]
    const promedios = psDim.map(p => p.promedio).filter((v): v is number => v !== null && v !== undefined)
    const promedioGlobal = promedios.length > 0
      ? Math.round((promedios.reduce((a, b) => a + b, 0) / promedios.length) * 10) / 10
      : null
    return { dimension: dim, preguntas: psDim, promedioGlobal }
  })
  const conDim = new Set(preguntas.value.filter(p => p.dimensionId).map(p => p.id))
  const sinDim = estadisticas.value.preguntas.filter(p => !conDim.has(p.preguntaId))
  if (sinDim.length > 0) grupos.push({ dimension: null, preguntas: sinDim, promedioGlobal: null })
  return grupos.filter(g => g.preguntas.length > 0)
})

function scoreBar(dim: GrupoDimension): number {
  if (dim.promedioGlobal === null) return 0
  const tipos = dim.preguntas.map(p => p.tipo)
  if (tipos.some(t => t === 'NPS'))          return Math.max(0, (dim.promedioGlobal / 10) * 100)
  if (tipos.some(t => t === 'CALIFICACION')) return (dim.promedioGlobal / 5) * 100
  if (tipos.some(t => t === 'ESCALA'))       return (dim.promedioGlobal / 10) * 100
  return Math.min(100, dim.promedioGlobal * 10)
}

// ── entidades: árbol aplanado con nivel para indentación ─
interface FilaEntidad { entidad: ResumenEntidad; nivel: number }

const filasPorEntidad = computed((): FilaEntidad[] => {
  const all = resumenEntidades.value
  const ids = new Set(all.map(e => e.entidadId))
  const rows: FilaEntidad[] = []

  function addRows(padreId: string | null | undefined, nivel: number) {
    const hijos = padreId == null
      ? all.filter(e => !e.entidadPadreId || !ids.has(e.entidadPadreId))
      : all.filter(e => e.entidadPadreId === padreId)
    hijos.sort((a, b) => a.nombreEntidad.localeCompare(b.nombreEntidad))
    for (const h of hijos) {
      rows.push({ entidad: h, nivel })
      addRows(h.entidadId, nivel + 1)
    }
  }

  addRows(null, 0)
  return rows
})

// Preguntas numéricas para columnas de comparación
const preguntasNumericas = computed(() =>
  preguntas.value.filter(p => ['CALIFICACION','NPS','ESCALA','NUMERO'].includes(p.tipo))
)

function metricaValor(ent: ResumenEntidad, preguntaId: string): string {
  const m = ent.metricas.find(m => m.preguntaId === preguntaId)
  if (!m) return '—'
  if (m.tipo === 'NPS' && m.puntajeNps !== undefined) return String(m.puntajeNps)
  if (m.promedio !== undefined && m.promedio !== null) return String(m.promedio)
  return '—'
}

function metricaColor(ent: ResumenEntidad, preguntaId: string): string {
  const m = ent.metricas.find(m => m.preguntaId === preguntaId)
  if (!m) return 'text-muted-foreground'
  const v = m.promedio ?? m.puntajeNps
  if (v === undefined || v === null) return 'text-muted-foreground'
  if (m.tipo === 'CALIFICACION') return v >= 4 ? 'text-emerald-600' : v >= 3 ? 'text-amber-600' : 'text-red-500'
  if (m.tipo === 'NPS') return v >= 50 ? 'text-emerald-600' : v >= 0 ? 'text-amber-600' : 'text-red-500'
  if (m.tipo === 'ESCALA') return v >= 7 ? 'text-emerald-600' : v >= 5 ? 'text-amber-600' : 'text-red-500'
  return 'text-foreground'
}
</script>

<template>
  <div class="space-y-6">

    <!-- Cabecera -->
    <div class="flex items-center gap-3">
      <AppButton variant="ghost" size="icon" @click="router.back()">
        <ArrowLeft class="h-5 w-5" />
      </AppButton>
      <div class="flex-1">
        <h2 class="text-xl font-semibold">{{ encuestaTitulo }}</h2>
        <p class="text-sm text-muted-foreground">{{ respuestas.length }} respuesta(s) recibidas</p>
      </div>
    </div>

    <!-- Tabs -->
    <div class="flex gap-1 border-b border-border">
      <button
        v-for="t in [
          { id: 'respuestas',   label: 'Respuestas',     icon: List      },
          { id: 'estadisticas', label: 'Estadísticas',   icon: BarChart2 },
          { id: 'entidades',    label: 'Por entidad',    icon: Building2 },
        ]"
        :key="t.id"
        :class="[
          'flex items-center gap-2 px-4 py-2.5 text-sm font-medium border-b-2 -mb-px transition-colors',
          tab === t.id
            ? 'border-primary text-primary'
            : 'border-transparent text-muted-foreground hover:text-foreground',
        ]"
        @click="switchTab(t.id as any)"
      >
        <component :is="t.icon" class="h-4 w-4" />
        {{ t.label }}
      </button>
    </div>

    <div v-if="loading" class="flex justify-center py-16"><AppSpinner size="lg" /></div>

    <!-- ── TAB: Respuestas individuales ─────────────────── -->
    <template v-else-if="tab === 'respuestas'">
      <AppCard v-if="respuestas.length === 0" class="p-12 text-center text-muted-foreground">
        <p class="font-medium">Sin respuestas aún</p>
        <p class="text-sm mt-1">Comparte el enlace de invitación para recibir respuestas.</p>
      </AppCard>

      <div v-else class="space-y-3">
        <AppCard v-for="(r, idx) in respuestas" :key="r.id" class="overflow-hidden">
          <button
            class="w-full flex items-center gap-4 px-5 py-4 text-left hover:bg-muted/40 transition-colors"
            @click="toggleDetalle(r.id)"
          >
            <span class="text-sm font-bold text-muted-foreground w-6 shrink-0 text-center">{{ idx + 1 }}</span>
            <div class="flex-1 flex items-center gap-3 flex-wrap">
              <AppBadge :variant="canalBadge[r.canal ?? ''] ?? 'outline'" class="text-xs">
                {{ r.canal ?? 'DIRECTO' }}
              </AppBadge>
              <span class="text-sm text-muted-foreground">
                Completado: <span class="text-foreground font-medium">{{ fmtFechaHora(r.completadoEn) }}</span>
              </span>
            </div>
            <component :is="expandedId === r.id ? ChevronUp : ChevronDown" class="h-4 w-4 text-muted-foreground shrink-0" />
          </button>

          <div v-if="expandedId === r.id" class="border-t border-border">
            <div v-if="loadingDetalle && !detallesCache[r.id]" class="flex justify-center py-6"><AppSpinner /></div>
            <div v-else-if="detallesCache[r.id]" class="divide-y divide-border">
              <div v-for="d in detallesCache[r.id]" :key="d.id" class="px-5 py-3 grid grid-cols-1 sm:grid-cols-2 gap-1">
                <p class="text-sm text-muted-foreground">{{ preguntaMap[d.preguntaId]?.titulo ?? d.preguntaId }}</p>
                <p class="text-sm font-medium break-words">{{ fmtValor(d) }}</p>
              </div>
              <div v-if="detallesCache[r.id].length === 0" class="px-5 py-4 text-sm text-muted-foreground">Sin detalles.</div>
            </div>
          </div>
        </AppCard>
      </div>
    </template>

    <!-- ── TAB: Estadísticas (con filtro de entidad) ──────── -->
    <template v-else-if="tab === 'estadisticas'">

      <!-- Selector de entidad como filtro -->
      <div class="flex items-center gap-2 flex-wrap">
        <span class="text-sm text-muted-foreground shrink-0">Filtrar por:</span>

        <button
          :class="[
            'rounded-full px-3 py-1 text-xs font-medium border transition-colors',
            !entidadFiltro
              ? 'bg-primary text-primary-foreground border-primary'
              : 'border-border text-muted-foreground hover:bg-accent',
          ]"
          @click="aplicarFiltroEntidad(null)"
        >
          Toda la organización
        </button>

        <template v-if="resumenEntidades.length === 0 && !loadingEntidades">
          <span class="text-xs text-muted-foreground italic" @click="cargarResumenEntidades">
            (cargando entidades...)
          </span>
        </template>

        <template v-for="ent in resumenEntidades" :key="ent.entidadId">
          <button
            :class="[
              'rounded-full px-3 py-1 text-xs font-medium border transition-colors flex items-center gap-1',
              entidadFiltro?.entidadId === ent.entidadId
                ? 'bg-primary text-primary-foreground border-primary'
                : 'border-border text-muted-foreground hover:bg-accent',
            ]"
            @click="aplicarFiltroEntidad(ent)"
          >
            {{ ent.nombreEntidad }}
            <span class="opacity-60">({{ ent.totalRespuestas }})</span>
          </button>
        </template>
      </div>

      <!-- Aviso de filtro activo -->
      <div v-if="entidadFiltro" class="flex items-center gap-2 rounded-lg bg-primary/5 border border-primary/20 px-3 py-2">
        <Building2 class="h-4 w-4 text-primary shrink-0" />
        <span class="text-sm text-primary font-medium flex-1">
          Mostrando estadísticas de: <strong>{{ entidadFiltro.nombreEntidad }}</strong>
          <span class="font-normal"> ({{ entidadFiltro.tipoEntidad }})</span>
        </span>
        <button @click="aplicarFiltroEntidad(null)" class="text-primary/60 hover:text-primary">
          <X class="h-4 w-4" />
        </button>
      </div>

      <div v-if="loadingStats" class="flex justify-center py-16"><AppSpinner size="lg" /></div>

      <template v-else-if="estadisticas">
        <!-- Resumen global -->
        <div class="grid grid-cols-2 sm:grid-cols-3 gap-4">
          <AppCard class="p-5 text-center">
            <p class="text-3xl font-bold">{{ estadisticas.totalRespuestas }}</p>
            <p class="text-sm text-muted-foreground mt-1">Respuestas totales</p>
          </AppCard>
          <AppCard v-for="dim in dimensiones" :key="dim.id" class="p-5 text-center">
            <p class="text-3xl font-bold text-primary">{{ Math.round(dim.peso * 100) }}%</p>
            <p class="text-sm text-muted-foreground mt-1 truncate" :title="dim.nombre">{{ dim.nombre }}</p>
          </AppCard>
        </div>

        <!-- Grupos por dimensión -->
        <template v-for="grupo in gruposDimension" :key="grupo.dimension?.id ?? 'sin-dimension'">
          <div v-if="grupo.dimension" class="flex items-center gap-3 pt-2">
            <div class="flex items-center gap-2 flex-1">
              <Layers class="h-4 w-4 text-violet-500 shrink-0" />
              <h3 class="font-semibold text-sm">{{ grupo.dimension.nombre }}</h3>
              <span class="text-xs text-muted-foreground">({{ Math.round(grupo.dimension.peso * 100) }}% del puntaje)</span>
            </div>
            <div v-if="grupo.promedioGlobal !== null" class="flex items-center gap-2 shrink-0">
              <div class="w-24 h-2 rounded-full bg-muted overflow-hidden">
                <div class="h-full rounded-full bg-violet-500 transition-all" :style="{ width: scoreBar(grupo) + '%' }" />
              </div>
              <span class="text-sm font-semibold text-violet-600">{{ grupo.promedioGlobal }}</span>
            </div>
          </div>
          <div v-else class="flex items-center gap-2 pt-2 text-muted-foreground">
            <div class="h-px flex-1 bg-border" />
            <span class="text-xs">Sin dimensión asignada</span>
            <div class="h-px flex-1 bg-border" />
          </div>

          <AppCard v-for="p in grupo.preguntas" :key="p.preguntaId" class="p-5 space-y-4">
            <div class="flex items-start justify-between gap-4">
              <div>
                <p class="font-semibold">{{ p.titulo }}</p>
                <p class="text-xs text-muted-foreground mt-0.5">{{ p.tipo }} · {{ p.totalRespuestas }} respuesta(s)</p>
              </div>
              <div v-if="p.promedio !== null && p.promedio !== undefined && p.tipo !== 'NPS'" class="text-right shrink-0">
                <p class="text-3xl font-bold">{{ p.promedio }}</p>
                <p class="text-xs text-muted-foreground">promedio</p>
              </div>
            </div>

            <div v-if="p.tipo === 'TEXTO' && p.textosLibres.length > 0" class="space-y-2 max-h-60 overflow-y-auto">
              <p v-for="(t, i) in p.textosLibres" :key="i" class="text-sm bg-muted/50 rounded-lg px-3 py-2 border border-border">{{ t }}</p>
            </div>

            <div v-if="['NUMERO','ESCALA','CALIFICACION'].includes(p.tipo) && p.promedio !== undefined" class="grid grid-cols-3 gap-4 text-center">
              <div><p class="text-xs text-muted-foreground">Mínimo</p><p class="text-xl font-semibold">{{ p.minimo }}</p></div>
              <div class="border-x border-border"><p class="text-xs text-muted-foreground">Promedio</p><p class="text-xl font-semibold text-primary">{{ p.promedio }}</p></div>
              <div><p class="text-xs text-muted-foreground">Máximo</p><p class="text-xl font-semibold">{{ p.maximo }}</p></div>
            </div>

            <div v-if="p.tipo === 'CALIFICACION' && p.promedio !== undefined" class="flex gap-0.5">
              <span v-for="n in 5" :key="n" :class="['text-xl', n <= Math.round(p.promedio) ? 'text-yellow-400' : 'text-muted-foreground/20']">★</span>
            </div>

            <div v-if="p.tipo === 'NPS' && p.puntajeNps !== undefined" class="space-y-3">
              <div class="flex items-center gap-3">
                <span :class="['text-4xl font-bold', npsColor(p.puntajeNps)]">{{ p.puntajeNps > 0 ? '+' : '' }}{{ p.puntajeNps }}</span>
                <span class="text-sm text-muted-foreground">NPS Score</span>
              </div>
              <div class="flex h-3 rounded-full overflow-hidden gap-0.5">
                <div v-for="c in p.conteos" :key="c.valor" :class="barColor('NPS', c.valor)" :style="{ width: c.porcentaje + '%' }" :title="`${c.etiqueta}: ${c.cantidad} (${c.porcentaje}%)`" />
              </div>
              <div class="flex gap-4 text-xs text-muted-foreground">
                <span v-for="c in p.conteos" :key="c.valor" class="flex items-center gap-1">
                  <span :class="['w-2 h-2 rounded-full inline-block', barColor('NPS', c.valor)]" />
                  {{ c.etiqueta }}: <strong class="text-foreground">{{ c.cantidad }}</strong> ({{ c.porcentaje }}%)
                </span>
              </div>
            </div>

            <div v-if="['SELECCION_UNICA','SELECCION_MULTIPLE','BOOLEANO','RANKING'].includes(p.tipo) && p.conteos.length > 0" class="space-y-2.5">
              <div v-for="c in p.conteos" :key="c.valor" class="space-y-1">
                <div class="flex items-center justify-between text-sm">
                  <span class="text-muted-foreground truncate max-w-[70%]">{{ c.etiqueta }}</span>
                  <span class="font-medium ml-2 shrink-0">{{ c.cantidad }} <span class="text-muted-foreground font-normal">({{ c.porcentaje }}%)</span></span>
                </div>
                <div class="h-2 rounded-full bg-muted overflow-hidden">
                  <div :class="['h-full rounded-full transition-all', barColor(p.tipo, c.valor)]" :style="{ width: (p.totalRespuestas > 0 ? (c.cantidad / maxConteo(p) * 100) : 0) + '%' }" />
                </div>
              </div>
            </div>

            <p v-if="p.totalRespuestas === 0" class="text-sm text-muted-foreground">Sin respuestas para esta pregunta.</p>
          </AppCard>
        </template>

        <AppCard v-if="gruposDimension.length === 0" class="p-12 text-center text-muted-foreground">
          <p class="font-medium">Sin datos estadísticos aún</p>
        </AppCard>
      </template>
    </template>

    <!-- ── TAB: Por entidad ──────────────────────────────── -->
    <template v-else-if="tab === 'entidades'">
      <div v-if="loadingEntidades" class="flex justify-center py-16"><AppSpinner size="lg" /></div>

      <template v-else-if="resumenEntidades.length === 0">
        <AppCard class="p-12 text-center text-muted-foreground">
          <Building2 class="h-10 w-10 mx-auto mb-3 opacity-30" />
          <p class="font-medium">Sin respuestas por entidad aún</p>
          <p class="text-sm mt-1">Cuando los empleados respondan, aquí verás la comparación.</p>
        </AppCard>
      </template>

      <template v-else>
        <!-- Tabla comparativa -->
        <div class="overflow-x-auto rounded-xl border border-border">
          <table class="w-full text-sm min-w-[600px]">
            <thead class="bg-muted/50">
              <tr>
                <th class="px-4 py-3 text-left font-medium text-muted-foreground sticky left-0 bg-muted/50 min-w-[160px]">
                  Entidad
                </th>
                <th class="px-4 py-3 text-center font-medium text-muted-foreground w-20">
                  Resp.
                </th>
                <th
                  v-for="p in preguntasNumericas"
                  :key="p.id"
                  class="px-4 py-3 text-center font-medium text-muted-foreground min-w-[110px]"
                >
                  <span class="truncate block max-w-[110px] mx-auto" :title="p.titulo">{{ p.titulo }}</span>
                  <span class="text-[10px] font-normal opacity-60">{{ p.tipo }}</span>
                </th>
              </tr>
            </thead>
            <tbody class="divide-y divide-border">
              <tr
                v-for="fila in filasPorEntidad"
                :key="fila.entidad.entidadId"
                :class="[
                  'hover:bg-muted/30 transition-colors',
                  fila.nivel === 0 ? 'bg-muted/20' : '',
                ]"
              >
                <td class="px-4 py-3 sticky left-0" :class="fila.nivel === 0 ? 'bg-muted/20' : 'bg-card'">
                  <div class="flex items-center gap-2" :style="{ paddingLeft: fila.nivel * 20 + 'px' }">
                    <Building2
                      :class="['h-3.5 w-3.5 shrink-0', fila.nivel === 0 ? 'text-violet-500' : 'text-muted-foreground/60']"
                    />
                    <div>
                      <p :class="['text-xs', fila.nivel === 0 ? 'font-semibold' : 'font-medium']">
                        {{ fila.entidad.nombreEntidad }}
                      </p>
                      <p class="text-[10px] text-muted-foreground">
                        {{ fila.entidad.tipoEntidad }}
                        <span v-if="fila.entidad.idExterno"> · {{ fila.entidad.idExterno }}</span>
                      </p>
                    </div>
                  </div>
                </td>
                <td
                  class="px-4 py-3 text-center text-xs"
                  :class="fila.nivel === 0 ? 'font-semibold' : 'text-muted-foreground'"
                >
                  {{ fila.entidad.totalRespuestas }}
                </td>
                <td
                  v-for="p in preguntasNumericas"
                  :key="p.id"
                  :class="[
                    'px-4 py-3 text-center text-xs font-medium',
                    metricaColor(fila.entidad, p.id),
                  ]"
                >
                  {{ metricaValor(fila.entidad, p.id) }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Leyenda de colores -->
        <div class="flex gap-4 text-xs text-muted-foreground flex-wrap">
          <span class="flex items-center gap-1.5"><span class="w-2.5 h-2.5 rounded-full bg-emerald-500 inline-block" /> Alto</span>
          <span class="flex items-center gap-1.5"><span class="w-2.5 h-2.5 rounded-full bg-amber-500 inline-block" /> Medio</span>
          <span class="flex items-center gap-1.5"><span class="w-2.5 h-2.5 rounded-full bg-red-500 inline-block" /> Bajo</span>
          <span class="ml-auto italic">Haz clic en "Estadísticas" y selecciona una entidad para ver el desglose completo</span>
        </div>

      </template>
    </template>

  </div>
</template>
