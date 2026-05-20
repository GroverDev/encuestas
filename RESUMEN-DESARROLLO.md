# Resumen de Desarrollo — Motor de Encuestas Enterprise

Fecha: 2026-05-19  
Stack: Vue 3 + TypeScript (frontend) · .NET 8 CSBP 4 capas (backend) · PostgreSQL

---

## Arquitectura general

```
frontend/                   Vue 3 + Vite + Tailwind CSS
backend/
  1-Servicios/Encuesta.Api       Controllers (HTTP)
  2-LogicaNegocios/              BLL (reglas de negocio)
  3-AccesoDatos/                 DAL (Dapper + PostgreSQL)
  4-ModeloDatos/                 Request / Response / Entidades
  5-Herramientas/Comun.*         Utilidades compartidas (DapperDb, Respuesta<T>, etc.)
base-de-datos/schema.sql         Esquema PostgreSQL completo (24 tablas)
```

Patrón de respuesta uniforme en toda la API:
```json
{ "ok": true, "datos": <T>, "mensaje": { "titulo": "", "descripcion": "", ... } }
```

---

## Módulos implementados

### 1. Autenticación (`AuthController`)

- `POST /api/auth/login` — login con correo + contraseña, emite JWT de 8 horas
- `POST /api/auth/token-servicio` — emite JWT de 365 días para cuentas de servicio (M2M)
  - Verifica que `EsCuentaServicio = true` antes de emitir
  - Útil para integrar sistemas externos sin intervención humana
- Claims del JWT: `sub`, `email`, `OrganizacionId`, `EsCuentaServicio`
- Archivos: `AuthController.cs`

---

### 2. Usuarios (`CuentaUsuario`)

CRUD completo con:
- Crear / editar / desactivar (soft delete via `EsActivo`)
- Campo `EsCuentaServicio` para cuentas M2M
- Pantalla frontend: `UsuariosView.vue`
  - Tabla con badge de tipo (Usuario / Servicio M2M)
  - Modal crear/editar con toggle de cuenta de servicio
  - Búsqueda por correo

Archivos backend modificados:
- `CuentaUsuario.cs`, `CuentaUsuarioRequest.cs`, `CuentaUsuarioResponse.cs` — agregado `EsCuentaServicio`
- `CuentaUsuarioDAL.cs` — SELECT/INSERT/UPDATE con `es_cuenta_servicio`

---

### 3. Entidades y jerarquía organizacional (`Entidad`)

Modelo genérico que representa tanto nodos organizacionales como objetos evaluables.

**Endpoints:**
- `GET /api/entidad` — lista todas las entidades de la organización
- `GET /api/entidad/{id}` — detalle
- `POST /api/entidad` — crear
- `PUT /api/entidad` — actualizar
- `DELETE /api/entidad/{id}` — desactivar (soft delete)
- `POST /api/entidad/sincronizar` — **upsert por `idExterno`** (para integración con RRHH/ERP)

**Sincronización por ID externo (`POST /api/entidad/sincronizar`):**
```json
{
  "tipoEntidadId": "<uuid>",
  "nombreVisible": "Juan Pérez",
  "idExterno": "EMP-001",
  "atributosJson": "{\"cargo\": \"Asesor\"}"
}
```
Usa `INSERT ... ON CONFLICT (organizacion_id, id_externo) DO UPDATE RETURNING id`.  
Requiere índice único parcial en BD:
```sql
CREATE UNIQUE INDEX uq_entidad_id_externo
  ON entidad(organizacion_id, id_externo)
  WHERE id_externo IS NOT NULL;
```

Pantalla frontend: `EntidadesView.vue`
- Árbol jerárquico con indentación visual
- Filtro por tipo de entidad
- Badges de tipo y estado

---

### 4. Encuestas — Ciclo de vida completo

#### Estado: `BORRADOR → PUBLICADA → CERRADA`

**Endpoints:**
- `GET /api/encuesta` — lista
- `POST /api/encuesta` — crear
- `PUT /api/encuesta` — actualizar
- `PATCH /api/encuesta/{id}/estado` — cambiar estado: `{ "nuevoEstado": "PUBLICADA" }`
- `DELETE /api/encuesta/{id}` — eliminar

Pantalla: `EncuestasView.vue`  
Detalle: `EncuestaDetalleView.vue` (hub central con pestañas)

---

### 5. Preguntas (`Pregunta`)

Tipos soportados:
`TEXTO | NUMERO | FECHA | BOOLEANO | SELECCION_UNICA | SELECCION_MULTIPLE | ESCALA | NPS | CALIFICACION | RANKING | MATRIZ`

Cada pregunta puede tener:
- `seccionId` — para agrupar visualmente en la encuesta
- `dimensionId` — para agrupación analítica en estadísticas
- `configuracionJson` — parámetros del tipo (`{ "estrellas": 5 }`, `{ "min": 1, "max": 10 }`, etc.)
- `peso` — para índices ponderados

**Endpoints:**
- `GET /api/pregunta/{encuestaId}`
- `POST /api/pregunta`
- `PUT /api/pregunta`
- `DELETE /api/pregunta/{id}`

---

### 6. Opciones de pregunta (`OpcionPregunta`)

Para preguntas de tipo selección, ranking o matriz.

- `GET /api/opcionpregunta/{preguntaId}`
- `POST /api/opcionpregunta`
- `PUT /api/opcionpregunta`
- `DELETE /api/opcionpregunta/{id}`

Cada opción tiene `etiqueta`, `valor` (clave interna), `puntaje` (para scoring), `orden`.

---

### 7. Secciones (`SeccionEncuesta`)

Organización **visual** de preguntas en páginas/bloques para el respondente.

**Endpoints:**
- `GET /api/seccionencuesta/{encuestaId}`
- `POST /api/seccionencuesta`
- `PUT /api/seccionencuesta`
- `DELETE /api/seccionencuesta/{id}`

En el formulario de respuesta (`ResponderView`), las secciones se muestran como **páginas navegables** con "Anterior / Siguiente / Enviar".

---

### 8. Dimensiones (`DimensionPregunta`)

Agrupación **analítica** de preguntas en ejes temáticos para estadísticas y puntajes ponderados.

**Endpoints:**
- `GET /api/dimensionpregunta/{encuestaId}`
- `POST /api/dimensionpregunta`
- `PUT /api/dimensionpregunta`
- `DELETE /api/dimensionpregunta/{id}`

Cada dimensión tiene `peso` (ej: 0.30 = 30% del puntaje total).  
En las estadísticas (`ResultadosView`), las preguntas se agrupan por dimensión mostrando promedio por eje.

---

### 9. Alcance (`AlcanceEncuesta`)

Define a qué nodos organizacionales aplica la encuesta.

**Endpoints:**
- `GET /api/alcanceencuesta/{encuestaId}`
- `POST /api/alcanceencuesta`
- `DELETE /api/alcanceencuesta/{id}`

`TipoRelacion`: `AMBITO` | `SUJETO` | `CONTEXTO`  
`IncluirDescendientes`: si aplica a toda la jerarquía bajo ese nodo.

---

### 10. Reglas condicionales (`ReglaEncuesta`)

Lógica if/then evaluada en el cliente al navegar la encuesta.

**Endpoints:**
- `GET /api/reglaencuesta/{encuestaId}`
- `POST /api/reglaencuesta`
- `DELETE /api/reglaencuesta/{id}`

Formato `reglaJson`:
```json
{
  "si": { "preguntaId": "<uuid>", "operador": "igual", "valor": "no_resuelto" },
  "entonces": { "accion": "mostrar", "preguntaObjetivoId": "<uuid>" }
}
```

Operadores: `igual | distinto | mayor | menor | contiene`  
Acciones: `mostrar | ocultar | saltar`

**Motor de reglas en `ResponderView`:**
- Preguntas objetivo de `mostrar` están **ocultas por defecto**
- El motor reevalúa reactivamente con `computed()` en cada cambio de respuesta
- Usa `v-show` (no `v-if`) para preservar el estado de respuestas ocultas
- `validar()` omite preguntas no visibles; `enviar()` filtra al conjunto visible

---

### 11. Invitaciones (`Invitacion`)

Gestiona quién fue invitado a responder y genera tokens de acceso únicos.

**Endpoints:**
- `GET /api/invitacion/{encuestaId}` — lista
- `POST /api/invitacion` — crear invitación
- `DELETE /api/invitacion/{id}` — cancelar

**Payload de creación:**
```json
{
  "encuestaId": "<uuid>",
  "correoDestino": "empleado@empresa.com",
  "entidadEvaluadaId": "<uuid>",          // UUID directo
  "entidadEvaluadaIdExterno": "EMP-001",  // O resolver por idExterno
  "canal": "EMAIL",
  "venceEn": "2026-06-01T00:00:00"
}
```

- `entidadEvaluadaIdExterno`: el BLL resuelve el UUID internamente buscando por `id_externo` en la misma organización
- `tokenAcceso` es un UUID único que el respondente usa para acceder sin cuenta
- Estados: `PENDIENTE → RESPONDIDA | EXPIRADA | CANCELADA`
- Canales: `EMAIL | SMS | QR | ENLACE_PUBLICO | KIOSCO | API`

Pantalla frontend: `InvitacionesView.vue`
- Dos pestañas: "Invitaciones" (gestión) y "Integración API" (documentación)
- La pestaña API documenta cómo crear invitaciones desde sistemas externos via M2M

---

### 12. Formulario público de respuesta (`PublicoController`)

Endpoints **sin autenticación** para acceso mediante token:

- `GET /api/publico/encuesta/{token}` — carga la encuesta pública con secciones, preguntas, opciones y reglas
- `POST /api/publico/responder/{token}` — guarda la respuesta y marca la invitación como RESPONDIDA

**Respuesta pública incluye:**
- `secciones[]` — lista ordenada de secciones con título y descripción
- `preguntas[]` — con `seccionId` para agrupar
- `reglas[]` — JSON strings de reglas condicionales
- `opciones[]` — por pregunta, con `puntaje` para scoring

**Pantalla `ResponderView.vue`:**
- Navegación sección por sección (paginación)
- Barra de progreso ("Sección 2 de 4 — 50%")
- Encabezado destacado por sección
- Validación por sección al avanzar
- Motor de reglas condicionales integrado
- Soporte para todos los tipos de pregunta

---

### 13. Resultados y Estadísticas (`ResultadosView.vue`)

Pantalla interna con dos pestañas:

**Respuestas individuales:**
- Lista expandible con detalle por pregunta
- Muestra canal de respuesta y fecha

**Estadísticas:**
- Resumen global (total respuestas)
- **Agrupación por dimensión**: preguntas ordenadas bajo su dimensión con barra de score
- Por tipo de pregunta: barras horizontales, gauge NPS, estrellas, min/avg/max, textos libres
- Carga dimensions con `GET /api/dimensionpregunta/{encuestaId}` y une con estadísticas

---

## Flujo de integración M2M (sistemas externos)

```
1. Registrar cuenta de servicio
   POST /api/cuentausuario  { esCuentaServicio: true, ... }

2. Obtener token de larga duración (365 días)
   POST /api/auth/token-servicio  { correo, contrasena }
   → { token: "eyJ..." }

3. Sincronizar empleados desde HR system
   POST /api/entidad/sincronizar
   Authorization: Bearer <token>
   { idExterno: "EMP-001", nombreVisible: "Juan", tipoEntidadId: "..." }
   → { datos: "<uuid-entidad>" }

4. Crear invitación para evaluación
   POST /api/invitacion
   { encuestaId: "...", entidadEvaluadaIdExterno: "EMP-001", canal: "API" }
   → { datos: { tokenAcceso: "<uuid>", ... } }

5. Construir enlace de encuesta
   https://tudominio.com/responder/<tokenAcceso>
```

---

## Demo creada: "Evaluación de Atención al Cliente 2024"

**Encuesta ID:** `6b2e3c79-5ba8-4ab3-9703-a204f8bb901f`

### Estructura

| Sección | Preguntas |
|---------|-----------|
| S1: Identificación del Servicio | Q1: Motivo de consulta (selección única) |
| S2: Calidad de la Atención | Q2: Amabilidad ⭐ · Q3: Conocimiento · Q4: Tiempo de espera |
| S3: Resolución de tu Caso | Q5: ¿Se resolvió? · Q6: ¿Por qué no? *(condicional)* |
| S4: Experiencia General | Q7: NPS 0-10 · Q8: ¿Qué mejorar? *(condicional)* · Q9: Comentario |

### Dimensiones (peso analítico)

| Dimensión | Peso | Preguntas |
|-----------|------|-----------|
| Amabilidad y Trato | 30% | Q2 |
| Conocimiento del Servicio | 25% | Q3 |
| Eficiencia y Tiempo | 20% | Q4 |
| Resolución del Problema | 25% | Q5, Q6 |

### Reglas condicionales

| Condición | Acción |
|-----------|--------|
| Q5 == "no_resuelto" | Mostrar Q6 (texto libre de explicación) |
| Q7 (NPS) < 7 | Mostrar Q8 (sugerencias de mejora) |

### Entidades de demo

```
TechCorp S.A. (EMPRESA-001)
├── Regional Norte (REG-NORTE)
│   ├── EMP-N01  → cliente1@example.com  token: 939ad13a-...
│   └── EMP-N02  → cliente2@example.com  token: 26230302-...
├── Regional Sur (REG-SUR)
│   ├── EMP-S01  → cliente3@example.com  token: 163761ab-...
│   └── EMP-S02  → cliente4@example.com  token: bf4ae438-...
└── Regional Centro (REG-CENTRO)
    ├── EMP-C01  → cliente5@example.com  token: ce42c20d-...
    └── EMP-C02  → cliente6@example.com  token: d75e8079-...
```

---

## Archivos clave modificados / creados

### Backend

| Archivo | Cambio |
|---------|--------|
| `AuthController.cs` | Login 8h · token-servicio 365d · claim EsCuentaServicio |
| `EntidadController.cs` + `EntidadBLL.cs` + `EntidadDAL.cs` | CRUD + endpoint `sincronizar` (upsert por idExterno) |
| `InvitacionController.cs` + `InvitacionBLL.cs` | Resolución de `entidadEvaluadaIdExterno` → UUID |
| `CuentaUsuario*.cs` + `CuentaUsuarioDAL.cs` | Campo `esCuentaServicio` en todas las capas |
| `EncuestaPublicaResponse.cs` | Clase `SeccionPublicaResponse` + propiedad `Secciones` |
| `PublicoDAL.cs` | Query de secciones en `ObtenerEncuestaPublica` |

### Frontend

| Archivo | Descripción |
|---------|-------------|
| `src/types/api.ts` | Interfaces: `SeccionPublica`, `SeccionEncuesta`, `DimensionPregunta`, `ReglaEncuesta`, `ReglaJson`; `EncuestaPublica` con `secciones[]` |
| `src/views/ResponderView.vue` | Paginación por secciones, motor de reglas, barra de progreso, todos los tipos de pregunta |
| `src/views/ResultadosView.vue` | Estadísticas agrupadas por dimensión, carga de dimensiones |
| `src/views/EncuestaDetalleView.vue` | Gestión de secciones, dimensiones, alcance, reglas y preguntas |
| `src/views/InvitacionesView.vue` | Gestión de invitaciones + pestaña de documentación API |
| `src/views/UsuariosView.vue` | CRUD usuarios con toggle de cuenta de servicio M2M |
| `src/router/index.ts` | Ruta `/encuestas/:id/invitaciones` |

### Base de datos

```sql
-- Campo agregado a cuenta_usuario
ALTER TABLE cuenta_usuario ADD COLUMN es_cuenta_servicio BOOLEAN NOT NULL DEFAULT FALSE;

-- Índice único parcial para sincronización por idExterno
CREATE UNIQUE INDEX uq_entidad_id_externo
  ON entidad(organizacion_id, id_externo)
  WHERE id_externo IS NOT NULL;
```

---

## Permisos y multi-tenancy

- Toda operación autenticada filtra por `OrganizacionId` extraído del JWT
- `CuentaUsuario.EntidadId` vincula el usuario a su entidad (nullable para admins)
- Las cuentas de servicio acceden a todos los endpoints protegidos con `[Authorize]`
- El formulario público (`/publico/*`) no requiere autenticación, solo `tokenAcceso` válido

---

## Notas de configuración

- Backend corre en `http://localhost:5092` (launchSettings.json → profile "http")
- Frontend corre en `http://localhost:5173` (Vite dev server)
- Credenciales de demo: `admin@empresa.com` / `Admin123`
- PostgreSQL: base de datos `encuestas` en servidor local
