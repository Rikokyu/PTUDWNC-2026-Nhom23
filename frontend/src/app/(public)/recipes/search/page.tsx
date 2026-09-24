"use client";

import { useState } from "react";

export default function RecipeSearchPage() {
  const [query, setQuery] = useState("");

  return (
    <main>
      <h1>Tim kiem cong thuc</h1>
      <input
        value={query}
        onChange={(event) => setQuery(event.target.value)}
        placeholder="Nhap tu khoa"
      />
      <p>Tu khoa: {query}</p>
    </main>
  );
}
