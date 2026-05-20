<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import http from '@/utils/http'
import type { EncuestaPublica, PreguntaPublica, DetalleRespuestaPublica, Respuesta, ReglaJson, SeccionPublica } from '@/types/api'
import AppSpinner from '@/components/ui/AppSpinner.vue'
import AppButton from '@/components/ui/AppButton.vue'
import { ChevronLeft, ChevronRight, Send } from 'lucide-vue-next'

const route = useRoute()
const token = route.params.token as string

const encuesta  = ref<EncuestaPublica | null>(null)
const loading   = ref(true)
const error     = ref<string | null>(null)
const enviando  = ref(false)
const enviado   = ref(false)

const respuestas = ref<Record<string, DetalleRespuestaPublica>>({})

// ── sección actual ────────────────────────────────────────
const seccionIdx = ref(0)

function initRespuesta(p: PreguntaPublica) {
  respuestas.value[p.id] = { preguntaId: p.id }
}

onMounted(async () => {
  try {
    const { data } = await http.get<Respuesta<EncuestaPublica>>(`/publico/encuesta/${token}`)
    if (data.ok) {
      encuesta.value = data.datos
      encuesta.value.preguntas.forEach(initRespuesta)
    } else {
      error.value = data.mensaje?.descripcion ?? 'Enlace no válido o encuesta no disponible.'
    }
  } catch {
    error.value = 'No se pudo cargar la encuesta.'
  } finally {
    loading.value = false
  }
})

// ── estructura de secciones ───────────────────────────────
const secciones = computed((): (SeccionPublica & { preguntas: PreguntaPublica[] })[] => {
  if (!encuesta.value) return []
  const all = encuesta.value.preguntas
  const secs = encuesta.value.secciones

  if (secs.length === 0) {
    return [{ id: '', titulo: '', descripcion: undefined, orden: 0, preguntas: all }]
  }

  const secMap = new Map(secs.map(s => [s.id, { ...s, preguntas: [] as PreguntaPublica[] }]))

  // questions without section go to a "general" bucket appended at end
  const sinSeccion: PreguntaPublica[] = []
  for (const p of all) {
    if (p.seccionId && secMap.has(p.seccionId)) {
      secMap.get(p.seccionId)!.preguntas.push(p)
    } else {
      sinSeccion.push(p)
    }
  }

  const result = [...secMap.values()].sort((a, b) => a.orden - b.orden)
  if (sinSeccion.length > 0) {
    result.push({ id: '', titulo: 'Otras preguntas', descripcion: undefined, orden: 9999, preguntas: sinSeccion })
  }
  return result.filter(s => s.preguntas.length > 0)
})

const seccionActual = computed(() => secciones.value[seccionIdx.value] ?? null)
const esUltimaSeccion = computed(() => seccionIdx.value === secciones.value.length - 1)
const progreso = computed(() =>
  secciones.value.length > 0 ? ((seccionIdx.value + 1) / secciones.value.length) * 100 : 100
)

// ── helpers de valor ──────────────────────────────────────
function setTexto(preguntaId: string, v: string) {
  respuestas.value[preguntaId] = { preguntaId, valorTexto: v }
}
function setNumero(preguntaId: string, v: number | null) {
  respuestas.value[preguntaId] = { preguntaId, valorNumero: v ?? undefined }
}
function setBooleano(preguntaId: string, v: boolean) {
  respuestas.value[preguntaId] = { preguntaId, valorBooleano: v }
}
function setFecha(preguntaId: string, v: string) {
  respuestas.value[preguntaId] = { preguntaId, valorFecha: v || undefined }
}
function toggleMultiple(preguntaId: string, valor: string) {
  const actual = getJsonArray(preguntaId)
  const idx = actual.indexOf(valor)
  if (idx === -1) actual.push(valor)
  else actual.splice(idx, 1)
  respuestas.value[preguntaId] = { preguntaId, valorJson: JSON.stringify(actual) }
}
function getJsonArray(preguntaId: string): string[] {
  try { return JSON.parse(respuestas.value[preguntaId]?.valorJson ?? '[]') }
  catch { return [] }
}
function isMultipleSelected(preguntaId: string, valor: string) {
  return getJsonArray(preguntaId).includes(valor)
}

// ── motor de reglas ───────────────────────────────────────
function valorActual(preguntaId: string): string {
  const r = respuestas.value[preguntaId]
  if (!r) return ''
  if (r.valorTexto    !== undefined) return r.valorTexto
  if (r.valorNumero   !== undefined) return String(r.valorNumero)
  if (r.valorBooleano !== undefined) return r.valorBooleano ? 'true' : 'false'
  if (r.valorFecha    !== undefined) return r.valorFecha
  return ''
}

function evaluarCondicion(si: ReglaJson['si']): boolean {
  const v = valorActual(si.preguntaId)
  if (v === '') return false
  switch (si.operador) {
    case 'igual':    return v === si.valor
    case 'distinto': return v !== si.valor
    case 'mayor':    return parseFloat(v) > parseFloat(si.valor)
    case 'menor':    return parseFloat(v) < parseFloat(si.valor)
    case 'contiene': return v.toLowerCase().includes(si.valor.toLowerCase())
    default:         return false
  }
}

const preguntasVisibles = computed((): Set<string> => {
  if (!encuesta.value) return new Set()

  const preguntas = encuesta.value.preguntas
  const reglas: ReglaJson[] = []
  for (const raw of encuesta.value.reglas ?? []) {
    try { reglas.push(JSON.parse(raw) as ReglaJson) } catch { /* skip */ }
  }

  const mostrarTargets = new Set<string>()
  for (const { entonces } of reglas) {
    if (entonces.accion === 'mostrar' && entonces.preguntaObjetivoId)
      mostrarTargets.add(entonces.preguntaObjetivoId)
  }

  const ocultas = new Set<string>(mostrarTargets)

  for (const { si, entonces } of reglas) {
    const ok = evaluarCondicion(si)
    if (entonces.accion === 'mostrar' && entonces.preguntaObjetivoId) {
      if (ok) ocultas.delete(entonces.preguntaObjetivoId)
    } else if (entonces.accion === 'ocultar' && entonces.preguntaObjetivoId) {
      if (ok) ocultas.add(entonces.preguntaObjetivoId)
      else    ocultas.delete(entonces.preguntaObjetivoId)
    } else if (entonces.accion === 'saltar' && ok) {
      const srcIdx = preguntas.findIndex(p => p.id === si.preguntaId)
      let tgtIdx = -1
      if (entonces.preguntaObjetivoId)
        tgtIdx = preguntas.findIndex(p => p.id === entonces.preguntaObjetivoId)
      else if (entonces.seccionObjetivoId)
        tgtIdx = preguntas.findIndex(p => p.seccionId === entonces.seccionObjetivoId)
      if (srcIdx !== -1 && tgtIdx > srcIdx + 1) {
        for (let i = srcIdx + 1; i < tgtIdx; i++) ocultas.add(preguntas[i].id)
      }
    }
  }

  const visible = new Set<string>()
  for (const p of preguntas) {
    if (!ocultas.has(p.id)) visible.add(p.id)
  }
  return visible
})

function esVisible(id: string): boolean {
  return preguntasVisibles.value.has(id)
}

// ── numeración global de preguntas ────────────────────────
const numeroGlobal = computed(() => {
  const map: Record<string, number> = {}
  let n = 1
  for (const p of encuesta.value?.preguntas ?? []) {
    map[p.id] = n++
  }
  return map
})

// ── validar sección actual ────────────────────────────────
function validarSeccion(): string | null {
  if (!seccionActual.value) return null
  for (const p of seccionActual.value.preguntas) {
    if (!p.esObligatoria || !esVisible(p.id)) continue
    const r = respuestas.value[p.id]
    const vacio =
      r.valorTexto == null && r.valorNumero == null &&
      r.valorBooleano == null && r.valorFecha == null &&
      (r.valorJson == null || r.valorJson === '[]')
    if (vacio) return `La pregunta "${p.titulo}" es obligatoria.`
  }
  return null
}

function siguiente() {
  const err = validarSeccion()
  if (err) { alert(err); return }
  if (!esUltimaSeccion.value) {
    seccionIdx.value++
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }
}

function anterior() {
  if (seccionIdx.value > 0) {
    seccionIdx.value--
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }
}

async function enviar() {
  const err = validarSeccion()
  if (err) { alert(err); return }

  enviando.value = true
  try {
    const detalles = Object.values(respuestas.value).filter(d => esVisible(d.preguntaId))
    const { data } = await http.post<Respuesta<boolean>>(`/publico/responder/${token}`, { detalles })
    if (data.ok) {
      enviado.value = true
    } else {
      alert(data.mensaje?.descripcion ?? 'No se pudo enviar la respuesta.')
    }
  } catch {
    alert('Error al enviar la respuesta.')
  } finally {
    enviando.value = false
  }
}

function configJson(p: PreguntaPublica): Record<string, unknown> {
  try { return JSON.parse(p.configuracionJson ?? '{}') }
  catch { return {} }
}
</script>

<template>
  <div class="min-h-screen bg-muted/40 py-10 px-4">
    <div class="max-w-2xl mx-auto space-y-5">

      <!-- Cargando -->
      <div v-if="loading" class="flex justify-center py-20">
        <AppSpinner size="lg" />
      </div>

      <!-- Error -->
      <div v-else-if="error" class="bg-card rounded-xl border border-border p-10 text-center">
        <p class="text-2xl font-semibold mb-2">Enlace no disponible</p>
        <p class="text-muted-foreground">{{ error }}</p>
      </div>

      <!-- Enviado -->
      <div v-else-if="enviado" class="bg-card rounded-xl border border-border p-12 text-center space-y-3">
        <div class="text-5xl">✓</div>
        <p class="text-2xl font-semibold">¡Gracias por responder!</p>
        <p class="text-muted-foreground">Tu respuesta fue registrada correctamente.</p>
      </div>

      <!-- Formulario -->
      <template v-else-if="encuesta">

        <!-- Encabezado encuesta -->
        <div class="bg-card rounded-xl border border-border p-6 space-y-3">
          <div>
            <h1 class="text-2xl font-bold">{{ encuesta.titulo }}</h1>
            <p v-if="encuesta.descripcion" class="text-muted-foreground text-sm mt-1">{{ encuesta.descripcion }}</p>
          </div>

          <!-- Progreso de secciones -->
          <div v-if="secciones.length > 1" class="space-y-1.5">
            <div class="flex justify-between text-xs text-muted-foreground">
              <span>Sección {{ seccionIdx + 1 }} de {{ secciones.length }}</span>
              <span>{{ Math.round(progreso) }}%</span>
            </div>
            <div class="h-1.5 rounded-full bg-muted overflow-hidden">
              <div
                class="h-full rounded-full bg-primary transition-all duration-500"
                :style="{ width: progreso + '%' }"
              />
            </div>
          </div>
        </div>

        <!-- Encabezado sección actual -->
        <div
          v-if="seccionActual?.titulo"
          class="bg-primary/5 border border-primary/20 rounded-xl px-6 py-4"
        >
          <p class="text-xs font-semibold text-primary uppercase tracking-wider mb-0.5">
            Sección {{ seccionIdx + 1 }}
          </p>
          <h2 class="text-lg font-semibold">{{ seccionActual.titulo }}</h2>
          <p v-if="seccionActual.descripcion" class="text-sm text-muted-foreground mt-0.5">
            {{ seccionActual.descripcion }}
          </p>
        </div>

        <!-- Preguntas de la sección actual -->
        <template v-if="seccionActual">
          <template v-for="(p, localIdx) in seccionActual.preguntas" :key="p.id">
            <div
              v-show="esVisible(p.id)"
              class="bg-card rounded-xl border border-border p-6 space-y-4 transition-all duration-200"
            >
              <div>
                <p class="font-semibold">
                  <span class="text-muted-foreground mr-2">{{ numeroGlobal[p.id] }}.</span>
                  {{ p.titulo }}
                  <span v-if="p.esObligatoria" class="text-destructive ml-1">*</span>
                </p>
                <p v-if="p.descripcion" class="text-sm text-muted-foreground mt-0.5">{{ p.descripcion }}</p>
              </div>

              <!-- TEXTO -->
              <textarea
                v-if="p.tipo === 'TEXTO'"
                :value="respuestas[p.id]?.valorTexto ?? ''"
                @input="setTexto(p.id, ($event.target as HTMLTextAreaElement).value)"
                rows="3"
                :placeholder="(configJson(p).placeholder as string) ?? 'Escribe tu respuesta...'"
                class="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring resize-none"
              />

              <!-- NUMERO -->
              <input
                v-else-if="p.tipo === 'NUMERO'"
                type="number"
                :value="respuestas[p.id]?.valorNumero ?? ''"
                @input="setNumero(p.id, ($event.target as HTMLInputElement).valueAsNumber)"
                placeholder="0"
                class="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
              />

              <!-- FECHA -->
              <input
                v-else-if="p.tipo === 'FECHA'"
                type="date"
                :value="respuestas[p.id]?.valorFecha ?? ''"
                @change="setFecha(p.id, ($event.target as HTMLInputElement).value)"
                class="rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
              />

              <!-- BOOLEANO -->
              <div v-else-if="p.tipo === 'BOOLEANO'" class="flex gap-3">
                <button
                  v-for="opt in [{ label: 'Sí', value: true }, { label: 'No', value: false }]"
                  :key="String(opt.value)"
                  type="button"
                  :class="[
                    'flex-1 rounded-lg border px-4 py-3 text-sm font-medium transition-colors',
                    respuestas[p.id]?.valorBooleano === opt.value
                      ? 'border-primary bg-primary/10 text-primary'
                      : 'border-border hover:bg-accent',
                  ]"
                  @click="setBooleano(p.id, opt.value)"
                >{{ opt.label }}</button>
              </div>

              <!-- SELECCION_UNICA -->
              <div v-else-if="p.tipo === 'SELECCION_UNICA'" class="space-y-2">
                <button
                  v-for="o in p.opciones" :key="o.id"
                  type="button"
                  :class="[
                    'w-full text-left rounded-lg border px-4 py-3 text-sm transition-colors',
                    respuestas[p.id]?.valorTexto === o.valor
                      ? 'border-primary bg-primary/10 text-primary font-medium'
                      : 'border-border hover:bg-accent',
                  ]"
                  @click="setTexto(p.id, o.valor)"
                >{{ o.etiqueta }}</button>
              </div>

              <!-- SELECCION_MULTIPLE -->
              <div v-else-if="p.tipo === 'SELECCION_MULTIPLE'" class="space-y-2">
                <button
                  v-for="o in p.opciones" :key="o.id"
                  type="button"
                  :class="[
                    'w-full text-left rounded-lg border px-4 py-3 text-sm transition-colors flex items-center gap-3',
                    isMultipleSelected(p.id, o.valor)
                      ? 'border-primary bg-primary/10 text-primary font-medium'
                      : 'border-border hover:bg-accent',
                  ]"
                  @click="toggleMultiple(p.id, o.valor)"
                >
                  <span :class="['w-4 h-4 rounded border flex items-center justify-center shrink-0 text-xs',
                    isMultipleSelected(p.id, o.valor) ? 'bg-primary border-primary text-primary-foreground' : 'border-muted-foreground']">
                    <span v-if="isMultipleSelected(p.id, o.valor)">✓</span>
                  </span>
                  {{ o.etiqueta }}
                </button>
              </div>

              <!-- ESCALA -->
              <div v-else-if="p.tipo === 'ESCALA'" class="space-y-2">
                <input
                  type="range"
                  :min="(configJson(p).min as number) ?? 1"
                  :max="(configJson(p).max as number) ?? 10"
                  :step="(configJson(p).paso as number) ?? 1"
                  :value="respuestas[p.id]?.valorNumero ?? (configJson(p).min as number) ?? 1"
                  @input="setNumero(p.id, ($event.target as HTMLInputElement).valueAsNumber)"
                  class="w-full accent-primary"
                />
                <div class="flex justify-between text-xs text-muted-foreground">
                  <span>{{ (configJson(p).etiquetaMin as string) ?? (configJson(p).min as number) ?? 1 }}</span>
                  <span class="font-bold text-foreground">{{ respuestas[p.id]?.valorNumero ?? '—' }}</span>
                  <span>{{ (configJson(p).etiquetaMax as string) ?? (configJson(p).max as number) ?? 10 }}</span>
                </div>
              </div>

              <!-- NPS (0-10) -->
              <div v-else-if="p.tipo === 'NPS'" class="space-y-3">
                <div class="flex gap-1 flex-wrap">
                  <button
                    v-for="n in 11" :key="n - 1"
                    type="button"
                    :class="[
                      'w-10 h-10 rounded-lg border text-sm font-medium transition-colors',
                      respuestas[p.id]?.valorNumero === n - 1
                        ? (n - 1 <= 6 ? 'border-red-500 bg-red-500 text-white' : n - 1 <= 8 ? 'border-amber-400 bg-amber-400 text-white' : 'border-emerald-500 bg-emerald-500 text-white')
                        : 'border-border hover:bg-accent',
                    ]"
                    @click="setNumero(p.id, n - 1)"
                  >{{ n - 1 }}</button>
                </div>
                <div class="flex justify-between text-xs text-muted-foreground">
                  <span>😞 Nada probable</span>
                  <span>😍 Muy probable</span>
                </div>
              </div>

              <!-- CALIFICACION (estrellas) -->
              <div v-else-if="p.tipo === 'CALIFICACION'" class="flex gap-1">
                <button
                  v-for="n in ((configJson(p).estrellas as number) ?? 5)"
                  :key="n"
                  type="button"
                  :class="[
                    'text-3xl transition-all hover:scale-110',
                    (respuestas[p.id]?.valorNumero ?? 0) >= n ? 'text-yellow-400' : 'text-muted-foreground/20',
                  ]"
                  @click="setNumero(p.id, n)"
                >★</button>
              </div>

              <!-- RANKING -->
              <div v-else-if="p.tipo === 'RANKING'" class="space-y-2">
                <p class="text-xs text-muted-foreground">Haz clic en las opciones en el orden de tu preferencia.</p>
                <div class="space-y-1.5">
                  <button
                    v-for="o in p.opciones" :key="o.id"
                    type="button"
                    :class="[
                      'w-full text-left rounded-lg border px-4 py-2.5 text-sm flex items-center gap-3 transition-colors',
                      getJsonArray(p.id).includes(o.valor) ? 'border-primary bg-primary/10' : 'border-border hover:bg-accent',
                    ]"
                    @click="toggleMultiple(p.id, o.valor)"
                  >
                    <span class="w-5 h-5 rounded-full border flex items-center justify-center text-xs font-bold shrink-0"
                      :class="getJsonArray(p.id).includes(o.valor) ? 'bg-primary text-primary-foreground border-primary' : 'border-muted-foreground'">
                      {{ getJsonArray(p.id).indexOf(o.valor) + 1 || '' }}
                    </span>
                    {{ o.etiqueta }}
                  </button>
                </div>
              </div>

              <!-- MATRIZ -->
              <div v-else-if="p.tipo === 'MATRIZ'" class="overflow-x-auto">
                <table class="w-full text-sm">
                  <thead>
                    <tr>
                      <th class="text-left pb-2 font-medium text-muted-foreground w-1/3"></th>
                      <th v-for="o in p.opciones" :key="o.id" class="text-center pb-2 font-medium px-2">{{ o.etiqueta }}</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(fila, fi) in (configJson(p).filas as string[])" :key="fi" class="border-t border-border">
                      <td class="py-2 pr-4 text-sm">{{ fila }}</td>
                      <td v-for="o in p.opciones" :key="o.id" class="text-center py-2 px-2">
                        <button
                          type="button"
                          :class="[
                            'w-5 h-5 rounded-full border mx-auto flex items-center justify-center transition-colors',
                            (() => { try { return JSON.parse(respuestas[p.id]?.valorJson ?? '{}')[fila] === o.valor } catch { return false } })()
                              ? 'bg-primary border-primary'
                              : 'border-muted-foreground hover:border-primary',
                          ]"
                          @click="() => {
                            let m: Record<string,string> = {}
                            try { m = JSON.parse(respuestas[p.id]?.valorJson ?? '{}') } catch {}
                            m[fila] = o.valor
                            respuestas[p.id] = { preguntaId: p.id, valorJson: JSON.stringify(m) }
                          }"
                        />
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>

            </div>
          </template>
        </template>

        <!-- Navegación entre secciones -->
        <div class="flex items-center justify-between pb-10">
          <AppButton
            v-if="seccionIdx > 0"
            variant="outline"
            @click="anterior"
          >
            <ChevronLeft class="h-4 w-4" />
            Anterior
          </AppButton>
          <div v-else />

          <AppButton
            v-if="!esUltimaSeccion"
            @click="siguiente"
            class="px-6"
          >
            Siguiente
            <ChevronRight class="h-4 w-4" />
          </AppButton>
          <AppButton
            v-else
            :loading="enviando"
            @click="enviar"
            class="px-8"
          >
            <Send class="h-4 w-4" />
            Enviar respuesta
          </AppButton>
        </div>

      </template>

    </div>
  </div>
</template>
