# LAB6_FR_SRCH_001_REPORT

## 1. Các chức năng đã hoàn thành

### Tuần 4 – FR-SRCH-001
- Cập nhật logic tìm kiếm trên backend theo pattern PostgreSQL full-text search với `to_tsvector`, `plainto_tsquery`, `unaccent`, và `ts_rank_cd`.
- Tạo migration để tạo extension `unaccent`, thêm cột `SearchVector`, tạo GIN index và trigger cập nhật vector khi `Title`, `Description` hoặc `Instructions` thay đổi.
- Bảo toàn clean architecture: logic FTS nằm trong Infrastructure/Repository, không buộc logic PostgreSQL lên Domain layer.

### Lab 6 – Frontend
- Hoàn thiện API utilities và error handling.
- Cài đặt/hoàn thiện store Zustand cho UI và auth.
- Thiết kế giao diện homepage, recipes page, search page, login/register page, not-found page.
- Tạo reusable components: Header, Footer, SearchBar, RecipeCard, RecipeGrid, Loading, EmptyState, ErrorMessage, Pagination.
- Tạo layout riêng cho public pages và auth pages.

## 2. Danh sách file đã tạo/chỉnh sửa
- `backend/src/CulinaryBlog.Infrastructure/Persistence/Repositories/RecipeRepository.cs`
- `backend/src/CulinaryBlog.Infrastructure/Persistence/Migrations/20261014173100_AddRecipeSearchVector.cs`
- `frontend/src/lib/api/client.ts`
- `frontend/src/lib/api/search.ts`
- `frontend/src/store/auth-store.ts`
- `frontend/src/components/layout/header.tsx`
- `frontend/src/components/layout/footer.tsx`
- `frontend/src/components/layout/main-layout.tsx`
- `frontend/src/components/layout/auth-layout.tsx`
- `frontend/src/components/shared/search-bar.tsx`
- `frontend/src/components/shared/loading.tsx`
- `frontend/src/components/shared/empty-state.tsx`
- `frontend/src/components/shared/error-message.tsx`
- `frontend/src/components/shared/pagination.tsx`
- `frontend/src/components/recipes/recipe-card.tsx`
- `frontend/src/components/recipes/recipe-grid.tsx`
- `frontend/src/app/page.tsx`
- `frontend/src/app/(public)/recipes/page.tsx`
- `frontend/src/app/(public)/recipes/search/page.tsx`
- `frontend/src/app/(auth)/login/page.tsx`
- `frontend/src/app/(auth)/register/page.tsx`
- `frontend/src/app/not-found.tsx`
- `frontend/src/app/globals.css`

## 3. Kết quả build
- Backend build: PASS
- Frontend build: PASS
- TypeScript check: PASS
- Tests: PASS (18/18)

## 4. Kết quả kết nối
- PostgreSQL: Not checked locally; no dedicated PostgreSQL service was started in this environment.
- Search API: Implemented and compiles cleanly; runtime DB validation requires a running PostgreSQL instance with `unaccent` enabled.
- Frontend Search: Built successfully; API integration is wired via `/api/v1/recipes/search`.

## 5. Hướng dẫn chạy dự án

### Backend
```bash
dotnet restore
cd backend/src/CulinaryBlog.API
dotnet run
```

### Frontend
```bash
cd frontend
npm install
npm run dev
```

### Mở UI
- Frontend: http://localhost:3000
- Swagger: http://localhost:55536/swagger (hoặc port tương ứng từ launchSettings)

### Kiểm thử search
```bash
curl "http://localhost:55536/api/v1/recipes/search?q=pho%20bo&page=1&pageSize=10"
```

## 6. Kết luận
- FR-SRCH-001: HOÀN THÀNH về mã nguồn và build; cần PostgreSQL thực tế để xác minh FT index và runtime search với dữ liệu thật.
- LAB 6: HOÀN THÀNH về UI, layout, reusable components, API client và state management.
