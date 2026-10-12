"use client";

import React, { useState } from "react";
import Link from "next/link";
import Image from "next/image";
import { useRouter } from "next/navigation";
import { User, Mail, Lock } from "lucide-react";
import axios from "axios";
import { Input } from "@/components/ui/Input";
import { Button } from "@/components/ui/Button";
import {
  registerAccount,
  saveAuthSession,
  type ValidationProblem,
} from "@/lib/api/auth";

export default function RegisterPage() {
  const router = useRouter();
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [formError, setFormError] = useState("");
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError("");
    setFieldErrors({});

    if (password !== confirmPassword) {
      setFieldErrors({ confirmPassword: ["Mật khẩu xác nhận không khớp."] });
      return;
    }

    setIsLoading(true);
    try {
      const session = await registerAccount({
        fullName,
        email,
        userName,
        password,
      });
      saveAuthSession(session);
      router.push("/dashboard");
    } catch (error) {
      if (axios.isAxiosError<ValidationProblem>(error)) {
        const problem = error.response?.data;
        setFieldErrors(problem?.errors ?? {});
        setFormError(
          problem?.detail ??
            (error.response?.status === 409
              ? "Email hoặc tên đăng nhập đã được sử dụng."
              : "Không thể đăng ký tài khoản.")
        );
      } else {
        setFormError("Không thể kết nối đến máy chủ.");
      }
    } finally {
      setIsLoading(false);
    }
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
          Đăng ký tài khoản Culinary Blog
        </h2>
        <p className="text-xs text-content-tertiary">
          Tham gia cộng đồng và bắt đầu chia sẻ công thức nấu ăn của bạn ngay hôm nay.
        </p>
      </div>

      <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-md px-4">
        <div className="bg-white py-8 px-6 shadow-subtle border border-slate-200 rounded-xl sm:px-10 space-y-6">
          <form onSubmit={handleSubmit} className="space-y-4">
            {formError && (
              <div
                role="alert"
                className="rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
              >
                {formError}
              </div>
            )}

            <Input
              label="Họ và tên *"
              placeholder="Nguyễn Văn A"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              leftIcon={<User className="w-4 h-4" />}
              autoComplete="name"
              error={fieldErrors.fullName?.[0]}
              required
            />

            <Input
              label="Địa chỉ Email *"
              type="email"
              placeholder="ten@example.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              leftIcon={<Mail className="w-4 h-4" />}
              autoComplete="email"
              error={fieldErrors.email?.[0]}
              required
            />

            <Input
              label="Tên đăng nhập *"
              placeholder="nguyenvana"
              value={userName}
              onChange={(e) => setUserName(e.target.value)}
              leftIcon={<User className="w-4 h-4" />}
              autoComplete="username"
              helperText="Chỉ gồm chữ cái và chữ số, tối thiểu 3 ký tự."
              error={fieldErrors.userName?.[0]}
              required
            />

            <Input
              label="Mật khẩu *"
              type="password"
              placeholder="Ít nhất 8 ký tự"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              leftIcon={<Lock className="w-4 h-4" />}
              autoComplete="new-password"
              helperText="Có ít nhất 1 chữ hoa, 1 chữ số và 1 ký tự đặc biệt."
              error={fieldErrors.password?.[0]}
              required
            />

            <Input
              label="Xác nhận mật khẩu *"
              type="password"
              placeholder="Nhập lại mật khẩu"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              leftIcon={<Lock className="w-4 h-4" />}
              autoComplete="new-password"
              error={fieldErrors.confirmPassword?.[0]}
              required
            />

            <Button
              type="submit"
              variant="primary"
              size="lg"
              className="w-full justify-center"
              isLoading={isLoading}
            >
              Tạo tài khoản
            </Button>
          </form>

          <div className="pt-4 border-t border-slate-100 text-center text-xs text-content-secondary">
            Đã có tài khoản?{" "}
            <Link href="/auth/login" className="text-brand font-bold hover:underline">
              Đăng nhập ngay
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
