-- Additional curated recipes to complete the 12-category, 60-recipe demo set.
-- Re-runnable: recipe content with existing slugs is left unchanged.
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
    SELECT "Id" INTO author_id
    FROM "ApplicationUsers"
    WHERE "UserName" = 'author' AND "IsActive" AND "Role" = 'Author';
    IF author_id IS NULL THEN
        RAISE EXCEPTION 'Missing active curated recipe author: author';
    END IF;

    FOR dish IN SELECT value FROM jsonb_array_elements($recipes$[
      {"slug":"ca-phe-sua-da","title":"Cà phê sữa đá","category":"do-uong","description":"Cà phê phin đậm đà hòa cùng sữa đặc, dùng với đá viên.","prep":5,"cook":8,"servings":1,"difficulty":1,"ingredients":[["Cà phê rang xay","25","g"],["Sữa đặc","30","ml"],["Nước nóng","100","ml"],["Đá viên","1","ly"]],"steps":[["Tráng phin","Tráng phin bằng nước nóng, cho cà phê vào và lắc nhẹ để mặt bột phẳng."],["Ủ cà phê","Rót một ít nước nóng để ủ cà phê 30 giây, sau đó thêm nước đến gần đầy phin và đậy nắp."],["Pha và dùng","Chờ cà phê nhỏ giọt hết vào ly có sữa đặc, khuấy đều rồi thêm đá viên."]]},
      {"slug":"tra-chanh-sa","title":"Trà chanh sả","category":"do-uong","description":"Trà xanh thơm nhẹ kết hợp chanh tươi và sả, uống mát sau khi để lạnh.","prep":10,"cook":10,"servings":3,"difficulty":1,"ingredients":[["Trà xanh","10","g"],["Sả","2","cây"],["Chanh","2","quả"],["Đường","50","g"],["Nước","700","ml"],["Đá viên","1","ly"]],"steps":[["Nấu nước sả","Rửa sả, đập dập và nấu với nước khoảng 5 phút rồi tắt bếp."],["Ủ trà","Cho trà vào nước sả nóng, ủ 3 phút rồi lọc bỏ bã và để nguội."],["Pha trà chanh","Hòa đường vào trà, thêm nước cốt chanh và điều chỉnh vị chua ngọt."],["Hoàn thành","Rót trà ra ly, thêm đá và vài lát chanh để dùng lạnh."]]},
      {"slug":"sinh-to-bo","title":"Sinh tố bơ","category":"do-uong","description":"Sinh tố bơ sánh mịn pha cùng sữa tươi và sữa đặc.","prep":10,"cook":0,"servings":2,"difficulty":1,"ingredients":[["Bơ chín","2","quả"],["Sữa tươi không đường","150","ml"],["Sữa đặc","30","ml"],["Đá viên","1","ly"]],"steps":[["Sơ chế bơ","Bổ đôi bơ, bỏ hạt và dùng thìa lấy phần thịt bơ."],["Xay sinh tố","Cho bơ, sữa tươi, sữa đặc và đá vào máy xay, xay đến khi mịn."],["Điều chỉnh và dùng","Nếm thử, thêm sữa nếu muốn loãng hoặc ngọt hơn; rót ra ly và dùng ngay."]]},
      {"slug":"nuoc-ep-cam-ca-rot","title":"Nước ép cam cà rốt","category":"do-uong","description":"Nước ép cam tươi và cà rốt có vị ngọt dịu, dùng ngay sau khi pha.","prep":10,"cook":0,"servings":2,"difficulty":1,"ingredients":[["Cam","3","quả"],["Cà rốt","2","củ"],["Mật ong","1","muỗng cà phê"],["Đá viên","1","ly"]],"steps":[["Sơ chế","Rửa sạch cà rốt, gọt vỏ và cắt khúc; rửa cam rồi bổ đôi."],["Ép nguyên liệu","Ép cà rốt bằng máy ép, vắt cam lấy nước và lọc hạt."],["Pha nước ép","Trộn nước cam với nước cà rốt, thêm mật ong nếu thích và khuấy đều."],["Dùng ngay","Rót ra ly, thêm đá tùy thích và dùng khi còn tươi."]]},
      {"slug":"tom-rang-thit-ba-chi","title":"Tôm rang thịt ba chỉ","category":"hai-san","description":"Tôm rang săn cùng thịt ba chỉ và nước mắm, ăn với cơm nóng.","prep":20,"cook":25,"servings":4,"difficulty":2,"ingredients":[["Tôm tươi","400","g"],["Thịt ba chỉ","200","g"],["Nước mắm","2","muỗng canh"],["Đường","1","muỗng canh"],["Hành tím","2","củ"],["Tiêu xay","1/2","muỗng cà phê"]],"steps":[["Sơ chế","Rửa tôm, bỏ chỉ lưng; thái thịt ba chỉ miếng mỏng và băm hành tím."],["Rang thịt","Cho thịt vào chảo nóng rang đến khi ra mỡ và cạnh thịt hơi vàng."],["Rang tôm","Thêm hành tím, cho tôm vào đảo đến khi tôm chuyển màu; nêm nước mắm và đường."],["Hoàn thành","Rang lửa vừa đến khi nước sốt sệt bám đều, rắc tiêu và dùng nóng."]]},
      {"slug":"muc-xao-sa-ot","title":"Mực xào sả ớt","category":"hai-san","description":"Mực xào nhanh với sả, ớt và hành tây, giữ độ giòn ngọt tự nhiên.","prep":20,"cook":12,"servings":3,"difficulty":2,"ingredients":[["Mực ống","500","g"],["Sả","3","cây"],["Ớt chuông","1","quả"],["Hành tây","1/2","củ"],["Nước mắm","1","muỗng canh"],["Dầu ăn","1","muỗng canh"]],"steps":[["Làm sạch mực","Làm sạch mực, bỏ túi mực và thái khoanh; để ráo rồi ướp chút muối."],["Chuẩn bị rau","Băm sả, thái lát hành tây và cắt ớt chuông thành miếng vừa ăn."],["Xào mực","Phi thơm sả, cho mực vào xào lửa lớn khoảng 2 phút; thêm hành tây và ớt chuông."],["Nêm món","Nêm nước mắm, đảo thêm đến khi mực vừa chín thì tắt bếp để mực không dai."]]},
      {"slug":"ca-hap-xi-dau","title":"Cá hấp xì dầu","category":"hai-san","description":"Cá hấp mềm cùng gừng, hành lá và nước tương đậm đà.","prep":15,"cook":20,"servings":3,"difficulty":2,"ingredients":[["Cá diêu hồng","700","g"],["Nước tương","3","muỗng canh"],["Gừng","1","nhánh"],["Hành lá","3","nhánh"],["Dầu mè","1","muỗng cà phê"],["Đường","1","muỗng cà phê"]],"steps":[["Sơ chế cá","Làm sạch cá, khứa vài đường hai bên mình cá để cá nhanh chín và thấm gia vị."],["Chuẩn bị nước sốt","Trộn nước tương, dầu mè, đường và 2 muỗng canh nước; thái sợi gừng và hành lá."],["Hấp cá","Đặt cá lên đĩa chịu nhiệt, rải gừng rồi hấp khoảng 15–20 phút đến khi thịt cá chín."],["Hoàn thành","Rưới nước sốt lên cá nóng, thêm hành lá và dùng ngay."]]},
      {"slug":"chao-hau","title":"Cháo hàu","category":"hai-san","description":"Cháo gạo mềm nấu với hàu tươi, gừng và hành lá.","prep":20,"cook":40,"servings":4,"difficulty":2,"ingredients":[["Hàu tươi","300","g"],["Gạo tẻ","150","g"],["Gạo nếp","50","g"],["Gừng","1","nhánh"],["Hành lá","3","nhánh"],["Nước mắm","1","muỗng canh"]],"steps":[["Sơ chế hàu và gạo","Rửa hàu nhẹ tay, để ráo; vo chung gạo tẻ và gạo nếp rồi để ráo."],["Nấu cháo","Cho gạo vào nồi với khoảng 1,5 lít nước, nấu lửa nhỏ đến khi hạt gạo nở mềm."],["Xào hàu","Phi thơm gừng thái sợi, cho hàu vào xào nhanh 1–2 phút và nêm chút nước mắm."],["Hoàn thành","Cho hàu vào nồi cháo, nấu thêm vài phút, nêm vừa ăn và rắc hành lá."]]},
      {"slug":"ga-nuong-mat-ong","title":"Gà nướng mật ong","category":"mon-ga","description":"Đùi gà ướp mật ong, nước tương và tỏi rồi nướng đến khi da vàng thơm.","prep":25,"cook":35,"servings":4,"difficulty":2,"ingredients":[["Đùi gà","4","cái"],["Mật ong","2","muỗng canh"],["Nước tương","2","muỗng canh"],["Tỏi","3","tép"],["Dầu ăn","1","muỗng canh"],["Tiêu xay","1/2","muỗng cà phê"]],"steps":[["Sơ chế và ướp","Rửa sạch đùi gà, thấm khô; trộn mật ong, nước tương, tỏi băm, dầu và tiêu rồi ướp gà ít nhất 20 phút."],["Làm nóng lò","Làm nóng lò ở 200°C, xếp gà lên khay có lót giấy nướng."],["Nướng gà","Nướng 30–35 phút, trở mặt và phết phần sốt ướp thêm một lần để gà vàng đều; kiểm tra thịt chín hoàn toàn."],["Dọn món","Để gà nghỉ vài phút trước khi dùng cùng cơm hoặc rau củ."]]},
      {"slug":"ga-xao-sa-ot","title":"Gà xào sả ớt","category":"mon-ga","description":"Thịt gà xào với sả, ớt và nước mắm, thơm cay vừa vị cho bữa cơm.","prep":20,"cook":20,"servings":4,"difficulty":2,"ingredients":[["Thịt đùi gà","500","g"],["Sả","3","cây"],["Ớt đỏ","1","quả"],["Nước mắm","2","muỗng canh"],["Đường","1","muỗng cà phê"],["Dầu ăn","1","muỗng canh"]],"steps":[["Sơ chế","Cắt gà thành miếng vừa ăn; băm sả, thái lát ớt và ướp gà với nước mắm, đường trong 15 phút."],["Phi thơm sả","Làm nóng dầu, cho sả và ớt vào phi thơm trên lửa vừa."],["Xào gà","Cho gà vào chảo, đảo đến khi săn mặt rồi hạ lửa, đậy nắp và nấu đến khi thịt chín."],["Hoàn thành","Mở nắp, đảo đến khi nước sốt bám đều quanh thịt rồi dọn nóng."]]},
      {"slug":"ga-sot-cam","title":"Gà sốt cam","category":"mon-ga","description":"Ức gà áp chảo ăn cùng sốt cam chua ngọt thơm nhẹ.","prep":20,"cook":20,"servings":3,"difficulty":2,"ingredients":[["Ức gà","400","g"],["Cam","2","quả"],["Mật ong","1","muỗng canh"],["Nước tương","1","muỗng canh"],["Bột bắp","1","muỗng cà phê"],["Dầu ăn","1","muỗng canh"]],"steps":[["Chuẩn bị gà và cam","Thấm khô ức gà, thái miếng vừa ăn; vắt cam lấy nước và hòa bột bắp với 2 muỗng canh nước."],["Áp chảo gà","Nêm gà với chút muối, áp chảo cùng dầu đến khi vàng hai mặt và chín bên trong; gắp ra."],["Nấu sốt cam","Đun nước cam với mật ong và nước tương, rót bột bắp đã hòa vào khuấy đến khi sốt hơi sánh."],["Hoàn thành","Cho gà trở lại chảo đảo phủ sốt, nêm vừa ăn và dùng nóng."]]},
      {"slug":"canh-ga-chien-nuoc-mam","title":"Cánh gà chiên nước mắm","category":"mon-ga","description":"Cánh gà chiên vàng áo sốt nước mắm tỏi ngọt mặn hài hòa.","prep":20,"cook":25,"servings":4,"difficulty":2,"ingredients":[["Cánh gà","700","g"],["Nước mắm","2","muỗng canh"],["Đường","1","muỗng canh"],["Tỏi","4","tép"],["Bột bắp","2","muỗng canh"],["Dầu ăn","300","ml"]],"steps":[["Ướp gà","Rửa cánh gà, thấm thật khô, ướp với chút muối và tiêu trong 15 phút."],["Chiên gà","Áo mỏng cánh gà với bột bắp, chiên ngập dầu ở lửa vừa đến khi vàng và chín; để ráo dầu."],["Pha sốt","Trộn nước mắm và đường với 2 muỗng canh nước; băm nhỏ tỏi."],["Áo sốt","Phi thơm tỏi, đổ hỗn hợp nước mắm vào đun sôi nhẹ rồi cho gà vào đảo nhanh đến khi sốt bám đều."]]},
      {"slug":"suon-nuong-mat-ong","title":"Sườn nướng mật ong","category":"mon-nuong","description":"Sườn heo ướp mật ong, tỏi và nước tương, nướng mềm thơm.","prep":30,"cook":45,"servings":4,"difficulty":2,"ingredients":[["Sườn non","800","g"],["Mật ong","2","muỗng canh"],["Nước tương","2","muỗng canh"],["Tỏi","4","tép"],["Dầu ăn","1","muỗng canh"],["Tiêu xay","1/2","muỗng cà phê"]],"steps":[["Sơ chế sườn","Chặt sườn miếng vừa ăn, chần sơ 2 phút rồi rửa sạch và để ráo."],["Ướp sườn","Trộn mật ong, nước tương, tỏi băm, dầu và tiêu; ướp sườn ít nhất 30 phút."],["Nướng sườn","Làm nóng lò ở 190°C, xếp sườn lên khay và nướng khoảng 40–45 phút, trở mặt giữa thời gian nướng."],["Hoàn thành","Phết phần sốt còn lại lên sườn trong 5 phút cuối, kiểm tra sườn chín mềm rồi dùng nóng."]]},
      {"slug":"bo-nuong-la-lot","title":"Bò nướng lá lốt","category":"mon-nuong","description":"Thịt bò băm cuốn lá lốt, nướng thơm và dùng với bún hoặc bánh tráng.","prep":30,"cook":15,"servings":4,"difficulty":2,"ingredients":[["Thịt bò xay","400","g"],["Lá lốt","25","lá"],["Mỡ heo","50","g"],["Sả","1","cây"],["Nước mắm","1","muỗng canh"],["Dầu ăn","1","muỗng canh"]],"steps":[["Chuẩn bị nhân","Băm nhỏ mỡ heo và sả, trộn với thịt bò, nước mắm, tiêu rồi để thấm 15 phút."],["Cuốn bò","Rửa lá lốt, để khô; đặt một thìa nhân lên mặt lá, cuốn chặt và cố định bằng cuống lá."],["Nướng bò","Phết ít dầu lên cuốn bò, nướng trên vỉ hoặc chảo nướng lửa vừa, trở đều đến khi lá se thơm và nhân chín."],["Dọn món","Xếp bò ra đĩa, dùng nóng với bún, rau sống và nước chấm phù hợp."]]},
      {"slug":"ca-nuong-giay-bac","title":"Cá nướng giấy bạc","category":"mon-nuong","description":"Cá ướp sả gừng và gia vị rồi nướng trong giấy bạc để giữ độ ngọt.","prep":20,"cook":30,"servings":3,"difficulty":2,"ingredients":[["Cá rô phi","700","g"],["Sả","2","cây"],["Gừng","1","nhánh"],["Nước mắm","1","muỗng canh"],["Dầu ăn","1","muỗng canh"],["Chanh","1/2","quả"]],"steps":[["Sơ chế cá","Làm sạch cá, khứa nhẹ hai bên, thấm ráo và xát chút muối cùng nước cốt chanh."],["Ướp cá","Băm sả và gừng, trộn với nước mắm và dầu rồi xoa đều lên cá, để thấm 15 phút."],["Bọc cá","Đặt cá lên giấy bạc, rải phần sả gừng còn lại lên trên và gói kín."],["Nướng cá","Nướng ở 200°C khoảng 25–30 phút tùy độ dày; mở giấy bạc cẩn thận và kiểm tra cá chín trước khi dọn."]]},
      {"slug":"banh-trang-nuong","title":"Bánh tráng nướng","category":"mon-nuong","description":"Bánh tráng nướng giòn với trứng, hành lá và tôm khô, ăn nóng.","prep":15,"cook":10,"servings":2,"difficulty":1,"ingredients":[["Bánh tráng","2","cái"],["Trứng cút","4","quả"],["Tôm khô","20","g"],["Hành lá","2","nhánh"],["Bơ","1","muỗng canh"],["Tương ớt","1","muỗng canh"]],"steps":[["Chuẩn bị","Ngâm tôm khô cho mềm rồi băm nhỏ; thái hành lá và đánh tan trứng cút."],["Nướng bánh","Đặt bánh tráng lên chảo hoặc vỉ nướng lửa nhỏ, phết một lớp bơ mỏng."],["Thêm nhân","Rải hành lá và tôm khô lên bánh, rót trứng đều mặt bánh rồi nướng đến khi trứng se và bánh giòn."],["Dọn món","Thêm tương ớt theo khẩu vị, gập bánh hoặc cắt miếng và ăn khi còn nóng."]]},
      {"slug":"bun-thit-nuong","title":"Bún thịt nướng","category":"mon-nuoc","description":"Bún tươi ăn với thịt heo nướng sả, rau sống, đồ chua và nước mắm pha.","prep":30,"cook":20,"servings":4,"difficulty":2,"ingredients":[["Thịt nạc vai heo","500","g"],["Bún tươi","800","g"],["Sả","2","cây"],["Nước mắm","3","muỗng canh"],["Rau sống","1","đĩa"],["Đồ chua","150","g"]],"steps":[["Ướp thịt","Thái thịt mỏng, ướp với sả băm, nước mắm, đường và chút dầu trong ít nhất 20 phút."],["Nướng thịt","Nướng thịt trên vỉ hoặc áp chảo đến khi chín và xém cạnh, tránh để thịt khô."],["Pha nước mắm","Hòa nước mắm với đường, nước ấm và nước cốt chanh; thêm tỏi ớt băm theo khẩu vị."],["Dọn bún","Cho bún vào tô, xếp rau sống, đồ chua và thịt nướng lên trên, dùng cùng nước mắm pha."]]},
      {"slug":"hu-tieu-nam-vang","title":"Hủ tiếu Nam Vang","category":"mon-nuoc","description":"Hủ tiếu với tôm, thịt bằm và trứng cút trong nước dùng xương thanh ngọt.","prep":30,"cook":90,"servings":4,"difficulty":3,"ingredients":[["Hủ tiếu khô","400","g"],["Tôm tươi","200","g"],["Thịt heo xay","200","g"],["Trứng cút","8","quả"],["Xương heo","500","g"],["Hành lá","3","nhánh"]],"steps":[["Nấu nước dùng","Chần xương, rửa sạch rồi ninh với khoảng 2 lít nước trong 60–75 phút; hớt bọt để nước trong."],["Chuẩn bị topping","Luộc trứng cút và bóc vỏ; luộc tôm vừa chín rồi bóc vỏ. Xào thịt bằm đến khi chín."],["Luộc hủ tiếu","Ngâm hoặc luộc hủ tiếu theo hướng dẫn trên bao bì, xả nhanh nước mát và để ráo."],["Dọn tô","Cho hủ tiếu vào tô, thêm tôm, thịt, trứng và hành lá, chan nước dùng nóng."]]},
      {"slug":"mi-ga-tiem","title":"Mì gà tiềm","category":"mon-nuoc","description":"Mì trứng ăn cùng đùi gà hầm mềm với nấm hương và gừng.","prep":20,"cook":75,"servings":4,"difficulty":2,"ingredients":[["Đùi gà","4","cái"],["Mì trứng","400","g"],["Nấm hương khô","30","g"],["Gừng","1","nhánh"],["Nước tương","2","muỗng canh"],["Cải thìa","200","g"]],"steps":[["Sơ chế","Ngâm nấm hương đến mềm, rửa sạch; rửa gà và thái gừng thành lát."],["Hầm gà","Cho gà, nấm, gừng, nước tương và khoảng 1,5 lít nước vào nồi, đun sôi rồi hầm nhỏ lửa đến khi gà mềm."],["Chuẩn bị mì và rau","Luộc mì theo hướng dẫn, chần cải thìa vừa chín rồi để ráo."],["Dọn món","Cho mì và cải thìa vào tô, đặt đùi gà lên trên rồi chan nước hầm nóng."]]},
      {"slug":"bun-cha-ca","title":"Bún chả cá","category":"mon-nuoc","description":"Bún cá với chả cá dai thơm, cà chua và nước dùng thanh nhẹ.","prep":25,"cook":35,"servings":4,"difficulty":2,"ingredients":[["Chả cá","400","g"],["Bún tươi","800","g"],["Cà chua","3","quả"],["Thơm","150","g"],["Thì là","1","nắm"],["Nước dùng cá","1.5","lít"]],"steps":[["Sơ chế","Cắt chả cá thành miếng, bổ múi cà chua, thái thơm và rửa sạch thì là."],["Nấu nước dùng","Phi thơm hành, xào cà chua và thơm đến khi mềm, thêm nước dùng cá rồi đun sôi."],["Nấu chả cá","Cho chả cá vào nồi, nấu nhỏ lửa khoảng 10 phút và nêm nước mắm, muối vừa ăn."],["Dọn bún","Chần bún, xếp vào tô, chan nước dùng cùng chả cá và thêm thì là."]]},
      {"slug":"cha-ca-la-vong","title":"Chả cá Lã Vọng","category":"mon-viet","description":"Cá ướp nghệ và riềng, áp chảo với thì là và hành lá, dùng kèm bún.","prep":35,"cook":15,"servings":4,"difficulty":2,"ingredients":[["Cá lăng hoặc cá basa","600","g"],["Riềng","1","nhánh"],["Nghệ tươi","1","nhánh"],["Thì là","1","nắm"],["Hành lá","4","nhánh"],["Mắm tôm","1","muỗng canh"]],"steps":[["Sơ chế cá","Cắt cá thành miếng vuông, thấm khô; giã riềng và nghệ rồi lọc lấy nước cốt."],["Ướp cá","Trộn cá với nước riềng nghệ, mắm tôm, chút muối và tiêu, để thấm khoảng 30 phút."],["Áp chảo cá","Làm nóng chảo với ít dầu, áp chảo cá đến khi vàng và chín đều hai mặt."],["Xào rau thơm","Thêm hành lá và thì là cắt khúc, đảo nhanh cho vừa héo; dùng nóng với bún và mắm tôm pha."]]},
      {"slug":"thit-rim-man-ngot","title":"Thịt rim mặn ngọt","category":"mon-viet","description":"Thịt heo rim nước mắm và đường đến khi mềm, sánh vị và lên màu đẹp.","prep":15,"cook":30,"servings":4,"difficulty":1,"ingredients":[["Thịt nạc vai","500","g"],["Nước mắm","2","muỗng canh"],["Đường","1.5","muỗng canh"],["Hành tím","2","củ"],["Tỏi","2","tép"],["Tiêu xay","1/2","muỗng cà phê"]],"steps":[["Sơ chế và ướp","Thái thịt thành miếng vừa ăn, ướp với nước mắm, đường, hành tím và tiêu trong 15 phút."],["Xào săn thịt","Phi thơm tỏi, cho thịt vào đảo đến khi săn mặt."],["Rim thịt","Thêm khoảng 100 ml nước, đun sôi rồi rim nhỏ lửa đến khi thịt mềm và nước sốt sánh."],["Hoàn thành","Nêm lại cho vừa, rắc tiêu và dùng cùng cơm nóng."]]},
      {"slug":"ca-basa-chien-gion","title":"Cá basa chiên giòn","category":"mon-viet","description":"Cá basa tẩm bột chiên vàng giòn, dùng cùng rau sống và nước chấm chua ngọt.","prep":15,"cook":15,"servings":3,"difficulty":1,"ingredients":[["Phi lê cá basa","500","g"],["Bột chiên giòn","100","g"],["Trứng gà","1","quả"],["Dầu ăn","300","ml"],["Chanh","1/2","quả"],["Muối","1/2","muỗng cà phê"]],"steps":[["Sơ chế cá","Rửa cá nhanh, thấm khô và cắt miếng vừa ăn; nêm muối và vài giọt chanh."],["Chuẩn bị bột","Đánh tan trứng trong một bát; cho bột chiên giòn ra đĩa."],["Chiên cá","Nhúng cá qua trứng rồi lăn bột, chiên trong dầu nóng vừa đến khi vàng giòn và cá chín."],["Dọn món","Vớt cá ra giấy thấm dầu, dùng ngay với rau sống và nước chấm."]]},
      {"slug":"goi-cuon-tom-thit","title":"Gỏi cuốn tôm thịt","category":"mon-viet","description":"Bánh tráng cuốn tôm, thịt, bún và rau sống, chấm tương đậu phộng.","prep":30,"cook":20,"servings":4,"difficulty":2,"ingredients":[["Tôm","250","g"],["Thịt ba chỉ","250","g"],["Bánh tráng","12","cái"],["Bún tươi","300","g"],["Xà lách và hẹ","1","bó"],["Tương đậu phộng","150","g"]],"steps":[["Luộc phần nhân","Luộc thịt đến khi chín, để nguội rồi thái lát mỏng; luộc tôm chín, bóc vỏ và chẻ đôi."],["Chuẩn bị rau","Rửa sạch xà lách, hẹ và để ráo; chần bún nếu cần."],["Cuốn gỏi","Làm mềm bánh tráng với nước, xếp rau, bún, thịt và tôm rồi cuốn chặt tay."],["Pha chấm và dọn","Khuấy tương đậu phộng với chút nước ấm cho vừa độ sánh, dọn cùng gỏi cuốn."]]},
      {"slug":"mi-xao-hai-san","title":"Mì xào hải sản","category":"mon-a","description":"Mì trứng xào tôm, mực và rau cải với nước tương thơm đậm.","prep":20,"cook":15,"servings":3,"difficulty":2,"ingredients":[["Mì trứng","300","g"],["Tôm","150","g"],["Mực","150","g"],["Cải thìa","200","g"],["Nước tương","2","muỗng canh"],["Tỏi","3","tép"]],"steps":[["Sơ chế","Luộc mì vừa chín, xả nhanh nước mát; làm sạch tôm, mực và cắt rau cải."],["Xào hải sản","Phi thơm tỏi, xào tôm và mực trên lửa lớn đến khi vừa chín rồi gắp ra."],["Xào mì và rau","Xào cải thìa, thêm mì và nước tương, đảo đều cho nóng."],["Hoàn thành","Cho hải sản trở lại chảo, đảo nhanh, nêm vừa ăn và dùng nóng."]]},
      {"slug":"sushi-cuon-trung","title":"Sushi cuộn trứng","category":"mon-a","description":"Cơm sushi cuộn rong biển cùng trứng cuộn, dưa leo và cà rốt.","prep":30,"cook":20,"servings":3,"difficulty":2,"ingredients":[["Gạo sushi","300","g"],["Rong biển cuộn","3","lá"],["Trứng gà","3","quả"],["Dưa leo","1","quả"],["Cà rốt","1","củ"],["Giấm gạo","2","muỗng canh"]],"steps":[["Nấu cơm sushi","Vo gạo, nấu chín; trộn nhẹ với giấm gạo và để nguội bớt."],["Làm nhân","Đánh trứng, tráng thành lớp mỏng rồi cuộn lại; cắt trứng, dưa leo và cà rốt thành thanh dài."],["Cuộn sushi","Đặt rong biển lên mành cuốn, trải cơm mỏng, xếp nhân rồi cuộn chặt."],["Cắt và dọn","Dùng dao ướt cắt cuộn thành khoanh vừa ăn, dọn cùng nước tương."]]},
      {"slug":"mi-y-sot-bo-bam","title":"Mì Ý sốt bò bằm","category":"mon-au","description":"Mì spaghetti phủ sốt cà chua nấu cùng thịt bò bằm và thảo mộc.","prep":15,"cook":30,"servings":4,"difficulty":2,"ingredients":[["Mì spaghetti","300","g"],["Thịt bò xay","300","g"],["Cà chua nghiền","400","g"],["Hành tây","1/2","củ"],["Tỏi","2","tép"],["Dầu ô liu","1","muỗng canh"]],"steps":[["Luộc mì","Luộc spaghetti trong nước sôi có muối theo hướng dẫn trên bao bì, để ráo và giữ lại ít nước luộc."],["Xào thịt bò","Phi hành tây và tỏi băm với dầu ô liu, thêm thịt bò xay và đảo đến khi thịt chín."],["Nấu sốt","Thêm cà chua nghiền, nêm muối tiêu và đun nhỏ lửa khoảng 20 phút; thêm ít nước luộc mì nếu sốt đặc."],["Dọn món","Cho mì ra đĩa, rưới sốt bò bằm lên trên và dùng nóng."]]},
      {"slug":"salad-ca-ngu","title":"Salad cá ngừ","category":"mon-au","description":"Salad rau xanh trộn cá ngừ, cà chua và sốt dầu ô liu chanh.","prep":15,"cook":0,"servings":2,"difficulty":1,"ingredients":[["Cá ngừ đóng hộp","150","g"],["Xà lách","150","g"],["Cà chua bi","100","g"],["Dưa leo","1/2","quả"],["Dầu ô liu","1","muỗng canh"],["Chanh","1/2","quả"]],"steps":[["Sơ chế rau","Rửa xà lách, cà chua bi và dưa leo; để ráo rồi cắt vừa ăn."],["Pha sốt","Trộn dầu ô liu với nước cốt chanh, chút muối và tiêu."],["Trộn salad","Cho rau và cá ngừ đã để ráo vào tô, rưới sốt và trộn nhẹ tay."],["Dọn món","Bày salad ra đĩa và dùng ngay để rau giữ độ giòn."]]},
      {"slug":"dau-hu-non-sot-nam","title":"Đậu hũ non sốt nấm","category":"mon-chay","description":"Đậu hũ non mềm dùng cùng sốt nấm đông cô và nước tương chay.","prep":15,"cook":15,"servings":3,"difficulty":1,"ingredients":[["Đậu hũ non","400","g"],["Nấm đông cô","150","g"],["Nước tương","1","muỗng canh"],["Hành boa-rô","1","nhánh"],["Bột bắp","1","muỗng cà phê"],["Dầu ăn","1","muỗng canh"]],"steps":[["Sơ chế","Cắt đậu hũ thành miếng lớn; rửa nấm, thái lát và cắt nhỏ hành boa-rô."],["Nấu sốt nấm","Phi thơm boa-rô, cho nấm vào xào mềm rồi thêm nước tương và khoảng 100 ml nước."],["Làm sánh sốt","Hòa bột bắp với nước nguội, rót vào chảo và khuấy đến khi sốt hơi sánh."],["Hoàn thành","Cho đậu hũ vào chảo, om nhẹ vài phút để nóng đều, tránh đảo mạnh làm vỡ đậu."]]},
      {"slug":"nam-kho-dau-hu","title":"Nấm kho đậu hũ","category":"mon-chay","description":"Nấm và đậu hũ kho cùng nước tương, tiêu và hành boa-rô.","prep":15,"cook":25,"servings":4,"difficulty":1,"ingredients":[["Đậu hũ trắng","300","g"],["Nấm đùi gà","200","g"],["Nước tương","2","muỗng canh"],["Đường","1","muỗng cà phê"],["Hành boa-rô","1","nhánh"],["Tiêu xay","1/2","muỗng cà phê"]],"steps":[["Sơ chế","Cắt đậu hũ thành khối, áp chảo vàng nhẹ; thái nấm đùi gà thành lát."],["Xào nấm","Phi thơm boa-rô, cho nấm vào xào đến khi hơi mềm."],["Kho món","Cho đậu hũ, nước tương, đường và khoảng 150 ml nước vào chảo, đun sôi rồi hạ lửa."],["Hoàn thành","Kho đến khi nước sốt sánh và nấm chín, rắc tiêu rồi dùng với cơm."]]},
      {"slug":"xoi-ga-xe","title":"Xôi gà xé","category":"bua-sang","description":"Xôi nếp dẻo ăn cùng gà luộc xé sợi, hành phi và nước mắm.","prep":20,"cook":45,"servings":4,"difficulty":2,"ingredients":[["Gạo nếp","400","g"],["Thịt gà","300","g"],["Hành tím","3","củ"],["Nước mắm","1","muỗng canh"],["Dầu ăn","2","muỗng canh"],["Muối","1/2","muỗng cà phê"]],"steps":[["Ngâm nếp","Vo gạo nếp, ngâm ít nhất 4 giờ hoặc qua đêm, để ráo rồi trộn với muối."],["Hấp xôi","Hấp nếp trong xửng khoảng 30–40 phút, xới đều giữa chừng đến khi hạt xôi chín dẻo."],["Luộc và xé gà","Luộc gà đến khi chín, để nguội rồi xé sợi; nêm nhẹ với nước mắm."],["Dọn món","Phi hành tím thái lát đến vàng, rưới chút dầu hành lên xôi và dùng cùng gà xé."]]},
      {"slug":"banh-mi-trung-op-la","title":"Bánh mì trứng ốp la","category":"bua-sang","description":"Bánh mì giòn ăn với trứng ốp la, dưa leo và nước tương hoặc tương ớt.","prep":10,"cook":8,"servings":2,"difficulty":1,"ingredients":[["Bánh mì","2","ổ"],["Trứng gà","2","quả"],["Dưa leo","1","quả"],["Dầu ăn","1","muỗng canh"],["Nước tương","1","muỗng cà phê"],["Tương ớt","1","muỗng cà phê"]],"steps":[["Chuẩn bị","Làm nóng bánh mì, rửa sạch dưa leo rồi thái lát."],["Chiên trứng","Làm nóng chảo với dầu, đập trứng vào chiên lửa vừa đến khi lòng trắng chín theo ý thích."],["Nêm trứng","Rưới chút nước tương lên trứng, tắt bếp và lấy trứng ra đĩa."],["Dọn bữa sáng","Kẹp trứng và dưa leo vào bánh mì, thêm tương ớt nếu thích."]]},
      {"slug":"trung-cuon-rau-cu","title":"Trứng cuộn rau củ","category":"mon-nhanh","description":"Trứng cuộn mềm với cà rốt và hành lá, làm nhanh cho bữa ăn nhẹ.","prep":10,"cook":10,"servings":2,"difficulty":1,"ingredients":[["Trứng gà","4","quả"],["Cà rốt","50","g"],["Hành lá","2","nhánh"],["Sữa tươi","1","muỗng canh"],["Dầu ăn","1","muỗng cà phê"],["Muối","1","nhúm"]],"steps":[["Chuẩn bị hỗn hợp","Băm nhỏ cà rốt và hành lá; đánh trứng với sữa, muối và rau củ."],["Tráng lớp trứng đầu","Làm nóng chảo chống dính với ít dầu, đổ một lớp trứng mỏng và chiên lửa nhỏ đến khi mặt se."],["Cuộn trứng","Cuộn lớp trứng về một phía, đổ thêm trứng vào phần chảo trống và tiếp tục cuộn khi lớp mới vừa se."],["Hoàn thành","Chiên nhẹ các mặt cho trứng chín, để nguội bớt rồi cắt khoanh."]]},
      {"slug":"salad-rau-cu-tron","title":"Salad rau củ trộn","category":"mon-nhanh","description":"Salad rau củ tươi giòn trộn sốt chanh dầu ô liu đơn giản.","prep":15,"cook":0,"servings":2,"difficulty":1,"ingredients":[["Xà lách","150","g"],["Cà chua","1","quả"],["Dưa leo","1","quả"],["Cà rốt","1/2","củ"],["Dầu ô liu","1","muỗng canh"],["Chanh","1/2","quả"]],"steps":[["Rửa và cắt rau","Rửa sạch rau củ, để ráo; xé xà lách, thái dưa leo và cà chua, bào sợi cà rốt."],["Pha nước trộn","Khuấy dầu ô liu với nước cốt chanh, chút muối và tiêu."],["Trộn salad","Cho rau củ vào tô, rưới sốt và trộn nhẹ để rau không bị dập."],["Dọn món","Bày salad ra đĩa và dùng ngay khi rau còn tươi giòn."]]},
      {"slug":"banh-flan-ca-phe","title":"Bánh flan cà phê","category":"trang-mieng","description":"Bánh flan trứng sữa mềm mịn phủ lớp caramel cà phê thơm nhẹ.","prep":20,"cook":40,"servings":6,"difficulty":2,"ingredients":[["Trứng gà","4","quả"],["Sữa tươi","500","ml"],["Đường","120","g"],["Cà phê đen","50","ml"],["Nước","2","muỗng canh"],["Vani","1","muỗng cà phê"]],"steps":[["Thắng caramel","Đun 60 g đường với nước đến màu vàng cánh gián, thêm cà phê nóng rồi rót vào đáy khuôn."],["Pha hỗn hợp flan","Đun ấm sữa với phần đường còn lại; đánh nhẹ trứng và vani, từ từ rót sữa vào rồi lọc qua rây."],["Đổ khuôn và hấp","Rót hỗn hợp vào khuôn, đậy giấy bạc và hấp cách thủy lửa nhỏ khoảng 30–35 phút đến khi flan vừa đông."],["Làm lạnh và dùng","Để flan nguội, làm lạnh ít nhất 2 giờ rồi úp ra đĩa khi ăn."]]}
    ]$recipes$::jsonb) LOOP
        SELECT "Id" INTO category_id
        FROM "Categories"
        WHERE "Slug" = dish->>'category' AND NOT "IsDeleted";
        IF category_id IS NULL THEN
            RAISE EXCEPTION 'Missing recipe category: %', dish->>'category';
        END IF;
        IF EXISTS (SELECT 1 FROM "Recipes" WHERE "Slug" = dish->>'slug') THEN
            CONTINUE;
        END IF;

        recipe_id := md5('curated-additional-recipe:' || (dish->>'slug'))::uuid;
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

UPDATE "Recipes"
SET "Status" = 0, "UpdatedAt" = now()
WHERE "Slug" IN (
    'ca-phe-sua-da',
    'nuoc-ep-cam-ca-rot',
    'ca-hap-xi-dau',
    'bo-nuong-la-lot'
)
  AND NOT "IsDeleted"
  AND "Status" <> 0;

UPDATE "Recipes"
SET "Status" = 2, "UpdatedAt" = now()
WHERE "Slug" IN (
    'sinh-to-bo',
    'chao-hau',
    'ca-nuong-giay-bac',
    'salad-ca-ngu'
)
  AND NOT "IsDeleted"
  AND "Status" <> 2;

-- Use the closest available local dish photo for each curated recipe.
INSERT INTO "RecipeImages" (
    "Id", "RecipeId", "OriginalUrl", "MediumUrl", "ThumbnailUrl",
    "AltText", "IsPrimary", "OrderIndex", "CreatedAt"
)
SELECT md5('curated-additional-photo:' || recipe."Slug")::uuid,
       recipe."Id", photo.image_url, photo.image_url, photo.image_url,
       'Ảnh món ' || recipe."Title", true, 0, now()
FROM "Recipes" AS recipe
JOIN "Categories" AS category ON category."Id" = recipe."CategoryId"
CROSS JOIN LATERAL (
    SELECT CASE recipe."Slug"
        WHEN 'tom-rang-thit-ba-chi' THEN '/images/dishes/admin-com-chien-hai-san.jpg'
        WHEN 'muc-xao-sa-ot' THEN '/images/dishes/admin-com-chien-hai-san.jpg'
        WHEN 'ca-hap-xi-dau' THEN '/images/dishes/canh-chua-ca.webp'
        WHEN 'chao-hau' THEN '/images/dishes/admin-com-chien-hai-san.jpg'
        WHEN 'ga-nuong-mat-ong' THEN '/images/dishes/ga-hap-la-chanh.webp'
        WHEN 'ga-xao-sa-ot' THEN '/images/dishes/admin-ga-kho-gung.jpg'
        WHEN 'ga-sot-cam' THEN '/images/dishes/ga-hap-la-chanh.webp'
        WHEN 'canh-ga-chien-nuoc-mam' THEN '/images/dishes/ga-hap-la-chanh.webp'
        WHEN 'suon-nuong-mat-ong' THEN '/images/dishes/bun-cha-ha-noi.webp'
        WHEN 'bo-nuong-la-lot' THEN '/images/dishes/bo-luc-lac-khoai-tay.webp'
        WHEN 'ca-nuong-giay-bac' THEN '/images/dishes/ca-kho-to-truyen-thong.webp'
        WHEN 'banh-trang-nuong' THEN '/images/dishes/bun-cha-ha-noi.webp'
        WHEN 'bun-thit-nuong' THEN '/images/dishes/bun-cha-ha-noi.webp'
        WHEN 'hu-tieu-nam-vang' THEN '/images/dishes/bun-rieu-cua-ca-chua.webp'
        WHEN 'mi-ga-tiem' THEN '/images/dishes/bun-bo-hue-gia-dinh.webp'
        WHEN 'bun-cha-ca' THEN '/images/dishes/bun-bo-hue-gia-dinh.webp'
        WHEN 'cha-ca-la-vong' THEN '/images/dishes/ca-kho-to-truyen-thong.webp'
        WHEN 'thit-rim-man-ngot' THEN '/images/dishes/thit-kho-tau-trung-vit.webp'
        WHEN 'ca-basa-chien-gion' THEN '/images/dishes/ca-kho-to-truyen-thong.webp'
        WHEN 'goi-cuon-tom-thit' THEN '/images/dishes/banh-beo-chen-hue.webp'
        WHEN 'mi-xao-hai-san' THEN '/images/dishes/admin-com-chien-hai-san.jpg'
        WHEN 'sushi-cuon-trung' THEN '/images/dishes/trung-chien-ca-chua.webp'
        WHEN 'mi-y-sot-bo-bam' THEN '/images/dishes/nui-xao-bo-rau-cu.webp'
        WHEN 'salad-ca-ngu' THEN '/images/dishes/canh-chua-ca.webp'
        WHEN 'dau-hu-non-sot-nam' THEN '/images/dishes/admin-dau-hu-sot-ca-chua.jpg'
        WHEN 'nam-kho-dau-hu' THEN '/images/dishes/admin-dau-hu-sot-ca-chua.jpg'
        WHEN 'xoi-ga-xe' THEN '/images/dishes/ga-hap-la-chanh.webp'
        WHEN 'banh-mi-trung-op-la' THEN '/images/dishes/trung-chien-ca-chua.webp'
        WHEN 'trung-cuon-rau-cu' THEN '/images/dishes/com-chien-trung.webp'
        WHEN 'salad-rau-cu-tron' THEN '/images/dishes/rau-muong-xao-toi.webp'
        WHEN 'banh-flan-ca-phe' THEN '/images/dishes/admin-che-ba-mau.jpg'
        ELSE category."ImageUrl"
    END AS image_url
) AS photo
WHERE recipe."Slug" = ANY (ARRAY[
    'ca-phe-sua-da','tra-chanh-sa','sinh-to-bo','nuoc-ep-cam-ca-rot',
    'tom-rang-thit-ba-chi','muc-xao-sa-ot','ca-hap-xi-dau','chao-hau',
    'ga-nuong-mat-ong','ga-xao-sa-ot','ga-sot-cam','canh-ga-chien-nuoc-mam',
    'suon-nuong-mat-ong','bo-nuong-la-lot','ca-nuong-giay-bac','banh-trang-nuong',
    'bun-thit-nuong','hu-tieu-nam-vang','mi-ga-tiem','bun-cha-ca',
    'cha-ca-la-vong','thit-rim-man-ngot','ca-basa-chien-gion','goi-cuon-tom-thit',
    'mi-xao-hai-san','sushi-cuon-trung','mi-y-sot-bo-bam','salad-ca-ngu',
    'dau-hu-non-sot-nam','nam-kho-dau-hu','xoi-ga-xe','banh-mi-trung-op-la',
    'trung-cuon-rau-cu','salad-rau-cu-tron','banh-flan-ca-phe'
])
AND NOT EXISTS (
    SELECT 1
    FROM "RecipeImages" AS existing_image
    WHERE existing_image."RecipeId" = recipe."Id"
      AND existing_image."Id" <> md5('curated-additional-photo:' || recipe."Slug")::uuid
)
ON CONFLICT ("Id") DO UPDATE SET
    "OriginalUrl" = EXCLUDED."OriginalUrl",
    "MediumUrl" = EXCLUDED."MediumUrl",
    "ThumbnailUrl" = EXCLUDED."ThumbnailUrl",
    "AltText" = EXCLUDED."AltText";

DO $verify$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM "Recipes" AS recipe
        WHERE recipe."Slug" = ANY (ARRAY[
            'ca-phe-sua-da','tra-chanh-sa','sinh-to-bo','nuoc-ep-cam-ca-rot',
            'tom-rang-thit-ba-chi','muc-xao-sa-ot','ca-hap-xi-dau','chao-hau',
            'ga-nuong-mat-ong','ga-xao-sa-ot','ga-sot-cam','canh-ga-chien-nuoc-mam',
            'suon-nuong-mat-ong','bo-nuong-la-lot','ca-nuong-giay-bac','banh-trang-nuong',
            'bun-thit-nuong','hu-tieu-nam-vang','mi-ga-tiem','bun-cha-ca',
            'cha-ca-la-vong','thit-rim-man-ngot','ca-basa-chien-gion','goi-cuon-tom-thit',
            'mi-xao-hai-san','sushi-cuon-trung','mi-y-sot-bo-bam','salad-ca-ngu',
            'dau-hu-non-sot-nam','nam-kho-dau-hu','xoi-ga-xe','banh-mi-trung-op-la',
            'trung-cuon-rau-cu','salad-rau-cu-tron','banh-flan-ca-phe'
        ])
          AND (
              NOT EXISTS (
                  SELECT 1 FROM "RecipeIngredients" AS ingredient
                  WHERE ingredient."RecipeId" = recipe."Id"
              )
              OR NOT EXISTS (
                  SELECT 1 FROM "RecipeSteps" AS cooking_step
                  WHERE cooking_step."RecipeId" = recipe."Id"
              )
              OR NOT EXISTS (
                  SELECT 1 FROM "RecipeImages" AS image
                  WHERE image."RecipeId" = recipe."Id"
              )
          )
    ) THEN
        RAISE EXCEPTION 'A curated additional recipe is missing ingredients, steps, or an image';
    END IF;
END;
$verify$;
COMMIT;
