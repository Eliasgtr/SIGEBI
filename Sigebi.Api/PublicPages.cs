namespace Sigebi.Api;

internal static class PublicPages
{
    internal const string HomeHtml =
        """
        <!DOCTYPE html>
        <html lang="es">
        <head>
          <meta charset="utf-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1" />
          <title>SIGEBI — API</title>
          <style>
            body { font-family: system-ui, sans-serif; max-width: 42rem; margin: 2rem auto; padding: 0 1rem; line-height: 1.5; }
            code { background: #f4f4f4; padding: 0.15rem 0.35rem; border-radius: 4px; }
            a { color: #0b5; }
            ul { padding-left: 1.2rem; }
          </style>
        </head>
        <body>
          <h1>SIGEBI API</h1>
          <p>El servicio está <strong>en ejecución</strong>. Esta URL es la API REST (no es la app Blazor).</p>
          <ul>
            <li><a href="api/books"><code>GET /api/books</code></a> — catálogo</li>
            <li><a href="api/users"><code>GET /api/users</code></a> — usuarios</li>
            <li><a href="api/loans/pending"><code>GET /api/loans/pending</code></a> — solicitudes pendientes</li>
            <li><a href="openapi/v1.json"><code>GET /openapi/v1.json</code></a> — OpenAPI (solo desarrollo)</li>
          </ul>
          <p>Interfaz web: <a href="https://localhost:7174/"><code>https://localhost:7174/</code></a></p>
          <p>Si el navegador avisa del certificado: <code>dotnet dev-certs https --trust</code></p>
        </body>
        </html>
        """;
}
