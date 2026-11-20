-- Curated, distinct recipes for the demo authors.
-- The accounts and categories must already exist; slug conflicts are not recreated.
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
      {"user":"author","slug":"banh-beo-chen-hue","title":"Bánh bèo chén Huế","category":"mon-viet","description":"Bánh bèo mềm trong chén nhỏ, ăn với tôm cháy, hành phi và nước mắm ngọt.","prep":35,"cook":25,"servings":4,"difficulty":2,"ingredients":[["Bột gạo","200","g"],["Bột năng","40","g"],["Tôm tươi","250","g"],["Hành tím","3","củ"],["Nước mắm","3","muỗng canh"],["Đường","2","muỗng canh"]],"steps":[["Pha bột","Khuấy bột gạo và bột năng với 550 ml nước, để nghỉ 20 phút rồi khuấy lại cho bột không lắng."],["Làm nhân tôm","Luộc tôm chín, bóc vỏ, giã nhỏ rồi rang khô trên chảo với chút muối; phi hành tím vàng."],["Hấp bánh","Làm nóng chén trong xửng, rót một lớp bột mỏng và hấp khoảng 6 phút đến khi bánh trong."],["Dọn món","Cho tôm cháy và hành phi lên bánh; pha nước mắm với đường, nước ấm và chút ớt để dùng kèm."]]},
      {"user":"author","slug":"ga-hap-la-chanh","title":"Gà hấp lá chanh","category":"mon-viet","description":"Gà hấp mềm ngọt cùng sả và lá chanh, dùng nóng với muối tiêu chanh.","prep":20,"cook":35,"servings":4,"difficulty":1,"ingredients":[["Gà ta","1.2","kg"],["Lá chanh","8","lá"],["Sả","3","cây"],["Gừng","1","nhánh"],["Muối","1","muỗng cà phê"],["Tiêu","1","muỗng cà phê"]],"steps":[["Sơ chế","Làm sạch gà, xát muối rồi rửa và để ráo; đập dập sả, thái lát gừng và xé lá chanh."],["Ướp gà","Xoa muối và tiêu đều khắp gà, nhét một phần sả, gừng và lá chanh vào bụng, để 15 phút."],["Hấp","Lót sả dưới xửng, đặt gà lên trên và hấp lửa vừa 30–35 phút đến khi phần thịt dày nhất chín hoàn toàn."],["Dọn món","Để gà nghỉ 10 phút rồi chặt miếng, rắc lá chanh thái chỉ và ăn với muối tiêu chanh."]]},
      {"user":"author","slug":"che-bap-nuoc-cot-dua","title":"Chè bắp nước cốt dừa","category":"trang-mieng","description":"Hạt bắp ngọt nấu cùng bột năng, dùng với nước cốt dừa sánh nhẹ.","prep":15,"cook":30,"servings":4,"difficulty":1,"ingredients":[["Bắp ngọt","3","trái"],["Đường","100","g"],["Bột năng","30","g"],["Nước cốt dừa","200","ml"],["Muối","1","nhúm"]],"steps":[["Tách bắp","Tách hạt bắp, giữ lõi để nấu nước; hòa bột năng với một ít nước nguội."],["Nấu chè","Đun lõi bắp với 1 lít nước 15 phút rồi bỏ lõi, cho hạt bắp vào nấu mềm và thêm đường."],["Tạo độ sánh","Rót từ từ bột năng đã hòa vào nồi, khuấy đến khi nước chè trong và sánh nhẹ."],["Làm nước cốt dừa","Đun nước cốt dừa với một nhúm muối trên lửa nhỏ, không để sôi mạnh; chan lên chè khi dùng."]]},
      {"user":"minhtran","slug":"bo-luc-lac-khoai-tay","title":"Bò lúc lắc khoai tây","category":"mon-viet","description":"Thịt bò áp chảo mềm, đậm vị, dùng cùng khoai tây và rau xanh.","prep":25,"cook":20,"servings":3,"difficulty":2,"ingredients":[["Thịt thăn bò","500","g"],["Khoai tây","3","củ"],["Hành tây","1","củ"],["Tỏi","4","tép"],["Nước tương","2","muỗng canh"],["Dầu hào","1","muỗng canh"]],"steps":[["Ướp bò","Cắt bò thành miếng vuông vừa ăn, ướp với nước tương, dầu hào, tỏi băm và ít tiêu trong 15 phút."],["Làm khoai","Cắt khoai tây thành miếng dày, rửa bớt tinh bột, để ráo rồi chiên hoặc nướng đến khi chín vàng."],["Áp chảo bò","Làm nóng chảo trên lửa lớn, xào nhanh bò từng mẻ để thịt xém ngoài mà vẫn mềm; cho hành tây vào đảo vừa chín."],["Dọn món","Dùng bò ngay với khoai tây và rau xà lách; nêm thêm tiêu nếu thích."]]},
      {"user":"minhtran","slug":"ca-kho-to-truyen-thong","title":"Cá kho tộ truyền thống","category":"mon-viet","description":"Cá kho nước màu cùng tiêu và nước mắm, ăn với cơm nóng.","prep":20,"cook":45,"servings":4,"difficulty":2,"ingredients":[["Cá basa hoặc cá lóc","700","g"],["Thịt ba chỉ","150","g"],["Nước mắm","3","muỗng canh"],["Đường","2","muỗng canh"],["Hành tím","3","củ"],["Tiêu xay","1","muỗng cà phê"]],"steps":[["Sơ chế","Rửa cá với nước muối loãng, để ráo; cắt thịt ba chỉ miếng nhỏ, băm hành tím."],["Ướp cá","Ướp cá với nước mắm, hành tím, tiêu và một nửa lượng đường trong 20 phút."],["Kho cá","Đun phần đường còn lại thành màu cánh gián, cho thịt vào đảo săn rồi xếp cá lên; thêm nước nóng xâm xấp và kho lửa nhỏ 35–40 phút."],["Hoàn thành","Nêm lại cho vừa, kho đến khi nước sánh và cá chín kỹ, rắc thêm tiêu trước khi dùng."]]},
      {"user":"minhtran","slug":"nui-xao-bo-rau-cu","title":"Nui xào bò rau củ","category":"mon-au","description":"Món pasta nui xào bò và rau củ theo kiểu bữa ăn gia đình, kết hợp nui luộc vừa chín với thịt bò áp chảo nhanh.","prep":15,"cook":20,"servings":3,"difficulty":1,"ingredients":[["Nui khô","300","g"],["Thịt bò","300","g"],["Cà rốt","1","củ"],["Cải thìa","200","g"],["Tỏi","3","tép"],["Nước tương","2","muỗng canh"]],"steps":[["Luộc nui","Luộc nui trong nước sôi có chút muối theo thời gian trên bao bì, vớt ra và để ráo."],["Sơ chế","Thái mỏng thịt bò, ướp ít nước tương; thái cà rốt sợi và cắt cải thìa vừa ăn."],["Xào","Phi tỏi, xào bò nhanh trên lửa lớn rồi gắp ra; xào rau củ đến khi vừa chín, cho nui và bò vào đảo đều."],["Nêm vị","Thêm nước tương còn lại và ít tiêu, đảo thêm 1–2 phút rồi dùng nóng."]]},
      {"user":"admin","slug":"thit-kho-tau-trung-vit","title":"Thịt kho tàu trứng vịt","category":"mon-viet","description":"Thịt ba chỉ kho mềm cùng trứng vịt và nước dừa, vị mặn ngọt cân bằng.","prep":25,"cook":90,"servings":5,"difficulty":2,"ingredients":[["Thịt ba chỉ","800","g"],["Trứng vịt","5","quả"],["Nước dừa tươi","600","ml"],["Nước mắm","4","muỗng canh"],["Đường","3","muỗng canh"],["Hành tím","4","củ"]],"steps":[["Sơ chế","Cắt thịt thành miếng vuông, chần nhanh rồi rửa sạch; luộc trứng chín và bóc vỏ."],["Ướp thịt","Ướp thịt với nước mắm, hành tím băm và một ít đường trong 20 phút."],["Kho","Thắng phần đường còn lại thành màu vàng nâu, cho thịt vào đảo săn; thêm nước dừa và kho nhỏ lửa khoảng 60 phút."],["Cho trứng","Thả trứng vào nồi, kho thêm 20–30 phút để trứng thấm vị và thịt mềm; nêm lại trước khi dùng."]]},
      {"user":"admin","slug":"canh-bi-do-thit-bam","title":"Canh bí đỏ thịt bằm","category":"mon-viet","description":"Canh bí đỏ ngọt dịu nấu với thịt heo bằm và hành lá.","prep":15,"cook":25,"servings":4,"difficulty":1,"ingredients":[["Bí đỏ","500","g"],["Thịt heo bằm","200","g"],["Hành tím","2","củ"],["Hành lá","2","nhánh"],["Nước mắm","1","muỗng canh"]],"steps":[["Sơ chế","Gọt bí đỏ, bỏ hạt và cắt miếng vừa ăn; băm hành tím, thái hành lá."],["Xào thịt","Phi hành tím, cho thịt bằm vào đảo tơi đến khi đổi màu."],["Nấu canh","Thêm khoảng 1 lít nước, đun sôi rồi cho bí vào nấu 15–20 phút đến khi mềm; nêm nước mắm vừa ăn."],["Hoàn thành","Rắc hành lá và chút tiêu, dùng canh khi còn nóng."]]},
      {"user":"admin","slug":"bun-rieu-cua-ca-chua","title":"Bún riêu cua cà chua","category":"mon-viet","description":"Bún riêu cua với nước dùng cà chua thanh chua, ăn kèm rau sống.","prep":35,"cook":50,"servings":4,"difficulty":3,"ingredients":[["Cua đồng xay","500","g"],["Bún tươi","800","g"],["Cà chua","4","quả"],["Đậu hũ chiên","200","g"],["Mắm tôm","1","muỗng cà phê"],["Rau sống","1","đĩa"]],"steps":[["Lọc cua","Hòa cua xay với nước, bóp nhẹ rồi lọc qua rây nhiều lần lấy khoảng 1,5 lít nước cua."],["Nấu riêu","Đun nước cua trên lửa vừa, khuấy nhẹ lúc đầu; khi riêu nổi thành mảng thì hạ lửa và vớt riêu ra."],["Nấu nước dùng","Xào cà chua cho mềm, đổ vào nồi nước cua, thêm đậu hũ và mắm tôm; nêm vừa ăn rồi đun nhỏ lửa 15 phút."],["Dọn bún","Chần bún, xếp riêu cua lên trên, chan nước dùng nóng và dùng với rau sống."]]},
      {"user":"author","slug":"pho-bo-tai-nam","title":"Phở bò tái nạm","category":"mon-viet","description":"Tô phở bò với nước dùng ninh thơm quế, hồi và gừng, ăn cùng bánh phở, thịt bò và rau thơm.","prep":35,"cook":150,"servings":5,"difficulty":3,"status":1,"ingredients":[["Xương ống bò","1.2","kg"],["Thịt nạm bò","400","g"],["Thịt bò phi lê","250","g"],["Bánh phở tươi","1","kg"],["Hành tây","2","củ"],["Gừng","100","g"],["Hoa hồi","3","cánh"],["Quế thanh","1","thanh"],["Nước mắm","3","muỗng canh"],["Hành lá và rau thơm","1","bó"]],"steps":[["Chần xương","Rửa xương, chần trong nước sôi 5 phút rồi rửa sạch để nước dùng trong hơn.",5],["Nướng gia vị","Nướng xém gừng và hành tây; rang nhanh hoa hồi, quế trên chảo khô để dậy mùi.",5],["Ninh nước dùng","Ninh xương với khoảng 3 lít nước, gừng, hành và gia vị trong 2 giờ; hớt bọt định kỳ. Cho nạm bò vào nấu đến mềm rồi vớt ra.",120],["Nêm nước phở","Lọc nước dùng, nêm nước mắm và muối vừa ăn. Thái mỏng nạm và phi lê bò.",10],["Dọn phở","Chần bánh phở, xếp thịt lên trên, chan nước dùng đang sôi và thêm hành lá, rau thơm.",3]]},
      {"user":"phuong","slug":"banh-cuon-nong-nhan-thit","title":"Bánh cuốn nóng nhân thịt","category":"bua-sang","description":"Bánh cuốn mỏng mềm cuộn nhân thịt heo, mộc nhĩ, dùng với hành phi và nước mắm pha.","prep":30,"cook":35,"servings":4,"difficulty":3,"status":1,"ingredients":[["Bột gạo","200","g"],["Bột năng","50","g"],["Thịt heo xay","250","g"],["Mộc nhĩ khô","15","g"],["Hành tím","4","củ"],["Nước mắm","3","muỗng canh"],["Dầu ăn","2","muỗng canh"],["Giá đỗ và rau thơm","1","đĩa"]],"steps":[["Pha bột","Khuấy bột gạo, bột năng với 600 ml nước và một nhúm muối; để bột nghỉ 20 phút.",20],["Chuẩn bị nhân","Ngâm mềm mộc nhĩ, băm nhỏ. Phi hành tím, xào thịt heo với mộc nhĩ đến khi chín và nêm vừa.",10],["Tráng bánh","Phết lớp dầu mỏng lên mặt chảo chống dính, đổ bột thành lớp mỏng, đậy nắp khoảng 1 phút đến khi bánh trong.",1],["Cuốn và dọn","Gỡ bánh ra đĩa, cho nhân vào giữa rồi cuốn lại. Rắc hành phi và dùng với nước mắm pha, giá cùng rau thơm.",5]]},
      {"user":"thanh","slug":"che-ba-mau","title":"Chè ba màu","category":"trang-mieng","description":"Món chè nhiều lớp gồm đậu xanh, đậu đỏ, thạch lá dứa và nước cốt dừa, dùng với đá khi trời nóng.","prep":30,"cook":60,"servings":6,"difficulty":2,"status":1,"ingredients":[["Đậu xanh cà vỏ","120","g"],["Đậu đỏ","150","g"],["Đậu trắng","120","g"],["Bột rau câu","8","g"],["Lá dứa","3","lá"],["Đường","180","g"],["Nước cốt dừa","250","ml"],["Muối","1","nhúm"]],"steps":[["Ngâm và nấu đậu","Ngâm riêng đậu đỏ và đậu trắng ít nhất 4 giờ. Nấu từng loại với nước đến khi mềm nhưng hạt còn nguyên; thêm đường vừa khẩu vị.",45],["Nấu đậu xanh","Vo đậu xanh, hấp hoặc nấu với lượng nước vừa đủ đến khi mềm bở; tán nhẹ và để nguội.",20],["Làm thạch lá dứa","Xay lá dứa với nước rồi lọc lấy nước màu. Nấu cùng bột rau câu và đường theo hướng dẫn trên gói, đổ khuôn cho đông rồi cắt hạt lựu.",15],["Hoàn thành","Múc lần lượt các loại đậu và thạch vào ly, chan nước cốt dừa đã đun ấm với chút muối; thêm đá khi dùng.",5]]},
      {"user":"phuong","slug":"dau-hu-sot-ca-chua","title":"Đậu hũ sốt cà chua","category":"mon-chay","description":"Đậu hũ áp chảo nhẹ rồi om trong sốt cà chua và hành, dùng cùng cơm nóng.","prep":15,"cook":20,"servings":3,"difficulty":1,"status":0,"ingredients":[["Đậu hũ trắng","400","g"],["Cà chua chín","3","quả"],["Hành tím","2","củ"],["Hành lá","2","nhánh"],["Dầu ăn","1","muỗng canh"],["Nước tương","1","muỗng canh"]],"steps":[["Sơ chế","Thấm khô đậu hũ rồi cắt miếng vuông; cắt cà chua hạt lựu, băm hành tím và thái hành lá.",5],["Áp chảo đậu","Làm nóng chảo với một ít dầu, áp chảo các mặt đậu đến khi vàng nhẹ rồi gắp ra.",8],["Nấu sốt","Phi hành tím, cho cà chua vào đảo mềm; thêm vài muỗng nước và nước tương. Cho đậu vào om nhỏ lửa đến khi sốt sệt, rắc hành lá.",10]]},
      {"user":"thanh","slug":"ga-kho-gung","title":"Gà kho gừng","category":"mon-viet","description":"Thịt gà kho với gừng, hành tím và nước mắm đến khi thấm vị, dùng trong bữa cơm gia đình.","prep":20,"cook":50,"servings":4,"difficulty":2,"status":2,"ingredients":[["Thịt gà","800","g"],["Gừng tươi","60","g"],["Hành tím","3","củ"],["Nước mắm","2","muỗng canh"],["Đường","1","muỗng canh"],["Nước dừa tươi","200","ml"],["Tiêu xay","1/2","muỗng cà phê"]],"steps":[["Sơ chế","Chặt gà thành miếng vừa ăn, rửa sạch và để ráo; thái gừng sợi, băm hành tím.",10],["Ướp gà","Ướp gà với nước mắm, đường, hành tím và một nửa lượng gừng trong 20 phút.",20],["Kho gà","Thắng đường đến màu cánh gián, cho gà vào đảo săn. Thêm nước dừa và phần gừng còn lại, đun sôi rồi hạ lửa nhỏ.",35],["Hoàn thành","Kho đến khi thịt chín mềm và nước sốt sánh, nêm lại vừa ăn rồi rắc tiêu.",5]]},
      {"user":"minhtran","slug":"mi-quang-tom-thit","title":"Mì Quảng tôm thịt","category":"mon-viet","description":"Mì Quảng với tôm, thịt rim đậm đà, nước dùng vừa đủ, ăn kèm rau sống, đậu phộng và bánh tráng nướng.","prep":30,"cook":45,"servings":4,"difficulty":3,"status":1,"ingredients":[["Tôm tươi","300","g"],["Thịt ba chỉ","300","g"],["Mì Quảng","800","g"],["Trứng cút","8","quả"],["Nước mắm","2","muỗng canh"],["Dầu điều","1","muỗng canh"],["Đậu phộng rang","50","g"],["Bánh tráng nướng","2","cái"],["Rau sống","1","đĩa"]],"steps":[["Sơ chế","Rửa tôm, cắt bỏ râu; thái thịt ba chỉ miếng mỏng. Luộc trứng cút chín và bóc vỏ.",15],["Ướp nguyên liệu","Ướp tôm và thịt với nước mắm, hành tím băm, tiêu và một ít dầu điều trong 15 phút.",15],["Rim tôm thịt","Phi thơm hành, cho thịt vào đảo săn rồi thêm tôm và trứng cút. Thêm nước nóng vừa đủ, rim đến khi tôm chín và nước dùng đậm vị.",20],["Chuẩn bị mì và rau","Chần mì Quảng theo hướng dẫn trên bao bì, rửa và để ráo rau sống; giã dập đậu phộng rang.",5],["Dọn món","Cho mì vào tô, xếp tôm, thịt và trứng lên trên, chan một lượng nước dùng vừa phải. Thêm rau, đậu phộng và bánh tráng nướng.",3]]}
    ]$recipes$::jsonb) LOOP
        SELECT "Id" INTO author_id FROM "ApplicationUsers"
        WHERE "UserName" = dish->>'user' AND "IsActive" AND "Role" IN ('Author', 'Admin');
        IF author_id IS NULL THEN
            RAISE EXCEPTION 'Missing active recipe author: %', dish->>'user';
        END IF;
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
            (dish->>'difficulty')::int, COALESCE((dish->>'status')::int, 1), category_id, author_id, now(), false
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
                "Id", "RecipeId", "StepNumber", "Title", "Description", "TimerMinutes", "CreatedAt"
            ) VALUES (
                gen_random_uuid(), recipe_id, position, cooking_step->>0,
                cooking_step->>1,
                CASE WHEN cooking_step->>2 IS NULL THEN NULL ELSE (cooking_step->>2)::integer END,
                now()
            );
        END LOOP;
    END LOOP;
END;
$seed$;

-- Keep the existing seed-owned pasta recipe in the same category on later runs.
UPDATE "Recipes"
SET "CategoryId" = category."Id", "UpdatedAt" = now()
FROM "Categories" AS category
WHERE "Recipes"."Id" = md5('curated-author-recipe:nui-xao-bo-rau-cu')::uuid
  AND "Recipes"."Slug" = 'nui-xao-bo-rau-cu'
  AND category."Slug" = 'mon-au'
  AND NOT "Recipes"."IsDeleted";

-- Local recipe assets live with the Next.js frontend. Keep any image that an
-- author has already uploaded instead of replacing it during later startups.
INSERT INTO "RecipeImages" (
    "Id", "RecipeId", "OriginalUrl", "MediumUrl", "ThumbnailUrl",
    "AltText", "IsPrimary", "OrderIndex", "CreatedAt"
)
SELECT md5('curated-author-photo:' || recipe."Slug")::uuid,
       recipe."Id",
       CASE recipe."Slug"
           WHEN 'pho-bo-tai-nam' THEN '/images/hero-pho-editorial.webp'
           WHEN 'banh-cuon-nong-nhan-thit' THEN '/images/dishes/admin-banh-cuon-nong-nhan-thit.jpg'
           WHEN 'che-ba-mau' THEN '/images/dishes/admin-che-ba-mau.jpg'
           WHEN 'dau-hu-sot-ca-chua' THEN '/images/dishes/admin-dau-hu-sot-ca-chua.jpg'
           WHEN 'ga-kho-gung' THEN '/images/dishes/admin-ga-kho-gung.jpg'
           WHEN 'mi-quang-tom-thit' THEN '/images/dishes/admin-mi-quang-tom-thit.jpg'
           ELSE '/images/dishes/' || recipe."Slug" || '.webp'
       END,
       CASE recipe."Slug"
           WHEN 'pho-bo-tai-nam' THEN '/images/hero-pho-editorial.webp'
           WHEN 'banh-cuon-nong-nhan-thit' THEN '/images/dishes/admin-banh-cuon-nong-nhan-thit.jpg'
           WHEN 'che-ba-mau' THEN '/images/dishes/admin-che-ba-mau.jpg'
           WHEN 'dau-hu-sot-ca-chua' THEN '/images/dishes/admin-dau-hu-sot-ca-chua.jpg'
           WHEN 'ga-kho-gung' THEN '/images/dishes/admin-ga-kho-gung.jpg'
           WHEN 'mi-quang-tom-thit' THEN '/images/dishes/admin-mi-quang-tom-thit.jpg'
           ELSE '/images/dishes/medium/' || recipe."Slug" || '.webp'
       END,
       CASE recipe."Slug"
           WHEN 'pho-bo-tai-nam' THEN '/images/hero-pho-editorial.webp'
           WHEN 'banh-cuon-nong-nhan-thit' THEN '/images/dishes/admin-banh-cuon-nong-nhan-thit.jpg'
           WHEN 'che-ba-mau' THEN '/images/dishes/admin-che-ba-mau.jpg'
           WHEN 'dau-hu-sot-ca-chua' THEN '/images/dishes/admin-dau-hu-sot-ca-chua.jpg'
           WHEN 'ga-kho-gung' THEN '/images/dishes/admin-ga-kho-gung.jpg'
           WHEN 'mi-quang-tom-thit' THEN '/images/dishes/admin-mi-quang-tom-thit.jpg'
           ELSE '/images/dishes/thumbnail/' || recipe."Slug" || '.webp'
       END,
       'Ảnh món ' || recipe."Title",
       true, 0, now()
FROM "Recipes" AS recipe
WHERE recipe."Slug" IN (
    'banh-beo-chen-hue', 'ga-hap-la-chanh', 'che-bap-nuoc-cot-dua',
    'bo-luc-lac-khoai-tay', 'ca-kho-to-truyen-thong', 'nui-xao-bo-rau-cu',
    'thit-kho-tau-trung-vit', 'canh-bi-do-thit-bam', 'bun-rieu-cua-ca-chua',
    'pho-bo-tai-nam', 'banh-cuon-nong-nhan-thit', 'che-ba-mau',
    'dau-hu-sot-ca-chua', 'ga-kho-gung', 'mi-quang-tom-thit'
)
AND NOT EXISTS (
    SELECT 1 FROM "RecipeImages" AS image WHERE image."RecipeId" = recipe."Id"
)
ON CONFLICT ("Id") DO NOTHING;

-- Nutrition figures are approximate per-serving demo values, not verified
-- nutrition-label data. Fill only completely empty seed nutrition fields.
UPDATE "Recipes" AS recipe
SET "Nutrition_Calories" = nutrition.calories,
    "Nutrition_Protein" = nutrition.protein,
    "Nutrition_Carbohydrates" = nutrition.carbs,
    "Nutrition_Fat" = nutrition.fat,
    "Nutrition_Fiber" = nutrition.fiber,
    "Nutrition_Sodium" = nutrition.sodium,
    "UpdatedAt" = now()
FROM jsonb_to_recordset($nutrition$[
    {"slug":"bun-cha-ha-noi","calories":520,"protein":31,"carbs":62,"fat":17,"fiber":4,"sodium":820},
    {"slug":"banh-xeo-tom-thit","calories":460,"protein":24,"carbs":48,"fat":19,"fiber":4,"sodium":780},
    {"slug":"canh-chua-ca","calories":210,"protein":26,"carbs":18,"fat":6,"fiber":4,"sodium":690},
    {"slug":"rau-muong-xao-toi","calories":95,"protein":4,"carbs":8,"fat":6,"fiber":4,"sodium":310},
    {"slug":"chao-ga-hanh-gung","calories":330,"protein":24,"carbs":39,"fat":8,"fiber":1,"sodium":540},
    {"slug":"trung-chien-ca-chua","calories":220,"protein":15,"carbs":8,"fat":14,"fiber":2,"sodium":420},
    {"slug":"bun-bo-hue-gia-dinh","calories":570,"protein":35,"carbs":68,"fat":18,"fiber":4,"sodium":1150},
    {"slug":"che-dau-xanh-nuoc-cot-dua","calories":310,"protein":9,"carbs":52,"fat":8,"fiber":6,"sodium":110},
    {"slug":"com-chien-trung","calories":410,"protein":14,"carbs":58,"fat":13,"fiber":4,"sodium":620},
    {"slug":"com-chien-duong-chau","calories":490,"protein":23,"carbs":61,"fat":17,"fiber":3,"sodium":760},
    {"slug":"banh-beo-chen-hue","calories":340,"protein":18,"carbs":52,"fat":7,"fiber":2,"sodium":680},
    {"slug":"ga-hap-la-chanh","calories":390,"protein":42,"carbs":2,"fat":23,"fiber":0,"sodium":520},
    {"slug":"che-bap-nuoc-cot-dua","calories":290,"protein":5,"carbs":55,"fat":7,"fiber":3,"sodium":95},
    {"slug":"bo-luc-lac-khoai-tay","calories":480,"protein":34,"carbs":36,"fat":22,"fiber":4,"sodium":710},
    {"slug":"ca-kho-to-truyen-thong","calories":420,"protein":32,"carbs":14,"fat":25,"fiber":1,"sodium":850},
    {"slug":"nui-xao-bo-rau-cu","calories":460,"protein":29,"carbs":57,"fat":13,"fiber":5,"sodium":680},
    {"slug":"thit-kho-tau-trung-vit","calories":560,"protein":32,"carbs":20,"fat":38,"fiber":1,"sodium":790},
    {"slug":"canh-bi-do-thit-bam","calories":170,"protein":12,"carbs":19,"fat":6,"fiber":3,"sodium":430},
    {"slug":"bun-rieu-cua-ca-chua","calories":450,"protein":27,"carbs":60,"fat":11,"fiber":5,"sodium":930},
    {"slug":"pho-bo-tai-nam","calories":520,"protein":34,"carbs":63,"fat":15,"fiber":3,"sodium":980},
    {"slug":"banh-cuon-nong-nhan-thit","calories":370,"protein":19,"carbs":55,"fat":9,"fiber":2,"sodium":720},
    {"slug":"che-ba-mau","calories":360,"protein":10,"carbs":65,"fat":8,"fiber":8,"sodium":120},
    {"slug":"dau-hu-sot-ca-chua","calories":230,"protein":14,"carbs":14,"fat":13,"fiber":4,"sodium":460},
    {"slug":"ga-kho-gung","calories":430,"protein":34,"carbs":13,"fat":26,"fiber":1,"sodium":770},
    {"slug":"mi-quang-tom-thit","calories":560,"protein":32,"carbs":66,"fat":19,"fiber":5,"sodium":910}
]$nutrition$::jsonb) AS nutrition(slug text, calories numeric, protein numeric, carbs numeric, fat numeric, fiber numeric, sodium numeric)
WHERE recipe."Slug" = nutrition.slug
  AND recipe."Slug" IN (
      'bun-cha-ha-noi','banh-xeo-tom-thit','canh-chua-ca','rau-muong-xao-toi',
      'chao-ga-hanh-gung','trung-chien-ca-chua','bun-bo-hue-gia-dinh',
      'che-dau-xanh-nuoc-cot-dua','com-chien-trung','com-chien-duong-chau',
      'banh-beo-chen-hue','ga-hap-la-chanh','che-bap-nuoc-cot-dua',
      'bo-luc-lac-khoai-tay','ca-kho-to-truyen-thong','nui-xao-bo-rau-cu',
      'thit-kho-tau-trung-vit','canh-bi-do-thit-bam','bun-rieu-cua-ca-chua',
      'pho-bo-tai-nam','banh-cuon-nong-nhan-thit','che-ba-mau',
      'dau-hu-sot-ca-chua','ga-kho-gung','mi-quang-tom-thit'
  )
  AND recipe."Nutrition_Calories" IS NULL
  AND recipe."Nutrition_Protein" IS NULL
  AND recipe."Nutrition_Carbohydrates" IS NULL
  AND recipe."Nutrition_Fat" IS NULL
  AND recipe."Nutrition_Fiber" IS NULL
  AND recipe."Nutrition_Sodium" IS NULL;

-- Seed saved items only for active demo readers and public recipes.
INSERT INTO "SavedRecipes" ("UserId", "RecipeId", "CreatedAt")
SELECT app_user."Id", recipe."Id", now()
FROM (VALUES
    ('user', 'pho-bo-tai-nam'),
    ('user', 'mi-quang-tom-thit'),
    ('user', 'bun-cha-ha-noi'),
    ('an', 'che-ba-mau'),
    ('an', 'com-chien-duong-chau'),
    ('an', 'banh-xeo-tom-thit')
) AS demo_save(user_name, recipe_slug)
JOIN "ApplicationUsers" AS app_user
  ON app_user."UserName" = demo_save.user_name
 AND app_user."IsActive"
 AND app_user."Role" = 'User'
JOIN "Recipes" AS recipe
  ON recipe."Slug" = demo_save.recipe_slug
 AND recipe."Status" = 1
 AND NOT recipe."IsDeleted"
ON CONFLICT ("UserId", "RecipeId") DO NOTHING;
COMMIT;
