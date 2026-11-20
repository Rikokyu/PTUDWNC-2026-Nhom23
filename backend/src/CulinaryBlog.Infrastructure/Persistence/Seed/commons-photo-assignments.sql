BEGIN;

-- Update only the deterministic seed images; leave any user-uploaded photos intact.
WITH photo_paths("Slug","ImagePath") AS (VALUES
  ('banh-beo-chen-hue','/images/dishes/commons/recipes/banh-beo-chen-hue.jpg'),
  ('banh-cuon-nong-nhan-thit','/images/dishes/commons/recipes/banh-cuon-nong-nhan-thit.jpg'),
  ('banh-flan-ca-phe','/images/dishes/commons/recipes/banh-flan-ca-phe.jpg'),
  ('banh-mi-trung-op-la','/images/dishes/commons/recipes/banh-mi-trung-op-la.jpg'),
  ('banh-xeo-tom-thit','/images/dishes/commons/recipes/banh-xeo-tom-thit.jpg'),
  ('bo-luc-lac-khoai-tay','/images/dishes/commons/recipes/bo-luc-lac-khoai-tay.jpg'),
  ('bun-bo-hue-gia-dinh','/images/dishes/commons/recipes/bun-bo-hue-gia-dinh.jpg'),
  ('bun-cha-ca','/images/dishes/commons/recipes/bun-cha-ca.jpg'),
  ('bun-cha-ha-noi','/images/dishes/commons/recipes/bun-cha-ha-noi.jpg'),
  ('bun-rieu-cua-ca-chua','/images/dishes/commons/recipes/bun-rieu-cua-ca-chua.jpg'),
  ('bun-thit-nuong','/images/dishes/commons/recipes/bun-thit-nuong.jpg'),
  ('ca-basa-chien-gion','/images/dishes/commons/recipes/ca-basa-chien-gion.jpg'),
  ('ca-hap-xi-dau','/images/dishes/commons/recipes/ca-hap-xi-dau.jpg'),
  ('ca-kho-to-truyen-thong','/images/dishes/commons/recipes/ca-kho-to-truyen-thong.jpg'),
  ('ca-nuong-giay-bac','/images/dishes/commons/recipes/ca-nuong-giay-bac.jpg'),
  ('ca-phe-sua-da','/images/dishes/commons/recipes/ca-phe-sua-da.jpg'),
  ('canh-bi-do-thit-bam','/images/dishes/commons/recipes/canh-bi-do-thit-bam.jpg'),
  ('canh-chua-ca','/images/dishes/commons/recipes/canh-chua-ca.jpg'),
  ('canh-ga-chien-nuoc-mam','/images/dishes/commons/recipes/canh-ga-chien-nuoc-mam.jpg'),
  ('cha-ca-la-vong','/images/dishes/commons/recipes/cha-ca-la-vong.jpg'),
  ('chao-ga-hanh-gung','/images/dishes/commons/recipes/chao-ga-hanh-gung.jpg'),
  ('chao-hau','/images/dishes/commons/recipes/chao-hau.jpg'),
  ('che-ba-mau','/images/dishes/commons/recipes/che-ba-mau.jpg'),
  ('che-bap-nuoc-cot-dua','/images/dishes/commons/recipes/che-bap-nuoc-cot-dua.jpg'),
  ('che-dau-xanh-nuoc-cot-dua','/images/dishes/commons/recipes/che-dau-xanh-nuoc-cot-dua.jpg'),
  ('com-chien-duong-chau','/images/dishes/commons/recipes/com-chien-duong-chau.jpg'),
  ('com-chien-trung','/images/dishes/commons/recipes/com-chien-trung.jpg'),
  ('dau-hu-non-sot-nam','/images/dishes/commons/recipes/dau-hu-non-sot-nam.jpg'),
  ('dau-hu-sot-ca-chua','/images/dishes/commons/recipes/dau-hu-sot-ca-chua.jpg'),
  ('ga-hap-la-chanh','/images/dishes/commons/recipes/ga-hap-la-chanh.jpg'),
  ('ga-kho-gung','/images/dishes/commons/recipes/ga-kho-gung.jpg'),
  ('ga-nuong-mat-ong','/images/dishes/commons/recipes/ga-nuong-mat-ong.jpg'),
  ('ga-sot-cam','/images/dishes/commons/recipes/ga-sot-cam.jpg'),
  ('ga-xao-sa-ot','/images/dishes/commons/recipes/ga-xao-sa-ot.jpg'),
  ('goi-cuon-tom-thit','/images/dishes/commons/recipes/goi-cuon-tom-thit.jpg'),
  ('hu-tieu-nam-vang','/images/dishes/commons/recipes/hu-tieu-nam-vang.jpg'),
  ('mi-ga-tiem','/images/dishes/commons/recipes/mi-ga-tiem.jpg'),
  ('mi-quang-tom-thit','/images/dishes/commons/recipes/mi-quang-tom-thit.jpg'),
  ('mi-xao-hai-san','/images/dishes/commons/recipes/mi-xao-hai-san.jpg'),
  ('mi-y-sot-bo-bam','/images/dishes/commons/recipes/mi-y-sot-bo-bam.jpg'),
  ('muc-xao-sa-ot','/images/dishes/commons/recipes/muc-xao-sa-ot.jpg'),
  ('nam-kho-dau-hu','/images/dishes/commons/recipes/nam-kho-dau-hu.jpg'),
  ('nui-xao-bo-rau-cu','/images/dishes/commons/recipes/nui-xao-bo-rau-cu.jpg'),
  ('nuoc-ep-cam-ca-rot','/images/dishes/commons/recipes/nuoc-ep-cam-ca-rot.jpg'),
  ('pho-bo-tai-nam','/images/dishes/commons/recipes/pho-bo-tai-nam.jpg'),
  ('rau-muong-xao-toi','/images/dishes/commons/recipes/rau-muong-xao-toi.jpg'),
  ('salad-ca-ngu','/images/dishes/commons/recipes/salad-ca-ngu.jpg'),
  ('salad-rau-cu-tron','/images/dishes/commons/recipes/salad-rau-cu-tron.jpg'),
  ('sinh-to-bo','/images/dishes/commons/recipes/sinh-to-bo.jpg'),
  ('suon-nuong-mat-ong','/images/dishes/commons/recipes/suon-nuong-mat-ong.jpg'),
  ('sushi-cuon-trung','/images/dishes/commons/recipes/sushi-cuon-trung.jpg'),
  ('thit-kho-tau-trung-vit','/images/dishes/commons/recipes/thit-kho-tau-trung-vit.jpg'),
  ('thit-rim-man-ngot','/images/dishes/commons/recipes/thit-rim-man-ngot.jpg'),
  ('tom-rang-thit-ba-chi','/images/dishes/commons/recipes/tom-rang-thit-ba-chi.jpg'),
  ('tra-chanh-sa','/images/dishes/commons/recipes/tra-chanh-sa.jpg'),
  ('trung-chien-ca-chua','/images/dishes/commons/recipes/trung-chien-ca-chua.jpg'),
  ('trung-cuon-rau-cu','/images/dishes/commons/recipes/trung-cuon-rau-cu.jpg'),
  ('xoi-ga-xe','/images/dishes/commons/recipes/xoi-ga-xe.jpg'),
  ('banh-trang-nuong','/images/dishes/commons/recipes/banh-trang-nuong.jpg'),
  ('bo-nuong-la-lot','/images/dishes/commons/recipes/bo-nuong-la-lot.jpg')
)
UPDATE "RecipeImages" AS image
SET "OriginalUrl"=photo."ImagePath",
    "MediumUrl"=photo."ImagePath",
    "ThumbnailUrl"=photo."ImagePath",
    "AltText"='?nh minh h?a m?n '||recipe."Title",
    "IsPrimary"=NOT EXISTS (SELECT 1 FROM "RecipeImages" AS current_image WHERE current_image."RecipeId"=recipe."Id" AND current_image."Id"<>image."Id" AND current_image."IsPrimary")
FROM photo_paths AS photo
JOIN "Recipes" AS recipe ON recipe."Slug"=photo."Slug" AND NOT recipe."IsDeleted"
WHERE image."RecipeId"=recipe."Id"
  AND image."Id" IN (md5('culinary-dish-photo:'||recipe."Slug")::uuid, md5('curated-author-photo:'||recipe."Slug")::uuid, md5('curated-additional-photo:'||recipe."Slug")::uuid);

WITH photo_paths("Slug","ImagePath") AS (VALUES
  ('mon-viet','/images/dishes/commons/categories/mon-viet.jpg'),
  ('mon-a','/images/dishes/commons/categories/mon-a.jpg'),
  ('mon-au','/images/dishes/commons/categories/mon-au.jpg'),
  ('mon-chay','/images/dishes/commons/categories/mon-chay.jpg'),
  ('trang-mieng','/images/dishes/commons/categories/trang-mieng.jpg'),
  ('do-uong','/images/dishes/commons/categories/do-uong.jpg'),
  ('bua-sang','/images/dishes/commons/categories/bua-sang.jpg'),
  ('mon-nhanh','/images/dishes/commons/categories/mon-nhanh.jpg'),
  ('mon-nuong','/images/dishes/commons/categories/mon-nuong.jpg'),
  ('hai-san','/images/dishes/commons/categories/hai-san.jpg'),
  ('mon-ga','/images/dishes/commons/categories/mon-ga.jpg'),
  ('mon-nuoc','/images/dishes/commons/categories/mon-nuoc.jpg')
)
UPDATE "Categories" AS category
SET "ImageUrl"=photo."ImagePath", "UpdatedAt"=now()
FROM photo_paths AS photo
WHERE category."Slug"=photo."Slug" AND NOT category."IsDeleted";

COMMIT;
