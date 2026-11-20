import credits from "@/data/commons-photo-credits.json";

export default function ImageCreditsPage() {
  return (
    <main className="space-y-8 pb-12">
      <header className="space-y-3">
        <p className="text-xs font-bold uppercase tracking-[0.2em] text-[#92714f]">
          Nguồn hình ảnh
        </p>
        <h1 className="font-display text-4xl font-bold text-[#202b22]">
          Ghi công hình ảnh
        </h1>
        <p className="max-w-3xl leading-7 text-stone-600">
          Ảnh món ăn và danh mục được tải từ Wikimedia Commons. Mỗi ảnh được
          dùng một lần; giấy phép và thông tin tác giả được ghi bên dưới.
        </p>
      </header>

      <ul className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
        {credits.map((credit) => (
          <li
            key={`${credit.kind}:${credit.slug}`}
            className="overflow-hidden rounded-2xl border border-stone-200 bg-white shadow-sm"
          >
            <img
              src={credit.localPath}
              alt={`Ảnh ${credit.kind === "category" ? "danh mục" : "món"} ${credit.label}`}
              className="aspect-[4/3] w-full object-cover"
              loading="lazy"
              decoding="async"
            />
            <div className="space-y-2 p-4 text-sm">
              <h2 className="font-semibold text-stone-900">{credit.label}</h2>
              <p className="text-stone-600">
                Tác giả: {credit.artist || "Không ghi tên"}
              </p>
              <p className="text-stone-600">
                Tệp:{" "}
                <a
                  href={credit.source}
                  target="_blank"
                  rel="noreferrer"
                  className="underline underline-offset-2 hover:text-[#8b5e3c]"
                >
                  {credit.commonsTitle}
                </a>
              </p>
              <p className="text-stone-600">
                Giấy phép:{" "}
                {credit.licenseUrl ? (
                  <a
                    href={credit.licenseUrl}
                    target="_blank"
                    rel="noreferrer"
                    className="underline underline-offset-2 hover:text-[#8b5e3c]"
                  >
                    {credit.license}
                  </a>
                ) : (
                  credit.license
                )}
              </p>
            </div>
          </li>
        ))}
      </ul>
    </main>
  );
}
