# Final Acceptance Report — Culinary Blog

**Verification date:** 2026-10-15  
**Scope:** Lab 6 frontend, FR-SRCH-001, PostgreSQL-backed runtime, and Git credential hygiene.

## Acceptance summary

| Area | Result | Evidence |
|---|---|---|
| PostgreSQL connection | **PASS** | Backend `/health/ready` returned HTTP 200; PostgreSQL container is running on host port 5433. |
| Search migration/schema | **PASS** | `AddRecipeSearchVector` is applied; `unaccent`, `Recipes.SearchVector` (`tsvector`) and its GIN index were verified in PostgreSQL. |
| FR-SRCH-001 | **PASS** | Live API returned expected PostgreSQL recipe results for five Vietnamese queries; injection-shaped query returned HTTP 200 with no results. |
| Categories | **PASS** | Production browser loaded 20 category cards; category detail loaded 8 recipes and pagination. |
| Lab 6 | **PASS** | UI components, shared layout, auth store integration, pagination, and loading/error/empty states are present and used. Browser also verified responsive menu behavior. |
| Frontend TypeScript/build | **PASS WITH WARNINGS** | `npm run typecheck` and `npm run build` succeeded. Build reports three existing `<img>` optimization warnings. |
| Backend build | **PASS** | API project build succeeded with zero warnings and errors. |
| API tests | **PASS** | 18 passed, 0 failed. A test constructor was updated to provide the new Redis health-check logger dependency. |
| Runtime services | **PASS** | Existing PostgreSQL, Redis, and MinIO containers are running; `/health/live` and `/health/ready` returned HTTP 200. |
| Current tracked appsettings | **PASS** | No database password remains in either tracked API `appsettings` file; local Development connection is provided by .NET User Secrets. |
| Git Security | **FAIL** | A local `origin/*` tracking ref contains a historical commit with a password-bearing appsettings change. A tracked `.env.example` also has non-placeholder-looking MinIO access fields requiring owner confirmation. |
| Commit/push | **NOT PERFORMED** | No files were staged, committed, pushed, or history-rewritten. |

## PostgreSQL and FR-SRCH-001 evidence

The existing Docker services were reused; no container was duplicated and no database, volume, or credential was reset.

- PostgreSQL: `culinary-blog-api-test-postgres`, host port `5433`.
- Redis: `culinary-blog-api-test-redis`, host port `6380`.
- MinIO: `culinary-blog-minio`, ports `9000` and `9001`.
- Both `http://127.0.0.1:5000/health/live` and `/health/ready` returned HTTP 200.
- Search migration `20261014173100_AddRecipeSearchVector` is applied.
- PostgreSQL contains the `unaccent` extension, a `tsvector` search vector column, and a GIN index on that vector.
- Search uses `websearch_to_tsquery` with `unaccent`, matches the stored vector, and ranks matches using `ts_rank`.
- API results were checked against the live service:

| Query | HTTP | Result |
|---|---:|---|
| `canh chua` | 200 | `Canh chua cá` |
| `pho bo` | 200 | `Phở bò` |
| `phở bò` | 200 | `Phở bò` |
| `bun bo hue` | 200 | `Bún bò Huế` |
| `ga nuong` | 200 | `Gà nướng` |
| `' OR 1=1 --` | 200 | Empty result; no SQL injection or server error observed |

The current API results are database-backed: backend readiness reports PostgreSQL healthy, and each query returned the expected stored recipe. Frontend search for `pho bo` rendered `Phở bò`.

## Lab 6 verification

| Requirement | Result | Evidence |
|---|---|---|
| API utilities | **PASS** | Typed category/recipe/search clients call the configured API. Live search and category pages loaded data. |
| Error handling | **PASS** | Shared error component supports retry; category error and empty states were browser-tested without displaying both states at once. |
| shadcn/ui | **PASS** | `components.json` and the Button, Input, Card, Badge, Skeleton, Alert, Dialog, Dropdown Menu, and Select components exist and are imported by app UI. |
| Auth Store | **PASS (frontend integration)** | Zustand auth store is consumed by Header/Login/Register/Profile UI. This acceptance did not authenticate a real user account. |
| Static Blog UI/pages | **PASS** | Home, recipes, search, recipe detail, categories, category detail, login, register, and profile routes were built; production routes returned HTTP 200 in the prior route sweep. |
| Shared components/layout | **PASS** | Categories and category detail render shared Header/Footer; browser confirmed both. Loading, retryable error, empty state, and recipe cards are integrated. |
| Pagination | **PASS** | Recipe, search, and category detail pages display current/total pages and use API-backed data; category detail rendered page 1 of 1 for its 8 recipes. |
| Responsive UI | **PASS** | At 390×844, the mobile navigation opened (`aria-expanded` changed from false to true) and document width did not overflow the viewport. |
| Categories | **PASS** | Production page showed 20 API categories; detail showed 8 recipes and shared layout. |

Production browser checks also confirmed:

- `/categories` rendered 20 category cards.
- `/categories/danh-muc-mon-an-1` rendered 8 recipe cards and pagination.
- `/recipes/search?q=pho%20bo` rendered `Phở bò`.
- The category list used the common Header and Footer.

## Build and tests

| Command | Result |
|---|---|
| `dotnet build src/CulinaryBlog.API/CulinaryBlog.API.csproj --no-restore` | **PASS**, 0 warnings, 0 errors |
| `dotnet test tests/CulinaryBlog.API.Tests/CulinaryBlog.API.Tests.csproj --no-restore` | **PASS**, 18 passed, 0 failed |
| `npm run typecheck` | **PASS**, `tsc --noEmit` |
| `npm run build` | **PASS**, production build completed; 3 `<img>` optimization warnings |

The first API test run exposed a compile error because `RedisHealthCheck` now requires an `ILogger`. The unit test now supplies `NullLogger<RedisHealthCheck>.Instance`; the full API test project then passed.

## Git and credential review

Current tracked appsettings files have no embedded database password. The API project has a `UserSecretsId`; its Development connection is kept in local .NET User Secrets, outside the repository. For non-Development hosting, configure `ConnectionStrings__DefaultConnection` through the deployment environment/secret manager. Do not restore the old password-bearing configuration from the backup.

Security follow-up is still required:

1. The local remote-tracking ref `origin/Lab02_2312610_NguyenTrungHiep` points to a history containing a commit that included a password-bearing appsettings change. The value is intentionally not reproduced here. If that commit was pushed, treat the credential as exposed: rotate it with the database/hosting owner and coordinate history cleanup with repository maintainers. No history rewrite or force-push was done.
2. Tracked `.env.example` contains MinIO access-key/secret-key fields that do not look like obvious placeholders. Confirm they are non-production examples; if they are real credentials, revoke/rotate them and replace the template values with blank or clearly invalid placeholders. Values are omitted from this report.
3. `frontend/tsconfig.tsbuildinfo` is tracked and modified. It is a generated build cache; consider removing it from version control in a separately reviewed change. It was not deleted or staged here.
4. `LAB6_FR_SRCH_001_REPORT.md` is an older report with contradictory findings; do not submit it in place of this updated final report.

No `.env` runtime file was added to Git, and no commit or push was made. The working tree contains multiple changed and new files, including the frontend Lab 6 work, search migration, appsettings cleanup, and this report; review `git status --short` before selecting files to submit. Nothing is staged.

## Final conclusion

- **FR-SRCH-001:** **PASS**.
- **Lab 6:** **PASS** for the requested frontend requirements; real account authentication was not exercised.
- **Backend/Frontend runtime:** **PASS**; live API and browser integration verified.
- **Git Security:** **FAIL / requires follow-up** because historical credential exposure remains possible and tracked MinIO template fields need owner confirmation.
- **Ready to submit:** **NO, not yet.** Do not submit until the historical database credential has been rotated if it reached a remote and the MinIO example values are confirmed safe. Review the generated `tsconfig.tsbuildinfo` change as well.
