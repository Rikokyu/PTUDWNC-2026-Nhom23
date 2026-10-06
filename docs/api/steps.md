# Recipe steps

Steps là child resources của recipe. Các endpoint yêu cầu role Author hoặc
Admin và tiền tố `/api/v1/recipes/{id}/steps`.

| Method | Route | Thành công |
|---|---|---|
| POST | `/` | `201` step mới |
| PUT | `/{stepId}` | `200` step đã cập nhật |
| DELETE | `/{stepId}` | `204` |

POST/PUT nhận JSON theo cấu trúc:

```json
{
  "title": "Prepare ingredients",
  "description": "Wash and chop the vegetables.",
  "timerMinutes": 5,
  "imageUrl": null
}
```

`timerMinutes` và `imageUrl` có thể để null. Step number được quản lý bởi
backend. Các route cần Bearer token; `id` và `stepId` là GUID.