"use client";

import React, { useState } from "react";
import Link from "next/link";
import Image from "next/image";
import { useRouter } from "next/navigation";
import { Mail, Lock } from "lucide-react";
import { Input } from "@/components/ui/Input";
import { Button } from "@/components/ui/Button";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setTimeout(() => {
      setIsLoading(false);
      router.push("/dashboard");
    }, 800);
  };

  return (
    <div className="min-h-screen bg-slate-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
      <div className="sm:mx-auto sm:w-full sm:max-w-md text-center space-y-3">
        <Link href="/" className="inline-flex items-center gap-3">
          <div className="relative w-10 h-10 flex items-center justify-center shrink-0 overflow-hidden rounded-xl">
            <Image
              src="/icons/logo.png"
              alt="Culinary Blog Logo"
              width={40}
              height={40}
              className="object-contain w-full h-full"
            />
          </div>
          <span className="font-extrabold text-2xl tracking-tight text-content-primary">
            Culinary<span className="text-brand">Blog</span>
          </span>
        </Link>
        <h2 className="text-2xl font-extrabold text-content-primary tracking-tight">
          Đăng nhập tài khoản
        </h2>
        <p className="text-xs text-content-tertiary">
          Nhập thông tin của bạn để truy cập trang cá nhân và quản lý công thức.
        </p>
      </div>

      <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-md px-4">
        <div className="bg-white py-8 px-6 shadow-subtle border border-slate-200 rounded-xl sm:px-10 space-y-6">
          <form onSubmit={handleSubmit} className="space-y-4">
            <Input
              label="Địa chỉ Email"
              type="email"
              placeholder="ten@example.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              leftIcon={<Mail className="w-4 h-4" />}
              required
            />

            <Input
              label="Mật khẩu"
              type="password"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              leftIcon={<Lock className="w-4 h-4" />}
              required
            />

            <div className="flex items-center justify-between text-xs">
              <label className="flex items-center gap-2 text-content-secondary cursor-pointer">
                <input type="checkbox" className="rounded border-slate-300 text-brand focus:ring-brand" />
                Ghi nhớ đăng nhập
              </label>
              <Link href="/auth/forgot-password" className="text-brand hover:underline font-semibold">
                Quên mật khẩu?
              </Link>
            </div>

            <Button
              type="submit"
              variant="primary"
              size="lg"
              className="w-full justify-center"
              isLoading={isLoading}
            >
              Đăng nhập ngay
            </Button>
          </form>

          <div className="pt-4 border-t border-slate-100 text-center text-xs text-content-secondary">
            Chưa có tài khoản?{" "}
            <Link href="/auth/register" className="text-brand font-bold hover:underline">
              Đăng ký tài khoản mới
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
