"use client";
import { FormEvent, Suspense, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { apiRequest } from "@/lib/api/client";
import { clearAuthSession } from "@/lib/auth/session";

export function PasswordForm({ mode }: { mode: "forgot" | "reset" | "change" }) {
  return <Suspense fallback={<p>Đang tải…</p>}><Form mode={mode} /></Suspense>;
}
function Form({ mode }: { mode: "forgot" | "reset" | "change" }) {
  const params = useSearchParams();
  const [email, setEmail] = useState(params.get("email") ?? "");
  const [currentPassword, setCurrent] = useState("");
  const [newPassword, setPassword] = useState("");
  const [confirmPassword, setConfirmation] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  async function submit(event: FormEvent) {
    event.preventDefault(); setError(""); setMessage("");
    if (mode !== "forgot" && newPassword !== confirmPassword) { setError("Mật khẩu xác nhận không khớp."); return; }
    if (mode === "reset" && !params.get("token")) { setError("Liên kết không hợp lệ. Hãy yêu cầu liên kết mới."); return; }
    setBusy(true);
    try {
      await apiRequest({ method: "POST", url: `/api/v1/auth/${mode === "change" ? "change-password" : mode === "reset" ? "reset-password" : "forgot-password"}`,
        data: mode === "forgot" ? { email } : { email, token: params.get("token"), currentPassword, newPassword, confirmPassword } });
      setCurrent(""); setPassword(""); setConfirmation("");
      if (mode === "forgot") setMessage("Nếu tài khoản tồn tại, liên kết đặt lại mật khẩu sẽ được gửi qua email.");
      else {
        setMessage("Mật khẩu đã cập nhật. Vui lòng đăng nhập lại.");
        if (mode === "change") { clearAuthSession(); window.location.assign("/login?passwordChanged=1"); }
      }
    } catch (ex) { setError(ex instanceof Error ? ex.message : "Không thể xử lý yêu cầu."); }
    finally { setBusy(false); }
  }
  const control = "w-full rounded-lg border border-slate-300 p-3";
  return <form onSubmit={submit} className="stack max-w-lg space-y-4">
    {error && <p role="alert" className="text-red-700">{error}</p>}
    {message && <p role="status">{message}</p>}
    {!(message && mode !== "forgot") && <>
      {mode !== "change" && <label className="block">Email<input className={control} type="email" autoComplete="email" required maxLength={320} value={email} onChange={e => setEmail(e.target.value)} /></label>}
      {mode === "change" && <label className="block">Mật khẩu hiện tại<input className={control} type="password" autoComplete="current-password" required maxLength={128} value={currentPassword} onChange={e => setCurrent(e.target.value)} /></label>}
      {mode !== "forgot" && <>
        <p className="muted">Mật khẩu 8–128 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt.</p>
        <label className="block">Mật khẩu mới<input className={control} type="password" autoComplete="new-password" required minLength={8} maxLength={128} pattern="(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).*" value={newPassword} onChange={e => setPassword(e.target.value)} /></label>
        <label className="block">Xác nhận mật khẩu mới<input className={control} type="password" autoComplete="new-password" required minLength={8} maxLength={128} value={confirmPassword} onChange={e => setConfirmation(e.target.value)} /></label>
      </>}
      <button className="button button-primary" disabled={busy}>{busy ? "Đang xử lý…" : mode === "forgot" ? "Gửi liên kết đặt lại" : "Cập nhật mật khẩu"}</button>
    </>}
    <p><Link href="/login">Đăng nhập</Link> · <Link href="/forgot-password">Yêu cầu liên kết mới</Link></p>
  </form>;
}
