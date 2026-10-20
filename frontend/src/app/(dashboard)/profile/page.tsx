"use client";

import Link from "next/link";
import { MainLayout } from "@/components/layout/main-layout";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useAuthStore } from "@/store/auth-store";

export default function ProfilePage() {
  const user = useAuthStore((state) => state.user);

  return (
    <MainLayout>
      <section className="section-block">
        <h1>Hồ sơ cá nhân</h1>
        {user ? (
          <Card className="profile-card">
            <CardHeader>
              <CardTitle>{user.displayName}</CardTitle>
              <p className="muted">{user.email}</p>
            </CardHeader>
            <CardContent className="stack">
              <div className="meta-row">
                {user.roles.map((role) => (
                  <Badge key={role} variant="secondary">{role}</Badge>
                ))}
                <Badge variant={user.emailConfirmed ? "default" : "outline"}>
                  {user.emailConfirmed ? "Email đã xác minh" : "Email chưa xác minh"}
                </Badge>
              </div>
              {user.bio ? <p>{user.bio}</p> : null}
            </CardContent>
          </Card>
        ) : (
          <Card className="profile-card">
            <CardContent className="stack">
              <p>Bạn chưa đăng nhập trong phiên hiện tại. Hãy đăng nhập để xem hồ sơ.</p>
              <Button asChild className="button-primary">
                <Link href="/login">Đăng nhập</Link>
              </Button>
            </CardContent>
          </Card>
        )}
      </section>
    </MainLayout>
  );
}
