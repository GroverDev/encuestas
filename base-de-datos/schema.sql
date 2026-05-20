-- =========================================================
-- MOTOR DE ENCUESTAS ENTERPRISE
-- PostgreSQL — snake_case
-- =========================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- =========================================================
-- ORGANIZACION
-- Raíz del tenant. Todo dato pertenece a una organización.
-- =========================================================

CREATE TABLE organizacion (
    id        UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    nombre    VARCHAR(200) NOT NULL,
    url_logo  TEXT,
    creado_en TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  organizacion          IS 'Raíz del tenant. Cada organización es un cliente independiente del sistema.';
COMMENT ON COLUMN organizacion.id       IS 'Identificador único de la organización.';
COMMENT ON COLUMN organizacion.nombre   IS 'Nombre comercial o razón social de la organización.';
COMMENT ON COLUMN organizacion.url_logo IS 'URL pública del logotipo de la organización.';
COMMENT ON COLUMN organizacion.creado_en IS 'Fecha y hora en que se registró la organización.';

-- =========================================================
-- TIPO DE ENTIDAD
-- Catálogo de los tipos de entidad que el sistema puede
-- evaluar mediante encuestas.
-- =========================================================

CREATE TABLE tipo_entidad (
    id      UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    codigo  VARCHAR(100) UNIQUE NOT NULL,
    nombre  VARCHAR(200) NOT NULL
);

COMMENT ON TABLE  tipo_entidad        IS 'Catálogo de tipos de entidad soportados. Define qué clases de objetos pueden ser evaluados.';
COMMENT ON COLUMN tipo_entidad.id     IS 'Identificador único del tipo de entidad.';
COMMENT ON COLUMN tipo_entidad.codigo IS 'Clave técnica única del tipo (ej: EMPLEADO, SUCURSAL). Usada en lógica de aplicación.';
COMMENT ON COLUMN tipo_entidad.nombre IS 'Nombre legible del tipo de entidad.';

-- =========================================================
-- ENTIDAD
-- Registro genérico que representa cualquier objeto evaluable
-- o nodo organizacional: empresa, regional, unidad (RRHH,
-- Servicios, Atención al Cliente), empleado, cliente, etc.
--
-- La jerarquía se modela con entidad_padre_id (auto-referencia):
--   Empresa → Regional Norte → Unidad RRHH
--                            → Unidad Atención al Cliente
--            → Regional Sur  → Unidad Servicios
--
-- Para agregar un nuevo tipo basta con insertar en tipo_entidad
-- y crear registros aquí, sin modificar el modelo de datos.
-- =========================================================

CREATE TABLE entidad (
    id               UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    organizacion_id  UUID         NOT NULL REFERENCES organizacion(id),
    tipo_entidad_id  UUID         NOT NULL REFERENCES tipo_entidad(id),
    entidad_padre_id UUID         NULL REFERENCES entidad(id),
    nombre_visible   VARCHAR(300) NOT NULL,
    id_externo       VARCHAR(100),
    es_activo        BOOLEAN      NOT NULL DEFAULT TRUE,
    atributos_json   JSONB,
    creado_en        TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  entidad                  IS 'Entidad genérica evaluable y nodo organizacional. Soporta jerarquía padre-hijo para modelar Empresa → Regional → Unidad sin tablas adicionales.';
COMMENT ON COLUMN entidad.id               IS 'Identificador único de la entidad.';
COMMENT ON COLUMN entidad.organizacion_id  IS 'Organización a la que pertenece esta entidad.';
COMMENT ON COLUMN entidad.tipo_entidad_id  IS 'Tipo de entidad según el catálogo tipo_entidad (EMPRESA, REGIONAL, UNIDAD, EMPLEADO, CLIENTE, etc.).';
COMMENT ON COLUMN entidad.entidad_padre_id IS 'Entidad padre en la jerarquía organizacional. NULL indica nodo raíz. Permite modelar Empresa → Regional → Unidad de forma recursiva.';
COMMENT ON COLUMN entidad.nombre_visible   IS 'Nombre que se muestra en la interfaz y en reportes.';
COMMENT ON COLUMN entidad.id_externo       IS 'Identificador en el sistema externo de origen (RRHH, CRM, ERP). Sirve para sincronización.';
COMMENT ON COLUMN entidad.es_activo        IS 'Indica si la entidad está activa y disponible para ser evaluada.';
COMMENT ON COLUMN entidad.atributos_json   IS 'Atributos adicionales específicos del tipo (ej: {"ciudad":"Lima","region":"Sur"} para una sucursal, {"cargo":"Gerente"} para un empleado).';
COMMENT ON COLUMN entidad.creado_en        IS 'Fecha y hora de creación del registro.';

CREATE INDEX ix_entidad_organizacion_id ON entidad(organizacion_id);
CREATE INDEX ix_entidad_tipo_entidad_id ON entidad(tipo_entidad_id);
CREATE INDEX ix_entidad_entidad_padre_id ON entidad(entidad_padre_id);
CREATE INDEX ix_entidad_atributos_json  ON entidad USING GIN(atributos_json);
CREATE UNIQUE INDEX uq_entidad_id_externo ON entidad(organizacion_id, id_externo) WHERE id_externo IS NOT NULL;

-- =========================================================
-- CUENTA DE USUARIO
-- Credenciales de acceso al sistema. Puede estar vinculada
-- a cualquier entidad (ej: un empleado, un cliente).
-- =========================================================

CREATE TABLE cuenta_usuario (
    id              UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    organizacion_id UUID         NOT NULL REFERENCES organizacion(id),
    entidad_id      UUID         NULL REFERENCES entidad(id),
    correo          VARCHAR(200) UNIQUE NOT NULL,
    hash_contrasena TEXT         NOT NULL,
    es_activo           BOOLEAN      NOT NULL DEFAULT TRUE,
    es_cuenta_servicio  BOOLEAN      NOT NULL DEFAULT FALSE,
    creado_en           TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  cuenta_usuario                         IS 'Credenciales de acceso al sistema. Un usuario puede estar vinculado a cualquier tipo de entidad o a ninguna.';
COMMENT ON COLUMN cuenta_usuario.id                      IS 'Identificador único de la cuenta.';
COMMENT ON COLUMN cuenta_usuario.organizacion_id         IS 'Organización a la que pertenece la cuenta.';
COMMENT ON COLUMN cuenta_usuario.entidad_id              IS 'Entidad asociada a esta cuenta (ej: el empleado o cliente que usa este acceso). NULL si es un administrador sin entidad propia.';
COMMENT ON COLUMN cuenta_usuario.correo                  IS 'Correo electrónico usado como nombre de usuario. Único en todo el sistema.';
COMMENT ON COLUMN cuenta_usuario.hash_contrasena         IS 'Hash de la contraseña (PBKDF2). Nunca se almacena en texto plano.';
COMMENT ON COLUMN cuenta_usuario.es_activo               IS 'Indica si la cuenta está habilitada para iniciar sesión.';
COMMENT ON COLUMN cuenta_usuario.es_cuenta_servicio      IS 'TRUE = cuenta machine-to-machine. No requiere TOTP y recibe tokens de larga duración.';
COMMENT ON COLUMN cuenta_usuario.creado_en               IS 'Fecha y hora de creación de la cuenta.';

CREATE INDEX ix_cuenta_usuario_organizacion_id ON cuenta_usuario(organizacion_id);
CREATE INDEX ix_cuenta_usuario_entidad_id      ON cuenta_usuario(entidad_id);

-- =========================================================
-- ROL
-- Agrupación de permisos asignable a usuarios.
-- =========================================================

CREATE TABLE rol (
    id              UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    organizacion_id UUID         NOT NULL REFERENCES organizacion(id),
    nombre          VARCHAR(100) NOT NULL
);

COMMENT ON TABLE  rol                IS 'Rol de acceso dentro de una organización. Agrupa permisos asignables a usuarios.';
COMMENT ON COLUMN rol.id             IS 'Identificador único del rol.';
COMMENT ON COLUMN rol.organizacion_id IS 'Organización propietaria del rol.';
COMMENT ON COLUMN rol.nombre         IS 'Nombre descriptivo del rol (ej: Administrador, Analista, Respondente).';

-- =========================================================
-- PERMISO
-- Acción específica que puede realizarse en el sistema.
-- =========================================================

CREATE TABLE permiso (
    id      UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    codigo  VARCHAR(100) UNIQUE NOT NULL,
    nombre  VARCHAR(200) NOT NULL
);

COMMENT ON TABLE  permiso        IS 'Catálogo de permisos del sistema. Cada permiso representa una acción específica.';
COMMENT ON COLUMN permiso.id     IS 'Identificador único del permiso.';
COMMENT ON COLUMN permiso.codigo IS 'Clave técnica del permiso (ej: encuesta.publicar, reporte.exportar). Usada en validaciones de código.';
COMMENT ON COLUMN permiso.nombre IS 'Nombre legible del permiso.';

-- =========================================================
-- USUARIO ROL
-- =========================================================

CREATE TABLE usuario_rol (
    usuario_id UUID NOT NULL REFERENCES cuenta_usuario(id),
    rol_id     UUID NOT NULL REFERENCES rol(id),
    PRIMARY KEY (usuario_id, rol_id)
);

COMMENT ON TABLE  usuario_rol           IS 'Asignación de roles a usuarios. Un usuario puede tener múltiples roles.';
COMMENT ON COLUMN usuario_rol.usuario_id IS 'Usuario al que se le asigna el rol.';
COMMENT ON COLUMN usuario_rol.rol_id    IS 'Rol asignado al usuario.';

-- =========================================================
-- ROL PERMISO
-- =========================================================

CREATE TABLE rol_permiso (
    rol_id     UUID NOT NULL REFERENCES rol(id),
    permiso_id UUID NOT NULL REFERENCES permiso(id),
    PRIMARY KEY (rol_id, permiso_id)
);

COMMENT ON TABLE  rol_permiso           IS 'Asignación de permisos a roles. Define qué acciones puede realizar cada rol.';
COMMENT ON COLUMN rol_permiso.rol_id    IS 'Rol al que se le asigna el permiso.';
COMMENT ON COLUMN rol_permiso.permiso_id IS 'Permiso asignado al rol.';

-- =========================================================
-- ENCUESTA
-- =========================================================

CREATE TABLE encuesta (
    id                    UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    organizacion_id       UUID         NOT NULL REFERENCES organizacion(id),
    titulo                VARCHAR(300) NOT NULL,
    descripcion           TEXT,
    version               INTEGER      NOT NULL DEFAULT 1,
    estado                VARCHAR(50)  NOT NULL DEFAULT 'BORRADOR',
    es_global             BOOLEAN      NOT NULL DEFAULT FALSE,
    es_plantilla          BOOLEAN      NOT NULL DEFAULT FALSE,
    plantilla_origen_id   UUID         NULL REFERENCES encuesta(id),
    etiquetas_json        JSONB,
    creado_por_usuario_id UUID         NULL REFERENCES cuenta_usuario(id),
    fecha_inicio          TIMESTAMPTZ NULL,
    fecha_fin             TIMESTAMPTZ NULL,
    publicado_en          TIMESTAMPTZ NULL,
    configuracion_json    JSONB,
    creado_en             TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  encuesta                        IS 'Encuesta creada por una organización. Ciclo de vida: BORRADOR → PUBLICADA → CERRADA.';
COMMENT ON COLUMN encuesta.id                     IS 'Identificador único de la encuesta.';
COMMENT ON COLUMN encuesta.organizacion_id        IS 'Organización propietaria de la encuesta.';
COMMENT ON COLUMN encuesta.titulo                 IS 'Título visible de la encuesta para respondentes y administradores.';
COMMENT ON COLUMN encuesta.descripcion            IS 'Descripción o instrucciones generales mostradas al inicio de la encuesta.';
COMMENT ON COLUMN encuesta.version                IS 'Versión estructural. Se incrementa cuando se modifican preguntas de una encuesta ya publicada para mantener integridad histórica.';
COMMENT ON COLUMN encuesta.estado                 IS 'Estado del ciclo de vida: BORRADOR, PUBLICADA, CERRADA.';
COMMENT ON COLUMN encuesta.es_global              IS 'TRUE = encuesta aplica a toda la organización sin filtro de alcance. FALSE = el alcance se define en alcance_encuesta.';
COMMENT ON COLUMN encuesta.es_plantilla           IS 'TRUE = esta encuesta es una plantilla reutilizable, no se distribuye directamente.';
COMMENT ON COLUMN encuesta.plantilla_origen_id    IS 'Encuesta plantilla de la que se originó esta por clonación. NULL si fue creada desde cero.';
COMMENT ON COLUMN encuesta.etiquetas_json         IS 'Etiquetas para categorización y filtrado. Ej: ["nps","clima-laboral","2024-q1"].';
COMMENT ON COLUMN encuesta.creado_por_usuario_id  IS 'Usuario que creó la encuesta.';
COMMENT ON COLUMN encuesta.fecha_inicio           IS 'Fecha desde la que la encuesta acepta respuestas. NULL si no tiene restricción de inicio.';
COMMENT ON COLUMN encuesta.fecha_fin              IS 'Fecha hasta la que la encuesta acepta respuestas. NULL si no tiene fecha de cierre automático.';
COMMENT ON COLUMN encuesta.publicado_en           IS 'Fecha y hora en que la encuesta fue publicada. NULL si aún no se ha publicado.';
COMMENT ON COLUMN encuesta.configuracion_json     IS 'Configuración de comportamiento: {"esAnonima":false,"permitirMultiplesRespuestas":false,"mostrarProgreso":true,"modoNavegacion":"lineal"}.';
COMMENT ON COLUMN encuesta.creado_en              IS 'Fecha y hora de creación de la encuesta.';

CREATE INDEX ix_encuesta_organizacion_id  ON encuesta(organizacion_id);
CREATE INDEX ix_encuesta_estado           ON encuesta(estado);
CREATE INDEX ix_encuesta_fecha_inicio     ON encuesta(fecha_inicio);
CREATE INDEX ix_encuesta_fecha_fin        ON encuesta(fecha_fin);
CREATE INDEX ix_encuesta_es_plantilla     ON encuesta(es_plantilla);
CREATE INDEX ix_encuesta_plantilla_origen ON encuesta(plantilla_origen_id);
CREATE INDEX ix_encuesta_etiquetas_json   ON encuesta USING GIN(etiquetas_json);

-- =========================================================
-- SECCION DE ENCUESTA
-- Agrupa preguntas en bloques o páginas dentro de una encuesta.
-- =========================================================

CREATE TABLE seccion_encuesta (
    id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    encuesta_id UUID         NOT NULL REFERENCES encuesta(id),
    titulo      VARCHAR(300) NOT NULL,
    descripcion TEXT,
    orden       INTEGER      NOT NULL
);

COMMENT ON TABLE  seccion_encuesta            IS 'Sección o página dentro de una encuesta. Permite organizar preguntas en bloques y habilitar paginación.';
COMMENT ON COLUMN seccion_encuesta.id         IS 'Identificador único de la sección.';
COMMENT ON COLUMN seccion_encuesta.encuesta_id IS 'Encuesta a la que pertenece esta sección.';
COMMENT ON COLUMN seccion_encuesta.titulo     IS 'Título visible de la sección (ej: "Datos generales", "Evaluación de desempeño").';
COMMENT ON COLUMN seccion_encuesta.descripcion IS 'Instrucción o contexto mostrado al inicio de la sección.';
COMMENT ON COLUMN seccion_encuesta.orden      IS 'Posición de la sección dentro de la encuesta.';

CREATE INDEX ix_seccion_encuesta_encuesta_id ON seccion_encuesta(encuesta_id);

-- =========================================================
-- DIMENSION DE PREGUNTA
-- Eje temático para agrupar preguntas con fines estadísticos.
-- =========================================================

CREATE TABLE dimension_pregunta (
    id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    encuesta_id UUID         NOT NULL REFERENCES encuesta(id),
    nombre      VARCHAR(200) NOT NULL,
    descripcion TEXT,
    peso        NUMERIC      NOT NULL DEFAULT 1.0,
    orden       INTEGER      NOT NULL
);

COMMENT ON TABLE  dimension_pregunta            IS 'Eje temático para análisis estadístico. Agrupa preguntas para calcular puntajes por área (ej: Liderazgo, Clima Laboral). Diferente de seccion_encuesta, que es para navegación visual.';
COMMENT ON COLUMN dimension_pregunta.id         IS 'Identificador único de la dimensión.';
COMMENT ON COLUMN dimension_pregunta.encuesta_id IS 'Encuesta a la que pertenece esta dimensión.';
COMMENT ON COLUMN dimension_pregunta.nombre     IS 'Nombre del eje temático (ej: "Liderazgo", "Comunicación", "Satisfacción").';
COMMENT ON COLUMN dimension_pregunta.descripcion IS 'Descripción del eje y qué aspectos mide.';
COMMENT ON COLUMN dimension_pregunta.peso       IS 'Peso relativo de la dimensión en el índice compuesto. La suma de pesos debe ser 1.0 para índices porcentuales.';
COMMENT ON COLUMN dimension_pregunta.orden      IS 'Orden de presentación en reportes y dashboards.';

CREATE INDEX ix_dimension_pregunta_encuesta_id ON dimension_pregunta(encuesta_id);

-- =========================================================
-- COLABORADOR DE ENCUESTA
-- =========================================================

CREATE TABLE colaborador_encuesta (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    encuesta_id UUID        NOT NULL REFERENCES encuesta(id),
    usuario_id  UUID        NOT NULL REFERENCES cuenta_usuario(id),
    rol         VARCHAR(50) NOT NULL DEFAULT 'EDITOR',
    invitado_en TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_colaborador_encuesta UNIQUE (encuesta_id, usuario_id)
);

COMMENT ON TABLE  colaborador_encuesta            IS 'Usuarios con acceso de edición o revisión a una encuesta específica.';
COMMENT ON COLUMN colaborador_encuesta.id         IS 'Identificador único del registro de colaboración.';
COMMENT ON COLUMN colaborador_encuesta.encuesta_id IS 'Encuesta compartida.';
COMMENT ON COLUMN colaborador_encuesta.usuario_id IS 'Usuario con acceso colaborativo.';
COMMENT ON COLUMN colaborador_encuesta.rol        IS 'Nivel de acceso: EDITOR (puede modificar), REVISOR (solo lectura), APROBADOR (puede publicar).';
COMMENT ON COLUMN colaborador_encuesta.invitado_en IS 'Fecha en que se otorgó el acceso colaborativo.';

CREATE INDEX ix_colaborador_encuesta_encuesta_id ON colaborador_encuesta(encuesta_id);
CREATE INDEX ix_colaborador_encuesta_usuario_id  ON colaborador_encuesta(usuario_id);

-- =========================================================
-- CUOTA DE RESPUESTA
-- =========================================================

CREATE TABLE cuota_respuesta (
    id                 UUID    PRIMARY KEY DEFAULT gen_random_uuid(),
    encuesta_id        UUID    NOT NULL REFERENCES encuesta(id),
    entidad_id         UUID    NULL REFERENCES entidad(id),
    limite             INTEGER NOT NULL,
    total_actual       INTEGER NOT NULL DEFAULT 0,
    cerrar_al_alcanzar BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE  cuota_respuesta                   IS 'Define un límite máximo de respuestas por segmento de entidad.';
COMMENT ON COLUMN cuota_respuesta.id                IS 'Identificador único de la cuota.';
COMMENT ON COLUMN cuota_respuesta.encuesta_id       IS 'Encuesta a la que aplica esta cuota.';
COMMENT ON COLUMN cuota_respuesta.entidad_id        IS 'Segmento al que aplica la cuota. NULL = cuota global de toda la encuesta.';
COMMENT ON COLUMN cuota_respuesta.limite            IS 'Número máximo de respuestas aceptadas para este segmento.';
COMMENT ON COLUMN cuota_respuesta.total_actual      IS 'Contador de respuestas completadas. Actualizado atómicamente al registrar cada respuesta.';
COMMENT ON COLUMN cuota_respuesta.cerrar_al_alcanzar IS 'TRUE = dejar de aceptar respuestas al alcanzar el límite. FALSE = solo alertar.';

CREATE INDEX ix_cuota_respuesta_encuesta_id ON cuota_respuesta(encuesta_id);
CREATE INDEX ix_cuota_respuesta_entidad_id  ON cuota_respuesta(entidad_id);

-- =========================================================
-- ALCANCE DE ENCUESTA
-- =========================================================

CREATE TABLE alcance_encuesta (
    id                   UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    encuesta_id          UUID         NOT NULL REFERENCES encuesta(id),
    entidad_id           UUID         NOT NULL REFERENCES entidad(id),
    tipo_relacion        VARCHAR(100) NOT NULL DEFAULT 'AMBITO',
    incluir_descendientes BOOLEAN     NOT NULL DEFAULT FALSE
);

COMMENT ON TABLE  alcance_encuesta                       IS 'Define el alcance de la encuesta cuando es_global = FALSE. Determina a qué entidades o jerarquías aplica.';
COMMENT ON COLUMN alcance_encuesta.id                    IS 'Identificador único del alcance.';
COMMENT ON COLUMN alcance_encuesta.encuesta_id           IS 'Encuesta que define el alcance.';
COMMENT ON COLUMN alcance_encuesta.entidad_id            IS 'Entidad o nodo organizacional incluido en el alcance.';
COMMENT ON COLUMN alcance_encuesta.tipo_relacion         IS 'Rol del nodo: AMBITO (delimita quiénes responden), SUJETO (el nodo es el evaluado), CONTEXTO (referencia sin ser evaluado).';
COMMENT ON COLUMN alcance_encuesta.incluir_descendientes IS 'TRUE = aplica también a todas las entidades hijas del nodo en la jerarquía.';

CREATE INDEX ix_alcance_encuesta_encuesta_id ON alcance_encuesta(encuesta_id);
CREATE INDEX ix_alcance_encuesta_entidad_id  ON alcance_encuesta(entidad_id);

-- =========================================================
-- PREGUNTA
-- =========================================================

CREATE TABLE pregunta (
    id                 UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    encuesta_id        UUID         NOT NULL REFERENCES encuesta(id),
    seccion_id         UUID         NULL REFERENCES seccion_encuesta(id),
    dimension_id       UUID         NULL REFERENCES dimension_pregunta(id),
    tipo               VARCHAR(100) NOT NULL,
    titulo             TEXT         NOT NULL,
    descripcion        TEXT,
    orden              INTEGER      NOT NULL,
    peso               NUMERIC      NOT NULL DEFAULT 1.0,
    es_obligatoria     BOOLEAN      NOT NULL DEFAULT FALSE,
    configuracion_json JSONB,
    creado_en          TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  pregunta                   IS 'Pregunta perteneciente a una encuesta. El tipo determina el componente de entrada y la columna de valor en detalle_respuesta.';
COMMENT ON COLUMN pregunta.id                IS 'Identificador único de la pregunta.';
COMMENT ON COLUMN pregunta.encuesta_id       IS 'Encuesta a la que pertenece esta pregunta.';
COMMENT ON COLUMN pregunta.seccion_id        IS 'Sección a la que pertenece esta pregunta. NULL si la encuesta no usa secciones.';
COMMENT ON COLUMN pregunta.dimension_id      IS 'Dimensión temática para análisis estadístico. NULL si la pregunta no forma parte de ningún eje analítico.';
COMMENT ON COLUMN pregunta.tipo              IS 'Tipo de pregunta: TEXTO, NUMERO, SELECCION_UNICA, SELECCION_MULTIPLE, ESCALA, FECHA, BOOLEANO, NPS, CALIFICACION, RANKING, MATRIZ.';
COMMENT ON COLUMN pregunta.titulo            IS 'Texto de la pregunta mostrado al respondente.';
COMMENT ON COLUMN pregunta.descripcion       IS 'Instrucción o aclaración adicional debajo del título.';
COMMENT ON COLUMN pregunta.orden             IS 'Posición de la pregunta dentro de la sección (o encuesta si no hay secciones).';
COMMENT ON COLUMN pregunta.peso              IS 'Peso de la pregunta dentro de su dimensión para el cálculo del puntaje.';
COMMENT ON COLUMN pregunta.es_obligatoria    IS 'Indica si la pregunta debe ser respondida para poder avanzar o enviar.';
COMMENT ON COLUMN pregunta.configuracion_json IS 'Configuración específica del tipo (ej: {"min":1,"max":5,"etiquetaMin":"Muy malo","etiquetaMax":"Excelente"}).';
COMMENT ON COLUMN pregunta.creado_en         IS 'Fecha y hora de creación de la pregunta.';

CREATE INDEX ix_pregunta_encuesta_id         ON pregunta(encuesta_id);
CREATE INDEX ix_pregunta_seccion_id          ON pregunta(seccion_id);
CREATE INDEX ix_pregunta_dimension_id        ON pregunta(dimension_id);
CREATE INDEX ix_pregunta_orden               ON pregunta(orden);
CREATE INDEX ix_pregunta_configuracion_json  ON pregunta USING GIN(configuracion_json);

-- =========================================================
-- OPCION DE PREGUNTA
-- =========================================================

CREATE TABLE opcion_pregunta (
    id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    pregunta_id UUID         NOT NULL REFERENCES pregunta(id),
    etiqueta    VARCHAR(300) NOT NULL,
    valor       VARCHAR(300) NOT NULL,
    puntaje     NUMERIC      NULL,
    orden       INTEGER      NOT NULL
);

COMMENT ON TABLE  opcion_pregunta           IS 'Opciones de respuesta para preguntas SELECCION_UNICA o SELECCION_MULTIPLE.';
COMMENT ON COLUMN opcion_pregunta.id        IS 'Identificador único de la opción.';
COMMENT ON COLUMN opcion_pregunta.pregunta_id IS 'Pregunta a la que pertenece esta opción.';
COMMENT ON COLUMN opcion_pregunta.etiqueta  IS 'Texto visible de la opción para el respondente.';
COMMENT ON COLUMN opcion_pregunta.valor     IS 'Valor interno almacenado cuando se selecciona esta opción.';
COMMENT ON COLUMN opcion_pregunta.puntaje   IS 'Puntaje numérico para cálculos estadísticos (ej: Likert: Muy de acuerdo=5). NULL si la pregunta no tiene puntaje.';
COMMENT ON COLUMN opcion_pregunta.orden     IS 'Posición de la opción dentro de la lista.';

CREATE INDEX ix_opcion_pregunta_pregunta_id ON opcion_pregunta(pregunta_id);

-- =========================================================
-- REGLA DE ENCUESTA
-- =========================================================

CREATE TABLE regla_encuesta (
    id          UUID  PRIMARY KEY DEFAULT gen_random_uuid(),
    encuesta_id UUID  NOT NULL REFERENCES encuesta(id),
    regla_json  JSONB NOT NULL
);

COMMENT ON TABLE  regla_encuesta            IS 'Reglas de lógica condicional de la encuesta. Evaluadas en tiempo de ejecución al navegar entre preguntas.';
COMMENT ON COLUMN regla_encuesta.id         IS 'Identificador único de la regla.';
COMMENT ON COLUMN regla_encuesta.encuesta_id IS 'Encuesta a la que aplica esta regla.';
COMMENT ON COLUMN regla_encuesta.regla_json IS 'Regla en JSON: {"si":{"preguntaId":"...","operador":"igual","valor":"SI"},"entonces":{"accion":"mostrar","preguntaObjetivoId":"..."}}. Operadores: igual|distinto|mayor|menor|contiene. Acciones: mostrar|ocultar|saltar.';

CREATE INDEX ix_regla_encuesta_encuesta_id ON regla_encuesta(encuesta_id);
CREATE INDEX ix_regla_encuesta_regla_json  ON regla_encuesta USING GIN(regla_json);

-- =========================================================
-- INVITACION
-- (debe ir antes de notificacion_envio, que la referencia)
-- =========================================================

CREATE TABLE invitacion (
    id                  UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    encuesta_id         UUID         NOT NULL REFERENCES encuesta(id),
    cuenta_usuario_id   UUID         NULL REFERENCES cuenta_usuario(id),
    correo_destino      VARCHAR(200) NULL,
    entidad_evaluada_id UUID         NULL REFERENCES entidad(id),
    token_acceso        UUID         NOT NULL DEFAULT gen_random_uuid(),
    canal               VARCHAR(50)  NOT NULL DEFAULT 'EMAIL',
    estado              VARCHAR(50)  NOT NULL DEFAULT 'PENDIENTE',
    enviado_en          TIMESTAMPTZ NULL,
    vence_en            TIMESTAMPTZ NULL,
    respondido_en       TIMESTAMPTZ NULL,

    CONSTRAINT ck_invitacion_destinatario
        CHECK (cuenta_usuario_id IS NOT NULL OR correo_destino IS NOT NULL OR canal IN ('ENLACE_PUBLICO', 'QR', 'KIOSCO'))
);

COMMENT ON TABLE  invitacion                     IS 'Gestión de distribución de la encuesta. Registra cada invitación enviada, su canal, estado y token de acceso único.';
COMMENT ON COLUMN invitacion.id                  IS 'Identificador único de la invitación.';
COMMENT ON COLUMN invitacion.encuesta_id         IS 'Encuesta a la que corresponde la invitación.';
COMMENT ON COLUMN invitacion.cuenta_usuario_id   IS 'Usuario invitado con cuenta en el sistema. NULL si se invita por correo sin cuenta.';
COMMENT ON COLUMN invitacion.correo_destino      IS 'Correo del destinatario cuando no tiene cuenta. Al menos uno entre cuenta_usuario_id y correo_destino debe estar presente.';
COMMENT ON COLUMN invitacion.entidad_evaluada_id IS 'Entidad que este respondente debe evaluar. NULL si la encuesta es general.';
COMMENT ON COLUMN invitacion.token_acceso        IS 'Token UUID único para acceso mediante enlace. Permite responder sin iniciar sesión.';
COMMENT ON COLUMN invitacion.canal               IS 'Canal de distribución: EMAIL, SMS, QR, ENLACE_PUBLICO, KIOSCO, API.';
COMMENT ON COLUMN invitacion.estado              IS 'Estado: PENDIENTE, RESPONDIDA, EXPIRADA, CANCELADA.';
COMMENT ON COLUMN invitacion.enviado_en          IS 'Fecha y hora en que se envió la invitación al destinatario.';
COMMENT ON COLUMN invitacion.vence_en            IS 'Fecha de expiración del token. NULL si no tiene vencimiento propio.';
COMMENT ON COLUMN invitacion.respondido_en       IS 'Fecha y hora en que el destinatario completó la encuesta.';

CREATE UNIQUE INDEX ix_invitacion_token_acceso    ON invitacion(token_acceso);
CREATE INDEX       ix_invitacion_encuesta_id      ON invitacion(encuesta_id);
CREATE INDEX       ix_invitacion_cuenta_usuario_id ON invitacion(cuenta_usuario_id);
CREATE INDEX       ix_invitacion_estado           ON invitacion(estado);

-- =========================================================
-- NOTIFICACION ENVIO
-- Historial de notificaciones enviadas. Append-only.
-- =========================================================

CREATE TABLE notificacion_envio (
    id            UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    invitacion_id UUID         NOT NULL REFERENCES invitacion(id),
    tipo          VARCHAR(50)  NOT NULL,
    canal         VARCHAR(50)  NOT NULL,
    destinatario  VARCHAR(200) NOT NULL,
    estado        VARCHAR(50)  NOT NULL DEFAULT 'PENDIENTE',
    intentos_envio INTEGER     NOT NULL DEFAULT 0,
    enviado_en    TIMESTAMPTZ NULL,
    entregado_en  TIMESTAMPTZ NULL,
    error_detalle TEXT         NULL,
    creado_en     TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  notificacion_envio               IS 'Historial de todos los mensajes enviados en el proceso de distribución (invitaciones, recordatorios, confirmaciones). Append-only.';
COMMENT ON COLUMN notificacion_envio.id            IS 'Identificador único del envío.';
COMMENT ON COLUMN notificacion_envio.invitacion_id IS 'Invitación a la que corresponde este envío.';
COMMENT ON COLUMN notificacion_envio.tipo          IS 'Tipo de mensaje: INVITACION, RECORDATORIO, CONFIRMACION, AGRADECIMIENTO.';
COMMENT ON COLUMN notificacion_envio.canal         IS 'Canal de envío: EMAIL, SMS.';
COMMENT ON COLUMN notificacion_envio.destinatario  IS 'Dirección de destino (correo o número de teléfono).';
COMMENT ON COLUMN notificacion_envio.estado        IS 'Estado del envío: PENDIENTE, ENVIADO, ENTREGADO, FALLIDO, REBOTADO.';
COMMENT ON COLUMN notificacion_envio.intentos_envio IS 'Número de intentos de envío realizados.';
COMMENT ON COLUMN notificacion_envio.enviado_en    IS 'Fecha y hora en que el mensaje salió del sistema.';
COMMENT ON COLUMN notificacion_envio.entregado_en  IS 'Fecha y hora de confirmación de entrega (webhook del proveedor). NULL si no hay confirmación.';
COMMENT ON COLUMN notificacion_envio.error_detalle IS 'Detalle del error en caso de fallo. NULL si el envío fue exitoso.';
COMMENT ON COLUMN notificacion_envio.creado_en     IS 'Fecha y hora en que se generó la notificación.';

CREATE INDEX ix_notificacion_envio_invitacion_id ON notificacion_envio(invitacion_id);
CREATE INDEX ix_notificacion_envio_estado        ON notificacion_envio(estado);
CREATE INDEX ix_notificacion_envio_tipo          ON notificacion_envio(tipo);

-- =========================================================
-- RESPUESTA
-- =========================================================

CREATE TABLE respuesta (
    id                      UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    encuesta_id             UUID         NOT NULL REFERENCES encuesta(id),
    version_encuesta        INTEGER      NOT NULL DEFAULT 1,
    invitacion_id           UUID         NULL REFERENCES invitacion(id),
    usuario_respondent_id   UUID         NULL REFERENCES cuenta_usuario(id),
    canal                   VARCHAR(50)  NULL,
    ultima_pregunta_id      UUID         NULL REFERENCES pregunta(id),
    peso_estadistico        NUMERIC      NOT NULL DEFAULT 1.0,
    consentimiento_otorgado BOOLEAN      NULL,
    fecha_consentimiento    TIMESTAMPTZ NULL,
    iniciado_en             TIMESTAMPTZ,
    completado_en           TIMESTAMPTZ,
    info_dispositivo        TEXT,
    direccion_ip            VARCHAR(100)
);

COMMENT ON TABLE  respuesta                            IS 'Sesión de respuesta a una encuesta. Representa un intento completo o parcial de un respondente.';
COMMENT ON COLUMN respuesta.id                         IS 'Identificador único de la sesión de respuesta.';
COMMENT ON COLUMN respuesta.encuesta_id                IS 'Encuesta que se está respondiendo.';
COMMENT ON COLUMN respuesta.version_encuesta           IS 'Versión de la encuesta al momento de responder. Garantiza integridad histórica.';
COMMENT ON COLUMN respuesta.invitacion_id              IS 'Invitación que originó esta respuesta. NULL si fue por enlace público o acceso directo.';
COMMENT ON COLUMN respuesta.usuario_respondent_id      IS 'Usuario que responde. NULL para encuestas anónimas.';
COMMENT ON COLUMN respuesta.canal                      IS 'Canal por el que llegó la respuesta: EMAIL, SMS, QR, ENLACE_PUBLICO, KIOSCO, API.';
COMMENT ON COLUMN respuesta.ultima_pregunta_id         IS 'Última pregunta vista o respondida. Permite análisis de abandono.';
COMMENT ON COLUMN respuesta.peso_estadistico           IS 'Factor de ponderación para análisis estadístico ponderado. DEFAULT 1.0 = sin ponderación.';
COMMENT ON COLUMN respuesta.consentimiento_otorgado    IS 'El respondente aceptó el aviso de privacidad/GDPR. NULL si la encuesta no requirió consentimiento explícito.';
COMMENT ON COLUMN respuesta.fecha_consentimiento       IS 'Fecha y hora en que se otorgó el consentimiento.';
COMMENT ON COLUMN respuesta.iniciado_en                IS 'Fecha y hora en que el respondente comenzó la encuesta.';
COMMENT ON COLUMN respuesta.completado_en              IS 'Fecha y hora en que se envió la encuesta. NULL si está en progreso o fue abandonada.';
COMMENT ON COLUMN respuesta.info_dispositivo           IS 'User-agent del dispositivo usado. Para auditoría.';
COMMENT ON COLUMN respuesta.direccion_ip               IS 'Dirección IP del respondente al momento del envío. Para auditoría.';

CREATE INDEX ix_respuesta_encuesta_id           ON respuesta(encuesta_id);
CREATE INDEX ix_respuesta_invitacion_id         ON respuesta(invitacion_id);
CREATE INDEX ix_respuesta_usuario_respondent_id ON respuesta(usuario_respondent_id);
CREATE INDEX ix_respuesta_completado_en         ON respuesta(completado_en);
CREATE INDEX ix_respuesta_canal                 ON respuesta(canal);
CREATE INDEX ix_respuesta_ultima_pregunta_id    ON respuesta(ultima_pregunta_id);

-- =========================================================
-- OBJETIVO DE RESPUESTA
-- =========================================================

CREATE TABLE objetivo_respuesta (
    id            UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    respuesta_id  UUID         NOT NULL REFERENCES respuesta(id),
    entidad_id    UUID         NOT NULL REFERENCES entidad(id),
    tipo_relacion VARCHAR(100) NOT NULL
);

COMMENT ON TABLE  objetivo_respuesta              IS 'Entidad(es) evaluadas en una sesión de respuesta. Permite evaluar múltiples entidades en un mismo envío.';
COMMENT ON COLUMN objetivo_respuesta.id           IS 'Identificador único del objetivo.';
COMMENT ON COLUMN objetivo_respuesta.respuesta_id IS 'Sesión de respuesta a la que pertenece este objetivo.';
COMMENT ON COLUMN objetivo_respuesta.entidad_id   IS 'Entidad que fue evaluada.';
COMMENT ON COLUMN objetivo_respuesta.tipo_relacion IS 'Rol de la entidad evaluada: EVALUADO (sujeto principal), CONTEXTO, EVENTO, etc.';

CREATE INDEX ix_objetivo_respuesta_respuesta_id ON objetivo_respuesta(respuesta_id);
CREATE INDEX ix_objetivo_respuesta_entidad_id   ON objetivo_respuesta(entidad_id);

-- =========================================================
-- DETALLE DE RESPUESTA
-- =========================================================

CREATE TABLE detalle_respuesta (
    id             UUID      PRIMARY KEY DEFAULT gen_random_uuid(),
    respuesta_id   UUID      NOT NULL REFERENCES respuesta(id),
    pregunta_id    UUID      NOT NULL REFERENCES pregunta(id),
    valor_texto    TEXT,
    valor_numero   NUMERIC,
    valor_booleano BOOLEAN,
    valor_fecha    TIMESTAMPTZ,
    valor_json     JSONB
);

COMMENT ON TABLE  detalle_respuesta               IS 'Respuesta individual a una pregunta. Se llena solo la columna de valor que corresponde al tipo de la pregunta.';
COMMENT ON COLUMN detalle_respuesta.id            IS 'Identificador único del detalle.';
COMMENT ON COLUMN detalle_respuesta.respuesta_id  IS 'Sesión de respuesta a la que pertenece este detalle.';
COMMENT ON COLUMN detalle_respuesta.pregunta_id   IS 'Pregunta que fue respondida.';
COMMENT ON COLUMN detalle_respuesta.valor_texto   IS 'Respuesta en texto. Usado para TEXTO y SELECCION_UNICA (almacena el valor de opcion_pregunta).';
COMMENT ON COLUMN detalle_respuesta.valor_numero  IS 'Respuesta numérica. Usado para NUMERO, ESCALA, NPS y CALIFICACION.';
COMMENT ON COLUMN detalle_respuesta.valor_booleano IS 'Respuesta booleana. Usado para BOOLEANO (sí/no).';
COMMENT ON COLUMN detalle_respuesta.valor_fecha   IS 'Respuesta de fecha/hora. Usado para FECHA.';
COMMENT ON COLUMN detalle_respuesta.valor_json    IS 'Respuesta en JSON. Usado para SELECCION_MULTIPLE, RANKING y MATRIZ.';

CREATE INDEX ix_detalle_respuesta_respuesta_id ON detalle_respuesta(respuesta_id);
CREATE INDEX ix_detalle_respuesta_pregunta_id  ON detalle_respuesta(pregunta_id);
CREATE INDEX ix_detalle_respuesta_valor_json   ON detalle_respuesta USING GIN(valor_json);

-- =========================================================
-- ADJUNTO
-- =========================================================

CREATE TABLE adjunto (
    id              UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    entidad_id      UUID         NOT NULL REFERENCES entidad(id),
    nombre_archivo  VARCHAR(300),
    url_archivo     TEXT         NOT NULL,
    subido_en       TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  adjunto               IS 'Archivos adjuntos vinculados a una entidad del sistema.';
COMMENT ON COLUMN adjunto.id            IS 'Identificador único del adjunto.';
COMMENT ON COLUMN adjunto.entidad_id    IS 'Entidad a la que pertenece el adjunto.';
COMMENT ON COLUMN adjunto.nombre_archivo IS 'Nombre original del archivo subido.';
COMMENT ON COLUMN adjunto.url_archivo   IS 'URL de acceso al archivo en el sistema de almacenamiento.';
COMMENT ON COLUMN adjunto.subido_en     IS 'Fecha y hora en que se subió el archivo.';

CREATE INDEX ix_adjunto_entidad_id ON adjunto(entidad_id);

-- =========================================================
-- COMENTARIO
-- =========================================================

CREATE TABLE comentario (
    id               UUID      PRIMARY KEY DEFAULT gen_random_uuid(),
    entidad_id       UUID      NULL REFERENCES entidad(id),
    respuesta_id     UUID      NULL REFERENCES respuesta(id),
    usuario_id       UUID      NULL REFERENCES cuenta_usuario(id),
    texto_comentario TEXT      NOT NULL,
    creado_en        TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT ck_comentario_objetivo
        CHECK (entidad_id IS NOT NULL OR respuesta_id IS NOT NULL)
);

COMMENT ON TABLE  comentario                  IS 'Anotaciones sobre entidades o respuestas. Permite comentar perfiles de entidades o anotar observaciones analíticas sobre respuestas.';
COMMENT ON COLUMN comentario.id               IS 'Identificador único del comentario.';
COMMENT ON COLUMN comentario.entidad_id       IS 'Entidad sobre la que se comenta. NULL si el comentario es sobre una respuesta.';
COMMENT ON COLUMN comentario.respuesta_id     IS 'Respuesta sobre la que se anota. NULL si el comentario es sobre una entidad. Al menos uno debe estar presente.';
COMMENT ON COLUMN comentario.usuario_id       IS 'Usuario que escribió el comentario. NULL si fue generado por el sistema.';
COMMENT ON COLUMN comentario.texto_comentario IS 'Contenido del comentario.';
COMMENT ON COLUMN comentario.creado_en        IS 'Fecha y hora en que se escribió el comentario.';

CREATE INDEX ix_comentario_entidad_id   ON comentario(entidad_id);
CREATE INDEX ix_comentario_respuesta_id ON comentario(respuesta_id);

-- =========================================================
-- DATOS INICIALES - TIPOS DE ENTIDAD
-- =========================================================

INSERT INTO tipo_entidad (codigo, nombre) VALUES
    ('EMPRESA',    'Empresa'),
    ('REGIONAL',   'Regional'),
    ('UNIDAD',     'Unidad'),
    ('EMPLEADO',   'Empleado'),
    ('CLIENTE',    'Cliente'),
    ('PROVEEDOR',  'Proveedor'),
    ('PRODUCTO',   'Producto'),
    ('SERVICIO',   'Servicio'),
    ('APLICACION', 'Aplicación'),
    ('PROYECTO',   'Proyecto'),
    ('EVENTO',     'Evento');

-- =========================================================
-- DATOS INICIALES - PERMISOS
-- =========================================================

INSERT INTO permiso (codigo, nombre) VALUES
    ('encuesta.ver',            'Ver encuestas'),
    ('encuesta.crear',          'Crear encuestas'),
    ('encuesta.editar',         'Editar encuestas'),
    ('encuesta.publicar',       'Publicar encuestas'),
    ('encuesta.cerrar',         'Cerrar encuestas'),
    ('encuesta.eliminar',       'Eliminar encuestas'),
    ('encuesta.plantilla.usar', 'Usar plantillas de encuesta'),
    ('encuesta.colaborar',      'Colaborar en encuestas compartidas'),
    ('invitacion.administrar',  'Administrar invitaciones'),
    ('invitacion.recordatorio', 'Enviar recordatorios'),
    ('respuesta.ver',           'Ver respuestas'),
    ('respuesta.anotar',        'Anotar respuestas'),
    ('reporte.ver',             'Ver reportes'),
    ('reporte.exportar',        'Exportar reportes'),
    ('reporte.estadistico',     'Ver reportes estadísticos avanzados'),
    ('cuota.administrar',       'Administrar cuotas de respuesta'),
    ('usuario.administrar',     'Administrar usuarios'),
    ('entidad.administrar',     'Administrar entidades');

-- =========================================================
-- REFERENCIA: TIPOS DE PREGUNTA → COLUMNA DE VALOR
-- =========================================================
--
-- TEXTO              → valor_texto
-- NUMERO             → valor_numero     {"min":0,"max":1000,"decimales":2}
-- BOOLEANO           → valor_booleano   {"etiquetaTrue":"Sí","etiquetaFalse":"No"}
-- SELECCION_UNICA    → valor_texto      (almacena opcion_pregunta.valor; incluye Likert)
-- SELECCION_MULTIPLE → valor_json       ["op1","op2"]
-- ESCALA             → valor_numero     {"min":1,"max":5,"paso":1,"etiquetaMin":"Muy malo","etiquetaMax":"Excelente"}
-- NPS                → valor_numero     0-10 | Detractor:0-6, Neutro:7-8, Promotor:9-10
-- CALIFICACION       → valor_numero     {"estrellas":5}
-- FECHA              → valor_fecha      {"incluirHora":false}
-- RANKING            → valor_json       ["op3","op1","op2"]
-- MATRIZ             → valor_json       {"Atención":"Bueno","Rapidez":"Excelente"}
--
-- =========================================================

-- =========================================================
-- REFERENCIA: JERARQUÍA CON WITH RECURSIVE
-- =========================================================
--
-- WITH RECURSIVE arbol AS (
--     SELECT id, nombre_visible, entidad_padre_id, 0 AS nivel
--     FROM entidad WHERE id = :id_raiz
--   UNION ALL
--     SELECT e.id, e.nombre_visible, e.entidad_padre_id, a.nivel + 1
--     FROM entidad e
--     INNER JOIN arbol a ON e.entidad_padre_id = a.id
-- )
-- SELECT * FROM arbol;
--
-- =========================================================

-- =========================================================
-- FIN
-- =========================================================
