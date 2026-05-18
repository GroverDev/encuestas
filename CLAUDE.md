# CLAUDE.md

Este archivo proporciona orientación a Claude Code (claude.ai/code) al trabajar con el código de este repositorio.

## Descripción del proyecto

**Motor de Encuestas Enterprise** — plataforma SaaS multi-tenant construida sobre PostgreSQL. Permite a organizaciones crear, distribuir y analizar encuestas que pueden evaluar **cualquier tipo de objeto** (empleados, departamentos, regionales, sucursales, servicios, aplicaciones, eventos, etc.) sin cambiar el modelo de datos.

Diseñado para ser comparable en funcionalidad a Qualtrics y SurveyMonkey Enterprise.

---

## Base de datos

- Esquema: `base-de-datos/schema.sql`
- Motor: PostgreSQL con extensión `pgcrypto` (`gen_random_uuid()`)
- Todo en español: tablas, columnas, comentarios
- 24 tablas, todas con `COMMENT ON TABLE` y `COMMENT ON COLUMN`

---

## Arquitectura del esquema

### Multi-tenancy

`OrganizacionId` es la raíz del tenant. **Toda consulta debe filtrar por `OrganizacionId`** para garantizar aislamiento entre clientes.

---

### Patrón de Entidad Genérica y jerarquía organizacional

`Entidad` es el punto de extensión central del sistema. Una sola tabla modela tanto los nodos organizacionales como los objetos evaluables:

```
EMPRESA "Acme Corp"
  REGIONAL "Regional Norte"         ← EntidadPadreId → Acme Corp
    UNIDAD "RRHH"                   ← EntidadPadreId → Regional Norte
    UNIDAD "Atención al Cliente"    ← EntidadPadreId → Regional Norte
    UNIDAD "Servicios"              ← EntidadPadreId → Regional Norte
  REGIONAL "Regional Sur"           ← EntidadPadreId → Acme Corp
    UNIDAD "RRHH"                   ← EntidadPadreId → Regional Sur
```

**Tipos organizacionales** (nodos de jerarquía): `EMPRESA`, `REGIONAL`, `UNIDAD`

**Tipos evaluables** (sujetos de encuesta): `EMPLEADO`, `CLIENTE`, `PROVEEDOR`, `PRODUCTO`, `SERVICIO`, `APLICACION`, `PROYECTO`, `EVENTO`

Para recorrer la jerarquía completa se usa `WITH RECURSIVE` sobre `Entidad.EntidadPadreId` (ver referencia al final del schema).

`Entidad.AtributosJson` (JSONB + GIN) almacena propiedades específicas de cada tipo sin crear tablas adicionales. Ejemplo: `{"ciudad":"Lima","region":"Sur"}` para una sucursal, `{"cargo":"Gerente"}` para un empleado.

Para agregar un nuevo tipo evaluable: insertar en `TipoEntidad` y crear registros en `Entidad`. No se requieren nuevas tablas.

---

### RBAC

`CuentaUsuario → UsuarioRol → Rol → RolPermiso → Permiso`

- `Permiso.Codigo` es la clave técnica usada en validaciones (ej: `encuesta.publicar`, `reporte.exportar`)
- `Rol` está scoped a `OrganizacionId`
- `CuentaUsuario.EntidadId` vincula el usuario a su entidad correspondiente (nullable para admins sin entidad)

---

### Ciclo de vida de una Encuesta

```
BORRADOR → PUBLICADA → CERRADA
```

- `Encuesta.Version` (entero) se incrementa al modificar preguntas de una encuesta ya publicada
- `Respuesta.VersionEncuesta` guarda la versión vigente al momento de responder — garantiza integridad histórica
- `FechaInicio` / `FechaFin` controlan el período de recepción de respuestas
- `EsPlantilla = TRUE` → la encuesta es una plantilla reutilizable, no se distribuye directamente
- `PlantillaOrigenId` registra de qué plantilla se clonó una encuesta
- `EtiquetasJson` (array JSONB) para categorización y filtrado del catálogo

---

### Alcance de una Encuesta

Controla a quién aplica la encuesta:

| Caso | Configuración |
|------|--------------|
| Global (toda la organización) | `EsGlobal = TRUE`, no usar `AlcanceEncuesta` |
| Solo una regional | `EsGlobal = FALSE` + `AlcanceEncuesta(EntidadId=Regional, IncluirDescendientes=FALSE)` |
| Regional + todas sus unidades | `EsGlobal = FALSE` + `AlcanceEncuesta(EntidadId=Regional, IncluirDescendientes=TRUE)` |
| Solo una unidad | `EsGlobal = FALSE` + `AlcanceEncuesta(EntidadId=Unidad, IncluirDescendientes=FALSE)` |

`AlcanceEncuesta.TipoRelacion`: `AMBITO` (delimita quiénes responden), `SUJETO` (el nodo es el evaluado), `CONTEXTO` (referencia sin ser evaluado).

---

### Estructura interna de una Encuesta

Las preguntas tienen dos niveles de organización **independientes entre sí**:

| Concepto | Tabla | Propósito |
|---|---|---|
| Sección | `SeccionEncuesta` | Organización **visual** — páginas/bloques para el respondente |
| Dimensión | `DimensionPregunta` | Agrupación **analítica** — ejes temáticos para estadísticas y puntajes |

Una `Pregunta` puede tener `SeccionId` y `DimensionId` simultáneamente o independientemente.

`DimensionPregunta.Peso` y `Pregunta.Peso` permiten calcular índices compuestos ponderados (ej: Liderazgo=30%, Comunicación=20%, Clima=50%).

---

### Tipos de pregunta y columna de valor

`DetalleRespuesta` usa valor polimórfico — solo se llena la columna que corresponde al tipo:

| Tipo | Columna | Notas |
|------|---------|-------|
| `TEXTO` | `ValorTexto` | Texto libre |
| `NUMERO` | `ValorNumero` | Numérico directo |
| `BOOLEANO` | `ValorBooleano` | Sí/No, Verdadero/Falso |
| `FECHA` | `ValorFecha` | Fecha y/u hora |
| `SELECCION_UNICA` | `ValorTexto` | Almacena `OpcionPregunta.Valor`; incluye Likert |
| `SELECCION_MULTIPLE` | `ValorJson` | Array: `["op1","op2"]` |
| `ESCALA` | `ValorNumero` | Deslizador 1-N |
| `NPS` | `ValorNumero` | 0-10; zonas: 0-6 Detractor, 7-8 Neutro, 9-10 Promotor |
| `CALIFICACION` | `ValorNumero` | Estrellas 1-N |
| `RANKING` | `ValorJson` | Orden resultante: `["op3","op1","op2"]` |
| `MATRIZ` | `ValorJson` | Mapa fila→opción: `{"Atención":"Bueno","Rapidez":"Excelente"}` |

`OpcionPregunta.Puntaje NUMERIC` asigna valor numérico a cada opción para cálculos estadísticos (Likert, NPS, etc.).

`Pregunta.ConfiguracionJson` (JSONB + GIN) lleva parámetros específicos del tipo: `min`, `max`, `paso`, `etiquetaMin`, `etiquetaMax`, `placeholder`, `estrellas`, `filas`, etc.

Para adjuntar un archivo como respuesta a una pregunta: almacenar la URL en `ValorTexto`.

---

### Lógica condicional

`ReglaEncuesta.ReglaJson` (JSONB + GIN) — reglas if/then evaluadas en la capa de aplicación al navegar:

```json
{
  "si":      { "preguntaId": "<uuid>", "operador": "igual", "valor": "SI" },
  "entonces": { "accion": "mostrar", "preguntaObjetivoId": "<uuid>" }
}
```

Operadores: `igual`, `distinto`, `mayor`, `menor`, `contiene`  
Acciones: `mostrar`, `ocultar`, `saltar`  
También soporta `seccionObjetivoId` para saltar secciones completas.

---

### Distribución: Invitaciones y Notificaciones

`Invitacion` gestiona quién fue invitado a responder:
- Destinatario: `CuentaUsuarioId` (usuario con cuenta) **o** `CorreoDestino` (externo) — al menos uno obligatorio (constraint CHECK)
- `EntidadEvaluadaId` — entidad que este respondente debe evaluar (ej: el empleado específico a calificar)
- `TokenAcceso UUID` — token único para acceso por enlace sin inicio de sesión (índice UNIQUE)
- `Canal`: `EMAIL`, `SMS`, `QR`, `ENLACE_PUBLICO`, `KIOSCO`, `API`
- `Estado`: `PENDIENTE` → `RESPONDIDA` / `EXPIRADA` / `CANCELADA`

`NotificacionEnvio` es el historial completo de mensajes enviados (invitaciones, recordatorios, confirmaciones). Registra `IntentosEnvio`, `Estado` (`ENVIADO`, `ENTREGADO`, `FALLIDO`, `REBOTADO`) y `ErrorDetalle` para depuración.

---

### Targeting de respuestas

Dos tablas con propósitos distintos:

| Tabla | Cuándo | Para qué |
|---|---|---|
| `AlcanceEncuesta` | Diseño (antes de publicar) | Define a qué nodos aplica la encuesta |
| `ObjetivoRespuesta` | Ejecución (al responder) | Registra qué entidades fueron realmente evaluadas |

Una sola `Respuesta` puede evaluar múltiples entidades simultáneamente con diferentes `TipoRelacion` (EVALUADO, CONTEXTO, EVENTO...).

---

### Respuesta — campos analíticos clave

| Campo | Para qué sirve |
|---|---|
| `VersionEncuesta` | Integridad histórica al versionar encuestas |
| `Canal` | Análisis de tasas de completación por canal |
| `UltimaPreguntaId` | Análisis de abandono: qué pregunta aleja a los respondentes |
| `PesoEstadistico` | Ponderación de segmentos sub-representados en la muestra |
| `ConsentimientoOtorgado` | Cumplimiento GDPR/LGPD |

---

### Cuotas de respuesta

`CuotaRespuesta` permite muestreo estadístico balanceado: cierra automáticamente un segmento al alcanzar el límite aunque la encuesta global siga activa.

- `EntidadId = NULL` → cuota global de toda la encuesta
- `EntidadId = Regional Norte` → cuota para ese segmento específico
- `TotalActual` se actualiza por trigger o por la capa de aplicación al registrar cada respuesta completada

---

### Colaboración

`ColaboradorEncuesta` permite co-autoría con roles:
- `EDITOR` — puede modificar preguntas y configuración
- `REVISOR` — solo lectura y comentarios
- `APROBADOR` — puede publicar la encuesta

---

### Anotaciones

`Comentario` puede colgar de una `Entidad` (perfil) **o** de una `Respuesta` (anotación analítica). Constraint `CK_Comentario_Objetivo` garantiza que al menos uno esté presente.

`Adjunto` está vinculado a `Entidad`. Para archivos adjuntos en respuestas a preguntas, usar `DetalleRespuesta.ValorTexto` con la URL.

---

## Índices JSONB

Todos los campos JSONB tienen índice GIN para operaciones de containment (`@>`):

| Tabla | Campo | Uso típico |
|---|---|---|
| `Entidad` | `AtributosJson` | Filtrar por atributos del tipo (ciudad, cargo, etc.) |
| `Encuesta` | `EtiquetasJson` | Filtrar encuestas por etiqueta |
| `Pregunta` | `ConfiguracionJson` | Buscar preguntas con configuración específica |
| `ReglaEncuesta` | `ReglaJson` | Buscar reglas que afectan una pregunta |
| `DetalleRespuesta` | `ValorJson` | Consultar selecciones múltiples y matrices |

---

## Decisiones de diseño clave

- `EsActivo` solo existe en `Entidad` y `CuentaUsuario` — las demás tablas usan hard delete
- `Entidad.IdExterno` es el hook de integración con RRHH, CRM o ERP externos
- `UsuarioRespondentId` es nullable en `Respuesta` para soportar encuestas anónimas
- `Invitacion.TokenAcceso` tiene índice UNIQUE — búsqueda O(1) en cada request de acceso por enlace
- `CuotaRespuesta.TotalActual` es un contador desnormalizado; debe actualizarse atómicamente (usar `UPDATE ... WHERE TotalActual < Limite` para evitar race conditions)
- `NotificacionEnvio` es append-only — nunca se editan registros de envío, solo se agregan nuevos intentos

---

## Permisos del sistema

Los códigos de permiso usados en validaciones de código:

```
encuesta.ver / crear / editar / publicar / cerrar / eliminar
encuesta.plantilla.usar
encuesta.colaborar
invitacion.administrar / recordatorio
respuesta.ver / anotar
reporte.ver / exportar / estadistico
cuota.administrar
usuario.administrar
entidad.administrar
```

---

## Roadmap — pendiente para versiones futuras

| Feature | Motivo |
|---|---|
| `BancoPregunta` — biblioteca de preguntas aprobadas y reutilizables | Garantiza consistencia en mediciones longitudinales |
| Snapshot de versión de encuesta | Actualmente `VersionEncuesta` registra el número pero no el estado estructural de esa versión |
| Soporte i18n — traducciones de encuesta/preguntas/opciones | Para organizaciones multi-país |
| `TemaVisual` — branding por organización/encuesta | Encuestas de cara al cliente con identidad visual propia |
