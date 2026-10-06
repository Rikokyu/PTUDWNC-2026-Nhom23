# Recipe ingredients

Ingredients là child resources của recipe. Các endpoint yêu cầu role Author
hoặc Admin và tiền tố `/api/v1/recipes/{id}/ingredients`.

| Method | Route | Thành công |
|---|---|---|
| POST | `/` | `201` ingredient mới |
| PUT | `/{ingredientId}` | `200` ingredient đã cập nhật |
| DELETE | `/{ingredientId}` | `204` |

POST/PUT nhận JSON theo cấu trúc:

```json
{
  "name": "Flour",
  "quantity": "250",
  "unit": "g",
  "notes": null,
  "orderIndex": 0
}
```

`quantity`, `unit`, `notes`, `orderIndex` có thể để null. Các route cần Bearer
token; `id` và `ingredientId` là GUID.