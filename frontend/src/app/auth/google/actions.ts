"use server";

import { signIn, signOut } from "@/auth";
import { safeReturnPath } from "@/lib/auth/return-path";

export async function startGoogleSignIn(formData: FormData) {
  const next = safeReturnPath(formData.get("next")?.toString() ?? null);
  await signIn("google", {
    redirectTo: `/auth/google/complete?next=${encodeURIComponent(next)}`,
  });
}

export async function clearGoogleSession() {
  await signOut({ redirect: false });
}
