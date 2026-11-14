"use client";

import { ChangeEvent, FormEvent, useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Button } from "@/components/ui/Button";
import { ErrorState } from "@/components/ui/ErrorState";
import { Input } from "@/components/ui/Input";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { authApi } from "@/lib/api/auth";
import { resolveApiAssetUrl } from "@/lib/api/axios";
import { normalizeImageFile } from "@/lib/imageFile";
import { getApiErrorMessage } from "@/lib/api/error";
import { useAuthStore } from "@/store/useAuthStore";
import type { UserProfileDto } from "@/types/api";

const MAX_AVATAR_SIZE = 100 * 1024 * 1024;
export default function ProfilePage() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const accessToken = useAuthStore((state) => state.accessToken);
  const user = useAuthStore((state) => state.user);
  const setSession = useAuthStore((state) => state.setSession);
  const avatarPreviewUrlRef = useRef<string | null>(null);
  const [displayName, setDisplayName] = useState("");
  const [avatarUrl, setAvatarUrl] = useState("");
  const [selectedAvatarFile, setSelectedAvatarFile] = useState<File | null>(null);
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null);
  const [avatarSaving, setAvatarSaving] = useState(false);
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

  useEffect(() => () => {
    if (avatarPreviewUrlRef.current) URL.revokeObjectURL(avatarPreviewUrlRef.current);
  }, []);

  function showLocalPreview(file: File | null) {
    if (avatarPreviewUrlRef.current) URL.revokeObjectURL(avatarPreviewUrlRef.current);
    avatarPreviewUrlRef.current = file ? URL.createObjectURL(file) : null;
    setAvatarPreview(avatarPreviewUrlRef.current);
  }

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
      syncProfile(profile);
      setProfileMessage("Đã cập nhật hồ sơ.");
    } catch (requestError) {
      setError(getApiErrorMessage(requestError));
    } finally {
      setSaving(false);
    }
  }

  function syncProfile(profile: UserProfileDto, keepPreview = false) {
    if (accessToken && user) {
      setSession(accessToken, {
        ...user,
        displayName: profile.displayName,
        avatarUrl: profile.avatarUrl,
      });
    }
    setSelectedAvatarFile(null);
    if (!keepPreview) showLocalPreview(null);
    setAvatarUrl(profile.avatarUrl ?? "");
    void queryClient.setQueryData(["auth", "me"], profile);
    void queryClient.invalidateQueries({ queryKey: ["recipes"] });
    void queryClient.invalidateQueries({ queryKey: ["author"] });
    void queryClient.invalidateQueries({ queryKey: ["recipe-search"] });
    void queryClient.invalidateQueries({ queryKey: ["categories"] });
  }

  function changeAvatar(event: ChangeEvent<HTMLInputElement>) {
    const input = event.currentTarget;
    const selectedFile = input.files?.[0];
    input.value = "";
    if (!selectedFile) return;
    const file = normalizeImageFile(selectedFile);
    if (!file) {
      setError("Vui lòng chọn ảnh JPEG, PNG, WebP hoặc AVIF.");
      setProfileMessage("");
      return;
    }
    if (file.size > MAX_AVATAR_SIZE) {
      setError("Ảnh đại diện không được vượt quá 100 MB.");
      setProfileMessage("");
      return;
    }
    setError("");
    setProfileMessage("");
    setSelectedAvatarFile(file);
    showLocalPreview(file);
  }

  async function saveAvatar() {
    if (!selectedAvatarFile) return;
    setError("");
    setProfileMessage("");
    setAvatarSaving(true);
    try {
      const profile = await authApi.uploadAvatar(selectedAvatarFile);
      syncProfile(profile, true);
      setProfileMessage("Đã cập nhật ảnh đại diện.");
    } catch (requestError) {
      setError(getApiErrorMessage(requestError));
    } finally {
      setAvatarSaving(false);
    }
  }

  function cancelAvatarSelection() {
    setSelectedAvatarFile(null);
    showLocalPreview(null);
    setError("");
    setProfileMessage("");
  }

  async function removeAvatar() {
    setError("");
    setProfileMessage("");
    setAvatarSaving(true);
    try {
      const profile = await authApi.updateProfile({
        displayName: profileQuery.data?.displayName ?? displayName.trim(),
        avatarUrl: null,
        bio: profileQuery.data?.bio ?? null,
      });
      syncProfile(profile);
      setProfileMessage("Đã xóa ảnh đại diện.");
    } catch (requestError) {
      setError(getApiErrorMessage(requestError));
    } finally {
      setAvatarSaving(false);
    }
  }

  const effectiveAvatar = avatarPreview || resolveApiAssetUrl(avatarUrl) || "/images/default-avatar.svg";
  const hasSavedAvatar = Boolean(avatarUrl);

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
    <div className="mx-auto max-w-3xl space-y-8">
      <section className="flex flex-col items-center gap-4 rounded-2xl border border-stone-200 bg-surface p-6 text-center shadow-subtle sm:flex-row sm:text-left">
        <img src={effectiveAvatar} alt="Ảnh đại diện" className="h-24 w-24 rounded-full border-4 border-brand-light object-cover shadow-sm" />
        <div className="space-y-2">
          <h1 className="font-display text-2xl font-bold text-content-primary">Ảnh đại diện</h1>
          <div className="flex flex-wrap justify-center gap-2 sm:justify-start">
            <label htmlFor="avatar-upload" className={`inline-flex cursor-pointer items-center rounded-xl border border-brand bg-brand px-4 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-brand-hover focus-within:ring-2 focus-within:ring-brand focus-within:ring-offset-2 ${avatarSaving ? "pointer-events-none opacity-50" : ""}`}>
              {selectedAvatarFile ? "Đổi ảnh" : hasSavedAvatar ? "Thay ảnh" : "Thêm ảnh"}
            </label>
            <input
              id="avatar-upload"
              type="file"
              accept=".jpg,.jpeg,.png,.webp,.avif,image/jpeg,image/png,image/webp,image/avif"
              aria-label="Chọn ảnh đại diện từ thiết bị"
              className="sr-only"
              onChange={changeAvatar}
              disabled={avatarSaving}
            />
            {selectedAvatarFile ? (
              <>
                <Button type="button" onClick={saveAvatar} isLoading={avatarSaving}>Lưu ảnh</Button>
                <Button type="button" variant="outline" onClick={cancelAvatarSelection} disabled={avatarSaving}>Hủy</Button>
              </>
            ) : hasSavedAvatar && (
              <Button type="button" variant="outline" onClick={removeAvatar} isLoading={avatarSaving}>
                Xóa ảnh
              </Button>
            )}
          </div>
          {profileMessage && <p role="status" className="text-sm text-emerald-700">{profileMessage}</p>}
          {selectedAvatarFile && <p role="status" className="text-sm text-content-secondary">Đã chọn: {selectedAvatarFile.name}</p>}
        </div>
      </section>
      <header className="border-b border-stone-200 pb-6">
        <p className="editorial-kicker mb-2">Tài khoản</p>
        <h1 className="font-display text-3xl font-bold text-content-primary">Hồ sơ cá nhân</h1>
        <p className="mt-1 text-sm text-content-secondary">
          Cập nhật thông tin hiển thị và bảo vệ tài khoản của bạn.
        </p>
      </header>
      {error && <ErrorState title="Không thể lưu thay đổi" message={error} />}

      <section className="rounded-2xl border border-stone-200 bg-surface p-5 shadow-subtle sm:p-6">
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
          <label className="block space-y-1.5 text-xs font-semibold uppercase tracking-wider text-content-secondary">
            Giới thiệu
            <textarea
              value={bio}
              onChange={(event) => setBio(event.target.value)}
              maxLength={500}
              rows={4}
              className="min-h-11 w-full rounded-xl border border-stone-300 bg-surface-cream px-3.5 py-2.5 text-sm font-normal normal-case tracking-normal text-content-primary focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/15"
            />
          </label>
          <Button type="submit" isLoading={saving}>Lưu hồ sơ</Button>
        </form>
      </section>

      <section className="rounded-2xl border border-stone-200 bg-surface p-5 shadow-subtle sm:p-6">
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
    </div>
  );
}
