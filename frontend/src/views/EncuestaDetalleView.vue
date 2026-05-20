<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import http from '@/utils/http'
import type { Encuesta, Pregunta, Respuesta, OpcionPregunta, OpcionPreguntaRequest, AlcanceEncuesta, AlcanceEncuestaRequest, Entidad, SeccionEncuesta, SeccionEncuestaRequest, ReglaEncuesta, ReglaEncuestaRequest, ReglaJson, DimensionPregunta, DimensionPreguntaRequest } from '@/types/api'
import { useToast } from '@/composables/useToast'
import AppCard from '@/components/ui/AppCard.vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppBadge from '@/components/ui/AppBadge.vue'
import AppSpinner from '@/components/ui/AppSpinner.vue'
import AppModal from '@/components/ui/AppModal.vue'
import AppInput from '@/components/ui/AppInput.vue'
import { fmtFecha } from '@/utils/dates'
import useVuelidate from '@vuelidate/core'
import { required, minLength } from '@vuelidate/validators'
import {
  ArrowLeft, Plus, Edit, Trash2, GripVertical,
  AlignLeft, Hash, CheckSquare, List, BarChart2,
  Calendar, ToggleLeft, Star, TrendingUp, Layers,
  SlidersHorizontal, X, Check, Send, Lock, Link, Copy, BarChart,
  Target, ChevronDown, ChevronUp, LayoutList, Zap, BookOpen,
} from 'lucide-vue-next'

// ── tipos locales ─────────────────────────────────────────
interface PreguntaRequest {
  id?: string
  encuestaId: string
  seccionId?: string | null
  dimensionId?: string | null
  tipo: string
  titulo: string
  descripcion?: string
  orden: number
  peso: number
  esObligatoria: boolean
  configuracionJson?: string
}

const TIPOS = [
  { value: 'TEXTO',              label: 'Texto libre',          icon: AlignLeft   },
  { value: 'NUMERO',             label: 'Número',               icon: Hash        },
  { value: 'SELECCION_UNICA',    label: 'Selección única',      icon: CheckSquare },
  { value: 'SELECCION_MULTIPLE', label: 'Selección múltiple',   icon: List        },
  { value: 'ESCALA',             label: 'Escala',               icon: BarChart2   },
  { value: 'NPS',                label: 'NPS (0-10)',           icon: TrendingUp  },
  { value: 'CALIFICACION',       label: 'Calificación',         icon: Star        },
  { value: 'FECHA',              label: 'Fecha',                icon: Calendar    },
  { value: 'BOOLEANO',           label: 'Sí / No',             icon: ToggleLeft  },
  { value: 'RANKING',            label: 'Ranking',              icon: Layers      },
  { value: 'MATRIZ',             label: 'Matriz',               icon: Layers      },
]

const TIPOS_CON_OPCIONES = new Set(['SELECCION_UNICA', 'SELECCION_MULTIPLE', 'RANKING', 'MATRIZ'])

const tipoIcon  = (tipo: string) => TIPOS.find(t => t.value === tipo)?.icon ?? AlignLeft
const tipoLabel = (tipo: string) => TIPOS.find(t => t.value === tipo)?.label ?? tipo

// ── estado ────────────────────────────────────────────────
const route  = useRoute()
const router = useRouter()
const { toast } = useToast()

const encuesta    = ref<Encuesta | null>(null)
const preguntas   = ref<Pregunta[]>([])
const loadingEnc  = ref(true)
const loadingPreg = ref(true)
const modalOpen   = ref(false)
const editTarget  = ref<Pregunta | null>(null)
const saving      = ref(false)
const deletingId  = ref<string | null>(null)

const emptyForm = (): PreguntaRequest => ({
  encuestaId:    route.params.id as string,
  seccionId:     null,
  dimensionId:   null,
  tipo:          'TEXTO',
  titulo:        '',
  descripcion:   '',
  orden:         preguntas.value.length + 1,
  peso:          1,
  esObligatoria: false,
})

const form = ref<PreguntaRequest>(emptyForm())

const rules = computed(() => ({
  titulo: { required, minLength: minLength(3) },
  tipo:   { required },
}))
const v$ = useVuelidate(rules, form)

// ── alcance ───────────────────────────────────────────────
const alcances          = ref<AlcanceEncuesta[]>([])
const entidades         = ref<Entidad[]>([])
const alcanceOpen       = ref(false)
const alcanceModalOpen  = ref(false)
const loadingAlcance    = ref(false)
const savingAlcance     = ref(false)
const deletingAlcanceId = ref<string | null>(null)

const emptyAlcanceForm = (): AlcanceEncuestaRequest => ({
  encuestaId:           route.params.id as string,
  entidadId:            '',
  tipoRelacion:         'AMBITO',
  incluirDescendientes: false,
})
const alcanceForm = ref<AlcanceEncuestaRequest>(emptyAlcanceForm())

const TIPO_RELACION_LABELS: Record<string, string> = {
  AMBITO:   'Ámbito — delimita quiénes responden',
  SUJETO:   'Sujeto — entidad que se evalúa',
  CONTEXTO: 'Contexto — referencia sin ser evaluado',
}

async function cargarAlcance() {
  if (alcances.value.length > 0) return
  loadingAlcance.value = true
  try {
    const [alcRes, entRes] = await Promise.all([
      http.get<Respuesta<AlcanceEncuesta[]>>(`/alcanceencuesta/${route.params.id}`),
      http.get<Respuesta<Entidad[]>>('/entidad'),
    ])
    if (alcRes.data.ok) alcances.value = alcRes.data.datos
    if (entRes.data.ok) entidades.value = entRes.data.datos.filter(e => e.esActivo)
  } finally {
    loadingAlcance.value = false
  }
}

function toggleAlcancePanel() {
  alcanceOpen.value = !alcanceOpen.value
  if (alcanceOpen.value) cargarAlcance()
}

function abrirAlcanceModal() {
  alcanceForm.value = emptyAlcanceForm()
  alcanceModalOpen.value = true
}

async function guardarAlcance() {
  if (!alcanceForm.value.entidadId) return
  savingAlcance.value = true
  try {
    await http.post('/alcanceencuesta', alcanceForm.value)
    toast.success('Alcance agregado')
    alcances.value = []
    await cargarAlcance()
    alcanceModalOpen.value = false
  } catch {
    toast.error('No se pudo guardar el alcance')
  } finally {
    savingAlcance.value = false
  }
}

async function eliminarAlcance(a: AlcanceEncuesta) {
  deletingAlcanceId.value = a.id
  try {
    await http.delete(`/alcanceencuesta/${a.encuestaId}/${a.id}`)
    toast.success('Alcance eliminado')
    alcances.value = alcances.value.filter(x => x.id !== a.id)
  } catch {
    toast.error('No se pudo eliminar')
  } finally {
    deletingAlcanceId.value = null
  }
}

function nombreEntidad(id: string) {
  return entidades.value.find(e => e.id === id)?.nombreVisible ?? id
}

// ── secciones ────────────────────────────────────────────
const secciones         = ref<SeccionEncuesta[]>([])
const seccionOpen       = ref(false)
const seccionModalOpen  = ref(false)
const loadingSecciones  = ref(false)
const savingSeccion     = ref(false)
const deletingSeccionId = ref<string | null>(null)
const editSeccion       = ref<SeccionEncuesta | null>(null)

const emptySeccionForm = (): SeccionEncuestaRequest => ({
  encuestaId: route.params.id as string,
  titulo:     '',
  descripcion: '',
  orden:      secciones.value.length + 1,
})
const seccionForm = ref<SeccionEncuestaRequest>(emptySeccionForm())

async function cargarSecciones() {
  if (secciones.value.length > 0 && !seccionOpen.value) return
  loadingSecciones.value = true
  try {
    const { data } = await http.get<Respuesta<SeccionEncuesta[]>>(`/seccionencuesta/${route.params.id}`)
    if (data.ok) secciones.value = data.datos
  } finally {
    loadingSecciones.value = false
  }
}

function toggleSeccionPanel() {
  seccionOpen.value = !seccionOpen.value
  if (seccionOpen.value) cargarSecciones()
}

function abrirCrearSeccion() {
  editSeccion.value = null
  seccionForm.value = emptySeccionForm()
  seccionModalOpen.value = true
}

function abrirEditarSeccion(s: SeccionEncuesta) {
  editSeccion.value = s
  seccionForm.value = {
    id:          s.id,
    encuestaId:  s.encuestaId,
    titulo:      s.titulo,
    descripcion: s.descripcion ?? '',
    orden:       s.orden,
  }
  seccionModalOpen.value = true
}

async function guardarSeccion() {
  if (!seccionForm.value.titulo.trim()) return
  savingSeccion.value = true
  try {
    if (editSeccion.value) {
      await http.put('/seccionencuesta', seccionForm.value)
      toast.success('Sección actualizada')
    } else {
      await http.post('/seccionencuesta', seccionForm.value)
      toast.success('Sección creada')
    }
    secciones.value = []
    await cargarSecciones()
    seccionModalOpen.value = false
  } catch {
    toast.error('No se pudo guardar la sección')
  } finally {
    savingSeccion.value = false
  }
}

async function eliminarSeccion(s: SeccionEncuesta) {
  if (!confirm(`¿Eliminar la sección "${s.titulo}"? Las preguntas asignadas quedarán sin sección.`)) return
  deletingSeccionId.value = s.id
  try {
    await http.delete(`/seccionencuesta/${s.encuestaId}/${s.id}`)
    toast.success('Sección eliminada')
    secciones.value = secciones.value.filter(x => x.id !== s.id)
    preguntas.value = preguntas.value.map(p => p.seccionId === s.id ? { ...p, seccionId: undefined } : p)
  } catch {
    toast.error('No se pudo eliminar')
  } finally {
    deletingSeccionId.value = null
  }
}

function nombreSeccion(id?: string | null) {
  if (!id) return null
  return secciones.value.find(s => s.id === id)?.titulo ?? null
}

// ── dimensiones ───────────────────────────────────────────
const dimensiones         = ref<DimensionPregunta[]>([])
const dimensionOpen       = ref(false)
const dimensionModalOpen  = ref(false)
const loadingDimensiones  = ref(false)
const savingDimension     = ref(false)
const deletingDimensionId = ref<string | null>(null)
const editDimension       = ref<DimensionPregunta | null>(null)

const emptyDimensionForm = (): DimensionPreguntaRequest => ({
  encuestaId:  route.params.id as string,
  nombre:      '',
  descripcion: '',
  peso:        1,
  orden:       dimensiones.value.length + 1,
})
const dimensionForm = ref<DimensionPreguntaRequest>(emptyDimensionForm())

async function cargarDimensiones() {
  loadingDimensiones.value = true
  try {
    const { data } = await http.get<Respuesta<DimensionPregunta[]>>(`/dimensionpregunta/${route.params.id}`)
    if (data.ok) dimensiones.value = data.datos
  } finally {
    loadingDimensiones.value = false
  }
}

function toggleDimensionPanel() {
  dimensionOpen.value = !dimensionOpen.value
  if (dimensionOpen.value && dimensiones.value.length === 0) cargarDimensiones()
}

function abrirCrearDimension() {
  editDimension.value = null
  dimensionForm.value = emptyDimensionForm()
  dimensionModalOpen.value = true
}

function abrirEditarDimension(d: DimensionPregunta) {
  editDimension.value = d
  dimensionForm.value = {
    id:          d.id,
    encuestaId:  d.encuestaId,
    nombre:      d.nombre,
    descripcion: d.descripcion ?? '',
    peso:        d.peso,
    orden:       d.orden,
  }
  dimensionModalOpen.value = true
}

async function guardarDimension() {
  if (!dimensionForm.value.nombre.trim()) return
  savingDimension.value = true
  try {
    if (editDimension.value) {
      await http.put('/dimensionpregunta', dimensionForm.value)
      toast.success('Dimensión actualizada')
    } else {
      await http.post('/dimensionpregunta', dimensionForm.value)
      toast.success('Dimensión creada')
    }
    dimensiones.value = []
    await cargarDimensiones()
    dimensionModalOpen.value = false
  } catch {
    toast.error('No se pudo guardar la dimensión')
  } finally {
    savingDimension.value = false
  }
}

async function eliminarDimension(d: DimensionPregunta) {
  if (!confirm(`¿Eliminar la dimensión "${d.nombre}"? Las preguntas asignadas quedarán sin dimensión.`)) return
  deletingDimensionId.value = d.id
  try {
    await http.delete(`/dimensionpregunta/${d.encuestaId}/${d.id}`)
    toast.success('Dimensión eliminada')
    dimensiones.value = dimensiones.value.filter(x => x.id !== d.id)
    preguntas.value = preguntas.value.map(p => p.dimensionId === d.id ? { ...p, dimensionId: undefined } : p)
  } catch {
    toast.error('No se pudo eliminar')
  } finally {
    deletingDimensionId.value = null
  }
}

function nombreDimension(id?: string | null) {
  if (!id) return null
  return dimensiones.value.find(d => d.id === id)?.nombre ?? null
}

const pesoTotalDimensiones = computed(() =>
  dimensiones.value.reduce((sum, d) => sum + Number(d.peso), 0)
)

// ── reglas condicionales ──────────────────────────────────
const OPERADORES = [
  { value: 'igual',    label: 'es igual a' },
  { value: 'distinto', label: 'es distinto de' },
  { value: 'mayor',    label: 'es mayor que' },
  { value: 'menor',    label: 'es menor que' },
  { value: 'contiene', label: 'contiene' },
] as const

const ACCIONES = [
  { value: 'mostrar', label: 'Mostrar' },
  { value: 'ocultar', label: 'Ocultar' },
  { value: 'saltar',  label: 'Saltar a' },
] as const

const reglas          = ref<ReglaEncuesta[]>([])
const reglaOpen       = ref(false)
const reglaModalOpen  = ref(false)
const loadingReglas   = ref(false)
const savingRegla     = ref(false)
const deletingReglaId = ref<string | null>(null)
const editRegla       = ref<ReglaEncuesta | null>(null)

const opcionesCondicion = ref<OpcionPregunta[]>([])
const loadingOpcionesCondicion = ref(false)

const emptyReglaForm = (): ReglaJson => ({
  si:      { preguntaId: '', operador: 'igual', valor: '' },
  entonces: { accion: 'mostrar', preguntaObjetivoId: '', seccionObjetivoId: '' },
})
const reglaForm = ref<ReglaJson>(emptyReglaForm())

async function cargarReglas() {
  loadingReglas.value = true
  try {
    const { data } = await http.get<Respuesta<ReglaEncuesta[]>>(`/reglaencuesta/${route.params.id}`)
    if (data.ok) reglas.value = data.datos
  } finally {
    loadingReglas.value = false
  }
}

function toggleReglaPanel() {
  reglaOpen.value = !reglaOpen.value
  if (reglaOpen.value && reglas.value.length === 0) cargarReglas()
}

function abrirCrearRegla() {
  editRegla.value = null
  reglaForm.value = emptyReglaForm()
  opcionesCondicion.value = []
  reglaModalOpen.value = true
}

function abrirEditarRegla(r: ReglaEncuesta) {
  editRegla.value = r
  try {
    reglaForm.value = JSON.parse(r.reglaJson) as ReglaJson
    // ensure empty strings for optional fields
    reglaForm.value.entonces.preguntaObjetivoId ??= ''
    reglaForm.value.entonces.seccionObjetivoId  ??= ''
  } catch {
    reglaForm.value = emptyReglaForm()
  }
  onCondicionPreguntaChange()
  reglaModalOpen.value = true
}

async function onCondicionPreguntaChange() {
  const pid = reglaForm.value.si.preguntaId
  if (!pid) { opcionesCondicion.value = []; return }
  const pregunta = preguntas.value.find(p => p.id === pid)
  if (!pregunta || !TIPOS_CON_OPCIONES.has(pregunta.tipo)) {
    opcionesCondicion.value = []
    return
  }
  loadingOpcionesCondicion.value = true
  try {
    const { data } = await http.get<Respuesta<OpcionPregunta[]>>(`/opcionpregunta/${pid}`)
    if (data.ok) opcionesCondicion.value = data.datos
  } finally {
    loadingOpcionesCondicion.value = false
  }
}

async function guardarRegla() {
  const { si, entonces } = reglaForm.value
  if (!si.preguntaId || !si.valor.trim()) return
  if (entonces.accion !== 'saltar' && !entonces.preguntaObjetivoId) return
  if (entonces.accion === 'saltar' && !entonces.preguntaObjetivoId && !entonces.seccionObjetivoId) return

  const payload: ReglaEncuestaRequest = {
    encuestaId: route.params.id as string,
    reglaJson: JSON.stringify({
      si,
      entonces: {
        accion: entonces.accion,
        ...(entonces.preguntaObjetivoId ? { preguntaObjetivoId: entonces.preguntaObjetivoId } : {}),
        ...(entonces.seccionObjetivoId  ? { seccionObjetivoId:  entonces.seccionObjetivoId  } : {}),
      },
    }),
  }
  if (editRegla.value) payload.id = editRegla.value.id

  savingRegla.value = true
  try {
    if (editRegla.value) {
      await http.put('/reglaencuesta', payload)
      toast.success('Regla actualizada')
    } else {
      await http.post('/reglaencuesta', payload)
      toast.success('Regla creada')
    }
    reglas.value = []
    await cargarReglas()
    reglaModalOpen.value = false
  } catch {
    toast.error('No se pudo guardar la regla')
  } finally {
    savingRegla.value = false
  }
}

async function eliminarRegla(r: ReglaEncuesta) {
  if (!confirm('¿Eliminar esta regla condicional?')) return
  deletingReglaId.value = r.id
  try {
    await http.delete(`/reglaencuesta/${r.encuestaId}/${r.id}`)
    toast.success('Regla eliminada')
    reglas.value = reglas.value.filter(x => x.id !== r.id)
  } catch {
    toast.error('No se pudo eliminar')
  } finally {
    deletingReglaId.value = null
  }
}

function reglaLabel(r: ReglaEncuesta): string {
  try {
    const { si, entonces } = JSON.parse(r.reglaJson) as ReglaJson
    const pregSi    = preguntas.value.find(p => p.id === si.preguntaId)?.titulo ?? si.preguntaId
    const op        = OPERADORES.find(o => o.value === si.operador)?.label ?? si.operador
    const accion    = ACCIONES.find(a => a.value === entonces.accion)?.label ?? entonces.accion
    const pregObj   = entonces.preguntaObjetivoId
      ? (preguntas.value.find(p => p.id === entonces.preguntaObjetivoId)?.titulo ?? '…')
      : null
    const secObj    = entonces.seccionObjetivoId
      ? (secciones.value.find(s => s.id === entonces.seccionObjetivoId)?.titulo ?? '…')
      : null
    const objetivo  = pregObj ?? secObj ?? '…'
    return `Si "${pregSi}" ${op} "${si.valor}" → ${accion} "${objetivo}"`
  } catch {
    return r.reglaJson
  }
}

// ── enlace público ────────────────────────────────────────
const generandoEnlace  = ref(false)
const enlacePublico    = ref<string | null>(null)

async function generarEnlace() {
  generandoEnlace.value = true
  try {
    const { data } = await http.post<Respuesta<string>>('/invitacion', {
      encuestaId: route.params.id,
      canal: 'ENLACE_PUBLICO',
    })
    if (data.ok) {
      enlacePublico.value = `${window.location.origin}/responder/${data.datos}`
      toast.success('Enlace generado')
    } else {
      toast.error(data.mensaje?.descripcion ?? 'No se pudo generar el enlace')
    }
  } catch {
    toast.error('Error al generar el enlace')
  } finally {
    generandoEnlace.value = false
  }
}

async function copiarEnlace() {
  if (!enlacePublico.value) return
  await navigator.clipboard.writeText(enlacePublico.value)
  toast.success('Enlace copiado')
}

// ── cambiar estado ────────────────────────────────────────
const cambiandoEstado = ref(false)

async function cambiarEstado(nuevoEstado: 'PUBLICADA' | 'CERRADA') {
  const accion = nuevoEstado === 'PUBLICADA' ? 'publicar' : 'cerrar'
  if (!confirm(`¿Seguro que deseas ${accion} esta encuesta?`)) return
  cambiandoEstado.value = true
  try {
    const { data } = await http.patch<Respuesta<boolean>>(
      `/encuesta/${route.params.id}/estado`,
      { nuevoEstado }
    )
    if (data.ok) {
      toast.success(nuevoEstado === 'PUBLICADA' ? 'Encuesta publicada' : 'Encuesta cerrada')
      await cargarEncuesta()
    } else {
      toast.error(data.mensaje?.descripcion ?? 'No se pudo cambiar el estado')
    }
  } catch {
    toast.error('Error al cambiar el estado')
  } finally {
    cambiandoEstado.value = false
  }
}

// ── estado opciones ───────────────────────────────────────
const opcionModalOpen        = ref(false)
const opcionTargetPregunta   = ref<Pregunta | null>(null)
const opciones               = ref<OpcionPregunta[]>([])
const loadingOpciones        = ref(false)
const savingOpcion           = ref(false)
const deletingOpcionId       = ref<string | null>(null)
const editingOpcion          = ref<OpcionPregunta | null>(null)

const emptyOpcionForm = (preguntaId: string): OpcionPreguntaRequest => ({
  preguntaId,
  etiqueta: '',
  valor:    '',
  puntaje:  undefined,
  orden:    opciones.value.length + 1,
})

const opcionForm = ref<OpcionPreguntaRequest>({ preguntaId: '', etiqueta: '', valor: '', orden: 1 })

// ── carga ─────────────────────────────────────────────────
async function cargarEncuesta() {
  try {
    const { data } = await http.get<Respuesta<Encuesta>>(`/encuesta/${route.params.id}`)
    if (data.ok) encuesta.value = data.datos
  } finally {
    loadingEnc.value = false
  }
}

async function cargarPreguntas() {
  loadingPreg.value = true
  try {
    const { data } = await http.get<Respuesta<Pregunta[]>>(`/pregunta/${route.params.id}`)
    if (data.ok) preguntas.value = data.datos
  } finally {
    loadingPreg.value = false
  }
}

async function cargarOpciones(preguntaId: string) {
  loadingOpciones.value = true
  try {
    const { data } = await http.get<Respuesta<OpcionPregunta[]>>(`/opcionpregunta/${preguntaId}`)
    if (data.ok) opciones.value = data.datos
  } finally {
    loadingOpciones.value = false
  }
}

// ── modal pregunta ────────────────────────────────────────
function abrirCrear() {
  editTarget.value = null
  form.value = emptyForm()
  v$.value.$reset()
  modalOpen.value = true
}

function abrirEditar(p: Pregunta) {
  editTarget.value = p
  form.value = {
    id:            p.id,
    encuestaId:    p.encuestaId,
    seccionId:     p.seccionId ?? null,
    dimensionId:   p.dimensionId ?? null,
    tipo:          p.tipo,
    titulo:        p.titulo,
    descripcion:   p.descripcion ?? '',
    orden:         p.orden,
    peso:          p.peso,
    esObligatoria: p.esObligatoria,
  }
  v$.value.$reset()
  modalOpen.value = true
}

async function guardar() {
  const valid = await v$.value.$validate()
  if (!valid) return
  saving.value = true
  try {
    if (editTarget.value) {
      await http.put('/pregunta', form.value)
      toast.success('Pregunta actualizada')
    } else {
      await http.post('/pregunta', form.value)
      toast.success('Pregunta creada')
    }
    modalOpen.value = false
    await cargarPreguntas()
  } catch {
    toast.error('No se pudo guardar la pregunta')
  } finally {
    saving.value = false
  }
}

async function eliminar(p: Pregunta) {
  if (!confirm(`¿Eliminar "${p.titulo}"?`)) return
  deletingId.value = p.id
  try {
    await http.delete(`/pregunta/${p.encuestaId}/${p.id}`)
    toast.success('Pregunta eliminada')
    await cargarPreguntas()
  } catch {
    toast.error('No se pudo eliminar')
  } finally {
    deletingId.value = null
  }
}

// ── modal opciones ────────────────────────────────────────
async function abrirOpciones(p: Pregunta) {
  opcionTargetPregunta.value = p
  opcionForm.value = emptyOpcionForm(p.id)
  editingOpcion.value = null
  opcionModalOpen.value = true
  await cargarOpciones(p.id)
}

function iniciarEditOpcion(o: OpcionPregunta) {
  editingOpcion.value = o
  opcionForm.value = {
    id:         o.id,
    preguntaId: o.preguntaId,
    etiqueta:   o.etiqueta,
    valor:      o.valor,
    puntaje:    o.puntaje,
    orden:      o.orden,
  }
}

function cancelarEditOpcion() {
  editingOpcion.value = null
  if (opcionTargetPregunta.value) {
    opcionForm.value = emptyOpcionForm(opcionTargetPregunta.value.id)
  }
}

async function guardarOpcion() {
  if (!opcionForm.value.etiqueta.trim()) return
  if (!opcionForm.value.valor.trim()) {
    opcionForm.value.valor = opcionForm.value.etiqueta
  }
  savingOpcion.value = true
  try {
    if (editingOpcion.value) {
      await http.put('/opcionpregunta', opcionForm.value)
      toast.success('Opción actualizada')
    } else {
      await http.post('/opcionpregunta', opcionForm.value)
      toast.success('Opción creada')
    }
    editingOpcion.value = null
    if (opcionTargetPregunta.value) {
      opcionForm.value = emptyOpcionForm(opcionTargetPregunta.value.id)
      await cargarOpciones(opcionTargetPregunta.value.id)
    }
  } catch {
    toast.error('No se pudo guardar la opción')
  } finally {
    savingOpcion.value = false
  }
}

async function eliminarOpcion(o: OpcionPregunta) {
  if (!confirm(`¿Eliminar la opción "${o.etiqueta}"?`)) return
  deletingOpcionId.value = o.id
  try {
    await http.delete(`/opcionpregunta/${o.preguntaId}/${o.id}`)
    toast.success('Opción eliminada')
    if (opcionTargetPregunta.value) {
      await cargarOpciones(opcionTargetPregunta.value.id)
    }
  } catch {
    toast.error('No se pudo eliminar la opción')
  } finally {
    deletingOpcionId.value = null
  }
}

// ── helpers ───────────────────────────────────────────────
const estadoBadge: Record<string, 'default' | 'success' | 'warning'> = {
  BORRADOR: 'warning', PUBLICADA: 'success', CERRADA: 'default',
}

const fmt = fmtFecha

onMounted(() => {
  cargarEncuesta()
  cargarPreguntas()
  cargarSecciones()
  cargarDimensiones()
  cargarReglas()
})
</script>

<template>
  <div class="space-y-6">

    <!-- Cabecera -->
    <div class="flex items-center gap-3">
      <AppButton variant="ghost" size="icon" @click="router.back()">
        <ArrowLeft class="h-5 w-5" />
      </AppButton>
      <div>
        <h2 class="text-xl font-semibold">{{ encuesta?.titulo ?? '...' }}</h2>
        <p class="text-sm text-muted-foreground">Detalle de encuesta</p>
      </div>
    </div>

    <div v-if="loadingEnc" class="flex justify-center py-8"><AppSpinner size="lg" /></div>

    <template v-else-if="encuesta">
      <!-- Info general -->
      <AppCard class="p-5">
        <div class="flex flex-wrap items-start justify-between gap-4">
          <div class="space-y-1">
            <p v-if="encuesta.descripcion" class="text-sm text-muted-foreground">{{ encuesta.descripcion }}</p>
            <div class="flex flex-wrap gap-2 mt-2">
              <AppBadge :variant="estadoBadge[encuesta.estado]">{{ encuesta.estado }}</AppBadge>
              <AppBadge v-if="encuesta.esGlobal" variant="secondary">Global</AppBadge>
              <AppBadge v-if="encuesta.esPlantilla" variant="secondary">Plantilla</AppBadge>
              <AppBadge variant="outline">v{{ encuesta.version }}</AppBadge>
            </div>
          </div>
          <div class="flex flex-col items-end gap-3">
            <div class="text-xs text-muted-foreground space-y-0.5 text-right">
              <p>Inicio: <span class="text-foreground font-medium">{{ fmt(encuesta.fechaInicio) }}</span></p>
              <p>Fin: <span class="text-foreground font-medium">{{ fmt(encuesta.fechaFin) }}</span></p>
              <p>Creado: <span class="text-foreground font-medium">{{ fmt(encuesta.creadoEn) }}</span></p>
            </div>
            <!-- Acciones de estado -->
            <AppButton
              v-if="encuesta.estado === 'BORRADOR'"
              :loading="cambiandoEstado"
              @click="cambiarEstado('PUBLICADA')"
            >
              <Send class="h-4 w-4" />
              Publicar encuesta
            </AppButton>
            <AppButton
              v-if="encuesta.estado === 'PUBLICADA'"
              variant="outline"
              :loading="cambiandoEstado"
              @click="cambiarEstado('CERRADA')"
            >
              <Lock class="h-4 w-4" />
              Cerrar encuesta
            </AppButton>
            <AppBadge v-if="encuesta.estado === 'CERRADA'" variant="default">Encuesta cerrada</AppBadge>
            <div class="flex gap-2">
              <router-link :to="`/encuestas/${encuesta.id}/invitaciones`">
                <AppButton variant="outline" size="sm">
                  <Link class="h-4 w-4" />
                  Invitaciones
                </AppButton>
              </router-link>
              <router-link v-if="encuesta.estado !== 'BORRADOR'" :to="`/encuestas/${encuesta.id}/resultados`">
                <AppButton variant="outline" size="sm">
                  <BarChart class="h-4 w-4" />
                  Ver respuestas
                </AppButton>
              </router-link>
            </div>
          </div>
        </div>
      </AppCard>

      <!-- Alcance -->
      <AppCard class="overflow-hidden">
        <!-- Header colapsable -->
        <button
          class="w-full flex items-center justify-between px-5 py-4 hover:bg-muted/40 transition-colors"
          @click="toggleAlcancePanel"
        >
          <div class="flex items-center gap-2 text-sm font-medium">
            <Target class="h-4 w-4 text-primary" />
            Alcance
            <span class="text-muted-foreground font-normal">
              — {{ encuesta.esGlobal ? 'toda la organización' : alcances.length > 0 ? `${alcances.length} entidad(es)` : 'no definido' }}
            </span>
          </div>
          <component :is="alcanceOpen ? ChevronUp : ChevronDown" class="h-4 w-4 text-muted-foreground" />
        </button>

        <!-- Panel expandido -->
        <div v-if="alcanceOpen" class="border-t border-border px-5 py-4 space-y-4">

          <!-- Encuesta global -->
          <div v-if="encuesta.esGlobal" class="flex items-center gap-2 text-sm text-muted-foreground">
            <span class="inline-block w-2 h-2 rounded-full bg-emerald-500" />
            Esta encuesta es <strong class="text-foreground">global</strong> — aplica a toda la organización sin restricción de alcance.
          </div>

          <!-- Alcance específico -->
          <template v-else>
            <div v-if="loadingAlcance" class="flex justify-center py-4"><AppSpinner /></div>

            <div v-else-if="alcances.length === 0" class="text-sm text-muted-foreground">
              Sin alcance definido. La encuesta no tiene destinatarios específicos.
            </div>

            <div v-else class="space-y-2">
              <div
                v-for="a in alcances" :key="a.id"
                class="flex items-center justify-between gap-3 rounded-lg border border-border px-3 py-2.5"
              >
                <div class="flex-1 min-w-0">
                  <p class="text-sm font-medium">{{ nombreEntidad(a.entidadId) }}</p>
                  <p class="text-xs text-muted-foreground mt-0.5">
                    {{ TIPO_RELACION_LABELS[a.tipoRelacion] }}
                    <span v-if="a.incluirDescendientes"> · Incluye descendientes</span>
                  </p>
                </div>
                <AppButton
                  variant="ghost" size="icon"
                  class="text-destructive hover:text-destructive h-7 w-7 shrink-0"
                  :loading="deletingAlcanceId === a.id"
                  @click="eliminarAlcance(a)"
                >
                  <Trash2 class="h-3.5 w-3.5" />
                </AppButton>
              </div>
            </div>

            <AppButton size="sm" variant="outline" @click="abrirAlcanceModal">
              <Plus class="h-3.5 w-3.5" />
              Agregar entidad al alcance
            </AppButton>
          </template>
        </div>
      </AppCard>

      <!-- Secciones -->
      <AppCard class="overflow-hidden">
        <button
          class="w-full flex items-center justify-between px-5 py-4 hover:bg-muted/40 transition-colors"
          @click="toggleSeccionPanel"
        >
          <div class="flex items-center gap-2 text-sm font-medium">
            <LayoutList class="h-4 w-4 text-primary" />
            Secciones
            <span class="text-muted-foreground font-normal">
              — {{ secciones.length === 0 ? 'sin secciones' : `${secciones.length} sección(es)` }}
            </span>
          </div>
          <component :is="seccionOpen ? ChevronUp : ChevronDown" class="h-4 w-4 text-muted-foreground" />
        </button>

        <div v-if="seccionOpen" class="border-t border-border px-5 py-4 space-y-3">
          <div v-if="loadingSecciones" class="flex justify-center py-4"><AppSpinner /></div>

          <div v-else-if="secciones.length === 0" class="text-sm text-muted-foreground">
            Sin secciones. Las preguntas se muestran en una única página.
          </div>

          <div v-else class="space-y-2">
            <div
              v-for="s in [...secciones].sort((a,b) => a.orden - b.orden)"
              :key="s.id"
              class="flex items-center justify-between gap-3 rounded-lg border border-border px-3 py-2.5"
            >
              <div class="flex items-center gap-3 min-w-0">
                <span class="text-xs font-bold text-muted-foreground w-5 text-center shrink-0">{{ s.orden }}</span>
                <div class="min-w-0">
                  <p class="text-sm font-medium truncate">{{ s.titulo }}</p>
                  <p v-if="s.descripcion" class="text-xs text-muted-foreground truncate">{{ s.descripcion }}</p>
                  <p class="text-xs text-muted-foreground">
                    {{ preguntas.filter(p => p.seccionId === s.id).length }} pregunta(s)
                  </p>
                </div>
              </div>
              <div class="flex items-center gap-1 shrink-0">
                <AppButton variant="ghost" size="icon" class="h-7 w-7" title="Editar" @click="abrirEditarSeccion(s)">
                  <Edit class="h-3.5 w-3.5" />
                </AppButton>
                <AppButton
                  variant="ghost" size="icon"
                  class="h-7 w-7 text-destructive hover:text-destructive"
                  title="Eliminar"
                  :loading="deletingSeccionId === s.id"
                  @click="eliminarSeccion(s)"
                >
                  <Trash2 class="h-3.5 w-3.5" />
                </AppButton>
              </div>
            </div>
          </div>

          <AppButton size="sm" variant="outline" @click="abrirCrearSeccion">
            <Plus class="h-3.5 w-3.5" />
            Nueva sección
          </AppButton>
        </div>
      </AppCard>

      <!-- Dimensiones -->
      <AppCard class="overflow-hidden">
        <button
          class="w-full flex items-center justify-between px-5 py-4 hover:bg-muted/40 transition-colors"
          @click="toggleDimensionPanel"
        >
          <div class="flex items-center gap-2 text-sm font-medium">
            <BookOpen class="h-4 w-4 text-primary" />
            Dimensiones
            <span class="text-muted-foreground font-normal">
              — {{ dimensiones.length === 0 ? 'sin dimensiones' : `${dimensiones.length} dimensión(es)` }}
            </span>
          </div>
          <component :is="dimensionOpen ? ChevronUp : ChevronDown" class="h-4 w-4 text-muted-foreground" />
        </button>

        <div v-if="dimensionOpen" class="border-t border-border px-5 py-4 space-y-3">
          <div v-if="loadingDimensiones" class="flex justify-center py-4"><AppSpinner /></div>

          <div v-else-if="dimensiones.length === 0" class="text-sm text-muted-foreground">
            Sin dimensiones. Todas las preguntas contribuyen al mismo resultado global.
          </div>

          <template v-else>
            <!-- Indicador de pesos -->
            <div class="flex items-center gap-2 text-xs text-muted-foreground">
              <span>Peso total:</span>
              <span :class="['font-bold', Math.abs(pesoTotalDimensiones - 1) < 0.01 ? 'text-emerald-500' : 'text-amber-500']">
                {{ (pesoTotalDimensiones * 100).toFixed(0) }}%
              </span>
              <span v-if="Math.abs(pesoTotalDimensiones - 1) > 0.01" class="text-amber-500">
                (debería sumar 100%)
              </span>
            </div>

            <!-- Lista de dimensiones -->
            <div class="space-y-2">
              <div
                v-for="d in [...dimensiones].sort((a,b) => a.orden - b.orden)"
                :key="d.id"
                class="flex items-center justify-between gap-3 rounded-lg border border-border px-3 py-2.5"
              >
                <div class="flex items-center gap-3 min-w-0 flex-1">
                  <span class="text-xs font-bold text-muted-foreground w-5 text-center shrink-0">{{ d.orden }}</span>
                  <div class="min-w-0 flex-1">
                    <div class="flex items-center gap-2 flex-wrap">
                      <p class="text-sm font-medium">{{ d.nombre }}</p>
                      <span class="text-xs rounded-full bg-primary/10 text-primary px-2 py-0.5 font-medium">
                        {{ (Number(d.peso) * 100).toFixed(0) }}%
                      </span>
                    </div>
                    <p v-if="d.descripcion" class="text-xs text-muted-foreground truncate">{{ d.descripcion }}</p>
                    <p class="text-xs text-muted-foreground">
                      {{ preguntas.filter(p => p.dimensionId === d.id).length }} pregunta(s)
                    </p>
                  </div>
                  <!-- Mini barra de peso -->
                  <div class="w-20 shrink-0">
                    <div class="h-1.5 rounded-full bg-muted overflow-hidden">
                      <div
                        class="h-full bg-primary rounded-full"
                        :style="{ width: `${Math.min(Number(d.peso) * 100, 100)}%` }"
                      />
                    </div>
                  </div>
                </div>
                <div class="flex items-center gap-1 shrink-0">
                  <AppButton variant="ghost" size="icon" class="h-7 w-7" title="Editar" @click="abrirEditarDimension(d)">
                    <Edit class="h-3.5 w-3.5" />
                  </AppButton>
                  <AppButton
                    variant="ghost" size="icon"
                    class="h-7 w-7 text-destructive hover:text-destructive"
                    title="Eliminar"
                    :loading="deletingDimensionId === d.id"
                    @click="eliminarDimension(d)"
                  >
                    <Trash2 class="h-3.5 w-3.5" />
                  </AppButton>
                </div>
              </div>
            </div>
          </template>

          <AppButton size="sm" variant="outline" @click="abrirCrearDimension">
            <Plus class="h-3.5 w-3.5" />
            Nueva dimensión
          </AppButton>
        </div>
      </AppCard>

      <!-- Reglas condicionales -->
      <AppCard class="overflow-hidden">
        <button
          class="w-full flex items-center justify-between px-5 py-4 hover:bg-muted/40 transition-colors"
          @click="toggleReglaPanel"
        >
          <div class="flex items-center gap-2 text-sm font-medium">
            <Zap class="h-4 w-4 text-primary" />
            Lógica condicional
            <span class="text-muted-foreground font-normal">
              — {{ reglas.length === 0 ? 'sin reglas' : `${reglas.length} regla(s)` }}
            </span>
          </div>
          <component :is="reglaOpen ? ChevronUp : ChevronDown" class="h-4 w-4 text-muted-foreground" />
        </button>

        <div v-if="reglaOpen" class="border-t border-border px-5 py-4 space-y-3">
          <div v-if="loadingReglas" class="flex justify-center py-4"><AppSpinner /></div>

          <div v-else-if="reglas.length === 0" class="text-sm text-muted-foreground">
            Sin reglas. Las preguntas siempre se muestran.
          </div>

          <div v-else class="space-y-2">
            <div
              v-for="r in reglas"
              :key="r.id"
              class="flex items-start justify-between gap-3 rounded-lg border border-border px-3 py-2.5"
            >
              <div class="flex items-start gap-2 min-w-0">
                <Zap class="h-3.5 w-3.5 text-primary mt-0.5 shrink-0" />
                <p class="text-sm">{{ reglaLabel(r) }}</p>
              </div>
              <div class="flex items-center gap-1 shrink-0">
                <AppButton variant="ghost" size="icon" class="h-7 w-7" title="Editar" @click="abrirEditarRegla(r)">
                  <Edit class="h-3.5 w-3.5" />
                </AppButton>
                <AppButton
                  variant="ghost" size="icon"
                  class="h-7 w-7 text-destructive hover:text-destructive"
                  title="Eliminar"
                  :loading="deletingReglaId === r.id"
                  @click="eliminarRegla(r)"
                >
                  <Trash2 class="h-3.5 w-3.5" />
                </AppButton>
              </div>
            </div>
          </div>

          <AppButton size="sm" variant="outline" :disabled="preguntas.length < 2" @click="abrirCrearRegla">
            <Plus class="h-3.5 w-3.5" />
            Nueva regla
          </AppButton>
          <p v-if="preguntas.length < 2" class="text-xs text-muted-foreground">
            Agrega al menos 2 preguntas para crear reglas condicionales.
          </p>
        </div>
      </AppCard>

      <!-- Enlace público -->
      <AppCard v-if="encuesta.estado === 'PUBLICADA'" class="p-5">
        <div class="flex flex-wrap items-center justify-between gap-3">
          <div class="flex items-center gap-2 text-sm font-medium">
            <Link class="h-4 w-4 text-primary" />
            Enlace público para responder
          </div>
          <AppButton size="sm" :loading="generandoEnlace" @click="generarEnlace">
            Generar enlace
          </AppButton>
        </div>
        <div v-if="enlacePublico" class="mt-3 flex items-center gap-2">
          <code class="flex-1 truncate rounded bg-muted px-3 py-2 text-xs font-mono">{{ enlacePublico }}</code>
          <AppButton variant="ghost" size="icon" title="Copiar" @click="copiarEnlace">
            <Copy class="h-4 w-4" />
          </AppButton>
        </div>
        <p v-if="enlacePublico" class="mt-1.5 text-xs text-muted-foreground">
          Cada clic en "Generar enlace" crea un nuevo token de un solo uso.
        </p>
      </AppCard>

      <!-- Preguntas -->
      <div class="space-y-4">
        <div class="flex items-center justify-between">
          <div>
            <h3 class="font-semibold">Preguntas</h3>
            <p class="text-sm text-muted-foreground">{{ preguntas.length }} pregunta(s)</p>
          </div>
          <AppButton @click="abrirCrear" :disabled="encuesta.estado === 'CERRADA'">
            <Plus class="h-4 w-4" />
            Agregar pregunta
          </AppButton>
        </div>

        <div v-if="loadingPreg" class="flex justify-center py-8"><AppSpinner /></div>

        <!-- Lista vacía -->
        <AppCard v-else-if="preguntas.length === 0" class="p-10 text-center text-muted-foreground">
          <div class="mx-auto mb-3 h-12 w-12 rounded-full bg-muted flex items-center justify-center">
            <AlignLeft class="h-6 w-6 opacity-40" />
          </div>
          <p class="font-medium">Sin preguntas</p>
          <p class="text-sm mt-1">Agrega la primera pregunta para comenzar a diseñar la encuesta.</p>
        </AppCard>

        <!-- Lista de preguntas -->
        <div v-else class="space-y-2">
          <AppCard
            v-for="(p, idx) in preguntas"
            :key="p.id"
            class="px-4 py-3"
          >
            <div class="flex items-start gap-3">
              <!-- Número y grip -->
              <div class="flex items-center gap-2 shrink-0 pt-0.5">
                <GripVertical class="h-4 w-4 text-muted-foreground/40" />
                <span class="text-sm font-bold text-muted-foreground w-5 text-center">{{ idx + 1 }}</span>
              </div>

              <!-- Icono tipo -->
              <div class="shrink-0 mt-0.5 rounded-md p-1.5 bg-primary/10 text-primary">
                <component :is="tipoIcon(p.tipo)" class="h-4 w-4" />
              </div>

              <!-- Contenido -->
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2 flex-wrap">
                  <p class="font-medium text-sm">{{ p.titulo }}</p>
                  <AppBadge v-if="p.esObligatoria" variant="destructive" class="text-[10px] py-0">Obligatoria</AppBadge>
                </div>
                <div class="flex items-center gap-3 mt-1 flex-wrap">
                  <span class="text-xs text-muted-foreground">{{ tipoLabel(p.tipo) }}</span>
                  <span v-if="nombreSeccion(p.seccionId)" class="text-xs text-primary/80 font-medium">
                    § {{ nombreSeccion(p.seccionId) }}
                  </span>
                  <span v-if="nombreDimension(p.dimensionId)" class="text-xs text-violet-500/90 font-medium">
                    ◈ {{ nombreDimension(p.dimensionId) }}
                  </span>
                  <span v-if="p.descripcion" class="text-xs text-muted-foreground truncate max-w-xs">{{ p.descripcion }}</span>
                </div>
              </div>

              <!-- Acciones -->
              <div class="flex items-center gap-1 shrink-0">
                <!-- Botón opciones — solo en tipos que lo requieren -->
                <AppButton
                  v-if="TIPOS_CON_OPCIONES.has(p.tipo)"
                  variant="ghost" size="sm"
                  title="Gestionar opciones"
                  class="text-xs gap-1"
                  @click="abrirOpciones(p)"
                >
                  <SlidersHorizontal class="h-3.5 w-3.5" />
                  Opciones
                </AppButton>

                <AppButton variant="ghost" size="icon" title="Editar" @click="abrirEditar(p)">
                  <Edit class="h-4 w-4" />
                </AppButton>
                <AppButton
                  variant="ghost" size="icon" title="Eliminar"
                  class="text-destructive hover:text-destructive"
                  :loading="deletingId === p.id"
                  @click="eliminar(p)"
                >
                  <Trash2 class="h-4 w-4" />
                </AppButton>
              </div>
            </div>
          </AppCard>
        </div>
      </div>
    </template>

    <div v-else class="text-center py-16 text-muted-foreground">Encuesta no encontrada.</div>

    <!-- ── Modal pregunta ───────────────────────────────── -->
    <AppModal
      :open="modalOpen"
      :title="editTarget ? 'Editar pregunta' : 'Nueva pregunta'"
      size="lg"
      @close="modalOpen = false"
    >
      <form class="space-y-4" @submit.prevent="guardar">

        <!-- Tipo -->
        <div class="space-y-1.5">
          <label class="text-sm font-medium">Tipo de pregunta</label>
          <div class="grid grid-cols-2 sm:grid-cols-3 gap-2">
            <button
              v-for="t in TIPOS"
              :key="t.value"
              type="button"
              :class="[
                'flex items-center gap-2 rounded-lg border px-3 py-2 text-xs font-medium transition-colors text-left',
                form.tipo === t.value
                  ? 'border-primary bg-primary/10 text-primary'
                  : 'border-border hover:bg-accent hover:text-foreground text-muted-foreground',
              ]"
              @click="form.tipo = t.value"
            >
              <component :is="t.icon" class="h-3.5 w-3.5 shrink-0" />
              {{ t.label }}
            </button>
          </div>
          <p v-if="v$.tipo.$error" class="text-xs text-destructive">Selecciona un tipo</p>
        </div>

        <!-- Título -->
        <AppInput
          v-model="form.titulo"
          label="Título de la pregunta"
          placeholder="¿Cómo calificarías...?"
          :error="v$.titulo.$error ? 'El título es requerido (mínimo 3 caracteres)' : undefined"
          @blur="v$.titulo.$touch"
        />

        <!-- Descripción -->
        <div class="space-y-1.5">
          <label class="text-sm font-medium text-foreground">Descripción / instrucción <span class="text-muted-foreground font-normal">(opcional)</span></label>
          <textarea
            v-model="form.descripcion"
            rows="2"
            placeholder="Instrucción adicional para el respondente..."
            class="flex w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring resize-none"
          />
        </div>

        <!-- Orden / Peso / Obligatoria -->
        <div class="grid grid-cols-2 sm:grid-cols-3 gap-4">
          <AppInput v-model="form.orden" label="Orden" type="number" placeholder="1" />
          <AppInput v-model="form.peso"  label="Peso"  type="number" placeholder="1" />
          <div class="flex flex-col justify-end pb-1">
            <label class="flex items-center gap-2 text-sm cursor-pointer select-none">
              <input type="checkbox" v-model="form.esObligatoria" class="rounded accent-primary" />
              <span>Obligatoria</span>
            </label>
          </div>
        </div>

        <!-- Sección + Dimensión -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div v-if="secciones.length > 0" class="space-y-1.5">
            <label class="text-sm font-medium">
              Sección <span class="text-muted-foreground font-normal">(opcional)</span>
            </label>
            <select
              v-model="form.seccionId"
              class="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            >
              <option :value="null">— Sin sección —</option>
              <option
                v-for="s in [...secciones].sort((a,b) => a.orden - b.orden)"
                :key="s.id"
                :value="s.id"
              >{{ s.orden }}. {{ s.titulo }}</option>
            </select>
          </div>
          <div v-if="dimensiones.length > 0" class="space-y-1.5">
            <label class="text-sm font-medium">
              Dimensión <span class="text-muted-foreground font-normal">(opcional)</span>
            </label>
            <select
              v-model="form.dimensionId"
              class="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            >
              <option :value="null">— Sin dimensión —</option>
              <option
                v-for="d in [...dimensiones].sort((a,b) => a.orden - b.orden)"
                :key="d.id"
                :value="d.id"
              >{{ d.orden }}. {{ d.nombre }} ({{ (Number(d.peso)*100).toFixed(0) }}%)</option>
            </select>
          </div>
        </div>
      </form>

      <template #footer>
        <AppButton variant="outline" @click="modalOpen = false">Cancelar</AppButton>
        <AppButton :loading="saving" @click="guardar">
          {{ editTarget ? 'Actualizar' : 'Crear pregunta' }}
        </AppButton>
      </template>
    </AppModal>

    <!-- ── Modal regla condicional ──────────────────────── -->
    <AppModal
      :open="reglaModalOpen"
      :title="editRegla ? 'Editar regla condicional' : 'Nueva regla condicional'"
      size="lg"
      @close="reglaModalOpen = false"
    >
      <div class="space-y-5">

        <!-- SI -->
        <div class="rounded-lg border border-border p-4 space-y-3">
          <p class="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Condición — SI</p>

          <!-- Pregunta origen -->
          <div class="space-y-1.5">
            <label class="text-sm font-medium">Pregunta</label>
            <select
              v-model="reglaForm.si.preguntaId"
              class="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
              @change="onCondicionPreguntaChange"
            >
              <option value="">— Selecciona una pregunta —</option>
              <option v-for="p in preguntas" :key="p.id" :value="p.id">
                {{ p.orden }}. {{ p.titulo }}
              </option>
            </select>
          </div>

          <!-- Operador -->
          <div class="space-y-1.5">
            <label class="text-sm font-medium">Operador</label>
            <div class="flex flex-wrap gap-2">
              <button
                v-for="op in OPERADORES"
                :key="op.value"
                type="button"
                :class="[
                  'rounded-md border px-3 py-1.5 text-xs font-medium transition-colors',
                  reglaForm.si.operador === op.value
                    ? 'border-primary bg-primary/10 text-primary'
                    : 'border-border text-muted-foreground hover:bg-accent',
                ]"
                @click="reglaForm.si.operador = op.value"
              >{{ op.label }}</button>
            </div>
          </div>

          <!-- Valor -->
          <div class="space-y-1.5">
            <label class="text-sm font-medium">Valor</label>
            <div v-if="loadingOpcionesCondicion" class="text-xs text-muted-foreground">Cargando opciones…</div>
            <template v-else>
              <select
                v-if="opcionesCondicion.length > 0"
                v-model="reglaForm.si.valor"
                class="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
              >
                <option value="">— Selecciona un valor —</option>
                <option v-for="o in opcionesCondicion" :key="o.id" :value="o.valor">
                  {{ o.etiqueta }} ({{ o.valor }})
                </option>
              </select>
              <input
                v-else
                v-model="reglaForm.si.valor"
                type="text"
                placeholder="Valor exacto a comparar (ej: SI, 5, Lima…)"
                class="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
              />
            </template>
          </div>
        </div>

        <!-- ENTONCES -->
        <div class="rounded-lg border border-border p-4 space-y-3">
          <p class="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Acción — ENTONCES</p>

          <!-- Acción -->
          <div class="space-y-1.5">
            <label class="text-sm font-medium">Acción</label>
            <div class="flex gap-2 flex-wrap">
              <button
                v-for="ac in ACCIONES"
                :key="ac.value"
                type="button"
                :class="[
                  'rounded-md border px-3 py-1.5 text-xs font-medium transition-colors',
                  reglaForm.entonces.accion === ac.value
                    ? 'border-primary bg-primary/10 text-primary'
                    : 'border-border text-muted-foreground hover:bg-accent',
                ]"
                @click="reglaForm.entonces.accion = ac.value"
              >{{ ac.label }}</button>
            </div>
          </div>

          <!-- Pregunta objetivo -->
          <div class="space-y-1.5">
            <label class="text-sm font-medium">
              Pregunta objetivo
              <span v-if="reglaForm.entonces.accion === 'saltar'" class="text-muted-foreground font-normal">(opcional si se elige sección)</span>
            </label>
            <select
              v-model="reglaForm.entonces.preguntaObjetivoId"
              class="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            >
              <option value="">— Sin pregunta objetivo —</option>
              <option
                v-for="p in preguntas.filter(p => p.id !== reglaForm.si.preguntaId)"
                :key="p.id"
                :value="p.id"
              >{{ p.orden }}. {{ p.titulo }}</option>
            </select>
          </div>

          <!-- Sección objetivo (solo para saltar) -->
          <div v-if="reglaForm.entonces.accion === 'saltar'" class="space-y-1.5">
            <label class="text-sm font-medium">
              Sección objetivo
              <span class="text-muted-foreground font-normal">(opcional si se elige pregunta)</span>
            </label>
            <select
              v-model="reglaForm.entonces.seccionObjetivoId"
              class="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            >
              <option value="">— Sin sección objetivo —</option>
              <option v-for="s in secciones" :key="s.id" :value="s.id">
                {{ s.orden }}. {{ s.titulo }}
              </option>
            </select>
          </div>
        </div>

        <!-- Resumen -->
        <p v-if="reglaForm.si.preguntaId && reglaForm.si.valor" class="text-xs text-muted-foreground bg-muted/50 rounded-md px-3 py-2">
          <strong>Vista previa:</strong>
          Si "{{ preguntas.find(p => p.id === reglaForm.si.preguntaId)?.titulo ?? '…' }}"
          {{ OPERADORES.find(o => o.value === reglaForm.si.operador)?.label }}
          "{{ reglaForm.si.valor }}"
          →
          {{ ACCIONES.find(a => a.value === reglaForm.entonces.accion)?.label }}
          "{{ reglaForm.entonces.preguntaObjetivoId
              ? preguntas.find(p => p.id === reglaForm.entonces.preguntaObjetivoId)?.titulo
              : secciones.find(s => s.id === reglaForm.entonces.seccionObjetivoId)?.titulo ?? '…' }}"
        </p>
      </div>

      <template #footer>
        <AppButton variant="outline" @click="reglaModalOpen = false">Cancelar</AppButton>
        <AppButton :loading="savingRegla" @click="guardarRegla">
          {{ editRegla ? 'Actualizar' : 'Crear regla' }}
        </AppButton>
      </template>
    </AppModal>

    <!-- ── Modal dimensión ──────────────────────────────── -->
    <AppModal
      :open="dimensionModalOpen"
      :title="editDimension ? 'Editar dimensión' : 'Nueva dimensión'"
      @close="dimensionModalOpen = false"
    >
      <div class="space-y-4">
        <AppInput
          v-model="dimensionForm.nombre"
          label="Nombre de la dimensión"
          placeholder="Ej: Liderazgo, Comunicación, Clima laboral..."
        />
        <div class="space-y-1.5">
          <label class="text-sm font-medium">
            Descripción <span class="text-muted-foreground font-normal">(opcional)</span>
          </label>
          <textarea
            v-model="dimensionForm.descripcion"
            rows="2"
            placeholder="Qué aspecto mide esta dimensión..."
            class="flex w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring resize-none"
          />
        </div>
        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-1.5">
            <label class="text-sm font-medium">Peso relativo</label>
            <div class="flex items-center gap-2">
              <input
                type="range" min="0.01" max="1" step="0.01"
                v-model.number="dimensionForm.peso"
                class="flex-1 accent-primary"
              />
              <span class="text-sm font-bold w-10 text-right">{{ (Number(dimensionForm.peso) * 100).toFixed(0) }}%</span>
            </div>
            <p class="text-xs text-muted-foreground">
              Peso total actual: <strong>{{ (pesoTotalDimensiones * 100).toFixed(0) }}%</strong>
            </p>
          </div>
          <AppInput
            v-model="dimensionForm.orden"
            label="Orden"
            type="number"
            placeholder="1"
          />
        </div>
      </div>
      <template #footer>
        <AppButton variant="outline" @click="dimensionModalOpen = false">Cancelar</AppButton>
        <AppButton :loading="savingDimension" :disabled="!dimensionForm.nombre.trim()" @click="guardarDimension">
          {{ editDimension ? 'Actualizar' : 'Crear dimensión' }}
        </AppButton>
      </template>
    </AppModal>

    <!-- ── Modal sección ────────────────────────────────── -->
    <AppModal
      :open="seccionModalOpen"
      :title="editSeccion ? 'Editar sección' : 'Nueva sección'"
      @close="seccionModalOpen = false"
    >
      <div class="space-y-4">
        <AppInput
          v-model="seccionForm.titulo"
          label="Título de la sección"
          placeholder="Ej: Datos personales, Satisfacción general..."
        />
        <div class="space-y-1.5">
          <label class="text-sm font-medium">
            Descripción <span class="text-muted-foreground font-normal">(opcional)</span>
          </label>
          <textarea
            v-model="seccionForm.descripcion"
            rows="2"
            placeholder="Instrucción o contexto para el respondente en esta sección..."
            class="flex w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring resize-none"
          />
        </div>
        <AppInput
          v-model="seccionForm.orden"
          label="Orden"
          type="number"
          placeholder="1"
        />
      </div>
      <template #footer>
        <AppButton variant="outline" @click="seccionModalOpen = false">Cancelar</AppButton>
        <AppButton :loading="savingSeccion" :disabled="!seccionForm.titulo.trim()" @click="guardarSeccion">
          {{ editSeccion ? 'Actualizar' : 'Crear sección' }}
        </AppButton>
      </template>
    </AppModal>

    <!-- ── Modal alcance ────────────────────────────────── -->
    <AppModal
      :open="alcanceModalOpen"
      title="Agregar entidad al alcance"
      @close="alcanceModalOpen = false"
    >
      <div class="space-y-4">

        <!-- Entidad -->
        <div class="space-y-1.5">
          <label class="text-sm font-medium">Entidad</label>
          <select
            v-model="alcanceForm.entidadId"
            class="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
          >
            <option value="">— Selecciona una entidad —</option>
            <option v-for="e in entidades" :key="e.id" :value="e.id">{{ e.nombreVisible }}</option>
          </select>
        </div>

        <!-- Tipo de relación -->
        <div class="space-y-1.5">
          <label class="text-sm font-medium">Tipo de relación</label>
          <div class="space-y-2">
            <label
              v-for="(desc, key) in TIPO_RELACION_LABELS"
              :key="key"
              class="flex items-start gap-3 rounded-lg border border-border px-3 py-2.5 cursor-pointer transition-colors hover:bg-accent/50"
              :class="alcanceForm.tipoRelacion === key ? 'border-primary bg-primary/5' : ''"
            >
              <input
                type="radio"
                :value="key"
                v-model="alcanceForm.tipoRelacion"
                class="mt-0.5 accent-primary"
              />
              <div>
                <p class="text-sm font-medium">{{ key }}</p>
                <p class="text-xs text-muted-foreground">{{ desc }}</p>
              </div>
            </label>
          </div>
        </div>

        <!-- Incluir descendientes -->
        <label class="flex items-center gap-2.5 cursor-pointer select-none">
          <input type="checkbox" v-model="alcanceForm.incluirDescendientes" class="rounded accent-primary" />
          <div>
            <p class="text-sm font-medium">Incluir descendientes</p>
            <p class="text-xs text-muted-foreground">La regla aplica también a todos los nodos hijos de esta entidad.</p>
          </div>
        </label>
      </div>

      <template #footer>
        <AppButton variant="outline" @click="alcanceModalOpen = false">Cancelar</AppButton>
        <AppButton :loading="savingAlcance" :disabled="!alcanceForm.entidadId" @click="guardarAlcance">
          Agregar
        </AppButton>
      </template>
    </AppModal>

    <!-- ── Modal opciones ───────────────────────────────── -->
    <AppModal
      :open="opcionModalOpen"
      :title="`Opciones — ${opcionTargetPregunta?.titulo ?? ''}`"
      size="lg"
      @close="opcionModalOpen = false"
    >
      <div class="space-y-4">

        <!-- Lista de opciones existentes -->
        <div v-if="loadingOpciones" class="flex justify-center py-6"><AppSpinner /></div>

        <div v-else-if="opciones.length === 0 && !editingOpcion" class="text-center py-4 text-sm text-muted-foreground">
          Sin opciones. Agrega la primera abajo.
        </div>

        <div v-else-if="opciones.length > 0" class="space-y-1.5">
          <div
            v-for="o in opciones"
            :key="o.id"
            :class="[
              'flex items-center gap-2 rounded-lg border px-3 py-2 text-sm transition-colors',
              editingOpcion?.id === o.id ? 'border-primary bg-primary/5' : 'border-border',
            ]"
          >
            <span class="text-muted-foreground w-5 text-center text-xs font-bold shrink-0">{{ o.orden }}</span>
            <span class="flex-1 font-medium">{{ o.etiqueta }}</span>
            <span v-if="o.valor !== o.etiqueta" class="text-xs text-muted-foreground font-mono">{{ o.valor }}</span>
            <span v-if="o.puntaje != null" class="text-xs text-muted-foreground">pts: {{ o.puntaje }}</span>
            <div class="flex items-center gap-0.5 shrink-0">
              <button
                class="p-1 rounded hover:bg-accent text-muted-foreground hover:text-foreground transition-colors"
                title="Editar"
                @click="iniciarEditOpcion(o)"
              >
                <Edit class="h-3.5 w-3.5" />
              </button>
              <button
                class="p-1 rounded hover:bg-destructive/10 text-muted-foreground hover:text-destructive transition-colors"
                title="Eliminar"
                :disabled="deletingOpcionId === o.id"
                @click="eliminarOpcion(o)"
              >
                <Trash2 class="h-3.5 w-3.5" />
              </button>
            </div>
          </div>
        </div>

        <!-- Formulario agregar / editar opción -->
        <div class="rounded-lg border border-dashed border-border p-4 space-y-3">
          <p class="text-xs font-medium text-muted-foreground uppercase tracking-wide">
            {{ editingOpcion ? 'Editar opción' : 'Nueva opción' }}
          </p>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <AppInput
              v-model="opcionForm.etiqueta"
              label="Etiqueta"
              placeholder="Muy satisfecho"
            />
            <AppInput
              v-model="opcionForm.valor"
              label="Valor"
              placeholder="= Etiqueta si se omite"
            />
          </div>
          <div class="grid grid-cols-2 gap-3">
            <AppInput
              v-model="opcionForm.puntaje"
              label="Puntaje"
              type="number"
              placeholder="Ej: 5"
            />
            <AppInput
              v-model="opcionForm.orden"
              label="Orden"
              type="number"
              placeholder="1"
            />
          </div>
          <div class="flex gap-2">
            <AppButton :loading="savingOpcion" @click="guardarOpcion" size="sm">
              <Check class="h-3.5 w-3.5" />
              {{ editingOpcion ? 'Actualizar' : 'Agregar' }}
            </AppButton>
            <AppButton v-if="editingOpcion" variant="ghost" size="sm" @click="cancelarEditOpcion">
              <X class="h-3.5 w-3.5" />
              Cancelar
            </AppButton>
          </div>
        </div>
      </div>

      <template #footer>
        <AppButton variant="outline" @click="opcionModalOpen = false">Cerrar</AppButton>
      </template>
    </AppModal>

  </div>
</template>
