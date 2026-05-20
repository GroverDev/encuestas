--
-- PostgreSQL database dump
--

-- Dumped from database version 16.1 (Debian 16.1-1.pgdg120+1)
-- Dumped by pg_dump version 16.1 (Debian 16.1-1.pgdg120+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: pgcrypto; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA public;


--
-- Name: EXTENSION pgcrypto; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION pgcrypto IS 'cryptographic functions';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: adjunto; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.adjunto (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    entidad_id uuid NOT NULL,
    nombre_archivo character varying(300),
    url_archivo text NOT NULL,
    subido_en timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.adjunto OWNER TO postgres;

--
-- Name: TABLE adjunto; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.adjunto IS 'Archivos adjuntos vinculados a una entidad del sistema.';


--
-- Name: COLUMN adjunto.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.adjunto.id IS 'Identificador único del adjunto.';


--
-- Name: COLUMN adjunto.entidad_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.adjunto.entidad_id IS 'Entidad a la que pertenece el adjunto.';


--
-- Name: COLUMN adjunto.nombre_archivo; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.adjunto.nombre_archivo IS 'Nombre original del archivo subido.';


--
-- Name: COLUMN adjunto.url_archivo; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.adjunto.url_archivo IS 'URL de acceso al archivo en el sistema de almacenamiento.';


--
-- Name: COLUMN adjunto.subido_en; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.adjunto.subido_en IS 'Fecha y hora en que se subió el archivo.';


--
-- Name: alcance_encuesta; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.alcance_encuesta (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    encuesta_id uuid NOT NULL,
    entidad_id uuid NOT NULL,
    tipo_relacion character varying(100) DEFAULT 'AMBITO'::character varying NOT NULL,
    incluir_descendientes boolean DEFAULT false NOT NULL
);


ALTER TABLE public.alcance_encuesta OWNER TO postgres;

--
-- Name: TABLE alcance_encuesta; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.alcance_encuesta IS 'Define el alcance de la encuesta cuando es_global = FALSE. Determina a qué entidades o jerarquías aplica.';


--
-- Name: COLUMN alcance_encuesta.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.alcance_encuesta.id IS 'Identificador único del alcance.';


--
-- Name: COLUMN alcance_encuesta.encuesta_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.alcance_encuesta.encuesta_id IS 'Encuesta que define el alcance.';


--
-- Name: COLUMN alcance_encuesta.entidad_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.alcance_encuesta.entidad_id IS 'Entidad o nodo organizacional incluido en el alcance.';


--
-- Name: COLUMN alcance_encuesta.tipo_relacion; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.alcance_encuesta.tipo_relacion IS 'Rol del nodo: AMBITO (delimita quiénes responden), SUJETO (el nodo es el evaluado), CONTEXTO (referencia sin ser evaluado).';


--
-- Name: COLUMN alcance_encuesta.incluir_descendientes; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.alcance_encuesta.incluir_descendientes IS 'TRUE = aplica también a todas las entidades hijas del nodo en la jerarquía.';


--
-- Name: colaborador_encuesta; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.colaborador_encuesta (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    encuesta_id uuid NOT NULL,
    usuario_id uuid NOT NULL,
    rol character varying(50) DEFAULT 'EDITOR'::character varying NOT NULL,
    invitado_en timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.colaborador_encuesta OWNER TO postgres;

--
-- Name: TABLE colaborador_encuesta; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.colaborador_encuesta IS 'Usuarios con acceso de edición o revisión a una encuesta específica.';


--
-- Name: COLUMN colaborador_encuesta.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.colaborador_encuesta.id IS 'Identificador único del registro de colaboración.';


--
-- Name: COLUMN colaborador_encuesta.encuesta_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.colaborador_encuesta.encuesta_id IS 'Encuesta compartida.';


--
-- Name: COLUMN colaborador_encuesta.usuario_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.colaborador_encuesta.usuario_id IS 'Usuario con acceso colaborativo.';


--
-- Name: COLUMN colaborador_encuesta.rol; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.colaborador_encuesta.rol IS 'Nivel de acceso: EDITOR (puede modificar), REVISOR (solo lectura), APROBADOR (puede publicar).';


--
-- Name: COLUMN colaborador_encuesta.invitado_en; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.colaborador_encuesta.invitado_en IS 'Fecha en que se otorgó el acceso colaborativo.';


--
-- Name: comentario; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.comentario (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    entidad_id uuid,
    respuesta_id uuid,
    usuario_id uuid,
    texto_comentario text NOT NULL,
    creado_en timestamp with time zone DEFAULT now() NOT NULL,
    CONSTRAINT ck_comentario_objetivo CHECK (((entidad_id IS NOT NULL) OR (respuesta_id IS NOT NULL)))
);


ALTER TABLE public.comentario OWNER TO postgres;

--
-- Name: TABLE comentario; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.comentario IS 'Anotaciones sobre entidades o respuestas. Permite comentar perfiles de entidades o anotar observaciones analíticas sobre respuestas.';


--
-- Name: COLUMN comentario.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.comentario.id IS 'Identificador único del comentario.';


--
-- Name: COLUMN comentario.entidad_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.comentario.entidad_id IS 'Entidad sobre la que se comenta. NULL si el comentario es sobre una respuesta.';


--
-- Name: COLUMN comentario.respuesta_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.comentario.respuesta_id IS 'Respuesta sobre la que se anota. NULL si el comentario es sobre una entidad. Al menos uno debe estar presente.';


--
-- Name: COLUMN comentario.usuario_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.comentario.usuario_id IS 'Usuario que escribió el comentario. NULL si fue generado por el sistema.';


--
-- Name: COLUMN comentario.texto_comentario; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.comentario.texto_comentario IS 'Contenido del comentario.';


--
-- Name: COLUMN comentario.creado_en; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.comentario.creado_en IS 'Fecha y hora en que se escribió el comentario.';


--
-- Name: cuenta_usuario; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cuenta_usuario (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    organizacion_id uuid NOT NULL,
    entidad_id uuid,
    correo character varying(200) NOT NULL,
    hash_contrasena text NOT NULL,
    es_activo boolean DEFAULT true NOT NULL,
    creado_en timestamp with time zone DEFAULT now() NOT NULL,
    es_cuenta_servicio boolean DEFAULT false NOT NULL
);


ALTER TABLE public.cuenta_usuario OWNER TO postgres;

--
-- Name: TABLE cuenta_usuario; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.cuenta_usuario IS 'Credenciales de acceso al sistema. Un usuario puede estar vinculado a cualquier tipo de entidad o a ninguna.';


--
-- Name: COLUMN cuenta_usuario.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.cuenta_usuario.id IS 'Identificador único de la cuenta.';


--
-- Name: COLUMN cuenta_usuario.organizacion_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.cuenta_usuario.organizacion_id IS 'Organización a la que pertenece la cuenta.';


--
-- Name: COLUMN cuenta_usuario.entidad_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.cuenta_usuario.entidad_id IS 'Entidad asociada a esta cuenta (ej: el empleado o cliente que usa este acceso). NULL si es un administrador sin entidad propia.';


--
-- Name: COLUMN cuenta_usuario.correo; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.cuenta_usuario.correo IS 'Correo electrónico usado como nombre de usuario. Único en todo el sistema.';


--
-- Name: COLUMN cuenta_usuario.hash_contrasena; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.cuenta_usuario.hash_contrasena IS 'Hash de la contraseña (PBKDF2). Nunca se almacena en texto plano.';


--
-- Name: COLUMN cuenta_usuario.es_activo; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.cuenta_usuario.es_activo IS 'Indica si la cuenta está habilitada para iniciar sesión.';


--
-- Name: COLUMN cuenta_usuario.creado_en; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.cuenta_usuario.creado_en IS 'Fecha y hora de creación de la cuenta.';


--
-- Name: cuota_respuesta; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cuota_respuesta (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    encuesta_id uuid NOT NULL,
    entidad_id uuid,
    limite integer NOT NULL,
    total_actual integer DEFAULT 0 NOT NULL,
    cerrar_al_alcanzar boolean DEFAULT true NOT NULL
);


ALTER TABLE public.cuota_respuesta OWNER TO postgres;

--
-- Name: TABLE cuota_respuesta; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.cuota_respuesta IS 'Define un límite máximo de respuestas por segmento de entidad.';


--
-- Name: COLUMN cuota_respuesta.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.cuota_respuesta.id IS 'Identificador único de la cuota.';


--
-- Name: COLUMN cuota_respuesta.encuesta_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.cuota_respuesta.encuesta_id IS 'Encuesta a la que aplica esta cuota.';


--
-- Name: COLUMN cuota_respuesta.entidad_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.cuota_respuesta.entidad_id IS 'Segmento al que aplica la cuota. NULL = cuota global de toda la encuesta.';


--
-- Name: COLUMN cuota_respuesta.limite; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.cuota_respuesta.limite IS 'Número máximo de respuestas aceptadas para este segmento.';


--
-- Name: COLUMN cuota_respuesta.total_actual; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.cuota_respuesta.total_actual IS 'Contador de respuestas completadas. Actualizado atómicamente al registrar cada respuesta.';


--
-- Name: COLUMN cuota_respuesta.cerrar_al_alcanzar; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.cuota_respuesta.cerrar_al_alcanzar IS 'TRUE = dejar de aceptar respuestas al alcanzar el límite. FALSE = solo alertar.';


--
-- Name: detalle_respuesta; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.detalle_respuesta (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    respuesta_id uuid NOT NULL,
    pregunta_id uuid NOT NULL,
    valor_texto text,
    valor_numero numeric,
    valor_booleano boolean,
    valor_fecha timestamp with time zone,
    valor_json jsonb
);


ALTER TABLE public.detalle_respuesta OWNER TO postgres;

--
-- Name: TABLE detalle_respuesta; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.detalle_respuesta IS 'Respuesta individual a una pregunta. Se llena solo la columna de valor que corresponde al tipo de la pregunta.';


--
-- Name: COLUMN detalle_respuesta.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.detalle_respuesta.id IS 'Identificador único del detalle.';


--
-- Name: COLUMN detalle_respuesta.respuesta_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.detalle_respuesta.respuesta_id IS 'Sesión de respuesta a la que pertenece este detalle.';


--
-- Name: COLUMN detalle_respuesta.pregunta_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.detalle_respuesta.pregunta_id IS 'Pregunta que fue respondida.';


--
-- Name: COLUMN detalle_respuesta.valor_texto; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.detalle_respuesta.valor_texto IS 'Respuesta en texto. Usado para TEXTO y SELECCION_UNICA (almacena el valor de opcion_pregunta).';


--
-- Name: COLUMN detalle_respuesta.valor_numero; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.detalle_respuesta.valor_numero IS 'Respuesta numérica. Usado para NUMERO, ESCALA, NPS y CALIFICACION.';


--
-- Name: COLUMN detalle_respuesta.valor_booleano; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.detalle_respuesta.valor_booleano IS 'Respuesta booleana. Usado para BOOLEANO (sí/no).';


--
-- Name: COLUMN detalle_respuesta.valor_fecha; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.detalle_respuesta.valor_fecha IS 'Respuesta de fecha/hora. Usado para FECHA.';


--
-- Name: COLUMN detalle_respuesta.valor_json; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.detalle_respuesta.valor_json IS 'Respuesta en JSON. Usado para SELECCION_MULTIPLE, RANKING y MATRIZ.';


--
-- Name: dimension_pregunta; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.dimension_pregunta (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    encuesta_id uuid NOT NULL,
    nombre character varying(200) NOT NULL,
    descripcion text,
    peso numeric DEFAULT 1.0 NOT NULL,
    orden integer NOT NULL
);


ALTER TABLE public.dimension_pregunta OWNER TO postgres;

--
-- Name: TABLE dimension_pregunta; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.dimension_pregunta IS 'Eje temático para análisis estadístico. Agrupa preguntas para calcular puntajes por área (ej: Liderazgo, Clima Laboral). Diferente de seccion_encuesta, que es para navegación visual.';


--
-- Name: COLUMN dimension_pregunta.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.dimension_pregunta.id IS 'Identificador único de la dimensión.';


--
-- Name: COLUMN dimension_pregunta.encuesta_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.dimension_pregunta.encuesta_id IS 'Encuesta a la que pertenece esta dimensión.';


--
-- Name: COLUMN dimension_pregunta.nombre; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.dimension_pregunta.nombre IS 'Nombre del eje temático (ej: "Liderazgo", "Comunicación", "Satisfacción").';


--
-- Name: COLUMN dimension_pregunta.descripcion; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.dimension_pregunta.descripcion IS 'Descripción del eje y qué aspectos mide.';


--
-- Name: COLUMN dimension_pregunta.peso; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.dimension_pregunta.peso IS 'Peso relativo de la dimensión en el índice compuesto. La suma de pesos debe ser 1.0 para índices porcentuales.';


--
-- Name: COLUMN dimension_pregunta.orden; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.dimension_pregunta.orden IS 'Orden de presentación en reportes y dashboards.';


--
-- Name: encuesta; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.encuesta (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    organizacion_id uuid NOT NULL,
    titulo character varying(300) NOT NULL,
    descripcion text,
    version integer DEFAULT 1 NOT NULL,
    estado character varying(50) DEFAULT 'BORRADOR'::character varying NOT NULL,
    es_global boolean DEFAULT false NOT NULL,
    es_plantilla boolean DEFAULT false NOT NULL,
    plantilla_origen_id uuid,
    etiquetas_json jsonb,
    creado_por_usuario_id uuid,
    fecha_inicio timestamp with time zone,
    fecha_fin timestamp with time zone,
    publicado_en timestamp with time zone,
    configuracion_json jsonb,
    creado_en timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.encuesta OWNER TO postgres;

--
-- Name: TABLE encuesta; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.encuesta IS 'Encuesta creada por una organización. Ciclo de vida: BORRADOR → PUBLICADA → CERRADA.';


--
-- Name: COLUMN encuesta.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.encuesta.id IS 'Identificador único de la encuesta.';


--
-- Name: COLUMN encuesta.organizacion_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.encuesta.organizacion_id IS 'Organización propietaria de la encuesta.';


--
-- Name: COLUMN encuesta.titulo; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.encuesta.titulo IS 'Título visible de la encuesta para respondentes y administradores.';


--
-- Name: COLUMN encuesta.descripcion; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.encuesta.descripcion IS 'Descripción o instrucciones generales mostradas al inicio de la encuesta.';


--
-- Name: COLUMN encuesta.version; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.encuesta.version IS 'Versión estructural. Se incrementa cuando se modifican preguntas de una encuesta ya publicada para mantener integridad histórica.';


--
-- Name: COLUMN encuesta.estado; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.encuesta.estado IS 'Estado del ciclo de vida: BORRADOR, PUBLICADA, CERRADA.';


--
-- Name: COLUMN encuesta.es_global; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.encuesta.es_global IS 'TRUE = encuesta aplica a toda la organización sin filtro de alcance. FALSE = el alcance se define en alcance_encuesta.';


--
-- Name: COLUMN encuesta.es_plantilla; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.encuesta.es_plantilla IS 'TRUE = esta encuesta es una plantilla reutilizable, no se distribuye directamente.';


--
-- Name: COLUMN encuesta.plantilla_origen_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.encuesta.plantilla_origen_id IS 'Encuesta plantilla de la que se originó esta por clonación. NULL si fue creada desde cero.';


--
-- Name: COLUMN encuesta.etiquetas_json; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.encuesta.etiquetas_json IS 'Etiquetas para categorización y filtrado. Ej: ["nps","clima-laboral","2024-q1"].';


--
-- Name: COLUMN encuesta.creado_por_usuario_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.encuesta.creado_por_usuario_id IS 'Usuario que creó la encuesta.';


--
-- Name: COLUMN encuesta.fecha_inicio; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.encuesta.fecha_inicio IS 'Fecha desde la que la encuesta acepta respuestas. NULL si no tiene restricción de inicio.';


--
-- Name: COLUMN encuesta.fecha_fin; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.encuesta.fecha_fin IS 'Fecha hasta la que la encuesta acepta respuestas. NULL si no tiene fecha de cierre automático.';


--
-- Name: COLUMN encuesta.publicado_en; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.encuesta.publicado_en IS 'Fecha y hora en que la encuesta fue publicada. NULL si aún no se ha publicado.';


--
-- Name: COLUMN encuesta.configuracion_json; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.encuesta.configuracion_json IS 'Configuración de comportamiento: {"esAnonima":false,"permitirMultiplesRespuestas":false,"mostrarProgreso":true,"modoNavegacion":"lineal"}.';


--
-- Name: COLUMN encuesta.creado_en; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.encuesta.creado_en IS 'Fecha y hora de creación de la encuesta.';


--
-- Name: entidad; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.entidad (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    organizacion_id uuid NOT NULL,
    tipo_entidad_id uuid NOT NULL,
    entidad_padre_id uuid,
    nombre_visible character varying(300) NOT NULL,
    id_externo character varying(100),
    es_activo boolean DEFAULT true NOT NULL,
    atributos_json jsonb,
    creado_en timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.entidad OWNER TO postgres;

--
-- Name: TABLE entidad; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.entidad IS 'Entidad genérica evaluable y nodo organizacional. Soporta jerarquía padre-hijo para modelar Empresa → Regional → Unidad sin tablas adicionales.';


--
-- Name: COLUMN entidad.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.entidad.id IS 'Identificador único de la entidad.';


--
-- Name: COLUMN entidad.organizacion_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.entidad.organizacion_id IS 'Organización a la que pertenece esta entidad.';


--
-- Name: COLUMN entidad.tipo_entidad_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.entidad.tipo_entidad_id IS 'Tipo de entidad según el catálogo tipo_entidad (EMPRESA, REGIONAL, UNIDAD, EMPLEADO, CLIENTE, etc.).';


--
-- Name: COLUMN entidad.entidad_padre_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.entidad.entidad_padre_id IS 'Entidad padre en la jerarquía organizacional. NULL indica nodo raíz. Permite modelar Empresa → Regional → Unidad de forma recursiva.';


--
-- Name: COLUMN entidad.nombre_visible; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.entidad.nombre_visible IS 'Nombre que se muestra en la interfaz y en reportes.';


--
-- Name: COLUMN entidad.id_externo; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.entidad.id_externo IS 'Identificador en el sistema externo de origen (RRHH, CRM, ERP). Sirve para sincronización.';


--
-- Name: COLUMN entidad.es_activo; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.entidad.es_activo IS 'Indica si la entidad está activa y disponible para ser evaluada.';


--
-- Name: COLUMN entidad.atributos_json; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.entidad.atributos_json IS 'Atributos adicionales específicos del tipo (ej: {"ciudad":"Lima","region":"Sur"} para una sucursal, {"cargo":"Gerente"} para un empleado).';


--
-- Name: COLUMN entidad.creado_en; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.entidad.creado_en IS 'Fecha y hora de creación del registro.';


--
-- Name: invitacion; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.invitacion (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    encuesta_id uuid NOT NULL,
    cuenta_usuario_id uuid,
    correo_destino character varying(200),
    entidad_evaluada_id uuid,
    token_acceso uuid DEFAULT gen_random_uuid() NOT NULL,
    canal character varying(50) DEFAULT 'EMAIL'::character varying NOT NULL,
    estado character varying(50) DEFAULT 'PENDIENTE'::character varying NOT NULL,
    enviado_en timestamp with time zone,
    vence_en timestamp with time zone,
    respondido_en timestamp with time zone,
    CONSTRAINT ck_invitacion_destinatario CHECK (((cuenta_usuario_id IS NOT NULL) OR (correo_destino IS NOT NULL) OR ((canal)::text = ANY ((ARRAY['ENLACE_PUBLICO'::character varying, 'QR'::character varying, 'KIOSCO'::character varying])::text[]))))
);


ALTER TABLE public.invitacion OWNER TO postgres;

--
-- Name: TABLE invitacion; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.invitacion IS 'Gestión de distribución de la encuesta. Registra cada invitación enviada, su canal, estado y token de acceso único.';


--
-- Name: COLUMN invitacion.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.invitacion.id IS 'Identificador único de la invitación.';


--
-- Name: COLUMN invitacion.encuesta_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.invitacion.encuesta_id IS 'Encuesta a la que corresponde la invitación.';


--
-- Name: COLUMN invitacion.cuenta_usuario_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.invitacion.cuenta_usuario_id IS 'Usuario invitado con cuenta en el sistema. NULL si se invita por correo sin cuenta.';


--
-- Name: COLUMN invitacion.correo_destino; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.invitacion.correo_destino IS 'Correo del destinatario cuando no tiene cuenta. Al menos uno entre cuenta_usuario_id y correo_destino debe estar presente.';


--
-- Name: COLUMN invitacion.entidad_evaluada_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.invitacion.entidad_evaluada_id IS 'Entidad que este respondente debe evaluar. NULL si la encuesta es general.';


--
-- Name: COLUMN invitacion.token_acceso; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.invitacion.token_acceso IS 'Token UUID único para acceso mediante enlace. Permite responder sin iniciar sesión.';


--
-- Name: COLUMN invitacion.canal; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.invitacion.canal IS 'Canal de distribución: EMAIL, SMS, QR, ENLACE_PUBLICO, KIOSCO, API.';


--
-- Name: COLUMN invitacion.estado; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.invitacion.estado IS 'Estado: PENDIENTE, RESPONDIDA, EXPIRADA, CANCELADA.';


--
-- Name: COLUMN invitacion.enviado_en; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.invitacion.enviado_en IS 'Fecha y hora en que se envió la invitación al destinatario.';


--
-- Name: COLUMN invitacion.vence_en; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.invitacion.vence_en IS 'Fecha de expiración del token. NULL si no tiene vencimiento propio.';


--
-- Name: COLUMN invitacion.respondido_en; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.invitacion.respondido_en IS 'Fecha y hora en que el destinatario completó la encuesta.';


--
-- Name: notificacion_envio; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.notificacion_envio (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    invitacion_id uuid NOT NULL,
    tipo character varying(50) NOT NULL,
    canal character varying(50) NOT NULL,
    destinatario character varying(200) NOT NULL,
    estado character varying(50) DEFAULT 'PENDIENTE'::character varying NOT NULL,
    intentos_envio integer DEFAULT 0 NOT NULL,
    enviado_en timestamp with time zone,
    entregado_en timestamp with time zone,
    error_detalle text,
    creado_en timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.notificacion_envio OWNER TO postgres;

--
-- Name: TABLE notificacion_envio; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.notificacion_envio IS 'Historial de todos los mensajes enviados en el proceso de distribución (invitaciones, recordatorios, confirmaciones). Append-only.';


--
-- Name: COLUMN notificacion_envio.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.notificacion_envio.id IS 'Identificador único del envío.';


--
-- Name: COLUMN notificacion_envio.invitacion_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.notificacion_envio.invitacion_id IS 'Invitación a la que corresponde este envío.';


--
-- Name: COLUMN notificacion_envio.tipo; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.notificacion_envio.tipo IS 'Tipo de mensaje: INVITACION, RECORDATORIO, CONFIRMACION, AGRADECIMIENTO.';


--
-- Name: COLUMN notificacion_envio.canal; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.notificacion_envio.canal IS 'Canal de envío: EMAIL, SMS.';


--
-- Name: COLUMN notificacion_envio.destinatario; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.notificacion_envio.destinatario IS 'Dirección de destino (correo o número de teléfono).';


--
-- Name: COLUMN notificacion_envio.estado; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.notificacion_envio.estado IS 'Estado del envío: PENDIENTE, ENVIADO, ENTREGADO, FALLIDO, REBOTADO.';


--
-- Name: COLUMN notificacion_envio.intentos_envio; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.notificacion_envio.intentos_envio IS 'Número de intentos de envío realizados.';


--
-- Name: COLUMN notificacion_envio.enviado_en; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.notificacion_envio.enviado_en IS 'Fecha y hora en que el mensaje salió del sistema.';


--
-- Name: COLUMN notificacion_envio.entregado_en; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.notificacion_envio.entregado_en IS 'Fecha y hora de confirmación de entrega (webhook del proveedor). NULL si no hay confirmación.';


--
-- Name: COLUMN notificacion_envio.error_detalle; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.notificacion_envio.error_detalle IS 'Detalle del error en caso de fallo. NULL si el envío fue exitoso.';


--
-- Name: COLUMN notificacion_envio.creado_en; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.notificacion_envio.creado_en IS 'Fecha y hora en que se generó la notificación.';


--
-- Name: objetivo_respuesta; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.objetivo_respuesta (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    respuesta_id uuid NOT NULL,
    entidad_id uuid NOT NULL,
    tipo_relacion character varying(100) NOT NULL
);


ALTER TABLE public.objetivo_respuesta OWNER TO postgres;

--
-- Name: TABLE objetivo_respuesta; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.objetivo_respuesta IS 'Entidad(es) evaluadas en una sesión de respuesta. Permite evaluar múltiples entidades en un mismo envío.';


--
-- Name: COLUMN objetivo_respuesta.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.objetivo_respuesta.id IS 'Identificador único del objetivo.';


--
-- Name: COLUMN objetivo_respuesta.respuesta_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.objetivo_respuesta.respuesta_id IS 'Sesión de respuesta a la que pertenece este objetivo.';


--
-- Name: COLUMN objetivo_respuesta.entidad_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.objetivo_respuesta.entidad_id IS 'Entidad que fue evaluada.';


--
-- Name: COLUMN objetivo_respuesta.tipo_relacion; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.objetivo_respuesta.tipo_relacion IS 'Rol de la entidad evaluada: EVALUADO (sujeto principal), CONTEXTO, EVENTO, etc.';


--
-- Name: opcion_pregunta; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.opcion_pregunta (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    pregunta_id uuid NOT NULL,
    etiqueta character varying(300) NOT NULL,
    valor character varying(300) NOT NULL,
    puntaje numeric,
    orden integer NOT NULL
);


ALTER TABLE public.opcion_pregunta OWNER TO postgres;

--
-- Name: TABLE opcion_pregunta; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.opcion_pregunta IS 'Opciones de respuesta para preguntas SELECCION_UNICA o SELECCION_MULTIPLE.';


--
-- Name: COLUMN opcion_pregunta.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.opcion_pregunta.id IS 'Identificador único de la opción.';


--
-- Name: COLUMN opcion_pregunta.pregunta_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.opcion_pregunta.pregunta_id IS 'Pregunta a la que pertenece esta opción.';


--
-- Name: COLUMN opcion_pregunta.etiqueta; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.opcion_pregunta.etiqueta IS 'Texto visible de la opción para el respondente.';


--
-- Name: COLUMN opcion_pregunta.valor; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.opcion_pregunta.valor IS 'Valor interno almacenado cuando se selecciona esta opción.';


--
-- Name: COLUMN opcion_pregunta.puntaje; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.opcion_pregunta.puntaje IS 'Puntaje numérico para cálculos estadísticos (ej: Likert: Muy de acuerdo=5). NULL si la pregunta no tiene puntaje.';


--
-- Name: COLUMN opcion_pregunta.orden; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.opcion_pregunta.orden IS 'Posición de la opción dentro de la lista.';


--
-- Name: organizacion; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.organizacion (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    nombre character varying(200) NOT NULL,
    url_logo text,
    creado_en timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.organizacion OWNER TO postgres;

--
-- Name: TABLE organizacion; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.organizacion IS 'Raíz del tenant. Cada organización es un cliente independiente del sistema.';


--
-- Name: COLUMN organizacion.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.organizacion.id IS 'Identificador único de la organización.';


--
-- Name: COLUMN organizacion.nombre; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.organizacion.nombre IS 'Nombre comercial o razón social de la organización.';


--
-- Name: COLUMN organizacion.url_logo; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.organizacion.url_logo IS 'URL pública del logotipo de la organización.';


--
-- Name: COLUMN organizacion.creado_en; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.organizacion.creado_en IS 'Fecha y hora en que se registró la organización.';


--
-- Name: permiso; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.permiso (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    codigo character varying(100) NOT NULL,
    nombre character varying(200) NOT NULL
);


ALTER TABLE public.permiso OWNER TO postgres;

--
-- Name: TABLE permiso; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.permiso IS 'Catálogo de permisos del sistema. Cada permiso representa una acción específica.';


--
-- Name: COLUMN permiso.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.permiso.id IS 'Identificador único del permiso.';


--
-- Name: COLUMN permiso.codigo; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.permiso.codigo IS 'Clave técnica del permiso (ej: encuesta.publicar, reporte.exportar). Usada en validaciones de código.';


--
-- Name: COLUMN permiso.nombre; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.permiso.nombre IS 'Nombre legible del permiso.';


--
-- Name: pregunta; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.pregunta (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    encuesta_id uuid NOT NULL,
    seccion_id uuid,
    dimension_id uuid,
    tipo character varying(100) NOT NULL,
    titulo text NOT NULL,
    descripcion text,
    orden integer NOT NULL,
    peso numeric DEFAULT 1.0 NOT NULL,
    es_obligatoria boolean DEFAULT false NOT NULL,
    configuracion_json jsonb,
    creado_en timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.pregunta OWNER TO postgres;

--
-- Name: TABLE pregunta; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.pregunta IS 'Pregunta perteneciente a una encuesta. El tipo determina el componente de entrada y la columna de valor en detalle_respuesta.';


--
-- Name: COLUMN pregunta.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.pregunta.id IS 'Identificador único de la pregunta.';


--
-- Name: COLUMN pregunta.encuesta_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.pregunta.encuesta_id IS 'Encuesta a la que pertenece esta pregunta.';


--
-- Name: COLUMN pregunta.seccion_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.pregunta.seccion_id IS 'Sección a la que pertenece esta pregunta. NULL si la encuesta no usa secciones.';


--
-- Name: COLUMN pregunta.dimension_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.pregunta.dimension_id IS 'Dimensión temática para análisis estadístico. NULL si la pregunta no forma parte de ningún eje analítico.';


--
-- Name: COLUMN pregunta.tipo; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.pregunta.tipo IS 'Tipo de pregunta: TEXTO, NUMERO, SELECCION_UNICA, SELECCION_MULTIPLE, ESCALA, FECHA, BOOLEANO, NPS, CALIFICACION, RANKING, MATRIZ.';


--
-- Name: COLUMN pregunta.titulo; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.pregunta.titulo IS 'Texto de la pregunta mostrado al respondente.';


--
-- Name: COLUMN pregunta.descripcion; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.pregunta.descripcion IS 'Instrucción o aclaración adicional debajo del título.';


--
-- Name: COLUMN pregunta.orden; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.pregunta.orden IS 'Posición de la pregunta dentro de la sección (o encuesta si no hay secciones).';


--
-- Name: COLUMN pregunta.peso; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.pregunta.peso IS 'Peso de la pregunta dentro de su dimensión para el cálculo del puntaje.';


--
-- Name: COLUMN pregunta.es_obligatoria; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.pregunta.es_obligatoria IS 'Indica si la pregunta debe ser respondida para poder avanzar o enviar.';


--
-- Name: COLUMN pregunta.configuracion_json; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.pregunta.configuracion_json IS 'Configuración específica del tipo (ej: {"min":1,"max":5,"etiquetaMin":"Muy malo","etiquetaMax":"Excelente"}).';


--
-- Name: COLUMN pregunta.creado_en; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.pregunta.creado_en IS 'Fecha y hora de creación de la pregunta.';


--
-- Name: regla_encuesta; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.regla_encuesta (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    encuesta_id uuid NOT NULL,
    regla_json jsonb NOT NULL
);


ALTER TABLE public.regla_encuesta OWNER TO postgres;

--
-- Name: TABLE regla_encuesta; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.regla_encuesta IS 'Reglas de lógica condicional de la encuesta. Evaluadas en tiempo de ejecución al navegar entre preguntas.';


--
-- Name: COLUMN regla_encuesta.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.regla_encuesta.id IS 'Identificador único de la regla.';


--
-- Name: COLUMN regla_encuesta.encuesta_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.regla_encuesta.encuesta_id IS 'Encuesta a la que aplica esta regla.';


--
-- Name: COLUMN regla_encuesta.regla_json; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.regla_encuesta.regla_json IS 'Regla en JSON: {"si":{"preguntaId":"...","operador":"igual","valor":"SI"},"entonces":{"accion":"mostrar","preguntaObjetivoId":"..."}}. Operadores: igual|distinto|mayor|menor|contiene. Acciones: mostrar|ocultar|saltar.';


--
-- Name: respuesta; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.respuesta (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    encuesta_id uuid NOT NULL,
    version_encuesta integer DEFAULT 1 NOT NULL,
    invitacion_id uuid,
    usuario_respondent_id uuid,
    canal character varying(50),
    ultima_pregunta_id uuid,
    peso_estadistico numeric DEFAULT 1.0 NOT NULL,
    consentimiento_otorgado boolean,
    fecha_consentimiento timestamp with time zone,
    iniciado_en timestamp with time zone,
    completado_en timestamp with time zone,
    info_dispositivo text,
    direccion_ip character varying(100)
);


ALTER TABLE public.respuesta OWNER TO postgres;

--
-- Name: TABLE respuesta; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.respuesta IS 'Sesión de respuesta a una encuesta. Representa un intento completo o parcial de un respondente.';


--
-- Name: COLUMN respuesta.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.respuesta.id IS 'Identificador único de la sesión de respuesta.';


--
-- Name: COLUMN respuesta.encuesta_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.respuesta.encuesta_id IS 'Encuesta que se está respondiendo.';


--
-- Name: COLUMN respuesta.version_encuesta; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.respuesta.version_encuesta IS 'Versión de la encuesta al momento de responder. Garantiza integridad histórica.';


--
-- Name: COLUMN respuesta.invitacion_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.respuesta.invitacion_id IS 'Invitación que originó esta respuesta. NULL si fue por enlace público o acceso directo.';


--
-- Name: COLUMN respuesta.usuario_respondent_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.respuesta.usuario_respondent_id IS 'Usuario que responde. NULL para encuestas anónimas.';


--
-- Name: COLUMN respuesta.canal; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.respuesta.canal IS 'Canal por el que llegó la respuesta: EMAIL, SMS, QR, ENLACE_PUBLICO, KIOSCO, API.';


--
-- Name: COLUMN respuesta.ultima_pregunta_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.respuesta.ultima_pregunta_id IS 'Última pregunta vista o respondida. Permite análisis de abandono.';


--
-- Name: COLUMN respuesta.peso_estadistico; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.respuesta.peso_estadistico IS 'Factor de ponderación para análisis estadístico ponderado. DEFAULT 1.0 = sin ponderación.';


--
-- Name: COLUMN respuesta.consentimiento_otorgado; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.respuesta.consentimiento_otorgado IS 'El respondente aceptó el aviso de privacidad/GDPR. NULL si la encuesta no requirió consentimiento explícito.';


--
-- Name: COLUMN respuesta.fecha_consentimiento; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.respuesta.fecha_consentimiento IS 'Fecha y hora en que se otorgó el consentimiento.';


--
-- Name: COLUMN respuesta.iniciado_en; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.respuesta.iniciado_en IS 'Fecha y hora en que el respondente comenzó la encuesta.';


--
-- Name: COLUMN respuesta.completado_en; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.respuesta.completado_en IS 'Fecha y hora en que se envió la encuesta. NULL si está en progreso o fue abandonada.';


--
-- Name: COLUMN respuesta.info_dispositivo; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.respuesta.info_dispositivo IS 'User-agent del dispositivo usado. Para auditoría.';


--
-- Name: COLUMN respuesta.direccion_ip; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.respuesta.direccion_ip IS 'Dirección IP del respondente al momento del envío. Para auditoría.';


--
-- Name: rol; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.rol (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    organizacion_id uuid NOT NULL,
    nombre character varying(100) NOT NULL
);


ALTER TABLE public.rol OWNER TO postgres;

--
-- Name: TABLE rol; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.rol IS 'Rol de acceso dentro de una organización. Agrupa permisos asignables a usuarios.';


--
-- Name: COLUMN rol.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.rol.id IS 'Identificador único del rol.';


--
-- Name: COLUMN rol.organizacion_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.rol.organizacion_id IS 'Organización propietaria del rol.';


--
-- Name: COLUMN rol.nombre; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.rol.nombre IS 'Nombre descriptivo del rol (ej: Administrador, Analista, Respondente).';


--
-- Name: rol_permiso; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.rol_permiso (
    rol_id uuid NOT NULL,
    permiso_id uuid NOT NULL
);


ALTER TABLE public.rol_permiso OWNER TO postgres;

--
-- Name: TABLE rol_permiso; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.rol_permiso IS 'Asignación de permisos a roles. Define qué acciones puede realizar cada rol.';


--
-- Name: COLUMN rol_permiso.rol_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.rol_permiso.rol_id IS 'Rol al que se le asigna el permiso.';


--
-- Name: COLUMN rol_permiso.permiso_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.rol_permiso.permiso_id IS 'Permiso asignado al rol.';


--
-- Name: seccion_encuesta; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.seccion_encuesta (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    encuesta_id uuid NOT NULL,
    titulo character varying(300) NOT NULL,
    descripcion text,
    orden integer NOT NULL
);


ALTER TABLE public.seccion_encuesta OWNER TO postgres;

--
-- Name: TABLE seccion_encuesta; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.seccion_encuesta IS 'Sección o página dentro de una encuesta. Permite organizar preguntas en bloques y habilitar paginación.';


--
-- Name: COLUMN seccion_encuesta.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.seccion_encuesta.id IS 'Identificador único de la sección.';


--
-- Name: COLUMN seccion_encuesta.encuesta_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.seccion_encuesta.encuesta_id IS 'Encuesta a la que pertenece esta sección.';


--
-- Name: COLUMN seccion_encuesta.titulo; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.seccion_encuesta.titulo IS 'Título visible de la sección (ej: "Datos generales", "Evaluación de desempeño").';


--
-- Name: COLUMN seccion_encuesta.descripcion; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.seccion_encuesta.descripcion IS 'Instrucción o contexto mostrado al inicio de la sección.';


--
-- Name: COLUMN seccion_encuesta.orden; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.seccion_encuesta.orden IS 'Posición de la sección dentro de la encuesta.';


--
-- Name: tipo_entidad; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.tipo_entidad (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    codigo character varying(100) NOT NULL,
    nombre character varying(200) NOT NULL
);


ALTER TABLE public.tipo_entidad OWNER TO postgres;

--
-- Name: TABLE tipo_entidad; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.tipo_entidad IS 'Catálogo de tipos de entidad soportados. Define qué clases de objetos pueden ser evaluados.';


--
-- Name: COLUMN tipo_entidad.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.tipo_entidad.id IS 'Identificador único del tipo de entidad.';


--
-- Name: COLUMN tipo_entidad.codigo; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.tipo_entidad.codigo IS 'Clave técnica única del tipo (ej: EMPLEADO, SUCURSAL). Usada en lógica de aplicación.';


--
-- Name: COLUMN tipo_entidad.nombre; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.tipo_entidad.nombre IS 'Nombre legible del tipo de entidad.';


--
-- Name: usuario_rol; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.usuario_rol (
    usuario_id uuid NOT NULL,
    rol_id uuid NOT NULL
);


ALTER TABLE public.usuario_rol OWNER TO postgres;

--
-- Name: TABLE usuario_rol; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.usuario_rol IS 'Asignación de roles a usuarios. Un usuario puede tener múltiples roles.';


--
-- Name: COLUMN usuario_rol.usuario_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.usuario_rol.usuario_id IS 'Usuario al que se le asigna el rol.';


--
-- Name: COLUMN usuario_rol.rol_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.usuario_rol.rol_id IS 'Rol asignado al usuario.';


--
-- Data for Name: adjunto; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.adjunto (id, entidad_id, nombre_archivo, url_archivo, subido_en) FROM stdin;
\.


--
-- Data for Name: alcance_encuesta; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.alcance_encuesta (id, encuesta_id, entidad_id, tipo_relacion, incluir_descendientes) FROM stdin;
\.


--
-- Data for Name: colaborador_encuesta; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.colaborador_encuesta (id, encuesta_id, usuario_id, rol, invitado_en) FROM stdin;
\.


--
-- Data for Name: comentario; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.comentario (id, entidad_id, respuesta_id, usuario_id, texto_comentario, creado_en) FROM stdin;
\.


--
-- Data for Name: cuenta_usuario; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cuenta_usuario (id, organizacion_id, entidad_id, correo, hash_contrasena, es_activo, creado_en, es_cuenta_servicio) FROM stdin;
800f0a0c-76cf-4cf0-b69b-f197e552267a	a83fc3c1-860a-4fd1-948b-aa4adab76f20	\N	admin@empresa.com	$pbkdf2-sha512$60000$e1jMeZisARcyN5rpvHu77cs2SJM0TQ0pdTMSiCQ4Iu+magLu81XajYYchjSeqiy9BmFqN815QW1HbXIsuivPgC7WAb8w/VKLmryyTYaK1Hk=	t	2026-05-19 19:56:58.1409+00	f
\.


--
-- Data for Name: cuota_respuesta; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cuota_respuesta (id, encuesta_id, entidad_id, limite, total_actual, cerrar_al_alcanzar) FROM stdin;
\.


--
-- Data for Name: detalle_respuesta; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.detalle_respuesta (id, respuesta_id, pregunta_id, valor_texto, valor_numero, valor_booleano, valor_fecha, valor_json) FROM stdin;
972e4778-6810-441d-9e06-56ab80244c16	bf940440-1567-4285-ae91-8a860c9a5c91	986fbd2f-3efc-45a5-9ee7-dd3f3b40f745	todo bien	\N	\N	\N	\N
558eef7b-76b3-4ff1-8d9a-b27a1a2d94dd	bf940440-1567-4285-ae91-8a860c9a5c91	433d7faf-eb41-4524-a190-79cf41988142	\N	20	\N	\N	\N
7943b6a9-680d-43df-930c-3ab502763ddc	bf940440-1567-4285-ae91-8a860c9a5c91	899ebd78-2631-4f0e-9865-101e74cd708e	regular	\N	\N	\N	\N
7849834c-8fd3-4e41-b596-1f4b4e532767	bf940440-1567-4285-ae91-8a860c9a5c91	c08b46ca-e0ac-4825-9646-eeba674f8c21	\N	\N	\N	\N	["telefono"]
38c91062-9af6-4144-aac8-a4267c1a808a	bf940440-1567-4285-ae91-8a860c9a5c91	5564626a-15b8-4e74-b946-7af3f48d109b	\N	7	\N	\N	\N
e7690bb3-4539-4d44-918a-a08ab9bc5d73	bf940440-1567-4285-ae91-8a860c9a5c91	2eb8d3e4-2884-494f-87c3-1c39d4c70a12	\N	6	\N	\N	\N
827790d0-5e0a-4d4c-8ca1-af18dfac2fbb	bf940440-1567-4285-ae91-8a860c9a5c91	630ebcef-d01d-4b12-b768-a777d79c8af4	\N	4	\N	\N	\N
ba3188c8-ba98-449d-9059-40610992aba8	bf940440-1567-4285-ae91-8a860c9a5c91	3a812041-a733-401d-a1cc-474c0489600d	\N	\N	\N	2026-05-18 00:00:00+00	\N
d7c31197-1dcb-415f-badc-4e410587b6e7	bf940440-1567-4285-ae91-8a860c9a5c91	c97f9608-3eb3-4092-98ad-ec8c84462b0c	\N	\N	t	\N	\N
d96df587-1691-4c10-8a19-7e92258924e2	bf940440-1567-4285-ae91-8a860c9a5c91	293ea7f3-48df-4234-a941-474105abd128	\N	\N	\N	\N	["precio", "atencion", "facilidad", "rapidez", "calidad"]
259177a1-c257-442b-8e11-6cc61bce8e61	bf940440-1567-4285-ae91-8a860c9a5c91	8a2f6a74-322f-4f2a-b039-fadca84d3759	\N	\N	\N	\N	{"Tiempo de respuesta": "excelente", "Atención al cliente": "regular", "Calidad del producto": "bueno", "Relación precio-calidad": "bueno"}
f75f699b-c316-4cf0-8784-c6ca2b3cc083	4b02e23f-6cb8-4f2f-a2b8-bd3f23d97beb	986fbd2f-3efc-45a5-9ee7-dd3f3b40f745	buena	\N	\N	\N	\N
2f9e641e-c34e-4def-85cb-5863987183b7	4b02e23f-6cb8-4f2f-a2b8-bd3f23d97beb	433d7faf-eb41-4524-a190-79cf41988142	\N	1	\N	\N	\N
7621310e-0a41-47a5-8e68-d1469cf20973	4b02e23f-6cb8-4f2f-a2b8-bd3f23d97beb	899ebd78-2631-4f0e-9865-101e74cd708e	malo	\N	\N	\N	\N
76c347e9-09fb-4caf-a8af-45c43a75bd24	4b02e23f-6cb8-4f2f-a2b8-bd3f23d97beb	c08b46ca-e0ac-4825-9646-eeba674f8c21	\N	\N	\N	\N	["email", "presencial"]
b731aec5-c5fe-4b98-a628-7a1c7bb4e32c	4b02e23f-6cb8-4f2f-a2b8-bd3f23d97beb	5564626a-15b8-4e74-b946-7af3f48d109b	\N	10	\N	\N	\N
66596def-635a-4b0f-bd4f-fa7cdf5becfd	4b02e23f-6cb8-4f2f-a2b8-bd3f23d97beb	2eb8d3e4-2884-494f-87c3-1c39d4c70a12	\N	5	\N	\N	\N
6d0c73ff-71e4-4391-b304-e4ec30ea2abc	4b02e23f-6cb8-4f2f-a2b8-bd3f23d97beb	630ebcef-d01d-4b12-b768-a777d79c8af4	\N	1	\N	\N	\N
eff28cea-ad00-4698-8c05-5195a3046795	4b02e23f-6cb8-4f2f-a2b8-bd3f23d97beb	3a812041-a733-401d-a1cc-474c0489600d	\N	\N	\N	2026-05-01 00:00:00+00	\N
d4b95996-7fab-481d-b81a-81a36098ea48	4b02e23f-6cb8-4f2f-a2b8-bd3f23d97beb	c97f9608-3eb3-4092-98ad-ec8c84462b0c	\N	\N	t	\N	\N
8e2b082c-b371-4c31-9935-88d1ce6a6edb	4b02e23f-6cb8-4f2f-a2b8-bd3f23d97beb	293ea7f3-48df-4234-a941-474105abd128	\N	\N	\N	\N	["facilidad", "calidad", "precio", "atencion", "rapidez"]
fd697aa6-e05e-4642-9d88-13e565264009	4b02e23f-6cb8-4f2f-a2b8-bd3f23d97beb	8a2f6a74-322f-4f2a-b039-fadca84d3759	\N	\N	\N	\N	{"Tiempo de respuesta": "bueno", "Atención al cliente": "excelente", "Calidad del producto": "regular", "Relación precio-calidad": "excelente"}
e142ef8c-2bb9-4e4d-8bcd-b577c94cc9f2	bf8aec44-79bf-4521-9a97-1e86c39af3c3	986fbd2f-3efc-45a5-9ee7-dd3f3b40f745	mas omenos	\N	\N	\N	\N
cd80778a-f69f-46bd-99de-33a3dee70d44	bf8aec44-79bf-4521-9a97-1e86c39af3c3	433d7faf-eb41-4524-a190-79cf41988142	\N	5	\N	\N	\N
65e770ae-fe8c-42f9-8a53-6bae7d5a890c	bf8aec44-79bf-4521-9a97-1e86c39af3c3	899ebd78-2631-4f0e-9865-101e74cd708e	regular	\N	\N	\N	\N
3d6c0a59-7b95-42bb-af41-c3ef77b4e36b	bf8aec44-79bf-4521-9a97-1e86c39af3c3	c08b46ca-e0ac-4825-9646-eeba674f8c21	\N	\N	\N	\N	["email", "presencial"]
40682cee-0574-420d-84f9-fc1ee424f41c	bf8aec44-79bf-4521-9a97-1e86c39af3c3	5564626a-15b8-4e74-b946-7af3f48d109b	\N	6	\N	\N	\N
c1efd529-a4ab-4329-8359-6645c4c3dfc1	bf8aec44-79bf-4521-9a97-1e86c39af3c3	2eb8d3e4-2884-494f-87c3-1c39d4c70a12	\N	7	\N	\N	\N
4da5982b-d241-49c1-abc7-31925033702f	bf8aec44-79bf-4521-9a97-1e86c39af3c3	630ebcef-d01d-4b12-b768-a777d79c8af4	\N	4	\N	\N	\N
64337b56-cca6-49de-9c11-ac5867b52df5	bf8aec44-79bf-4521-9a97-1e86c39af3c3	3a812041-a733-401d-a1cc-474c0489600d	\N	\N	\N	2026-01-31 00:00:00+00	\N
9d6d4e70-46ba-4341-ac55-3821a98747d4	bf8aec44-79bf-4521-9a97-1e86c39af3c3	c97f9608-3eb3-4092-98ad-ec8c84462b0c	\N	\N	t	\N	\N
22637429-9d9f-4f38-b914-4201679f302e	bf8aec44-79bf-4521-9a97-1e86c39af3c3	293ea7f3-48df-4234-a941-474105abd128	\N	\N	\N	\N	["precio"]
39651af4-e753-4ac3-8a6a-04fb69515032	bf8aec44-79bf-4521-9a97-1e86c39af3c3	8a2f6a74-322f-4f2a-b039-fadca84d3759	\N	\N	\N	\N	{"Tiempo de respuesta": "regular", "Atención al cliente": "malo", "Calidad del producto": "malo", "Relación precio-calidad": "bueno"}
1dd7b0d6-e886-47b6-a6d7-2b54edd47552	412bd626-c83d-400c-8279-8ca067ff2721	986fbd2f-3efc-45a5-9ee7-dd3f3b40f745	saludos 	\N	\N	\N	\N
b8b37f92-4474-4354-9f53-d95ceddf0185	412bd626-c83d-400c-8279-8ca067ff2721	433d7faf-eb41-4524-a190-79cf41988142	\N	5	\N	\N	\N
35d35d06-d1a6-490a-b670-881210bd4b39	412bd626-c83d-400c-8279-8ca067ff2721	899ebd78-2631-4f0e-9865-101e74cd708e	bueno	\N	\N	\N	\N
55c30652-751a-4bac-9e9f-acb6988ac723	412bd626-c83d-400c-8279-8ca067ff2721	c08b46ca-e0ac-4825-9646-eeba674f8c21	\N	\N	\N	\N	["email"]
c964e9a3-527d-4299-b72a-f41aa1255172	412bd626-c83d-400c-8279-8ca067ff2721	5564626a-15b8-4e74-b946-7af3f48d109b	\N	6	\N	\N	\N
77c1263f-de05-427e-992d-65187155a3d6	412bd626-c83d-400c-8279-8ca067ff2721	2eb8d3e4-2884-494f-87c3-1c39d4c70a12	\N	7	\N	\N	\N
116a4346-3e2a-4d12-83f2-104fdf499c31	412bd626-c83d-400c-8279-8ca067ff2721	630ebcef-d01d-4b12-b768-a777d79c8af4	\N	4	\N	\N	\N
bbb94af2-b343-4a99-aa3b-14dfdd0b8974	412bd626-c83d-400c-8279-8ca067ff2721	3a812041-a733-401d-a1cc-474c0489600d	\N	\N	\N	2026-05-19 00:00:00+00	\N
860ac7a1-c179-4ea5-9918-1f45fafe1a2e	412bd626-c83d-400c-8279-8ca067ff2721	c97f9608-3eb3-4092-98ad-ec8c84462b0c	\N	\N	t	\N	\N
5b272a19-729c-47b7-b79d-7430f1d7d999	412bd626-c83d-400c-8279-8ca067ff2721	293ea7f3-48df-4234-a941-474105abd128	\N	\N	\N	\N	["atencion"]
94f58080-735b-4ff4-9889-f26ce6411c5a	412bd626-c83d-400c-8279-8ca067ff2721	8a2f6a74-322f-4f2a-b039-fadca84d3759	\N	\N	\N	\N	{"Tiempo de respuesta": "regular", "Atención al cliente": "excelente", "Calidad del producto": "malo", "Relación precio-calidad": "bueno"}
e3d260bb-382a-4572-b83b-53507b62b92a	4b7323c5-5975-4daa-aedd-a855a1ef7005	36a5e4e1-ad9d-4a75-b70b-e18ec98f3f01	tramite	\N	\N	\N	\N
58a44aa0-ab5c-4ad5-92fd-9ca21013efc8	4b7323c5-5975-4daa-aedd-a855a1ef7005	f7f9044f-7d27-4b3a-8d84-b73d82195fa1	\N	4	\N	\N	\N
bd749209-7586-486f-bac3-68a8be0dcc40	4b7323c5-5975-4daa-aedd-a855a1ef7005	cc6d8dc6-512f-443e-b71d-637efe34f750	si_completo	\N	\N	\N	\N
5dabc0ff-3830-40c8-bc7b-6d32bffcae10	4b7323c5-5975-4daa-aedd-a855a1ef7005	c1baf43e-1b94-4bd1-af39-405883284b5e	menos_5	\N	\N	\N	\N
473e235e-9b33-442b-a19d-fa6d3473131b	4b7323c5-5975-4daa-aedd-a855a1ef7005	c8ffe020-28b2-4c92-9c0f-2fc410166eb3	si_completo	\N	\N	\N	\N
e03c6bb1-8871-4d24-9793-24dee1ef670e	4b7323c5-5975-4daa-aedd-a855a1ef7005	9a0bdaee-670c-4e2a-8e10-949a118ee0a4	\N	5	\N	\N	\N
43dbba9e-b51d-4b0b-954f-92e992264bfd	4b7323c5-5975-4daa-aedd-a855a1ef7005	9739928a-96fd-4bb0-87fc-0f58d38a75df	nada	\N	\N	\N	\N
17273e89-9bfe-4e18-ba02-1c4b340d0005	4b7323c5-5975-4daa-aedd-a855a1ef7005	8c6368f6-a36d-49e3-8cf2-98fc04a2fdcf	ninguni	\N	\N	\N	\N
43eea47e-7f8e-42c9-b951-7f67a4d1e0e1	07ae864d-3c8d-45a9-89f5-edc3228ad647	36a5e4e1-ad9d-4a75-b70b-e18ec98f3f01	problema	\N	\N	\N	\N
5a16bc5f-d600-4422-9510-4da20fb27513	07ae864d-3c8d-45a9-89f5-edc3228ad647	f7f9044f-7d27-4b3a-8d84-b73d82195fa1	\N	4	\N	\N	\N
9bdfa047-75ec-4255-8ac4-1275102d95a0	07ae864d-3c8d-45a9-89f5-edc3228ad647	cc6d8dc6-512f-443e-b71d-637efe34f750	si_completo	\N	\N	\N	\N
54e186dd-b2e9-493d-b09f-5a9fd785e5a3	07ae864d-3c8d-45a9-89f5-edc3228ad647	c1baf43e-1b94-4bd1-af39-405883284b5e	5_15	\N	\N	\N	\N
95a2bfdb-a1cc-47cf-8031-8ee4e7995e6c	07ae864d-3c8d-45a9-89f5-edc3228ad647	c8ffe020-28b2-4c92-9c0f-2fc410166eb3	si_completo	\N	\N	\N	\N
14bc36c9-2d22-4c8c-ae5a-6201e3036197	07ae864d-3c8d-45a9-89f5-edc3228ad647	9a0bdaee-670c-4e2a-8e10-949a118ee0a4	\N	2	\N	\N	\N
fb6dedb0-9bed-454b-89dc-c286c63b78ca	07ae864d-3c8d-45a9-89f5-edc3228ad647	9739928a-96fd-4bb0-87fc-0f58d38a75df	toedo	\N	\N	\N	\N
d3fe2e65-1990-463a-b884-f857dd29a87c	07ae864d-3c8d-45a9-89f5-edc3228ad647	8c6368f6-a36d-49e3-8cf2-98fc04a2fdcf	nada	\N	\N	\N	\N
95ced74d-4938-4a0e-bc06-2eae6d654669	7a6f431c-af42-4573-95b0-4c9c79b485c9	36a5e4e1-ad9d-4a75-b70b-e18ec98f3f01	problema	\N	\N	\N	\N
89365076-523a-4a05-b267-98aa831293fa	7a6f431c-af42-4573-95b0-4c9c79b485c9	f7f9044f-7d27-4b3a-8d84-b73d82195fa1	\N	4	\N	\N	\N
d4619421-2c31-4d7d-b9b1-7d9b6e6f1f5b	7a6f431c-af42-4573-95b0-4c9c79b485c9	cc6d8dc6-512f-443e-b71d-637efe34f750	parcialmente	\N	\N	\N	\N
8177f1bb-8539-41cc-a8ab-ba028908459e	7a6f431c-af42-4573-95b0-4c9c79b485c9	c1baf43e-1b94-4bd1-af39-405883284b5e	5_15	\N	\N	\N	\N
3b075322-f9df-4f5e-a802-fa931ca243c0	7a6f431c-af42-4573-95b0-4c9c79b485c9	c8ffe020-28b2-4c92-9c0f-2fc410166eb3	parcialmente	\N	\N	\N	\N
1fb1c66a-6782-4806-9d70-ee80357ca33d	7a6f431c-af42-4573-95b0-4c9c79b485c9	9a0bdaee-670c-4e2a-8e10-949a118ee0a4	\N	5	\N	\N	\N
cc258514-c67a-4c02-a6b9-3501653658d6	7a6f431c-af42-4573-95b0-4c9c79b485c9	9739928a-96fd-4bb0-87fc-0f58d38a75df	jhf khgfhg	\N	\N	\N	\N
1a381702-b0f2-42fd-a40f-8fdcd7d31157	7a6f431c-af42-4573-95b0-4c9c79b485c9	8c6368f6-a36d-49e3-8cf2-98fc04a2fdcf	\N	\N	\N	\N	\N
35c04c15-2d0c-4753-847f-aeac41668ac3	647bca39-0a27-49d4-8f03-816803c79c19	36a5e4e1-ad9d-4a75-b70b-e18ec98f3f01	tramite	\N	\N	\N	\N
913df9e8-5077-4abb-a831-818261957762	647bca39-0a27-49d4-8f03-816803c79c19	f7f9044f-7d27-4b3a-8d84-b73d82195fa1	\N	4	\N	\N	\N
d67c9609-6ac8-402f-8f9a-e7b2e5362de2	647bca39-0a27-49d4-8f03-816803c79c19	cc6d8dc6-512f-443e-b71d-637efe34f750	parcialmente	\N	\N	\N	\N
a6dd3530-7e8c-475d-844f-00497fb95e52	647bca39-0a27-49d4-8f03-816803c79c19	c1baf43e-1b94-4bd1-af39-405883284b5e	5_15	\N	\N	\N	\N
3d8cfe15-68df-4aac-9d9c-909b632cf1da	647bca39-0a27-49d4-8f03-816803c79c19	c8ffe020-28b2-4c92-9c0f-2fc410166eb3	parcialmente	\N	\N	\N	\N
9a8140f1-afa2-47c7-aace-009196629b1f	647bca39-0a27-49d4-8f03-816803c79c19	9a0bdaee-670c-4e2a-8e10-949a118ee0a4	\N	6	\N	\N	\N
305a9109-b31b-406c-a50f-4313f9457f0f	647bca39-0a27-49d4-8f03-816803c79c19	9739928a-96fd-4bb0-87fc-0f58d38a75df	sdfsd	\N	\N	\N	\N
cb809c6b-a7d4-46cc-94d8-f1da38167685	647bca39-0a27-49d4-8f03-816803c79c19	8c6368f6-a36d-49e3-8cf2-98fc04a2fdcf	sdf	\N	\N	\N	\N
\.


--
-- Data for Name: dimension_pregunta; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.dimension_pregunta (id, encuesta_id, nombre, descripcion, peso, orden) FROM stdin;
a5dfc135-f8c6-41ef-a8c4-e0a03f9a3389	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	Amabilidad y Trato	Calidez y respeto en la atención	0.30	1
900def68-dbcf-4cdb-a270-7d2776409a87	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	Conocimiento del Servicio	Dominio técnico y claridad en la información	0.25	2
e008ecd0-b14b-4711-892f-33c1f415c870	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	Eficiencia y Tiempo	Rapidez y puntualidad en la atención	0.20	3
e7dcc572-1644-4fa1-8e97-5406db91c4d9	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	Resolución del Problema	Capacidad para resolver la necesidad del cliente	0.25	4
\.


--
-- Data for Name: encuesta; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.encuesta (id, organizacion_id, titulo, descripcion, version, estado, es_global, es_plantilla, plantilla_origen_id, etiquetas_json, creado_por_usuario_id, fecha_inicio, fecha_fin, publicado_en, configuracion_json, creado_en) FROM stdin;
5a8e01e8-b060-4644-8df6-f78ad5655b0c	a83fc3c1-860a-4fd1-948b-aa4adab76f20	Encuesta de Satisfacción Completa	Encuesta de prueba con todos los tipos de preguntas disponibles.	1	PUBLICADA	t	f	\N	\N	800f0a0c-76cf-4cf0-b69b-f197e552267a	\N	\N	2026-05-19 20:03:50.969542+00	\N	2026-05-19 20:02:05.997554+00
6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	a83fc3c1-860a-4fd1-948b-aa4adab76f20	Evaluación de Atención al Cliente 2024	Encuesta para medir la calidad del servicio brindado por nuestros empleados en todas las regionales.	1	PUBLICADA	t	f	\N	\N	800f0a0c-76cf-4cf0-b69b-f197e552267a	\N	\N	2026-05-19 21:45:44.231113+00	\N	2026-05-19 21:37:42.513467+00
\.


--
-- Data for Name: entidad; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.entidad (id, organizacion_id, tipo_entidad_id, entidad_padre_id, nombre_visible, id_externo, es_activo, atributos_json, creado_en) FROM stdin;
048c4437-22ef-4d38-9f2c-104a13646259	a83fc3c1-860a-4fd1-948b-aa4adab76f20	bac8b4fe-8169-4cb5-a237-7cde24126a2a	\N	CSBP	a	t	\N	2026-05-19 21:04:51.64503+00
ac8801d8-1133-446c-b2b2-246f8e0ce4e8	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4455c604-d852-47d9-8911-146e996dc57e	048c4437-22ef-4d38-9f2c-104a13646259	La Paz	1	t	\N	2026-05-19 21:05:36.104568+00
46253438-07a0-488e-bb1a-a280bb2e5fe6	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4455c604-d852-47d9-8911-146e996dc57e	048c4437-22ef-4d38-9f2c-104a13646259	Cochabamba	2	t	\N	2026-05-19 21:05:48.400243+00
020b0176-ea71-4995-9508-15b5b194357b	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4455c604-d852-47d9-8911-146e996dc57e	048c4437-22ef-4d38-9f2c-104a13646259	Potosi	5	t	\N	2026-05-19 21:06:18.418292+00
1ac570a1-0e57-4f6d-b97c-6178403d8319	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4455c604-d852-47d9-8911-146e996dc57e	048c4437-22ef-4d38-9f2c-104a13646259	Sucre	6	t	\N	2026-05-19 21:06:27.814867+00
a33ec182-356a-4fd0-b6da-38f464c54b12	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4455c604-d852-47d9-8911-146e996dc57e	048c4437-22ef-4d38-9f2c-104a13646259	Tarija	7	t	\N	2026-05-19 21:06:39.429072+00
1f9bdc81-dee8-4129-bacf-1b3ea2cd65de	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4455c604-d852-47d9-8911-146e996dc57e	048c4437-22ef-4d38-9f2c-104a13646259	Trinidad	8	t	\N	2026-05-19 21:06:51.164505+00
8ea46660-368d-4e2c-ae03-fecd8561d7b7	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4455c604-d852-47d9-8911-146e996dc57e	048c4437-22ef-4d38-9f2c-104a13646259	Cobija	9	t	\N	2026-05-19 21:06:59.924037+00
ce1f8131-5a73-4b95-a769-437166238ed4	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4455c604-d852-47d9-8911-146e996dc57e	048c4437-22ef-4d38-9f2c-104a13646259	Santa Cruz	3	t	\N	2026-05-19 21:05:58.794651+00
9e9b770f-ff63-4d1d-b72a-258548084b8f	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4455c604-d852-47d9-8911-146e996dc57e	048c4437-22ef-4d38-9f2c-104a13646259	Oruro	4	t	\N	2026-05-19 21:06:07.864916+00
5a11cb49-7cd2-485d-911c-e07d80f6cfdc	a83fc3c1-860a-4fd1-948b-aa4adab76f20	bac8b4fe-8169-4cb5-a237-7cde24126a2a	\N	TechCorp S.A.	\N	t	\N	2026-05-19 21:36:29.623066+00
e0454426-92af-4e40-b5c9-5aa4e28b257e	a83fc3c1-860a-4fd1-948b-aa4adab76f20	bac8b4fe-8169-4cb5-a237-7cde24126a2a	\N	TechCorp S.A.	EMPRESA-001	t	\N	2026-05-19 21:37:06.720139+00
57f41f6a-e363-4d50-86da-0a873b67b760	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4d014459-51a5-42ae-90df-c70f3c517dd5	97bcb5b8-f0dc-4a0d-813c-5fcdf12d8d2d	EMP-N01	EMP-N01	t	{"cargo": "Técnico Senior", "servicio": "Soporte Técnico"}	2026-05-19 21:37:21.442116+00
e3721370-2bdf-41fe-93d3-c660b0004fa2	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4d014459-51a5-42ae-90df-c70f3c517dd5	05e82e68-7cfa-440b-b0ee-656cbf585679	EMP-S01	EMP-S01	t	{"cargo": "Agente", "servicio": "Atención al Cliente"}	2026-05-19 21:37:21.588162+00
38b6014d-3472-45cb-901e-fdbd3a9db985	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4d014459-51a5-42ae-90df-c70f3c517dd5	05e82e68-7cfa-440b-b0ee-656cbf585679	EMP-S02	EMP-S02	t	{"cargo": "Técnico Junior", "servicio": "Soporte Técnico"}	2026-05-19 21:37:21.657985+00
28d9e0c5-3687-4ed8-9fe9-fe6660bc2b99	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4d014459-51a5-42ae-90df-c70f3c517dd5	979661af-aea7-4c8a-8aef-009097912b45	EMP-C01	EMP-C01	t	{"cargo": "Ejecutivo Senior", "servicio": "Ventas"}	2026-05-19 21:37:21.694487+00
2d4595b8-1f79-45c1-9cbf-13687e73bee5	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4d014459-51a5-42ae-90df-c70f3c517dd5	979661af-aea7-4c8a-8aef-009097912b45	EMP-C02	EMP-C02	t	{"cargo": "Supervisor", "servicio": "Atención al Cliente"}	2026-05-19 21:37:21.760149+00
22016ec7-fa94-483a-acef-10e5d3900061	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4d014459-51a5-42ae-90df-c70f3c517dd5	97bcb5b8-f0dc-4a0d-813c-5fcdf12d8d2d	EMP-N02	EMP-N02	t	{"cargo": "Ejecutivo", "servicio": "Ventas"}	2026-05-19 21:37:32.394831+00
97bcb5b8-f0dc-4a0d-813c-5fcdf12d8d2d	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4455c604-d852-47d9-8911-146e996dc57e	e0454426-92af-4e40-b5c9-5aa4e28b257e	Regional Norte	REG-NORTE	t	\N	2026-05-19 21:37:06.81569+00
979661af-aea7-4c8a-8aef-009097912b45	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4455c604-d852-47d9-8911-146e996dc57e	e0454426-92af-4e40-b5c9-5aa4e28b257e	Regional Centro	REG-CENTRO	t	\N	2026-05-19 21:37:06.90939+00
05e82e68-7cfa-440b-b0ee-656cbf585679	a83fc3c1-860a-4fd1-948b-aa4adab76f20	4455c604-d852-47d9-8911-146e996dc57e	e0454426-92af-4e40-b5c9-5aa4e28b257e	Regional Sur	REG-SUR	t	\N	2026-05-19 21:37:06.865586+00
\.


--
-- Data for Name: invitacion; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.invitacion (id, encuesta_id, cuenta_usuario_id, correo_destino, entidad_evaluada_id, token_acceso, canal, estado, enviado_en, vence_en, respondido_en) FROM stdin;
97fe35a4-518b-4536-980f-77ed9852543a	5a8e01e8-b060-4644-8df6-f78ad5655b0c	\N	\N	\N	6ded6efb-e651-4abe-969c-3acd41918ed9	ENLACE_PUBLICO	RESPONDIDA	\N	\N	2026-05-19 20:05:58.244807+00
a499b067-1fae-46c3-8ddd-21d04d805551	5a8e01e8-b060-4644-8df6-f78ad5655b0c	\N	\N	\N	02250c16-f068-4b7d-b0c2-764bb1a941f9	ENLACE_PUBLICO	RESPONDIDA	\N	\N	2026-05-19 20:06:44.990098+00
b8d339c7-27b2-4f8c-947e-c163eb5f9299	5a8e01e8-b060-4644-8df6-f78ad5655b0c	\N	\N	\N	297c6dff-c521-4644-975b-76883fb7ddd3	ENLACE_PUBLICO	RESPONDIDA	\N	\N	2026-05-19 20:35:02.619784+00
2e5470a3-0329-4bf3-9a73-eaaa5b15b32e	5a8e01e8-b060-4644-8df6-f78ad5655b0c	\N	\N	46253438-07a0-488e-bb1a-a280bb2e5fe6	8f96b9a9-e948-4ccd-9bd1-f9dd76020f56	QR	RESPONDIDA	\N	2026-05-20 17:31:00+00	2026-05-19 21:32:27.263173+00
1efe94f9-6e89-43af-aa12-ce08becc7887	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	\N	cliente2@example.com	22016ec7-fa94-483a-acef-10e5d3900061	26230302-d62b-460e-9533-0e7907429caa	EMAIL	PENDIENTE	\N	\N	\N
45091ef2-2a21-4b83-a09a-56eb8710c131	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	\N	cliente3@example.com	e3721370-2bdf-41fe-93d3-c660b0004fa2	163761ab-1d9a-4340-a6b6-57e43086530c	EMAIL	PENDIENTE	\N	\N	\N
97d1ed27-77f4-4e29-822e-1cbfeb751c1f	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	\N	cliente4@example.com	38b6014d-3472-45cb-901e-fdbd3a9db985	bf4ae438-f26b-4e15-a27a-f52b1fb8baa6	EMAIL	PENDIENTE	\N	\N	\N
f6056a7d-55ba-4044-930b-005326292ab2	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	\N	cliente6@example.com	2d4595b8-1f79-45c1-9cbf-13687e73bee5	d75e8079-3cf7-4667-a910-52357bffecda	EMAIL	PENDIENTE	\N	\N	\N
8424a6c8-feff-4d45-b971-e8a4aff5e8e6	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	\N	\N	\N	f1b5ac3e-5ef5-45f8-b638-bced075bcd48	ENLACE_PUBLICO	RESPONDIDA	\N	\N	2026-05-19 21:47:27.212865+00
2a042259-8f3f-4510-ad31-fc2d823d67f5	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	\N	\N	\N	01d2ecfd-0142-4309-8825-75e3b8b88ef7	ENLACE_PUBLICO	RESPONDIDA	\N	\N	2026-05-19 21:58:01.975291+00
18de3340-88f1-4511-8564-4282d928c442	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	\N	cliente1@example.com	57f41f6a-e363-4d50-86da-0a873b67b760	939ad13a-3be1-4f70-a746-ed9e563b36a4	EMAIL	RESPONDIDA	\N	\N	2026-05-20 00:59:00.722922+00
07c41f02-312d-425f-aec2-003f8080c27a	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	\N	cliente5@example.com	28d9e0c5-3687-4ed8-9fe9-fe6660bc2b99	ce42c20d-badb-4c88-9d6c-859792eaf1be	EMAIL	RESPONDIDA	\N	\N	2026-05-20 01:12:20.020037+00
\.


--
-- Data for Name: notificacion_envio; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.notificacion_envio (id, invitacion_id, tipo, canal, destinatario, estado, intentos_envio, enviado_en, entregado_en, error_detalle, creado_en) FROM stdin;
\.


--
-- Data for Name: objetivo_respuesta; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.objetivo_respuesta (id, respuesta_id, entidad_id, tipo_relacion) FROM stdin;
\.


--
-- Data for Name: opcion_pregunta; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.opcion_pregunta (id, pregunta_id, etiqueta, valor, puntaje, orden) FROM stdin;
26e1654b-9eed-47bd-9b25-33685f37907e	899ebd78-2631-4f0e-9865-101e74cd708e	Excelente	excelente	4	1
9ae919fb-c1f6-4b95-a0c6-c00d3fd0ea01	899ebd78-2631-4f0e-9865-101e74cd708e	Bueno	bueno	3	2
e0556cb1-a4b2-498d-b608-b8f2beaf8799	899ebd78-2631-4f0e-9865-101e74cd708e	Regular	regular	2	3
942e1795-ee80-483a-91d0-301d2d15be1f	899ebd78-2631-4f0e-9865-101e74cd708e	Malo	malo	1	4
04ee83e3-94e6-4437-a738-c97cfa8ddfc5	c08b46ca-e0ac-4825-9646-eeba674f8c21	Correo electrónico	email	\N	1
2f8047ff-7d0a-4756-8ccd-2885b38bfc1c	c08b46ca-e0ac-4825-9646-eeba674f8c21	WhatsApp	whatsapp	\N	2
fcb687c7-7397-45ae-8dfb-941c65d00a04	c08b46ca-e0ac-4825-9646-eeba674f8c21	Teléfono	telefono	\N	3
0183c0ca-0c27-4d6d-91e4-21fde4d4c675	c08b46ca-e0ac-4825-9646-eeba674f8c21	Presencial	presencial	\N	4
c43e490b-c1fa-4fa7-b1b6-0b445c67ba10	c08b46ca-e0ac-4825-9646-eeba674f8c21	Redes sociales	redes	\N	5
1a753ae1-a00b-477b-90e3-759d71cf7348	293ea7f3-48df-4234-a941-474105abd128	Precio	precio	\N	1
402401c1-0167-41fe-9b1a-12975af3d1e1	293ea7f3-48df-4234-a941-474105abd128	Calidad del producto	calidad	\N	2
f4dc24a9-b738-4f9d-b05e-13c2866e6d6a	293ea7f3-48df-4234-a941-474105abd128	Atención al cliente	atencion	\N	3
fbe1926d-4010-4940-a9d8-0b76961d5370	293ea7f3-48df-4234-a941-474105abd128	Rapidez de entrega	rapidez	\N	4
00354100-d351-4c80-9aa8-62809b72f976	293ea7f3-48df-4234-a941-474105abd128	Facilidad de uso	facilidad	\N	5
66d637a4-1940-407b-ae9f-20b9eb763536	8a2f6a74-322f-4f2a-b039-fadca84d3759	Malo	malo	1	1
79e6db68-2115-480d-a4c4-66df915f814e	8a2f6a74-322f-4f2a-b039-fadca84d3759	Regular	regular	2	2
390094e2-ac86-45bb-ac26-26009d9c84ab	8a2f6a74-322f-4f2a-b039-fadca84d3759	Bueno	bueno	3	3
653aa932-f88f-43b8-bedf-d60a8079987b	8a2f6a74-322f-4f2a-b039-fadca84d3759	Excelente	excelente	4	4
36bd4f9e-0157-486e-b9f2-c9b49bf1b98b	36a5e4e1-ad9d-4a75-b70b-e18ec98f3f01	Información general	informacion	\N	1
451eb164-9b68-455d-b9f1-66f5b9597889	36a5e4e1-ad9d-4a75-b70b-e18ec98f3f01	Resolución de problema	problema	\N	2
c33fc20e-da31-4e1c-9b66-5a852c284346	36a5e4e1-ad9d-4a75-b70b-e18ec98f3f01	Trámite / gestión	tramite	\N	3
85bcf2d2-5fd2-41a0-adb7-776f2beae78a	36a5e4e1-ad9d-4a75-b70b-e18ec98f3f01	Reclamo	reclamo	\N	4
0c5b6044-05fd-4777-adda-408d7aa7d71e	36a5e4e1-ad9d-4a75-b70b-e18ec98f3f01	Otro	otro	\N	5
96d8b6f0-c79a-4134-88f9-6a74f1b4a92e	cc6d8dc6-512f-443e-b71d-637efe34f750	Sí, completamente	si_completo	3	1
c214f447-1e8f-4a70-8666-cb56f5d17f2c	cc6d8dc6-512f-443e-b71d-637efe34f750	Parcialmente	parcialmente	2	2
86b9a432-4832-4f68-8006-aef84fccd9a2	cc6d8dc6-512f-443e-b71d-637efe34f750	No, necesitaba más información	no_suficiente	1	3
11449ec6-f8fe-4c35-b6ae-13ed9fe5161e	c1baf43e-1b94-4bd1-af39-405883284b5e	Menos de 5 minutos	menos_5	4	1
3ad26c6e-ed35-4b0d-a545-dbfb5a39378c	c1baf43e-1b94-4bd1-af39-405883284b5e	Entre 5 y 15 minutos	5_15	3	2
aa59b3d8-ae6f-4ee2-97e6-bacef47fca14	c1baf43e-1b94-4bd1-af39-405883284b5e	Entre 15 y 30 minutos	15_30	2	3
f542b733-283b-449a-9cae-15f11341590e	c1baf43e-1b94-4bd1-af39-405883284b5e	Más de 30 minutos	mas_30	1	4
856ea279-9d0f-4b91-b5f2-5eec0ca97e77	c8ffe020-28b2-4c92-9c0f-2fc410166eb3	Sí, completamente	si_completo	3	1
f419dcd9-6926-4964-be1a-aafebe9175c8	c8ffe020-28b2-4c92-9c0f-2fc410166eb3	Parcialmente	parcialmente	2	2
01d96da4-ded4-4d23-bcfc-4096c16d543d	c8ffe020-28b2-4c92-9c0f-2fc410166eb3	No fue resuelto	no_resuelto	1	3
\.


--
-- Data for Name: organizacion; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.organizacion (id, nombre, url_logo, creado_en) FROM stdin;
a83fc3c1-860a-4fd1-948b-aa4adab76f20	Mi Empresa	\N	2026-05-19 19:56:57.924853+00
\.


--
-- Data for Name: permiso; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.permiso (id, codigo, nombre) FROM stdin;
e42bb7bf-fba7-4efd-b9c7-be8e2cc2133d	encuesta.ver	Ver encuestas
4548e0f9-b1f7-452a-8fbf-9d2d8bc705d9	encuesta.crear	Crear encuestas
3d4964f5-0d8a-458e-96b8-5f5182970712	encuesta.editar	Editar encuestas
b88652db-b888-4cdb-8e61-d59d0004b679	encuesta.publicar	Publicar encuestas
e6abb45c-857d-42b2-951f-4bb767af1282	encuesta.cerrar	Cerrar encuestas
00a6523d-7086-46a8-a6af-ddc7b828f31a	encuesta.eliminar	Eliminar encuestas
8289451e-1cbf-420e-b23e-aa58736def7c	encuesta.plantilla.usar	Usar plantillas de encuesta
74589ef3-beeb-4edb-a79e-53b7860a873f	encuesta.colaborar	Colaborar en encuestas compartidas
10160e44-0f8a-43aa-a72f-2ce2673d53e7	invitacion.administrar	Administrar invitaciones
b5b74730-84fe-46bd-91db-546c88f09b8e	invitacion.recordatorio	Enviar recordatorios
1cedc278-58f1-4a1b-8143-007250234820	respuesta.ver	Ver respuestas
e8e289b3-9edc-46cc-a9d7-478b1dadb5ae	respuesta.anotar	Anotar respuestas
f161de8d-b4e4-4261-ad08-0208d0464393	reporte.ver	Ver reportes
6e6b05be-e0b4-41bc-b91a-35366c1420aa	reporte.exportar	Exportar reportes
41b6e520-b951-45e2-a676-a90c1d9900ce	reporte.estadistico	Ver reportes estadísticos avanzados
c027d9dc-6c82-4caa-a22c-abd8266ef615	cuota.administrar	Administrar cuotas de respuesta
9df41af8-c7b9-423c-93cb-d459d0523d25	usuario.administrar	Administrar usuarios
36c87905-ab3d-4a7c-9d8e-2d9b821da0d9	entidad.administrar	Administrar entidades
\.


--
-- Data for Name: pregunta; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.pregunta (id, encuesta_id, seccion_id, dimension_id, tipo, titulo, descripcion, orden, peso, es_obligatoria, configuracion_json, creado_en) FROM stdin;
986fbd2f-3efc-45a5-9ee7-dd3f3b40f745	5a8e01e8-b060-4644-8df6-f78ad5655b0c	\N	\N	TEXTO	¿Cómo describirías tu experiencia general con nosotros?	Escribe con tus propias palabras, sin límite de extensión.	1	1	t	\N	2026-05-19 20:03:14.759582+00
433d7faf-eb41-4524-a190-79cf41988142	5a8e01e8-b060-4644-8df6-f78ad5655b0c	\N	\N	NUMERO	¿Cuántos años llevas trabajando en la empresa?	\N	2	1	f	\N	2026-05-19 20:03:14.784229+00
899ebd78-2631-4f0e-9865-101e74cd708e	5a8e01e8-b060-4644-8df6-f78ad5655b0c	\N	\N	SELECCION_UNICA	¿Cómo calificarías la atención recibida?	\N	3	2	t	\N	2026-05-19 20:03:14.919255+00
c08b46ca-e0ac-4825-9646-eeba674f8c21	5a8e01e8-b060-4644-8df6-f78ad5655b0c	\N	\N	SELECCION_MULTIPLE	¿Qué canales de comunicación utilizas con frecuencia?	Puedes seleccionar más de una opción.	4	1	f	\N	2026-05-19 20:03:14.957488+00
5564626a-15b8-4e74-b946-7af3f48d109b	5a8e01e8-b060-4644-8df6-f78ad5655b0c	\N	\N	ESCALA	¿Qué tan satisfecho estás con nuestro servicio?	\N	5	3	t	{"max": 10, "min": 1, "paso": 1, "etiquetaMax": "Muy satisfecho", "etiquetaMin": "Muy insatisfecho"}	2026-05-19 20:03:15.023466+00
2eb8d3e4-2884-494f-87c3-1c39d4c70a12	5a8e01e8-b060-4644-8df6-f78ad5655b0c	\N	\N	NPS	¿Qué tan probable es que nos recomiendes a un amigo o colega?	\N	6	3	t	\N	2026-05-19 20:03:15.059569+00
630ebcef-d01d-4b12-b768-a777d79c8af4	5a8e01e8-b060-4644-8df6-f78ad5655b0c	\N	\N	CALIFICACION	¿Cuántas estrellas le darías a nuestro producto?	\N	7	2	t	{"estrellas": 5}	2026-05-19 20:03:15.098942+00
3a812041-a733-401d-a1cc-474c0489600d	5a8e01e8-b060-4644-8df6-f78ad5655b0c	\N	\N	FECHA	¿Cuándo fue tu última interacción con nuestro equipo?	\N	8	1	f	\N	2026-05-19 20:03:15.20898+00
c97f9608-3eb3-4092-98ad-ec8c84462b0c	5a8e01e8-b060-4644-8df6-f78ad5655b0c	\N	\N	BOOLEANO	¿Volverías a contratar nuestros servicios?	\N	9	2	t	\N	2026-05-19 20:03:15.345727+00
293ea7f3-48df-4234-a941-474105abd128	5a8e01e8-b060-4644-8df6-f78ad5655b0c	\N	\N	RANKING	Ordena los siguientes aspectos de mayor a menor importancia para ti.	\N	10	2	f	\N	2026-05-19 20:03:15.360998+00
8a2f6a74-322f-4f2a-b039-fadca84d3759	5a8e01e8-b060-4644-8df6-f78ad5655b0c	\N	\N	MATRIZ	Evalúa los siguientes aspectos de nuestro servicio.	\N	11	3	t	{"filas": ["Atención al cliente", "Calidad del producto", "Tiempo de respuesta", "Relación precio-calidad"]}	2026-05-19 20:03:15.389243+00
36a5e4e1-ad9d-4a75-b70b-e18ec98f3f01	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	a7c75b9a-ffe3-43ef-b66e-cca04814494f	\N	SELECCION_UNICA	¿Cuál fue el motivo de tu consulta?	\N	1	1.0	t	\N	2026-05-19 21:44:23.329143+00
f7f9044f-7d27-4b3a-8d84-b73d82195fa1	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	60e08ef7-92db-4346-82a8-bc3f89c8a145	a5dfc135-f8c6-41ef-a8c4-e0a03f9a3389	CALIFICACION	¿Cómo calificarías la amabilidad y trato del empleado?	\N	2	1.0	t	{"estrellas": 5}	2026-05-19 21:44:23.352188+00
cc6d8dc6-512f-443e-b71d-637efe34f750	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	60e08ef7-92db-4346-82a8-bc3f89c8a145	900def68-dbcf-4cdb-a270-7d2776409a87	SELECCION_UNICA	¿El empleado demostró conocimiento suficiente del servicio?	\N	3	1.0	t	\N	2026-05-19 21:44:23.379134+00
c1baf43e-1b94-4bd1-af39-405883284b5e	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	60e08ef7-92db-4346-82a8-bc3f89c8a145	e008ecd0-b14b-4711-892f-33c1f415c870	SELECCION_UNICA	¿En qué tiempo fuiste atendido?	\N	4	1.0	t	\N	2026-05-19 21:44:23.406398+00
c8ffe020-28b2-4c92-9c0f-2fc410166eb3	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	7e0f5e50-9644-409e-8607-fbc00e4095ac	e7dcc572-1644-4fa1-8e97-5406db91c4d9	SELECCION_UNICA	¿Se resolvió tu consulta o problema?	\N	5	1.0	t	\N	2026-05-19 21:44:23.432428+00
0d3b1e6e-5ae2-4d70-8e98-1b6f294e96b1	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	7e0f5e50-9644-409e-8607-fbc00e4095ac	e7dcc572-1644-4fa1-8e97-5406db91c4d9	TEXTO	¿Por qué no se resolvió tu caso? Cuéntanos qué ocurrió.	\N	6	1.0	f	{"placeholder": "Describe brevemente lo sucedido..."}	2026-05-19 21:44:23.461422+00
9a0bdaee-670c-4e2a-8e10-949a118ee0a4	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	7f5b8400-0e72-4982-b529-29d1f5e81747	\N	NPS	¿Qué probabilidad hay de que recomiendes nuestros servicios a un conocido?	0 = nada probable, 10 = muy probable	7	1.0	t	\N	2026-05-19 21:44:23.493683+00
9739928a-96fd-4bb0-87fc-0f58d38a75df	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	7f5b8400-0e72-4982-b529-29d1f5e81747	\N	TEXTO	¿Qué deberíamos mejorar para brindarte una mejor experiencia?	\N	8	1.0	f	{"placeholder": "Tus sugerencias son muy valiosas para nosotros..."}	2026-05-19 21:44:23.523617+00
8c6368f6-a36d-49e3-8cf2-98fc04a2fdcf	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	7f5b8400-0e72-4982-b529-29d1f5e81747	\N	TEXTO	¿Tienes algún comentario adicional que quieras compartir?	\N	9	1.0	f	\N	2026-05-19 21:44:23.551894+00
\.


--
-- Data for Name: regla_encuesta; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.regla_encuesta (id, encuesta_id, regla_json) FROM stdin;
89fcd445-ccde-46c2-b28c-3705ddf3f2df	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	{"si": {"valor": "no_resuelto", "operador": "igual", "preguntaId": "c8ffe020-28b2-4c92-9c0f-2fc410166eb3"}, "entonces": {"accion": "mostrar", "preguntaObjetivoId": "0d3b1e6e-5ae2-4d70-8e98-1b6f294e96b1"}}
8867eb4c-2df1-42a9-816b-80d40bdab6f3	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	{"si": {"valor": "7", "operador": "menor", "preguntaId": "9a0bdaee-670c-4e2a-8e10-949a118ee0a4"}, "entonces": {"accion": "mostrar", "preguntaObjetivoId": "9739928a-96fd-4bb0-87fc-0f58d38a75df"}}
\.


--
-- Data for Name: respuesta; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.respuesta (id, encuesta_id, version_encuesta, invitacion_id, usuario_respondent_id, canal, ultima_pregunta_id, peso_estadistico, consentimiento_otorgado, fecha_consentimiento, iniciado_en, completado_en, info_dispositivo, direccion_ip) FROM stdin;
bf940440-1567-4285-ae91-8a860c9a5c91	5a8e01e8-b060-4644-8df6-f78ad5655b0c	1	97fe35a4-518b-4536-980f-77ed9852543a	\N	ENLACE_PUBLICO	\N	1.0	\N	\N	\N	2026-05-19 20:05:58.244807+00	\N	\N
4b02e23f-6cb8-4f2f-a2b8-bd3f23d97beb	5a8e01e8-b060-4644-8df6-f78ad5655b0c	1	a499b067-1fae-46c3-8ddd-21d04d805551	\N	ENLACE_PUBLICO	\N	1.0	\N	\N	\N	2026-05-19 20:06:44.990098+00	\N	\N
bf8aec44-79bf-4521-9a97-1e86c39af3c3	5a8e01e8-b060-4644-8df6-f78ad5655b0c	1	b8d339c7-27b2-4f8c-947e-c163eb5f9299	\N	ENLACE_PUBLICO	\N	1.0	\N	\N	\N	2026-05-19 20:35:02.619784+00	\N	\N
412bd626-c83d-400c-8279-8ca067ff2721	5a8e01e8-b060-4644-8df6-f78ad5655b0c	1	2e5470a3-0329-4bf3-9a73-eaaa5b15b32e	\N	ENLACE_PUBLICO	\N	1.0	\N	\N	\N	2026-05-19 21:32:27.263173+00	\N	\N
4b7323c5-5975-4daa-aedd-a855a1ef7005	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	1	8424a6c8-feff-4d45-b971-e8a4aff5e8e6	\N	ENLACE_PUBLICO	\N	1.0	\N	\N	\N	2026-05-19 21:47:27.212865+00	\N	\N
07ae864d-3c8d-45a9-89f5-edc3228ad647	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	1	2a042259-8f3f-4510-ad31-fc2d823d67f5	\N	ENLACE_PUBLICO	\N	1.0	\N	\N	\N	2026-05-19 21:58:01.975291+00	\N	\N
7a6f431c-af42-4573-95b0-4c9c79b485c9	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	1	18de3340-88f1-4511-8564-4282d928c442	\N	ENLACE_PUBLICO	\N	1.0	\N	\N	\N	2026-05-20 00:59:00.722922+00	\N	\N
647bca39-0a27-49d4-8f03-816803c79c19	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	1	07c41f02-312d-425f-aec2-003f8080c27a	\N	ENLACE_PUBLICO	\N	1.0	\N	\N	\N	2026-05-20 01:12:20.020037+00	\N	\N
\.


--
-- Data for Name: rol; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.rol (id, organizacion_id, nombre) FROM stdin;
\.


--
-- Data for Name: rol_permiso; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.rol_permiso (rol_id, permiso_id) FROM stdin;
\.


--
-- Data for Name: seccion_encuesta; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.seccion_encuesta (id, encuesta_id, titulo, descripcion, orden) FROM stdin;
a7c75b9a-ffe3-43ef-b66e-cca04814494f	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	Identificación del Servicio	Cuéntanos sobre la atención que recibiste	1
60e08ef7-92db-4346-82a8-bc3f89c8a145	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	Calidad de la Atención	Evalúa al empleado que te atendió	2
7e0f5e50-9644-409e-8607-fbc00e4095ac	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	Resolución de tu Caso	Cuéntanos cómo quedó tu solicitud	3
7f5b8400-0e72-4982-b529-29d1f5e81747	6b2e3c79-5ba8-4ab3-9703-a204f8bb901f	Experiencia General	Tu opinión global nos ayuda a mejorar	4
\.


--
-- Data for Name: tipo_entidad; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.tipo_entidad (id, codigo, nombre) FROM stdin;
bac8b4fe-8169-4cb5-a237-7cde24126a2a	EMPRESA	Empresa
4455c604-d852-47d9-8911-146e996dc57e	REGIONAL	Regional
26082f63-3687-4ecd-a536-a3fb2b7d3713	UNIDAD	Unidad
4d014459-51a5-42ae-90df-c70f3c517dd5	EMPLEADO	Empleado
282e40db-118c-4ffe-9ac3-4724e6ac3f76	CLIENTE	Cliente
be32293e-6f80-4725-b43b-03cd098bd4a8	PROVEEDOR	Proveedor
ad29d907-bf26-4bb4-b810-49f58902a898	PRODUCTO	Producto
d5d2adaf-ef33-4cba-a993-2947d323c0ef	SERVICIO	Servicio
74471432-992c-4117-82d1-e99735188110	APLICACION	Aplicación
b28106cb-7988-4a2e-a119-5d118c3aadf3	PROYECTO	Proyecto
d9cb4317-a184-499d-be99-8db9e4216722	EVENTO	Evento
\.


--
-- Data for Name: usuario_rol; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.usuario_rol (usuario_id, rol_id) FROM stdin;
\.


--
-- Name: adjunto adjunto_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.adjunto
    ADD CONSTRAINT adjunto_pkey PRIMARY KEY (id);


--
-- Name: alcance_encuesta alcance_encuesta_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.alcance_encuesta
    ADD CONSTRAINT alcance_encuesta_pkey PRIMARY KEY (id);


--
-- Name: colaborador_encuesta colaborador_encuesta_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.colaborador_encuesta
    ADD CONSTRAINT colaborador_encuesta_pkey PRIMARY KEY (id);


--
-- Name: comentario comentario_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.comentario
    ADD CONSTRAINT comentario_pkey PRIMARY KEY (id);


--
-- Name: cuenta_usuario cuenta_usuario_correo_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cuenta_usuario
    ADD CONSTRAINT cuenta_usuario_correo_key UNIQUE (correo);


--
-- Name: cuenta_usuario cuenta_usuario_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cuenta_usuario
    ADD CONSTRAINT cuenta_usuario_pkey PRIMARY KEY (id);


--
-- Name: cuota_respuesta cuota_respuesta_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cuota_respuesta
    ADD CONSTRAINT cuota_respuesta_pkey PRIMARY KEY (id);


--
-- Name: detalle_respuesta detalle_respuesta_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.detalle_respuesta
    ADD CONSTRAINT detalle_respuesta_pkey PRIMARY KEY (id);


--
-- Name: dimension_pregunta dimension_pregunta_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.dimension_pregunta
    ADD CONSTRAINT dimension_pregunta_pkey PRIMARY KEY (id);


--
-- Name: encuesta encuesta_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.encuesta
    ADD CONSTRAINT encuesta_pkey PRIMARY KEY (id);


--
-- Name: entidad entidad_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.entidad
    ADD CONSTRAINT entidad_pkey PRIMARY KEY (id);


--
-- Name: invitacion invitacion_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.invitacion
    ADD CONSTRAINT invitacion_pkey PRIMARY KEY (id);


--
-- Name: notificacion_envio notificacion_envio_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.notificacion_envio
    ADD CONSTRAINT notificacion_envio_pkey PRIMARY KEY (id);


--
-- Name: objetivo_respuesta objetivo_respuesta_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.objetivo_respuesta
    ADD CONSTRAINT objetivo_respuesta_pkey PRIMARY KEY (id);


--
-- Name: opcion_pregunta opcion_pregunta_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.opcion_pregunta
    ADD CONSTRAINT opcion_pregunta_pkey PRIMARY KEY (id);


--
-- Name: organizacion organizacion_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.organizacion
    ADD CONSTRAINT organizacion_pkey PRIMARY KEY (id);


--
-- Name: permiso permiso_codigo_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.permiso
    ADD CONSTRAINT permiso_codigo_key UNIQUE (codigo);


--
-- Name: permiso permiso_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.permiso
    ADD CONSTRAINT permiso_pkey PRIMARY KEY (id);


--
-- Name: pregunta pregunta_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pregunta
    ADD CONSTRAINT pregunta_pkey PRIMARY KEY (id);


--
-- Name: regla_encuesta regla_encuesta_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.regla_encuesta
    ADD CONSTRAINT regla_encuesta_pkey PRIMARY KEY (id);


--
-- Name: respuesta respuesta_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.respuesta
    ADD CONSTRAINT respuesta_pkey PRIMARY KEY (id);


--
-- Name: rol_permiso rol_permiso_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rol_permiso
    ADD CONSTRAINT rol_permiso_pkey PRIMARY KEY (rol_id, permiso_id);


--
-- Name: rol rol_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rol
    ADD CONSTRAINT rol_pkey PRIMARY KEY (id);


--
-- Name: seccion_encuesta seccion_encuesta_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.seccion_encuesta
    ADD CONSTRAINT seccion_encuesta_pkey PRIMARY KEY (id);


--
-- Name: tipo_entidad tipo_entidad_codigo_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tipo_entidad
    ADD CONSTRAINT tipo_entidad_codigo_key UNIQUE (codigo);


--
-- Name: tipo_entidad tipo_entidad_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tipo_entidad
    ADD CONSTRAINT tipo_entidad_pkey PRIMARY KEY (id);


--
-- Name: colaborador_encuesta uq_colaborador_encuesta; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.colaborador_encuesta
    ADD CONSTRAINT uq_colaborador_encuesta UNIQUE (encuesta_id, usuario_id);


--
-- Name: usuario_rol usuario_rol_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.usuario_rol
    ADD CONSTRAINT usuario_rol_pkey PRIMARY KEY (usuario_id, rol_id);


--
-- Name: ix_adjunto_entidad_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_adjunto_entidad_id ON public.adjunto USING btree (entidad_id);


--
-- Name: ix_alcance_encuesta_encuesta_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_alcance_encuesta_encuesta_id ON public.alcance_encuesta USING btree (encuesta_id);


--
-- Name: ix_alcance_encuesta_entidad_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_alcance_encuesta_entidad_id ON public.alcance_encuesta USING btree (entidad_id);


--
-- Name: ix_colaborador_encuesta_encuesta_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_colaborador_encuesta_encuesta_id ON public.colaborador_encuesta USING btree (encuesta_id);


--
-- Name: ix_colaborador_encuesta_usuario_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_colaborador_encuesta_usuario_id ON public.colaborador_encuesta USING btree (usuario_id);


--
-- Name: ix_comentario_entidad_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_comentario_entidad_id ON public.comentario USING btree (entidad_id);


--
-- Name: ix_comentario_respuesta_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_comentario_respuesta_id ON public.comentario USING btree (respuesta_id);


--
-- Name: ix_cuenta_usuario_entidad_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_cuenta_usuario_entidad_id ON public.cuenta_usuario USING btree (entidad_id);


--
-- Name: ix_cuenta_usuario_organizacion_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_cuenta_usuario_organizacion_id ON public.cuenta_usuario USING btree (organizacion_id);


--
-- Name: ix_cuota_respuesta_encuesta_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_cuota_respuesta_encuesta_id ON public.cuota_respuesta USING btree (encuesta_id);


--
-- Name: ix_cuota_respuesta_entidad_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_cuota_respuesta_entidad_id ON public.cuota_respuesta USING btree (entidad_id);


--
-- Name: ix_detalle_respuesta_pregunta_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_detalle_respuesta_pregunta_id ON public.detalle_respuesta USING btree (pregunta_id);


--
-- Name: ix_detalle_respuesta_respuesta_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_detalle_respuesta_respuesta_id ON public.detalle_respuesta USING btree (respuesta_id);


--
-- Name: ix_detalle_respuesta_valor_json; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_detalle_respuesta_valor_json ON public.detalle_respuesta USING gin (valor_json);


--
-- Name: ix_dimension_pregunta_encuesta_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_dimension_pregunta_encuesta_id ON public.dimension_pregunta USING btree (encuesta_id);


--
-- Name: ix_encuesta_es_plantilla; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_encuesta_es_plantilla ON public.encuesta USING btree (es_plantilla);


--
-- Name: ix_encuesta_estado; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_encuesta_estado ON public.encuesta USING btree (estado);


--
-- Name: ix_encuesta_etiquetas_json; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_encuesta_etiquetas_json ON public.encuesta USING gin (etiquetas_json);


--
-- Name: ix_encuesta_fecha_fin; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_encuesta_fecha_fin ON public.encuesta USING btree (fecha_fin);


--
-- Name: ix_encuesta_fecha_inicio; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_encuesta_fecha_inicio ON public.encuesta USING btree (fecha_inicio);


--
-- Name: ix_encuesta_organizacion_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_encuesta_organizacion_id ON public.encuesta USING btree (organizacion_id);


--
-- Name: ix_encuesta_plantilla_origen; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_encuesta_plantilla_origen ON public.encuesta USING btree (plantilla_origen_id);


--
-- Name: ix_entidad_atributos_json; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_entidad_atributos_json ON public.entidad USING gin (atributos_json);


--
-- Name: ix_entidad_entidad_padre_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_entidad_entidad_padre_id ON public.entidad USING btree (entidad_padre_id);


--
-- Name: ix_entidad_organizacion_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_entidad_organizacion_id ON public.entidad USING btree (organizacion_id);


--
-- Name: ix_entidad_tipo_entidad_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_entidad_tipo_entidad_id ON public.entidad USING btree (tipo_entidad_id);


--
-- Name: ix_invitacion_cuenta_usuario_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_invitacion_cuenta_usuario_id ON public.invitacion USING btree (cuenta_usuario_id);


--
-- Name: ix_invitacion_encuesta_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_invitacion_encuesta_id ON public.invitacion USING btree (encuesta_id);


--
-- Name: ix_invitacion_estado; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_invitacion_estado ON public.invitacion USING btree (estado);


--
-- Name: ix_invitacion_token_acceso; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX ix_invitacion_token_acceso ON public.invitacion USING btree (token_acceso);


--
-- Name: ix_notificacion_envio_estado; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_notificacion_envio_estado ON public.notificacion_envio USING btree (estado);


--
-- Name: ix_notificacion_envio_invitacion_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_notificacion_envio_invitacion_id ON public.notificacion_envio USING btree (invitacion_id);


--
-- Name: ix_notificacion_envio_tipo; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_notificacion_envio_tipo ON public.notificacion_envio USING btree (tipo);


--
-- Name: ix_objetivo_respuesta_entidad_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_objetivo_respuesta_entidad_id ON public.objetivo_respuesta USING btree (entidad_id);


--
-- Name: ix_objetivo_respuesta_respuesta_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_objetivo_respuesta_respuesta_id ON public.objetivo_respuesta USING btree (respuesta_id);


--
-- Name: ix_opcion_pregunta_pregunta_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_opcion_pregunta_pregunta_id ON public.opcion_pregunta USING btree (pregunta_id);


--
-- Name: ix_pregunta_configuracion_json; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_pregunta_configuracion_json ON public.pregunta USING gin (configuracion_json);


--
-- Name: ix_pregunta_dimension_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_pregunta_dimension_id ON public.pregunta USING btree (dimension_id);


--
-- Name: ix_pregunta_encuesta_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_pregunta_encuesta_id ON public.pregunta USING btree (encuesta_id);


--
-- Name: ix_pregunta_orden; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_pregunta_orden ON public.pregunta USING btree (orden);


--
-- Name: ix_pregunta_seccion_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_pregunta_seccion_id ON public.pregunta USING btree (seccion_id);


--
-- Name: ix_regla_encuesta_encuesta_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_regla_encuesta_encuesta_id ON public.regla_encuesta USING btree (encuesta_id);


--
-- Name: ix_regla_encuesta_regla_json; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_regla_encuesta_regla_json ON public.regla_encuesta USING gin (regla_json);


--
-- Name: ix_respuesta_canal; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_respuesta_canal ON public.respuesta USING btree (canal);


--
-- Name: ix_respuesta_completado_en; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_respuesta_completado_en ON public.respuesta USING btree (completado_en);


--
-- Name: ix_respuesta_encuesta_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_respuesta_encuesta_id ON public.respuesta USING btree (encuesta_id);


--
-- Name: ix_respuesta_invitacion_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_respuesta_invitacion_id ON public.respuesta USING btree (invitacion_id);


--
-- Name: ix_respuesta_ultima_pregunta_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_respuesta_ultima_pregunta_id ON public.respuesta USING btree (ultima_pregunta_id);


--
-- Name: ix_respuesta_usuario_respondent_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_respuesta_usuario_respondent_id ON public.respuesta USING btree (usuario_respondent_id);


--
-- Name: ix_seccion_encuesta_encuesta_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_seccion_encuesta_encuesta_id ON public.seccion_encuesta USING btree (encuesta_id);


--
-- Name: uq_entidad_id_externo; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX uq_entidad_id_externo ON public.entidad USING btree (organizacion_id, id_externo) WHERE (id_externo IS NOT NULL);


--
-- Name: adjunto adjunto_entidad_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.adjunto
    ADD CONSTRAINT adjunto_entidad_id_fkey FOREIGN KEY (entidad_id) REFERENCES public.entidad(id);


--
-- Name: alcance_encuesta alcance_encuesta_encuesta_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.alcance_encuesta
    ADD CONSTRAINT alcance_encuesta_encuesta_id_fkey FOREIGN KEY (encuesta_id) REFERENCES public.encuesta(id);


--
-- Name: alcance_encuesta alcance_encuesta_entidad_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.alcance_encuesta
    ADD CONSTRAINT alcance_encuesta_entidad_id_fkey FOREIGN KEY (entidad_id) REFERENCES public.entidad(id);


--
-- Name: colaborador_encuesta colaborador_encuesta_encuesta_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.colaborador_encuesta
    ADD CONSTRAINT colaborador_encuesta_encuesta_id_fkey FOREIGN KEY (encuesta_id) REFERENCES public.encuesta(id);


--
-- Name: colaborador_encuesta colaborador_encuesta_usuario_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.colaborador_encuesta
    ADD CONSTRAINT colaborador_encuesta_usuario_id_fkey FOREIGN KEY (usuario_id) REFERENCES public.cuenta_usuario(id);


--
-- Name: comentario comentario_entidad_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.comentario
    ADD CONSTRAINT comentario_entidad_id_fkey FOREIGN KEY (entidad_id) REFERENCES public.entidad(id);


--
-- Name: comentario comentario_respuesta_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.comentario
    ADD CONSTRAINT comentario_respuesta_id_fkey FOREIGN KEY (respuesta_id) REFERENCES public.respuesta(id);


--
-- Name: comentario comentario_usuario_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.comentario
    ADD CONSTRAINT comentario_usuario_id_fkey FOREIGN KEY (usuario_id) REFERENCES public.cuenta_usuario(id);


--
-- Name: cuenta_usuario cuenta_usuario_entidad_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cuenta_usuario
    ADD CONSTRAINT cuenta_usuario_entidad_id_fkey FOREIGN KEY (entidad_id) REFERENCES public.entidad(id);


--
-- Name: cuenta_usuario cuenta_usuario_organizacion_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cuenta_usuario
    ADD CONSTRAINT cuenta_usuario_organizacion_id_fkey FOREIGN KEY (organizacion_id) REFERENCES public.organizacion(id);


--
-- Name: cuota_respuesta cuota_respuesta_encuesta_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cuota_respuesta
    ADD CONSTRAINT cuota_respuesta_encuesta_id_fkey FOREIGN KEY (encuesta_id) REFERENCES public.encuesta(id);


--
-- Name: cuota_respuesta cuota_respuesta_entidad_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cuota_respuesta
    ADD CONSTRAINT cuota_respuesta_entidad_id_fkey FOREIGN KEY (entidad_id) REFERENCES public.entidad(id);


--
-- Name: detalle_respuesta detalle_respuesta_pregunta_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.detalle_respuesta
    ADD CONSTRAINT detalle_respuesta_pregunta_id_fkey FOREIGN KEY (pregunta_id) REFERENCES public.pregunta(id);


--
-- Name: detalle_respuesta detalle_respuesta_respuesta_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.detalle_respuesta
    ADD CONSTRAINT detalle_respuesta_respuesta_id_fkey FOREIGN KEY (respuesta_id) REFERENCES public.respuesta(id);


--
-- Name: dimension_pregunta dimension_pregunta_encuesta_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.dimension_pregunta
    ADD CONSTRAINT dimension_pregunta_encuesta_id_fkey FOREIGN KEY (encuesta_id) REFERENCES public.encuesta(id);


--
-- Name: encuesta encuesta_creado_por_usuario_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.encuesta
    ADD CONSTRAINT encuesta_creado_por_usuario_id_fkey FOREIGN KEY (creado_por_usuario_id) REFERENCES public.cuenta_usuario(id);


--
-- Name: encuesta encuesta_organizacion_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.encuesta
    ADD CONSTRAINT encuesta_organizacion_id_fkey FOREIGN KEY (organizacion_id) REFERENCES public.organizacion(id);


--
-- Name: encuesta encuesta_plantilla_origen_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.encuesta
    ADD CONSTRAINT encuesta_plantilla_origen_id_fkey FOREIGN KEY (plantilla_origen_id) REFERENCES public.encuesta(id);


--
-- Name: entidad entidad_entidad_padre_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.entidad
    ADD CONSTRAINT entidad_entidad_padre_id_fkey FOREIGN KEY (entidad_padre_id) REFERENCES public.entidad(id);


--
-- Name: entidad entidad_organizacion_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.entidad
    ADD CONSTRAINT entidad_organizacion_id_fkey FOREIGN KEY (organizacion_id) REFERENCES public.organizacion(id);


--
-- Name: entidad entidad_tipo_entidad_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.entidad
    ADD CONSTRAINT entidad_tipo_entidad_id_fkey FOREIGN KEY (tipo_entidad_id) REFERENCES public.tipo_entidad(id);


--
-- Name: invitacion invitacion_cuenta_usuario_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.invitacion
    ADD CONSTRAINT invitacion_cuenta_usuario_id_fkey FOREIGN KEY (cuenta_usuario_id) REFERENCES public.cuenta_usuario(id);


--
-- Name: invitacion invitacion_encuesta_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.invitacion
    ADD CONSTRAINT invitacion_encuesta_id_fkey FOREIGN KEY (encuesta_id) REFERENCES public.encuesta(id);


--
-- Name: invitacion invitacion_entidad_evaluada_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.invitacion
    ADD CONSTRAINT invitacion_entidad_evaluada_id_fkey FOREIGN KEY (entidad_evaluada_id) REFERENCES public.entidad(id);


--
-- Name: notificacion_envio notificacion_envio_invitacion_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.notificacion_envio
    ADD CONSTRAINT notificacion_envio_invitacion_id_fkey FOREIGN KEY (invitacion_id) REFERENCES public.invitacion(id);


--
-- Name: objetivo_respuesta objetivo_respuesta_entidad_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.objetivo_respuesta
    ADD CONSTRAINT objetivo_respuesta_entidad_id_fkey FOREIGN KEY (entidad_id) REFERENCES public.entidad(id);


--
-- Name: objetivo_respuesta objetivo_respuesta_respuesta_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.objetivo_respuesta
    ADD CONSTRAINT objetivo_respuesta_respuesta_id_fkey FOREIGN KEY (respuesta_id) REFERENCES public.respuesta(id);


--
-- Name: opcion_pregunta opcion_pregunta_pregunta_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.opcion_pregunta
    ADD CONSTRAINT opcion_pregunta_pregunta_id_fkey FOREIGN KEY (pregunta_id) REFERENCES public.pregunta(id);


--
-- Name: pregunta pregunta_dimension_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pregunta
    ADD CONSTRAINT pregunta_dimension_id_fkey FOREIGN KEY (dimension_id) REFERENCES public.dimension_pregunta(id);


--
-- Name: pregunta pregunta_encuesta_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pregunta
    ADD CONSTRAINT pregunta_encuesta_id_fkey FOREIGN KEY (encuesta_id) REFERENCES public.encuesta(id);


--
-- Name: pregunta pregunta_seccion_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.pregunta
    ADD CONSTRAINT pregunta_seccion_id_fkey FOREIGN KEY (seccion_id) REFERENCES public.seccion_encuesta(id);


--
-- Name: regla_encuesta regla_encuesta_encuesta_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.regla_encuesta
    ADD CONSTRAINT regla_encuesta_encuesta_id_fkey FOREIGN KEY (encuesta_id) REFERENCES public.encuesta(id);


--
-- Name: respuesta respuesta_encuesta_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.respuesta
    ADD CONSTRAINT respuesta_encuesta_id_fkey FOREIGN KEY (encuesta_id) REFERENCES public.encuesta(id);


--
-- Name: respuesta respuesta_invitacion_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.respuesta
    ADD CONSTRAINT respuesta_invitacion_id_fkey FOREIGN KEY (invitacion_id) REFERENCES public.invitacion(id);


--
-- Name: respuesta respuesta_ultima_pregunta_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.respuesta
    ADD CONSTRAINT respuesta_ultima_pregunta_id_fkey FOREIGN KEY (ultima_pregunta_id) REFERENCES public.pregunta(id);


--
-- Name: respuesta respuesta_usuario_respondent_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.respuesta
    ADD CONSTRAINT respuesta_usuario_respondent_id_fkey FOREIGN KEY (usuario_respondent_id) REFERENCES public.cuenta_usuario(id);


--
-- Name: rol rol_organizacion_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rol
    ADD CONSTRAINT rol_organizacion_id_fkey FOREIGN KEY (organizacion_id) REFERENCES public.organizacion(id);


--
-- Name: rol_permiso rol_permiso_permiso_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rol_permiso
    ADD CONSTRAINT rol_permiso_permiso_id_fkey FOREIGN KEY (permiso_id) REFERENCES public.permiso(id);


--
-- Name: rol_permiso rol_permiso_rol_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rol_permiso
    ADD CONSTRAINT rol_permiso_rol_id_fkey FOREIGN KEY (rol_id) REFERENCES public.rol(id);


--
-- Name: seccion_encuesta seccion_encuesta_encuesta_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.seccion_encuesta
    ADD CONSTRAINT seccion_encuesta_encuesta_id_fkey FOREIGN KEY (encuesta_id) REFERENCES public.encuesta(id);


--
-- Name: usuario_rol usuario_rol_rol_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.usuario_rol
    ADD CONSTRAINT usuario_rol_rol_id_fkey FOREIGN KEY (rol_id) REFERENCES public.rol(id);


--
-- Name: usuario_rol usuario_rol_usuario_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.usuario_rol
    ADD CONSTRAINT usuario_rol_usuario_id_fkey FOREIGN KEY (usuario_id) REFERENCES public.cuenta_usuario(id);


--
-- PostgreSQL database dump complete
--

