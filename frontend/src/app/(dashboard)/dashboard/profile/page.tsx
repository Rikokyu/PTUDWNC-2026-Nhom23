"use client";

import { useState, type FormEvent } from "react";
import {
  AtSign,
  ChefHat,
  Globe2,
  MapPin,
  Save,
  ShieldCheck,
  UserRound,
} from "lucide-react";
import { useAuth } from "@/components/auth/AuthProvider";
import {
  useDashboardPreviewStore,
  type PreviewProfile,
} from "@/store/dashboard-preview-store";
import {
  DashboardHeading,
  Field,
  formControl,
  Notice,
  usePreviewReady,
} from "@/components/dashboard/preview-ui";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Loading } from "@/components/shared/loading";
import { ErrorMessage } from "@/components/shared/error-message";

export default function ProfilePage() {
  const ready = usePreviewReady();
  const { user } = useAuth();
  const profiles = useDashboardPreviewStore((state) => state.profiles);
  if (!ready || !user) return <Loading label="Đang mở hồ sơ…" />;
  return (
    <ProfileForm
      key={user.id}
      userId={user.id}
      email={user.email}
      role={user.role}
      initialProfile={
        profiles[user.id] ?? {
          displayName: user.displayName,
          bio: "",
          website: "",
          location: "",
        }
      }
    />
  );
}

function ProfileForm({
  userId,
  email,
  role,
  initialProfile,
}: {
  userId: string;
  email: string;
  role: string;
  initialProfile: PreviewProfile;
}) {
  const saveProfile = useDashboardPreviewStore((state) => state.saveProfile);
  const [profile, setProfile] = useState({ ...initialProfile });
  const [baseline, setBaseline] = useState({ ...initialProfile });
  const [notice, setNotice] = useState("");
  const [error, setError] = useState("");
  const dirty = JSON.stringify(profile) !== JSON.stringify(baseline);
  const initials =
    profile.displayName
      .trim()
      .split(/\s+/)
      .slice(-2)
      .map((part) => part[0])
      .join("")
      .toLocaleUpperCase("vi") || "CB";
  function update(key: keyof PreviewProfile, value: string) {
    setProfile((current) => ({ ...current, [key]: value }));
    setNotice("");
  }
  function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    if (profile.displayName.trim().length < 2) {
      setError("Tên hiển thị cần có ít nhất 2 ký tự.");
      return;
    }
    if (profile.website.trim()) {
      try {
        const url = new URL(profile.website);
        if (!["http:", "https:"].includes(url.protocol)) throw new Error();
      } catch {
        setError(
          "Nhập địa chỉ website đầy đủ bắt đầu bằng https:// hoặc http://.",
        );
        return;
      }
    }
    const saved = Object.fromEntries(
      Object.entries(profile).map(([key, value]) => [key, value.trim()]),
    ) as PreviewProfile;
    saveProfile(userId, saved);
    setProfile(saved);
    setBaseline(saved);
    setNotice("Đã lưu bản xem trước hồ sơ trên trình duyệt này.");
  }
  return (
    <div className="mx-auto max-w-5xl space-y-6">
      <DashboardHeading
        eyebrow="Một chút về bạn"
        title="Hồ sơ cá nhân"
        description="Để cộng đồng biết người đứng sau những công thức ngon."
      />
      {notice && <Notice>{notice}</Notice>}
      {error && <ErrorMessage message={error} />}
      <div className="grid items-start gap-6 xl:grid-cols-[minmax(0,1fr)_280px]">
        <form
          onSubmit={save}
          className="space-y-5 rounded-2xl border border-slate-200 bg-white p-5 sm:p-6"
        >
          <div className="flex items-center gap-3 border-b border-slate-100 pb-5">
            <div className="flex size-11 items-center justify-center rounded-xl bg-orange-50 text-orange-700">
              <UserRound className="size-5" />
            </div>
            <div>
              <h2 className="font-semibold">Thông tin giới thiệu</h2>
              <p className="mt-1 text-xs text-slate-500">
                Xem trước cách hồ sơ của bạn hiển thị.
              </p>
            </div>
          </div>
          <Field label="Tên hiển thị *" htmlFor="profile-name">
            <Input
              id="profile-name"
              required
              minLength={2}
              maxLength={80}
              autoComplete="name"
              value={profile.displayName}
              onChange={(event) => update("displayName", event.target.value)}
            />
          </Field>
          <Field
            label="Email tài khoản"
            htmlFor="profile-email"
            hint="Email được lấy từ tài khoản đang đăng nhập và không thay đổi trong bản xem trước."
          >
            <Input id="profile-email" type="email" value={email} disabled />
          </Field>
          <Field
            label="Giới thiệu bản thân"
            htmlFor="profile-bio"
            hint={`${profile.bio.length}/500 ký tự`}
          >
            <textarea
              id="profile-bio"
              rows={5}
              className={formControl}
              maxLength={500}
              placeholder="Bạn thích nấu món gì? Điều gì mang bạn đến với căn bếp?"
              value={profile.bio}
              onChange={(event) => update("bio", event.target.value)}
            />
          </Field>
          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="Nơi sống" htmlFor="profile-location">
              <Input
                id="profile-location"
                maxLength={100}
                autoComplete="address-level2"
                placeholder="Ví dụ: TP. Hồ Chí Minh"
                value={profile.location}
                onChange={(event) => update("location", event.target.value)}
              />
            </Field>
            <Field label="Website" htmlFor="profile-website">
              <Input
                id="profile-website"
                type="url"
                maxLength={250}
                placeholder="https://…"
                value={profile.website}
                onChange={(event) => update("website", event.target.value)}
              />
            </Field>
          </div>
          <div className="flex flex-wrap items-center justify-between gap-3 border-t border-slate-100 pt-5">
            <p aria-live="polite" className="text-xs text-slate-500">
              {dirty
                ? "Có thay đổi chưa lưu."
                : "Thông tin xem trước đã được lưu."}
            </p>
            <div className="flex gap-2">
              <Button
                type="button"
                variant="outline"
                disabled={!dirty}
                onClick={() => {
                  setProfile({ ...baseline });
                  setNotice("");
                  setError("");
                }}
              >
                Hủy thay đổi
              </Button>
              <Button type="submit" disabled={!dirty}>
                <Save />
                Lưu bản xem trước
              </Button>
            </div>
          </div>
        </form>
        <aside className="space-y-4">
          <section className="overflow-hidden rounded-2xl border border-slate-200 bg-white">
            <div className="h-20 bg-gradient-to-br from-orange-100 via-amber-50 to-rose-100" />
            <div className="px-5 pb-6">
              <div
                aria-hidden="true"
                className="relative -mt-9 mb-4 flex size-20 items-center justify-center rounded-full border-4 border-white bg-orange-700 text-2xl font-semibold text-white"
              >
                {initials}
              </div>
              <Badge variant="secondary">Xem trước hồ sơ</Badge>
              <h2 className="mt-3 break-words text-xl font-semibold">
                {profile.displayName || "Tên hiển thị"}
              </h2>
              <p className="mt-1 flex items-center gap-1 text-xs text-orange-700">
                <ChefHat className="size-3" />
                Người yêu ẩm thực
              </p>
              <p className="mt-4 whitespace-pre-wrap break-words text-sm leading-relaxed text-slate-500">
                {profile.bio ||
                  "Một câu chuyện nhỏ về bạn sẽ giúp góc bếp thêm gần gũi."}
              </p>
              {profile.location && (
                <p className="mt-4 flex items-center gap-2 break-words text-xs text-slate-500">
                  <MapPin className="size-4 shrink-0" />
                  {profile.location}
                </p>
              )}
              {profile.website && (
                <p className="mt-2 flex items-center gap-2 text-xs text-slate-500">
                  <Globe2 className="size-4 shrink-0" />
                  <span className="break-all">{profile.website}</span>
                </p>
              )}
            </div>
          </section>
          <section className="space-y-3 rounded-2xl border border-slate-200 bg-white p-5">
            <h2 className="flex items-center gap-2 text-sm font-semibold">
              <ShieldCheck className="size-4 text-emerald-600" />
              Tài khoản đang đăng nhập
            </h2>
            <p className="flex items-start gap-2 break-all text-xs text-slate-500">
              <AtSign className="mt-0.5 size-4 shrink-0" />
              {email}
            </p>
            <Badge variant="outline">
              {role === "Admin" ? "Quản trị viên" : "Tác giả"}
            </Badge>
            <p className="text-xs leading-relaxed text-slate-500">
              Tên và giới thiệu ở bản xem trước không thay đổi thông tin xác
              thực của tài khoản.
            </p>
          </section>
        </aside>
      </div>
    </div>
  );
}
