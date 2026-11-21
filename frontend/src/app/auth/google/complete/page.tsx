import { auth } from "@/auth";
import { GoogleSignInComplete } from "@/components/auth/GoogleSignInComplete";
import { safeReturnPath } from "@/lib/auth/return-path";

export const dynamic = "force-dynamic";

export default async function GoogleSignInCompletePage({
  searchParams,
}: {
  searchParams: Promise<{ next?: string }>;
}) {
  const session = await auth() as ({ googleIdToken?: string } | null);
  const { next } = await searchParams;
  return <GoogleSignInComplete idToken={session?.googleIdToken ?? null} next={safeReturnPath(next ?? null)} />;
}
