"use client";

import { useEffect, useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { LockKeyhole, Save, UserRound } from "lucide-react";
import { useAuth } from "@/components/auth/AuthProvider";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { getApiErrorMessage } from "@/lib/api/error";
import { authApi } from "@/lib/api/auth";
import { useAuthStore } from "@/store/useAuthStore";

const profileSchema = z.object({
  displayName: z.string().trim().min(1, "Vui lòng nhập tên hiển thị.").max(100),
  bio: z.string().trim().max(1000, "Tiểu sử tối đa 1000 ký tự."),
  avatarUrl: z.union([
    z.literal(""),
    z.string().trim().url("Vui lòng nhập URL ảnh hợp lệ.").max(500),
  ]),
});

const changePasswordSchema = z.object({
  currentPassword: z.string().min(1, "Vui lòng nhập mật khẩu hiện tại."),
  newPassword: z.string()
    .min(8, "Mật khẩu cần ít nhất 8 ký tự.")
    .max(128, "Mật khẩu tối đa 128 ký tự.")
    .regex(/[a-z]/, "Cần có ít nhất một chữ thường.")
    .regex(/[A-Z]/, "Cần có ít nhất một chữ hoa.")
    .regex(/[0-9]/, "Cần có ít nhất một chữ số.")
    .regex(/[^a-zA-Z0-9]/, "Cần có ít nhất một ký tự đặc biệt."),
  confirmPassword: z.string(),
}).refine((values) => values.newPassword === values.confirmPassword, {
  path: ["confirmPassword"],
  message: "Mật khẩu xác nhận không khớp.",
});

type ProfileFormValues = z.infer<typeof profileSchema>;
type ChangePasswordFormValues = z.infer<typeof changePasswordSchema>;

export default function ProfilePage() {
  const { user, updateUser } = useAuth();
  const storedUser = useAuthStore((state) => state.user);
  const [profileError, setProfileError] = useState<string | null>(null);
  const [profileSaved, setProfileSaved] = useState(false);
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [passwordSaved, setPasswordSaved] = useState(false);

  const profileForm = useForm<ProfileFormValues>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      displayName: user?.displayName ?? "",
      bio: user?.bio ?? "",
      avatarUrl: user?.avatarUrl ?? "",
    },
  });
  const passwordForm = useForm<ChangePasswordFormValues>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: {
      currentPassword: "",
      newPassword: "",
      confirmPassword: "",
    },
  });

  useEffect(() => {
    if (user) {
      profileForm.reset({
        displayName: user.displayName,
        bio: user.bio ?? "",
        avatarUrl: user.avatarUrl ?? "",
      });
    }
  }, [user, profileForm]);

  async function submitProfile(values: ProfileFormValues) {
    setProfileError(null);
    setProfileSaved(false);
    try {
      const updated = await authApi.updateProfile({
        displayName: values.displayName,
        bio: values.bio || null,
        avatarUrl: values.avatarUrl || null,
      });
      updateUser({
        id: updated.id,
        email: updated.email,
        displayName: updated.displayName,
        role: updated.role,
        bio: updated.bio,
        avatarUrl: updated.avatarUrl,
      });
      setProfileSaved(true);
    } catch (error) {
      setProfileError(getApiErrorMessage(error));
    }
  }

  async function submitPassword(values: ChangePasswordFormValues) {
    setPasswordError(null);
    setPasswordSaved(false);
    try {
      await authApi.changePassword(values.currentPassword, values.newPassword);
      passwordForm.reset();
      setPasswordSaved(true);
    } catch (error) {
      setPasswordError(getApiErrorMessage(error));
    }
  }

  return (
    <div className="mx-auto max-w-4xl space-y-8">
      <header className="border-b border-slate-200 pb-5">
        <p className="text-sm font-semibold text-emerald-700">Tài khoản</p>
        <h1 className="mt-1 text-2xl font-bold text-content-primary">Hồ sơ cá nhân</h1>
        <p className="mt-2 text-sm text-content-secondary">
          Quản lý thông tin công khai và bảo mật đăng nhập của bạn.
        </p>
      </header>

      <section className="grid gap-6 md:grid-cols-[220px_minmax(0,1fr)]">
        <div className="flex flex-col items-center gap-3 border-b border-slate-200 pb-6 md:border-b-0 md:border-r md:pb-0 md:pr-6">
          {storedUser?.avatarUrl ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img
              src={storedUser.avatarUrl}
              alt="Ảnh đại diện"
              className="h-28 w-28 rounded-full border border-slate-200 object-cover"
            />
          ) : (
            <div className="flex h-28 w-28 items-center justify-center rounded-full bg-emerald-50 text-emerald-800">
              <UserRound className="h-12 w-12" aria-hidden="true" />
            </div>
          )}
          <div className="text-center">
            <p className="font-semibold text-content-primary">{user?.displayName}</p>
            <p className="mt-1 break-all text-sm text-content-secondary">{user?.email}</p>
          </div>
        </div>

        <form className="space-y-5" onSubmit={profileForm.handleSubmit(submitProfile)}>
          <h2 className="text-lg font-semibold text-content-primary">Thông tin cá nhân</h2>
          {profileError && <p role="alert" className="text-sm text-rose-700">{profileError}</p>}
          {profileSaved && <p role="status" className="text-sm text-emerald-700">Hồ sơ đã được cập nhật.</p>}
          <Input
            label="Tên hiển thị"
            autoComplete="name"
            error={profileForm.formState.errors.displayName?.message}
            {...profileForm.register("displayName")}
          />
          <Input
            label="Ảnh đại diện (URL)"
            type="url"
            autoComplete="url"
            placeholder="https://example.com/avatar.jpg"
            error={profileForm.formState.errors.avatarUrl?.message}
            {...profileForm.register("avatarUrl")}
          />
          <div className="space-y-1.5">
            <label htmlFor="profile-bio" className="block text-xs font-semibold uppercase tracking-wider text-content-secondary">
              Giới thiệu
            </label>
            <textarea
              id="profile-bio"
              rows={4}
              maxLength={1000}
              className="w-full resize-y rounded-lg border border-slate-300 bg-white px-3.5 py-2.5 text-sm text-content-primary placeholder:text-slate-400 focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/20"
              {...profileForm.register("bio")}
            />
            {profileForm.formState.errors.bio && (
              <p className="text-xs text-rose-600">{profileForm.formState.errors.bio.message}</p>
            )}
          </div>
          <Button
            type="submit"
            isLoading={profileForm.formState.isSubmitting}
            leftIcon={<Save className="h-4 w-4" />}
          >
            Lưu hồ sơ
          </Button>
        </form>
      </section>

      <section className="border-t border-slate-200 pt-7">
        <div className="mb-5 flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-amber-50 text-amber-800">
            <LockKeyhole className="h-5 w-5" aria-hidden="true" />
          </div>
          <div>
            <h2 className="text-lg font-semibold text-content-primary">Đổi mật khẩu</h2>
            <p className="text-sm text-content-secondary">Xác nhận mật khẩu hiện tại trước khi thay đổi.</p>
          </div>
        </div>
        {passwordError && <p role="alert" className="mb-4 text-sm text-rose-700">{passwordError}</p>}
        {passwordSaved && <p role="status" className="mb-4 text-sm text-emerald-700">Mật khẩu đã được thay đổi.</p>}
        <form
          className="grid gap-4 md:grid-cols-2"
          onSubmit={passwordForm.handleSubmit(submitPassword)}
        >
          <Input
            label="Mật khẩu hiện tại"
            type="password"
            autoComplete="current-password"
            error={passwordForm.formState.errors.currentPassword?.message}
            {...passwordForm.register("currentPassword")}
          />
          <div className="hidden md:block" />
          <Input
            label="Mật khẩu mới"
            type="password"
            autoComplete="new-password"
            error={passwordForm.formState.errors.newPassword?.message}
            {...passwordForm.register("newPassword")}
          />
          <Input
            label="Xác nhận mật khẩu mới"
            type="password"
            autoComplete="new-password"
            error={passwordForm.formState.errors.confirmPassword?.message}
            {...passwordForm.register("confirmPassword")}
          />
          <div className="md:col-span-2">
            <Button type="submit" isLoading={passwordForm.formState.isSubmitting}>
              Cập nhật mật khẩu
            </Button>
          </div>
        </form>
      </section>
    </div>
  );
}
