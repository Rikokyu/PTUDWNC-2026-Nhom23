-- ============================================================================
-- CULINARY BLOG - Lược đồ cơ sở dữ liệu PostgreSQL 16 + Dữ liệu mẫu phục vụ phát triển
-- Công nghệ áp dụng: PostgreSQL 16 + .NET 10 / EF Core 10
--
-- CÁC QUY ƯỚC QUAN TRỌNG CỦA DỰ ÁN ĐƯỢC ÁP DỤNG
-- 1) Recipes và Categories sử dụng xóa mềm thông qua cột "IsDeleted".
-- 2) Việc sắp xếp thuộc tầng API/truy vấn: sortBy=<tên trường>&sortOrder=asc|desc.
-- 3) RecipeNutrition được lưu trực tiếp trong bảng "Recipes" bằng các cột Nutrition_*.
-- 4) RecipeStep."StepNumber" được backend/database tự gán; phía client không
--    cần gửi giá trị này.
-- 5) RecipeIngredient."Quantity" là số thập phân và "Unit" là chuỗi.
--
-- Script này phù hợp để khởi tạo cơ sở dữ liệu trong môi trường PHÁT TRIỂN CỤC BỘ.
-- Trong ứng dụng thực tế, việc thay đổi lược đồ cơ sở dữ liệu vẫn nên được quản lý bằng
-- EF Core Code-First Migrations theo đúng yêu cầu của SRS.
-- ============================================================================

BEGIN;

-- ----------------------------------------------------------------------------
-- 0. CÁC EXTENSION CẦN THIẾT
-- ----------------------------------------------------------------------------
CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE EXTENSION IF NOT EXISTS unaccent;
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- ----------------------------------------------------------------------------
-- 1. XÓA CÁC ĐỐI TƯỢNG CŨ (DÙNG KHI RESET MÔI TRƯỜNG DEV)
-- ----------------------------------------------------------------------------
DROP TABLE IF EXISTS "RefreshTokens" CASCADE;
DROP TABLE IF EXISTS "AspNetUserTokens" CASCADE;
DROP TABLE IF EXISTS "AspNetUserLogins" CASCADE;
DROP TABLE IF EXISTS "AspNetUserClaims" CASCADE;
DROP TABLE IF EXISTS "AspNetRoleClaims" CASCADE;
DROP TABLE IF EXISTS "AspNetUserRoles" CASCADE;
DROP TABLE IF EXISTS "AspNetRoles" CASCADE;
DROP TABLE IF EXISTS "RecipeImages" CASCADE;
DROP TABLE IF EXISTS "RecipeIngredients" CASCADE;
DROP TABLE IF EXISTS "RecipeSteps" CASCADE;
DROP TABLE IF EXISTS "Recipes" CASCADE;
DROP TABLE IF EXISTS "Categories" CASCADE;
DROP TABLE IF EXISTS "AspNetUsers" CASCADE;

DROP FUNCTION IF EXISTS culinary_set_audit_fields() CASCADE;
DROP FUNCTION IF EXISTS culinary_refresh_row_version() CASCADE;
DROP FUNCTION IF EXISTS culinary_update_recipe_search_vector() CASCADE;
DROP FUNCTION IF EXISTS culinary_assign_step_number() CASCADE;
DROP FUNCTION IF EXISTS culinary_renumber_recipe_steps() CASCADE;
DROP FUNCTION IF EXISTS culinary_enforce_single_primary_image() CASCADE;

-- ----------------------------------------------------------------------------
-- 2. ASP.NET CORE IDENTITY - NGƯỜI DÙNG
--    ApplicationUser kế thừa IdentityUser<string>.
-- ----------------------------------------------------------------------------
CREATE TABLE "AspNetUsers" (
    "Id"                    varchar(450) PRIMARY KEY,
    "UserName"              varchar(256),
    "NormalizedUserName"    varchar(256),
    "Email"                 varchar(256),
    "NormalizedEmail"       varchar(256),
    "EmailConfirmed"        boolean NOT NULL DEFAULT false,
    "PasswordHash"          text,
    "SecurityStamp"         text,
    "ConcurrencyStamp"      text,
    "PhoneNumber"           text,
    "PhoneNumberConfirmed"  boolean NOT NULL DEFAULT false,
    "TwoFactorEnabled"      boolean NOT NULL DEFAULT false,
    "LockoutEnd"            timestamptz,
    "LockoutEnabled"        boolean NOT NULL DEFAULT true,
    "AccessFailedCount"     integer NOT NULL DEFAULT 0,

    -- ApplicationUser custom columns from SRS Chapter 7.7
    "DisplayName"            varchar(100) NOT NULL,
    "AvatarUrl"              varchar(500),
    "Bio"                    text,
    "IsActive"               boolean NOT NULL DEFAULT true,
    "CreatedAt"              timestamptz NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX "UserNameIndex"
    ON "AspNetUsers" ("NormalizedUserName")
    WHERE "NormalizedUserName" IS NOT NULL;

CREATE INDEX "EmailIndex"
    ON "AspNetUsers" ("NormalizedEmail");

-- ----------------------------------------------------------------------------
-- 3. ASP.NET CORE IDENTITY - VAI TRÒ / CÁC BẢNG QUAN HỆ
-- ----------------------------------------------------------------------------
CREATE TABLE "AspNetRoles" (
    "Id"                varchar(450) PRIMARY KEY,
    "Name"              varchar(256),
    "NormalizedName"    varchar(256),
    "ConcurrencyStamp"  text
);

CREATE UNIQUE INDEX "RoleNameIndex"
    ON "AspNetRoles" ("NormalizedName")
    WHERE "NormalizedName" IS NOT NULL;

CREATE TABLE "AspNetUserRoles" (
    "UserId" varchar(450) NOT NULL,
    "RoleId" varchar(450) NOT NULL,
    PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_Users"
        FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_Roles"
        FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AspNetUserRoles_RoleId"
    ON "AspNetUserRoles" ("RoleId");

CREATE TABLE "AspNetUserClaims" (
    "Id"        integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "UserId"    varchar(450) NOT NULL,
    "ClaimType" text,
    "ClaimValue" text,
    CONSTRAINT "FK_AspNetUserClaims_Users"
        FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AspNetUserClaims_UserId"
    ON "AspNetUserClaims" ("UserId");

CREATE TABLE "AspNetRoleClaims" (
    "Id"        integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "RoleId"    varchar(450) NOT NULL,
    "ClaimType" text,
    "ClaimValue" text,
    CONSTRAINT "FK_AspNetRoleClaims_Roles"
        FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AspNetRoleClaims_RoleId"
    ON "AspNetRoleClaims" ("RoleId");

CREATE TABLE "AspNetUserLogins" (
    "LoginProvider"       varchar(128) NOT NULL,
    "ProviderKey"         varchar(128) NOT NULL,
    "ProviderDisplayName" text,
    "UserId"              varchar(450) NOT NULL,
    PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_Users"
        FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AspNetUserLogins_UserId"
    ON "AspNetUserLogins" ("UserId");

CREATE TABLE "AspNetUserTokens" (
    "UserId"        varchar(450) NOT NULL,
    "LoginProvider" varchar(128) NOT NULL,
    "Name"          varchar(128) NOT NULL,
    "Value"         text,
    PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_Users"
        FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

-- ----------------------------------------------------------------------------
-- 4. DANH MỤC
--    Sử dụng xóa mềm theo quy ước đã thống nhất.
-- ----------------------------------------------------------------------------
CREATE TABLE "Categories" (
    "Id"          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name"        varchar(100) NOT NULL,
    "Slug"        varchar(120) NOT NULL,
    "Description" text,
    "ImageUrl"    varchar(500),
    "OrderIndex"  integer NOT NULL DEFAULT 0 CHECK ("OrderIndex" >= 0),

    -- Các trường kế thừa từ BaseEntity
    "CreatedAt"   timestamptz NOT NULL DEFAULT NOW(),
    "UpdatedAt"   timestamptz,
    "IsDeleted"   boolean NOT NULL DEFAULT false,
    "RowVersion"  bytea NOT NULL DEFAULT gen_random_bytes(8)
);

-- Chỉ kiểm tra duy nhất trên các bản ghi chưa xóa mềm để có thể tái sử dụng slug/tên sau khi xóa.
CREATE UNIQUE INDEX "UX_Categories_Name_Active"
    ON "Categories" ("Name")
    WHERE "IsDeleted" = false;

CREATE UNIQUE INDEX "UX_Categories_Slug_Active"
    ON "Categories" ("Slug")
    WHERE "IsDeleted" = false;

CREATE INDEX "IX_Categories_OrderIndex"
    ON "Categories" ("OrderIndex")
    WHERE "IsDeleted" = false;

-- ----------------------------------------------------------------------------
-- 5. CÔNG THỨC
--    RecipeNutrition là owned entity và được lưu bằng các cột Nutrition_* trong bảng Recipes.
-- ----------------------------------------------------------------------------
CREATE TABLE "Recipes" (
    "Id"          uuid PRIMARY KEY DEFAULT gen_random_uuid(),

    "Title"       varchar(200) NOT NULL,
    "Slug"        varchar(220) NOT NULL,
    "Description" text NOT NULL CHECK (char_length("Description") <= 2000),
    "Instructions" text NOT NULL DEFAULT '',

    "PrepTime"    integer NOT NULL CHECK ("PrepTime" > 0),
    "CookTime"    integer NOT NULL CHECK ("CookTime" >= 0),
    "Servings"    integer NOT NULL CHECK ("Servings" > 0),

    -- 1=Easy, 2=Medium, 3=Hard, 4=Expert
    "Difficulty"  smallint NOT NULL DEFAULT 1
                  CHECK ("Difficulty" BETWEEN 1 AND 4),

    -- 0=Draft, 1=Published, 2=Archived
    "Status"      smallint NOT NULL DEFAULT 0
                  CHECK ("Status" BETWEEN 0 AND 2),

    "CategoryId"  uuid NOT NULL,
    "AuthorId"    varchar(450) NOT NULL,

    "SearchVector" tsvector,
    "PublishedAt" timestamptz,

    -- RecipeNutrition - owned entity
    "Nutrition_Calories"      numeric(8,2) CHECK ("Nutrition_Calories" >= 0),
    "Nutrition_Protein"       numeric(8,2) CHECK ("Nutrition_Protein" >= 0),
    "Nutrition_Carbohydrates" numeric(8,2) CHECK ("Nutrition_Carbohydrates" >= 0),
    "Nutrition_Fat"           numeric(8,2) CHECK ("Nutrition_Fat" >= 0),
    "Nutrition_Fiber"         numeric(8,2) CHECK ("Nutrition_Fiber" >= 0),
    "Nutrition_Sodium"        numeric(8,2) CHECK ("Nutrition_Sodium" >= 0),

    -- Các trường kế thừa từ BaseEntity
    "CreatedAt"   timestamptz NOT NULL DEFAULT NOW(),
    "UpdatedAt"   timestamptz,
    "IsDeleted"   boolean NOT NULL DEFAULT false,
    "RowVersion"  bytea NOT NULL DEFAULT gen_random_bytes(8),

    CONSTRAINT "FK_Recipes_Categories"
        FOREIGN KEY ("CategoryId")
        REFERENCES "Categories"("Id")
        ON DELETE RESTRICT,

    CONSTRAINT "FK_Recipes_Authors"
        FOREIGN KEY ("AuthorId")
        REFERENCES "AspNetUsers"("Id")
        ON DELETE RESTRICT,

    CONSTRAINT "CK_Recipes_PublishedAt"
        CHECK (
            ("Status" = 1 AND "PublishedAt" IS NOT NULL)
            OR
            ("Status" <> 1)
        )
);

-- Slug của các công thức chưa bị xóa mềm phải là duy nhất.
CREATE UNIQUE INDEX "UX_Recipes_Slug_Active"
    ON "Recipes" ("Slug")
    WHERE "IsDeleted" = false;

CREATE INDEX "IX_Recipes_CategoryId"
    ON "Recipes" ("CategoryId")
    WHERE "IsDeleted" = false;

CREATE INDEX "IX_Recipes_AuthorId"
    ON "Recipes" ("AuthorId")
    WHERE "IsDeleted" = false;

CREATE INDEX "IX_Recipes_Difficulty"
    ON "Recipes" ("Difficulty")
    WHERE "IsDeleted" = false;

CREATE INDEX "IX_Recipes_Status"
    ON "Recipes" ("Status")
    WHERE "IsDeleted" = false;

CREATE INDEX "IX_Recipes_PublishedAt"
    ON "Recipes" ("PublishedAt" DESC)
    WHERE "IsDeleted" = false AND "Status" = 1;

CREATE INDEX "IX_Recipes_CreatedAt"
    ON "Recipes" ("CreatedAt" DESC)
    WHERE "IsDeleted" = false;

CREATE INDEX "IX_Recipes_Title_Trgm"
    ON "Recipes" USING GIN ("Title" gin_trgm_ops)
    WHERE "IsDeleted" = false;

CREATE INDEX "IX_Recipes_SearchVector"
    ON "Recipes" USING GIN ("SearchVector")
    WHERE "IsDeleted" = false;

-- ----------------------------------------------------------------------------
-- 6. CÁC BƯỚC CHẾ BIẾN
--    StepNumber được tự động gán. Phía client không nên tự điều khiển giá trị này.
-- ----------------------------------------------------------------------------
CREATE TABLE "RecipeSteps" (
    "Id"           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "RecipeId"     uuid NOT NULL,
    "StepNumber"   integer NOT NULL CHECK ("StepNumber" > 0),
    "Title"        varchar(200) NOT NULL,
    "Description"  text NOT NULL CHECK (char_length("Description") <= 2000),
    "TimerMinutes" integer CHECK ("TimerMinutes" >= 0),
    "ImageUrl"     varchar(500),

    -- Các trường kế thừa từ BaseEntity
    "CreatedAt"    timestamptz NOT NULL DEFAULT NOW(),
    "UpdatedAt"    timestamptz,
    "IsDeleted"    boolean NOT NULL DEFAULT false,
    "RowVersion"   bytea NOT NULL DEFAULT gen_random_bytes(8),

    CONSTRAINT "FK_RecipeSteps_Recipes"
        FOREIGN KEY ("RecipeId")
        REFERENCES "Recipes"("Id")
        ON DELETE CASCADE
);

CREATE UNIQUE INDEX "UX_RecipeSteps_Recipe_Step_Active"
    ON "RecipeSteps" ("RecipeId", "StepNumber")
    WHERE "IsDeleted" = false;

CREATE INDEX "IX_RecipeSteps_RecipeId"
    ON "RecipeSteps" ("RecipeId")
    WHERE "IsDeleted" = false;

-- ----------------------------------------------------------------------------
-- 7. NGUYÊN LIỆU CỦA CÔNG THỨC
--    Quy ước dự án: Quantity = số thập phân, Unit = chuỗi.
-- ----------------------------------------------------------------------------
CREATE TABLE "RecipeIngredients" (
    "Id"         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "RecipeId"   uuid NOT NULL,
    "Name"       varchar(200) NOT NULL,
    "Quantity"   numeric(10,3) NOT NULL CHECK ("Quantity" > 0),
    "Unit"       varchar(50) NOT NULL CHECK (btrim("Unit") <> ''),
    "Notes"      varchar(500),
    "OrderIndex" integer NOT NULL DEFAULT 0 CHECK ("OrderIndex" >= 0),

    -- Các trường kế thừa từ BaseEntity
    "CreatedAt"  timestamptz NOT NULL DEFAULT NOW(),
    "UpdatedAt"  timestamptz,
    "IsDeleted"  boolean NOT NULL DEFAULT false,
    "RowVersion" bytea NOT NULL DEFAULT gen_random_bytes(8),

    CONSTRAINT "FK_RecipeIngredients_Recipes"
        FOREIGN KEY ("RecipeId")
        REFERENCES "Recipes"("Id")
        ON DELETE CASCADE
);

CREATE INDEX "IX_RecipeIngredients_RecipeId_OrderIndex"
    ON "RecipeIngredients" ("RecipeId", "OrderIndex")
    WHERE "IsDeleted" = false;

-- ----------------------------------------------------------------------------
-- 8. ẢNH CỦA CÔNG THỨC
-- ----------------------------------------------------------------------------
CREATE TABLE "RecipeImages" (
    "Id"           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "RecipeId"     uuid NOT NULL,
    "OriginalUrl"  varchar(500) NOT NULL,
    "MediumUrl"    varchar(500),
    "ThumbnailUrl" varchar(500),
    "AltText"      varchar(200),
    "IsPrimary"    boolean NOT NULL DEFAULT false,
    "OrderIndex"   integer NOT NULL DEFAULT 0 CHECK ("OrderIndex" >= 0),

    -- Các trường kế thừa từ BaseEntity
    "CreatedAt"    timestamptz NOT NULL DEFAULT NOW(),
    "UpdatedAt"    timestamptz,
    "IsDeleted"    boolean NOT NULL DEFAULT false,
    "RowVersion"   bytea NOT NULL DEFAULT gen_random_bytes(8),

    CONSTRAINT "FK_RecipeImages_Recipes"
        FOREIGN KEY ("RecipeId")
        REFERENCES "Recipes"("Id")
        ON DELETE CASCADE
);

CREATE INDEX "IX_RecipeImages_RecipeId_OrderIndex"
    ON "RecipeImages" ("RecipeId", "OrderIndex")
    WHERE "IsDeleted" = false;

-- Mỗi công thức chỉ được có tối đa một ảnh chính đang hoạt động.
CREATE UNIQUE INDEX "UX_RecipeImages_OnePrimaryPerRecipe"
    ON "RecipeImages" ("RecipeId")
    WHERE "IsDeleted" = false AND "IsPrimary" = true;

-- ----------------------------------------------------------------------------
-- 9. REFRESH TOKEN
-- ----------------------------------------------------------------------------
CREATE TABLE "RefreshTokens" (
    "Id"                  uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId"              varchar(450) NOT NULL,
    "TokenHash"           varchar(64) NOT NULL,
    "ExpiresAt"           timestamptz NOT NULL,
    "RevokedAt"           timestamptz,
    "ReplacedByTokenHash" varchar(64),
    "CreatedAt"           timestamptz NOT NULL DEFAULT NOW(),
    "CreatedByIp"         varchar(45),

    CONSTRAINT "FK_RefreshTokens_Users"
        FOREIGN KEY ("UserId")
        REFERENCES "AspNetUsers"("Id")
        ON DELETE CASCADE,

    CONSTRAINT "CK_RefreshTokens_Expiry"
        CHECK ("ExpiresAt" > "CreatedAt")
);

CREATE UNIQUE INDEX "UX_RefreshTokens_TokenHash"
    ON "RefreshTokens" ("TokenHash");

CREATE INDEX "IX_RefreshTokens_UserId"
    ON "RefreshTokens" ("UserId");

CREATE INDEX "IX_RefreshTokens_Active"
    ON "RefreshTokens" ("UserId", "ExpiresAt")
    WHERE "RevokedAt" IS NULL;

-- ============================================================================
-- 10. TRIGGER CẬP NHẬT THỜI GIAN VÀ ROWVERSION
-- ============================================================================

CREATE OR REPLACE FUNCTION culinary_set_audit_fields()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        NEW."CreatedAt" := COALESCE(NEW."CreatedAt", NOW());
    ELSE
        NEW."UpdatedAt" := NOW();
    END IF;
    RETURN NEW;
END;
$$;

CREATE OR REPLACE FUNCTION culinary_refresh_row_version()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    NEW."RowVersion" := gen_random_bytes(8);
    RETURN NEW;
END;
$$;

-- Bảng Categories
CREATE TRIGGER "TR_Categories_Audit"
BEFORE INSERT OR UPDATE ON "Categories"
FOR EACH ROW EXECUTE FUNCTION culinary_set_audit_fields();

CREATE TRIGGER "TR_Categories_RowVersion"
BEFORE UPDATE ON "Categories"
FOR EACH ROW EXECUTE FUNCTION culinary_refresh_row_version();

-- Bảng Recipes
CREATE TRIGGER "TR_Recipes_Audit"
BEFORE INSERT OR UPDATE ON "Recipes"
FOR EACH ROW EXECUTE FUNCTION culinary_set_audit_fields();

CREATE TRIGGER "TR_Recipes_RowVersion"
BEFORE UPDATE ON "Recipes"
FOR EACH ROW EXECUTE FUNCTION culinary_refresh_row_version();

-- Bảng RecipeSteps
CREATE TRIGGER "TR_RecipeSteps_Audit"
BEFORE INSERT OR UPDATE ON "RecipeSteps"
FOR EACH ROW EXECUTE FUNCTION culinary_set_audit_fields();

CREATE TRIGGER "TR_RecipeSteps_RowVersion"
BEFORE UPDATE ON "RecipeSteps"
FOR EACH ROW EXECUTE FUNCTION culinary_refresh_row_version();

-- Bảng RecipeIngredients
CREATE TRIGGER "TR_RecipeIngredients_Audit"
BEFORE INSERT OR UPDATE ON "RecipeIngredients"
FOR EACH ROW EXECUTE FUNCTION culinary_set_audit_fields();

CREATE TRIGGER "TR_RecipeIngredients_RowVersion"
BEFORE UPDATE ON "RecipeIngredients"
FOR EACH ROW EXECUTE FUNCTION culinary_refresh_row_version();

-- Bảng RecipeImages
CREATE TRIGGER "TR_RecipeImages_Audit"
BEFORE INSERT OR UPDATE ON "RecipeImages"
FOR EACH ROW EXECUTE FUNCTION culinary_set_audit_fields();

CREATE TRIGGER "TR_RecipeImages_RowVersion"
BEFORE UPDATE ON "RecipeImages"
FOR EACH ROW EXECUTE FUNCTION culinary_refresh_row_version();

-- ============================================================================
-- 11. TRIGGER TÌM KIẾM TOÀN VĂN BẢN
--     Cấu hình "simple" kết hợp unaccent phù hợp để tìm kiếm nội dung tiếng Việt.
-- ============================================================================

CREATE OR REPLACE FUNCTION culinary_update_recipe_search_vector()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    NEW."SearchVector" :=
        to_tsvector(
            'simple',
            unaccent(
                COALESCE(NEW."Title", '') || ' ' ||
                COALESCE(NEW."Description", '')
            )
        );
    RETURN NEW;
END;
$$;

CREATE TRIGGER "TR_Recipes_SearchVector"
BEFORE INSERT OR UPDATE OF "Title", "Description"
ON "Recipes"
FOR EACH ROW
EXECUTE FUNCTION culinary_update_recipe_search_vector();

-- ============================================================================
-- 12. TỰ ĐỘNG GÁN SỐ THỨ TỰ BƯỚC
--     Trong ứng dụng thực tế, backend nên gán StepNumber bên trong transaction.
--     Trigger này là lớp dự phòng khi insert trực tiếp hoặc khi tạo dữ liệu mẫu.
-- ============================================================================

CREATE OR REPLACE FUNCTION culinary_assign_step_number()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF NEW."StepNumber" IS NULL OR NEW."StepNumber" <= 0 THEN
        SELECT COALESCE(MAX(rs."StepNumber"), 0) + 1
        INTO NEW."StepNumber"
        FROM "RecipeSteps" rs
        WHERE rs."RecipeId" = NEW."RecipeId"
          AND rs."IsDeleted" = false;
    END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER "TR_RecipeSteps_AssignStepNumber"
BEFORE INSERT ON "RecipeSteps"
FOR EACH ROW EXECUTE FUNCTION culinary_assign_step_number();

-- Hàm hỗ trợ để ứng dụng có thể đánh lại số thứ tự sau khi xóa mềm hoặc sắp xếp lại bước.
CREATE OR REPLACE FUNCTION culinary_renumber_recipe_steps(p_recipe_id uuid)
RETURNS void
LANGUAGE plpgsql
AS $$
BEGIN
    WITH ordered AS (
        SELECT
            "Id",
            row_number() OVER (ORDER BY "StepNumber", "CreatedAt", "Id") AS new_no
        FROM "RecipeSteps"
        WHERE "RecipeId" = p_recipe_id
          AND "IsDeleted" = false
    )
    UPDATE "RecipeSteps" rs
    SET "StepNumber" = ordered.new_no
    FROM ordered
    WHERE rs."Id" = ordered."Id";
END;
$$;

-- ============================================================================
-- 13. DỮ LIỆU MẪU CHO ROLE VÀ NGƯỜI DÙNG
--     PasswordHash được để NULL có chủ đích. KHÔNG lưu mật khẩu dạng văn bản thuần.
-- ============================================================================

INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES
    ('role-admin',  'Admin',  'ADMIN',  gen_random_uuid()::text),
    ('role-author', 'Author', 'AUTHOR', gen_random_uuid()::text);

INSERT INTO "AspNetUsers" (
    "Id", "UserName", "NormalizedUserName",
    "Email", "NormalizedEmail", "EmailConfirmed",
    "SecurityStamp", "ConcurrencyStamp",
    "DisplayName", "AvatarUrl", "Bio", "IsActive"
)
VALUES
    ('user-admin-001', 'admin', 'ADMIN',
     'admin@culinary.local', 'ADMIN@CULINARY.LOCAL', true,
     gen_random_uuid()::text, gen_random_uuid()::text,
     'Quản trị Culinary Blog',
     'https://picsum.photos/seed/admin/300/300',
     'Tài khoản quản trị mẫu phục vụ development.', true),

    ('user-author-001', 'author01', 'AUTHOR01',
     'author01@culinary.local', 'AUTHOR01@CULINARY.LOCAL', true,
     gen_random_uuid()::text, gen_random_uuid()::text,
     'Nguyễn An',
     'https://picsum.photos/seed/author01/300/300',
     'Tác giả yêu thích các món Việt Nam.', true),

    ('user-author-002', 'author02', 'AUTHOR02',
     'author02@culinary.local', 'AUTHOR02@CULINARY.LOCAL', true,
     gen_random_uuid()::text, gen_random_uuid()::text,
     'Trần Bình',
     'https://picsum.photos/seed/author02/300/300',
     'Tác giả chuyên món gia đình và món nhanh.', true),

    ('user-author-003', 'author03', 'AUTHOR03',
     'author03@culinary.local', 'AUTHOR03@CULINARY.LOCAL', true,
     gen_random_uuid()::text, gen_random_uuid()::text,
     'Lê Chi',
     'https://picsum.photos/seed/author03/300/300',
     'Tác giả chuyên bánh và món tráng miệng.', true),

    ('user-author-004', 'author04', 'AUTHOR04',
     'author04@culinary.local', 'AUTHOR04@CULINARY.LOCAL', true,
     gen_random_uuid()::text, gen_random_uuid()::text,
     'Phạm Dương',
     'https://picsum.photos/seed/author04/300/300',
     'Tác giả thích ẩm thực Á - Âu.', true),

    ('user-author-005', 'author05', 'AUTHOR05',
     'author05@culinary.local', 'AUTHOR05@CULINARY.LOCAL', true,
     gen_random_uuid()::text, gen_random_uuid()::text,
     'Võ Giang',
     'https://picsum.photos/seed/author05/300/300',
     'Tác giả chia sẻ công thức lành mạnh.', true);

INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES
    ('user-admin-001',  'role-admin'),
    ('user-admin-001',  'role-author'),
    ('user-author-001', 'role-author'),
    ('user-author-002', 'role-author'),
    ('user-author-003', 'role-author'),
    ('user-author-004', 'role-author'),
    ('user-author-005', 'role-author');

-- ============================================================================
-- 14. DỮ LIỆU MẪU: 20 DANH MỤC
-- ============================================================================

INSERT INTO "Categories"
("Name", "Slug", "Description", "ImageUrl", "OrderIndex")
VALUES
('Món khai vị',       'mon-khai-vi',       'Các món mở đầu nhẹ nhàng, kích thích vị giác.', 'https://picsum.photos/seed/cat01/1200/630', 1),
('Món chính',         'mon-chinh',         'Các món chính dùng trong bữa cơm gia đình.', 'https://picsum.photos/seed/cat02/1200/630', 2),
('Món canh',          'mon-canh',          'Các món canh thanh mát và bổ dưỡng.', 'https://picsum.photos/seed/cat03/1200/630', 3),
('Món xào',           'mon-xao',           'Các món xào nhanh, đậm vị.', 'https://picsum.photos/seed/cat04/1200/630', 4),
('Món kho',           'mon-kho',           'Các món kho truyền thống, đưa cơm.', 'https://picsum.photos/seed/cat05/1200/630', 5),
('Món chiên',         'mon-chien',         'Các món chiên giòn hấp dẫn.', 'https://picsum.photos/seed/cat06/1200/630', 6),
('Món nướng',         'mon-nuong',         'Các món nướng thơm ngon cho gia đình.', 'https://picsum.photos/seed/cat07/1200/630', 7),
('Món hấp',           'mon-hap',           'Các món hấp giữ vị tự nhiên.', 'https://picsum.photos/seed/cat08/1200/630', 8),
('Món nước',          'mon-nuoc',          'Phở, bún, mì và các món nước.', 'https://picsum.photos/seed/cat09/1200/630', 9),
('Cơm và cháo',       'com-va-chao',       'Cơm, cháo và các biến tấu từ gạo.', 'https://picsum.photos/seed/cat10/1200/630', 10),
('Bánh Việt Nam',     'banh-viet-nam',     'Các loại bánh truyền thống Việt Nam.', 'https://picsum.photos/seed/cat11/1200/630', 11),
('Bánh ngọt',         'banh-ngot',         'Bánh ngọt, bánh kem và món nướng ngọt.', 'https://picsum.photos/seed/cat12/1200/630', 12),
('Tráng miệng',       'trang-mieng',       'Các món tráng miệng sau bữa ăn.', 'https://picsum.photos/seed/cat13/1200/630', 13),
('Đồ uống',           'do-uong',           'Nước ép, sinh tố, trà và đồ uống không cồn.', 'https://picsum.photos/seed/cat14/1200/630', 14),
('Món chay',          'mon-chay',          'Các món chay đa dạng từ rau củ và đậu.', 'https://picsum.photos/seed/cat15/1200/630', 15),
('Món ăn sáng',       'mon-an-sang',       'Các món phù hợp cho bữa sáng.', 'https://picsum.photos/seed/cat16/1200/630', 16),
('Món ăn nhanh',      'mon-an-nhanh',      'Các món đơn giản, thời gian chế biến ngắn.', 'https://picsum.photos/seed/cat17/1200/630', 17),
('Món healthy',       'mon-healthy',       'Công thức cân bằng dinh dưỡng.', 'https://picsum.photos/seed/cat18/1200/630', 18),
('Ẩm thực Á',         'am-thuc-a',         'Các món tiêu biểu từ nhiều nền ẩm thực châu Á.', 'https://picsum.photos/seed/cat19/1200/630', 19),
('Ẩm thực Âu',        'am-thuc-au',        'Các món Âu phổ biến và dễ thực hiện.', 'https://picsum.photos/seed/cat20/1200/630', 20);

-- ============================================================================
-- 15. DỮ LIỆU MẪU: 100 CÔNG THỨC
--     Sử dụng hàm random() của PostgreSQL để tạo dữ liệu mẫu đa dạng.
--     Mỗi công thức sẽ có:
--       - một danh mục trong 20 danh mục
--       - một trong 5 tác giả mẫu
--       - dữ liệu dinh dưỡng được lưu trực tiếp trong Recipes
--       - trạng thái Published
--       - slug duy nhất từ recipe-001 đến recipe-100
-- ============================================================================

WITH
category_pool AS (
    SELECT array_agg("Id" ORDER BY "OrderIndex") AS ids
    FROM "Categories"
    WHERE "IsDeleted" = false
),
author_pool AS (
    SELECT ARRAY[
        'user-author-001',
        'user-author-002',
        'user-author-003',
        'user-author-004',
        'user-author-005'
    ]::varchar[] AS ids
),
dish_names AS (
    SELECT ARRAY[
        'Gà nướng mật ong', 'Bò xào rau củ', 'Cá kho tiêu', 'Canh chua cá',
        'Cơm chiên hải sản', 'Phở bò', 'Bún bò', 'Mì xào', 'Gỏi cuốn',
        'Chả giò', 'Thịt kho trứng', 'Sườn nướng', 'Tôm hấp', 'Cá chiên',
        'Salad gà', 'Cháo sườn', 'Cơm gà', 'Bánh xèo', 'Bánh cuốn',
        'Bánh flan', 'Chè đậu', 'Sinh tố xoài', 'Nước ép cam',
        'Đậu hũ sốt cà', 'Rau củ áp chảo', 'Mì Ý sốt bò', 'Pizza rau củ',
        'Khoai tây nghiền', 'Súp bí đỏ', 'Sandwich gà'
    ]::text[] AS names
)
INSERT INTO "Recipes" (
    "Title", "Slug", "Description", "Instructions",
    "PrepTime", "CookTime", "Servings", "Difficulty", "Status",
    "CategoryId", "AuthorId", "PublishedAt",
    "Nutrition_Calories", "Nutrition_Protein",
    "Nutrition_Carbohydrates", "Nutrition_Fat",
    "Nutrition_Fiber", "Nutrition_Sodium",
    "CreatedAt"
)
SELECT
    d.names[1 + floor(random() * array_length(d.names, 1))::int]
        || ' #' || lpad(gs::text, 3, '0') AS "Title",

    'recipe-' || lpad(gs::text, 3, '0') AS "Slug",

    'Công thức mẫu số ' || gs ||
    ' được tạo để phục vụ phát triển, kiểm thử tìm kiếm, lọc, sắp xếp và phân trang.'
        AS "Description",

    'Thực hiện lần lượt theo danh sách RecipeSteps. Điều chỉnh gia vị theo khẩu vị.'
        AS "Instructions",

    (10 + floor(random() * 31))::int AS "PrepTime",       -- từ 10 đến 40
    floor(random() * 61)::int AS "CookTime",              -- từ 0 đến 60
    (1 + floor(random() * 7))::int AS "Servings",         -- từ 1 đến 7
    (1 + floor(random() * 4))::smallint AS "Difficulty",  -- từ 1 đến 4
    1::smallint AS "Status",                              -- Đã xuất bản

    c.ids[1 + floor(random() * array_length(c.ids, 1))::int] AS "CategoryId",
    a.ids[1 + floor(random() * array_length(a.ids, 1))::int] AS "AuthorId",

    NOW() - (floor(random() * 365) || ' days')::interval AS "PublishedAt",

    round((180 + random() * 620)::numeric, 2) AS "Nutrition_Calories",
    round((5 + random() * 55)::numeric, 2) AS "Nutrition_Protein",
    round((10 + random() * 90)::numeric, 2) AS "Nutrition_Carbohydrates",
    round((3 + random() * 35)::numeric, 2) AS "Nutrition_Fat",
    round((1 + random() * 15)::numeric, 2) AS "Nutrition_Fiber",
    round((80 + random() * 1300)::numeric, 2) AS "Nutrition_Sodium",

    NOW() - (floor(random() * 365) || ' days')::interval AS "CreatedAt"
FROM generate_series(1, 100) gs
CROSS JOIN category_pool c
CROSS JOIN author_pool a
CROSS JOIN dish_names d;

-- ============================================================================
-- 16. DỮ LIỆU MẪU: NGUYÊN LIỆU
--     Mỗi công thức có ít nhất 10 nguyên liệu.
--     Script này tạo chính xác 12 nguyên liệu cho mỗi công thức = 1.200 bản ghi.
-- ============================================================================

WITH ingredient_pool AS (
    SELECT
        ARRAY[
            'Thịt gà', 'Thịt bò', 'Thịt heo', 'Cá', 'Tôm',
            'Trứng gà', 'Đậu hũ', 'Cà chua', 'Cà rốt', 'Khoai tây',
            'Hành tây', 'Hành lá', 'Tỏi', 'Gừng', 'Sả',
            'Ớt', 'Rau mùi', 'Rau xà lách', 'Nấm', 'Bắp cải',
            'Bí đỏ', 'Đậu que', 'Gạo', 'Bún', 'Mì',
            'Bột mì', 'Đường', 'Muối', 'Nước mắm', 'Dầu ăn',
            'Dầu hào', 'Nước tương', 'Tiêu', 'Sữa tươi', 'Bơ'
        ]::text[] AS names,

        ARRAY[
            'g', 'kg', 'ml', 'lít', 'muỗng cà phê',
            'muỗng canh', 'cái', 'quả', 'củ', 'nhánh'
        ]::text[] AS units
)
INSERT INTO "RecipeIngredients" (
    "RecipeId", "Name", "Quantity", "Unit", "Notes", "OrderIndex"
)
SELECT
    r."Id",
    p.names[1 + floor(random() * array_length(p.names, 1))::int]
        || ' ' || ing_no AS "Name",

    round((0.5 + random() * 499.5)::numeric, 3) AS "Quantity",

    p.units[1 + floor(random() * array_length(p.units, 1))::int] AS "Unit",

    CASE
        WHEN random() < 0.35 THEN 'Sơ chế sạch trước khi sử dụng'
        WHEN random() < 0.55 THEN 'Điều chỉnh lượng theo khẩu vị'
        ELSE NULL
    END AS "Notes",

    ing_no - 1 AS "OrderIndex"
FROM "Recipes" r
CROSS JOIN generate_series(1, 12) AS ing_no
CROSS JOIN ingredient_pool p
WHERE r."IsDeleted" = false;

-- ============================================================================
-- 17. DỮ LIỆU MẪU: CÁC BƯỚC CHẾ BIẾN
--     Mỗi công thức có ít nhất 5 bước chế biến.
--     Script này tạo chính xác 6 bước cho mỗi công thức = 600 bản ghi.
--     StepNumber chỉ được gán trực tiếp ở đây để dữ liệu seed có thứ tự ổn định; request từ client
--     trong ứng dụng thực tế nên bỏ trường này để backend tự gán.
-- ============================================================================

INSERT INTO "RecipeSteps" (
    "RecipeId", "StepNumber", "Title", "Description", "TimerMinutes", "ImageUrl"
)
SELECT
    r."Id",
    step_no,
    CASE step_no
        WHEN 1 THEN 'Chuẩn bị nguyên liệu'
        WHEN 2 THEN 'Sơ chế'
        WHEN 3 THEN 'Ướp và nêm gia vị'
        WHEN 4 THEN 'Chế biến chính'
        WHEN 5 THEN 'Hoàn thiện món'
        ELSE 'Trình bày và thưởng thức'
    END AS "Title",
    CASE step_no
        WHEN 1 THEN 'Cân, đong và chuẩn bị đầy đủ nguyên liệu theo danh sách.'
        WHEN 2 THEN 'Rửa sạch, cắt thái và sơ chế nguyên liệu phù hợp với món ăn.'
        WHEN 3 THEN 'Kết hợp gia vị và ướp nguyên liệu để hương vị thấm đều.'
        WHEN 4 THEN 'Tiến hành nấu, xào, hấp, nướng hoặc chế biến theo đặc trưng công thức.'
        WHEN 5 THEN 'Kiểm tra độ chín, nêm nếm lần cuối và tắt bếp.'
        ELSE 'Cho món ăn ra đĩa, trang trí phù hợp và dùng khi còn ngon.'
    END AS "Description",
    CASE step_no
        WHEN 1 THEN 10
        WHEN 2 THEN 10
        WHEN 3 THEN 15
        WHEN 4 THEN 25
        WHEN 5 THEN 5
        ELSE 3
    END AS "TimerMinutes",
    NULL
FROM "Recipes" r
CROSS JOIN generate_series(1, 6) AS step_no
WHERE r."IsDeleted" = false;

-- ============================================================================
-- 18. DỮ LIỆU MẪU: MỖI CÔNG THỨC CÓ MỘT ẢNH
-- ============================================================================

INSERT INTO "RecipeImages" (
    "RecipeId", "OriginalUrl", "MediumUrl", "ThumbnailUrl",
    "AltText", "IsPrimary", "OrderIndex"
)
SELECT
    r."Id",
    'https://picsum.photos/seed/' || r."Slug" || '/1200/800',
    'https://picsum.photos/seed/' || r."Slug" || '-medium/800/600',
    'https://picsum.photos/seed/' || r."Slug" || '-thumb/300/300',
    'Ảnh minh họa ' || r."Title",
    true,
    0
FROM "Recipes" r
WHERE r."IsDeleted" = false;

-- ============================================================================
-- 19. DỮ LIỆU MẪU REFRESH TOKEN (TÙY CHỌN)
--     Chỉ lưu giá trị TokenHash bằng SHA-256; không lưu refresh token gốc.
-- ============================================================================

INSERT INTO "RefreshTokens" (
    "UserId", "TokenHash", "ExpiresAt", "CreatedByIp"
)
SELECT
    u."Id",
    encode(digest(u."Id" || ':' || gen_random_uuid()::text, 'sha256'), 'hex'),
    NOW() + interval '7 days',
    '127.0.0.1'
FROM "AspNetUsers" u
WHERE u."Id" LIKE 'user-author-%';

COMMIT;

-- ============================================================================
-- 20. CÁC TRUY VẤN KIỂM TRA
--     Kết quả mong đợi sau khi chạy thành công:
--       Categories        = 20
--       Recipes           = 100
--       RecipeIngredients = 1.200 (mỗi recipe 12 nguyên liệu)
--       RecipeSteps       = 600 (mỗi recipe 6 bước)
--       RecipeImages      = 100
-- ============================================================================

SELECT COUNT(*) AS "CategoryCount"
FROM "Categories"
WHERE "IsDeleted" = false;

SELECT COUNT(*) AS "RecipeCount"
FROM "Recipes"
WHERE "IsDeleted" = false;

SELECT
    MIN(x.ingredient_count) AS "MinIngredientsPerRecipe",
    MAX(x.ingredient_count) AS "MaxIngredientsPerRecipe"
FROM (
    SELECT r."Id", COUNT(ri."Id") AS ingredient_count
    FROM "Recipes" r
    LEFT JOIN "RecipeIngredients" ri
        ON ri."RecipeId" = r."Id"
       AND ri."IsDeleted" = false
    WHERE r."IsDeleted" = false
    GROUP BY r."Id"
) x;

SELECT
    MIN(x.step_count) AS "MinStepsPerRecipe",
    MAX(x.step_count) AS "MaxStepsPerRecipe"
FROM (
    SELECT r."Id", COUNT(rs."Id") AS step_count
    FROM "Recipes" r
    LEFT JOIN "RecipeSteps" rs
        ON rs."RecipeId" = r."Id"
       AND rs."IsDeleted" = false
    WHERE r."IsDeleted" = false
    GROUP BY r."Id"
) x;

-- Kiểm tra khóa ngoại và toàn vẹn dữ liệu: kết quả đúng phải trả về 0 dòng.
SELECT r."Id", r."Title"
FROM "Recipes" r
LEFT JOIN "Categories" c ON c."Id" = r."CategoryId"
LEFT JOIN "AspNetUsers" u ON u."Id" = r."AuthorId"
WHERE c."Id" IS NULL OR u."Id" IS NULL;

-- Ví dụ truy vấn danh sách theo đúng quy ước sắp xếp của nhóm:
-- sortBy = createdAt, sortOrder = desc
SELECT
    r."Id",
    r."Title",
    r."Slug",
    r."CreatedAt"
FROM "Recipes" r
WHERE r."IsDeleted" = false
  AND r."Status" = 1
ORDER BY r."CreatedAt" DESC
LIMIT 12 OFFSET 0;
