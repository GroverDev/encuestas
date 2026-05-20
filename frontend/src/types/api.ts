export interface Mensaje {
  titulo: string
  descripcion: string
  tipoMensaje: string
  tieneMensaje: boolean
  idMensaje: string
}

export interface Respuesta<T> {
  ok: boolean
  datos: T
  mensaje: Mensaje
}

export interface LoginRequest {
  correo: string
  contrasena: string
}

export interface LoginResponse {
  token: string
  usuario: CuentaUsuario
}

export interface Encuesta {
  id: string
  organizacionId: string
  titulo: string
  descripcion?: string
  version: number
  estado: 'BORRADOR' | 'PUBLICADA' | 'CERRADA'
  esGlobal: boolean
  esPlantilla: boolean
  plantillaOrigenId?: string
  etiquetasJson?: string
  creadoPorUsuarioId: string
  fechaInicio?: string
  fechaFin?: string
  publicadoEn?: string
  configuracionJson?: string
  creadoEn: string
}

export interface EncuestaRequest {
  id?: string
  organizacionId?: string
  titulo: string
  descripcion?: string
  esGlobal: boolean
  esPlantilla: boolean
  plantillaOrigenId?: string
  etiquetasJson?: string
  creadoPorUsuarioId?: string
  fechaInicio?: string
  fechaFin?: string
  configuracionJson?: string
}

export interface Pregunta {
  id: string
  encuestaId: string
  seccionId?: string
  dimensionId?: string
  tipo: string
  titulo: string
  descripcion?: string
  orden: number
  peso: number
  esObligatoria: boolean
  configuracionJson?: string
  creadoEn: string
}

export interface SeccionPublica {
  id: string
  titulo: string
  descripcion?: string
  orden: number
}

export interface EncuestaPublica {
  invitacionId: string
  encuestaId: string
  titulo: string
  descripcion?: string
  version: number
  secciones: SeccionPublica[]
  preguntas: PreguntaPublica[]
  reglas: string[]
}

export interface PreguntaPublica {
  id: string
  seccionId?: string
  tipo: string
  titulo: string
  descripcion?: string
  orden: number
  esObligatoria: boolean
  configuracionJson?: string
  opciones: OpcionPregunta[]
}

export interface DetalleRespuestaPublica {
  preguntaId: string
  valorTexto?: string
  valorNumero?: number
  valorBooleano?: boolean
  valorFecha?: string
  valorJson?: string
}

export interface OpcionPregunta {
  id: string
  preguntaId: string
  etiqueta: string
  valor: string
  puntaje?: number
  orden: number
}

export interface OpcionPreguntaRequest {
  id?: string
  preguntaId: string
  etiqueta: string
  valor: string
  puntaje?: number
  orden: number
}

export interface Organizacion {
  id: string
  nombre: string
  urlLogo?: string
  creadoEn: string
}

export interface CuentaUsuario {
  id: string
  organizacionId: string
  entidadId?: string
  correo: string
  esActivo: boolean
  esCuentaServicio: boolean
  creadoEn: string
}

export interface Invitacion {
  id: string
  encuestaId: string
  cuentaUsuarioId?: string
  correoDestino?: string
  entidadEvaluadaId?: string
  tokenAcceso: string
  canal: string
  estado: string
  enviadoEn?: string
  venceEn?: string
  respondidoEn?: string
}

export interface SeccionEncuesta {
  id: string
  encuestaId: string
  titulo: string
  descripcion?: string
  orden: number
}

export interface SeccionEncuestaRequest {
  id?: string
  encuestaId: string
  titulo: string
  descripcion?: string
  orden: number
}

export interface AlcanceEncuesta {
  id: string
  encuestaId: string
  entidadId: string
  tipoRelacion: 'AMBITO' | 'SUJETO' | 'CONTEXTO'
  incluirDescendientes: boolean
}

export interface AlcanceEncuestaRequest {
  id?: string
  encuestaId: string
  entidadId: string
  tipoRelacion: string
  incluirDescendientes: boolean
}

export interface DimensionPregunta {
  id: string
  encuestaId: string
  nombre: string
  descripcion?: string
  peso: number
  orden: number
}

export interface DimensionPreguntaRequest {
  id?: string
  encuestaId: string
  nombre: string
  descripcion?: string
  peso: number
  orden: number
}

export interface ReglaEncuesta {
  id: string
  encuestaId: string
  reglaJson: string
}

export interface ReglaEncuestaRequest {
  id?: string
  encuestaId: string
  reglaJson: string
}

export interface ReglaJson {
  si: {
    preguntaId: string
    operador: 'igual' | 'distinto' | 'mayor' | 'menor' | 'contiene'
    valor: string
  }
  entonces: {
    accion: 'mostrar' | 'ocultar' | 'saltar'
    preguntaObjetivoId?: string
    seccionObjetivoId?: string
  }
}

export interface TipoEntidad {
  id: string
  codigo: string
  nombre: string
}

export interface Entidad {
  id: string
  organizacionId: string
  tipoEntidadId: string
  entidadPadreId?: string
  nombreVisible: string
  idExterno?: string
  esActivo: boolean
  atributosJson?: string
  creadoEn: string
}

export interface Rol {
  id: string
  organizacionId: string
  nombre: string
}

export interface Permiso {
  id: string
  codigo: string
  nombre: string
}

export interface MetricaEntidad {
  preguntaId: string
  titulo: string
  tipo: string
  promedio?: number
  puntajeNps?: number
}

export interface ResumenEntidad {
  entidadId: string
  nombreEntidad: string
  entidadPadreId?: string
  idExterno?: string
  tipoEntidad: string
  totalRespuestas: number
  metricas: MetricaEntidad[]
}

export interface DashboardStats {
  totalEncuestas: number
  encuestasActivas: number
  totalRespuestas: number
  totalInvitaciones: number
}

export interface EstadisticasEncuesta {
  totalRespuestas: number
  preguntas: EstadisticasPregunta[]
}

export interface EstadisticasPregunta {
  preguntaId: string
  tipo: string
  titulo: string
  totalRespuestas: number
  promedio?: number
  minimo?: number
  maximo?: number
  puntajeNps?: number
  conteos: ConteoOpcion[]
  textosLibres: string[]
}

export interface ConteoOpcion {
  valor: string
  etiqueta: string
  cantidad: number
  porcentaje: number
}
