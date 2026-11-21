"use client";

import { useEffect, useRef, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { clearGoogleSession } from "@/app/auth/google/actions";
import { authApi, saveAuthSession } from "@/lib/api/auth";
import { getApiErrorMessage } from "@/lib/api/error";

export function GoogleSignInComplete({ idToken, next }: { idToken: string | null; next: string }) {
  const router = useRouter();
  const [error, setError] = useState("");
  const exchangeStarted = useRef(false);

  useEffect(() => {
    if (!idToken) {
      setError("Không nhận được thông tin xác thực từ Google. Vui lòng thử lại.");
      return;
    }
    if (exchangeStarted.current) return;
    exchangeStarted.current = true;
    (async () => {
      try {
        const session = await authApi.google(idToken);
        saveAuthSession(session);
        try { await clearGoogleSession(); } catch { /* The API session is already active. */ }
        router.replace(next);
      } catch (cause) {
        setError(getApiErrorMessage(cause));
      }
    })();
  }, [idToken, next, router]);

  return (
    <main className="mx-auto max-w-md py-16 text-center">
      <h1 className="font-display text-2xl font-bold text-content-primary">Đăng nhập với Google</h1>
      {error ? (
        <div role="alert" className="mt-5 space-y-4">
          <p className="text-sm text-rose-700">{error}</p>
          <Link href="/auth/login" className="font-semibold text-brand hover:underline">Quay lại đăng nhập</Link>
        </div>
      ) : <p role="status" className="mt-4 text-sm text-content-secondary">Đang xác minh tài khoản…</p>}
    </main>
  );
}
