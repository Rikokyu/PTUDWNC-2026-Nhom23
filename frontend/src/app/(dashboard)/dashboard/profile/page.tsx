"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Button } from "@/components/ui/Button";
import { ErrorState } from "@/components/ui/ErrorState";
import { Input } from "@/components/ui/Input";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { authApi } from "@/lib/api/auth";
import { getApiErrorMessage } from "@/lib/api/error";
import { useAuthStore } from "@/store/useAuthStore";

export default function ProfilePage() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const accessToken = useAuthStore((state) => state.accessToken);
  const user = useAuthStore((state) => state.user);
  const setSession = useAuthStore((state) => state.setSession);
  const [displayName, setDisplayName] = useState("");
  const [avatarUrl, setAvatarUrl] = useState("");
  const [bio, setBio] = useState("");
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [profileMessage, setProfileMessage] = useState("");
  const [passwordMessage, setPasswordMessage] = useState("");
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  const profileQuery = useQuery({
    queryKey: ["auth", "me"],
    queryFn: authApi.getMe,
    enabled: Boolean(accessToken),
  });

  useEffect(() => {
    if (!accessToken) router.replace("/auth/login");
  }, [accessToken, router]);

  useEffect(() => {
    if (!profileQuery.data) return;
    setDisplayName(profileQuery.data.displayName);
    setAvatarUrl(profileQuery.data.avatarUrl ?? "");
    setBio(profileQuery.data.bio ?? "");
  }, [profileQuery.data]);

  async function saveProfile(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setProfileMessage("");
    setSaving(true);
    try {
      const profile = await authApi.updateProfile({
        displayName: displayName.trim(),
        avatarUrl: avatarUrl.trim() || null,
        bio: bio.trim() || null,
      });
      if (accessToken && user) {
        setSession(accessToken, { ...user, displayName: profile.displayName });
      }
      await queryClient.setQueryData(["auth", "me"], profile);
      setProfileMessage("Đã cập nhật hồ sơ.");
    } catch (requestError) {
      setError(getApiErrorMessage(requestError));
    } finally {
      setSaving(false);
    }
  }

  async function changePassword(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setPasswordMessage("");
    if (newPassword !== confirmPassword) {
      setError("Mật khẩu xác nhận không khớp.");
      return;
    }
    setSaving(true);
    try {
      await authApi.changePassword(currentPassword, newPassword);
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
      setPasswordMessage("Đã đổi mật khẩu. Các refresh token hiện tại đã bị thu hồi.");
    } catch (requestError) {
      setError(getApiErrorMessage(requestError));
    } finally {
      setSaving(false);
    }
  }

  if (!accessToken) {
    return <p className="py-12 text-center text-sm">Đang chuyển đến trang đăng nhập...</p>;
  }
  if (profileQuery.isLoading) return <RecipeCardSkeleton />;
  if (profileQuery.isError) {
    return <ErrorState onRetry={() => void profileQuery.refetch()} />;
  }
  if (!profileQuery.data) {
    return <ErrorState message="Máy chủ không trả về hồ sơ người dùng." />;
  }

  return (
    <main className="mx-auto max-w-3xl space-y-8">
      <header>
        <h1 className="text-2xl font-bold text-content-primary">Hồ sơ cá nhân</h1>
        <p className="mt-1 text-sm text-content-secondary">
          Cập nhật thông tin hiển thị và bảo vệ tài khoản của bạn.
        </p>
      </header>
      {error && <ErrorState title="Không thể lưu thay đổi" message={error} />}

      <section className="rounded-xl border border-slate-200 bg-white p-5 sm:p-6">
        <h2 className="text-lg font-semibold">Thông tin cá nhân</h2>
        <p className="mt-1 text-sm text-content-secondary">{profileQuery.data.email}</p>
        <form onSubmit={saveProfile} className="mt-5 space-y-4">
          <Input
            label="Tên hiển thị"
            value={displayName}
            minLength={2}
            maxLength={100}
            onChange={(event) => setDisplayName(event.target.value)}
            required
          />
          <Input
            label="URL ảnh đại diện"
            type="url"
            value={avatarUrl}
            onChange={(event) => setAvatarUrl(event.target.value)}
            placeholder="https://..."
          />
          <label className="block space-y-1.5 text-xs font-semibold uppercase tracking-wider text-content-secondary">
            Giới thiệu
            <textarea
              value={bio}
              onChange={(event) => setBio(event.target.value)}
              maxLength={500}
              rows={4}
              className="w-full rounded-lg border border-slate-300 px-3.5 py-2.5 text-sm font-normal normal-case tracking-normal text-content-primary"
            />
          </label>
          <Button type="submit" isLoading={saving}>Lưu hồ sơ</Button>
          {profileMessage && <p role="status" className="text-sm text-emerald-700">{profileMessage}</p>}
        </form>
      </section>

      <section className="rounded-xl border border-slate-200 bg-white p-5 sm:p-6">
        <h2 className="text-lg font-semibold">Đổi mật khẩu</h2>
        <p className="mt-1 text-sm text-content-secondary">
          Dùng mật khẩu tối thiểu 8 ký tự gồm chữ hoa, chữ thường, số và ký tự đặc biệt.
        </p>
        <form onSubmit={changePassword} className="mt-5 space-y-4">
          <Input
            label="Mật khẩu hiện tại"
            type="password"
            autoComplete="current-password"
            value={currentPassword}
            onChange={(event) => setCurrentPassword(event.target.value)}
            required
          />
          <Input
            label="Mật khẩu mới"
            type="password"
            autoComplete="new-password"
            minLength={8}
            value={newPassword}
            onChange={(event) => setNewPassword(event.target.value)}
            required
          />
          <Input
            label="Nhập lại mật khẩu mới"
            type="password"
            autoComplete="new-password"
            minLength={8}
            value={confirmPassword}
            onChange={(event) => setConfirmPassword(event.target.value)}
            required
          />
          <Button type="submit" variant="outline" isLoading={saving}>Đổi mật khẩu</Button>
          {passwordMessage && <p role="status" className="text-sm text-emerald-700">{passwordMessage}</p>}
        </form>
      </section>
    </main>
  );
}
