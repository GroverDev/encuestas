<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import http from '@/utils/http'
import type { Invitacion, Encuesta, Entidad, Respuesta } from '@/types/api'
import { useToast } from '@/composables/useToast'
import { fmtFechaHora } from '@/utils/dates'
import AppCard from '@/components/ui/AppCard.vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppBadge from '@/components/ui/AppBadge.vue'
import AppSpinner from '@/components/ui/AppSpinner.vue'
import AppModal from '@/components/ui/AppModal.vue'
import AppInput from '@/components/ui/AppInput.vue'
import {
  ArrowLeft, Plus, Copy, Trash2, Link, Mail, QrCode,
  CheckCircle2, Clock, XCircle, Code2, Users, ExternalLink,
} from 'lucide-vue-next'

const route  = useRoute()
const router = useRouter()
const { toast } = useToast()
const encuestaId = route.params.id as string

// ── estado ────────────────────────────────────────────────
const encuesta    = ref<Encuesta | null>(null)
const invitaciones = ref<Invitacion[]>([])
const entidades   = ref<Entidad[]>([])
const loading     = ref(true)
const tab         = ref<'lista' | 'api'>('lista')
const filtroEstado = ref<string>('TODAS')

// modal crear
const modalOpen    = ref(false)
const saving       = ref(false)
const deletingId   = ref<string | null>(null)
const copiadoId    = ref<string | null>(null)
const modoEntidad  = ref<'selector' | 'idExterno'>('selector')

const form = ref({
  entidadId:               '',
  entidadEvaluadaIdExterno: '',
  correoDestino:            '',
  canal:                    'ENLACE_PUBLICO',
  venceEn:                  '',
})

// ── canales / estados ─────────────────────────────────────
const CANALES = [
  { value: 'ENLACE_PUBLICO', label: 'Enlace público', icon: Link  },
  { value: 'EMAIL',          label: 'Email',           icon: Mail  },
  { value: 'QR',             label: 'QR',              icon: QrCode },
  { value: 'KIOSCO',         label: 'Kiosco',          icon: Users },
]

const ESTADO_BADGE: Record<string, 'warning' | 'success' | 'default' | 'destructive'> = {
  PENDIENTE:  'warning',
  RESPONDIDA: 'success',
  EXPIRADA:   'default',
  CANCELADA:  'destructive',
}

// ── carga ─────────────────────────────────────────────────
async function cargar() {
  loading.value = true
  try {
    const [encRes, invRes, entRes] = await Promise.all([
      http.get<Respuesta<Encuesta>>(`/encuesta/${encuestaId}`),
      http.get<Respuesta<Invitacion[]>>(`/invitacion/${encuestaId}`),
      http.get<Respuesta<Entidad[]>>('/entidad'),
    ])
    if (encRes.data.ok) encuesta.value = encRes.data.datos
    if (invRes.data.ok) invitaciones.value = invRes.data.datos
    if (entRes.data.ok) entidades.value = entRes.data.datos.filter(e => e.esActivo)
  } finally {
    loading.value = false
  }
}

onMounted(cargar)

// ── helpers ───────────────────────────────────────────────
function nombreEntidad(id?: string) {
  if (!id) return null
  return entidades.value.find(e => e.id === id)?.nombreVisible ?? null
}

function etiquetaInvitacion(inv: Invitacion): string {
  if (inv.entidadEvaluadaId) return nombreEntidad(inv.entidadEvaluadaId) ?? inv.entidadEvaluadaId
  if (inv.correoDestino)     return inv.correoDestino
  return '— General —'
}

function enlaceRespuesta(token: string): string {
  return `${window.location.origin}/responder/${token}`
}

async function copiarEnlace(inv: Invitacion) {
  await navigator.clipboard.writeText(enlaceRespuesta(inv.tokenAcceso))
  copiadoId.value = inv.id
  setTimeout(() => { copiadoId.value = null }, 2000)
}

// ── filtro y stats ────────────────────────────────────────
const stats = computed(() => ({
  total:      invitaciones.value.length,
  pendiente:  invitaciones.value.filter(i => i.estado === 'PENDIENTE').length,
  respondida: invitaciones.value.filter(i => i.estado === 'RESPONDIDA').length,
  expirada:   invitaciones.value.filter(i => i.estado === 'EXPIRADA').length,
}))

const listaFiltrada = computed(() => {
  if (filtroEstado.value === 'TODAS') return invitaciones.value
  return invitaciones.value.filter(i => i.estado === filtroEstado.value)
})

// ── crear ─────────────────────────────────────────────────
function abrirModal() {
  form.value = { entidadId: '', entidadEvaluadaIdExterno: '', correoDestino: '', canal: 'ENLACE_PUBLICO', venceEn: '' }
  modoEntidad.value = 'selector'
  modalOpen.value = true
}

async function crear() {
  saving.value = true
  try {
    const payload: Record<string, unknown> = {
      encuestaId,
      canal: form.value.canal,
      ...(form.value.venceEn ? { venceEn: form.value.venceEn } : {}),
      ...(form.value.correoDestino ? { correoDestino: form.value.correoDestino } : {}),
    }
    if (modoEntidad.value === 'selector' && form.value.entidadId) {
      payload.entidadEvaluadaId = form.value.entidadId
    } else if (modoEntidad.value === 'idExterno' && form.value.entidadEvaluadaIdExterno.trim()) {
      payload.entidadEvaluadaIdExterno = form.value.entidadEvaluadaIdExterno.trim()
    }

    const { data } = await http.post<Respuesta<string>>('/invitacion', payload)
    if (data.ok) {
      toast.success('Invitación creada')
      modalOpen.value = false
      await cargar()
    } else {
      toast.error(data.mensaje?.descripcion ?? 'No se pudo crear')
    }
  } catch {
    toast.error('Error al crear la invitación')
  } finally {
    saving.value = false
  }
}

// ── eliminar ──────────────────────────────────────────────
async function eliminar(inv: Invitacion) {
  if (!confirm('¿Eliminar esta invitación? El enlace quedará inválido.')) return
  deletingId.value = inv.id
  try {
    await http.delete(`/invitacion/${inv.encuestaId}/${inv.id}`)
    toast.success('Invitación eliminada')
    invitaciones.value = invitaciones.value.filter(i => i.id !== inv.id)
  } catch {
    toast.error('No se pudo eliminar')
  } finally {
    deletingId.value = null
  }
}

// ── api snippet ───────────────────────────────────────────
const apiBaseUrl = computed(() => `${window.location.origin.replace('5173', '5000')}/api`)

const snippetIdExterno = computed(() => `POST ${apiBaseUrl.value}/invitacion
Authorization: Bearer {tu_token_jwt}
Content-Type: application/json

{
  "encuestaId": "${encuestaId}",
  "entidadEvaluadaIdExterno": "EMP001",
  "canal": "ENLACE_PUBLICO"
}`)

const snippetUUID = computed(() => `POST ${apiBaseUrl.value}/invitacion
Authorization: Bearer {tu_token_jwt}
Content-Type: application/json

{
  "encuestaId": "${encuestaId}",
  "entidadEvaluadaId": "550e8400-e29b-41d4-a716-446655440000",
  "canal": "ENLACE_PUBLICO"
}`)

const snippetRespuesta = `// Respuesta exitosa
{
  "ok": true,
  "datos": "abc12345-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
}

// Construye el enlace con el token devuelto:
https://tu-dominio/responder/{datos}`

async function copiarSnippet(text: string) {
  await navigator.clipboard.writeText(text)
  toast.success('Copiado al portapapeles')
}
</script>

<template>
  <div class="space-y-6">

    <!-- Cabecera -->
    <div class="flex items-center gap-3">
      <AppButton variant="ghost" size="icon" @click="router.back()">
        <ArrowLeft class="h-5 w-5" />
      </AppButton>
      <div class="flex-1 min-w-0">
        <h2 class="text-xl font-semibold truncate">
          Invitaciones — {{ encuesta?.titulo ?? '…' }}
        </h2>
        <p class="text-sm text-muted-foreground">Gestiona quién puede responder esta encuesta</p>
      </div>
      <AppButton @click="abrirModal">
        <Plus class="h-4 w-4" />
        Nueva invitación
      </AppButton>
    </div>

    <div v-if="loading" class="flex justify-center py-16"><AppSpinner size="lg" /></div>

    <template v-else>

      <!-- Stats -->
      <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
        <AppCard
          v-for="s in [
            { label: 'Total', value: stats.total,      icon: Link,         color: 'text-primary'     },
            { label: 'Pendientes', value: stats.pendiente,  icon: Clock,        color: 'text-amber-500'   },
            { label: 'Respondidas', value: stats.respondida, icon: CheckCircle2, color: 'text-emerald-500' },
            { label: 'Expiradas',  value: stats.expirada,   icon: XCircle,      color: 'text-muted-foreground' },
          ]"
          :key="s.label"
          class="p-4"
        >
          <div class="flex items-center justify-between">
            <div>
              <p class="text-xs text-muted-foreground">{{ s.label }}</p>
              <p class="text-2xl font-bold">{{ s.value }}</p>
            </div>
            <component :is="s.icon" :class="['h-5 w-5', s.color]" />
          </div>
        </AppCard>
      </div>

      <!-- Tabs -->
      <div class="flex gap-1 border-b border-border">
        <button
          v-for="t in [{ key: 'lista', label: 'Invitaciones' }, { key: 'api', label: 'Integración API' }]"
          :key="t.key"
          :class="[
            'px-4 py-2 text-sm font-medium border-b-2 -mb-px transition-colors',
            tab === t.key
              ? 'border-primary text-primary'
              : 'border-transparent text-muted-foreground hover:text-foreground',
          ]"
          @click="tab = t.key as 'lista' | 'api'"
        >{{ t.label }}</button>
      </div>

      <!-- ── TAB LISTA ───────────────────────────────────── -->
      <template v-if="tab === 'lista'">

        <!-- Filtro por estado -->
        <div class="flex gap-2 flex-wrap">
          <button
            v-for="f in ['TODAS', 'PENDIENTE', 'RESPONDIDA', 'EXPIRADA', 'CANCELADA']"
            :key="f"
            :class="[
              'rounded-full px-3 py-1 text-xs font-medium border transition-colors',
              filtroEstado === f
                ? 'bg-primary text-primary-foreground border-primary'
                : 'border-border text-muted-foreground hover:bg-accent',
            ]"
            @click="filtroEstado = f"
          >{{ f === 'TODAS' ? 'Todas' : f.charAt(0) + f.slice(1).toLowerCase() }}</button>
        </div>

        <!-- Lista vacía -->
        <AppCard v-if="listaFiltrada.length === 0" class="p-12 text-center text-muted-foreground">
          <Link class="h-10 w-10 mx-auto mb-3 opacity-30" />
          <p class="font-medium">Sin invitaciones{{ filtroEstado !== 'TODAS' ? ` ${filtroEstado.toLowerCase()}s` : '' }}</p>
          <p class="text-sm mt-1">Crea una nueva invitación para que alguien pueda responder.</p>
        </AppCard>

        <!-- Lista de invitaciones -->
        <div v-else class="space-y-2">
          <div
            v-for="inv in listaFiltrada"
            :key="inv.id"
            class="rounded-lg border border-border bg-card px-4 py-3 space-y-2"
          >
            <!-- Fila superior: canal + info + acciones -->
            <div class="flex items-center gap-3">
              <!-- Ícono canal -->
              <div class="shrink-0 rounded-md p-2 bg-muted">
                <component
                  :is="CANALES.find(c => c.value === inv.canal)?.icon ?? Link"
                  class="h-4 w-4 text-muted-foreground"
                />
              </div>

              <!-- Info principal -->
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium truncate">{{ etiquetaInvitacion(inv) }}</p>
                <div class="flex items-center gap-2 mt-0.5 flex-wrap">
                  <AppBadge :variant="ESTADO_BADGE[inv.estado] ?? 'default'" class="text-[10px] py-0">
                    {{ inv.estado }}
                  </AppBadge>
                  <span class="text-xs text-muted-foreground">{{ inv.canal }}</span>
                  <span v-if="inv.respondidoEn" class="text-xs text-muted-foreground">
                    Respondida: {{ fmtFechaHora(inv.respondidoEn) }}
                  </span>
                  <span v-else-if="inv.venceEn" class="text-xs text-muted-foreground">
                    Vence: {{ fmtFechaHora(inv.venceEn) }}
                  </span>
                </div>
              </div>

              <!-- Acciones -->
              <div class="flex items-center gap-1 shrink-0">
                <AppButton
                  variant="ghost" size="icon"
                  class="h-8 w-8"
                  title="Abrir enlace"
                  @click="() => window.open(enlaceRespuesta(inv.tokenAcceso), '_blank')"
                >
                  <ExternalLink class="h-3.5 w-3.5" />
                </AppButton>
                <AppButton
                  variant="ghost" size="icon"
                  :class="['h-8 w-8 transition-colors', copiadoId === inv.id ? 'text-emerald-500' : '']"
                  title="Copiar enlace"
                  @click="copiarEnlace(inv)"
                >
                  <CheckCircle2 v-if="copiadoId === inv.id" class="h-3.5 w-3.5" />
                  <Copy v-else class="h-3.5 w-3.5" />
                </AppButton>
                <AppButton
                  variant="ghost" size="icon"
                  class="h-8 w-8 text-destructive hover:text-destructive"
                  title="Eliminar"
                  :loading="deletingId === inv.id"
                  @click="eliminar(inv)"
                >
                  <Trash2 class="h-3.5 w-3.5" />
                </AppButton>
              </div>
            </div>

            <!-- Fila del enlace (siempre visible) -->
            <div class="flex items-center gap-2 rounded-md bg-muted px-3 py-1.5">
              <Link class="h-3 w-3 text-muted-foreground shrink-0" />
              <span class="text-xs font-mono text-muted-foreground truncate flex-1">
                {{ enlaceRespuesta(inv.tokenAcceso) }}
              </span>
            </div>
          </div>
        </div>

      </template>

      <!-- ── TAB API ─────────────────────────────────────── -->
      <template v-else>
        <div class="space-y-6">

          <AppCard class="p-5 space-y-3">
            <div class="flex items-center gap-2">
              <Code2 class="h-4 w-4 text-primary" />
              <h3 class="font-semibold text-sm">Autenticación</h3>
            </div>
            <p class="text-sm text-muted-foreground">
              Todas las llamadas requieren un token JWT obtenido desde
              <code class="bg-muted px-1.5 py-0.5 rounded text-xs">POST /api/login</code>.
              Inclúyelo en el header:
            </p>
            <div class="relative">
              <pre class="bg-muted rounded-lg p-4 text-xs font-mono overflow-x-auto">Authorization: Bearer {tu_token_jwt}</pre>
            </div>
          </AppCard>

          <!-- Opción 1: ID externo -->
          <AppCard class="p-5 space-y-3">
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2">
                <Code2 class="h-4 w-4 text-primary" />
                <h3 class="font-semibold text-sm">Crear invitación con ID externo</h3>
              </div>
              <AppButton variant="ghost" size="sm" @click="copiarSnippet(snippetIdExterno)">
                <Copy class="h-3.5 w-3.5" />
                Copiar
              </AppButton>
            </div>
            <p class="text-sm text-muted-foreground">
              Usa el <code class="bg-muted px-1.5 py-0.5 rounded text-xs">idExterno</code> del empleado
              registrado en la plataforma. El backend resuelve el UUID automáticamente.
            </p>
            <pre class="bg-muted rounded-lg p-4 text-xs font-mono overflow-x-auto whitespace-pre">{{ snippetIdExterno }}</pre>
          </AppCard>

          <!-- Opción 2: UUID directo -->
          <AppCard class="p-5 space-y-3">
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2">
                <Code2 class="h-4 w-4 text-primary" />
                <h3 class="font-semibold text-sm">Crear invitación con UUID interno</h3>
              </div>
              <AppButton variant="ghost" size="sm" @click="copiarSnippet(snippetUUID)">
                <Copy class="h-3.5 w-3.5" />
                Copiar
              </AppButton>
            </div>
            <p class="text-sm text-muted-foreground">
              Usa el UUID de la entidad si ya lo tienes resuelto en tu sistema.
            </p>
            <pre class="bg-muted rounded-lg p-4 text-xs font-mono overflow-x-auto whitespace-pre">{{ snippetUUID }}</pre>
          </AppCard>

          <!-- Respuesta -->
          <AppCard class="p-5 space-y-3">
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2">
                <Code2 class="h-4 w-4 text-primary" />
                <h3 class="font-semibold text-sm">Respuesta y construcción del enlace</h3>
              </div>
              <AppButton variant="ghost" size="sm" @click="copiarSnippet(snippetRespuesta)">
                <Copy class="h-3.5 w-3.5" />
                Copiar
              </AppButton>
            </div>
            <p class="text-sm text-muted-foreground">
              El campo <code class="bg-muted px-1.5 py-0.5 rounded text-xs">datos</code>
              contiene el token UUID. Úsalo para construir el enlace del respondente.
            </p>
            <pre class="bg-muted rounded-lg p-4 text-xs font-mono overflow-x-auto whitespace-pre">{{ snippetRespuesta }}</pre>
          </AppCard>

          <!-- Tabla de campos -->
          <AppCard class="p-5 space-y-3">
            <h3 class="font-semibold text-sm">Campos del body</h3>
            <div class="overflow-x-auto">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b border-border">
                    <th class="text-left py-2 pr-4 font-medium text-muted-foreground">Campo</th>
                    <th class="text-left py-2 pr-4 font-medium text-muted-foreground">Tipo</th>
                    <th class="text-left py-2 font-medium text-muted-foreground">Descripción</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-border">
                  <tr v-for="f in [
                    { campo: 'encuestaId',                tipo: 'UUID',    desc: `ID de esta encuesta: ${encuestaId}` },
                    { campo: 'entidadEvaluadaIdExterno',  tipo: 'string?', desc: 'ID externo del empleado en tu sistema (recomendado)' },
                    { campo: 'entidadEvaluadaId',         tipo: 'UUID?',   desc: 'UUID interno de la entidad (alternativa)' },
                    { campo: 'canal',                     tipo: 'string',  desc: 'ENLACE_PUBLICO | EMAIL | QR | KIOSCO' },
                    { campo: 'correoDestino',             tipo: 'string?', desc: 'Requerido solo si canal = EMAIL' },
                    { campo: 'venceEn',                   tipo: 'datetime?',desc: 'Fecha de expiración (ISO 8601). Omitir = sin vencimiento' },
                  ]" :key="f.campo">
                    <td class="py-2 pr-4 font-mono text-xs">{{ f.campo }}</td>
                    <td class="py-2 pr-4 text-xs text-muted-foreground">{{ f.tipo }}</td>
                    <td class="py-2 text-xs text-muted-foreground">{{ f.desc }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </AppCard>

        </div>
      </template>

    </template>

    <!-- ── Modal nueva invitación ─────────────────────────── -->
    <AppModal
      :open="modalOpen"
      title="Nueva invitación"
      @close="modalOpen = false"
    >
      <div class="space-y-4">

        <!-- Modo entidad -->
        <div class="space-y-1.5">
          <label class="text-sm font-medium">Identificar evaluado por</label>
          <div class="flex rounded-lg border border-border overflow-hidden">
            <button
              v-for="m in [{ key: 'selector', label: 'Seleccionar entidad' }, { key: 'idExterno', label: 'ID externo' }]"
              :key="m.key"
              type="button"
              :class="[
                'flex-1 py-2 text-xs font-medium transition-colors',
                modoEntidad === m.key
                  ? 'bg-primary text-primary-foreground'
                  : 'text-muted-foreground hover:bg-accent',
              ]"
              @click="modoEntidad = m.key as 'selector' | 'idExterno'"
            >{{ m.label }}</button>
          </div>
        </div>

        <!-- Selector de entidad -->
        <div v-if="modoEntidad === 'selector'" class="space-y-1.5">
          <label class="text-sm font-medium">
            Entidad evaluada <span class="text-muted-foreground font-normal">(opcional)</span>
          </label>
          <select
            v-model="form.entidadId"
            class="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
          >
            <option value="">— Sin entidad específica (enlace general) —</option>
            <option v-for="e in entidades" :key="e.id" :value="e.id">
              {{ e.nombreVisible }}{{ e.idExterno ? ` (${e.idExterno})` : '' }}
            </option>
          </select>
        </div>

        <!-- ID externo -->
        <AppInput
          v-else
          v-model="form.entidadEvaluadaIdExterno"
          label="ID externo del evaluado"
          placeholder="Ej: EMP001"
        />

        <!-- Canal -->
        <div class="space-y-1.5">
          <label class="text-sm font-medium">Canal</label>
          <div class="grid grid-cols-2 gap-2">
            <button
              v-for="c in CANALES" :key="c.value"
              type="button"
              :class="[
                'flex items-center gap-2 rounded-lg border px-3 py-2.5 text-xs font-medium transition-colors text-left',
                form.canal === c.value
                  ? 'border-primary bg-primary/10 text-primary'
                  : 'border-border text-muted-foreground hover:bg-accent',
              ]"
              @click="form.canal = c.value"
            >
              <component :is="c.icon" class="h-3.5 w-3.5 shrink-0" />
              {{ c.label }}
            </button>
          </div>
        </div>

        <!-- Correo destino (solo EMAIL) -->
        <AppInput
          v-if="form.canal === 'EMAIL'"
          v-model="form.correoDestino"
          label="Correo destino"
          placeholder="cliente@ejemplo.com"
          type="email"
        />

        <!-- Fecha de vencimiento -->
        <div class="space-y-1.5">
          <label class="text-sm font-medium">
            Vence el <span class="text-muted-foreground font-normal">(opcional)</span>
          </label>
          <input
            v-model="form.venceEn"
            type="datetime-local"
            class="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
          />
        </div>

      </div>

      <template #footer>
        <AppButton variant="outline" @click="modalOpen = false">Cancelar</AppButton>
        <AppButton :loading="saving" @click="crear">Crear invitación</AppButton>
      </template>
    </AppModal>

  </div>
</template>
