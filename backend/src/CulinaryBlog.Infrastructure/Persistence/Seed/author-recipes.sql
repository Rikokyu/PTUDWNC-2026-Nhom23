-- Nine complete, distinct Vietnamese recipes for the three curated public authors.
-- The account and category must already exist. Existing recipes are never changed.
-- Safe to run on every API startup.
BEGIN;
DO $seed$
DECLARE
    dish jsonb;
    ingredient jsonb;
    cooking_step jsonb;
    author_id uuid;
    category_id uuid;
    recipe_id uuid;
    position integer;
BEGIN
    FOR dish IN SELECT value FROM jsonb_array_elements($recipes$[
      {"user":"lannguyen","slug":"banh-beo-chen-hue","title":"Bánh bèo chén Huế","category":"mon-viet","description":"Bánh bèo mềm trong chén nhỏ, ăn với tôm cháy, hành phi và nước mắm ngọt.","prep":35,"cook":25,"servings":4,"difficulty":2,"ingredients":[["Bột gạo","200","g"],["Bột năng","40","g"],["Tôm tươi","250","g"],["Hành tím","3","củ"],["Nước mắm","3","muỗng canh"],["Đường","2","muỗng canh"]],"steps":[["Pha bột","Khuấy bột gạo và bột năng với 550 ml nước, để nghỉ 20 phút rồi khuấy lại cho bột không lắng."],["Làm nhân tôm","Luộc tôm chín, bóc vỏ, giã nhỏ rồi rang khô trên chảo với chút muối; phi hành tím vàng."],["Hấp bánh","Làm nóng chén trong xửng, rót một lớp bột mỏng và hấp khoảng 6 phút đến khi bánh trong."],["Dọn món","Cho tôm cháy và hành phi lên bánh; pha nước mắm với đường, nước ấm và chút ớt để dùng kèm."]]},
      {"user":"lannguyen","slug":"ga-hap-la-chanh","title":"Gà hấp lá chanh","category":"mon-viet","description":"Gà hấp mềm ngọt cùng sả và lá chanh, dùng nóng với muối tiêu chanh.","prep":20,"cook":35,"servings":4,"difficulty":1,"ingredients":[["Gà ta","1.2","kg"],["Lá chanh","8","lá"],["Sả","3","cây"],["Gừng","1","nhánh"],["Muối","1","muỗng cà phê"],["Tiêu","1","muỗng cà phê"]],"steps":[["Sơ chế","Làm sạch gà, xát muối rồi rửa và để ráo; đập dập sả, thái lát gừng và xé lá chanh."],["Ướp gà","Xoa muối và tiêu đều khắp gà, nhét một phần sả, gừng và lá chanh vào bụng, để 15 phút."],["Hấp","Lót sả dưới xửng, đặt gà lên trên và hấp lửa vừa 30–35 phút đến khi phần thịt dày nhất chín hoàn toàn."],["Dọn món","Để gà nghỉ 10 phút rồi chặt miếng, rắc lá chanh thái chỉ và ăn với muối tiêu chanh."]]},
      {"user":"lannguyen","slug":"che-bap-nuoc-cot-dua","title":"Chè bắp nước cốt dừa","category":"trang-mieng","description":"Hạt bắp ngọt nấu cùng bột năng, dùng với nước cốt dừa sánh nhẹ.","prep":15,"cook":30,"servings":4,"difficulty":1,"ingredients":[["Bắp ngọt","3","trái"],["Đường","100","g"],["Bột năng","30","g"],["Nước cốt dừa","200","ml"],["Muối","1","nhúm"]],"steps":[["Tách bắp","Tách hạt bắp, giữ lõi để nấu nước; hòa bột năng với một ít nước nguội."],["Nấu chè","Đun lõi bắp với 1 lít nước 15 phút rồi bỏ lõi, cho hạt bắp vào nấu mềm và thêm đường."],["Tạo độ sánh","Rót từ từ bột năng đã hòa vào nồi, khuấy đến khi nước chè trong và sánh nhẹ."],["Làm nước cốt dừa","Đun nước cốt dừa với một nhúm muối trên lửa nhỏ, không để sôi mạnh; chan lên chè khi dùng."]]},
      {"user":"minhtran","slug":"bo-luc-lac-khoai-tay","title":"Bò lúc lắc khoai tây","category":"mon-viet","description":"Thịt bò áp chảo mềm, đậm vị, dùng cùng khoai tây và rau xanh.","prep":25,"cook":20,"servings":3,"difficulty":2,"ingredients":[["Thịt thăn bò","500","g"],["Khoai tây","3","củ"],["Hành tây","1","củ"],["Tỏi","4","tép"],["Nước tương","2","muỗng canh"],["Dầu hào","1","muỗng canh"]],"steps":[["Ướp bò","Cắt bò thành miếng vuông vừa ăn, ướp với nước tương, dầu hào, tỏi băm và ít tiêu trong 15 phút."],["Làm khoai","Cắt khoai tây thành miếng dày, rửa bớt tinh bột, để ráo rồi chiên hoặc nướng đến khi chín vàng."],["Áp chảo bò","Làm nóng chảo trên lửa lớn, xào nhanh bò từng mẻ để thịt xém ngoài mà vẫn mềm; cho hành tây vào đảo vừa chín."],["Dọn món","Dùng bò ngay với khoai tây và rau xà lách; nêm thêm tiêu nếu thích."]]},
      {"user":"minhtran","slug":"ca-kho-to-truyen-thong","title":"Cá kho tộ truyền thống","category":"mon-viet","description":"Cá kho nước màu cùng tiêu và nước mắm, ăn với cơm nóng.","prep":20,"cook":45,"servings":4,"difficulty":2,"ingredients":[["Cá basa hoặc cá lóc","700","g"],["Thịt ba chỉ","150","g"],["Nước mắm","3","muỗng canh"],["Đường","2","muỗng canh"],["Hành tím","3","củ"],["Tiêu xay","1","muỗng cà phê"]],"steps":[["Sơ chế","Rửa cá với nước muối loãng, để ráo; cắt thịt ba chỉ miếng nhỏ, băm hành tím."],["Ướp cá","Ướp cá với nước mắm, hành tím, tiêu và một nửa lượng đường trong 20 phút."],["Kho cá","Đun phần đường còn lại thành màu cánh gián, cho thịt vào đảo săn rồi xếp cá lên; thêm nước nóng xâm xấp và kho lửa nhỏ 35–40 phút."],["Hoàn thành","Nêm lại cho vừa, kho đến khi nước sánh và cá chín kỹ, rắc thêm tiêu trước khi dùng."]]},
      {"user":"minhtran","slug":"nui-xao-bo-rau-cu","title":"Nui xào bò rau củ","category":"mon-nhanh","description":"Nui mềm vừa, xào với thịt bò, cà rốt và cải xanh cho bữa ăn nhanh.","prep":15,"cook":20,"servings":3,"difficulty":1,"ingredients":[["Nui khô","300","g"],["Thịt bò","300","g"],["Cà rốt","1","củ"],["Cải thìa","200","g"],["Tỏi","3","tép"],["Nước tương","2","muỗng canh"]],"steps":[["Luộc nui","Luộc nui trong nước sôi có chút muối theo thời gian trên bao bì, vớt ra và để ráo."],["Sơ chế","Thái mỏng thịt bò, ướp ít nước tương; thái cà rốt sợi và cắt cải thìa vừa ăn."],["Xào","Phi tỏi, xào bò nhanh trên lửa lớn rồi gắp ra; xào rau củ đến khi vừa chín, cho nui và bò vào đảo đều."],["Nêm vị","Thêm nước tương còn lại và ít tiêu, đảo thêm 1–2 phút rồi dùng nóng."]]},
      {"user":"admin","slug":"thit-kho-tau-trung-vit","title":"Thịt kho tàu trứng vịt","category":"mon-viet","description":"Thịt ba chỉ kho mềm cùng trứng vịt và nước dừa, vị mặn ngọt cân bằng.","prep":25,"cook":90,"servings":5,"difficulty":2,"ingredients":[["Thịt ba chỉ","800","g"],["Trứng vịt","5","quả"],["Nước dừa tươi","600","ml"],["Nước mắm","4","muỗng canh"],["Đường","3","muỗng canh"],["Hành tím","4","củ"]],"steps":[["Sơ chế","Cắt thịt thành miếng vuông, chần nhanh rồi rửa sạch; luộc trứng chín và bóc vỏ."],["Ướp thịt","Ướp thịt với nước mắm, hành tím băm và một ít đường trong 20 phút."],["Kho","Thắng phần đường còn lại thành màu vàng nâu, cho thịt vào đảo săn; thêm nước dừa và kho nhỏ lửa khoảng 60 phút."],["Cho trứng","Thả trứng vào nồi, kho thêm 20–30 phút để trứng thấm vị và thịt mềm; nêm lại trước khi dùng."]]},
      {"user":"admin","slug":"canh-bi-do-thit-bam","title":"Canh bí đỏ thịt bằm","category":"mon-viet","description":"Canh bí đỏ ngọt dịu nấu với thịt heo bằm và hành lá.","prep":15,"cook":25,"servings":4,"difficulty":1,"ingredients":[["Bí đỏ","500","g"],["Thịt heo bằm","200","g"],["Hành tím","2","củ"],["Hành lá","2","nhánh"],["Nước mắm","1","muỗng canh"]],"steps":[["Sơ chế","Gọt bí đỏ, bỏ hạt và cắt miếng vừa ăn; băm hành tím, thái hành lá."],["Xào thịt","Phi hành tím, cho thịt bằm vào đảo tơi đến khi đổi màu."],["Nấu canh","Thêm khoảng 1 lít nước, đun sôi rồi cho bí vào nấu 15–20 phút đến khi mềm; nêm nước mắm vừa ăn."],["Hoàn thành","Rắc hành lá và chút tiêu, dùng canh khi còn nóng."]]},
      {"user":"admin","slug":"bun-rieu-cua-ca-chua","title":"Bún riêu cua cà chua","category":"mon-viet","description":"Bún riêu cua với nước dùng cà chua thanh chua, ăn kèm rau sống.","prep":35,"cook":50,"servings":4,"difficulty":3,"ingredients":[["Cua đồng xay","500","g"],["Bún tươi","800","g"],["Cà chua","4","quả"],["Đậu hũ chiên","200","g"],["Mắm tôm","1","muỗng cà phê"],["Rau sống","1","đĩa"]],"steps":[["Lọc cua","Hòa cua xay với nước, bóp nhẹ rồi lọc qua rây nhiều lần lấy khoảng 1,5 lít nước cua."],["Nấu riêu","Đun nước cua trên lửa vừa, khuấy nhẹ lúc đầu; khi riêu nổi thành mảng thì hạ lửa và vớt riêu ra."],["Nấu nước dùng","Xào cà chua cho mềm, đổ vào nồi nước cua, thêm đậu hũ và mắm tôm; nêm vừa ăn rồi đun nhỏ lửa 15 phút."],["Dọn bún","Chần bún, xếp riêu cua lên trên, chan nước dùng nóng và dùng với rau sống."]]}
    ]$recipes$::jsonb) LOOP
        SELECT "Id" INTO author_id FROM "ApplicationUsers"
        WHERE "UserName" = dish->>'user' AND "IsActive" AND "Role" IN ('Author', 'Admin');
        IF author_id IS NULL THEN CONTINUE; END IF;
        SELECT "Id" INTO category_id FROM "Categories"
        WHERE "Slug" = dish->>'category' AND NOT "IsDeleted";
        IF category_id IS NULL THEN
            RAISE EXCEPTION 'Missing recipe category: %', dish->>'category';
        END IF;
        IF EXISTS (SELECT 1 FROM "Recipes" WHERE "Slug" = dish->>'slug') THEN CONTINUE; END IF;

        recipe_id := md5('curated-author-recipe:' || (dish->>'slug'))::uuid;
        INSERT INTO "Recipes" (
            "Id", "Title", "Slug", "Description", "Instructions",
            "PrepTimeMinutes", "CookTimeMinutes", "Servings", "Difficulty",
            "Status", "CategoryId", "AuthorId", "CreatedAt", "IsDeleted"
        ) VALUES (
            recipe_id, dish->>'title', dish->>'slug', dish->>'description',
            (SELECT string_agg(value->>1, E'\n' ORDER BY ord)
             FROM jsonb_array_elements(dish->'steps') WITH ORDINALITY AS step(value, ord)),
            (dish->>'prep')::int, (dish->>'cook')::int, (dish->>'servings')::int,
            (dish->>'difficulty')::int, 1, category_id, author_id, now(), false
        );
        position := 0;
        FOR ingredient IN SELECT value FROM jsonb_array_elements(dish->'ingredients') LOOP
            position := position + 1;
            INSERT INTO "RecipeIngredients" (
                "Id", "RecipeId", "Name", "Quantity", "Unit", "OrderIndex", "CreatedAt"
            ) VALUES (
                gen_random_uuid(), recipe_id, ingredient->>0, ingredient->>1,
                ingredient->>2, position, now()
            );
        END LOOP;
        position := 0;
        FOR cooking_step IN SELECT value FROM jsonb_array_elements(dish->'steps') LOOP
            position := position + 1;
            INSERT INTO "RecipeSteps" (
                "Id", "RecipeId", "StepNumber", "Title", "Description", "CreatedAt"
            ) VALUES (
                gen_random_uuid(), recipe_id, position, cooking_step->>0,
                cooking_step->>1, now()
            );
        END LOOP;
    END LOOP;
END;
$seed$;

-- Static WebP assets live with the Next.js frontend. Keep any image that an
-- author has already uploaded instead of replacing it during later startups.
INSERT INTO "RecipeImages" (
    "Id", "RecipeId", "OriginalUrl", "MediumUrl", "ThumbnailUrl",
    "AltText", "IsPrimary", "OrderIndex", "CreatedAt"
)
SELECT md5('curated-author-photo:' || recipe."Slug")::uuid,
       recipe."Id",
       '/images/dishes/' || recipe."Slug" || '.webp',
       '/images/dishes/medium/' || recipe."Slug" || '.webp',
       '/images/dishes/thumbnail/' || recipe."Slug" || '.webp',
       'Ảnh món ' || recipe."Title",
       true, 0, now()
FROM "Recipes" AS recipe
WHERE recipe."Slug" IN (
    'banh-beo-chen-hue', 'ga-hap-la-chanh', 'che-bap-nuoc-cot-dua',
    'bo-luc-lac-khoai-tay', 'ca-kho-to-truyen-thong', 'nui-xao-bo-rau-cu',
    'thit-kho-tau-trung-vit', 'canh-bi-do-thit-bam', 'bun-rieu-cua-ca-chua'
)
AND NOT EXISTS (
    SELECT 1 FROM "RecipeImages" AS image WHERE image."RecipeId" = recipe."Id"
)
ON CONFLICT ("Id") DO NOTHING;
COMMIT;
