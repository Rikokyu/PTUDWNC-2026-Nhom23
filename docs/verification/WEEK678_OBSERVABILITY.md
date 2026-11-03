# OpenTelemetry verification

## Playwright local credentials

Set `E2E_ADMIN_PASSWORD` to the existing isolated test Admin password in the process environment or in `frontend/.env.local` (ignored by Git). The tracked `frontend/.env.example` contains only a placeholder. Playwright loads local environment files using `@next/env`; never use a `NEXT_PUBLIC_` prefix for this credential. The Lab8 runtime test fails with a configuration message when this variable is absent; a skipped test is not a PASS. Do not change the database password to satisfy the test.

Configuration follows the official SDK registration pattern:
https://opentelemetry.io/docs/languages/dotnet/traces/getting-started-aspnetcore/

Start the development receiver alongside the existing services:

```powershell
docker compose -f docker-compose.yml -f docker-compose.observability.yml up -d otel-collector
$env:OpenTelemetry__Enabled = "true"
$env:OpenTelemetry__ExportEnabled = "true"
$env:OpenTelemetry__Endpoint = "http://localhost:4317"
dotnet run --project backend/src/CulinaryBlog.API
```

Exporter defaults to disabled, SDK defaults to enabled. Set `OpenTelemetry__Enabled=false` to disable SDK registration. Standard `OTEL_EXPORTER_OTLP_*` variables remain available; the explicit endpoint above overrides the SDK endpoint. Export uses bounded batches, a 2-second timeout, and does not throw into business requests.

Call recipe search, create a test draft, upload an image, add steps, publish and archive. Then inspect:

```powershell
docker compose -f docker-compose.yml -f docker-compose.observability.yml logs --since 5m otel-collector
```

Expect server spans, MediatR operation child spans, Npgsql database spans, HttpClient spans and MinIO operation spans. JSON request logs contain CorrelationId, TraceId and SpanId. Database tracing uses Npgsql's native ActivitySource, avoiding duplicate EF instrumentation. No request object, credentials or free-text search keyword is added to custom spans/metrics.

Business metrics: `culinary.recipe.searches`, `culinary.recipe.searches.empty`, `culinary.recipe.changes` (create or actual committed status change), `culinary.image.uploads` (success/failure), `culinary.operation.duration` (seconds). HTTP metrics use ASP.NET Core built-in instrumentation; error responses appear through status-code dimensions. Publication/archive retries that already reached the target state do not increment status metrics. Creation is counted per successful command; creation has no cross-request idempotency key.

`TelemetryExportTests` sends actual OTLP HTTP protobuf to an in-process receiver and checks a W3C inbound trace plus API success while the exporter endpoint is unreachable. This verifies the transport and application behavior, not the Docker collector. Collector acceptance remains unverified until its runtime logs show the received spans and metrics.

User directory policy: the SRS does not define a user administration module. `/api/v1/users` is read-only: Admin receives names, avatars, roles and email; Author receives only Author profiles with null email. No account administration or status field was invented. Team review should confirm this limited Lab 8 extension.
