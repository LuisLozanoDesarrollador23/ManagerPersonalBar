# Autenticación, contraseña y tokens

## Decisión para la primera versión

La interfaz Blazor Server y la API viven en el mismo servidor. Se autentican con **ASP.NET Core Identity**, que guarda usuarios en SQL Server, y con la **cookie de sesión** de Identity. No se utiliza Auth0 ni un proveedor de identidad externo.

Identity nunca guarda la contraseña original. Al registrar un usuario, genera un hash lento y salado; durante el login vuelve a calcularlo y lo compara. Por ello una contraseña olvidada se restablece, no se recupera. Esta versión exige al menos diez caracteres y habilita el bloqueo para nuevos usuarios.

## Por qué una cookie y no JWT para Blazor Server

Tras el login, el servidor devuelve una cookie con un ticket de autenticación protegido criptográficamente. El navegador la manda automáticamente al mismo sitio y el servidor valida el ticket en cada petición. No lleva la contraseña.

Es el mecanismo apropiado para Blazor Server porque el código de la interfaz se ejecuta en el servidor y no hay que entregar un token reutilizable al JavaScript del navegador. En producción la cookie debe enviarse sólo por HTTPS y tener `HttpOnly`, `Secure` y `SameSite` revisados para el dominio final.

## Tres significados distintos de “token”

1. **Token antiforgery.** Se adjunta a formularios que usan la cookie. Evita ataques CSRF: otro sitio no puede forzar una operación autenticada. No es una sesión ni sirve para llamar a la API.
2. **Token temporal de Identity.** Se usa para confirmar correo, restablecer contraseña o segundo factor. Tiene un propósito y caducidad concretos.
3. **Access token de API.** Una futura aplicación móvil, SPA alojada en otro origen o integración externa puede enviarlo explícitamente en `Authorization: Bearer <token>`.

## JWT para una futura API externa

Un JWT tiene cabecera, payload y firma. El payload suele llevar el identificador de usuario (`sub`), roles o permisos, emisor, audiencia y vencimiento. Está firmado, pero normalmente **no cifrado**: quien lo posea puede leerlo. No debe contener contraseñas, secretos ni datos personales sensibles.

La API debe validar firma, emisor, audiencia, vencimiento y permisos. Un JWT robado permite actuar hasta que caduque, así que se recomienda un access token corto (por ejemplo 10–15 minutos), HTTPS obligatorio y no guardarlo en `localStorage` si se puede evitar. Si se necesitan sesiones prolongadas se usan refresh tokens rotatorios, guardados y revocables en el servidor. Para la arquitectura actual, la cookie del mismo origen es más sencilla y reduce esa superficie de ataque.

## Próximos endurecimientos antes de producción

- Mover cadena de conexión y claves a secretos o variables de entorno.
- Separar los roles `Administrador` y `Empleado`.
- Configurar correo y confirmación de cuenta/recuperación de contraseña.
- Añadir limitación de intentos de login, logs de auditoría y copias de seguridad de SQL Server.
