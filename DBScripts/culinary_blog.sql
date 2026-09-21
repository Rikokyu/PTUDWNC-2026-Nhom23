-- ============================================================
-- CULINARY BLOG - POSTGRESQL DATABASE FOR LAB 2
-- Mục tiêu:
--   1) Tạo đầy đủ schema chính của Culinary Blog
--   2) Có ít nhất 20 categories
--   3) Có ít nhất 100 recipes
--   4) Mỗi recipe có ít nhất 10 nguyên liệu
--   5) Mỗi recipe có ít nhất 5 bước chế biến
--
--   - Tạo database mới, ví dụ: culinary_blog
--
-- LƯU Ý:
--   File này có DROP TABLE IF EXISTS ở đầu nên có thể chạy lại
--   trong môi trường development mà không bị lỗi "relation already exists".
-- ============================================================

BEGIN;

-- ============================================================
-- 0. XÓA CÁC BẢNG CŨ THEO THỨ TỰ PHỤ THUỘC
-- ============================================================

DROP TABLE IF EXISTS favorites CASCADE;
DROP TABLE IF EXISTS ratings CASCADE;
DROP TABLE IF EXISTS comments CASCADE;
DROP TABLE IF EXISTS recipe_nutritions CASCADE;
DROP TABLE IF EXISTS recipe_steps CASCADE;
DROP TABLE IF EXISTS recipe_ingredients CASCADE;
DROP TABLE IF EXISTS ingredients CASCADE;
DROP TABLE IF EXISTS recipe_categories CASCADE;
DROP TABLE IF EXISTS categories CASCADE;
DROP TABLE IF EXISTS recipes CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- ============================================================
-- 1. USERS
-- ============================================================

CREATE TABLE users (
    id BIGSERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(150) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(100),
    avatar_url TEXT,
    bio TEXT,

    role VARCHAR(20) NOT NULL DEFAULT 'USER'
        CHECK (role IN ('USER', 'ADMIN')),

    status VARCHAR(20) NOT NULL DEFAULT 'ACTIVE'
        CHECK (status IN ('ACTIVE', 'BLOCKED')),

    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================
-- 2. CATEGORIES
-- Xóa mềm bằng is_deleted
-- ============================================================

CREATE TABLE categories (
    id BIGSERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(120) NOT NULL,
    description TEXT,
    image_url TEXT,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX uq_categories_name_active
ON categories (LOWER(name))
WHERE is_deleted = FALSE;

CREATE UNIQUE INDEX uq_categories_slug_active
ON categories (LOWER(slug))
WHERE is_deleted = FALSE;

-- ============================================================
-- 3. RECIPES
-- Xóa mềm bằng is_deleted
-- ============================================================

CREATE TABLE recipes (
    id BIGSERIAL PRIMARY KEY,

    author_id BIGINT NOT NULL,

    title VARCHAR(200) NOT NULL,
    slug VARCHAR(220) NOT NULL,
    description TEXT,
    thumbnail_url TEXT,

    prep_time_minutes INTEGER
        CHECK (prep_time_minutes IS NULL OR prep_time_minutes >= 0),

    cook_time_minutes INTEGER
        CHECK (cook_time_minutes IS NULL OR cook_time_minutes >= 0),

    servings INTEGER
        CHECK (servings IS NULL OR servings > 0),

    difficulty VARCHAR(20)
        CHECK (
            difficulty IS NULL
            OR difficulty IN ('EASY', 'MEDIUM', 'HARD')
        ),

    status VARCHAR(20) NOT NULL DEFAULT 'DRAFT'
        CHECK (
            status IN ('DRAFT', 'PENDING', 'PUBLISHED', 'REJECTED')
        ),

    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,

    view_count BIGINT NOT NULL DEFAULT 0
        CHECK (view_count >= 0),

    published_at TIMESTAMPTZ,

    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_recipes_author
        FOREIGN KEY (author_id)
        REFERENCES users(id)
);

CREATE UNIQUE INDEX uq_recipes_slug_active
ON recipes (LOWER(slug))
WHERE is_deleted = FALSE;

-- ============================================================
-- 4. RECIPE_CATEGORIES
-- Quan hệ N-N giữa recipes và categories
-- ============================================================

CREATE TABLE recipe_categories (
    recipe_id BIGINT NOT NULL,
    category_id BIGINT NOT NULL,

    PRIMARY KEY (recipe_id, category_id),

    CONSTRAINT fk_recipe_categories_recipe
        FOREIGN KEY (recipe_id)
        REFERENCES recipes(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_recipe_categories_category
        FOREIGN KEY (category_id)
        REFERENCES categories(id)
);

-- ============================================================
-- 5. INGREDIENTS
-- ============================================================

CREATE TABLE ingredients (
    id BIGSERIAL PRIMARY KEY,
    name VARCHAR(150) NOT NULL UNIQUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================
-- 6. RECIPE_INGREDIENTS
-- quantity là số thập phân, unit là chuỗi
-- ============================================================

CREATE TABLE recipe_ingredients (
    id BIGSERIAL PRIMARY KEY,

    recipe_id BIGINT NOT NULL,
    ingredient_id BIGINT NOT NULL,

    quantity NUMERIC(10,3)
        CHECK (quantity IS NULL OR quantity >= 0),

    unit VARCHAR(50),
    note VARCHAR(255),

    sort_order INTEGER NOT NULL DEFAULT 0
        CHECK (sort_order >= 0),

    CONSTRAINT fk_recipe_ingredients_recipe
        FOREIGN KEY (recipe_id)
        REFERENCES recipes(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_recipe_ingredients_ingredient
        FOREIGN KEY (ingredient_id)
        REFERENCES ingredients(id),

    CONSTRAINT uq_recipe_ingredient
        UNIQUE (recipe_id, ingredient_id)
);

-- ============================================================
-- 7. RECIPE_STEPS
-- step_number do backend tự gán trong ứng dụng thật
-- ============================================================

CREATE TABLE recipe_steps (
    id BIGSERIAL PRIMARY KEY,

    recipe_id BIGINT NOT NULL,

    step_number INTEGER NOT NULL
        CHECK (step_number > 0),

    instruction TEXT NOT NULL,
    image_url TEXT,

    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_recipe_steps_recipe
        FOREIGN KEY (recipe_id)
        REFERENCES recipes(id)
        ON DELETE CASCADE,

    CONSTRAINT uq_recipe_step_number
        UNIQUE (recipe_id, step_number)
);

-- ============================================================
-- 8. RECIPE_NUTRITIONS
-- Quan hệ 1-1 với recipes
-- ============================================================

CREATE TABLE recipe_nutritions (
    recipe_id BIGINT PRIMARY KEY,

    calories NUMERIC(10,2)
        CHECK (calories IS NULL OR calories >= 0),

    protein NUMERIC(10,2)
        CHECK (protein IS NULL OR protein >= 0),

    carbohydrates NUMERIC(10,2)
        CHECK (carbohydrates IS NULL OR carbohydrates >= 0),

    fat NUMERIC(10,2)
        CHECK (fat IS NULL OR fat >= 0),

    fiber NUMERIC(10,2)
        CHECK (fiber IS NULL OR fiber >= 0),

    sugar NUMERIC(10,2)
        CHECK (sugar IS NULL OR sugar >= 0),

    sodium NUMERIC(10,2)
        CHECK (sodium IS NULL OR sodium >= 0),

    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_nutrition_recipe
        FOREIGN KEY (recipe_id)
        REFERENCES recipes(id)
        ON DELETE CASCADE
);

-- ============================================================
-- 9. COMMENTS
-- ============================================================

CREATE TABLE comments (
    id BIGSERIAL PRIMARY KEY,

    user_id BIGINT NOT NULL,
    recipe_id BIGINT NOT NULL,
    parent_id BIGINT,

    content TEXT NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,

    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_comments_user
        FOREIGN KEY (user_id)
        REFERENCES users(id),

    CONSTRAINT fk_comments_recipe
        FOREIGN KEY (recipe_id)
        REFERENCES recipes(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_comments_parent
        FOREIGN KEY (parent_id)
        REFERENCES comments(id)
        ON DELETE CASCADE
);

-- ============================================================
-- 10. RATINGS
-- ============================================================

CREATE TABLE ratings (
    id BIGSERIAL PRIMARY KEY,

    user_id BIGINT NOT NULL,
    recipe_id BIGINT NOT NULL,

    rating SMALLINT NOT NULL
        CHECK (rating BETWEEN 1 AND 5),

    review TEXT,

    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT uq_user_recipe_rating
        UNIQUE (user_id, recipe_id),

    CONSTRAINT fk_ratings_user
        FOREIGN KEY (user_id)
        REFERENCES users(id),

    CONSTRAINT fk_ratings_recipe
        FOREIGN KEY (recipe_id)
        REFERENCES recipes(id)
        ON DELETE CASCADE
);

-- ============================================================
-- 11. FAVORITES
-- ============================================================

CREATE TABLE favorites (
    user_id BIGINT NOT NULL,
    recipe_id BIGINT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (user_id, recipe_id),

    CONSTRAINT fk_favorites_user
        FOREIGN KEY (user_id)
        REFERENCES users(id),

    CONSTRAINT fk_favorites_recipe
        FOREIGN KEY (recipe_id)
        REFERENCES recipes(id)
        ON DELETE CASCADE
);

-- ============================================================
-- 12. INDEXES
-- ============================================================

CREATE INDEX idx_recipes_author_id
ON recipes(author_id);

CREATE INDEX idx_recipes_status
ON recipes(status);

CREATE INDEX idx_recipes_active
ON recipes(is_deleted)
WHERE is_deleted = FALSE;

CREATE INDEX idx_recipes_created_at
ON recipes(created_at DESC);

CREATE INDEX idx_categories_active
ON categories(is_deleted)
WHERE is_deleted = FALSE;

CREATE INDEX idx_recipe_ingredients_recipe_id
ON recipe_ingredients(recipe_id);

CREATE INDEX idx_recipe_ingredients_ingredient_id
ON recipe_ingredients(ingredient_id);

CREATE INDEX idx_recipe_steps_recipe_id
ON recipe_steps(recipe_id);

CREATE INDEX idx_ratings_recipe_id
ON ratings(recipe_id);

CREATE INDEX idx_comments_recipe_id
ON comments(recipe_id);

CREATE INDEX idx_favorites_user_id
ON favorites(user_id);

-- ============================================================
-- 13. SEED DATA
-- ============================================================

-- ------------------------------------------------------------
-- 13.1 USERS MẪU
-- ------------------------------------------------------------

INSERT INTO users (
    username,
    email,
    password_hash,
    full_name,
    role
)
VALUES
('admin',      'admin@culinaryblog.local', '$2b$12$development_admin_hash', 'Administrator', 'ADMIN'),
('user01',     'user01@example.com',        '$2b$12$development_user_hash',  'Người dùng 01', 'USER'),
('user02',     'user02@example.com',        '$2b$12$development_user_hash',  'Người dùng 02', 'USER'),
('user03',     'user03@example.com',        '$2b$12$development_user_hash',  'Người dùng 03', 'USER'),
('user04',     'user04@example.com',        '$2b$12$development_user_hash',  'Người dùng 04', 'USER');

-- ------------------------------------------------------------
-- 13.2 20 CATEGORIES
-- ------------------------------------------------------------

INSERT INTO categories (name, slug, description)
VALUES
('Món Việt Nam',     'mon-viet-nam',     'Các món ăn Việt Nam'),
('Món Á',            'mon-a',            'Các món ăn châu Á'),
('Món Âu',           'mon-au',           'Các món ăn châu Âu'),
('Món chay',         'mon-chay',         'Các món ăn chay'),
('Món tráng miệng',  'mon-trang-mieng',  'Các món tráng miệng'),
('Đồ uống',          'do-uong',          'Các loại đồ uống'),
('Món ăn sáng',      'mon-an-sang',      'Các món ăn sáng'),
('Món ăn trưa',      'mon-an-trua',      'Các món ăn trưa'),
('Món ăn tối',       'mon-an-toi',       'Các món ăn tối'),
('Món khai vị',      'mon-khai-vi',      'Các món khai vị'),
('Món chính',        'mon-chinh',        'Các món chính'),
('Món nước',         'mon-nuoc',         'Các món nước'),
('Món chiên',        'mon-chien',        'Các món chiên'),
('Món xào',          'mon-xao',          'Các món xào'),
('Món nướng',        'mon-nuong',        'Các món nướng'),
('Món hấp',          'mon-hap',          'Các món hấp'),
('Món kho',          'mon-kho',          'Các món kho'),
('Món canh',         'mon-canh',         'Các món canh'),
('Salad',            'salad',            'Các món salad'),
('Món ăn vặt',       'mon-an-vat',       'Các món ăn vặt');

-- ------------------------------------------------------------
-- 13.3 80 INGREDIENTS
-- ------------------------------------------------------------

INSERT INTO ingredients (name)
VALUES
('Thịt bò'), ('Thịt heo'), ('Thịt gà'), ('Cá hồi'), ('Cá basa'),
('Tôm'), ('Mực'), ('Trứng gà'), ('Đậu hũ'), ('Cà chua'),
('Cà rốt'), ('Khoai tây'), ('Khoai lang'), ('Hành tây'), ('Hành tím'),
('Hành lá'), ('Tỏi'), ('Gừng'), ('Sả'), ('Ớt'),
('Ớt chuông'), ('Rau cải'), ('Rau muống'), ('Xà lách'), ('Bắp cải'),
('Rau thơm'), ('Ngò rí'), ('Húng quế'), ('Nấm rơm'), ('Nấm hương'),
('Bí đỏ'), ('Bí xanh'), ('Đậu que'), ('Bắp ngọt'), ('Dưa leo'),
('Bún tươi'), ('Phở'), ('Mì'), ('Miến'), ('Cơm'),
('Gạo'), ('Bột mì'), ('Bột gạo'), ('Bột năng'), ('Đường'),
('Muối'), ('Tiêu'), ('Nước mắm'), ('Nước tương'), ('Dầu ăn'),
('Dầu mè'), ('Giấm'), ('Sữa tươi'), ('Sữa đặc'), ('Bơ'),
('Phô mai'), ('Kem tươi'), ('Chanh'), ('Cam'), ('Dừa'),
('Đậu phộng'), ('Mè trắng'), ('Mè đen'), ('Hạt điều'), ('Mật ong'),
('Nước cốt dừa'), ('Tương ớt'), ('Tương cà'), ('Dầu hào'), ('Hạt nêm'),
('Bột ngọt'), ('Quế'), ('Hoa hồi'), ('Thảo quả'), ('Lá chanh'),
('Lá dứa'), ('Đậu xanh'), ('Đậu đỏ'), ('Chuối'), ('Táo');

-- ------------------------------------------------------------
-- 13.4 100 RECIPES
-- ------------------------------------------------------------

INSERT INTO recipes (
    author_id,
    title,
    slug,
    description,
    thumbnail_url,
    prep_time_minutes,
    cook_time_minutes,
    servings,
    difficulty,
    status,
    view_count,
    published_at
)
SELECT
    2 + ((n - 1) % 4) AS author_id,
    'Công thức món ăn ' || LPAD(n::TEXT, 3, '0') AS title,
    'cong-thuc-mon-an-' || LPAD(n::TEXT, 3, '0') AS slug,
    'Dữ liệu mẫu cho công thức số ' || n || ' của Culinary Blog.' AS description,
    'https://picsum.photos/seed/recipe-' || n || '/800/600' AS thumbnail_url,
    5 + ((n * 3) % 40) AS prep_time_minutes,
    10 + ((n * 7) % 110) AS cook_time_minutes,
    1 + (n % 6) AS servings,
    CASE ((n - 1) % 3)
        WHEN 0 THEN 'EASY'
        WHEN 1 THEN 'MEDIUM'
        ELSE 'HARD'
    END AS difficulty,
    'PUBLISHED' AS status,
    (n * 37) % 5000 AS view_count,
    CURRENT_TIMESTAMP - ((100 - n) * INTERVAL '1 hour') AS published_at
FROM generate_series(1, 100) AS gs(n);

-- ------------------------------------------------------------
-- 13.5 MỖI RECIPE GẮN 2 CATEGORIES
-- ------------------------------------------------------------

INSERT INTO recipe_categories (recipe_id, category_id)
SELECT
    r.id,
    1 + ((r.id - 1) % 20)
FROM recipes r;

INSERT INTO recipe_categories (recipe_id, category_id)
SELECT
    r.id,
    1 + (r.id % 20)
FROM recipes r;

-- ------------------------------------------------------------
-- 13.6 MỖI RECIPE CÓ ĐÚNG 10 NGUYÊN LIỆU
-- ------------------------------------------------------------

INSERT INTO recipe_ingredients (
    recipe_id,
    ingredient_id,
    quantity,
    unit,
    note,
    sort_order
)
SELECT
    r.id AS recipe_id,

    1 + (((r.id - 1) * 10 + (pos - 1)) % 80) AS ingredient_id,

    ROUND(
        (
            0.5
            + (((r.id + pos) % 20) * 0.25)
        )::NUMERIC,
        3
    ) AS quantity,

    CASE (pos % 8)
        WHEN 0 THEN 'kg'
        WHEN 1 THEN 'g'
        WHEN 2 THEN 'ml'
        WHEN 3 THEN 'muỗng canh'
        WHEN 4 THEN 'muỗng cà phê'
        WHEN 5 THEN 'quả'
        WHEN 6 THEN 'củ'
        ELSE 'phần'
    END AS unit,

    CASE
        WHEN pos % 3 = 0 THEN 'Sơ chế sạch trước khi sử dụng'
        WHEN pos % 3 = 1 THEN 'Chuẩn bị theo khẩu phần'
        ELSE NULL
    END AS note,

    pos AS sort_order

FROM recipes r
CROSS JOIN generate_series(1, 10) AS gs(pos);

-- ------------------------------------------------------------
-- 13.7 MỖI RECIPE CÓ 5-8 BƯỚC
-- ============================================================

INSERT INTO recipe_steps (
    recipe_id,
    step_number,
    instruction,
    image_url
)
SELECT
    r.id AS recipe_id,
    s.step_number,

    CASE s.step_number
        WHEN 1 THEN 'Chuẩn bị đầy đủ nguyên liệu và dụng cụ cần thiết.'
        WHEN 2 THEN 'Rửa sạch và sơ chế nguyên liệu theo yêu cầu của món ăn.'
        WHEN 3 THEN 'Ướp hoặc nêm nguyên liệu với gia vị phù hợp.'
        WHEN 4 THEN 'Tiến hành chế biến nguyên liệu chính ở nhiệt độ phù hợp.'
        WHEN 5 THEN 'Nêm nếm lần cuối, hoàn thiện và trình bày món ăn.'
        WHEN 6 THEN 'Chuẩn bị phần nước sốt hoặc món ăn kèm nếu có.'
        WHEN 7 THEN 'Kiểm tra độ chín và điều chỉnh gia vị lần cuối.'
        WHEN 8 THEN 'Trang trí món ăn và sẵn sàng phục vụ.'
    END AS instruction,

    CASE
        WHEN s.step_number IN (1, 5)
            THEN 'https://picsum.photos/seed/recipe-step-' ||
                 r.id || '-' || s.step_number || '/800/600'
        ELSE NULL
    END AS image_url

FROM recipes r
CROSS JOIN LATERAL generate_series(
    1,
    5 + ((r.id - 1) % 4)
) AS s(step_number);

-- ------------------------------------------------------------
-- 13.8 NUTRITION CHO 100 RECIPES
-- ============================================================

INSERT INTO recipe_nutritions (
    recipe_id,
    calories,
    protein,
    carbohydrates,
    fat,
    fiber,
    sugar,
    sodium
)
SELECT
    r.id,
    250 + (r.id % 500),
    ROUND((10 + (r.id % 40) * 0.80)::NUMERIC, 2),
    ROUND((20 + (r.id % 70) * 1.10)::NUMERIC, 2),
    ROUND((5 + (r.id % 30) * 0.70)::NUMERIC, 2),
    ROUND((2 + (r.id % 10) * 0.40)::NUMERIC, 2),
    ROUND((3 + (r.id % 15) * 0.50)::NUMERIC, 2),
    200 + (r.id % 800)
FROM recipes r;

-- ------------------------------------------------------------
-- 13.9 COMMENT / RATING / FAVORITE MẪU
-- Không bắt buộc cho Lab 2 nhưng giúp database đầy đủ hơn
-- ------------------------------------------------------------

INSERT INTO comments (
    user_id,
    recipe_id,
    content
)
SELECT
    2,
    r.id,
    'Bình luận mẫu cho ' || r.title
FROM recipes r
WHERE r.id <= 20;

INSERT INTO ratings (
    user_id,
    recipe_id,
    rating,
    review
)
SELECT
    2,
    r.id,
    1 + ((r.id - 1) % 5)::INT,
    'Đánh giá mẫu cho ' || r.title
FROM recipes r
WHERE r.id <= 20;

INSERT INTO favorites (
    user_id,
    recipe_id
)
SELECT
    2,
    r.id
FROM recipes r
WHERE r.id <= 10;

-- ============================================================
-- 14. COMMENTS / DOCUMENTATION
-- ============================================================

COMMENT ON TABLE users IS
'Tài khoản người dùng. Admin và User dùng chung bảng, phân biệt bằng role.';

COMMENT ON TABLE categories IS
'Danh mục công thức. Dùng xóa mềm bằng is_deleted.';

COMMENT ON TABLE recipes IS
'Công thức nấu ăn. Dùng xóa mềm bằng is_deleted.';

COMMENT ON TABLE recipe_categories IS
'Bảng nối quan hệ nhiều-nhiều giữa recipes và categories.';

COMMENT ON TABLE ingredients IS
'Danh mục nguyên liệu dùng chung.';

COMMENT ON TABLE recipe_ingredients IS
'Nguyên liệu của từng công thức. quantity là NUMERIC, unit là chuỗi.';

COMMENT ON TABLE recipe_steps IS
'Các bước chế biến. step_number do backend tự gán trong ứng dụng thật.';

COMMENT ON TABLE recipe_nutritions IS
'Thông tin dinh dưỡng, quan hệ 1-1 với recipes.';

SELECT * FROM users;

SELECT * FROM categories;

SELECT * FROM recipes;

SELECT * FROM recipe_categories;

SELECT * FROM ingredients;

SELECT * FROM recipe_ingredients;

SELECT * FROM recipe_steps;

SELECT * FROM recipe_nutritions;

SELECT * FROM comments;

SELECT * FROM ratings;

SELECT * FROM favorites;


COMMIT;

-- ============================================================
-- KẾT QUẢ MONG ĐỢI:
-- categories                  = 20
-- recipes                     = 100
-- min_ingredients_per_recipe  = 10
-- min_steps_per_recipe        = 5
-- Hai truy vấn HAVING         = 0 dòng
-- ============================================================
