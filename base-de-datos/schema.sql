-- =========================================================
-- MOTOR DE ENCUESTAS ENTERPRISE
-- PostgreSQL
-- =========================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- =========================================================
-- ORGANIZACION
-- Raíz del tenant. Todo dato pertenece a una organización.
-- =========================================================

CREATE TABLE Organizacion (
    Id        UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    Nombre    VARCHAR(200) NOT NULL,
    UrlLogo   TEXT,
    CreadoEn  TIMESTAMP    NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  Organizacion          IS 'Raíz del tenant. Cada organización es un cliente independiente del sistema.';
COMMENT ON COLUMN Organizacion.Id       IS 'Identificador único de la organización.';
COMMENT ON COLUMN Organizacion.Nombre   IS 'Nombre comercial o razón social de la organización.';
COMMENT ON COLUMN Organizacion.UrlLogo  IS 'URL pública del logotipo de la organización.';
COMMENT ON COLUMN Organizacion.CreadoEn IS 'Fecha y hora en que se registró la organización.';

-- =========================================================
-- TIPO DE ENTIDAD
-- Catálogo de los tipos de entidad que el sistema puede
-- evaluar mediante encuestas.
-- =========================================================

CREATE TABLE TipoEntidad (
    Id      UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    Codigo  VARCHAR(100) UNIQUE NOT NULL,
    Nombre  VARCHAR(200) NOT NULL
);

COMMENT ON TABLE  TipoEntidad        IS 'Catálogo de tipos de entidad soportados. Define qué clases de objetos pueden ser evaluados.';
COMMENT ON COLUMN TipoEntidad.Id     IS 'Identificador único del tipo de entidad.';
COMMENT ON COLUMN TipoEntidad.Codigo IS 'Clave técnica única del tipo (ej: EMPLEADO, SUCURSAL). Usada en lógica de aplicación.';
COMMENT ON COLUMN TipoEntidad.Nombre IS 'Nombre legible del tipo de entidad.';

-- =========================================================
-- ENTIDAD
-- Registro genérico que representa cualquier objeto evaluable
-- o nodo organizacional: empresa, regional, unidad (RRHH,
-- Servicios, Atención al Cliente), empleado, cliente, etc.
--
-- La jerarquía se modela con EntidadPadreId (auto-referencia):
--   Empresa → Regional Norte → Unidad RRHH
--                            → Unidad Atención al Cliente
--            → Regional Sur  → Unidad Servicios
--
-- Para agregar un nuevo tipo basta con insertar en TipoEntidad
-- y crear registros aquí, sin modificar el modelo de datos.
-- =========================================================

CREATE TABLE Entidad (
    Id              UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    OrganizacionId  UUID         NOT NULL REFERENCES Organizacion(Id),
    TipoEntidadId   UUID         NOT NULL REFERENCES TipoEntidad(Id),
    EntidadPadreId  UUID         NULL REFERENCES Entidad(Id),
    NombreVisible   VARCHAR(300) NOT NULL,
    IdExterno       VARCHAR(100),
    EsActivo        BOOLEAN      NOT NULL DEFAULT TRUE,
    AtributosJson   JSONB,
    CreadoEn        TIMESTAMP    NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  Entidad                IS 'Entidad genérica evaluable y nodo organizacional. Soporta jerarquía padre-hijo para modelar Empresa → Regional → Unidad sin tablas adicionales.';
COMMENT ON COLUMN Entidad.Id             IS 'Identificador único de la entidad.';
COMMENT ON COLUMN Entidad.OrganizacionId IS 'Organización a la que pertenece esta entidad.';
COMMENT ON COLUMN Entidad.TipoEntidadId  IS 'Tipo de entidad según el catálogo TipoEntidad (EMPRESA, REGIONAL, UNIDAD, EMPLEADO, CLIENTE, etc.).';
COMMENT ON COLUMN Entidad.EntidadPadreId IS 'Entidad padre en la jerarquía organizacional. NULL indica nodo raíz. Permite modelar Empresa → Regional → Unidad de forma recursiva.';
COMMENT ON COLUMN Entidad.NombreVisible  IS 'Nombre que se muestra en la interfaz y en reportes.';
COMMENT ON COLUMN Entidad.IdExterno      IS 'Identificador en el sistema externo de origen (RRHH, CRM, ERP). Sirve para sincronización.';
COMMENT ON COLUMN Entidad.EsActivo       IS 'Indica si la entidad está activa y disponible para ser evaluada.';
COMMENT ON COLUMN Entidad.AtributosJson  IS 'Atributos adicionales específicos del tipo (ej: {"ciudad":"Lima","region":"Sur"} para una sucursal, {"cargo":"Gerente"} para un empleado).';
COMMENT ON COLUMN Entidad.CreadoEn       IS 'Fecha y hora de creación del registro.';

CREATE INDEX IX_Entidad_OrganizacionId ON Entidad(OrganizacionId);
CREATE INDEX IX_Entidad_TipoEntidadId  ON Entidad(TipoEntidadId);
CREATE INDEX IX_Entidad_EntidadPadreId ON Entidad(EntidadPadreId);
CREATE INDEX IX_Entidad_AtributosJson  ON Entidad USING GIN(AtributosJson);

-- =========================================================
-- CUENTA DE USUARIO
-- Credenciales de acceso al sistema. Puede estar vinculada
-- a cualquier Entidad (ej: un empleado, un cliente).
-- =========================================================

CREATE TABLE CuentaUsuario (
    Id             UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    OrganizacionId UUID         NOT NULL REFERENCES Organizacion(Id),
    EntidadId      UUID         NULL REFERENCES Entidad(Id),
    Correo         VARCHAR(200) UNIQUE NOT NULL,
    HashContrasena TEXT         NOT NULL,
    EsActivo       BOOLEAN      NOT NULL DEFAULT TRUE,
    CreadoEn       TIMESTAMP    NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  CuentaUsuario                IS 'Credenciales de acceso al sistema. Un usuario puede estar vinculado a cualquier tipo de entidad o a ninguna.';
COMMENT ON COLUMN CuentaUsuario.Id             IS 'Identificador único de la cuenta.';
COMMENT ON COLUMN CuentaUsuario.OrganizacionId IS 'Organización a la que pertenece la cuenta.';
COMMENT ON COLUMN CuentaUsuario.EntidadId      IS 'Entidad asociada a esta cuenta (ej: el empleado o cliente que usa este acceso). NULL si es un administrador sin entidad propia.';
COMMENT ON COLUMN CuentaUsuario.Correo         IS 'Correo electrónico usado como nombre de usuario. Único en todo el sistema.';
COMMENT ON COLUMN CuentaUsuario.HashContrasena IS 'Hash de la contraseña (bcrypt). Nunca se almacena en texto plano.';
COMMENT ON COLUMN CuentaUsuario.EsActivo       IS 'Indica si la cuenta está habilitada para iniciar sesión.';
COMMENT ON COLUMN CuentaUsuario.CreadoEn       IS 'Fecha y hora de creación de la cuenta.';

CREATE INDEX IX_CuentaUsuario_OrganizacionId ON CuentaUsuario(OrganizacionId);
CREATE INDEX IX_CuentaUsuario_EntidadId      ON CuentaUsuario(EntidadId);

-- =========================================================
-- ROL
-- Agrupación de permisos asignable a usuarios.
-- =========================================================

CREATE TABLE Rol (
    Id             UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    OrganizacionId UUID         NOT NULL REFERENCES Organizacion(Id),
    Nombre         VARCHAR(100) NOT NULL
);

COMMENT ON TABLE  Rol                IS 'Rol de acceso dentro de una organización. Agrupa permisos asignables a usuarios.';
COMMENT ON COLUMN Rol.Id             IS 'Identificador único del rol.';
COMMENT ON COLUMN Rol.OrganizacionId IS 'Organización propietaria del rol.';
COMMENT ON COLUMN Rol.Nombre         IS 'Nombre descriptivo del rol (ej: Administrador, Analista, Respondente).';

-- =========================================================
-- PERMISO
-- Acción específica que puede realizarse en el sistema.
-- =========================================================

CREATE TABLE Permiso (
    Id      UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    Codigo  VARCHAR(100) UNIQUE NOT NULL,
    Nombre  VARCHAR(200) NOT NULL
);

COMMENT ON TABLE  Permiso        IS 'Catálogo de permisos del sistema. Cada permiso representa una acción específica.';
COMMENT ON COLUMN Permiso.Id     IS 'Identificador único del permiso.';
COMMENT ON COLUMN Permiso.Codigo IS 'Clave técnica del permiso (ej: encuesta.publicar, reporte.exportar). Usada en validaciones de código.';
COMMENT ON COLUMN Permiso.Nombre IS 'Nombre legible del permiso.';

-- =========================================================
-- USUARIO ROL
-- =========================================================

CREATE TABLE UsuarioRol (
    UsuarioId UUID NOT NULL REFERENCES CuentaUsuario(Id),
    RolId     UUID NOT NULL REFERENCES Rol(Id),
    PRIMARY KEY (UsuarioId, RolId)
);

COMMENT ON TABLE  UsuarioRol           IS 'Asignación de roles a usuarios. Un usuario puede tener múltiples roles.';
COMMENT ON COLUMN UsuarioRol.UsuarioId IS 'Usuario al que se le asigna el rol.';
COMMENT ON COLUMN UsuarioRol.RolId     IS 'Rol asignado al usuario.';

-- =========================================================
-- ROL PERMISO
-- =========================================================

CREATE TABLE RolPermiso (
    RolId     UUID NOT NULL REFERENCES Rol(Id),
    PermisoId UUID NOT NULL REFERENCES Permiso(Id),
    PRIMARY KEY (RolId, PermisoId)
);

COMMENT ON TABLE  RolPermiso           IS 'Asignación de permisos a roles. Define qué acciones puede realizar cada rol.';
COMMENT ON COLUMN RolPermiso.RolId     IS 'Rol al que se le asigna el permiso.';
COMMENT ON COLUMN RolPermiso.PermisoId IS 'Permiso asignado al rol.';

-- =========================================================
-- ENCUESTA
-- =========================================================

CREATE TABLE Encuesta (
    Id                  UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    OrganizacionId      UUID         NOT NULL REFERENCES Organizacion(Id),
    Titulo              VARCHAR(300) NOT NULL,
    Descripcion         TEXT,
    Version             INTEGER      NOT NULL DEFAULT 1,
    Estado              VARCHAR(50)  NOT NULL DEFAULT 'BORRADOR',
    EsGlobal            BOOLEAN      NOT NULL DEFAULT FALSE,
    EsPlantilla         BOOLEAN      NOT NULL DEFAULT FALSE,
    PlantillaOrigenId   UUID         NULL REFERENCES Encuesta(Id),
    EtiquetasJson       JSONB,
    CreadoPorUsuarioId  UUID         NULL REFERENCES CuentaUsuario(Id),
    FechaInicio         TIMESTAMP    NULL,
    FechaFin            TIMESTAMP    NULL,
    PublicadoEn         TIMESTAMP    NULL,
    ConfiguracionJson   JSONB,
    CreadoEn            TIMESTAMP    NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  Encuesta                      IS 'Encuesta creada por una organización. Ciclo de vida: BORRADOR → PUBLICADA → CERRADA.';
COMMENT ON COLUMN Encuesta.Id                   IS 'Identificador único de la encuesta.';
COMMENT ON COLUMN Encuesta.OrganizacionId       IS 'Organización propietaria de la encuesta.';
COMMENT ON COLUMN Encuesta.Titulo               IS 'Título visible de la encuesta para respondentes y administradores.';
COMMENT ON COLUMN Encuesta.Descripcion          IS 'Descripción o instrucciones generales mostradas al inicio de la encuesta.';
COMMENT ON COLUMN Encuesta.Version              IS 'Versión estructural. Se incrementa cuando se modifican preguntas de una encuesta ya publicada para mantener integridad histórica.';
COMMENT ON COLUMN Encuesta.Estado               IS 'Estado del ciclo de vida: BORRADOR, PUBLICADA, CERRADA.';
COMMENT ON COLUMN Encuesta.EsGlobal             IS 'TRUE = encuesta aplica a toda la organización sin filtro de alcance. FALSE = el alcance se define en AlcanceEncuesta (por regional, unidad, etc.).';
COMMENT ON COLUMN Encuesta.EsPlantilla          IS 'TRUE = esta encuesta es una plantilla reutilizable, no se distribuye directamente. FALSE = encuesta operativa.';
COMMENT ON COLUMN Encuesta.PlantillaOrigenId    IS 'Encuesta plantilla de la que se originó esta encuesta por clonación. NULL si fue creada desde cero.';
COMMENT ON COLUMN Encuesta.EtiquetasJson        IS 'Etiquetas para categorización y filtrado. Ej: ["nps","clima-laboral","2024-q1"]. Permite organizar el catálogo de encuestas.';
COMMENT ON COLUMN Encuesta.CreadoPorUsuarioId   IS 'Usuario que creó la encuesta.';
COMMENT ON COLUMN Encuesta.FechaInicio          IS 'Fecha desde la que la encuesta acepta respuestas. NULL si no tiene restricción de inicio.';
COMMENT ON COLUMN Encuesta.FechaFin             IS 'Fecha hasta la que la encuesta acepta respuestas. NULL si no tiene fecha de cierre automático.';
COMMENT ON COLUMN Encuesta.PublicadoEn          IS 'Fecha y hora en que la encuesta fue publicada. NULL si aún no se ha publicado.';
COMMENT ON COLUMN Encuesta.ConfiguracionJson    IS 'Configuración de comportamiento: {"esAnonima":false,"permitirMultiplesRespuestas":false,"mostrarProgreso":true,"modoNavegacion":"lineal","limiteRespuestas":null,"mensajeCompletado":"Gracias","requiereInvitacion":true}.';
COMMENT ON COLUMN Encuesta.CreadoEn             IS 'Fecha y hora de creación de la encuesta.';

CREATE INDEX IX_Encuesta_OrganizacionId   ON Encuesta(OrganizacionId);
CREATE INDEX IX_Encuesta_Estado           ON Encuesta(Estado);
CREATE INDEX IX_Encuesta_FechaInicio      ON Encuesta(FechaInicio);
CREATE INDEX IX_Encuesta_FechaFin         ON Encuesta(FechaFin);
CREATE INDEX IX_Encuesta_EsPlantilla      ON Encuesta(EsPlantilla);
CREATE INDEX IX_Encuesta_PlantillaOrigen  ON Encuesta(PlantillaOrigenId);
CREATE INDEX IX_Encuesta_EtiquetasJson    ON Encuesta USING GIN(EtiquetasJson);

-- =========================================================
-- SECCION DE ENCUESTA
-- Agrupa preguntas en bloques o páginas dentro de una encuesta.
-- Permite organizar encuestas largas, aplicar lógica
-- condicional a nivel de sección y generar estadísticas
-- por bloque temático.
-- =========================================================

CREATE TABLE SeccionEncuesta (
    Id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    EncuestaId  UUID         NOT NULL REFERENCES Encuesta(Id),
    Titulo      VARCHAR(300) NOT NULL,
    Descripcion TEXT,
    Orden       INTEGER      NOT NULL
);

COMMENT ON TABLE  SeccionEncuesta            IS 'Sección o página dentro de una encuesta. Permite organizar preguntas en bloques y habilitar paginación.';
COMMENT ON COLUMN SeccionEncuesta.Id         IS 'Identificador único de la sección.';
COMMENT ON COLUMN SeccionEncuesta.EncuestaId IS 'Encuesta a la que pertenece esta sección.';
COMMENT ON COLUMN SeccionEncuesta.Titulo     IS 'Título visible de la sección (ej: "Datos generales", "Evaluación de desempeño").';
COMMENT ON COLUMN SeccionEncuesta.Descripcion IS 'Instrucción o contexto mostrado al inicio de la sección.';
COMMENT ON COLUMN SeccionEncuesta.Orden      IS 'Posición de la sección dentro de la encuesta.';

CREATE INDEX IX_SeccionEncuesta_EncuestaId ON SeccionEncuesta(EncuestaId);

-- =========================================================
-- DIMENSION DE PREGUNTA
-- Eje temático para agrupar preguntas con fines estadísticos.
-- Permite calcular puntajes y promedios por dimensión
-- (ej: "Liderazgo", "Comunicación", "Satisfacción General").
-- Es diferente a SeccionEncuesta: la sección es visual/navegación,
-- la dimensión es analítica/estadística.
-- =========================================================

CREATE TABLE DimensionPregunta (
    Id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    EncuestaId  UUID         NOT NULL REFERENCES Encuesta(Id),
    Nombre      VARCHAR(200) NOT NULL,
    Descripcion TEXT,
    Peso        NUMERIC      NOT NULL DEFAULT 1.0,
    Orden       INTEGER      NOT NULL
);

COMMENT ON TABLE  DimensionPregunta            IS 'Eje temático para análisis estadístico. Agrupa preguntas para calcular puntajes por área (ej: Liderazgo, Clima Laboral). Diferente de SeccionEncuesta, que es para navegación visual.';
COMMENT ON COLUMN DimensionPregunta.Id         IS 'Identificador único de la dimensión.';
COMMENT ON COLUMN DimensionPregunta.EncuestaId IS 'Encuesta a la que pertenece esta dimensión.';
COMMENT ON COLUMN DimensionPregunta.Nombre     IS 'Nombre del eje temático (ej: "Liderazgo", "Comunicación", "Satisfacción").';
COMMENT ON COLUMN DimensionPregunta.Descripcion IS 'Descripción del eje y qué aspectos mide.';
COMMENT ON COLUMN DimensionPregunta.Peso       IS 'Peso relativo de la dimensión en el índice compuesto. Ej: Liderazgo=0.3, Comunicación=0.2, Clima=0.5. La suma de pesos debe ser 1.0 para índices porcentuales.';
COMMENT ON COLUMN DimensionPregunta.Orden      IS 'Orden de presentación en reportes y dashboards.';

CREATE INDEX IX_DimensionPregunta_EncuestaId ON DimensionPregunta(EncuestaId);

-- =========================================================
-- COLABORADOR DE ENCUESTA
-- Permite que múltiples usuarios co-editen una encuesta
-- con diferentes niveles de acceso.
-- =========================================================

CREATE TABLE ColaboradorEncuesta (
    Id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    EncuestaId  UUID        NOT NULL REFERENCES Encuesta(Id),
    UsuarioId   UUID        NOT NULL REFERENCES CuentaUsuario(Id),
    Rol         VARCHAR(50) NOT NULL DEFAULT 'EDITOR',
    InvitadoEn  TIMESTAMP   NOT NULL DEFAULT NOW(),

    CONSTRAINT UQ_ColaboradorEncuesta UNIQUE (EncuestaId, UsuarioId)
);

COMMENT ON TABLE  ColaboradorEncuesta           IS 'Usuarios con acceso de edición o revisión a una encuesta específica. Soporta flujos de trabajo multi-persona (RRHH crea, analista edita, director aprueba).';
COMMENT ON COLUMN ColaboradorEncuesta.Id        IS 'Identificador único del registro de colaboración.';
COMMENT ON COLUMN ColaboradorEncuesta.EncuestaId IS 'Encuesta compartida.';
COMMENT ON COLUMN ColaboradorEncuesta.UsuarioId IS 'Usuario con acceso colaborativo.';
COMMENT ON COLUMN ColaboradorEncuesta.Rol       IS 'Nivel de acceso: EDITOR (puede modificar), REVISOR (solo lectura y comentarios), APROBADOR (puede publicar).';
COMMENT ON COLUMN ColaboradorEncuesta.InvitadoEn IS 'Fecha en que se otorgó el acceso colaborativo.';

CREATE INDEX IX_ColaboradorEncuesta_EncuestaId ON ColaboradorEncuesta(EncuestaId);
CREATE INDEX IX_ColaboradorEncuesta_UsuarioId  ON ColaboradorEncuesta(UsuarioId);

-- =========================================================
-- CUOTA DE RESPUESTA
-- Límite de respuestas por segmento de entidad.
-- Cuando se alcanza la cuota del segmento, ese grupo se
-- cierra automáticamente aunque la encuesta siga activa.
-- Permite muestreo estadístico balanceado.
-- =========================================================

CREATE TABLE CuotaRespuesta (
    Id                  UUID    PRIMARY KEY DEFAULT gen_random_uuid(),
    EncuestaId          UUID    NOT NULL REFERENCES Encuesta(Id),
    EntidadId           UUID    NULL REFERENCES Entidad(Id),
    Limite              INTEGER NOT NULL,
    TotalActual         INTEGER NOT NULL DEFAULT 0,
    CerrarAlAlcanzar    BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE  CuotaRespuesta                  IS 'Define un límite máximo de respuestas por segmento de entidad. Cuando se alcanza la cuota, ese segmento se cierra automáticamente aunque la encuesta global siga activa.';
COMMENT ON COLUMN CuotaRespuesta.Id               IS 'Identificador único de la cuota.';
COMMENT ON COLUMN CuotaRespuesta.EncuestaId       IS 'Encuesta a la que aplica esta cuota.';
COMMENT ON COLUMN CuotaRespuesta.EntidadId        IS 'Segmento al que aplica la cuota (ej: Regional Norte). NULL = cuota global de toda la encuesta.';
COMMENT ON COLUMN CuotaRespuesta.Limite           IS 'Número máximo de respuestas aceptadas para este segmento.';
COMMENT ON COLUMN CuotaRespuesta.TotalActual      IS 'Contador de respuestas completadas. Actualizado por trigger o por la capa de aplicación al registrar cada respuesta.';
COMMENT ON COLUMN CuotaRespuesta.CerrarAlAlcanzar IS 'TRUE = dejar de aceptar respuestas de este segmento al alcanzar el límite. FALSE = solo alertar sin cerrar.';

CREATE INDEX IX_CuotaRespuesta_EncuestaId ON CuotaRespuesta(EncuestaId);
CREATE INDEX IX_CuotaRespuesta_EntidadId  ON CuotaRespuesta(EntidadId);

-- =========================================================
-- NOTIFICACION ENVIO
-- Historial de notificaciones enviadas (invitaciones,
-- recordatorios, confirmaciones). Permite auditar la
-- distribución y depurar problemas de entrega.
-- =========================================================

CREATE TABLE NotificacionEnvio (
    Id            UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    InvitacionId  UUID         NOT NULL REFERENCES Invitacion(Id),
    Tipo          VARCHAR(50)  NOT NULL,
    Canal         VARCHAR(50)  NOT NULL,
    Destinatario  VARCHAR(200) NOT NULL,
    Estado        VARCHAR(50)  NOT NULL DEFAULT 'PENDIENTE',
    IntentosEnvio INTEGER      NOT NULL DEFAULT 0,
    EnviadoEn     TIMESTAMP    NULL,
    EntregadoEn   TIMESTAMP    NULL,
    ErrorDetalle  TEXT         NULL,
    CreadoEn      TIMESTAMP    NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  NotificacionEnvio               IS 'Historial de todos los mensajes enviados en el proceso de distribución de una encuesta (invitaciones, recordatorios, confirmaciones de completado).';
COMMENT ON COLUMN NotificacionEnvio.Id            IS 'Identificador único del envío.';
COMMENT ON COLUMN NotificacionEnvio.InvitacionId  IS 'Invitación a la que corresponde este envío.';
COMMENT ON COLUMN NotificacionEnvio.Tipo          IS 'Tipo de mensaje: INVITACION, RECORDATORIO, CONFIRMACION, AGRADECIMIENTO.';
COMMENT ON COLUMN NotificacionEnvio.Canal         IS 'Canal de envío: EMAIL, SMS.';
COMMENT ON COLUMN NotificacionEnvio.Destinatario  IS 'Dirección de destino (correo o número de teléfono).';
COMMENT ON COLUMN NotificacionEnvio.Estado        IS 'Estado del envío: PENDIENTE, ENVIADO, ENTREGADO, FALLIDO, REBOTADO.';
COMMENT ON COLUMN NotificacionEnvio.IntentosEnvio IS 'Número de intentos de envío realizados. Útil para lógica de reintento.';
COMMENT ON COLUMN NotificacionEnvio.EnviadoEn     IS 'Fecha y hora en que el mensaje salió del sistema.';
COMMENT ON COLUMN NotificacionEnvio.EntregadoEn   IS 'Fecha y hora de confirmación de entrega (webhook del proveedor de email/SMS). NULL si no hay confirmación.';
COMMENT ON COLUMN NotificacionEnvio.ErrorDetalle  IS 'Detalle del error en caso de fallo. NULL si el envío fue exitoso.';
COMMENT ON COLUMN NotificacionEnvio.CreadoEn      IS 'Fecha y hora en que se generó la notificación.';

CREATE INDEX IX_NotificacionEnvio_InvitacionId ON NotificacionEnvio(InvitacionId);
CREATE INDEX IX_NotificacionEnvio_Estado       ON NotificacionEnvio(Estado);
CREATE INDEX IX_NotificacionEnvio_Tipo         ON NotificacionEnvio(Tipo);

-- =========================================================
-- ALCANCE DE ENCUESTA
-- Define a qué tipo(s) de entidad está dirigida una encuesta
-- y cuál es su rol (sujeto evaluado, contexto, etc.).
-- =========================================================

CREATE TABLE AlcanceEncuesta (
    Id                    UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    EncuestaId            UUID         NOT NULL REFERENCES Encuesta(Id),
    EntidadId             UUID         NOT NULL REFERENCES Entidad(Id),
    TipoRelacion          VARCHAR(100) NOT NULL DEFAULT 'AMBITO',
    IncluirDescendientes  BOOLEAN      NOT NULL DEFAULT FALSE
);

COMMENT ON TABLE  AlcanceEncuesta                       IS 'Define el alcance de la encuesta cuando EsGlobal = FALSE. Determina a qué entidades o jerarquías aplica la encuesta.';
COMMENT ON COLUMN AlcanceEncuesta.Id                    IS 'Identificador único del alcance.';
COMMENT ON COLUMN AlcanceEncuesta.EncuestaId            IS 'Encuesta que define el alcance.';
COMMENT ON COLUMN AlcanceEncuesta.EntidadId             IS 'Entidad o nodo organizacional incluido en el alcance (ej: "Regional Norte", "Unidad RRHH").';
COMMENT ON COLUMN AlcanceEncuesta.TipoRelacion          IS 'Rol del nodo en la encuesta: AMBITO (el nodo delimita quiénes responden), SUJETO (el nodo es el evaluado), CONTEXTO (referencia sin ser evaluado).';
COMMENT ON COLUMN AlcanceEncuesta.IncluirDescendientes  IS 'TRUE = la encuesta aplica también a todas las entidades hijas del nodo en la jerarquía (ej: Regional Norte + todas sus unidades). FALSE = solo aplica al nodo exacto.';

CREATE INDEX IX_AlcanceEncuesta_EncuestaId ON AlcanceEncuesta(EncuestaId);
CREATE INDEX IX_AlcanceEncuesta_EntidadId  ON AlcanceEncuesta(EntidadId);

-- =========================================================
-- PREGUNTA
-- =========================================================

CREATE TABLE Pregunta (
    Id                UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    EncuestaId        UUID         NOT NULL REFERENCES Encuesta(Id),
    SeccionId         UUID         NULL REFERENCES SeccionEncuesta(Id),
    DimensionId       UUID         NULL REFERENCES DimensionPregunta(Id),
    Tipo              VARCHAR(100) NOT NULL,
    Titulo            TEXT         NOT NULL,
    Descripcion       TEXT,
    Orden             INTEGER      NOT NULL,
    Peso              NUMERIC      NOT NULL DEFAULT 1.0,
    EsObligatoria     BOOLEAN      NOT NULL DEFAULT FALSE,
    ConfiguracionJson JSONB,
    CreadoEn          TIMESTAMP    NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  Pregunta                   IS 'Pregunta perteneciente a una encuesta. El Tipo determina el componente de entrada a renderizar y la columna de valor a usar en DetalleRespuesta.';
COMMENT ON COLUMN Pregunta.Id                IS 'Identificador único de la pregunta.';
COMMENT ON COLUMN Pregunta.EncuestaId        IS 'Encuesta a la que pertenece esta pregunta.';
COMMENT ON COLUMN Pregunta.SeccionId         IS 'Sección a la que pertenece esta pregunta. NULL si la encuesta no usa secciones.';
COMMENT ON COLUMN Pregunta.DimensionId       IS 'Dimensión temática para análisis estadístico. NULL si la pregunta no forma parte de ningún eje analítico.';
COMMENT ON COLUMN Pregunta.Tipo              IS 'Tipo de pregunta: TEXTO, NUMERO, SELECCION_UNICA, SELECCION_MULTIPLE, ESCALA, FECHA, BOOLEANO.';
COMMENT ON COLUMN Pregunta.Titulo            IS 'Texto de la pregunta mostrado al respondente.';
COMMENT ON COLUMN Pregunta.Descripcion       IS 'Instrucción o aclaración adicional debajo del título.';
COMMENT ON COLUMN Pregunta.Orden             IS 'Posición de la pregunta dentro de la sección (o encuesta si no hay secciones).';
COMMENT ON COLUMN Pregunta.Peso              IS 'Peso de la pregunta dentro de su dimensión para el cálculo del puntaje. DEFAULT 1.0 = todas las preguntas pesan igual.';
COMMENT ON COLUMN Pregunta.EsObligatoria     IS 'Indica si la pregunta debe ser respondida para poder avanzar o enviar.';
COMMENT ON COLUMN Pregunta.ConfiguracionJson IS 'Configuración específica del tipo (ej: {"min":1,"max":5,"paso":1,"etiquetaMin":"Muy malo","etiquetaMax":"Excelente"}).';
COMMENT ON COLUMN Pregunta.CreadoEn          IS 'Fecha y hora de creación de la pregunta.';

CREATE INDEX IX_Pregunta_EncuestaId        ON Pregunta(EncuestaId);
CREATE INDEX IX_Pregunta_SeccionId         ON Pregunta(SeccionId);
CREATE INDEX IX_Pregunta_DimensionId       ON Pregunta(DimensionId);
CREATE INDEX IX_Pregunta_Orden             ON Pregunta(Orden);
CREATE INDEX IX_Pregunta_ConfiguracionJson ON Pregunta USING GIN(ConfiguracionJson);

-- =========================================================
-- OPCION DE PREGUNTA
-- Opciones predefinidas para preguntas de selección.
-- =========================================================

CREATE TABLE OpcionPregunta (
    Id         UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    PreguntaId UUID         NOT NULL REFERENCES Pregunta(Id),
    Etiqueta   VARCHAR(300) NOT NULL,
    Valor      VARCHAR(300) NOT NULL,
    Puntaje    NUMERIC      NULL,
    Orden      INTEGER      NOT NULL
);

COMMENT ON TABLE  OpcionPregunta           IS 'Opciones de respuesta para preguntas SELECCION_UNICA o SELECCION_MULTIPLE.';
COMMENT ON COLUMN OpcionPregunta.Id        IS 'Identificador único de la opción.';
COMMENT ON COLUMN OpcionPregunta.PreguntaId IS 'Pregunta a la que pertenece esta opción.';
COMMENT ON COLUMN OpcionPregunta.Etiqueta  IS 'Texto visible de la opción para el respondente.';
COMMENT ON COLUMN OpcionPregunta.Valor     IS 'Valor interno almacenado cuando se selecciona esta opción.';
COMMENT ON COLUMN OpcionPregunta.Puntaje   IS 'Puntaje numérico de la opción para cálculos estadísticos (ej: en escala Likert: Muy de acuerdo=5, De acuerdo=4...). NULL si la pregunta no tiene puntaje.';
COMMENT ON COLUMN OpcionPregunta.Orden     IS 'Posición de la opción dentro de la lista.';

CREATE INDEX IX_OpcionPregunta_PreguntaId ON OpcionPregunta(PreguntaId);

-- =========================================================
-- REGLA DE ENCUESTA
-- Lógica condicional: mostrar, ocultar o saltar preguntas
-- según las respuestas del respondente.
-- =========================================================

CREATE TABLE ReglaEncuesta (
    Id         UUID  PRIMARY KEY DEFAULT gen_random_uuid(),
    EncuestaId UUID  NOT NULL REFERENCES Encuesta(Id),
    ReglaJson  JSONB NOT NULL
);

COMMENT ON TABLE  ReglaEncuesta           IS 'Reglas de lógica condicional de la encuesta. Evaluadas en tiempo de ejecución al navegar entre preguntas.';
COMMENT ON COLUMN ReglaEncuesta.Id        IS 'Identificador único de la regla.';
COMMENT ON COLUMN ReglaEncuesta.EncuestaId IS 'Encuesta a la que aplica esta regla.';
COMMENT ON COLUMN ReglaEncuesta.ReglaJson IS 'Regla en JSON: {"si":{"preguntaId":"...","operador":"igual","valor":"SI"},"entonces":{"accion":"mostrar","preguntaObjetivoId":"..."}}. Operadores: igual|distinto|mayor|menor|contiene. Acciones: mostrar|ocultar|saltar.';

CREATE INDEX IX_ReglaEncuesta_EncuestaId ON ReglaEncuesta(EncuestaId);
CREATE INDEX IX_ReglaEncuesta_ReglaJson  ON ReglaEncuesta USING GIN(ReglaJson);

-- =========================================================
-- INVITACION
-- Gestión de la distribución de encuestas. Registra a quién
-- se invitó, con qué token de acceso, en qué estado está
-- y qué entidad debe evaluar.
-- Permite enviar recordatorios y controlar el acceso.
-- =========================================================

CREATE TABLE Invitacion (
    Id                UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    EncuestaId        UUID         NOT NULL REFERENCES Encuesta(Id),
    CuentaUsuarioId   UUID         NULL REFERENCES CuentaUsuario(Id),
    CorreoDestino     VARCHAR(200) NULL,
    EntidadEvaluadaId UUID         NULL REFERENCES Entidad(Id),
    TokenAcceso       UUID         NOT NULL DEFAULT gen_random_uuid(),
    Canal             VARCHAR(50)  NOT NULL DEFAULT 'EMAIL',
    Estado            VARCHAR(50)  NOT NULL DEFAULT 'PENDIENTE',
    EnviadoEn         TIMESTAMP    NULL,
    VenceEn           TIMESTAMP    NULL,
    RespondidoEn      TIMESTAMP    NULL,

    CONSTRAINT CK_Invitacion_Destinatario
        CHECK (CuentaUsuarioId IS NOT NULL OR CorreoDestino IS NOT NULL)
);

COMMENT ON TABLE  Invitacion                   IS 'Gestión de distribución de la encuesta. Registra cada invitación enviada, su canal, estado y token de acceso único.';
COMMENT ON COLUMN Invitacion.Id                IS 'Identificador único de la invitación.';
COMMENT ON COLUMN Invitacion.EncuestaId        IS 'Encuesta a la que corresponde la invitación.';
COMMENT ON COLUMN Invitacion.CuentaUsuarioId   IS 'Usuario invitado con cuenta en el sistema. NULL si se invita por correo sin cuenta.';
COMMENT ON COLUMN Invitacion.CorreoDestino     IS 'Correo del destinatario cuando no tiene cuenta en el sistema. Al menos uno entre CuentaUsuarioId y CorreoDestino debe estar presente.';
COMMENT ON COLUMN Invitacion.EntidadEvaluadaId IS 'Entidad que este respondente debe evaluar. NULL si la encuesta es general o el respondente elige libremente.';
COMMENT ON COLUMN Invitacion.TokenAcceso       IS 'Token UUID único para acceso mediante enlace. Permite responder sin iniciar sesión.';
COMMENT ON COLUMN Invitacion.Canal             IS 'Canal por el que se distribuyó la invitación: EMAIL, SMS, QR, ENLACE_PUBLICO, KIOSCO, API.';
COMMENT ON COLUMN Invitacion.Estado            IS 'Estado de la invitación: PENDIENTE, RESPONDIDA, EXPIRADA, CANCELADA.';
COMMENT ON COLUMN Invitacion.EnviadoEn         IS 'Fecha y hora en que se envió la invitación al destinatario.';
COMMENT ON COLUMN Invitacion.VenceEn           IS 'Fecha de expiración del token de acceso. NULL si no tiene vencimiento propio (usa la FechaFin de la encuesta).';
COMMENT ON COLUMN Invitacion.RespondidoEn      IS 'Fecha y hora en que el destinatario completó la encuesta.';

CREATE UNIQUE INDEX IX_Invitacion_TokenAcceso    ON Invitacion(TokenAcceso);
CREATE INDEX       IX_Invitacion_EncuestaId      ON Invitacion(EncuestaId);
CREATE INDEX       IX_Invitacion_CuentaUsuarioId ON Invitacion(CuentaUsuarioId);
CREATE INDEX       IX_Invitacion_Estado          ON Invitacion(Estado);

-- =========================================================
-- RESPUESTA
-- Sesión de respuesta de un respondente a una encuesta.
-- Un mismo respondente puede tener múltiples respuestas
-- a la misma encuesta (ej: evaluar distintas entidades).
-- =========================================================

CREATE TABLE Respuesta (
    Id                      UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    EncuestaId              UUID         NOT NULL REFERENCES Encuesta(Id),
    VersionEncuesta         INTEGER      NOT NULL DEFAULT 1,
    InvitacionId            UUID         NULL REFERENCES Invitacion(Id),
    UsuarioRespondentId     UUID         NULL REFERENCES CuentaUsuario(Id),
    Canal                   VARCHAR(50)  NULL,
    UltimaPreguntaId        UUID         NULL REFERENCES Pregunta(Id),
    PesoEstadistico         NUMERIC      NOT NULL DEFAULT 1.0,
    ConsentimientoOtorgado  BOOLEAN      NULL,
    FechaConsentimiento     TIMESTAMP    NULL,
    IniciadoEn              TIMESTAMP,
    CompletadoEn            TIMESTAMP,
    InfoDispositivo         TEXT,
    DireccionIp             VARCHAR(100)
);

COMMENT ON TABLE  Respuesta                           IS 'Sesión de respuesta a una encuesta. Representa un intento completo o parcial de un respondente.';
COMMENT ON COLUMN Respuesta.Id                        IS 'Identificador único de la sesión de respuesta.';
COMMENT ON COLUMN Respuesta.EncuestaId                IS 'Encuesta que se está respondiendo.';
COMMENT ON COLUMN Respuesta.VersionEncuesta           IS 'Versión de la encuesta al momento de responder. Garantiza integridad histórica cuando la encuesta se modifica.';
COMMENT ON COLUMN Respuesta.InvitacionId              IS 'Invitación que originó esta respuesta. NULL si la respuesta fue por enlace público o acceso directo.';
COMMENT ON COLUMN Respuesta.UsuarioRespondentId       IS 'Usuario que responde. NULL para encuestas anónimas.';
COMMENT ON COLUMN Respuesta.Canal                     IS 'Canal por el que llegó esta respuesta: EMAIL, SMS, QR, ENLACE_PUBLICO, KIOSCO, API. Se copia desde Invitacion.Canal o se asigna directamente para respuestas sin invitación.';
COMMENT ON COLUMN Respuesta.UltimaPreguntaId          IS 'Última pregunta vista o respondida. Permite análisis de abandono: qué pregunta hace que la gente deje la encuesta.';
COMMENT ON COLUMN Respuesta.PesoEstadistico           IS 'Factor de ponderación para análisis estadístico ponderado. DEFAULT 1.0 = sin ponderación. Se ajusta cuando un segmento está sub-representado en la muestra.';
COMMENT ON COLUMN Respuesta.ConsentimientoOtorgado    IS 'El respondente aceptó el aviso de privacidad/GDPR. NULL si la encuesta no requirió consentimiento explícito.';
COMMENT ON COLUMN Respuesta.FechaConsentimiento       IS 'Fecha y hora en que se otorgó el consentimiento.';
COMMENT ON COLUMN Respuesta.IniciadoEn                IS 'Fecha y hora en que el respondente comenzó la encuesta.';
COMMENT ON COLUMN Respuesta.CompletadoEn              IS 'Fecha y hora en que se envió la encuesta. NULL si está en progreso o fue abandonada.';
COMMENT ON COLUMN Respuesta.InfoDispositivo           IS 'User-agent del dispositivo usado. Para auditoría y análisis de canal.';
COMMENT ON COLUMN Respuesta.DireccionIp               IS 'Dirección IP del respondente al momento del envío. Para auditoría.';

CREATE INDEX IX_Respuesta_EncuestaId          ON Respuesta(EncuestaId);
CREATE INDEX IX_Respuesta_InvitacionId        ON Respuesta(InvitacionId);
CREATE INDEX IX_Respuesta_UsuarioRespondentId ON Respuesta(UsuarioRespondentId);
CREATE INDEX IX_Respuesta_CompletadoEn        ON Respuesta(CompletadoEn);
CREATE INDEX IX_Respuesta_Canal               ON Respuesta(Canal);
CREATE INDEX IX_Respuesta_UltimaPreguntaId    ON Respuesta(UltimaPreguntaId);

-- =========================================================
-- OBJETIVO DE RESPUESTA
-- Registra qué entidad(es) fueron evaluadas en una respuesta.
-- Una respuesta puede evaluar múltiples entidades a la vez
-- (ej: un empleado + una sucursal + un evento simultáneamente).
-- =========================================================

CREATE TABLE ObjetivoRespuesta (
    Id           UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    RespuestaId  UUID         NOT NULL REFERENCES Respuesta(Id),
    EntidadId    UUID         NOT NULL REFERENCES Entidad(Id),
    TipoRelacion VARCHAR(100) NOT NULL
);

COMMENT ON TABLE  ObjetivoRespuesta              IS 'Entidad(es) evaluadas en una sesión de respuesta. Permite evaluar múltiples entidades en un mismo envío.';
COMMENT ON COLUMN ObjetivoRespuesta.Id           IS 'Identificador único del objetivo.';
COMMENT ON COLUMN ObjetivoRespuesta.RespuestaId  IS 'Sesión de respuesta a la que pertenece este objetivo.';
COMMENT ON COLUMN ObjetivoRespuesta.EntidadId    IS 'Entidad que fue evaluada.';
COMMENT ON COLUMN ObjetivoRespuesta.TipoRelacion IS 'Rol de la entidad evaluada: EVALUADO (sujeto principal), CONTEXTO, EVENTO, etc.';

CREATE INDEX IX_ObjetivoRespuesta_RespuestaId ON ObjetivoRespuesta(RespuestaId);
CREATE INDEX IX_ObjetivoRespuesta_EntidadId   ON ObjetivoRespuesta(EntidadId);

-- =========================================================
-- DETALLE DE RESPUESTA
-- Respuesta individual a cada pregunta dentro de una sesión.
-- Se usa la columna de valor correspondiente al Tipo de pregunta.
-- =========================================================

CREATE TABLE DetalleRespuesta (
    Id            UUID     PRIMARY KEY DEFAULT gen_random_uuid(),
    RespuestaId   UUID     NOT NULL REFERENCES Respuesta(Id),
    PreguntaId    UUID     NOT NULL REFERENCES Pregunta(Id),
    ValorTexto    TEXT,
    ValorNumero   NUMERIC,
    ValorBooleano BOOLEAN,
    ValorFecha    TIMESTAMP,
    ValorJson     JSONB
);

COMMENT ON TABLE  DetalleRespuesta               IS 'Respuesta individual a una pregunta. Se llena solo la columna de valor que corresponde al Tipo de la pregunta.';
COMMENT ON COLUMN DetalleRespuesta.Id            IS 'Identificador único del detalle.';
COMMENT ON COLUMN DetalleRespuesta.RespuestaId   IS 'Sesión de respuesta a la que pertenece este detalle.';
COMMENT ON COLUMN DetalleRespuesta.PreguntaId    IS 'Pregunta que fue respondida.';
COMMENT ON COLUMN DetalleRespuesta.ValorTexto    IS 'Respuesta en texto. Usado para TEXTO y SELECCION_UNICA (almacena el Valor de OpcionPregunta).';
COMMENT ON COLUMN DetalleRespuesta.ValorNumero   IS 'Respuesta numérica. Usado para NUMERO y ESCALA.';
COMMENT ON COLUMN DetalleRespuesta.ValorBooleano IS 'Respuesta booleana. Usado para BOOLEANO (sí/no).';
COMMENT ON COLUMN DetalleRespuesta.ValorFecha    IS 'Respuesta de fecha/hora. Usado para FECHA.';
COMMENT ON COLUMN DetalleRespuesta.ValorJson     IS 'Respuesta en JSON. Usado para SELECCION_MULTIPLE (ej: ["op1","op2"]) o respuestas matriciales.';

CREATE INDEX IX_DetalleRespuesta_RespuestaId ON DetalleRespuesta(RespuestaId);
CREATE INDEX IX_DetalleRespuesta_PreguntaId  ON DetalleRespuesta(PreguntaId);
CREATE INDEX IX_DetalleRespuesta_ValorJson   ON DetalleRespuesta USING GIN(ValorJson);

-- =========================================================
-- ADJUNTO
-- Archivos vinculados a cualquier Entidad del sistema
-- (perfil de empleado, documentos de un proyecto, etc.).
-- Para adjuntar un archivo como respuesta a una pregunta,
-- almacenar la URL en DetalleRespuesta.ValorTexto.
-- =========================================================

CREATE TABLE Adjunto (
    Id            UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    EntidadId     UUID         NOT NULL REFERENCES Entidad(Id),
    NombreArchivo VARCHAR(300),
    UrlArchivo    TEXT         NOT NULL,
    SubidoEn      TIMESTAMP    NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  Adjunto              IS 'Archivos adjuntos vinculados a una Entidad del sistema.';
COMMENT ON COLUMN Adjunto.Id           IS 'Identificador único del adjunto.';
COMMENT ON COLUMN Adjunto.EntidadId    IS 'Entidad a la que pertenece el adjunto.';
COMMENT ON COLUMN Adjunto.NombreArchivo IS 'Nombre original del archivo subido.';
COMMENT ON COLUMN Adjunto.UrlArchivo   IS 'URL de acceso al archivo en el sistema de almacenamiento.';
COMMENT ON COLUMN Adjunto.SubidoEn     IS 'Fecha y hora en que se subió el archivo.';

CREATE INDEX IX_Adjunto_EntidadId ON Adjunto(EntidadId);

-- =========================================================
-- COMENTARIO
-- Anotaciones sobre Entidades o sobre Respuestas específicas.
-- Permite que analistas anoten observaciones directamente
-- sobre una sesión de respuesta (ej: "respuesta sospechosa",
-- "requiere seguimiento").
-- =========================================================

CREATE TABLE Comentario (
    Id              UUID      PRIMARY KEY DEFAULT gen_random_uuid(),
    EntidadId       UUID      NULL REFERENCES Entidad(Id),
    RespuestaId     UUID      NULL REFERENCES Respuesta(Id),
    UsuarioId       UUID      NULL REFERENCES CuentaUsuario(Id),
    TextoComentario TEXT      NOT NULL,
    CreadoEn        TIMESTAMP NOT NULL DEFAULT NOW(),

    CONSTRAINT CK_Comentario_Objetivo
        CHECK (EntidadId IS NOT NULL OR RespuestaId IS NOT NULL)
);

COMMENT ON TABLE  Comentario                 IS 'Anotaciones sobre Entidades o Respuestas. Permite comentar perfiles de entidades o anotar observaciones analíticas sobre respuestas específicas.';
COMMENT ON COLUMN Comentario.Id              IS 'Identificador único del comentario.';
COMMENT ON COLUMN Comentario.EntidadId       IS 'Entidad sobre la que se comenta. NULL si el comentario es sobre una Respuesta.';
COMMENT ON COLUMN Comentario.RespuestaId     IS 'Respuesta sobre la que se anota. NULL si el comentario es sobre una Entidad. Al menos uno entre EntidadId y RespuestaId debe estar presente.';
COMMENT ON COLUMN Comentario.UsuarioId       IS 'Usuario que escribió el comentario. NULL si fue generado por el sistema.';
COMMENT ON COLUMN Comentario.TextoComentario IS 'Contenido del comentario.';
COMMENT ON COLUMN Comentario.CreadoEn        IS 'Fecha y hora en que se escribió el comentario.';

CREATE INDEX IX_Comentario_EntidadId   ON Comentario(EntidadId);
CREATE INDEX IX_Comentario_RespuestaId ON Comentario(RespuestaId);

-- =========================================================
-- DATOS INICIALES - TIPOS DE ENTIDAD
-- Incluye nodos organizacionales (jerarquía) y entidades
-- evaluables (sujetos de encuesta).
-- =========================================================

INSERT INTO TipoEntidad (Codigo, Nombre) VALUES
    -- Nodos organizacionales (forman la jerarquía con EntidadPadreId)
    ('EMPRESA',          'Empresa'),
    ('REGIONAL',         'Regional'),
    ('UNIDAD',           'Unidad'),
    -- Entidades evaluables
    ('EMPLEADO',         'Empleado'),
    ('CLIENTE',          'Cliente'),
    ('PROVEEDOR',        'Proveedor'),
    -- Objetos evaluables no organizacionales
    ('PRODUCTO',         'Producto'),
    ('SERVICIO',         'Servicio'),
    ('APLICACION',       'Aplicación'),
    ('PROYECTO',         'Proyecto'),
    ('EVENTO',           'Evento');

-- =========================================================
-- DATOS INICIALES - PERMISOS
-- =========================================================

INSERT INTO Permiso (Codigo, Nombre) VALUES
    ('encuesta.ver',               'Ver encuestas'),
    ('encuesta.crear',             'Crear encuestas'),
    ('encuesta.editar',            'Editar encuestas'),
    ('encuesta.publicar',          'Publicar encuestas'),
    ('encuesta.cerrar',            'Cerrar encuestas'),
    ('encuesta.eliminar',          'Eliminar encuestas'),
    ('encuesta.plantilla.usar',    'Usar plantillas de encuesta'),
    ('encuesta.colaborar',         'Colaborar en encuestas compartidas'),
    ('invitacion.administrar',     'Administrar invitaciones'),
    ('invitacion.recordatorio',    'Enviar recordatorios'),
    ('respuesta.ver',              'Ver respuestas'),
    ('respuesta.anotar',           'Anotar respuestas'),
    ('reporte.ver',                'Ver reportes'),
    ('reporte.exportar',           'Exportar reportes'),
    ('reporte.estadistico',        'Ver reportes estadísticos avanzados'),
    ('cuota.administrar',          'Administrar cuotas de respuesta'),
    ('usuario.administrar',        'Administrar usuarios'),
    ('entidad.administrar',        'Administrar entidades');

-- =========================================================
-- REFERENCIA: TIPOS DE PREGUNTA → COLUMNA DE VALOR
-- =========================================================
--
-- TEXTO              → ValorTexto
--   Respuesta abierta de texto libre.
--   ConfiguracionJson: {"placeholder":"Escriba aquí","maxCaracteres":500}
--
-- NUMERO             → ValorNumero
--   Ingreso numérico directo.
--   ConfiguracionJson: {"min":0,"max":1000,"decimales":2}
--
-- BOOLEANO           → ValorBooleano
--   Verdadero / Falso  |  Sí / No.
--   ConfiguracionJson: {"etiquetaTrue":"Verdadero","etiquetaFalse":"Falso"}
--
-- SELECCION_UNICA    → ValorTexto    (almacena OpcionPregunta.Valor)
--   Una opción de una lista. Incluye escalas Likert:
--   Muy insatisfecho | Insatisfecho | Regular | Satisfecho | Muy satisfecho
--   OpcionPregunta.Puntaje: 1, 2, 3, 4, 5
--
-- SELECCION_MULTIPLE → ValorJson     (ej: ["op1","op2"])
--   Varias opciones de una lista.
--
-- ESCALA             → ValorNumero   (ej: 1 al 5, 1 al 10)
--   Deslizador o botones numéricos.
--   ConfiguracionJson: {"min":1,"max":5,"paso":1,"etiquetaMin":"Muy malo","etiquetaMax":"Excelente"}
--
-- NPS                → ValorNumero   (0 al 10)
--   Net Promoter Score. Renderizado especial con colores por zona.
--   ConfiguracionJson: {"min":0,"max":10,"etiquetaMin":"Nada probable","etiquetaMax":"Muy probable"}
--   Zonas: 0-6 = Detractor, 7-8 = Neutro, 9-10 = Promotor
--
-- CALIFICACION       → ValorNumero   (estrellas / iconos)
--   Calificación visual con estrellas.
--   ConfiguracionJson: {"estrellas":5,"icono":"estrella"}
--
-- FECHA              → ValorFecha
--   Selector de fecha y/u hora.
--   ConfiguracionJson: {"incluirHora":false,"fechaMin":"2024-01-01","fechaMax":null}
--
-- RANKING            → ValorJson     (orden de opciones)
--   El respondente ordena las opciones por preferencia.
--   ValorJson almacena el orden resultante: ["op3","op1","op2"]
--
-- MATRIZ             → ValorJson     (mapa fila→opcion)
--   Evalúa múltiples ítems con la misma escala en una tabla.
--   ConfiguracionJson: {"filas":["Atención","Rapidez","Calidad"],"escala":["Malo","Regular","Bueno","Excelente"]}
--   ValorJson: {"Atención":"Bueno","Rapidez":"Excelente","Calidad":"Regular"}
--
-- =========================================================

-- =========================================================
-- REFERENCIA: ESTADOS DE ENCUESTA
-- =========================================================
-- BORRADOR  → en construcción, no visible para respondentes
-- PUBLICADA → activa, acepta respuestas dentro de FechaInicio/FechaFin
-- CERRADA   → finalizada, no acepta nuevas respuestas
-- =========================================================

-- =========================================================
-- REFERENCIA: ESTADOS DE INVITACION
-- =========================================================
-- PENDIENTE  → enviada, sin respuesta aún
-- RESPONDIDA → el destinatario completó la encuesta
-- EXPIRADA   → venció el plazo sin respuesta
-- CANCELADA  → anulada manualmente
-- =========================================================

-- =========================================================
-- REFERENCIA: JERARQUÍA DE ENTIDADES (EntidadPadreId)
-- =========================================================
-- Para recorrer la jerarquía completa usar WITH RECURSIVE:
--
-- WITH RECURSIVE Arbol AS (
--     SELECT Id, NombreVisible, EntidadPadreId, 0 AS Nivel
--     FROM Entidad WHERE Id = :idRaiz
--   UNION ALL
--     SELECT e.Id, e.NombreVisible, e.EntidadPadreId, a.Nivel + 1
--     FROM Entidad e
--     INNER JOIN Arbol a ON e.EntidadPadreId = a.Id
-- )
-- SELECT * FROM Arbol;
--
-- Ejemplo de árbol:
--   EMPRESA  "Acme Corp"
--     REGIONAL "Regional Norte"       (EntidadPadreId → Acme Corp)
--       UNIDAD "RRHH"                 (EntidadPadreId → Regional Norte)
--       UNIDAD "Atención al Cliente"  (EntidadPadreId → Regional Norte)
--       UNIDAD "Servicios"            (EntidadPadreId → Regional Norte)
--     REGIONAL "Regional Sur"         (EntidadPadreId → Acme Corp)
--       UNIDAD "RRHH"                 (EntidadPadreId → Regional Sur)
-- =========================================================

-- =========================================================
-- REFERENCIA: REGLAS DE ALCANCE DE ENCUESTA
-- =========================================================
-- EsGlobal = TRUE  → aplica a toda la organización
--                    (no usar AlcanceEncuesta)
--
-- EsGlobal = FALSE → usar AlcanceEncuesta para definir:
--
--   Encuesta solo para "Regional Norte":
--     AlcanceEncuesta(EntidadId=Regional Norte, IncluirDescendientes=FALSE)
--     → solo quienes pertenecen exactamente a Regional Norte
--
--   Encuesta para "Regional Norte" y todas sus unidades:
--     AlcanceEncuesta(EntidadId=Regional Norte, IncluirDescendientes=TRUE)
--     → Regional Norte + RRHH + Atención al Cliente + Servicios
--
--   Encuesta solo para la unidad "Atención al Cliente":
--     AlcanceEncuesta(EntidadId=Atención al Cliente, IncluirDescendientes=FALSE)
--
--   Encuesta para evaluar satisfacción con el sistema/app:
--     EsGlobal = TRUE  (cualquier usuario puede responder)
--     Encuesta.TipoSujeto = APLICACION (la app es la evaluada)
--     ObjetivoRespuesta.EntidadId = Entidad de la aplicación
-- =========================================================

-- =========================================================
-- FIN
-- =========================================================
