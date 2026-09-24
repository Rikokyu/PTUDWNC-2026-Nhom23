"use client";

export default function GlobalError({ reset }: { reset: () => void }) {
  return (
    <main>
      <h1>Da xay ra loi</h1>
      <button type="button" onClick={reset}>
        Thu lai
      </button>
    </main>
  );
}
