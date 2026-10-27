"use client";
import { FormEvent, useEffect, useState } from "react";
import Link from "next/link";
import Image from "next/image";
import { apiRequest } from "@/lib/api/client";
import { useAuth } from "@/components/auth/AuthProvider";
import type { AuthUserDto } from "@/types/api";

type Profile = AuthUserDto & { bio: string | null; avatarUrl: string | null };
export function ProfileForm() {
  const { updateUser } = useAuth();
  const [profile, setProfile] = useState<Profile>();
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [busy, setBusy] = useState(false);
  const [revision, setRevision] = useState(0);
  useEffect(() => {
    let active = true;
    apiRequest<Profile>({ url: "/api/v1/auth/me" }).then(data => { if (active) { setProfile(data); setError(""); } })
      .catch(ex => { if (active) setError(ex instanceof Error ? ex.message : "Không thể tải hồ sơ."); });
    return () => { active = false; };
  }, [revision]);
  async function uploadAvatar(file?: File) {
    if (!file) return;
    setBusy(true); setError(""); setSuccess("");
    try {
      const data = new FormData(); data.append("file", file);
      const updated = await apiRequest<Profile>({ method: "POST", url: "/api/v1/auth/me/avatar", data, headers: { "Content-Type": "multipart/form-data" } });
      setProfile(updated);
      updateUser(updated);
      setSuccess("Đã cập nhật ảnh đại diện.");
    } catch (ex) { setError(ex instanceof Error ? ex.message : "Không thể tải ảnh."); }
    finally { setBusy(false); }
  }
  async function save(event: FormEvent) {
    event.preventDefault(); if (!profile) return; setBusy(true); setError(""); setSuccess("");
    try {
      const updated = await apiRequest<Profile>({ method: "PATCH", url: "/api/v1/auth/me", data: {
        displayName: profile.displayName, bio: profile.bio, avatarUrl: profile.avatarUrl,
      } });
      setProfile(updated);
      updateUser(updated);
      setSuccess("Đã lưu hồ sơ.");
    } catch (ex) { setError(ex instanceof Error ? ex.message : "Không thể cập nhật hồ sơ."); }
    finally { setBusy(false); }
  }
  return <section className="section-block max-w-2xl space-y-4">
    <h1>Hồ sơ cá nhân</h1>
    {error && <div role="alert"><p className="text-red-700">{error}</p><button onClick={() => setRevision(x => x + 1)}>Tải lại hồ sơ</button></div>}
    {success && <p role="status">{success}</p>}
    {!profile && !error && <p role="status">Đang tải hồ sơ…</p>}
    {profile && <form className="stack space-y-4" onSubmit={save}>
      {profile.avatarUrl && <Image unoptimized src={profile.avatarUrl} alt="Ảnh đại diện" width={96} height={96} className="rounded-full object-cover" />}
      <label className="block">Tải ảnh đại diện<input aria-label="Tải ảnh đại diện" type="file" accept="image/jpeg,image/png,image/webp,image/avif" disabled={busy} onChange={e => void uploadAvatar(e.target.files?.[0])} /></label>
      <p>{profile.email} · {profile.role}</p>
      <label className="block">Họ tên<input className="w-full rounded-lg border p-3" required maxLength={100} autoComplete="name" value={profile.displayName} onChange={e => setProfile({ ...profile, displayName: e.target.value })} /></label>
      <label className="block">Giới thiệu<textarea className="w-full rounded-lg border p-3" maxLength={1000} rows={4} value={profile.bio ?? ""} onChange={e => setProfile({ ...profile, bio: e.target.value })} /></label>
      <label className="block">Địa chỉ ảnh đại diện<input className="w-full rounded-lg border p-3" inputMode="url" maxLength={500} value={profile.avatarUrl ?? ""} onChange={e => setProfile({ ...profile, avatarUrl: e.target.value })} /></label>
      <button className="button button-primary" disabled={busy}>{busy ? "Đang lưu…" : "Lưu hồ sơ"}</button>
    </form>}
    <p><Link href="/change-password">Đổi mật khẩu</Link></p>
  </section>;
}
