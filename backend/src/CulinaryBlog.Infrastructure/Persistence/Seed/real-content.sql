-- Curated Vietnamese recipes for the local Culinary Blog database.
-- Re-runnable: existing slugs are left unchanged. Synthetic seed rows are soft hidden.
BEGIN;

INSERT INTO "Categories" ("Id","Name","Slug","Description","OrderIndex","CreatedAt","IsDeleted")
SELECT md5('culinary-category:' || category.slug)::uuid,
       category.name, category.slug, category.description, category.order_index, now(), false
FROM jsonb_to_recordset($categories$[
  {"name":"Món Việt","slug":"mon-viet","description":"Các món ăn truyền thống từ ba miền Việt Nam.","order_index":1},
  {"name":"Món Á","slug":"mon-a","description":"Ẩm thực đa dạng từ các quốc gia châu Á.","order_index":2},
  {"name":"Món Âu","slug":"mon-au","description":"Những món ăn mang phong cách châu Âu.","order_index":3},
  {"name":"Món chay","slug":"mon-chay","description":"Món chay thanh đạm từ rau củ và đậu.","order_index":4},
  {"name":"Tráng miệng","slug":"trang-mieng","description":"Bánh, chè và các món ngọt.","order_index":5},
  {"name":"Đồ uống","slug":"do-uong","description":"Thức uống nóng và lạnh.","order_index":6},
  {"name":"Bữa sáng","slug":"bua-sang","description":"Món ăn cho một ngày mới.","order_index":7},
  {"name":"Món nhanh","slug":"mon-nhanh","description":"Món đơn giản cho ngày bận rộn.","order_index":8}
]$categories$::jsonb) AS category(name text, slug text, description text, order_index integer)
ON CONFLICT ("Slug") DO NOTHING;

UPDATE "Recipes" SET "IsDeleted" = true, "UpdatedAt" = now()
WHERE "Slug" ~ '^cong-thuc-mon-an-[0-9]+$' AND NOT "IsDeleted";

UPDATE "Categories" SET "IsDeleted" = true, "UpdatedAt" = now()
WHERE "Slug" ~ '^danh-muc-mon-an-[0-9]+$' AND NOT "IsDeleted";

DO $seed$
DECLARE
  dish jsonb;
  ingredient jsonb;
  cooking_step jsonb;
  recipe_id uuid;
  category_id uuid;
  position integer;
BEGIN
  FOR dish IN SELECT value FROM jsonb_array_elements($data$[
    {"slug":"bun-cha-ha-noi","title":"Bún chả Hà Nội","category":"mon-viet","description":"Thịt nướng thơm ăn cùng bún, rau sống và nước mắm chua ngọt.","prep":30,"cook":25,"servings":4,"difficulty":2,"ingredients":[["Thịt nạc vai heo","500","g"],["Bún tươi","800","g"],["Hành tím","3","củ"],["Nước mắm","5","muỗng canh"],["Đường","3","muỗng canh"],["Rau sống","1","đĩa"]],"steps":[["Ướp thịt","Thái thịt mỏng, ướp cùng hành tím băm, nước mắm và một ít đường trong 30 phút."],["Nướng thịt","Nướng thịt trên than hoặc chảo nướng đến khi chín đều, mặt ngoài xém nhẹ."],["Pha nước chấm","Hòa nước mắm, đường, nước ấm và chút giấm; thêm tỏi, ớt theo khẩu vị. Dùng với bún và rau sống."]] },
    {"slug":"banh-xeo-tom-thit","title":"Bánh xèo tôm thịt","category":"mon-viet","description":"Bánh xèo vỏ giòn, nhân tôm thịt và giá, cuốn rau chấm nước mắm.","prep":25,"cook":30,"servings":4,"difficulty":2,"ingredients":[["Bột bánh xèo","400","g"],["Tôm","300","g"],["Thịt ba chỉ","250","g"],["Giá đỗ","300","g"],["Hành lá","3","nhánh"],["Rau sống","1","đĩa"]],"steps":[["Pha bột","Pha bột bánh xèo theo hướng dẫn trên gói; thêm hành lá cắt nhỏ và để nghỉ 20 phút."],["Làm nhân","Xào thịt ba chỉ và tôm đến khi chín, nêm vừa ăn."],["Đổ bánh","Làm nóng chảo có ít dầu, tráng bột mỏng, thêm nhân và giá. Đậy nắp vài phút rồi mở nắp cho vỏ giòn; gập đôi bánh."]] },
    {"slug":"canh-chua-ca","title":"Canh chua cá","category":"mon-viet","description":"Canh cá vị chua thanh với cà chua, dứa và rau thơm, ăn cùng cơm nóng.","prep":20,"cook":25,"servings":4,"difficulty":2,"ingredients":[["Cá lóc hoặc cá basa","600","g"],["Cà chua","2","quả"],["Dứa","150","g"],["Đậu bắp","150","g"],["Me chua","30","g"],["Rau ngổ và ngò gai","1","nắm"]],"steps":[["Sơ chế","Rửa cá, cắt khúc; cắt cà chua, dứa và đậu bắp vừa ăn. Dầm me với nước ấm rồi lọc lấy nước."],["Nấu nước canh","Đun nước sôi, cho cá vào nấu chín. Thêm cà chua, dứa và nước me; nêm nước mắm, đường cho vị chua ngọt cân bằng."],["Hoàn thành","Cho đậu bắp vào nấu thêm vài phút, tắt bếp và thêm rau thơm."]] },
    {"slug":"rau-muong-xao-toi","title":"Rau muống xào tỏi","category":"mon-chay","description":"Rau muống xanh giòn xào nhanh với tỏi phi thơm.","prep":10,"cook":8,"servings":4,"difficulty":1,"ingredients":[["Rau muống","500","g"],["Tỏi","5","tép"],["Dầu ăn","2","muỗng canh"],["Nước mắm chay","1","muỗng canh"]],"steps":[["Sơ chế","Nhặt rau muống, rửa sạch và để ráo. Băm tỏi."],["Xào rau","Phi thơm tỏi với dầu trên lửa lớn, cho rau vào đảo nhanh và thêm ít nước."],["Nêm vị","Nêm nước mắm chay, đảo đến khi rau vừa chín xanh rồi dùng ngay."]] },
    {"slug":"chao-ga-hanh-gung","title":"Cháo gà hành gừng","category":"bua-sang","description":"Cháo gà ấm bụng với gừng, hành lá và thịt gà xé.","prep":15,"cook":45,"servings":4,"difficulty":1,"ingredients":[["Gạo tẻ","200","g"],["Thịt gà","500","g"],["Gừng","1","nhánh"],["Hành lá","3","nhánh"],["Nước","1.5","lít"]],"steps":[["Luộc gà","Luộc gà với gừng và chút muối đến khi chín; vớt ra để nguội rồi xé sợi."],["Nấu cháo","Vo gạo, cho vào nước luộc gà và nấu lửa nhỏ đến khi hạt gạo nở mềm."],["Hoàn thành","Nêm vừa ăn, cho thịt gà xé và hành lá vào bát cháo nóng."]] },
    {"slug":"trung-chien-ca-chua","title":"Trứng chiên cà chua","category":"mon-nhanh","description":"Món trứng mềm với cà chua chín, dễ làm cho bữa cơm thường ngày.","prep":10,"cook":10,"servings":3,"difficulty":1,"ingredients":[["Trứng gà","4","quả"],["Cà chua","2","quả"],["Hành lá","2","nhánh"],["Dầu ăn","1","muỗng canh"]],"steps":[["Chuẩn bị","Đánh tan trứng với chút muối; cắt cà chua múi nhỏ và thái hành lá."],["Xào cà chua","Làm nóng dầu, xào cà chua đến khi mềm và ra nước."],["Chiên trứng","Đổ trứng vào chảo, đảo nhẹ trên lửa vừa đến khi vừa chín; rắc hành lá."]] },
    {"slug":"bun-bo-hue-gia-dinh","title":"Bún bò Huế tại nhà","category":"mon-viet","description":"Bún bò với nước dùng sả thơm, thịt bò mềm và chút vị cay đặc trưng.","prep":35,"cook":120,"servings":4,"difficulty":3,"ingredients":[["Bắp bò","600","g"],["Bún sợi to","800","g"],["Sả","5","cây"],["Mắm ruốc Huế","1","muỗng canh"],["Hành tây","1","củ"],["Rau sống","1","đĩa"]],"steps":[["Ninh nước dùng","Chần thịt bò rồi ninh với sả đập dập và hành tây đến khi mềm; hớt bọt để nước trong."],["Nêm vị","Hòa mắm ruốc với nước, để lắng rồi cho phần nước trong vào nồi. Nêm nước mắm và chút đường."],["Dọn bún","Chần bún, xếp thịt bò thái lát, chan nước dùng nóng và ăn cùng rau sống."]] },
    {"slug":"che-dau-xanh-nuoc-cot-dua","title":"Chè đậu xanh nước cốt dừa","category":"trang-mieng","description":"Chè đậu xanh mềm bùi, dùng nóng hoặc lạnh cùng nước cốt dừa.","prep":15,"cook":40,"servings":4,"difficulty":1,"ingredients":[["Đậu xanh cà vỏ","250","g"],["Đường","100","g"],["Nước cốt dừa","200","ml"],["Muối","1","nhúm"]],"steps":[["Ngâm đậu","Rửa đậu xanh và ngâm trong nước khoảng 2 giờ; xả sạch."],["Nấu chè","Nấu đậu với nước đến khi mềm, thêm đường và chút muối; khuấy nhẹ cho tan."],["Dọn món","Hâm ấm nước cốt dừa, chan lên chè khi dùng."]] },
    {"slug":"com-chien-trung","title":"Cơm chiên trứng","category":"mon-nhanh","description":"Cơm chiên vàng thơm với trứng, cà rốt và hành lá, phù hợp bữa ăn nhanh.","prep":10,"cook":15,"servings":3,"difficulty":1,"ingredients":[["Cơm nguội","600","g"],["Trứng gà","3","quả"],["Cà rốt","100","g"],["Đậu Hà Lan","100","g"],["Hành lá","2","nhánh"],["Dầu ăn","2","muỗng canh"]],"steps":[["Chuẩn bị","Đánh tan trứng; thái nhỏ cà rốt và hành lá. Tơi cơm nguội để hạt cơm rời nhau."],["Chiên trứng và rau","Làm nóng chảo với dầu, đảo trứng đến khi vừa chín; thêm cà rốt và đậu Hà Lan, xào mềm."],["Chiên cơm","Cho cơm vào chảo, đảo trên lửa lớn đến khi nóng đều và hơi vàng. Nêm vừa ăn, trộn hành lá rồi dùng nóng."]] }
  ]$data$::jsonb) LOOP
    IF EXISTS (SELECT 1 FROM "Recipes" WHERE "Slug" = dish->>'slug') THEN CONTINUE; END IF;
    SELECT "Id" INTO category_id FROM "Categories" WHERE "Slug" = dish->>'category' AND NOT "IsDeleted";
    IF category_id IS NULL THEN RAISE EXCEPTION 'Missing category %', dish->>'category'; END IF;
    recipe_id := md5('culinary-real:' || (dish->>'slug'))::uuid;
    INSERT INTO "Recipes" ("Id","Title","Slug","Description","Instructions","PrepTimeMinutes","CookTimeMinutes","Servings","Difficulty","Status","CategoryId","CreatedAt","IsDeleted")
    VALUES (recipe_id,dish->>'title',dish->>'slug',dish->>'description',(SELECT string_agg(value->>1,E'\n' ORDER BY ord) FROM jsonb_array_elements(dish->'steps') WITH ORDINALITY AS t(value,ord)),(dish->>'prep')::int,(dish->>'cook')::int,(dish->>'servings')::int,(dish->>'difficulty')::int,1,category_id,now(),false);
    position := 0;
    FOR ingredient IN SELECT value FROM jsonb_array_elements(dish->'ingredients') LOOP
      position := position + 1;
      INSERT INTO "RecipeIngredients" ("Id","RecipeId","Name","Quantity","Unit","OrderIndex","CreatedAt") VALUES (gen_random_uuid(),recipe_id,ingredient->>0,ingredient->>1,ingredient->>2,position,now());
    END LOOP;
    position := 0;
    FOR cooking_step IN SELECT value FROM jsonb_array_elements(dish->'steps') LOOP
      position := position + 1;
      INSERT INTO "RecipeSteps" ("Id","RecipeId","StepNumber","Title","Description","CreatedAt") VALUES (gen_random_uuid(),recipe_id,position,cooking_step->>0,cooking_step->>1,now());
    END LOOP;
  END LOOP;
END;
$seed$;

-- Project-owned dish photos are served from frontend/public/images/dishes.
-- Only add an image when the recipe does not already have one.
INSERT INTO "RecipeImages" ("Id","RecipeId","OriginalUrl","MediumUrl","ThumbnailUrl","AltText","IsPrimary","OrderIndex","CreatedAt")
SELECT md5('culinary-dish-photo:' || recipe."Slug")::uuid,
       recipe."Id", '/images/dishes/' || recipe."Slug" || '.webp',
       '/images/dishes/' || recipe."Slug" || '.webp',
       '/images/dishes/' || recipe."Slug" || '.webp',
       'Ảnh món ' || recipe."Title", true, 0, now()
FROM "Recipes" AS recipe
WHERE recipe."Slug" IN (
  'bun-cha-ha-noi','banh-xeo-tom-thit','canh-chua-ca','rau-muong-xao-toi',
  'chao-ga-hanh-gung','trung-chien-ca-chua','bun-bo-hue-gia-dinh',
  'che-dau-xanh-nuoc-cot-dua','com-chien-trung'
)
AND NOT EXISTS (SELECT 1 FROM "RecipeImages" AS image WHERE image."RecipeId" = recipe."Id")
ON CONFLICT ("Id") DO NOTHING;

-- Replace the identical placeholder image on each category with a related
-- photo already attached to a real recipe. Leave categories without one blank.
UPDATE "Categories" AS category
SET "ImageUrl" = (
  SELECT image."ThumbnailUrl"
  FROM "Recipes" AS recipe
  JOIN "RecipeImages" AS image ON image."RecipeId" = recipe."Id"
  WHERE recipe."Slug" = CASE category."Slug"
    WHEN 'mon-viet' THEN 'pho-bo-ha-noi'
    WHEN 'mon-chay' THEN 'salad-dau-hu-me-rang'
    WHEN 'trang-mieng' THEN 'banh-flan-caramel'
    WHEN 'do-uong' THEN 'ca-phe-sua-da'
    WHEN 'bua-sang' THEN 'banh-mi-trung-op-la'
    WHEN 'mon-a' THEN 'mi-xao-rau-cu'
    ELSE '' END
  LIMIT 1
), "UpdatedAt" = now()
WHERE category."Slug" IN ('mon-viet','mon-chay','trang-mieng','do-uong','bua-sang','mon-a','mon-au','mon-nhanh')
  AND category."ImageUrl" = '/images/image 1.png';
COMMIT;
